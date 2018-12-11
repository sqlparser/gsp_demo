using System;
using System.Collections.Generic;
using System.Text;

namespace demos.dlineage.metadata
{

	using EDbVendor = gudusoft.gsqlparser.EDbVendor;
	using ESetOperatorType = gudusoft.gsqlparser.ESetOperatorType;
	using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
	using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
	using TStatementList = gudusoft.gsqlparser.TStatementList;
	using TColumnDefinitionList = gudusoft.gsqlparser.nodes.TColumnDefinitionList;
	using TObjectNameList = gudusoft.gsqlparser.nodes.TObjectNameList;
	using TResultColumn = gudusoft.gsqlparser.nodes.TResultColumn;
	using TViewAliasItemList = gudusoft.gsqlparser.nodes.TViewAliasItemList;
	using TCreateTableSqlStatement = gudusoft.gsqlparser.stmt.TCreateTableSqlStatement;
	using TCreateViewSqlStatement = gudusoft.gsqlparser.stmt.TCreateViewSqlStatement;
	using TInsertSqlStatement = gudusoft.gsqlparser.stmt.TInsertSqlStatement;
	using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;
	using TUseDatabase = gudusoft.gsqlparser.stmt.TUseDatabase;


	using ColumnImpact = demos.dlineage.columnImpact.ColumnImpact;
	using ColumnMetaData = demos.dlineage.model.metadata.ColumnMetaData;
	using TableMetaData = demos.dlineage.model.metadata.TableMetaData;
	using ColumnImpactModel = demos.dlineage.model.view.ColumnImpactModel;
	using ColumnModel = demos.dlineage.model.view.ColumnModel;
	using ReferenceModel = demos.dlineage.model.view.ReferenceModel;
	using SQLUtil = demos.dlineage.util.SQLUtil;

	public class ViewParser
	{

		private IDictionary<TableMetaData, IList<ColumnMetaData>> tableColumns;
		private bool strict = false;
		private EDbVendor vendor = EDbVendor.dbvmssql;
		private string database = null;

		public ViewParser(IDictionary<TableMetaData, IList<ColumnMetaData>> tableColumns, EDbVendor vendor, string sqlText, bool strict, string database)
		{
			this.database = database;
			this.strict = strict;
			this.vendor = vendor;
			this.tableColumns = tableColumns;
			TGSqlParser parser = new TGSqlParser(vendor);
			parser.sqltext = sqlText;
			parser.MetaDatabase = new MetaDB(tableColumns, strict);
			checkDDL(parser);
		}

		private void checkDDL(TGSqlParser sqlparser)
		{
			int ret = sqlparser.parse();
			if (ret == 0)
			{
				TStatementList stmts = sqlparser.sqlstatements;
				for (int i = 0; i < stmts.size(); i++)
				{
					TCustomSqlStatement stmt = stmts.get(i);
					parseStatement(stmt);
				}
			}
		}

		private void parseStatement(TCustomSqlStatement stmt)
		{
			if (stmt is TCreateViewSqlStatement)
			{
				TCreateViewSqlStatement createView = ((TCreateViewSqlStatement) stmt);
				parseCreateView(createView);
			}
			else if (stmt is TCreateTableSqlStatement && ((TCreateTableSqlStatement) stmt).SubQuery != null)
			{
				TCreateTableSqlStatement createTable = ((TCreateTableSqlStatement) stmt);
				parseCreateTable(createTable);
			}
			else if (stmt is TInsertSqlStatement && ((TInsertSqlStatement) stmt).SubQuery != null)
			{
				TInsertSqlStatement insert = ((TInsertSqlStatement) stmt);
				parseInsertStmt(insert);
			}
			if (stmt is TUseDatabase)
			{
				TUseDatabase use = (TUseDatabase) stmt;
				database = use.DatabaseName.ToString();
			}
		}

