using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.model.view
{


	public class TableModel
	{
		private string schema;
		private string name;
		private string alias;
		private string highlightInfo;
		private string coordinate;

		public virtual string Coordinate
		{
			get
			{
				return coordinate;
			}
			set
			{
				this.coordinate = value;
			}
		}


		private List<ColumnModel> columns = new List<ColumnModel>();

		public virtual string HighlightInfo
		{
			get
			{
				return highlightInfo;
			}
			set
			{
				this.highlightInfo = value;
			}
		}


		public virtual string Alias
		{
			get
			{
				return alias;
			}
			set
			{
				this.alias = value;
			}
		}


		public virtual ColumnModel[] Columns
		{
			get
			{
				return columns.ToArray();
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


		public virtual string FullName
		{
			get
			{
				if (!string.ReferenceEquals(schema, null))
				{
					return schema + "." + name;
				}
				else
				{
					return name;
				}
			}
		}

		public virtual string Schema
		{
			get
			{
				return schema;
			}
			set
			{
				this.schema = value;
			}
		}


		public virtual void addColumn(ColumnModel column)
		{
			if (column != null && !columns.Contains(column))
			{
				columns.Add(column);
			}
		}

		public virtual void reset()
		{
			schema = null;
			name = null;
			columns.Clear();
		}

		public virtual bool containsColumn(ColumnModel column)
		{
			return column.Table == this && getColumn(column.Name) != null;
		}

		public virtual ColumnModel getColumn(string columnName)
		{
			for (int i = 0; i < columns.Count; i++)
			{
				ColumnModel column = columns[i];
				if (!string.ReferenceEquals(columnName, null) && columnName.Equals(column.Name))
				{
					return column;
				}
			}
			return null;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((string.ReferenceEquals(name, null)) ? 0 : name.GetHashCode());
			result = prime * result + ((string.ReferenceEquals(schema, null)) ? 0 : schema.GetHashCode());
			return result;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (obj == null)
			{
				return false;
			}
			if (this.GetType() != obj.GetType())
			{
				return false;
			}
			TableModel other = (TableModel) obj;
			if (string.ReferenceEquals(name, null))
			{
				if (!string.ReferenceEquals(other.name, null))
				{
					return false;
				}
			}
			else if (!name.Equals(other.name))
			{
				return false;
			}
			if (string.ReferenceEquals(schema, null))
			{
				if (!string.ReferenceEquals(other.schema, null))
				{
					return false;
				}
			}
			else if (!schema.Equals(other.schema))
			{
				return false;
			}
			return true;
		}

	}

}