namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

	public class ResultColumnRelationElement : RelationElement
	{

		private ResultColumn column;

		public ResultColumnRelationElement(ResultColumn column)
		{
			this.column = column;
		}

		public object Element
		{
			get
			{
				return column;
			}
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((column == null) ? 0 : column.GetHashCode());
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
			ResultColumnRelationElement other = (ResultColumnRelationElement) obj;
			if (column == null)
			{
				if (other.column != null)
				{
					return false;
				}
			}
			else if (!column.Equals(other.column))
			{
				return false;
			}
			return true;
		}

	}

}