using System;
using System.Collections.Generic;

namespace demos.dlineage.model.metadata
{
    using demos.util;
    using SQLUtil = demos.dlineage.util.SQLUtil;

    //JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
    //ORIGINAL LINE: @SuppressWarnings("serial") public class ColumnMetaData extends java.util.LinkedHashMap<String, Object>
    public class ColumnMetaData : LinkedHashMap<string, object>
	{

		private const string PROP_AUTOINCREMENT = "autoIncrement";
		private const string PROP_TYPENAME = "typeName";
		private const string PROP_TYPECODE = "typeCode";
		public const string PROP_TABLENAME = "tableName";
		private const string PROP_COLUMNDISPLAYSIZE = "columnDisplaySize";
		private const string PROP_ALIAS = "alias";
		public const string PROP_CATALOGNAME = "catalogName";
		private const string PROP_PRECISION = "precision";
		private const string PROP_SCALE = "scale";
		public const string PROP_SCHEMANAME = "schemaName";
		private const string PROP_READONLY = "readOnly";
		private const string PROP_WRITEABLE = "writeable";
		private const string PROP_COMMENT = "comment";
		private const string PROP_NULL = "isNull";
		private const string PROP_DEFAULTVALUE = "defaultValue";
		private const string PROP_PRIMARYKEY = "primaryKey";
		private const string PROP_INDEX = "index";
		private const string PROP_NOTNULL = "isNotNull";
		private const string PROP_UNIQUE = "unique";
		private const string PROP_FOREIGNKEY = "foreignKey";
		private const string PROP_CHECK = "check";

		private TableMetaData table;
		private List<ColumnMetaData> referColumns = new List<ColumnMetaData>();
		private string name;
		private string displayName;

		private string orphan;

