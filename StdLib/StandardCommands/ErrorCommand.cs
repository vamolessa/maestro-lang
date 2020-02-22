using System.Text;

namespace Maestro.StdLib
{
	public sealed class ErrorCommand : ICommand<Tuple1>
	{
		private readonly StringBuilder sb = new StringBuilder();

		public void Execute(ref Context context, Tuple1 args)
		{
			args.value0.AppendTo(sb);
			context.Error(sb.ToString());
			sb.Clear();
		}
	}
}
