using gudusoft.gsqlparser.demos.dlineage.util;
using System;
using System.Collections.Generic;
using System.Text;

namespace gudusoft.gsqlparser.demos.dlineage.model.metadata
{


	public class MetaScanner
	{

		private DlineageCommon dlineage;

		public MetaScanner(DlineageCommon dlineage)
		{
			this.dlineage = dlineage;
		}

		public virtual ColumnMetaData getColumnMetaData(string columnFullName)
		{
			string database = null;
			string tableSchema = null;
			string tableName = null;
			string columnName = null;
            if (columnFullName.StartsWith(SQLUtil.TABLE_CONSTANT))
            {
                tableName = SQLUtil.TABLE_CONSTANT;
                columnName = columnFullName.Substring(SQLUtil.TABLE_CONSTANT.Length + 1);
            }
            else
            {
                IList<string> segments = SQLUtil.parseNames(columnFullName);

                if (segments.Count == 4)
                {
                    database = segments[0];
                    segments.RemoveAt(0);
                }

                if (segments.Count == 3)
                {
                    tableSchema = segments[0];
                    segments.RemoveAt(0);
                }

                if (segments.Count == 2)
                {
                    tableName = segments[0];
                    segments.RemoveAt(0);
                }

                columnName = segments[0];
            }

			return getColumnMetaData(getTableMetaData(database, tableSchema, tableName), columnName);
		}

		public virtual ColumnMetaData ColumnMetaData(string database, string tableSchema, string tableName, string columnName)
		{
			return getColumnMetaData(getTableMetaData(database, tableSchema, tableName), columnName);
		}

		public virtual TableMetaData getTableMetaData(string database, string tableSchema, string tableName)
		{
			TableMetaData tableMetaData = new TableMetaData(dlineage.Vendor, dlineage.Strict);
			tableMetaData.Name = tableName;
			tableMetaData.SchemaName = tableSchema;
			if (!string.ReferenceEquals(database, null))
			{
				tableMetaData.CatalogName = database;
			}
			tableMetaData = getTableMetaData(tableMetaData);
			return tableMetaData;
		}

		public virtual ColumnMetaData getColumnMetaData(TableMetaData tableMetaData, string columnName)
		{
			ColumnMetaData columnMetaData = new ColumnMetaData();
			columnMetaData.Name = columnName;
			columnMetaData.Table = tableMetaData;

			if (dlineage.MetaData[tableMetaData] == null)
			{
				return null;
			}
			int index = dlineage.MetaData[tableMetaData].IndexOf(columnMetaData);
			if (index != -1)
			{
				columnMetaData = dlineage.MetaData[tableMetaData][index];
			}
			else
			{
				return null;
			}
			return columnMetaData;
		}

		private TableMetaData getTableMetaData(TableMetaData tableMetaData)
		{
			IList<TableMetaData> tables = new List<TableMetaData>(dlineage.MetaData.Keys);
			int index = tables.IndexOf(tableMetaData);
			if (index != -1)
			{
				return tables[index];
			}
			else
			{
				return null;
			}
		}

		public virtual ColumnMetaData getColumnMetaData(string tableFullName, string columnName)
		{
			string database = null;
			string tableSchema = null;
			string tableName = null;
            if (!tableFullName.Equals(SQLUtil.TABLE_CONSTANT))
            {
                IList<string> segments = SQLUtil.parseNames(tableFullName);
                if (segments.Count == 3)
                {
                    database = segments[0];
                    segments.RemoveAt(0);
                }

                if (segments.Count == 2)
                {
                    tableSchema = segments[0];
                    segments.RemoveAt(0);
                }

                if (segments.Count == 1)
                {
                    tableName = segments[0];
                    segments.RemoveAt(0);
                }
            }
            else
            {
                tableName = tableFullName;
            }

            return getColumnMetaData(getTableMetaData(database, tableSchema, tableName), columnName);
		}
	}

}