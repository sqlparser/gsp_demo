using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.model.metadata
{

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;


    using foreignKey = gudusoft.gsqlparser.demos.dlineage.model.ddl.schema.foreignKey;
    using index = gudusoft.gsqlparser.demos.dlineage.model.ddl.schema.index;
    using unique = gudusoft.gsqlparser.demos.dlineage.model.ddl.schema.unique;
    using SQLUtil = gudusoft.gsqlparser.demos.dlineage.util.SQLUtil;

    public class TableMetaData : Dictionary<string, object>
    {

        private const string PROP_NAME = "name";
        private const string PROP_CATALOGNAME = "catalogName";
        private const string PROP_SCHEMANAME = "schemaName";
        private const string PROP_COMMENT = "comment";

        private string name;
        private string schemaName;
        private string catalogName;

        private string displayName;
        private string catalogDisplayName;
        private string schemaDisplayName;

        private bool isView = false;

        private List<foreignKey> foreignKeys = new List<foreignKey>();

        private List<index> indices = new List<index>();

        private List<unique> uniques = new List<unique>();

        private bool strict = false;

        private EDbVendor vendor = EDbVendor.dbvmssql;

        public TableMetaData(EDbVendor vendor, bool strict)
        {
            this.vendor = vendor;
            this.strict = strict;
            this.isView = false;
            this[PROP_CATALOGNAME] = "";
            this[PROP_SCHEMANAME] = "";
            this[PROP_COMMENT] = "";
        }

        public virtual string Name
        {
            set
            {
                if (SQLUtil.isEmpty(value))
                {
                    return;
                }
                displayName = value;
                value = SQLUtil.trimObjectName(value);
                this.name = value;
                if (!string.ReferenceEquals(value, null))
                {
                    this[PROP_NAME] = value;
                }
            }
            get
            {
                return (string)this[PROP_NAME];
            }
        }

        public virtual string CatalogName
        {
            set
            {
                if (SQLUtil.isEmpty(value))
                {
                    return;
                }
                catalogDisplayName = value;
                value = SQLUtil.trimObjectName(value);
                this.catalogName = value;
                if (!string.ReferenceEquals(value, null))
                {
                    this[PROP_CATALOGNAME] = value;
                }
            }
            get
            {
                return (string)this[PROP_CATALOGNAME];
            }
        }

        public virtual string SchemaName
        {
            set
            {
                if (SQLUtil.isEmpty(value))
                {
                    return;
                }
                if (EDbVendor.dbvmysql == vendor)
                {
                    CatalogName = value;
                }
                else
                {
                    schemaDisplayName = value;
                    value = SQLUtil.trimObjectName(value);
                    this.schemaName = value;
                    if (!string.ReferenceEquals(value, null))
                    {
                        this[PROP_SCHEMANAME] = value;
                    }
                }
            }
            get
            {
                return (string)this[PROP_SCHEMANAME];
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


        public virtual string CatalogDisplayName
        {
            get
            {
                return catalogDisplayName;
            }
            set
            {
                this.catalogDisplayName = value;
            }
        }


        public virtual string SchemaDisplayName
        {
            get
            {
                return schemaDisplayName;
            }
            set
            {
                this.schemaDisplayName = value;
            }
        }


        public virtual string Comment
        {
            set
            {
                if (!string.ReferenceEquals(schemaName, null))
                {
                    this[PROP_COMMENT] = value;
                }
            }
            get
            {
                return (string)this[PROP_COMMENT];
            }
        }


        public override int GetHashCode()
        {
            const int prime = 31;
            int result = 0;
            result = prime * result + ((string.ReferenceEquals(name, null)) ? 0 : name.GetHashCode());
            if (strict)
            {
                result = prime * result + ((string.ReferenceEquals(catalogName, null)) ? 0 : catalogName.GetHashCode());
                result = prime * result + ((string.ReferenceEquals(schemaName, null)) ? 0 : schemaName.GetHashCode());
            }
            return result;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is TableMetaData))
            {
                return false;
            }

            TableMetaData other = (TableMetaData)obj;

            if (strict)
            {
                if (string.ReferenceEquals(catalogName, null))
                {
                    if (!string.ReferenceEquals(other.catalogName, null))
                    {
                        return false;
                    }
                }
                else if (!catalogName.Equals(other.catalogName))
                {
                    return false;
                }

                if (string.ReferenceEquals(schemaName, null))
                {
                    if (!string.ReferenceEquals(other.schemaName, null))
                    {
                        return false;
                    }
                }
                else if (!schemaName.Equals(other.schemaName))
                {
                    return false;
                }
            }

            if (catalogName != null
                && other.schemaName != null
                && !catalogName.Equals(other.catalogName))
            {
                return false;
            }

            if (schemaName != null
                    && other.schemaName != null
                    && !schemaName.Equals(other.schemaName))
            {
                return false;
            }


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

            return true;
        }

        public virtual string FullName
        {
            get
            {
                string fullName = name;
                if (!string.ReferenceEquals(schemaName, null))
                {
                    fullName = schemaName + "." + fullName;
                }
                if (!string.ReferenceEquals(catalogName, null))
                {
                    fullName = catalogName + "." + fullName;
                }
                return fullName;
            }
        }

        public virtual string DisplayFullName
        {
            get
            {
                string fullName = displayName;
                if (!string.ReferenceEquals(schemaDisplayName, null))
                {
                    fullName = schemaDisplayName + "." + fullName;
                }
                if (!string.ReferenceEquals(catalogDisplayName, null))
                {
                    fullName = catalogDisplayName + "." + fullName;
                }
                return fullName;
            }
        }

        public virtual List<foreignKey> ForeignKeys
        {
            get
            {
                return foreignKeys;
            }
        }

        public virtual List<index> Indices
        {
            get
            {
                return indices;
            }
        }

        public virtual List<unique> Uniques
        {
            get
            {
                return uniques;
            }
        }

        public virtual bool View
        {
            get
            {
                return isView;
            }
            set
            {
                this.isView = value;
            }
        }

    }
}