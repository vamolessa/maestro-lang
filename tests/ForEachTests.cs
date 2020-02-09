using Xunit;
using Maestro;

public sealed class ForEachTests
{
	public sealed class IncrementIterationCountCommand : ICommand<Tuple0>
	{
		public int iterationCount = 0;

		public void Execute(ref Context context, Tuple0 args)
		{
			iterationCount += 1;
		}
	}

	public sealed class AppendElementCommand : ICommand<Tuple1>
	{
		public Buffer<Value> elements = new Buffer<Value>();

		public void Execute(ref Context context, Tuple1 args)
		{
			elements.PushBack(args.value0);
		}
	}

	[Theory]
	[InlineData("foreach $_ in 1 {increment;}", 1)]
	[InlineData("foreach $_ in 1,2,3 {increment;}", 3)]
	[InlineData("foreach $_ in 1 | bypass {increment;}", 1)]
	[InlineData("foreach $_ in 1,2,3 | bypass {increment;}", 3)]
	[InlineData("foreach $_ in bypass {increment;}", 0)]
	public void IterationCount(string source, int expectedIterationCount)
	{
		var incrementCommand = new IncrementIterationCountCommand();

		var engine = new Engine();
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		engine.RegisterCommand("increment", () => incrementCommand);
		source = "external command bypass 0;external command increment 0;\n" + source;
		TestHelper.Compile(engine, source).Run();
		Assert.Equal(expectedIterationCount, incrementCommand.iterationCount);
	}

	[Theory]
	[InlineData("foreach $e in 1 {append $e;}", 1)]
	[InlineData("foreach $e in 1,2,3 {append $e;}", 1, 2, 3)]
	[InlineData("foreach $e in 1 | bypass {append $e;}", 1)]
	[InlineData("foreach $e in 1,2,3 | bypass {append $e;}", 1, 2, 3)]
	[InlineData("foreach $e in bypass {append $e;}")]
	public void IterationElements(string source, params int[] expected)
	{
		var appendCommand = new AppendElementCommand();
		var expectedElements = new Value[expected.Length];
		for (var i = 0; i < expectedElements.Length; i++)
			expectedElements[i] = new Value(expected[i]);

		var engine = new Engine();
		engine.RegisterCommand("bypass", () => new BypassCommand<Tuple0>());
		engine.RegisterCommand("append", () => appendCommand);
		source = "external command bypass 0;external command append 1;\n" + source;
		TestHelper.Compile(engine, source).Run();
		Assert.Equal(expectedElements, appendCommand.elements.ToArray());
	}
}