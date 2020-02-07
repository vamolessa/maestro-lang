using Maestro;
using System.Text;

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

	public static void Run(TestCompiled compiled)
	{
		Run(compiled.engine, compiled.result.executable);
	}

	public static void Run<T>(TestCommand<T> command) where T : struct, ITuple
	{
		Run(command.engine, command.executable);
	}

	private static void Run<T>(Engine engine, Executable<T> executable) where T : struct, ITuple
	{
		var executeResult = engine.Execute(executable, default);
		if (executeResult.error.isSome)
			throw new RuntimeErrorException(executeResult);
	}
}