		private void parseInsertStmt(TInsertSqlStatement insert)
		{
			if (insert.TargetTable.TableName != null)
			{
				string tableName = insert.TargetTable.TableName.TableString;
				string tableSchema = insert.TargetTable.TableName.SchemaString;
				string databaseName = insert.TargetTable.TableName.DatabaseString;
				TableMetaData tableMetaData = new TableMetaData(vendor, strict);
				tableMetaData.Name = tableName;
				tableMetaData.SchemaName = tableSchema;
				if (isNotEmpty(databaseName))
				{
					tableMetaData.CatalogName = databaseName;
				}
				else
				{
					tableMetaData.CatalogName = database;
				}
				tableMetaData.View = false;
				if (!tableColumns.ContainsKey(tableMetaData))
				{
					tableColumns[tableMetaData] = new List<ColumnMetaData>();
				}
				else
				{
					IList<TableMetaData> tables = new List<TableMetaData>(tableColumns.Keys);
					tableMetaData = (TableMetaData) tables[tables.IndexOf(tableMetaData)];
					tableMetaData.View = false;
				}
				if (insert.SubQuery != null)
				{
					ColumnImpact impact = new ColumnImpact(insert.SubQuery.ToString(), insert.dbvendor, tableColumns, strict);
					impact.ignoreTopSelect(true);
					impact.Debug = false;
					impact.ShowUIInfo = true;
					impact.TraceErrorMessage = false;
					impact.impactSQL();
					ColumnImpactModel columnImpactModel = impact.generateModel();
					parseSubQueryColumnDefinition(insert, insert.SubQuery, tableMetaData, columnImpactModel);

				}
			}

		}

		private void parseCreateView(TCreateViewSqlStatement createView)
		{
			if (createView.ViewName != null)
			{
				string tableName = createView.ViewName.TableString;
				string tableSchema = createView.ViewName.SchemaString;
				string databaseName = createView.ViewName.DatabaseString;
				TableMetaData viewMetaData = new TableMetaData(vendor, strict);
				viewMetaData.Name = tableName;
				viewMetaData.SchemaName = tableSchema;
				if (isNotEmpty(databaseName))
				{
					viewMetaData.CatalogName = databaseName;
				}
				else
				{
					viewMetaData.CatalogName = database;
				}
				viewMetaData.View = true;
				if (!tableColumns.ContainsKey(viewMetaData))
				{
					tableColumns[viewMetaData] = new List<ColumnMetaData>();
				}
				else
				{
					IList<TableMetaData> tables = new List<TableMetaData>(tableColumns.Keys);
					viewMetaData = (TableMetaData) tables[tables.IndexOf(viewMetaData)];
					viewMetaData.View = true;
				}
				if (createView.Subquery != null)
				{
					ColumnImpact impact = new ColumnImpact(createView.Subquery.ToString(), createView.dbvendor, tableColumns, strict);
					impact.ignoreTopSelect(true);
					impact.Debug = false;
					impact.ShowUIInfo = true;
					impact.TraceErrorMessage = false;
					impact.impactSQL();
					ColumnImpactModel columnImpactModel = impact.generateModel();
					parseSubQueryColumnDefinition(createView, createView.Subquery, viewMetaData, columnImpactModel);

				}
			}
		}

		private void parseCreateTable(TCreateTableSqlStatement createTable)
		{
			if (createTable.TableName != null)
			{
				string tableName = createTable.TableName.TableString;
				string tableSchema = createTable.TableName.SchemaString;
				string databaseName = createTable.TableName.DatabaseString;
				TableMetaData tableMetaData = new TableMetaData(vendor, strict);
				tableMetaData.Name = tableName;
				tableMetaData.SchemaName = tableSchema;
				if (isNotEmpty(databaseName))
				{
					tableMetaData.CatalogName = databaseName;
				}
				else
				{
					tableMetaData.CatalogName = database;
				}
				tableMetaData.View = false;
				if (!tableColumns.ContainsKey(tableMetaData))
				{
					tableColumns[tableMetaData] = new List<ColumnMetaData>();
				}
				else
				{
					IList<TableMetaData> tables = new List<TableMetaData>(tableColumns.Keys);
					tableMetaData = (TableMetaData) tables[tables.IndexOf(tableMetaData)];
					tableMetaData.View = false;
				}
				if (createTable.SubQuery != null)
				{
					ColumnImpact impact = new ColumnImpact(removeParentheses(createTable.SubQuery.ToString().Trim()), createTable.dbvendor, tableColumns, strict);
					impact.ignoreTopSelect(true);
					impact.Debug = false;
					impact.ShowUIInfo = true;
					impact.TraceErrorMessage = false;
					impact.impactSQL();
					ColumnImpactModel columnImpactModel = impact.generateModel();
					parseSubQueryColumnDefinition(createTable, createTable.SubQuery, tableMetaData, columnImpactModel);

				}
			}
		}

