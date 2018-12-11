namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.nodes;
    using System;
    using EExpressionType = gudusoft.gsqlparser.EExpressionType;
    using TParseTreeNode = gudusoft.gsqlparser.nodes.TParseTreeNode;
    using TResultColumn = gudusoft.gsqlparser.nodes.TResultColumn;
    using TResultColumnList = gudusoft.gsqlparser.nodes.TResultColumnList;

    public class SelectSetResultColumn : ResultColumn
	{

		public SelectSetResultColumn(ResultSet resultSet, TResultColumn resultColumnObject) : base(resultSet, resultColumnObject)
        {

            if (resultColumnObject.AliasClause != null)
            {
                this.name = resultColumnObject.AliasClause.ToString();
            }
            else
            {
                if (resultColumnObject.Expr.ExpressionType == EExpressionType.simple_constant_t)
                {
                    if (resultSet is SelectResultSet)
                    {
                        this.name = "DUMMY" + getIndexOf(((SelectResultSet)resultSet).ResultColumnObject, resultColumnObject);
                    }
                    else if (resultSet is SelectSetResultSet)
                    {
                        this.name = "DUMMY" + getIndexOf(((SelectSetResultSet)resultSet).ResultColumnObject, resultColumnObject);
                    }
                    else
                    {
                        this.name = resultColumnObject.ColumnNameOnly;
                    }
                }
                else
                {
                    this.name = resultColumnObject.ColumnNameOnly;
                }
            }

            this.fullName = this.name;
        }

        public SelectSetResultColumn(ResultSet resultSet, ResultColumn resultColumn)
        {
            if (resultColumn == null || resultSet == null)
            {
                throw new System.ArgumentException("ResultColumn arguments can't be null.");
            }

            id = ++TableColumn.TABLE_COLUMN_ID;

            this.resultSet = resultSet;
            resultSet.addColumn(this);

            if (resultColumn.ColumnObject is TResultColumn)
            {
                TResultColumn resultColumnObject = (TResultColumn)resultColumn.ColumnObject;
                if (resultColumnObject.AliasClause != null)
                {
                    this.alias = resultColumnObject.AliasClause.ToString();
                    TSourceToken aliasStartToken = resultColumnObject.AliasClause.startToken;
                    TSourceToken aliasEndToken = resultColumnObject.AliasClause.endToken;
                    this.aliasStartPosition = new Tuple<long, long>(aliasStartToken.lineNo, aliasStartToken.columnNo);
                    this.aliasEndPosition = new Tuple<long, long>(aliasEndToken.lineNo, aliasEndToken.columnNo + aliasEndToken.astext.Length);

                    this.name = this.alias;
                }
                else
                {
                    if (resultColumnObject.Expr.ExpressionType == EExpressionType.simple_constant_t)
                    {
                        if (resultSet is SelectResultSet)
                        {
                            this.name = "DUMMY" + getIndexOf(((SelectResultSet)resultSet).ResultColumnObject, resultColumnObject);
                        }
                        else if (resultSet is SelectSetResultSet)
                        {
                            this.name = "DUMMY" + getIndexOf(((SelectSetResultSet)resultSet).ResultColumnObject, resultColumnObject);
                        }
                        else
                        {
                            this.name = resultColumnObject.ToString();
                        }

                    }
                    else if (resultColumnObject.Expr.ExpressionType == EExpressionType.function_t)
                    {
                        this.name = resultColumnObject.Expr.FunctionCall.FunctionName.ToString();
                    }
                    else if (resultColumnObject.ColumnNameOnly != null && !"".Equals(resultColumnObject.ColumnNameOnly))
                    {
                        this.name = resultColumnObject.ColumnNameOnly;
                    }
                    else
                    {
                        this.name = resultColumnObject.ToString();
                    }
                }

                if (resultColumnObject.Expr.ExpressionType == EExpressionType.function_t)
                {
                    this.fullName = resultColumnObject.Expr.FunctionCall.FunctionName.ToString();
                }
                else
                {
                    this.fullName = resultColumnObject.ToString();
                }

                TSourceToken startToken = resultColumnObject.startToken;
                TSourceToken endToken = resultColumnObject.endToken;
                this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
                this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);
                this.columnObject = resultColumnObject;
            }
            else if (resultColumn.ColumnObject is TObjectName)
            {
                TObjectName resultColumnObject = (TObjectName)resultColumn.ColumnObject;

                if (resultColumnObject.ColumnNameOnly != null && !"".Equals(resultColumnObject.ColumnNameOnly))
                {
                    this.name = resultColumnObject.ColumnNameOnly;
                }
                else
                {
                    this.name = resultColumnObject.ToString();
                }

                this.fullName = this.name;

                TSourceToken startToken = resultColumnObject.startToken;
                TSourceToken endToken = resultColumnObject.endToken;
                this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
                this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);
                this.columnObject = resultColumnObject;
            }
            else
            {
                this.name = resultColumn.Name;
                this.fullName = this.name;

                TSourceToken startToken = resultColumn.ColumnObject.startToken;
                TSourceToken endToken = resultColumn.ColumnObject.endToken;
                this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
                this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);
                this.columnObject = resultColumn.ColumnObject;
            }
        }

        private int getIndexOf(TResultColumnList resultColumnList, TResultColumn resultColumnObject)
		{
			for (int i = 0; i < resultColumnList.size(); i++)
			{
				if (resultColumnList.getResultColumn(i) == resultColumnObject)
				{
					return i;
				}
			}
			return -1;
		}

	}

}