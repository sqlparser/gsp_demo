using System;

namespace demos.dlineage.model.view
{

	public class AliasModel
	{
		private string name;
		private Tuple<long,long> location;
		private string highlightInfo;
		private FieldModel field;
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


		public virtual FieldModel Field
		{
			get
			{
				return field;
			}
			set
			{
				this.field = value;
			}
		}


	}

}