namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

    using TSourceToken = gudusoft.gsqlparser.TSourceToken;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using System;
    using System.Collections.Generic;

    public class ViewColumn
	{

		private View view;

		private int id;
		private string name;

		private Tuple<long, long> startPosition;
		private Tuple<long, long> endPosition;

		private TObjectName columnObject;

        private List<TObjectName> starLinkColumns = new List<TObjectName>();

        private int columnIndex;

        public ViewColumn(View view, TObjectName columnObject, int index)
        {
			if (view == null || columnObject == null)
			{
				throw new System.ArgumentException("TableColumn arguments can't be null.");
			}

			id = ++TableColumn.TABLE_COLUMN_ID;

			this.columnObject = columnObject;

			TSourceToken startToken = columnObject.startToken;
			TSourceToken endToken = columnObject.endToken;
			this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
			this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);

			if (!"".Equals(columnObject.ColumnNameOnly))
			{
				this.name = columnObject.ColumnNameOnly;
			}
			else
			{
				this.name = columnObject.ToString();
			}

			this.view = view;
            this.columnIndex = index;
            view.addColumn(this);
		}

		public virtual View View
		{
			get
			{
				return view;
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

        public virtual void bindStarLinkColumns(List<TObjectName> starLinkColumns)
        {
            if (starLinkColumns != null && starLinkColumns.Count > 0)
            {
                this.starLinkColumns = starLinkColumns;
            }
        }

        public virtual List<TObjectName> StarLinkColumns
        {
            get
            {
                return starLinkColumns;
            }
        }

        public virtual int ColumnIndex
        {
            get
            {
                return columnIndex;
            }
        }

    }

}