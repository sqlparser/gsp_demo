using System;
using System.Collections.Generic;
using System.Text;

namespace gudusoft.gsqlparser.demos.dlineage.metadata
{

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using EKeyActionType = gudusoft.gsqlparser.EKeyActionType;
    using ESetOperatorType = gudusoft.gsqlparser.ESetOperatorType;
    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using TStatementList = gudusoft.gsqlparser.TStatementList;
    using TAlterTableOption = gudusoft.gsqlparser.nodes.TAlterTableOption;
    using TColumnDefinition = gudusoft.gsqlparser.nodes.TColumnDefinition;
    using TConstraint = gudusoft.gsqlparser.nodes.TConstraint;
    using TKeyAction = gudusoft.gsqlparser.nodes.TKeyAction;
    using TMergeWhenClause = gudusoft.gsqlparser.nodes.TMergeWhenClause;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using TResultColumn = gudusoft.gsqlparser.nodes.TResultColumn;
    using TResultColumnList = gudusoft.gsqlparser.nodes.TResultColumnList;
    using TTableElement = gudusoft.gsqlparser.nodes.TTableElement;
    using TTypeName = gudusoft.gsqlparser.nodes.TTypeName;
    using TAlterTableStatement = gudusoft.gsqlparser.stmt.TAlterTableStatement;
    using TCreateIndexSqlStatement = gudusoft.gsqlparser.stmt.TCreateIndexSqlStatement;
    using TCreateTableSqlStatement = gudusoft.gsqlparser.stmt.TCreateTableSqlStatement;
    using TInsertSqlStatement = gudusoft.gsqlparser.stmt.TInsertSqlStatement;
    using TMergeSqlStatement = gudusoft.gsqlparser.stmt.TMergeSqlStatement;
    using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;
    using TStoredProcedureSqlStatement = gudusoft.gsqlparser.stmt.TStoredProcedureSqlStatement;
    using TUpdateSqlStatement = gudusoft.gsqlparser.stmt.TUpdateSqlStatement;
    using TUseDatabase = gudusoft.gsqlparser.stmt.TUseDatabase;
    using TOracleCommentOnSqlStmt = gudusoft.gsqlparser.stmt.oracle.TOracleCommentOnSqlStmt;


    using ColumnImpact = gudusoft.gsqlparser.demos.dlineage.columnImpact.ColumnImpact;
    using foreignKey = gudusoft.gsqlparser.demos.dlineage.model.ddl.schema.foreignKey;
    using index = gudusoft.gsqlparser.demos.dlineage.model.ddl.schema.index;
    using indexColumn = gudusoft.gsqlparser.demos.dlineage.model.ddl.schema.indexColumn;
    using reference = gudusoft.gsqlparser.demos.dlineage.model.ddl.schema.reference;
    using unique = gudusoft.gsqlparser.demos.dlineage.model.ddl.schema.unique;
    using uniqueColumn = gudusoft.gsqlparser.demos.dlineage.model.ddl.schema.uniqueColumn;
    using ColumnMetaData = gudusoft.gsqlparser.demos.dlineage.model.metadata.ColumnMetaData;
    using ProcedureMetaData = gudusoft.gsqlparser.demos.dlineage.model.metadata.ProcedureMetaData;
    using TableMetaData = gudusoft.gsqlparser.demos.dlineage.model.metadata.TableMetaData;
    using ColumnImpactModel = gudusoft.gsqlparser.demos.dlineage.model.view.ColumnImpactModel;
    using ColumnModel = gudusoft.gsqlparser.demos.dlineage.model.view.ColumnModel;
    using ReferenceModel = gudusoft.gsqlparser.demos.dlineage.model.view.ReferenceModel;
    using procedureImpactResult = gudusoft.gsqlparser.demos.dlineage.model.xml.procedureImpactResult;
    using SQLUtil = gudusoft.gsqlparser.demos.dlineage.util.SQLUtil;
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.nodes;

    public class DDLParser
	{

		private IDictionary<TableMetaData, IList<ColumnMetaData>> tableColumns;
		private Tuple<procedureImpactResult, IList<ProcedureMetaData>> procedures;
		private bool strict = false;
		private string database = null;
		private EDbVendor vendor = EDbVendor.dbvmssql;
        private List<String> stmtList = new List<String>();

		public DDLParser(IDictionary<TableMetaData, IList<ColumnMetaData>> tableColumns, Tuple<procedureImpactResult, IList<ProcedureMetaData>> procedures, EDbVendor vendor, string sqlText, bool strict, string database)
		{
			this.strict = strict;
			this.vendor = vendor;
			this.database = database;
			this.tableColumns = tableColumns;
			this.procedures = procedures;
			TGSqlParser parser = new TGSqlParser(vendor);
			parser.sqltext = sqlText;
			checkDDL(parser);
            stmtList.Clear();

        }

		private void checkDDL(TGSqlParser sqlparser)
		{
			int ret = sqlparser.parse();
			if (ret == 0)
			{
				TStatementList stmts = sqlparser.sqlstatements;
				parseStatementList(stmts);
			}
		}

