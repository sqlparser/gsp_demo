namespace demos.dlineage.model.view
{


	public class ColumnModel
	{
		private string name;
		private TableModel table;
		private string highlightInfos;
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


		public virtual string HighlightInfos
		{
			get
			{
				return highlightInfos;
			}
			set
			{
				this.highlightInfos = value;
			}
		}


		public virtual TableModel Table
		{
			get
			{
				return table;
			}
			set
			{
				this.table = value;
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


		public ColumnModel(string name, TableModel table)
		{
			this.name = name;
			this.table = table;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((string.ReferenceEquals(name, null)) ? 0 : name.GetHashCode());
			result = prime * result + ((table == null) ? 0 : table.GetHashCode());
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
			ColumnModel other = (ColumnModel) obj;
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
			if (table == null)
			{
				if (other.table != null)
				{
					return false;
				}
			}
			else if (!table.Equals(other.table))
			{
				return false;
			}
			return true;
		}

		public virtual string FullName
		{
			get
			{
				return table.FullName + "." + name;
			}
		}

	}

}