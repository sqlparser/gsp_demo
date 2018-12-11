namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.nodes;
    using System;
    using TSourceToken = gudusoft.gsqlparser.TSourceToken;
    using TTable = gudusoft.gsqlparser.nodes.TTable;

    public class QueryTable : ResultSet
	{

		private string alias;
		private Tuple<long, long> startPosition;
		private Tuple<long, long> endPosition;

		private TTable tableObject;

		public QueryTable(TTable tableObject) : base(tableObject.CTE != null ? getCTEQuery(tableObject.CTE) : tableObject.Subquery)
        {

			this.tableObject = tableObject;

			TSourceToken startToken = tableObject.startToken;
			TSourceToken endToken = tableObject.endToken;

            if (tableObject.AliasClause != null)
            {
                startToken = tableObject.AliasClause.getStartToken();
                endToken = tableObject.AliasClause.getEndToken();
            }

            this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
			this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);

			if (tableObject.AliasClause != null)
			{
				this.alias = tableObject.AliasName;
			}
		}

        private static TCustomSqlStatement getCTEQuery(TCTE cte)
        {
            if (cte.Subquery != null)
            {
                return cte.Subquery;
            }
            else if (cte.UpdateStmt != null)
            {
                return cte.UpdateStmt;
            }
            else if (cte.InsertStmt != null)
            {
                return cte.InsertStmt;
            }
            else if (cte.DeleteStmt != null)
            {
                return cte.DeleteStmt;
            }
            else
            {
                return null;
            }
        }

        public virtual string Alias
		{
			get
			{
				return alias;
			}
		}

		public override Tuple<long, long> StartPosition
		{
			get
			{
				return startPosition;
			}
		}

		public override Tuple<long, long> EndPosition
		{
			get
			{
				return endPosition;
			}
		}

		public virtual TTable TableObject
		{
			get
			{
				return tableObject;
			}
		}

	}

}