		private string removeParentheses(string sql)
		{
			if (sql.StartsWith("(", StringComparison.Ordinal) && sql.EndsWith(")", StringComparison.Ordinal))
			{
				sql = sql.Substring(1, (sql.Length - 1) - 1).Trim();
				return removeParentheses(sql);
			}
			return sql;
		}

		private void parseSubQueryColumnDefinition(TCreateViewSqlStatement createView, TSelectSqlStatement stmt, TableMetaData viewMetaData, ColumnImpactModel columnImpactModel)
		{
			if (stmt.SetOperatorType != ESetOperatorType.none)
			{
				parseSubQueryColumnDefinition(createView, stmt.LeftStmt, viewMetaData, columnImpactModel);
				parseSubQueryColumnDefinition(createView, stmt.RightStmt, viewMetaData, columnImpactModel);
			}
			else
			{
				int columnCount = stmt.ResultColumnList.size();
				string[] aliasNames = new string[columnCount];
				if (createView.ViewAliasClause != null)
				{
					columnCount = createView.ViewAliasClause.ViewAliasItemList.size();
					aliasNames = new string[columnCount];
					TViewAliasItemList items = createView.ViewAliasClause.ViewAliasItemList;
					for (int i = 0; i < items.size(); i++)
					{
						aliasNames[i] = items.getViewAliasItem(i).Alias.ToString();
					}
				}
				for (int i = 0; i < columnCount; i++)
				{
					TResultColumn resultColumn = stmt.ResultColumnList.getResultColumn(i);
					parseColumnDefinition(resultColumn, viewMetaData, columnImpactModel, aliasNames[i]);
				}
			}
		}

		private void parseSubQueryColumnDefinition(TCreateTableSqlStatement createTable, TSelectSqlStatement stmt, TableMetaData tableMetaData, ColumnImpactModel columnImpactModel)
		{
			if (stmt.SetOperatorType != ESetOperatorType.none)
			{
				parseSubQueryColumnDefinition(createTable, stmt.LeftStmt, tableMetaData, columnImpactModel);
				parseSubQueryColumnDefinition(createTable, stmt.RightStmt, tableMetaData, columnImpactModel);
			}
			else
			{
				int columnCount = stmt.ResultColumnList.size();
				string[] aliasNames = new string[columnCount];
				if (createTable.ColumnList != null && createTable.ColumnList.size() > 0)
				{
					columnCount = createTable.ColumnList.size();
					aliasNames = new string[columnCount];
					TColumnDefinitionList items = createTable.ColumnList;
					for (int i = 0; i < items.size(); i++)
					{
						aliasNames[i] = items.getColumn(i).ToString();
					}
				}
				for (int i = 0; i < columnCount; i++)
				{
					TResultColumn resultColumn = stmt.ResultColumnList.getResultColumn(i);
					parseColumnDefinition(resultColumn, tableMetaData, columnImpactModel, aliasNames[i]);
				}
			}
		}

