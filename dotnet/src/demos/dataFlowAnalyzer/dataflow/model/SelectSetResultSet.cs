namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{
    using gudusoft.gsqlparser.nodes;
    using ESetOperatorType = gudusoft.gsqlparser.ESetOperatorType;
    using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;

    public class SelectSetResultSet : ResultSet
	{

		private TSelectSqlStatement selectObject;

		public SelectSetResultSet(TSelectSqlStatement select) : base(select)
		{
			this.selectObject = select;
		}

		public virtual ESetOperatorType SetOperatorType
		{
			get
			{
				return selectObject.SetOperatorType;
			}
		}

        public virtual TResultColumnList ResultColumnObject
        {
            get
            {
                if (selectObject.LeftStmt != null && selectObject.LeftStmt.ResultColumnList != null)
                {
                    return selectObject.LeftStmt.ResultColumnList;
                }
                else if (selectObject.RightStmt != null && selectObject.RightStmt.ResultColumnList != null)
                {
                    return selectObject.RightStmt.ResultColumnList;
                }
                return selectObject.ResultColumnList;
            }
        }

    }

}