		public ColumnMetaData()
		{
			this[PROP_ALIAS] = "";
			this[PROP_TYPENAME] = "";
			this[PROP_TYPECODE] = "";
			this[PROP_COLUMNDISPLAYSIZE] = "";
			this[PROP_PRECISION] = "";
			this[PROP_SCALE] = "";
			this[PROP_AUTOINCREMENT] = "";
			this[PROP_READONLY] = "";
			this[PROP_WRITEABLE] = "";
			this[PROP_COMMENT] = "";
			this[PROP_NULL] = "";
			this[PROP_NOTNULL] = "";
			this[PROP_DEFAULTVALUE] = "";
			this[PROP_UNIQUE] = "";
			this[PROP_CHECK] = "";
			this[PROP_PRIMARYKEY] = "";
			this[PROP_INDEX] = "";
			this[PROP_FOREIGNKEY] = "";
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = prime + ((string.ReferenceEquals(name, null)) ? 0 : name.GetHashCode());
			result = prime * result + ((table == null) ? 0 : table.GetHashCode());
			return result;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			if (!(obj is ColumnMetaData))
			{
				return false;
			}
			ColumnMetaData other = (ColumnMetaData) obj;
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

		public virtual TableMetaData Table
		{
			set
			{
				this.table = value;
			}
			get
			{
				return table;
			}
		}


		public virtual string DisplayName
		{
			get
			{
				return displayName;
			}
			set
			{
				this.displayName = value;
			}
		}


		public virtual ColumnMetaData[] ReferColumns
		{
			get
			{
				return referColumns.ToArray();
			}
			set
			{
				if (value != null)
				{
					for (int i = 0; i < value.Length; i++)
					{
						addReferColumn(value[i]);
					}
				}
			}
		}


		public virtual void addReferColumn(ColumnMetaData columnMetaData)
		{
			if (!referColumns.Contains(columnMetaData))
			{
				referColumns.Add(columnMetaData);
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
				if (SQLUtil.isEmpty(value))
				{
					return;
				}
				displayName = value;
				value = SQLUtil.trimObjectName(value);
				this.name = value;
			}
		}


		public virtual bool AutoIncrement
		{
			set
			{
				this[PROP_AUTOINCREMENT] = value;
			}
		}

		public virtual string TypeName
		{
			set
			{
				this[PROP_TYPENAME] = value;
			}
		}

		public virtual int TypeCode
		{
			set
			{
				this[PROP_TYPECODE] = value;
			}
		}

		public virtual string ColumnDisplaySize
		{
			set
			{
				this[PROP_COLUMNDISPLAYSIZE] = value;
			}
		}

		public virtual int Precision
		{
			set
			{
				this[PROP_PRECISION] = value;
			}
		}

		public virtual int Scale
		{
			set
			{
				this[PROP_SCALE] = value;
			}
		}

		public virtual bool Writeable
		{
			set
			{
				this[PROP_WRITEABLE] = value;
			}
		}

		public virtual bool ReadOnly
		{
			set
			{
				this[PROP_READONLY] = value;
			}
		}

		public virtual string Comment
		{
			set
			{
				this[PROP_COMMENT] = value;
			}
			get
			{
				if (this.ContainsKey(PROP_COMMENT) && this[PROP_COMMENT] !=null && this[PROP_COMMENT].ToString().Length > 0)
				{
					return (string)this[PROP_COMMENT];
				}
				else
				{
					return null;
				}
			}
		}

		public virtual bool Null
		{
			set
			{
				this[PROP_NULL] = value;
			}
		}

		public virtual void setPrimaryKey(bool primaryKey)
		{
			this[PROP_PRIMARYKEY] = primaryKey;
		}

		public virtual bool Index
		{
			set
			{
				this[PROP_INDEX] = value;
			}
		}

		public virtual bool Unique
		{
			set
			{
				this[PROP_UNIQUE] = value;
			}
		}

		public virtual bool NotNull
		{
			set
			{
				this[PROP_NOTNULL] = value;
			}
		}

		public virtual string DefaultVlaue
		{
			set
			{
				this[PROP_DEFAULTVALUE] = value;
			}
		}

		public virtual bool Check
		{
			set
			{
				this[PROP_CHECK] = value;
			}
		}

		public virtual bool ForeignKey
		{
			set
			{
				this[PROP_FOREIGNKEY] = value;
			}
		}

		public virtual string FullName
		{
			get
			{
				if (Table != null)
				{
					return Table.FullName + "." + name;
				}
				return name;
			}
		}

		public virtual string DisplayFullName
		{
			get
			{
				if (Table != null)
				{
					return Table.DisplayFullName + "." + displayName;
				}
				return name;
			}
		}

		public virtual string Type
		{
			get
			{
				if (this.ContainsKey(PROP_TYPENAME) && this[PROP_TYPENAME] != null && this[PROP_TYPENAME].ToString().Length > 0)
				{
					return (string) this[PROP_TYPENAME];
				}
				return null;
			}
		}

		public virtual string Size
		{
			get
			{
				if (this.ContainsKey(PROP_COLUMNDISPLAYSIZE) && this[PROP_COLUMNDISPLAYSIZE] != null && this[PROP_COLUMNDISPLAYSIZE].ToString().Length > 0)
				{
					return this[PROP_COLUMNDISPLAYSIZE].ToString();
				}
				else
				{
					return null;
				}
			}
		}

		public virtual string isPrimaryKey()
		{
			if (this.ContainsKey(PROP_PRIMARYKEY) && this[PROP_PRIMARYKEY] != null && this[PROP_PRIMARYKEY].ToString().Length > 0)
			{
				return this[PROP_PRIMARYKEY].ToString();
			}
			else
			{
				return null;
			}
		}


		public virtual string DefaultValue
		{
			get
			{
				if (this.ContainsKey(PROP_DEFAULTVALUE) && this[PROP_DEFAULTVALUE] != null && this[PROP_DEFAULTVALUE].ToString().Length > 0)
				{
					return this[PROP_DEFAULTVALUE].ToString();
				}
				else
				{
					return null;
				}
			}
		}

		public virtual string Required
		{
			get
			{
				if (this.ContainsKey(PROP_NOTNULL) && this[PROP_NOTNULL] != null && this[PROP_NOTNULL].ToString().Length > 0 && Convert.ToBoolean(this[PROP_NOTNULL].ToString()))
				{
					return this[PROP_NOTNULL].ToString();
				}
				else
				{
					return null;
				}
			}
		}

		public virtual string AutoIncrease
		{
			get
			{
				if (this.ContainsKey(PROP_AUTOINCREMENT) && this[PROP_AUTOINCREMENT] != null && this[PROP_AUTOINCREMENT].ToString().Length > 0 && Convert.ToBoolean(this[PROP_AUTOINCREMENT].ToString()))
				{
					return this[PROP_AUTOINCREMENT].ToString();
				}
				else
				{
					return null;
				}
			}
		}

		public virtual bool isOrphan()
		{
			if (string.ReferenceEquals(orphan, null))
			{
				return false;
			}
			else
			{
				return bool.Parse(orphan);
			}
		}

		public virtual void setOrphan(string orphan)
		{
			if ("false".Equals(this.orphan) || string.ReferenceEquals(orphan, null))
			{
				return;
			}
			this.orphan = orphan;
		}

	}
}