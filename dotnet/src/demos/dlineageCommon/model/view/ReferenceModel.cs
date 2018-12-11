using System;

namespace gudusoft.gsqlparser.demos.dlineage.model.view
{

	public class ReferenceModel
	{
		public const int TYPE_ALIAS_TABLE = 1;
		public const int TYPE_ALIAS_COLUMN = 2;
		public const int TYPE_FIELD_TABLE = 3;
		public const int TYPE_FIELD_COLUMN = 4;

		private Tuple<long,long> location;
		private int referenceType;
		private TableModel table;
		private ColumnModel column;
		private AliasModel alias;
		private FieldModel field;
		private Clause clause;

		public virtual Clause Clause
		{
			get
			{
				return clause;
			}
			set
			{
				this.clause = value;
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


		public virtual int ReferenceType
		{
			get
			{
				return referenceType;
			}
			set
			{
				this.referenceType = value;
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


		public virtual ColumnModel Column
		{
			get
			{
				return column;
			}
			set
			{
				this.column = value;
			}
		}


		public virtual AliasModel Alias
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