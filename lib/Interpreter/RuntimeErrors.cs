namespace Flow.RuntimeErrors
{
	internal struct HasCompileErrors : IFormattedMessage
	{
		public string Format() => "Loaded code contains compile errors. Could not execute";
	}

	internal struct ExternalCommandNotFound : IFormattedMessage
	{
		public string name;
		public string Format() => $"Could not find external command named '{name}'";
	}

	internal struct IncompatibleExternalCommand : IFormattedMessage
	{
		public string name;
		public int expectedParameters;
		public int gotParameters;

		public string Format() => $"Incompatible external command '{name}' found. Expected {expectedParameters} parameters. Got {gotParameters}";
	}
}