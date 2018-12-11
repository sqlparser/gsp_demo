namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

	using TResultColumnList = gudusoft.gsqlparser.nodes.TResultColumnList;
	using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;

	public class SelectResultSet : ResultSet
	{

		private TSelectSqlStatement selectObject;

		public SelectResultSet(TSelectSqlStatement select) : base(select.ResultColumnList)
		{
			this.selectObject = select;
		}

		public virtual TResultColumnList ResultColumnObject
		{
			get
			{
				return selectObject.ResultColumnList;
			}
		}

		public virtual TSelectSqlStatement SelectStmt
		{
			get
			{
				return selectObject;
			}
		}

	}

}