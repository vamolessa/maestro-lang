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
	public readonly Value[] expectedValues;
	public Value[] gotValues;

	public AssertCommand(params object[] expectValues)
	{
		this.expectedValues = TestHelper.ToValueArray(expectValues);
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
		Assert.Equal(expectedValues, gotValues);
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

public readonly struct TestCompiled
{
	public readonly Engine engine;
	public readonly CompileResult result;

	public TestCompiled(Engine engine, CompileResult result)
	{
		this.engine = engine;
		this.result = result;
	}

	public TestExecuteScope<Tuple0> ExecuteScope()
	{
		return new TestExecuteScope<Tuple0>(
			engine,
			engine.ExecuteScope(),
			result.executable
		);
	}
}

public readonly struct TestCommand<T> where T : struct, ITuple
{
	public readonly Engine engine;
	public readonly Executable<T> executable;

	public TestCommand(Engine engine, Executable<T> executable)
	{
		this.engine = engine;
		this.executable = executable;
	}

	public TestExecuteScope<T> ExecuteScope()
	{
		return new TestExecuteScope<T>(
			engine,
			engine.ExecuteScope(),
			executable
		);
	}
}

public readonly struct TestExecuteScope<T> : System.IDisposable where T : struct, ITuple
{
	public readonly Engine engine;
	public readonly ExecuteScope scope;
	public readonly Executable<T> executable;

	internal TestExecuteScope(Engine engine, ExecuteScope scope, Executable<T> executable)
	{
		this.engine = engine;
		this.scope = scope;
		this.executable = executable;
	}

	public void Run(T args, int expectedStackCount = 0)
	{
		TestHelper.Run(engine, scope, executable, args, expectedStackCount);
	}

	public void Dispose()
	{
		scope.Dispose();
	}
}

public static class TestHelper
{
	public static readonly Mode CompilerMode = Mode.Debug;

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

	public static TestCompiled Compile(string source)
	{
		return Compile(new Engine(), source);
	}

	public static TestCompiled Compile(Engine engine, string source)
	{
		var compileResult = engine.CompileSource(new Source(new Uri("source"), source), CompilerMode, Option.None);
		if (compileResult.errors.count > 0)
			throw new CompileErrorException(compileResult);
		return new TestCompiled(engine, compileResult);
	}

	public static TestCommand<T> Intantiate<T>(this TestCompiled compiled, string commandName) where T : struct, ITuple
	{
		var executable = compiled.engine.InstantiateCommand<T>(compiled.result, commandName);
		if (!executable.isSome)
			throw new CommandNotFound(commandName);
		return new TestCommand<T>(compiled.engine, executable.value);
	}

	public static void Run(this TestCompiled compiled, int expectedStackCount = 0)
	{
		using var scope = compiled.ExecuteScope();
		scope.Run(default, expectedStackCount);
	}

	public static void Run<T>(this TestCommand<T> command, T args, int expectedStackCount = 0) where T : struct, ITuple
	{
		using var scope = command.ExecuteScope();
		scope.Run(args, expectedStackCount);
	}

	internal static void Run<T>(Engine engine, ExecuteScope scope, Executable<T> executable, T args, int expectedStackCount) where T : struct, ITuple
	{
		engine.SetDebugger(new AssertCleanupDebugger(expectedStackCount));
		var executeResult = scope.Execute(executable, args);
		if (executeResult.error.isSome)
			throw new RuntimeErrorException(executeResult);
	}
}