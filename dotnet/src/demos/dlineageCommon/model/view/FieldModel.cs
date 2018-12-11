using System;

namespace gudusoft.gsqlparser.demos.dlineage.model.view
{


	public class FieldModel
	{
		private string name;
		private string schema;
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


		private Tuple<long,long> location;

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


		public virtual Tuple<long,long> Location
		{
			get
			{
				return location;
			}
			set
			{
				this.location = value;
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

	}

}