		private void parseStatementList(TStatementList stmts)
		{
			for (int i = 0; i < stmts.size(); i++)
			{
				TCustomSqlStatement stmt = stmts.get(i);
                parseStatement(stmt);
			}
		}

		private void parseStatement(TCustomSqlStatement stmt)
		{
            if (!stmtList.Contains(stmt.ToString()))
            {
                stmtList.Add(stmt.ToString());
            }else
            {
                return;
            }
            if (stmt is TCreateTableSqlStatement && ((TCreateTableSqlStatement) stmt).SubQuery == null)
			{
				TCreateTableSqlStatement createTable = (TCreateTableSqlStatement) stmt;
				parseCreateTable(createTable);
			}
			else if (stmt is TOracleCommentOnSqlStmt)
			{
				TOracleCommentOnSqlStmt commentOn = (TOracleCommentOnSqlStmt) stmt;
				parseCommentOn(commentOn);
			}
			else if (stmt is TAlterTableStatement)
			{
				TAlterTableStatement alterTable = (TAlterTableStatement) stmt;
				TableMetaData tableMetaData = new TableMetaData(vendor, strict);
				tableMetaData.Name = alterTable.TableName.TableString;
				tableMetaData.SchemaName = alterTable.TableName.SchemaString;
				if (isNotEmpty(alterTable.TableName.DatabaseString))
				{
					tableMetaData.CatalogName = alterTable.TableName.DatabaseString;
				}
				else
				{
					tableMetaData.CatalogName = database;
				}
				tableMetaData = getTableMetaData(tableMetaData);

				parseAlterTable(alterTable, tableMetaData);
			}
			else if (stmt is TCreateIndexSqlStatement)
			{
				TCreateIndexSqlStatement createIndex = (TCreateIndexSqlStatement) stmt;
				parseCreateIndex(createIndex);
			}
			else if (stmt is TUseDatabase)
			{
				TUseDatabase use = (TUseDatabase) stmt;
				database = use.DatabaseName.ToString();
			}
			else if (stmt is TSelectSqlStatement)
			{
				TSelectSqlStatement selectStmt = (TSelectSqlStatement) stmt;
				parseSelectStmt(selectStmt);
			}
			else if (stmt is TInsertSqlStatement && ((TInsertSqlStatement) stmt).SubQuery == null)
			{
				TInsertSqlStatement insertStmt = (TInsertSqlStatement) stmt;
				parseInsertStmt(insertStmt);
			}
			else if (stmt is TUpdateSqlStatement)
			{
				TUpdateSqlStatement updateStmt = (TUpdateSqlStatement) stmt;
				parseUpdateStmt(updateStmt);
			}
			else if (stmt is TMergeSqlStatement)
			{
				TMergeSqlStatement mergeStmt = (TMergeSqlStatement) stmt;
				parseMergeStmt(mergeStmt);
			}
			else if (stmt is TStoredProcedureSqlStatement)
			{
				TStoredProcedureSqlStatement procedureStmt = (TStoredProcedureSqlStatement) stmt;
				parseProcedureStmt(procedureStmt);

				if (stmt.Statements != null && stmt.Statements.size() > 0)
				{
					parseStatementList(stmt.Statements);
				}
			}
			else if (stmt.Statements != null && stmt.Statements.size() > 0)
			{
				parseStatementList(stmt.Statements);
			}
			else
			{
				// System.err.println( stmt );
			}
		}

		private void parseProcedureStmt(TStoredProcedureSqlStatement procedureStmt)
		{
            if(procedureStmt.StoredProcedureName == null)
            {
                return;
            }
			ProcedureMetaData procedureMetaData = getProcedureMetaData(procedureStmt.StoredProcedureName);
			procedureMetaData = getProcedureMetaData(procedureMetaData, true);
			string stmtType = Enum.GetName(typeof(ESqlStatementType), procedureStmt.sqlstatementtype).ToLower().Trim();
			if (stmtType.EndsWith("procedure", StringComparison.Ordinal))
			{
				procedureMetaData.Function = false;
				procedureMetaData.Trigger = false;
			}
			else if (stmtType.EndsWith("function", StringComparison.Ordinal))
			{
				procedureMetaData.Function = true;
				procedureMetaData.Trigger = false;
			}
			else if (stmtType.EndsWith("trigger", StringComparison.Ordinal))
			{
				procedureMetaData.Function = false;
				procedureMetaData.Trigger = true;
			}

		}

