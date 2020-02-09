using Xunit;
using Maestro;

public sealed class CommandTests
{
	[Theory]
	[InlineData("command c {}")]
	[InlineData("command c $_arg1 {}")]
	[InlineData("command c $_arg1 $_arg2 {}")]
	[InlineData("command c $_arg1 $_arg2 $_arg3 {}")]
	public void Declaration(string source)
	{
		var engine = new Engine();
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		source = "external command bypass 0;\n" + source;
		TestHelper.Compile(engine, source).Run();
	}

	[Theory]
	[InlineData("command c $arg {}")]
	[InlineData("command c $_arg $_arg {}")]
	[InlineData("command c {} command c {}")]
	public void FailDeclaration(string source)
	{
		Assert.Throws<CompileErrorException>(() =>
		{
			var engine = new Engine();
			engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
			source = "external command bypass 0;\n" + source;
			TestHelper.Compile(engine, source).Run();
		});
	}

	[Theory]
	[InlineData("command c {assert;} c;")]
	[InlineData("command c {1 | assert;} c;", 1)]
	[InlineData("command c {1,2,3 | assert;} c;", 1, 2, 3)]
	[InlineData("command c {$$ | assert;} c;")]
	[InlineData("command c {$$ | assert;} 1 | c;", 1)]
	[InlineData("command c {$$ | assert;} 1,2,3 | c;", 1, 2, 3)]
	[InlineData("command c {return $$;} c | assert;")]
	[InlineData("command c {return $$;} 1 | c | assert;", 1)]
	[InlineData("command c {return $$;} 1,2,3 | c | assert;", 1, 2, 3)]
	[InlineData("command c {return $$ | bypass;} c | assert;")]
	[InlineData("command c {return $$ | bypass;} 1 | c | assert;", 1)]
	[InlineData("command c {return $$ | bypass;} 1,2,3 | c | assert;", 1, 2, 3)]
	[InlineData("command c $arg {return $arg;} c 1 | assert;", 1)]
	public void Execute(string source, params int[] expected)
	{
		var expectedValues = TestHelper.ToValueArray(expected);
		var assertCommand = new AssertCommand(expectedValues);

		var engine = new Engine();
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		engine.RegisterCommand("assert", () => assertCommand);
		source = "external command assert 0;external command bypass 0;\n" + source;
		TestHelper.Compile(engine, source).Run();
	}
}