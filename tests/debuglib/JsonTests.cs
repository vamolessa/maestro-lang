using Xunit;
using Maestro.Debug;
using System.Text;

public sealed class JsonTests
{
	private static Json.Value ObjectToValue(object value)
	{
		switch (value)
		{
		case bool b: return new Json.Value(b);
		case int i: return new Json.Value(i);
		case float f: return new Json.Value(f);
		case string s: return new Json.Value(s);
		default: return new Json.Value();
		}
	}

	private static object ValueToObject(Json.Value value)
	{
		if (value.TryGet(out bool b))
			return b;
		if (value.TryGet(out int i))
			return i;
		if (value.TryGet(out float f))
			return f;
		if (value.TryGet(out string s))
			return s;
		return null;
	}

	[Theory]
	[InlineData(null, "null")]
	[InlineData(true, "true")]
	[InlineData(false, "false")]
	[InlineData(0, "0")]
	[InlineData(1, "1")]
	[InlineData(-1, "-1")]
	[InlineData(99.5f, "99.5")]
	[InlineData("string", "\"string\"")]
	[InlineData("\"\\/\b\f\n\r\t", "\"\\\"\\\\/\\b\\f\\n\\r\\t\"")]
	public void SerializeValue(object value, string expectedJson)
	{
		var jsonValue = ObjectToValue(value);
		var sb = new StringBuilder();
		Json.Serialize(jsonValue, sb);
		Assert.Equal(expectedJson, sb.ToString());
	}

	[Fact]
	public void SerializeComplex()
	{
		var smallObject = Json.Value.NewObject();
		smallObject["int"] = 7;
		smallObject["bool"] = false;
		smallObject["null"] = null;
		smallObject["string"] = "some text";

		var a = Json.Value.NewArray();
		a.Add("string");
		a.Add(false);
		a.Add(null);
		a.Add(0.25f);
		a.Add(smallObject);
		a.Add(Json.Value.NewArray());

		var o = Json.Value.NewObject();
		o["array"] = a;
		o["str"] = "asdad";
		o["empty"] = Json.Value.NewObject();

		var sb = new StringBuilder();
		Json.Serialize(o, sb);

		Assert.Equal(
			"{\"array\":[\"string\",false,null,0.25,{\"int\":7,\"bool\":false,\"null\":null,\"string\":\"some text\"},[]],\"str\":\"asdad\",\"empty\":{}}",
			sb.ToString()
		);
	}

	[Theory]
	[InlineData("null", null)]
	[InlineData("true", true)]
	[InlineData("false", false)]
	[InlineData("0", 0)]
	[InlineData("1", 1)]
	[InlineData("-1", -1)]
	[InlineData("99.5", 99.5f)]
	[InlineData("99.25", 99.25f)]
	[InlineData("99.125", 99.125f)]
	[InlineData("\"string\"", "string")]
	[InlineData("\"\\\"\\\\\\/\\b\\f\\n\\r\\t\"", "\"\\/\b\f\n\r\t")]
	public void DeserializeValue(string json, object expectedValue)
	{
		var success = Json.TryDeserialize(json, out var value);
		Assert.True(success);
		Assert.Equal(expectedValue, ValueToObject(value));
	}

	[Fact]
	public void DeserializeComplex()
	{
		Assert.True(Json.TryDeserialize(
			" { \"array\"  : [\"string\",  false,null,0.25,\n{\"int\":  7,  \"bool\":false,\"null\":null, \t\n   \"string\":\"some text\"},[]],   \n\"str\":\"asdad\", \"empty\":{}}",
			out var value
		));

		Assert.True(value.IsObject);

		Assert.True(value["array"].IsArray);
		Assert.Equal(6, value["array"].Count);
		Assert.Equal("string", value["array"][0].GetOr(""));
		Assert.False(value["array"][1].GetOr(true));
		Assert.True(value["array"][2].IsNull);
		Assert.Equal(0.25f, value["array"][3].GetOr(0.0f));

		Assert.True(value["array"][4].IsObject);
		Assert.Equal(7, value["array"][4]["int"].GetOr(0));
		Assert.False(value["array"][4]["bool"].GetOr(true));
		Assert.True(value["array"][4]["null"].IsNull);
		Assert.Equal("some text", value["array"][4]["string"].GetOr(""));

		Assert.True(value["array"][5].IsArray);
		Assert.Equal(0, value["array"][5].Count);

		Assert.Equal("asdad", value["str"].GetOr(""));
		Assert.True(value["empty"].IsObject);
	}
}