		public virtual ProcedureMetaData getProcedureMetaData(ProcedureMetaData parentProcedure, TObjectName procedureName)
		{
			ProcedureMetaData procedureMetaData = new ProcedureMetaData(vendor, strict);
			procedureMetaData.Name = procedureName.PartString == null ? procedureName.ObjectString : procedureName.PartString;
			if (procedureName.SchemaString != null)
			{
				procedureMetaData.SchemaName = procedureName.SchemaString;
			}
			else
			{
				procedureMetaData.SchemaName = parentProcedure.SchemaName;
				procedureMetaData.SchemaDisplayName = parentProcedure.SchemaDisplayName;
			}

			if (isNotEmpty(procedureName.DatabaseString))
			{
				procedureMetaData.CatalogName = procedureName.DatabaseString;
			}
			else
			{
				procedureMetaData.CatalogName = parentProcedure.CatalogName;
				procedureMetaData.CatalogDisplayName = parentProcedure.CatalogDisplayName;
			}
			return procedureMetaData;
		}

		private ProcedureMetaData getProcedureMetaData(TObjectName procedureName)
		{
			ProcedureMetaData procedureMetaData = new ProcedureMetaData(vendor, strict);
			procedureMetaData.Name = procedureName.PartString == null ? procedureName.ObjectString : procedureName.PartString;
			procedureMetaData.SchemaName = procedureName.SchemaString;
			if (isNotEmpty(procedureName.DatabaseString))
			{
				procedureMetaData.CatalogName = procedureName.DatabaseString;
			}
			else
			{
				procedureMetaData.CatalogName = database;
			}
			return procedureMetaData;
		}

		private void parseInsertStmt(TInsertSqlStatement insertStmt)
		{
			ColumnImpact impact = new ColumnImpact(insertStmt.ToString(), insertStmt.dbvendor, tableColumns, strict);
			impact.Debug = false;
			impact.ShowUIInfo = true;
			impact.TraceErrorMessage = false;
			impact.impactSQL();
			ColumnImpactModel columnImpactModel = impact.generateModel();

			if (insertStmt.SubQuery != null)
			{
				parseStatement(insertStmt.SubQuery);
			}

			if (insertStmt.ResultColumnList != null)
			{
				int columnCount = insertStmt.ResultColumnList.size();

				for (int i = 0; i < columnCount; i++)
				{
					TResultColumn resultColumn = insertStmt.ResultColumnList.getResultColumn(i);
					parseColumnDefinition(resultColumn, columnImpactModel);
				}
			}

			if (insertStmt.ColumnList != null)
			{
				int columnCount = insertStmt.ColumnList.size();

				TableMetaData tableMetaData = new TableMetaData(vendor, strict);
				tableMetaData.Name = insertStmt.TargetTable.TableName.TableString;
				tableMetaData.SchemaName = insertStmt.TargetTable.TableName.SchemaString;
				if (isNotEmpty(insertStmt.TargetTable.TableName.DatabaseString))
				{
					tableMetaData.CatalogName = insertStmt.TargetTable.TableName.DatabaseString;
				}
				else
				{
					tableMetaData.CatalogName = database;
				}
				tableMetaData = getTableMetaData(tableMetaData);

				for (int i = 0; i < columnCount; i++)
				{
					TObjectName resultColumn = insertStmt.ColumnList.getObjectName(i);
					getColumnMetaData(tableMetaData, resultColumn);
					parseColumnDefinition(resultColumn, columnImpactModel);
				}
			}

		}

		private void parseUpdateStmt(TUpdateSqlStatement updateStmt)
		{
			ColumnImpact impact = new ColumnImpact(updateStmt.ToString(), updateStmt.dbvendor, tableColumns, strict);
			impact.Debug = false;
			impact.ShowUIInfo = true;
			impact.TraceErrorMessage = false;
			impact.impactSQL();
			ColumnImpactModel columnImpactModel = impact.generateModel();

			if (updateStmt.ResultColumnList != null)
			{
				int columnCount = updateStmt.ResultColumnList.size();

				for (int i = 0; i < columnCount; i++)
				{
					TResultColumn resultColumn = updateStmt.ResultColumnList.getResultColumn(i);
					parseColumnDefinition(resultColumn, columnImpactModel);
				}
			}
		}

