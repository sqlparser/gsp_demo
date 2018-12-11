using System;
using System.Collections.Generic;
using System.Text;

namespace demos.dlineage.model.metadata
{


	public class MetaScanner
	{

		private Dlineage dlineage;

		public MetaScanner(Dlineage dlineage)
		{
			this.dlineage = dlineage;
		}

		public virtual ColumnMetaData getColumnMetaData(string columnFullName)
		{
			IList<string> segments = parseNames(columnFullName);

			string database = null;
			string tableSchema = null;
			string tableName = null;
			string columnName = null;

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

		private static IList<string> parseNames(string nameString)
		{
			string[] splits = nameString.ToUpper().Split(new char[] { '.' });
			IList<string> names = new List<string>();
			for (int i = 0; i < splits.Length; i++)
			{
				string split = splits[i].Trim();
				if (split.StartsWith("[", StringComparison.Ordinal) && !split.EndsWith("]", StringComparison.Ordinal))
				{
					StringBuilder buffer = new StringBuilder();
					buffer.Append(split);
					while (!(split = splits[++i].Trim()).EndsWith("]", StringComparison.Ordinal))
					{
						buffer.Append(".");
						buffer.Append(split);
					}

					buffer.Append(".");
					buffer.Append(split);

					names.Add(buffer.ToString());
					continue;
				}
				if (split.StartsWith("\"", StringComparison.Ordinal) && !split.EndsWith("\"", StringComparison.Ordinal))
				{
					StringBuilder buffer = new StringBuilder();
					buffer.Append(split);
					while (!(split = splits[++i].Trim()).EndsWith("\"", StringComparison.Ordinal))
					{
						buffer.Append(".");
						buffer.Append(split);
					}

					buffer.Append(".");
					buffer.Append(split);

					names.Add(buffer.ToString());
					continue;
				}
				names.Add(split);
			}
			return names;
		}

		public virtual ColumnMetaData getColumnMetaData(string tableFullName, string columnName)
		{
			IList<string> segments = parseNames(tableFullName);

			string database = null;
			string tableSchema = null;
			string tableName = null;

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

			return getColumnMetaData(getTableMetaData(database, tableSchema, tableName), columnName);
		}
	}

}