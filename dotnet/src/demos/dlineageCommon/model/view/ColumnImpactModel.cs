using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.model.view
{


	public class ColumnImpactModel
	{
		private string sql;
		private List<TableModel> tables = new List<TableModel>();
		private List<AliasModel> aliases = new List<AliasModel>();
		private List<FieldModel> fields = new List<FieldModel>();
		private List<ReferenceModel> references = new List<ReferenceModel>();

		public virtual ReferenceModel[] References
		{
			get
			{
				return references.ToArray();
			}
		}

		public virtual string Sql
		{
			get
			{
				return sql;
			}
			set
			{
				this.sql = value;
			}
		}


		public virtual TableModel[] Tables
		{
			get
			{
				return tables.ToArray();
			}
		}

		public virtual AliasModel[] Aliases
		{
			get
			{
				return aliases.ToArray();
			}
		}

		public virtual FieldModel[] Fields
		{
			get
			{
				return fields.ToArray();
			}
		}

		public virtual void addAlias(AliasModel alias)
		{
			if (alias != null && !aliases.Contains(alias))
			{
				aliases.Add(alias);
			}
		}

		public virtual void addField(FieldModel field)
		{
			if (field != null && !fields.Contains(field))
			{
				fields.Add(field);
			}
		}

		public virtual void addTable(TableModel table)
		{
			if (table != null && !tables.Contains(table))
			{
				tables.Add(table);
			}

		}

		public virtual void addReference(ReferenceModel reference)
		{
			if (reference != null && !references.Contains(reference))
			{
				references.Add(reference);
			}
		}

		public virtual void reset()
		{
			sql = null;
			tables.Clear();
			aliases.Clear();
			fields.Clear();
			references.Clear();
		}

		public virtual bool containsTable(string tableOwner, string tableName)
		{
			return getTable(tableOwner, tableName) != null;
		}

		public virtual TableModel getTable(string tableOwner, string tableName)
		{
			string fullName = (string.ReferenceEquals(tableOwner, null) ? tableName : (tableOwner + "." + tableName));
			for (int i = 0; i < tables.Count; i++)
			{
				TableModel table = tables[i];
				if (!string.ReferenceEquals(fullName, null) && fullName.Equals(table.FullName))
				{
					return table;
				}
			}
			return null;
		}
	}

}