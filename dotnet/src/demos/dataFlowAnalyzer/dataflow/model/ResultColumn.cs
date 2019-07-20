namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

    using EExpressionType = gudusoft.gsqlparser.EExpressionType;
    using TSourceToken = gudusoft.gsqlparser.TSourceToken;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using TParseTreeNode = gudusoft.gsqlparser.nodes.TParseTreeNode;
    using TResultColumn = gudusoft.gsqlparser.nodes.TResultColumn;
    using TResultColumnList = gudusoft.gsqlparser.nodes.TResultColumnList;
    using System;
    using System.Collections.Generic;

    public class ResultColumn
	{

        protected ResultSet resultSet;

        protected int id;

        protected string alias;
        protected Tuple<long, long> aliasStartPosition;
        protected Tuple<long, long> aliasEndPosition;

        protected string fullName;
        protected string name;

        protected Tuple<long, long> startPosition;
        protected Tuple<long, long> endPosition;

        protected TParseTreeNode columnObject;

        protected List<TObjectName> starLinkColumns = new List<TObjectName>();

        public ResultColumn()
		{

		}

		public ResultColumn(ResultSet resultSet, TParseTreeNode columnObject)
		{
			if (columnObject == null || resultSet == null)
			{
				throw new System.ArgumentException("ResultColumn arguments can't be null.");
			}

			id = ++TableColumn.TABLE_COLUMN_ID;

			this.resultSet = resultSet;
			resultSet.addColumn(this);

			this.columnObject = columnObject;

			TSourceToken startToken = columnObject.startToken;
			TSourceToken endToken = columnObject.endToken;

			if (columnObject is TObjectName)
			{
                if (((TObjectName)columnObject).ColumnNameOnly != null && !"".Equals(((TObjectName)columnObject).ColumnNameOnly))
                {
                    this.name = ((TObjectName)columnObject).ColumnNameOnly;
                }
                else
                {
                    this.name = ((TObjectName)columnObject).ToString();
                }
            }
			else
			{
				this.name = columnObject.ToString();
			}

			this.fullName = columnObject.ToString();

			this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
			this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);
		}

		public ResultColumn(ResultSet resultSet, TResultColumn resultColumnObject)
		{
			if (resultColumnObject == null || resultSet == null)
			{
				throw new System.ArgumentException("ResultColumn arguments can't be null.");
			}

			id = ++TableColumn.TABLE_COLUMN_ID;

			this.resultSet = resultSet;
			resultSet.addColumn(this);

			this.columnObject = resultColumnObject;

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
                    this.name = resultColumnObject.ToString();
                }
                else if (resultColumnObject.Expr.ExpressionType == EExpressionType.sqlserver_proprietary_column_alias_t)
                {
                    TSourceToken startToken1 = resultColumnObject.Expr
                            .LeftOperand
                            .startToken;
                    TSourceToken endToken1 = resultColumnObject.Expr
                            .LeftOperand
                            .endToken;
                    this.alias = resultColumnObject.Expr
                            .LeftOperand
                            .ToString();
                    this.aliasStartPosition = new Tuple<long, long>(startToken1.lineNo,
                            startToken1.columnNo);
                    this.aliasEndPosition = new Tuple<long, long>(endToken1.lineNo,
                            endToken1.columnNo + endToken1.astext.Length);

                    this.name = this.alias;
                }
                else if (resultColumnObject.Expr.ExpressionType == EExpressionType.function_t)
                {
                    this.name = resultColumnObject.Expr
                            .FunctionCall
                            .FunctionName
                            .ToString();
                }
                else if (resultColumnObject.ColumnNameOnly != null
                      && !"".Equals(resultColumnObject.ColumnNameOnly))
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
                this.fullName = resultColumnObject.Expr
                            .FunctionCall
                            .FunctionName
                            .ToString();
            }
            else
            {
                this.fullName = resultColumnObject.ToString();
            }

			TSourceToken startToken = resultColumnObject.startToken;
			TSourceToken endToken = resultColumnObject.endToken;
			this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
			this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);
		}

        public ResultColumn(SelectResultSet resultSet, Tuple<TResultColumn, TObjectName> starColumnPair)
        {
            if (starColumnPair == null || resultSet == null)
            {
                throw new System.ArgumentException("ResultColumn arguments can't be null.");
            }

            id = ++TableColumn.TABLE_COLUMN_ID;

            this.resultSet = resultSet;
            resultSet.addColumn(this);

            this.columnObject = starColumnPair.Item1;

            TSourceToken startToken = columnObject.startToken;
            TSourceToken endToken = columnObject.endToken;

            this.name = ((TObjectName)columnObject).ColumnNameOnly;
            this.fullName = columnObject.ToString();

            this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
            this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);
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

		public virtual ResultSet ResultSet
		{
			get
			{
				return resultSet;
			}
		}

		public virtual int Id
		{
			get
			{
				return id;
			}
		}

		public virtual string Alias
		{
			get
			{
				return alias;
			}
		}

		public virtual Tuple<long, long> AliasStartPosition
		{
			get
			{
				return aliasStartPosition;
			}
		}

		public virtual Tuple<long, long> AliasEndPosition
		{
			get
			{
				return aliasEndPosition;
			}
		}

		public virtual string FullName
		{
			get
			{
				return fullName;
			}
		}

		public virtual Tuple<long, long> StartPosition
		{
			get
			{
				return startPosition;
			}
		}

		public virtual Tuple<long, long> EndPosition
		{
			get
			{
				return endPosition;
			}
		}

		public virtual TParseTreeNode ColumnObject
		{
			get
			{
				return columnObject;
			}
		}

		public virtual string Name
		{
			get
			{
				return name;
			}
		}

        public virtual void bindStarLinkColumn(TObjectName objectName)
        {
            if (objectName != null && !starLinkColumns.Contains(objectName))
            {
                starLinkColumns.Add(objectName);
            }
        }


        public virtual List<TObjectName> StarLinkColumns
        {
            get
            {
                return starLinkColumns;
            }
        }

    }

}