		private void parseSubQueryColumnDefinition(TInsertSqlStatement insert, TSelectSqlStatement stmt, TableMetaData tableMetaData, ColumnImpactModel columnImpactModel)
		{
			if (stmt.SetOperatorType != ESetOperatorType.none)
			{
				parseSubQueryColumnDefinition(insert, stmt.LeftStmt, tableMetaData, columnImpactModel);
				parseSubQueryColumnDefinition(insert, stmt.RightStmt, tableMetaData, columnImpactModel);
			}
			else
			{
				if (insert.ColumnList != null)
				{
					TObjectNameList items = insert.ColumnList;
					int columnCount = items.size();
					string[] aliasNames = new string[columnCount];

					for (int i = 0; i < items.size(); i++)
					{
						aliasNames[i] = items.getObjectName(i).ToString();
					}

					for (int i = 0; i < columnCount; i++)
					{
						TResultColumn resultColumn = null;
						if (i < stmt.ResultColumnList.size())
						{
							resultColumn = stmt.ResultColumnList.getResultColumn(i);
						}
						else
						{
							resultColumn = stmt.ResultColumnList.getResultColumn(stmt.ResultColumnList.size() - 1);
						}
						parseInsertColumnDefinition(resultColumn, tableMetaData, columnImpactModel, aliasNames[i]);
					}
				}
			}
		}

		private void parseInsertColumnDefinition(TResultColumn resultColumn, TableMetaData viewMetaData, ColumnImpactModel columnImpactModel, string aliasName)
		{
			ColumnMetaData columnMetaData = new ColumnMetaData();
			columnMetaData.Table = viewMetaData;

			if (!string.ReferenceEquals(aliasName, null))
			{
				columnMetaData.DisplayName = aliasName;
				columnMetaData.Name = aliasName;
			}

			if (tableColumns[viewMetaData] == null)
			{
				return;
			}

			int index = tableColumns[viewMetaData].IndexOf(columnMetaData);
			if (index != -1)
			{
				columnMetaData = tableColumns[viewMetaData][index];
			}
			else
			{
				tableColumns[viewMetaData].Add(columnMetaData);
			}

			ColumnMetaData[] referColumns = getRefTableColumns(resultColumn, columnImpactModel);
			for (int i = 0; i < referColumns.Length; i++)
			{
				columnMetaData.addReferColumn(referColumns[i]);
			}
		}

		private void parseColumnDefinition(TResultColumn resultColumn, TableMetaData viewMetaData, ColumnImpactModel columnImpactModel, string aliasName)
		{
			ColumnMetaData columnMetaData = new ColumnMetaData();
			columnMetaData.Table = viewMetaData;

			if (resultColumn != null)
			{
				if (resultColumn.AliasClause != null)
				{
					columnMetaData.Name = resultColumn.AliasClause.AliasName.ToString();
				}
				else if (isNotEmpty(resultColumn.ColumnNameOnly))
				{
					columnMetaData.Name = resultColumn.ColumnNameOnly;
				}
				else
				{
					columnMetaData.Name = resultColumn.ToString();
				}
			}
			if (!string.ReferenceEquals(aliasName, null))
			{
				columnMetaData.DisplayName = aliasName;
			}

			if (tableColumns[viewMetaData] == null)
			{
				return;
			}

			int index = tableColumns[viewMetaData].IndexOf(columnMetaData);
			if (index != -1)
			{
				columnMetaData = tableColumns[viewMetaData][index];
			}
			else
			{
				tableColumns[viewMetaData].Add(columnMetaData);
			}

			if (resultColumn != null)
			{
				ColumnMetaData[] referColumns = getRefTableColumns(resultColumn, columnImpactModel);
				for (int i = 0; i < referColumns.Length; i++)
				{
					columnMetaData.addReferColumn(referColumns[i]);
				}
			}
		}