		private void parseMergeStmt(TMergeSqlStatement mergeStmt)
		{
			ColumnImpact impact = new ColumnImpact(mergeStmt.ToString(), mergeStmt.dbvendor, tableColumns, strict);
			impact.Debug = false;
			impact.ShowUIInfo = true;
			impact.TraceErrorMessage = false;
			impact.impactSQL();
			ColumnImpactModel columnImpactModel = impact.generateModel();

			TMergeSqlStatement merge = (TMergeSqlStatement) mergeStmt;
			for (int i = 0; i < merge.WhenClauses.Count; i++)
			{
				TMergeWhenClause whenClause = merge.WhenClauses[i];
				if (whenClause.UpdateClause != null)
				{
					// int columnCount = whenClause.getUpdateClause( )
					// .getUpdateColumnList( )
					// .size( );
					// String[] aliasNames = new String[columnCount];
					//
					// for ( int j = 0; j < whenClause.getUpdateClause( )
					// .getUpdateColumnList( )
					// .size( ); j++ )
					// {
					// TResultColumn resultColumn = whenClause.getUpdateClause( )
					// .getUpdateColumnList( )
					// .getResultColumn( j );
					// parseColumnDefinition( resultColumn,
					// columnImpactModel,
					// aliasNames[j] );
					// }
				}
				else if (whenClause.InsertClause != null)
				{
					if (whenClause.InsertClause.ColumnList != null)
					{
						int columnCount = whenClause.InsertClause.ColumnList.size();

						TableMetaData tableMetaData = new TableMetaData(vendor, strict);
						tableMetaData.Name = merge.TargetTable.TableName.TableString;
						tableMetaData.SchemaName = merge.TargetTable.TableName.SchemaString;
						if (isNotEmpty(merge.TargetTable.TableName.DatabaseString))
						{
							tableMetaData.CatalogName = merge.TargetTable.TableName.DatabaseString;
						}
						else
						{
							tableMetaData.CatalogName = database;
						}
						tableMetaData = getTableMetaData(tableMetaData);

						for (int j = 0; j < columnCount; j++)
						{
							TObjectName resultColumn = whenClause.InsertClause.ColumnList.getObjectName(j);
							getColumnMetaData(tableMetaData, resultColumn);
							parseColumnDefinition(resultColumn, columnImpactModel);
						}
					}
				}
				else if (whenClause.DeleteClause != null)
				{

				}
			}
		}

