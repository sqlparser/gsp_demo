namespace gudusoft.gsqlparser.demos.antiSQLInjection
{

	/// <summary>
	/// value returned when GEval can't evaluate a value from an expression.
	/// </summary>
	public class UnknownValue
	{
		public override string ToString()
		{
			return "unknown value";
		}
	}
}