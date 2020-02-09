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

	public AssertCommand(params Value[] expectValues)
	{
		this.expectedValues = expectValues;
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
	public int expectedStackSize = 0;

	public void OnBegin(VirtualMachine vm)
	{
	}

	public void OnEnd(VirtualMachine vm)
	{
		Assert.Equal(expectedStackSize, vm.stack.count);
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
}

public static class TestHelper
{
	public static readonly Mode CompilerMode = Mode.Debug;
	public static readonly AssertCleanupDebugger assertCleanupDebugger = new AssertCleanupDebugger();

	public static Value[] ToValueArray(params int[] values)
	{
		var array = new Value[values.Length];
		for (var i = 0; i < array.Length; i++)
			array[i] = new Value(values[i]);
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

	public static TestCommand<T> Intantiate<T>(TestCompiled compiled, string commandName) where T : struct, ITuple
	{
		var executable = compiled.engine.InstantiateCommand<T>(compiled.result, commandName);
		if (!executable.isSome)
			throw new CommandNotFound(commandName);
		return new TestCommand<T>(compiled.engine, executable.value);
	}

	public static void Run(this TestCompiled compiled, int expectedStackSize = 0)
	{
		Run(compiled.engine, compiled.result.executable, expectedStackSize);
	}

	public static void Run<T>(this TestCommand<T> command, int expectedStackSize = 0) where T : struct, ITuple
	{
		Run(command.engine, command.executable, expectedStackSize);
	}

	private static void Run<T>(Engine engine, Executable<T> executable, int expectedStackSize) where T : struct, ITuple
	{
		assertCleanupDebugger.expectedStackSize = expectedStackSize;
		engine.SetDebugger(assertCleanupDebugger);
		var executeResult = engine.Execute(executable, default);
		if (executeResult.error.isSome)
			throw new RuntimeErrorException(executeResult);
	}
}