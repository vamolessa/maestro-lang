namespace Maestro.StdLib
{
	internal static class Errors
	{
		internal static void ExpectType(ref Context context, string what, string expectedType, Value value)
		{
			context.Error($"Expected {what} of type {expectedType}. Got {value.GetTypeName()}");
		}
	}
}