using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

    using TSourceToken = gudusoft.gsqlparser.TSourceToken;
    using TCreateViewSqlStatement = gudusoft.gsqlparser.stmt.TCreateViewSqlStatement;
    using System;

    public class View
	{

		private int id;
		private string name;
		private Tuple<long, long> startPosition;
		private Tuple<long, long> endPosition;
		private IList<ViewColumn> columns = new List<ViewColumn>();
		private TCreateViewSqlStatement viewObject;

		public View(TCreateViewSqlStatement view)
		{
            if (view == null)
            {
                throw new System.ArgumentException("Table arguments can't be null.");
            }

            id = ++Table.TABLE_ID;

            this.viewObject = view;

            TSourceToken startToken = viewObject.startToken;
            TSourceToken endToken = viewObject.endToken;
            if (viewObject.ViewName != null)
            {
                startToken = viewObject.ViewName.startToken;
                endToken = viewObject.ViewName.endToken;
                this.name = viewObject.ViewName.ToString();
            }
            else
            {
                this.name = "";
                Console.Error.WriteLine();
                Console.Error.WriteLine("Can't get view name. View is ");
                Console.Error.WriteLine(view.ToString());
            }

            this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
            this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);
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
			set
			{
				this.name = value;
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

		public virtual IList<ViewColumn> Columns
		{
			get
			{
				return columns;
			}
		}

		public virtual void addColumn(ViewColumn column)
		{
			if (column != null && !this.columns.Contains(column))
			{
				this.columns.Add(column);
			}
		}

		public virtual TCreateViewSqlStatement ViewObject
		{
			get
			{
				return viewObject;
			}
		}

		public virtual string DisplayName
		{
			get
			{
				return Name;
			}
		}
	}

}