		private ColumnMetaData[] getRefTableColumns(TResultColumn resultColumn, ColumnImpactModel columnImpactModel)
		{
			ReferenceModel[] referenceModels = columnImpactModel.References;
			List<ColumnMetaData> columns = new List<ColumnMetaData>();
			if (resultColumn.AliasClause != null)
			{
				for (int i = 0; i < referenceModels.Length; i++)
				{
					ReferenceModel referModel = referenceModels[i];
					// if ( referModel.getClause( ) != Clause.SELECT )
					// continue;
					if (referModel.Alias != null)
					{
						string aliasName = resultColumn.AliasClause.AliasName.ToString();
						if (removeQuote(referModel.Alias.Name).Equals(removeQuote(aliasName), StringComparison.CurrentCultureIgnoreCase))
						{
							ColumnMetaData columnMetaData = getColumn(referModel.Column);
							if (columnMetaData != null && !columns.Contains(columnMetaData))
							{
								columns.Add(columnMetaData);
							}
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < referenceModels.Length; i++)
				{
					ReferenceModel referModel = referenceModels[i];
					// if ( referModel.getClause( ) != Clause.SELECT )
					// continue;
					if (referModel.Field != null)
					{
						if (resultColumn.FieldAttr != null)
						{
							if (removeQuote(resultColumn.FieldAttr.ToString()).Equals(removeQuote(referModel.Field.FullName), StringComparison.CurrentCultureIgnoreCase))
							{
								ColumnMetaData columnMetaData = getColumn(referModel.Column);
								if (!columns.Contains(columnMetaData))
								{
									columns.Add(columnMetaData);
								}
							}
						}
						else
						{
							if (removeQuote(resultColumn.ToString()).Equals(removeQuote(referModel.Field.FullName), StringComparison.CurrentCultureIgnoreCase))
							{
								ColumnMetaData columnMetaData = getColumn(referModel.Column);
								if (!columns.Contains(columnMetaData))
								{
									columns.Add(columnMetaData);
								}

							}
						}
					}
				}
			}
			return columns.ToArray();
		}

		private string removeQuote(string @string)
		{
			if (!string.ReferenceEquals(@string, null) && @string.IndexOf('.') != -1)
			{
				string[] splits = @string.Split(new char[] { '.' });
				StringBuilder result = new StringBuilder();
				for (int i = 0; i < splits.Length; i++)
				{
					result.Append(removeQuote(splits[i]));
					if (i < splits.Length - 1)
					{
						result.Append(".");
					}
				}
				return result.ToString();
			}
			return SQLUtil.trimObjectName(@string);
		}

		private ColumnMetaData getColumn(ColumnModel column)
		{
			if (column == null)
			{
				return null;
			}
			string tableFullName = column.Table.FullName;
			IList<string> splits = new List<string>(tableFullName.Split(new char[] { '.' }));
			ColumnMetaData columnMetadata = new ColumnMetaData();
			columnMetadata.Name = column.Name;
			TableMetaData tableMetaData = new TableMetaData(vendor, strict);
			if (splits.Count > 0)
			{
				tableMetaData.Name = splits[splits.Count - 1];
			}
			if (splits.Count > 1)
			{
				tableMetaData.SchemaName = splits[splits.Count - 2];
			}
			if (splits.Count > 2)
			{
				tableMetaData.CatalogName = splits[splits.Count - 3];
			}
			if (!tableColumns.ContainsKey(tableMetaData))
			{
				tableColumns[tableMetaData] = new List<ColumnMetaData>();
			}
			else
			{
				IList<TableMetaData> tables = new List<TableMetaData>(tableColumns.Keys);
				tableMetaData = tables[tables.IndexOf(tableMetaData)];
			}
			columnMetadata.Table = tableMetaData;
			IList<ColumnMetaData> columns = tableColumns[tableMetaData];
			if (columns.Contains(columnMetadata))
			{
				columnMetadata = columns[columns.IndexOf(columnMetadata)];
			}
			else
			{
				columns.Add(columnMetadata);
			}
			return columnMetadata;
		}

		private static bool isNotEmpty(string str)
		{
			return !string.ReferenceEquals(str, null) && str.Trim().Length > 0;
		}

		public virtual string Database
		{
			get
			{
				return database;
			}
		}
	}

}