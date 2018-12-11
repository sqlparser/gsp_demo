namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

	public class TableRelationElement : RelationElement
	{

		private Table table;

		public TableRelationElement(Table table)
		{
			this.table = table;
		}

		public object Element
		{
			get
			{
				return table;
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
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
			TableRelationElement other = (TableRelationElement) obj;
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



	}

}