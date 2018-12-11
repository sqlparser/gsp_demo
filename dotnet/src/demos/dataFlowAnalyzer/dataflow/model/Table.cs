using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

    using TSourceToken = gudusoft.gsqlparser.TSourceToken;
    using TTable = gudusoft.gsqlparser.nodes.TTable;
    using System;

    public class Table
	{

		public static int TABLE_ID = 0;

		private int id;
		private string name;
		private string fullName;
		private string alias;
		private Tuple<long, long> startPosition;
		private Tuple<long, long> endPosition;
		private IList<TableColumn> columns = new List<TableColumn>();
		private bool subquery = false;

		private TTable tableObject;

		public Table(TTable table)
		{
			if (table == null)
			{
				throw new System.ArgumentException("Table arguments can't be null.");
			}

			id = ++TABLE_ID;

			this.tableObject = table;

			TSourceToken startToken = table.startToken;
			TSourceToken endToken = table.endToken;
			this.startPosition = new Tuple<long, long>(startToken.lineNo, startToken.columnNo);
			this.endPosition = new Tuple<long, long>(endToken.lineNo, endToken.columnNo + endToken.astext.Length);

            if (table.LinkTable != null)
            {
                this.fullName = table.LinkTable.FullName;
                this.name = table.LinkTable.Name;
                this.alias = table.Name;
            }
            else
            {
                this.fullName = table.FullName;
                this.name = table.Name;
                this.alias = table.AliasName;
            }

            if (table.Subquery != null)
			{
				subquery = true;
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

		public virtual IList<TableColumn> Columns
		{
			get
			{
				return columns;
			}
		}

		public virtual void addColumn(TableColumn column)
		{
			if (column != null && !this.columns.Contains(column))
			{
				this.columns.Add(column);
			}
		}

		public virtual TTable TableObject
		{
			get
			{
				return tableObject;
			}
		}

		public virtual string Alias
		{
			get
			{
				return alias;
			}
		}

		public virtual string FullName
		{
			get
			{
				return fullName;
			}
		}

		public virtual bool hasSubquery()
		{
			return subquery;
		}

		public virtual string DisplayName
		{
			get
			{
				return FullName + (!string.ReferenceEquals(Alias, null) ? " [" + Alias + "]" : "");
			}
		}
	}

}