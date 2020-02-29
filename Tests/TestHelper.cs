using Maestro;
using System.Text;
using Xunit;

public sealed class BypassCommand<T> : ICommand<T> where T : struct, ITuple
{
	public void Execute(ref Context context, T args)
	{
		for (var i = 0; i < context.inputCount; i++)
			context.PushValue(context.GetInput(i));
	}
}

public sealed class AssertCommand : ICommand<Tuple0>
{
	public readonly object[] expectedValues;
	public Value[] gotValues;

	public AssertCommand(params object[] expectedValues)
	{
		this.expectedValues = expectedValues;
		this.gotValues = null;
	}

	public void Execute(ref Context context, Tuple0 args)
	{
		gotValues = new Value[context.inputCount];

		for (var i = 0; i < gotValues.Length; i++)
			gotValues[i] = context.GetInput(i);
	}

	public void AssertExpectedInputs()
	{
		Assert.Equal(expectedValues, TestHelper.ToObjectArray(gotValues));
	}

	public void AssertNotCalled()
	{
		Assert.Null(gotValues);
	}
}

public sealed class AssertCleanupDebugger : IDebugger
{
	public readonly int expectedStackCount;

	public AssertCleanupDebugger(int expectedStackCount)
	{
		this.expectedStackCount = expectedStackCount;
	}

	public void OnBegin(VirtualMachine vm)
	{
	}

	public void OnEnd(VirtualMachine vm)
	{
		Assert.Equal(expectedStackCount, vm.stack.count);
		Assert.Equal(0, vm.debugInfo.frames.count);
		Assert.Equal(0, vm.debugInfo.variableInfos.count);
	}

	public void OnHook(VirtualMachine vm)
	{
	}
}

public sealed class CompileErrorException : System.Exception
{
	public readonly CompileResult result;

	public CompileErrorException(CompileResult result) : base(GetErrorString(result))
	{
		this.result = result;
	}

	private static string GetErrorString(CompileResult result)
	{
		var sb = new StringBuilder();
		result.FormatErrors(sb);
		return sb.ToString();
	}
}

public sealed class LinkErrorException : System.Exception
{
	public readonly LinkResult result;

	public LinkErrorException(LinkResult result) : base(GetErrorString(result))
	{
		this.result = result;
	}

	private static string GetErrorString(LinkResult result)
	{
		var sb = new StringBuilder();
		result.FormatErrors(sb);
		return sb.ToString();
	}
}

public sealed class RuntimeErrorException : System.Exception
{
	public readonly ExecuteResult result;

	public RuntimeErrorException(ExecuteResult result)
	{
		this.result = result;
	}

	private static string GetErrorString(ExecuteResult result)
	{
		var sb = new StringBuilder();
		result.FormatError(sb);
		return sb.ToString();
	}
}

public sealed class CommandNotFound : System.Exception
{
	private readonly string commandName;

	public CommandNotFound(string commandName) : base($"Could not find command '{commandName}'")
	{
		this.commandName = commandName;
	}
}

public readonly struct TestExecutable
{
	public readonly Engine engine;
	public readonly Executable executable;

	public TestExecutable(Engine engine, Executable executable)
	{
		this.engine = engine;
		this.executable = executable;
	}

	public TestExecuteScope ExecuteScope()
	{
		return new TestExecuteScope(
			engine,
			engine.ExecuteScope(),
			executable
		);
	}
}

public readonly struct TestExecuteScope : System.IDisposable
{
	public readonly Engine engine;
	public readonly ExecuteScope scope;
	public readonly Executable executable;

	internal TestExecuteScope(Engine engine, ExecuteScope scope, Executable executable)
	{
		this.engine = engine;
		this.scope = scope;
		this.executable = executable;
	}

	public void Run(int expectedStackCount = 0)
	{
		TestHelper.Run(engine, scope, executable, expectedStackCount);
	}

	public void Dispose()
	{
		scope.Dispose();
	}
}

public static class TestHelper
{
	public static readonly Mode CompileMode = Mode.Debug;

	public static object[] ToObjectArray(params Value[] values)
	{
		var array = new object[values.Length];
		for (var index = 0; index < array.Length; index++)
		{
			switch (values[index].asObject)
			{
			case ValueKind.False _: array[index] = false; break;
			case ValueKind.True _: array[index] = true; break;
			case ValueKind.Int _: array[index] = values[index].asNumber.asInt; break;
			case ValueKind.Float _: array[index] = values[index].asNumber.asFloat; break;
			default: array[index] = values[index].asObject; break;
			}
		}
		return array;
	}

	public static Value[] ToValueArray(params object[] values)
	{
		var array = new Value[values.Length];
		for (var index = 0; index < array.Length; index++)
		{
			switch (values[index])
			{
			case int i: array[index] = new Value(i); break;
			case float f: array[index] = new Value(f); break;
			case bool b: array[index] = new Value(b); break;
			case object o: array[index] = new Value(o); break;
			}
		}
		return array;
	}

	public static TestExecutable Compile(Engine engine, string source)
	{
		var compileResult = engine.CompileSource(new Source("source", source), CompileMode);
		if (!compileResult.TryGetAssembly(out var assembly))
			throw new CompileErrorException(compileResult);
		var linkResult = engine.LinkAssembly(assembly);
		if (!linkResult.TryGetExecutable(out var executable))
			throw new LinkErrorException(linkResult);
		return new TestExecutable(engine, executable);
	}

	public static void Run(this TestExecutable compiled, int expectedStackCount = 0)
	{
		using var scope = compiled.ExecuteScope();
		scope.Run(expectedStackCount);
	}

	internal static void Run(Engine engine, ExecuteScope scope, Executable executable, int expectedStackCount)
	{
		engine.SetDebugger(new AssertCleanupDebugger(expectedStackCount));
		var executeResult = scope.Execute(executable);
		if (executeResult.error.isSome)
			throw new RuntimeErrorException(executeResult);
	}
}