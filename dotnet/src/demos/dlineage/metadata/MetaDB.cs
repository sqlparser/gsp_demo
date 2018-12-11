using System;
using System.Collections.Generic;

namespace demos.dlineage.metadata
{

	using IMetaDatabase = gudusoft.gsqlparser.IMetaDatabase;


	using ColumnMetaData = demos.dlineage.model.metadata.ColumnMetaData;
	using TableMetaData = demos.dlineage.model.metadata.TableMetaData;
	using SQLUtil = demos.dlineage.util.SQLUtil;

	public class MetaDB : IMetaDatabase
	{

		private string[][] columns;
		private bool strict = false;

		public MetaDB(IDictionary<TableMetaData, IList<ColumnMetaData>> metaMap, bool strict)
		{
			List<string[]> columnList = new List<string[]>();
			if (metaMap != null)
			{
				IEnumerator<TableMetaData> tableIter = metaMap.Keys.GetEnumerator();
				while (tableIter.MoveNext())
				{
					TableMetaData table = tableIter.Current;
					IList<ColumnMetaData> columnMetadatas = metaMap[table];
					for (int i = 0; i < columnMetadatas.Count; i++)
					{
						ColumnMetaData columnMetadata = columnMetadatas[i];
						string[] column = new string[5];
						column[0] = "";
						column[1] = columnMetadata.Table.CatalogName;
						column[2] = columnMetadata.Table.SchemaName;
						column[3] = columnMetadata.Table.Name;
						column[4] = columnMetadata.Name;
						columnList.Add(column);
					}
				}
			}
			columns = columnList.ToArray();
			this.strict = strict;
		}

		public virtual bool checkColumn(string server, string database, string schema, string table, string column)
		{
			bool bServer , bDatabase , bSchema , bTable , bColumn , bRet = false;
			for (int i = 0; i < columns.Length; i++)
			{
				if (strict)
				{
					if ((string.ReferenceEquals(server, null)) || (server.Length == 0))
					{
						bServer = true;
					}
					else
					{
						bServer = columns[i][0].Equals(SQLUtil.trimObjectName(server), StringComparison.CurrentCultureIgnoreCase);
					}
					if (!bServer)
					{
						continue;
					}

					if ((string.ReferenceEquals(database, null)) || (database.Length == 0))
					{
						bDatabase = true;
					}
					else
					{
						bDatabase = columns[i][1].Equals(SQLUtil.trimObjectName(database), StringComparison.CurrentCultureIgnoreCase);
					}
					if (!bDatabase)
					{
						continue;
					}

					if ((string.ReferenceEquals(schema, null)) || (schema.Length == 0))
					{
						bSchema = true;
					}
					else
					{
						bSchema = columns[i][2].Equals(SQLUtil.trimObjectName(schema), StringComparison.CurrentCultureIgnoreCase);
					}

					if (!bSchema)
					{
						continue;
					}
				}

				bTable = columns[i][3].Equals(SQLUtil.trimObjectName(table), StringComparison.CurrentCultureIgnoreCase);
				if (!bTable)
				{
					continue;
				}

				bColumn = columns[i][4].Equals(SQLUtil.trimObjectName(column), StringComparison.CurrentCultureIgnoreCase);
				if (!bColumn)
				{
					continue;
				}

				bRet = true;
				break;

			}

			return bRet;
		}
	}

}