		private void parseSelectStmt(TSelectSqlStatement selectStmt)
		{
			if (selectStmt.ParentStmt != null)
			{
				return;
			}
			ColumnImpact impact = new ColumnImpact(selectStmt.ToString(), selectStmt.dbvendor, tableColumns, strict);
			impact.Debug = false;
			impact.ShowUIInfo = true;
			impact.TraceErrorMessage = false;
			impact.VirtualTableName = SQLUtil.generateVirtualTableName(selectStmt);
			impact.impactSQL();
			ColumnImpactModel columnImpactModel = impact.generateModel();

			TableMetaData viewMetaData = new TableMetaData(vendor, strict);
			viewMetaData.Name = SQLUtil.generateVirtualTableName(selectStmt);
			if (selectStmt.TargetTable != null && selectStmt.TargetTable.TableName != null)
			{
				if (selectStmt.TargetTable.TableName.SchemaString != null)
				{
					viewMetaData.SchemaName = selectStmt.TargetTable.TableName.SchemaString;
				}
				if (selectStmt.TargetTable.TableName.DatabaseString != null)
				{
					viewMetaData.CatalogName = selectStmt.TargetTable.TableName.DatabaseString;
				}
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

			parseSubQueryColumnDefinition(selectStmt, viewMetaData, columnImpactModel);
		}

		private void parseSubQueryColumnDefinition(TSelectSqlStatement stmt, TableMetaData viewMetaData, ColumnImpactModel columnImpactModel)
		{
			if (stmt.SetOperatorType != ESetOperatorType.none)
			{
				parseSubQueryColumnDefinition(stmt.LeftStmt, viewMetaData, columnImpactModel);
				parseSubQueryColumnDefinition(stmt.RightStmt, viewMetaData, columnImpactModel);
			}
			else
			{
				int columnCount = stmt.ResultColumnList.size();

				for (int i = 0; i < columnCount; i++)
				{
					TResultColumn resultColumn = stmt.ResultColumnList.getResultColumn(i);
					parseColumnDefinition(resultColumn, columnImpactModel);
					parseColumnDefinition(resultColumn, viewMetaData, columnImpactModel);
				}
			}
		}

		private void parseColumnDefinition(TResultColumn resultColumn, TableMetaData viewMetaData, ColumnImpactModel columnImpactModel)
		{
			ColumnMetaData columnMetaData = new ColumnMetaData();
			columnMetaData.Table = viewMetaData;

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

		private void parseColumnDefinition(TResultColumn resultColumn, ColumnImpactModel columnImpactModel)
		{
            if (resultColumn.Expr.ExpressionType == EExpressionType.assignment_t)
            {
                TExpression leftExpr = getColumnExpression(resultColumn.Expr
                    .LeftOperand);
                TExpression rightExpr = getColumnExpression(resultColumn.Expr
                        .RightOperand);
                if (leftExpr.ExpressionType == EExpressionType.simple_object_name_t)
                {
                    parseColumnDefinition(leftExpr.ObjectOperand,
                            columnImpactModel);
                }
                if (rightExpr.ExpressionType == EExpressionType.simple_object_name_t)
                {
                    parseColumnDefinition(rightExpr.ObjectOperand,
                            columnImpactModel);
                }
            }
            else
                getRefTableColumns(resultColumn, columnImpactModel);
		}

        private TExpression getColumnExpression(TExpression expr)
        {
            if (expr.ExpressionType == EExpressionType.simple_object_name_t)
            {
                return expr;
            }
            else if (expr.LeftOperand != null)
            {
                return getColumnExpression(expr.LeftOperand);
            }
            else
                return expr;
        }

        private void parseColumnDefinition(TObjectName resultColumn, ColumnImpactModel columnImpactModel)
		{
			getRefTableColumns(resultColumn, columnImpactModel);
		}

		private string removeQuote(string @string)
		{
			if (!string.ReferenceEquals(@string, null) && @string.IndexOf('.') != -1)
			{
				string[] splits = @string.Split(new char[] { '.'});
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
						else if (resultColumn.Expr.LeftOperand != null && resultColumn.Expr.LeftOperand.ExprList != null)
						{
							for (int j = 0; j < resultColumn.Expr.LeftOperand.ExprList.size(); j++)
							{
								if (removeQuote(resultColumn.Expr.LeftOperand.ExprList.getExpression(j).ToString()).Equals(removeQuote(referModel.Field.FullName), StringComparison.CurrentCultureIgnoreCase))
								{
									ColumnMetaData columnMetaData = getColumn(referModel.Column);
									if (!columns.Contains(columnMetaData))
									{
										columns.Add(columnMetaData);
									}

								}
							}
						}
						else if (resultColumn.Expr != null)
						{

							if (removeQuote(resultColumn.Expr.ToString()).Equals(removeQuote(referModel.Field.FullName), StringComparison.CurrentCultureIgnoreCase))
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

		private ColumnMetaData[] getRefTableColumns(TObjectName resultColumn, ColumnImpactModel columnImpactModel)
		{
			ReferenceModel[] referenceModels = columnImpactModel.References;
			List<ColumnMetaData> columns = new List<ColumnMetaData>();

			for (int i = 0; i < referenceModels.Length; i++)
			{
				ReferenceModel referModel = referenceModels[i];
				// if ( referModel.getClause( ) != Clause.SELECT )
				// continue;
				if (referModel.Field != null)
				{
					if (resultColumn != null)
					{
						if (resultColumn.SchemaString != null)
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
						else
						{
							if (removeQuote(resultColumn.ToString()).Equals(removeQuote(referModel.Field.Name), StringComparison.CurrentCultureIgnoreCase))
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
				else if (referModel.Alias != null)
				{
					if (resultColumn != null)
					{
						if (resultColumn.SchemaString != null)
						{
							if (removeQuote(resultColumn.ToString()).Equals(removeQuote(referModel.Alias.Name), StringComparison.CurrentCultureIgnoreCase))
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
							if (removeQuote(resultColumn.ToString()).Equals(removeQuote(referModel.Alias.Name), StringComparison.CurrentCultureIgnoreCase))
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

		private void parseCreateIndex(TCreateIndexSqlStatement createIndex)
		{
			if (createIndex.CreateIndexNode == null || createIndex.CreateIndexNode.TableName == null)
			{
				return;
			}
			string tableName = createIndex.CreateIndexNode.TableName.TableString;
			string tableSchema = createIndex.CreateIndexNode.TableName.SchemaString;
			TableMetaData tableMetaData = new TableMetaData(vendor, strict);
			tableMetaData.Name = tableName;
			tableMetaData.SchemaName = tableSchema;
			if (isNotEmpty(createIndex.CreateIndexNode.TableName.DatabaseString))
			{
				tableMetaData.CatalogName = createIndex.CreateIndexNode.TableName.DatabaseString;
			}
			else
			{
				tableMetaData.CatalogName = database;
			}
			tableMetaData = getTableMetaData(tableMetaData);

			if (createIndex.IndexName != null)
			{
				index index = new index();
				index.name = createIndex.IndexName.ToString();
				tableMetaData.Indices.Add(index);

				if (createIndex.ColumnNameList != null)
				{
					for (int i = 0; i < createIndex.ColumnNameList.size(); i++)
					{
						indexColumn indexColumn = new indexColumn();
						indexColumn.name = createIndex.ColumnNameList.getOrderByItem(i).ToString();

                        List<indexColumn> indexColumns = new List<indexColumn>();
                        if (index.indexColumns != null)
                            indexColumns.AddRange(index.indexColumns);
                        indexColumns.Add(indexColumn);
                        index.indexColumns = indexColumns.ToArray();

                        ColumnMetaData columnMetaData = getColumnMetaData(tableMetaData, indexColumn.name);
						if (columnMetaData != null)
						{
							columnMetaData.Index = true;
						}
					}
				}
			}
		}

		private void parseAlterTable(TAlterTableStatement alterTable, TableMetaData tableMetaData)
		{
			if (alterTable.AlterTableOptionList == null && alterTable.TableElementList == null)
			{

				return;
			}

			if (alterTable.AlterTableOptionList != null)
			{
				for (int i = 0; i < alterTable.AlterTableOptionList.size(); i++)
				{
					parseAlterTableOption(alterTable.AlterTableOptionList.getAlterTableOption(i), tableMetaData);
				}
			}
			if (alterTable.TableElementList != null)
			{
				for (int i = 0; i < alterTable.TableElementList.size(); i++)
				{
					parseAlterTableElement(alterTable.TableElementList.getTableElement(i), tableMetaData);
				}
			}
		}

		private void parseAlterTableElement(TTableElement tableElement, TableMetaData tableMetaData)
		{
			switch (tableElement.Type)
			{
				case TTableElement.type_table_constraint :
				{
					parseTableConstraint(tableElement.Constraint, tableMetaData);
				}
					goto default;
				default :

			break;
			}

		}

		private void parseAlterTableOption(TAlterTableOption alterTableOption, TableMetaData tableMetaData)
		{
			switch (alterTableOption.OptionType)
			{
				case EAlterTableOptionType.AddColumn :
					for (int i = 0; i < alterTableOption.ColumnDefinitionList.size(); i++)
					{
						parseColumnDefinition(alterTableOption.ColumnDefinitionList.getColumn(i), tableMetaData);
					}
					break;
				case EAlterTableOptionType.ModifyColumn:
					for (int i = 0; i < alterTableOption.ColumnDefinitionList.size(); i++)
					{
						parseColumnDefinition(alterTableOption.ColumnDefinitionList.getColumn(i), tableMetaData);
					}
					break;
				case EAlterTableOptionType.AddConstraint:
					for (int i = 0; i < alterTableOption.ConstraintList.size(); i++)
					{
						parseTableConstraint(alterTableOption.ConstraintList.getConstraint(i), tableMetaData);
					}
					goto default;
				default :

			break;
			}
		}

		private static bool isNotEmpty(string str)
		{
			return !string.ReferenceEquals(str, null) && str.Trim().Length > 0;
		}

		private void parseCreateTable(TCreateTableSqlStatement createTable)
		{
			if (createTable.TableName != null)
			{
				TObjectName tableName = createTable.TableName;
				TableMetaData tableMetaData = getTableMetaData(tableName);
				if (createTable.TableComment != null)
				{
					tableMetaData.Comment = createTable.TableComment.ToString();
				}
				if (createTable.ColumnList != null)
				{
					for (int i = 0; i < createTable.ColumnList.size(); i++)
					{
						TColumnDefinition columnDef = createTable.ColumnList.getColumn(i);
						parseColumnDefinition(columnDef, tableMetaData);
					}
				}
				else if (createTable.SubQuery != null)
				{
					TResultColumnList columns = createTable.SubQuery.ResultColumnList;
					for (int i = 0; i < columns.size(); i++)
					{
						TResultColumn column = columns.getResultColumn(i);
						if (column.AliasClause != null)
						{
							getColumnMetaData(tableMetaData, column.AliasClause.AliasName);
						}
						else
						{
							if (column.FieldAttr != null)
							{
								getColumnMetaData(tableMetaData, column.FieldAttr);
							}
						}
					}
				}
				if (createTable.TableConstraints != null)
				{
					for (int i = 0; i < createTable.TableConstraints.size(); i++)
					{
						TConstraint constraint = createTable.TableConstraints.getConstraint(i);
						parseTableConstraint(constraint, tableMetaData);
					}
				}
			}
		}

		private void parseTableConstraint(TConstraint constraint, TableMetaData tableMetaData)
		{
			if (constraint.ColumnList == null)
			{
				return;
			}
			switch (constraint.Constraint_type)
			{
				case EConstraintType.primary_key:
					setColumnMetaDataPrimaryKey(tableMetaData, constraint);
					break;
				case EConstraintType.unique:
					setColumnMetaDataUnique(tableMetaData, constraint);
					break;
				case EConstraintType.foreign_key:
					setColumnMetaDataForeignKey(tableMetaData, constraint);
					break;
				case EConstraintType.index:
					setColumnMetaDataIndex(tableMetaData, constraint);
					break;
				default :
					break;
			}
		}

		private void setColumnMetaDataIndex(TableMetaData tableMetaData, TConstraint constraint)
		{
			index index = new index();
			if (constraint.ConstraintName != null)
			{
				index.name = constraint.ConstraintName.ToString();
			}
			for (int i = 0; i < constraint.ColumnList.size(); i++)
			{
				TObjectName @object = constraint.ColumnList.getObjectName(i);
				ColumnMetaData columnMetaData = getColumnMetaData(tableMetaData, @object);
				columnMetaData.Index = true;
				indexColumn indexColumn = new indexColumn();
				indexColumn.name = columnMetaData.DisplayName;
				
                List<indexColumn> indexColumns = new List<indexColumn>();
                if (index.indexColumns != null)
                    indexColumns.AddRange(index.indexColumns);
                indexColumns.Add(indexColumn);
                index.indexColumns = indexColumns.ToArray();
            }
			tableMetaData.Indices.Add(index);

		}

		private void setColumnMetaDataForeignKey(TableMetaData tableMetaData, TConstraint constraint)
		{
			foreignKey foreignKey = new foreignKey();
			if (constraint.ConstraintName != null)
			{
				foreignKey.name = constraint.ConstraintName.ToString();
			}
			if (constraint.ReferencedObject != null)
			{
				foreignKey.foreignTable = constraint.ReferencedObject.ToString();
			}
			if (constraint.ColumnList != null && constraint.ReferencedColumnList != null)
			{
				for (int i = 0; i < constraint.ColumnList.size(); i++)
				{
					TObjectName @object = constraint.ColumnList.getObjectName(i);
					ColumnMetaData columnMetaData = getColumnMetaData(tableMetaData, @object);
					columnMetaData.ForeignKey = true;
					reference reference = new reference();
					reference.local = columnMetaData.DisplayName;
					if (constraint.ReferencedColumnList.size() > i)
					{
						reference.foreign = constraint.ReferencedColumnList.getObjectName(i).ToString();
					}

                    List<reference> references = new List<reference>();
                    if (foreignKey.references != null)
                        references.AddRange(foreignKey.references);
                    references.Add(reference);
                    foreignKey.references = references.ToArray();
				}
			}
			if (constraint.KeyActions != null)
			{
				for (int i = 0; i < constraint.KeyActions.Count; i++)
				{
					TKeyAction keyAction = constraint.KeyActions[i];
					if (keyAction.ActionType == EKeyActionType.delete)
					{
						foreignKey.onDelete = true.ToString();
					}
					else if (keyAction.ActionType == EKeyActionType.update)
					{
						foreignKey.onUpdate = true.ToString();
					}
				}
			}
			tableMetaData.ForeignKeys.Add(foreignKey);
		}

		private void setColumnMetaDataUnique(TableMetaData tableMetaData, TConstraint constraint)
		{
			unique unique = new unique();
			if (constraint.ConstraintName != null)
			{
				unique.name = constraint.ConstraintName.ToString();
			}
			for (int i = 0; i < constraint.ColumnList.size(); i++)
			{
				TObjectName @object = constraint.ColumnList.getObjectName(i);
				ColumnMetaData columnMetaData = getColumnMetaData(tableMetaData, @object);
				columnMetaData.Unique = true;
				uniqueColumn uniqueColumn = new uniqueColumn();
				uniqueColumn.name = columnMetaData.DisplayName;
                List<uniqueColumn> columns = new List<uniqueColumn>();
                if (unique.uniqueColumns != null)
                    columns.AddRange(unique.uniqueColumns);
                columns.Add(uniqueColumn);
                unique.uniqueColumns = columns.ToArray();
			}
			tableMetaData.Uniques.Add(unique);
		}

		private void setColumnMetaDataPrimaryKey(TableMetaData tableMetaData, TConstraint constraint)
		{
			for (int i = 0; i < constraint.ColumnList.size(); i++)
			{
				TObjectName @object = constraint.ColumnList.getObjectName(i);
				ColumnMetaData columnMetaData = getColumnMetaData(tableMetaData, @object);
				columnMetaData.setPrimaryKey(true);
			}
		}

		private void parseColumnDefinition(TColumnDefinition columnDef, TableMetaData tableMetaData)
		{
			if (columnDef.ColumnName != null)
			{
				TObjectName @object = columnDef.ColumnName;
				ColumnMetaData columnMetaData = getColumnMetaData(tableMetaData, @object);

				if (@object.CommentString != null)
				{
					string columnComment = @object.CommentString.ToString();
					columnMetaData.Comment = columnComment;
				}

				if (columnDef.DefaultExpression != null)
				{
					columnMetaData.DefaultVlaue = columnDef.DefaultExpression.ToString();
				}

				if (columnDef.Datatype != null)
				{
					TTypeName type = columnDef.Datatype;
					string typeName = type.ToString();
					int typeNameIndex = typeName.IndexOf("(", StringComparison.Ordinal);
					if (typeNameIndex != -1)
					{
						typeName = typeName.Substring(0, typeNameIndex);
					}
					columnMetaData.TypeName = typeName;
					if (type.Scale != null)
					{
						try
						{
							columnMetaData.Scale = int.Parse(type.Scale.ToString());
						}
						catch (System.FormatException)
						{
						}
					}
					if (type.Precision != null)
					{
						try
						{
							columnMetaData.Precision = int.Parse(type.Precision.ToString());
						}
						catch (System.FormatException)
						{
						}
					}
					if (type.Length != null)
					{
						try
						{
							columnMetaData.ColumnDisplaySize = type.Length.ToString();
						}
						catch (System.FormatException)
						{
						}
					}
				}

				if (columnDef.Null)
				{
					columnMetaData.Null = true;
				}

				if (columnDef.Constraints != null)
				{
					for (int i = 0; i < columnDef.Constraints.size(); i++)
					{
						TConstraint constraint = columnDef.Constraints.getConstraint(i);
						switch (constraint.Constraint_type)
						{
							case EConstraintType.notnull:
								columnMetaData.NotNull = true;
								break;
							case EConstraintType.primary_key:
								columnMetaData.setPrimaryKey(true);
								break;
							case EConstraintType.unique:
								columnMetaData.Unique = true;
								break;
							case EConstraintType.check:
								columnMetaData.Check = true;
								break;
							case EConstraintType.foreign_key:
								columnMetaData.ForeignKey = true;
								break;
							case EConstraintType.fake_auto_increment:
								columnMetaData.AutoIncrement = true;
								break;
							case EConstraintType.fake_comment:
								// Can't get comment information.
							default :
								break;
						}
					}
				}
			}
		}

		private ColumnMetaData getColumnMetaData(TableMetaData tableMetaData, TObjectName @object)
		{
			if (@object == null)
			{
				return null;
			}
			return getColumnMetaData(tableMetaData, @object.ColumnNameOnly);
		}

		private TableMetaData getTableMetaData(TObjectName tableObjectName)
		{
			string tableName = tableObjectName.TableString;
			string tableSchema = tableObjectName.SchemaString;
			TableMetaData tableMetaData = new TableMetaData(vendor, strict);
			tableMetaData.Name = tableName;
			tableMetaData.SchemaName = tableSchema;
			if (isNotEmpty(tableObjectName.DatabaseString))
			{
				tableMetaData.CatalogName = tableObjectName.DatabaseString;
			}
			else
			{
				tableMetaData.CatalogName = database;
			}
			tableMetaData = getTableMetaData(tableMetaData);
			return tableMetaData;
		}

		private ColumnMetaData getColumnMetaData(TableMetaData tableMetaData, string columnName)
		{
			ColumnMetaData columnMetaData = new ColumnMetaData();
			columnMetaData.Name = columnName;
			columnMetaData.Table = tableMetaData;

			int index = tableColumns[tableMetaData].IndexOf(columnMetaData);
			if (index != -1)
			{
				columnMetaData = tableColumns[tableMetaData][index];
			}
			else
			{
				tableColumns[tableMetaData].Add(columnMetaData);
			}
			return columnMetaData;
		}

		private TableMetaData getTableMetaData(TableMetaData tableMetaData)
		{
			IList<TableMetaData> tables = new List<TableMetaData>(tableColumns.Keys);
			int index = tables.IndexOf(tableMetaData);
			if (index != -1)
			{
				return tables[index];
			}
			else
			{
				tableColumns[tableMetaData] = new List<ColumnMetaData>();
				return tableMetaData;
			}
		}

		private ProcedureMetaData getProcedureMetaData(ProcedureMetaData procedureMetaData, bool replace)
		{
			int index = procedures.Item2.IndexOf(procedureMetaData);
			if (index != -1)
			{
				if (replace)
				{
					procedures.Item2.RemoveAt(index);
					procedures.Item2.Add(procedureMetaData);
					return procedureMetaData;
				}
				else
				{
					return procedures.Item2[index];
				}
			}
			else
			{
				procedures.Item2.Add(procedureMetaData);
				return procedureMetaData;
			}
		}

		private void parseCommentOn(TOracleCommentOnSqlStmt commentOn)
		{
			if (commentOn.DbObjType == TObjectName.ttobjTable)
			{
				string tableName = commentOn.ObjectName.PartString;
				string tableSchema = commentOn.ObjectName.ObjectString;
				TableMetaData tableMetaData = new TableMetaData(vendor, strict);
				tableMetaData.Name = tableName;
				tableMetaData.SchemaName = tableSchema;
				if (isNotEmpty(commentOn.ObjectName.DatabaseString))
				{
					tableMetaData.CatalogName = commentOn.ObjectName.DatabaseString;
				}
				else
				{
					tableMetaData.CatalogName = database;
				}
				tableMetaData = getTableMetaData(tableMetaData);
				tableMetaData.Comment = commentOn.Message.ToString();
			}
			else if (commentOn.DbObjType == TObjectName.ttobjColumn)
			{
				ColumnMetaData columnMetaData = new ColumnMetaData();
				string columnName = commentOn.ObjectName.ColumnNameOnly;
				columnMetaData.Name = columnName;

				TableMetaData tableMetaData = new TableMetaData(vendor, strict);
				if (isNotEmpty(commentOn.ObjectName.TableString))
				{
					tableMetaData.Name = commentOn.ObjectName.TableString;
				}
				if (isNotEmpty(commentOn.ObjectName.SchemaString))
				{
					tableMetaData.SchemaName = commentOn.ObjectName.SchemaString;
				}
				if (isNotEmpty(commentOn.ObjectName.DatabaseString))
				{
					tableMetaData.CatalogName = commentOn.ObjectName.SchemaString;
				}
				else
				{
					tableMetaData.CatalogName = database;
				}
				tableMetaData = getTableMetaData(tableMetaData);
				columnMetaData.Table = tableMetaData;

				int index = tableColumns[tableMetaData].IndexOf(columnMetaData);
				if (index != -1)
				{
					tableColumns[tableMetaData][index].Comment = commentOn.Message.ToString();
				}
				else
				{
					columnMetaData.Comment = commentOn.Message.ToString();
					tableColumns[tableMetaData].Add(columnMetaData);
				}
			}
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