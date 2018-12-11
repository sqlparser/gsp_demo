namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

    using TSourceToken = gudusoft.gsqlparser.TSourceToken;
    using TConstant = gudusoft.gsqlparser.nodes.TConstant;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using System;
    using System.Collections.Generic;

    public class TableColumn
	{

		public static int TABLE_COLUMN_ID = 0;

		private Table table;

		private int id;
		private string name;

		private Tuple<long, long> startPosition;
		private Tuple<long, long> endPosition;

		private TObjectName columnObject;
        private IList<TObjectName> starLinkColumns = new List<TObjectName>();

        public TableColumn(Table table, TObjectName columnObject)
		{
			if (table == null || columnObject == null)
			{
				throw new System.ArgumentException("TableColumn arguments can't be null.");
			}

			id = ++TABLE_COLUMN_ID;

			this.columnObject = columnObject;

			TSourceToken startToken = columnObject.startToken;
			TSourceToken endToken = columnObject.endToken;
			this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
			this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);

            if (columnObject.ColumnNameOnly != null && !"".Equals(columnObject.ColumnNameOnly))
            {
                this.name = columnObject.ColumnNameOnly;
            }
            else
            {
                this.name = columnObject.ToString();
            }

            this.table = table;
			table.addColumn(this);
		}

		public TableColumn(Table table, TConstant columnObject, int columnIndex)
		{
			if (table == null || columnObject == null)
			{
				throw new System.ArgumentException("TableColumn arguments can't be null.");
			}

			id = ++TABLE_COLUMN_ID;

			TSourceToken startToken = columnObject.startToken;
			TSourceToken endToken = columnObject.endToken;
			this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
			this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);

			this.name = "DUMMY" + columnIndex;

			this.table = table;
			table.addColumn(this);
		}

		public virtual Table Table
		{
			get
			{
				return table;
			}
		}

		public virtual int Id
		{
			get
			{
				return id;
			}
		}

		public virtual string Name
		{
			get
			{
				return name;
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

		public virtual TObjectName ColumnObject
		{
			get
			{
				return columnObject;
			}
		}

        public virtual void bindStarLinkColumns(IList<TObjectName> starLinkColumns)
        {
            if (starLinkColumns != null && starLinkColumns.Count > 0)
            {
                this.starLinkColumns = starLinkColumns;
            }
        }

        public virtual IList<TObjectName> StarLinkColumns
        {
            get
            {
                return starLinkColumns;
            }
        }

    }

}