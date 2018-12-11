using demos.dlineage.model.ddl.schema;
using System;
using System.Collections.Generic;

namespace demos.dlineage.metadata
{
    using column = demos.dlineage.model.ddl.schema.column;
    using database = demos.dlineage.model.ddl.schema.database;
    using table = demos.dlineage.model.ddl.schema.table;
    using ColumnMetaData = demos.dlineage.model.metadata.ColumnMetaData;
    using TableMetaData = demos.dlineage.model.metadata.TableMetaData;
    using demos.util;
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;

    public class DDLSchema
	{

		
		private string result;
		private database[] databases;

		public virtual string SchemaXML
		{
			get
			{
				return result;
			}
		}

		public virtual database[] DataMetaInfos
		{
			get
			{
				return databases;
			}
		}

		private IDictionary<TableMetaData, IList<ColumnMetaData>> tableColumns;

		public DDLSchema(IDictionary<TableMetaData, IList<ColumnMetaData>> tableColumns)
		{
			this.tableColumns = tableColumns;
			databases = collectDDLInfo();
			if (databases != null)
			{
				try
				{
                    StringBuilder sw = new StringBuilder();
                    StringWriter writer = new StringWriter(sw);
					writer.WriteLine("<?xml version=\"1.0\"?>");
					writer.WriteLine("<!DOCTYPE database SYSTEM \"http://db.apache.org/torque/dtd/database.dtd\">");
					if (databases.Length > 1)
					{
						writer.WriteLine("<dbSchema>");
					}

					writer.Write(getDatabaseSchema(databases));

					if (databases.Length > 1)
					{
						writer.WriteLine("</dbSchema>");
					}
					result = sw.ToString();
					writer.Close();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
			if (string.ReferenceEquals(result, null))
			{
				result = "";
			}
		}

		private string getDatabaseSchema(database[] databases)
		{
            StringBuilder sw = new StringBuilder();
            StringWriter writer = new StringWriter(sw);
            for (int i = 0; i < databases.Length; i++)
			{
				database db = databases[i];
				try
				{
                    XmlSerializer serializer = new XmlSerializer(typeof(database));
                    serializer.Serialize(writer, db);
					writer.WriteLine();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
			string schema = sw.ToString();
			writer.Close();

			if (databases.Length > 1)
			{
				sw = new StringBuilder();
				writer = new StringWriter(sw);
                StringReader reader = new StringReader(schema);
				string line = null;
				while (!string.ReferenceEquals((line = reader.ReadLine()), null))
				{
					writer.Write("   ");
					writer.WriteLine(line);
				}
				schema = sw.ToString();
				writer.Close();
			}
			return schema;
		}

		private database[] collectDDLInfo()
		{
            LinkedHashMap<string, database> databaseMap = new LinkedHashMap<string, database>();
			if (tableColumns != null)
			{
				IEnumerator<TableMetaData> tableIter = tableColumns.Keys.GetEnumerator();
				while (tableIter.MoveNext())
				{
					TableMetaData tableMetadata = tableIter.Current;

					string databaseName = tableMetadata.CatalogDisplayName;
					if (string.ReferenceEquals(databaseName, null))
					{
						databaseName = "unknown";
					}
					if (!databaseMap.ContainsKey(databaseName))
					{
						database database = new database();
                        database.name = databaseName;
						databaseMap[databaseName] = database;
					}

					database datasource = databaseMap[databaseName];
					table table = new table();
					if (!string.ReferenceEquals(tableMetadata.SchemaDisplayName, null))
					{
						table.name = tableMetadata.SchemaDisplayName + "." + tableMetadata.DisplayName;
					}
					else
					{
						table.name = tableMetadata.DisplayName;
					}

					if (tableMetadata.View)
					{
						table.isView = true.ToString();
					}
					if (tableMetadata.Indices != null && tableMetadata.Indices.Count > 0)
					{
						table.indices = tableMetadata.Indices.ToArray();
					}
					if (tableMetadata.Uniques != null && tableMetadata.Uniques.Count > 0)
					{
						table.uniques = tableMetadata.Uniques.ToArray();
					}
					if (tableMetadata.ForeignKeys != null && tableMetadata.ForeignKeys.Count > 0)
					{
						table.foreignKeys = tableMetadata.ForeignKeys.ToArray();
					}

                    List<table> tables = new List<table>();
                    if (datasource.tables != null)
                        tables.AddRange(datasource.tables);
                    tables.Add(table);
                    tables.Sort();
                    datasource.tables = tables.ToArray();

					IList<ColumnMetaData> columnMetadatas = tableColumns[tableMetadata];
					if (columnMetadatas != null)
					{
						for (int i = 0; i < columnMetadatas.Count; i++)
						{
							ColumnMetaData columnMetadata = columnMetadatas[i];
							column column = new column();
							column.name = columnMetadata.DisplayName;
							column.type = columnMetadata.Type;
							column.size = columnMetadata.Size;
							column.primaryKey = columnMetadata.isPrimaryKey();
							column.description = columnMetadata.Comment;
							column.defaultValue = columnMetadata.DefaultValue;
							column.required = columnMetadata.Required;
							column.autoIncrement = columnMetadata.AutoIncrease;

                            List<column> columns = new List<column>();
                            if (table.columns != null)
                                columns.AddRange(table.columns);
                            columns.Add(column);
                            table.columns = columns.ToArray();
						}
					}
				}
			}
			if (databaseMap.Count>0)
			{
                List<database> databases = new List<database>();

                IEnumerator<string> databaseIter = databaseMap.Keys.GetEnumerator();
                while (databaseIter.MoveNext())
                {
                    database database = databaseMap[databaseIter.Current];
                    databases.Add(database);
                }
 				return databases.ToArray();
			}
			else
			{
				return null;
			}
		}
	}

}