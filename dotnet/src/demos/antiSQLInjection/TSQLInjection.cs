namespace gudusoft.gsqlparser.demos.antiSQLInjection
{

	/// <summary>
	/// This class represents a sql injection.
	/// </summary>
	public class TSQLInjection
	{

		private ESQLInjectionType type = ESQLInjectionType.syntax_error;

		public TSQLInjection(ESQLInjectionType pType)
		{
			this.type = pType;
			this.description = pType.ToString();
		}


		public virtual ESQLInjectionType Type
		{
			get
			{
				return type;
			}
		}

		private string description = null;

		public virtual string Description
		{
			get
			{
				return description;
			}
			set
			{
				this.description = value;
			}
		}


	}
}