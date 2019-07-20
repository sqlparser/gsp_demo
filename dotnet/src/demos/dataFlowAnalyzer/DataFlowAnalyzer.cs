using System;
using System.Collections.Generic;
using System.Text;

namespace gudusoft.gsqlparser.demos.dlineage
{

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using EExpressionType = gudusoft.gsqlparser.EExpressionType;
    using ESetOperatorType = gudusoft.gsqlparser.ESetOperatorType;
    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using IExpressionVisitor = gudusoft.gsqlparser.nodes.IExpressionVisitor;
    using TAliasClause = gudusoft.gsqlparser.nodes.TAliasClause;
    using TConstant = gudusoft.gsqlparser.nodes.TConstant;
    using TExpression = gudusoft.gsqlparser.nodes.TExpression;
    using TExpressionList = gudusoft.gsqlparser.nodes.TExpressionList;
    using TFunctionCall = gudusoft.gsqlparser.nodes.TFunctionCall;
    using TGroupByItem = gudusoft.gsqlparser.nodes.TGroupByItem;
    using TGroupByItemList = gudusoft.gsqlparser.nodes.TGroupByItemList;
    using TJoin = gudusoft.gsqlparser.nodes.TJoin;
    using TJoinItem = gudusoft.gsqlparser.nodes.TJoinItem;
    using TMergeInsertClause = gudusoft.gsqlparser.nodes.TMergeInsertClause;
    using TMergeUpdateClause = gudusoft.gsqlparser.nodes.TMergeUpdateClause;
    using TMergeWhenClause = gudusoft.gsqlparser.nodes.TMergeWhenClause;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using TObjectNameList = gudusoft.gsqlparser.nodes.TObjectNameList;
    using TParseTreeNode = gudusoft.gsqlparser.nodes.TParseTreeNode;
    using TResultColumn = gudusoft.gsqlparser.nodes.TResultColumn;
    using TResultColumnList = gudusoft.gsqlparser.nodes.TResultColumnList;
    using TTable = gudusoft.gsqlparser.nodes.TTable;
    using TTableList = gudusoft.gsqlparser.nodes.TTableList;
    using TViewAliasItemList = gudusoft.gsqlparser.nodes.TViewAliasItemList;
    using TCreateViewSqlStatement = gudusoft.gsqlparser.stmt.TCreateViewSqlStatement;
    using TInsertSqlStatement = gudusoft.gsqlparser.stmt.TInsertSqlStatement;
    using TMergeSqlStatement = gudusoft.gsqlparser.stmt.TMergeSqlStatement;
    using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;
    using TUpdateSqlStatement = gudusoft.gsqlparser.stmt.TUpdateSqlStatement;


    using DataFlow = gudusoft.gsqlparser.demos.dlineage.dataflow.model.xml.dataflow;
    using gudusoft.gsqlparser.demos.dlineage.dataflow.model;
    using gudusoft.gsqlparser.demos.dlineage.dataflow.model.xml;
    using SQLUtil = gudusoft.gsqlparser.demos.dlineage.util.SQLUtil;
    using System.IO;


    using Document = System.Xml.Linq.XDocument;
    using Element = System.Xml.Linq.XElement;

    using System.Xml.Linq;
    using demos.util;
    using dataFlowAnalyzer.dataflow.model.xml;
    using gudusoft.gsqlparser.stmt;
    using gudusoft.gsqlparser.nodes;
    using gudusoft.gsqlparser.util;

    public class DataFlowAnalyzer
    {
        private static readonly List<string> TERADATA_BUILTIN_FUNCTIONS = new List<string>(new string[] { "ACCOUNT", "CURRENT_DATE", "CURRENT_ROLE", "CURRENT_TIME", "CURRENT_TIMESTAMP", "CURRENT_USER", "DATABASE", "DATE", "PROFILE", "ROLE", "SESSION", "TIME", "USER", "SYSDATE" });

        private Stack<TCustomSqlStatement> stmtStack = new Stack<TCustomSqlStatement>();
        private List<ResultSet> appendResultSetList = new List<ResultSet>();
        private IList<TCustomSqlStatement> accessedStatements = new List<TCustomSqlStatement>();

        private FileInfo sqlDir;
        private FileInfo[] sqlFiles;
        private string sqlContent;
        private EDbVendor vendor;

        private String dataflowString;
        private DataFlow dataflowResult;

        public DataFlowAnalyzer(string sqlContent, EDbVendor dbVendor)
        {
            this.sqlContent = sqlContent;
            this.vendor = dbVendor;
        }

        public DataFlowAnalyzer(FileInfo[] sqlFiles, EDbVendor dbVendor)
        {
            this.sqlFiles = sqlFiles;
            this.vendor = dbVendor;
        }

        public DataFlowAnalyzer(FileInfo sqlDir, EDbVendor dbVendor)
        {
            this.sqlDir = sqlDir;
            this.vendor = dbVendor;
        }

        public virtual string generateDataFlow(StringBuilder errorMessage)
        {
            lock (this)
            {
                StringBuilder sw = new StringBuilder();
                TextWriter systemSteam = Console.Error;
                TextWriter pw = new StringWriter(sw);
                Console.SetError(pw);

                dataflowString = analyzeSqlScript();

                Console.SetError(systemSteam);

                if (sw != null)
                {
                    if (errorMessage != null)
                    {
                        errorMessage.Append(sw.ToString().Trim());
                    }
                }

                return dataflowString;
            }
        }

        internal class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb) { }

            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        public virtual DataFlow DataFlow
        {
            get
            {
                lock (this)
                {
                    if (dataflowResult != null)
                    {
                        return dataflowResult;
                    }
                    else if (!string.ReferenceEquals(dataflowString, null))
                    {
                        dataflowResult = XML2Model<DataFlow>.loadXML(dataflowString);
                        return dataflowResult;
                    }
                    return null;
                }
            }
        }


        private FileInfo[] listFiles(FileInfo sqlFiles)
        {
            List<FileInfo> children = new List<FileInfo>();
            if (sqlFiles != null)
            {
                listFiles(sqlFiles.FullName, children);
            }
            return children.ToArray();
        }

        private void listFiles(string rootFilePath, List<FileInfo> children)
        {
            FileInfo rootFile = new FileInfo(rootFilePath);

            if (!rootFile.Attributes.HasFlag(FileAttributes.Directory))
            {
                children.Add(rootFile);
            }
            else
            {
                FileInfo[] files = new DirectoryInfo(rootFile.FullName).GetFiles();
                for (int i = 0; i < files.Length; i++)
                {
                    listFiles(files[i].FullName, children);
                }

                DirectoryInfo[] dirs = new DirectoryInfo(rootFile.FullName).GetDirectories();
                for (int i = 0; i < dirs.Length; i++)
                {
                    listFiles(dirs[i].FullName, children);
                }
            }
        }

        private string analyzeSqlScript()
        {
            lock (this)
            {
                init();

                Document doc = null;
                Element dlineageResult = null;

                doc = new Document();
                XDeclaration declaration = new XDeclaration("1.0", "utf-8", "no");
                doc.Declaration = declaration;
                dlineageResult = new XElement("dlineage");

                if (sqlDir != null)
                {
                    FileInfo[] children = listFiles(sqlDir);
                    for (int i = 0; i < children.Length; i++)
                    {
                        FileInfo child = children[i];
                        if (child.Attributes.HasFlag(FileAttributes.Directory))
                        {
                            continue;
                        }
                        string content = SQLUtil.getFileContent(child.FullName);
                        TGSqlParser sqlparser = new TGSqlParser(vendor);
                        sqlparser.sqltext = content.ToUpper();
                        analyzeAndOutputResult(sqlparser, doc, dlineageResult);
                    }
                }
                else if (!string.ReferenceEquals(sqlContent, null))
                {
                    TGSqlParser sqlparser = new TGSqlParser(vendor);
                    sqlparser.sqltext = sqlContent.ToUpper();
                    analyzeAndOutputResult(sqlparser, doc, dlineageResult);
                }
                else if (sqlFiles != null)
                {
                    FileInfo[] children = sqlFiles;
                    for (int i = 0; i < children.Length; i++)
                    {
                        FileInfo child = children[i];
                        if (child.Attributes.HasFlag(FileAttributes.Directory))
                        {
                            continue;
                        }
                        string content = SQLUtil.getFileContent(child.FullName);
                        TGSqlParser sqlparser = new TGSqlParser(vendor);
                        sqlparser.sqltext = content.ToUpper();
                        analyzeAndOutputResult(sqlparser, doc, dlineageResult);
                    }
                }

                doc.Add(dlineageResult);

                try
                {
                    StringBuilder xmlBuffer = new StringBuilder();

                    using (StringWriter writer = new Utf8StringWriter(xmlBuffer))
                    {
                        doc.Save(writer, SaveOptions.None);
                    }
                    return xmlBuffer.ToString().Trim();
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }

                return null;
            }
        }

        private void init()
        {
            dataflowString = null;
            dataflowResult = null;
            appendResultSetList.Clear();
            Table.TABLE_ID = 0;
            TableColumn.TABLE_COLUMN_ID = 0;
            AbstractRelation.RELATION_ID = 0;
            ResultSet.DISPLAY_ID.Clear();
            ResultSet.DISPLAY_NAME.Clear();
        }

        private void analyzeAndOutputResult(TGSqlParser sqlparser, Document doc, Element dlineageResult)
        {
            try
            {
                accessedStatements.Clear();
                stmtStack.Clear();
                ModelBindingManager.reset();

                try
                {
                    int result = sqlparser.parse();
                    if (result != 0)
                    {
                        Console.Error.WriteLine(sqlparser.Errormessage);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                    return;
                }

                for (int i = 0; i < sqlparser.sqlstatements.size(); i++)
                {
                    TCustomSqlStatement stmt = sqlparser.sqlstatements.get(i);
                    if (stmt.ErrorCount == 0)
                    {
                        analyzeCustomSqlStmt(stmt);
                    }
                }

                appendTables(dlineageResult);
                appendViews(dlineageResult);
                appendResultSets(dlineageResult);
                appendRelations(dlineageResult);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

        }

        private void analyzeCustomSqlStmt(TCustomSqlStatement stmt)
        {
            if (stmt is TCreateTableSqlStatement)
            {
                stmtStack.Push(stmt);
                analyzeCreateTableStmt((TCreateTableSqlStatement)stmt);
                stmtStack.Pop();
            }
            else if (stmt is TSelectSqlStatement)
            {
                analyzeSelectStmt((TSelectSqlStatement)stmt);
            }
            else if (stmt is TCreateViewSqlStatement)
            {
                stmtStack.Push(stmt);
                analyzeCreateViewStmt((TCreateViewSqlStatement)stmt);
                stmtStack.Pop();
            }
            else if (stmt is TInsertSqlStatement)
            {
                stmtStack.Push(stmt);
                analyzeInsertStmt((TInsertSqlStatement)stmt);
                stmtStack.Pop();
            }
            else if (stmt is TUpdateSqlStatement)
            {
                stmtStack.Push(stmt);
                analyzeUpdateStmt((TUpdateSqlStatement)stmt);
                stmtStack.Pop();
            }
            else if (stmt is TMergeSqlStatement)
            {
                stmtStack.Push(stmt);
                analyzeMergeStmt((TMergeSqlStatement)stmt);
                stmtStack.Pop();
            }
            else if (stmt.Statements != null && stmt.Statements.size() > 0)
            {
                for (int i = 0; i < stmt.Statements.size(); i++)
                {
                    analyzeCustomSqlStmt(stmt.Statements.get(i));
                }
            }
        }

        private void analyzeCreateTableStmt(TCreateTableSqlStatement stmt)
        {
            TTable table = stmt.TargetTable;
            if (table != null)
            {
                Table tableModel = ModelFactory.createTableFromCreateDML(table);
                if (stmt.ColumnList != null && stmt.ColumnList.size() > 0)
                {
                    for (int i = 0; i < stmt.ColumnList.size(); i++)
                    {
                        TColumnDefinition column = stmt.ColumnList.getColumn(i);
                        ModelFactory.createTableColumn(tableModel, column.ColumnName);
                    }
                }

                if (stmt.SubQuery != null)
                {
                    analyzeSelectStmt(stmt.SubQuery);
                }

                if (stmt.SubQuery != null && stmt.SubQuery.ResultColumnList != null)
                {
                    SelectResultSet resultSetModel = (SelectResultSet)ModelBindingManager.getModel(stmt.SubQuery.ResultColumnList);
                    for (int i = 0; i < resultSetModel.Columns.Count; i++)
                    {
                        ResultColumn resultColumn = resultSetModel.Columns[i];
                        if (resultColumn.ColumnObject is TResultColumn)
                        {
                            TResultColumn columnObject = (TResultColumn)resultColumn.ColumnObject;

                            TAliasClause alias = columnObject.AliasClause;
                            if (alias != null && alias.AliasName != null)
                            {
                                TableColumn tableColumn = ModelFactory.createTableColumn(tableModel, alias.AliasName);
                                DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                                relation.Target = new TableColumnRelationElement(tableColumn);
                                relation.addSource(new ResultColumnRelationElement(resultColumn));
                            }
                            else if (columnObject.FieldAttr != null)
                            {
                                TableColumn tableColumn = ModelFactory.createTableColumn(tableModel, columnObject.FieldAttr);

                                ResultColumn column = (ResultColumn)ModelBindingManager.getModel(resultColumn.ColumnObject);
                                if (column != null && column.StarLinkColumns.Count > 0)
                                {
                                    tableColumn.bindStarLinkColumns(column.StarLinkColumns);
                                }

                                DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                                relation.Target = new TableColumnRelationElement(tableColumn);
                                relation.addSource(new ResultColumnRelationElement(resultColumn));
                            }
                            else
                            {
                                Console.Error.WriteLine();
                                Console.Error.WriteLine("Can't handle table column, the create table statement is");
                                Console.Error.WriteLine(stmt.ToString());
                                continue;
                            }
                        }
                        else if (resultColumn.ColumnObject is TObjectName)
                        {
                            TableColumn tableColumn = ModelFactory.createTableColumn(tableModel, (TObjectName)resultColumn.ColumnObject);
                            DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                            relation.Target = new TableColumnRelationElement(tableColumn);
                            relation.addSource(new ResultColumnRelationElement(resultColumn));
                        }
                    }
                }
                else if (stmt.SubQuery != null)
                {
                    SelectSetResultSet resultSetModel = (SelectSetResultSet)ModelBindingManager.getModel(stmt.SubQuery);
                    for (int i = 0; i < resultSetModel.Columns.Count; i++)
                    {
                        ResultColumn resultColumn = resultSetModel.Columns[i];
                        if (resultColumn.ColumnObject is TResultColumn)
                        {
                            TResultColumn columnObject = (TResultColumn)resultColumn.ColumnObject;

                            TAliasClause alias = columnObject.AliasClause;
                            if (alias != null && alias.AliasName != null)
                            {
                                TableColumn viewColumn = ModelFactory.createTableColumn(tableModel, alias.AliasName);
                                DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                                relation.Target = new TableColumnRelationElement(viewColumn);
                                relation.addSource(new ResultColumnRelationElement(resultColumn));
                            }
                            else if (columnObject.FieldAttr != null)
                            {
                                TableColumn tableColumn = ModelFactory.createTableColumn(tableModel, columnObject.FieldAttr);

                                ResultColumn column = (ResultColumn)ModelBindingManager.getModel(resultColumn.ColumnObject);
                                if (column != null && column.StarLinkColumns.Count > 0)
                                {
                                    tableColumn.bindStarLinkColumns(column.StarLinkColumns);
                                }

                                DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                                relation.Target = new TableColumnRelationElement(tableColumn);
                                relation.addSource(new ResultColumnRelationElement(resultColumn));
                            }
                            else
                            {
                                Console.Error.WriteLine();
                                Console.Error.WriteLine("Can't handle table column, the create table statement is");
                                Console.Error.WriteLine(stmt.ToString());
                                continue;
                            }
                        }
                        else if (resultColumn.ColumnObject is TObjectName)
                        {
                            TableColumn viewColumn = ModelFactory.createTableColumn(tableModel, (TObjectName)resultColumn.ColumnObject);
                            DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                            relation.Target = new TableColumnRelationElement(viewColumn);
                            relation.addSource(new ResultColumnRelationElement(resultColumn));
                        }
                    }
                }
            }
            else
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Can't get target table. CreateTableSqlStatement is ");
                Console.Error.WriteLine(stmt.ToString());
            }
        }
        private void analyzeMergeStmt(TMergeSqlStatement stmt)
        {
            if (stmt.UsingTable != null)
            {
                TTable table = stmt.TargetTable;
                Table tableModel = ModelFactory.createTable(table);

                if (stmt.UsingTable.Subquery != null)
                {
                    ModelFactory.createQueryTable(stmt.UsingTable);
                    analyzeSelectStmt(stmt.UsingTable.Subquery);
                }
                else
                {
                    ModelFactory.createTable(stmt.UsingTable);
                }

                if (stmt.WhenClauses != null && stmt.WhenClauses.Count > 0)
                {
                    for (int i = 0; i < stmt.WhenClauses.Count; i++)
                    {
                        TMergeWhenClause clause = stmt.WhenClauses[i];
                        if (clause.UpdateClause != null)
                        {
                            TResultColumnList columns = clause.UpdateClause.UpdateColumnList;
                            if (columns == null || columns.size() == 0)
                            {
                                continue;
                            }

                            ResultSet resultSet = ModelFactory.createResultSet(clause.UpdateClause);

                            for (int j = 0; j < columns.size(); j++)
                            {
                                TResultColumn resultColumn = columns.getResultColumn(j);
                                if (resultColumn.Expr.LeftOperand.ExpressionType == EExpressionType.simple_object_name_t)
                                {
                                    TObjectName columnObject = resultColumn.Expr.LeftOperand.ObjectOperand;

                                    ResultColumn updateColumn = ModelFactory.createMergeResultColumn(resultSet, columnObject);

                                    TExpression valueExpression = resultColumn.Expr.RightOperand;
                                    if (valueExpression == null)
                                    {
                                        continue;
                                    }

                                    columnsInExpr visitor = new columnsInExpr(this);
                                    valueExpression.inOrderTraverse(visitor);
                                    IList<TObjectName> objectNames = visitor.ObjectNames;
                                    analyzeDataFlowRelation(updateColumn, objectNames);

                                    TableColumn tableColumn = ModelFactory.createTableColumn(tableModel, columnObject);

                                    DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                                    relation.Target = new TableColumnRelationElement(tableColumn);
                                    relation.addSource(new ResultColumnRelationElement(updateColumn));
                                }
                            }
                        }
                        if (clause.InsertClause != null)
                        {
                            TObjectNameList columns = clause.InsertClause.ColumnList;
                            TResultColumnList values = clause.InsertClause.Valuelist;
                            if (columns == null || columns.size() == 0 || values == null || values.size() == 0)
                            {
                                continue;
                            }

                            ResultSet resultSet = ModelFactory.createResultSet(clause.InsertClause);

                            for (int j = 0; j < columns.size() && j < values.size(); j++)
                            {
                                TObjectName columnObject = columns.getObjectName(j);

                                ResultColumn insertColumn = ModelFactory.createMergeResultColumn(resultSet, columnObject);

                                TExpression valueExpression = values.getResultColumn(j).Expr;
                                if (valueExpression == null)
                                {
                                    continue;
                                }

                                columnsInExpr visitor = new columnsInExpr(this);
                                valueExpression.inOrderTraverse(visitor);
                                IList<TObjectName> objectNames = visitor.ObjectNames;
                                analyzeDataFlowRelation(insertColumn, objectNames);

                                TableColumn tableColumn = ModelFactory.createTableColumn(tableModel, columnObject);

                                DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                                relation.Target = new TableColumnRelationElement(tableColumn);
                                relation.addSource(new ResultColumnRelationElement(insertColumn));
                            }
                        }
                    }

                }

                if (stmt.Condition != null)
                {
                    analyzeFilterCondtion(stmt.Condition);
                }
            }
        }

        private void analyzeInsertStmt(TInsertSqlStatement stmt)
        {
            if (stmt.SubQuery != null)
            {
                TTable table = stmt.TargetTable;
                Table tableModel = ModelFactory.createTable(table);

                analyzeSelectStmt(stmt.SubQuery);

                if (stmt.ColumnList != null && stmt.ColumnList.size() > 0)
                {
                    TObjectNameList items = stmt.ColumnList;

                    ResultSet resultSetModel = null;

                    if (stmt.SubQuery.ResultColumnList == null)
                    {
                        resultSetModel = (ResultSet)ModelBindingManager.getModel(stmt.SubQuery);
                    }
                    else
                    {
                        resultSetModel = (ResultSet)ModelBindingManager.getModel(stmt.SubQuery.ResultColumnList);
                    }
                    if (resultSetModel == null)
                    {
                        Console.Error.WriteLine("Can't get resultset model");
                    }

                    for (int i = 0; i < items.size() && i < resultSetModel.Columns.Count; i++)
                    {
                        TObjectName column = items.getObjectName(i);
                        ResultColumn resultColumn = resultSetModel.Columns[i];
                        if (column != null)
                        {
                            TableColumn tableColumn = ModelFactory.createTableColumn(tableModel, column);
                            DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                            relation.Target = new TableColumnRelationElement(tableColumn);
                            relation.addSource(new ResultColumnRelationElement(resultColumn));
                        }
                    }
                }
                else if (stmt.SubQuery.ResultColumnList != null)
                {
                    SelectResultSet resultSetModel = (SelectResultSet)ModelBindingManager.getModel(stmt.SubQuery.ResultColumnList);
                    for (int i = 0; i < resultSetModel.Columns.Count; i++)
                    {
                        ResultColumn resultColumn = resultSetModel.Columns[i];
                        TAliasClause alias = ((TResultColumn)resultColumn.ColumnObject).AliasClause;
                        if (alias != null && alias.AliasName != null)
                        {
                            TableColumn tableColumn = ModelFactory.createInsertTableColumn(tableModel, alias.AliasName);
                            DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                            relation.Target = new TableColumnRelationElement(tableColumn);
                            relation.addSource(new ResultColumnRelationElement(resultColumn));
                        }
                        else if (((TResultColumn)resultColumn.ColumnObject).FieldAttr != null)
                        {
                            TableColumn tableColumn = ModelFactory.createInsertTableColumn(tableModel, ((TResultColumn)resultColumn.ColumnObject).FieldAttr);

                            if ("*".Equals(getColumnName(((TResultColumn)resultColumn.ColumnObject).FieldAttr)))
                            {
                                TObjectName columnObject = ((TResultColumn)resultColumn.ColumnObject).FieldAttr;
                                TTable sourceTable = columnObject.SourceTable;
                                if (columnObject.TableToken != null && sourceTable != null)
                                {
                                    TObjectName[] columns = ModelBindingManager.getTableColumns(sourceTable);
                                    for (int j = 0; j < columns.Length; j++)
                                    {
                                        TObjectName columnName = columns[j];
                                        if ("*".Equals(getColumnName(columnName)))
                                        {
                                            continue;
                                        }
                                        resultColumn.bindStarLinkColumn(columnName);
                                    }
                                }
                                else
                                {
                                    TTableList tables = stmt.tables;
                                    for (int k = 0; k < tables.size(); k++)
                                    {
                                        TTable tableElement = tables.getTable(k);
                                        TObjectName[] columns = ModelBindingManager.getTableColumns(tableElement);
                                        for (int j = 0; j < columns.Length; j++)
                                        {
                                            TObjectName columnName = columns[j];
                                            if ("*".Equals(getColumnName(columnName)))
                                            {
                                                continue;
                                            }
                                            resultColumn.bindStarLinkColumn(columnName);
                                        }
                                    }
                                }
                            }

                            if (resultColumn != null && resultColumn.StarLinkColumns.Count > 0)
                            {
                                tableColumn.bindStarLinkColumns(resultColumn.StarLinkColumns);
                            }

                            DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                            relation.Target = new TableColumnRelationElement(tableColumn);
                            relation.addSource(new ResultColumnRelationElement(resultColumn));
                        }
                        else if (((TResultColumn)resultColumn.ColumnObject).Expr.ExpressionType == EExpressionType.simple_constant_t)
                        {
                            TableColumn tableColumn = ModelFactory.createInsertTableColumn(tableModel, ((TResultColumn)resultColumn.ColumnObject).Expr.ConstantOperand, i);
                            DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                            relation.Target = new TableColumnRelationElement(tableColumn);
                            relation.addSource(new ResultColumnRelationElement(resultColumn));
                        }
                    }
                }
                else if (stmt.SubQuery != null)
                {
                    SelectSetResultSet resultSetModel = (SelectSetResultSet)ModelBindingManager.getModel(stmt.SubQuery);
                    if (resultSetModel != null)
                    {
                        for (int i = 0; i < resultSetModel.Columns.Count; i++)
                        {
                            ResultColumn resultColumn = resultSetModel.Columns[i];
                            TAliasClause alias = ((TResultColumn)resultColumn.ColumnObject).AliasClause;
                            if (alias != null && alias.AliasName != null)
                            {
                                TableColumn tableColumn = ModelFactory.createInsertTableColumn(tableModel, alias.AliasName);
                                DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                                relation.Target = new TableColumnRelationElement(tableColumn);
                                relation.addSource(new ResultColumnRelationElement(resultColumn));
                            }
                            else if (((TResultColumn)resultColumn.ColumnObject).FieldAttr != null)
                            {
                                TableColumn tableColumn = ModelFactory.createInsertTableColumn(tableModel, ((TResultColumn)resultColumn.ColumnObject).FieldAttr);
                                DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                                relation.Target = new TableColumnRelationElement(tableColumn);
                                relation.addSource(new ResultColumnRelationElement(resultColumn));
                            }
                            else if (((TResultColumn)resultColumn.ColumnObject).Expr.ExpressionType == EExpressionType.simple_constant_t)
                            {
                                TableColumn tableColumn = ModelFactory.createInsertTableColumn(tableModel, ((TResultColumn)resultColumn.ColumnObject).Expr.ConstantOperand, i);
                                DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                                relation.Target = new TableColumnRelationElement(tableColumn);
                                relation.addSource(new ResultColumnRelationElement(resultColumn));
                            }
                        }
                    }
                }
            }
        }

        private void analyzeUpdateStmt(TUpdateSqlStatement stmt)
        {
            if (stmt.ResultColumnList == null)
            {
                return;
            }

            TTable table = stmt.TargetTable;
            Table tableModel = ModelFactory.createTable(table);

            for (int i = 0; i < stmt.tables.size(); i++)
            {
                TTable tableElement = stmt.tables.getTable(i);
                if (tableElement.Subquery != null)
                {
                    QueryTable queryTable = ModelFactory.createQueryTable(tableElement);
                    TSelectSqlStatement subquery = tableElement.Subquery;
                    analyzeSelectStmt(subquery);

                    if (subquery.SetOperatorType != ESetOperatorType.none)
                    {
                        SelectSetResultSet selectSetResultSetModel = (SelectSetResultSet)ModelBindingManager.getModel(subquery);
                        for (int j = 0; j < selectSetResultSetModel.Columns.Count; j++)
                        {
                            ResultColumn sourceColumn = selectSetResultSetModel.Columns[j];
                            ResultColumn targetColumn = ModelFactory.createSelectSetResultColumn(queryTable, sourceColumn);
                            DataFlowRelation selectSetRalation = ModelFactory.createDataFlowRelation();
                            selectSetRalation.Target = new ResultColumnRelationElement(targetColumn);
                            selectSetRalation.addSource(new ResultColumnRelationElement(sourceColumn));
                        }
                    }
                }
                else
                {
                    ModelFactory.createTable(stmt.tables.getTable(i));
                }
            }

            for (int i = 0; i < stmt.ResultColumnList.size(); i++)
            {
                TResultColumn field = stmt.ResultColumnList.getResultColumn(i);

                TExpression expression = field.Expr.LeftOperand;
                if (expression == null)
                {
                    Console.Error.WriteLine();
                    Console.Error.WriteLine("Can't handle this case. Expression is ");
                    Console.Error.WriteLine(field.Expr.ToString());
                    continue;
                }
                if (expression.ExpressionType == EExpressionType.list_t)
                {
                    TExpression setExpression = field.Expr.RightOperand;
                    if (setExpression != null && setExpression.SubQuery != null)
                    {
                        TSelectSqlStatement query = setExpression.SubQuery;
                        analyzeSelectStmt(query);

                        SelectResultSet resultSetModel = (SelectResultSet)ModelBindingManager.getModel(query.ResultColumnList);

                        TExpressionList columnList = expression.ExprList;
                        for (int j = 0; j < columnList.size(); j++)
                        {
                            TObjectName column = columnList.getExpression(j).ObjectOperand;
                            ResultColumn resultColumn = resultSetModel.Columns[j];
                            TableColumn tableColumn = ModelFactory.createTableColumn(tableModel, column);
                            DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                            relation.Target = new TableColumnRelationElement(tableColumn);
                            relation.addSource(new ResultColumnRelationElement(resultColumn));

                        }
                    }
                }
                else if (expression.ExpressionType == EExpressionType.simple_object_name_t)
                {
                    TExpression setExpression = field.Expr.RightOperand;
                    if (setExpression != null && setExpression.SubQuery != null)
                    {
                        TSelectSqlStatement query = setExpression.SubQuery;
                        analyzeSelectStmt(query);

                        SelectResultSet resultSetModel = (SelectResultSet)ModelBindingManager.getModel(query.ResultColumnList);

                        TObjectName column = expression.ObjectOperand;
                        ResultColumn resultColumn = resultSetModel.Columns[0];
                        TableColumn tableColumn = ModelFactory.createTableColumn(tableModel, column);
                        DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                        relation.Target = new TableColumnRelationElement(tableColumn);
                        relation.addSource(new ResultColumnRelationElement(resultColumn));
                    }
                    else if (setExpression != null)
                    {
                        ResultSet resultSet = ModelFactory.createResultSet(stmt);

                        TObjectName columnObject = expression.ObjectOperand;

                        ResultColumn updateColumn = ModelFactory.createUpdateResultColumn(resultSet, columnObject);

                        columnsInExpr visitor = new columnsInExpr(this);
                        field.Expr.RightOperand.inOrderTraverse(visitor);

                        IList<TObjectName> objectNames = visitor.ObjectNames;
                        analyzeDataFlowRelation(updateColumn, objectNames);

                        TableColumn tableColumn = ModelFactory.createTableColumn(tableModel, columnObject);

                        DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                        relation.Target = new TableColumnRelationElement(tableColumn);
                        relation.addSource(new ResultColumnRelationElement(updateColumn));
                    }
                }
            }

            if (stmt.joins != null && stmt.joins.size() > 0)
            {
                for (int i = 0; i < stmt.joins.size(); i++)
                {
                    TJoin join = stmt.joins.getJoin(i);
                    if (join.JoinItems != null)
                    {
                        for (int j = 0; j < join.JoinItems.size(); j++)
                        {
                            TJoinItem joinItem = join.JoinItems.getJoinItem(j);
                            TExpression expr = joinItem.OnCondition;
                            analyzeFilterCondtion(expr);
                        }
                    }
                }
            }

            if (stmt.WhereClause != null && stmt.WhereClause.Condition != null)
            {
                analyzeFilterCondtion(stmt.WhereClause.Condition);
            }
        }

        private void analyzeCreateViewStmt(TCreateViewSqlStatement stmt)
        {
            if (stmt.Subquery != null)
            {
                analyzeSelectStmt(stmt.Subquery);
            }

            if (stmt.ViewAliasClause != null && stmt.ViewAliasClause.ViewAliasItemList != null)
            {
                TViewAliasItemList viewItems = stmt.ViewAliasClause.ViewAliasItemList;
                View viewModel = ModelFactory.createView(stmt);
                ResultSet resultSetModel = (ResultSet)ModelBindingManager.getModel(stmt.Subquery);
                for (int i = 0; i < viewItems.size(); i++)
                {
                    TObjectName alias = viewItems.getViewAliasItem(i).Alias;
                    ResultColumn resultColumn;
                    if (resultSetModel.Columns.Count <= i)
                    {
                        resultColumn = resultSetModel.Columns[resultSetModel.Columns.Count - 1];
                    }
                    else
                    {
                        resultColumn = resultSetModel.Columns[i];
                    }
                    if (alias != null)
                    {
                        ViewColumn viewColumn = ModelFactory.createViewColumn(viewModel, alias, i);
                        DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                        relation.Target = new ViewColumnRelationElement(viewColumn);
                        relation.addSource(new ResultColumnRelationElement(resultColumn));
                    }
                    else if (resultColumn.ColumnObject is TObjectName)
                    {
                        ViewColumn viewColumn = ModelFactory.createViewColumn(viewModel, (TObjectName)resultColumn.ColumnObject, i);
                        DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                        relation.Target = new ViewColumnRelationElement(viewColumn);
                        relation.addSource(new ResultColumnRelationElement(resultColumn));
                    }
                    else if (resultColumn.ColumnObject is TResultColumn)
                    {
                        ViewColumn viewColumn = ModelFactory.createViewColumn(viewModel, ((TResultColumn)resultColumn.ColumnObject).FieldAttr, i);
                        ResultColumn column = (ResultColumn)ModelBindingManager.getModel(resultColumn.ColumnObject);
                        if (column != null && column.StarLinkColumns.Count > 0)
                        {
                            viewColumn.bindStarLinkColumns(column.StarLinkColumns);
                        }
                        DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                        relation.Target = new ViewColumnRelationElement(viewColumn);
                        relation.addSource(new ResultColumnRelationElement(resultColumn));
                    }
                }
            }
            else
            {
                View viewModel = ModelFactory.createView(stmt);
                if (stmt.Subquery != null && stmt.Subquery.ResultColumnList != null)
                {
                    SelectResultSet resultSetModel = (SelectResultSet)ModelBindingManager.getModel(stmt.Subquery.ResultColumnList);
					for (int i = 0; i < resultSetModel.Columns.Count; i++)
					{
						ResultColumn resultColumn = resultSetModel.Columns[i];
						if (resultColumn.ColumnObject is TResultColumn)
						{
							TResultColumn columnObject = ((TResultColumn)resultColumn.ColumnObject);

							TAliasClause alias = ((TResultColumn)resultColumn.ColumnObject).AliasClause;
							if (alias != null && alias.AliasName != null)
							{
								ViewColumn viewColumn = ModelFactory.createViewColumn(viewModel, alias.AliasName, i);
								DataFlowRelation relation = ModelFactory.createDataFlowRelation();
								relation.Target = new ViewColumnRelationElement(viewColumn);
								relation.addSource(new ResultColumnRelationElement(resultColumn));
							}
							else if (columnObject.FieldAttr != null)
							{
								ViewColumn viewColumn = ModelFactory.createViewColumn(viewModel, columnObject.FieldAttr, i);
								ResultColumn column = (ResultColumn)ModelBindingManager.getModel(resultColumn.ColumnObject);
								if (column != null && column.StarLinkColumns.Count > 0)
								{
									viewColumn.bindStarLinkColumns(column.StarLinkColumns);
								}
								DataFlowRelation relation = ModelFactory.createDataFlowRelation();
								relation.Target = new ViewColumnRelationElement(viewColumn);
								relation.addSource(new ResultColumnRelationElement(resultColumn));
							}
							else if (resultColumn.Alias != null
								&& columnObject.Expr.ExpressionType == EExpressionType.sqlserver_proprietary_column_alias_t)
							{
								ViewColumn viewColumn = ModelFactory.createViewColumn(viewModel,
										columnObject.Expr
												.LeftOperand
												.ObjectOperand,
										i);
								DataFlowRelation relation = ModelFactory.createDataFlowRelation();
								relation.Target = new ViewColumnRelationElement(viewColumn);
								relation.addSource(new ResultColumnRelationElement(resultColumn));
							}
							else
							{
								Console.Error.WriteLine();
								Console.Error.WriteLine("Can't handle view column, the view statement is");
								Console.Error.WriteLine(stmt.ToString());
								continue;
							}
						}
						else if (resultColumn.ColumnObject is TObjectName)
						{
							ViewColumn viewColumn = ModelFactory.createViewColumn(viewModel, (TObjectName)resultColumn.ColumnObject, i);
							DataFlowRelation relation = ModelFactory.createDataFlowRelation();
							relation.Target = new ViewColumnRelationElement(viewColumn);
							relation.addSource(new ResultColumnRelationElement(resultColumn));
						}
					}
				}
                else if (stmt.Subquery != null)
                {
                    SelectSetResultSet resultSetModel = (SelectSetResultSet)ModelBindingManager.getModel(stmt.Subquery);
                    for (int i = 0; i < resultSetModel.Columns.Count; i++)
                    {
                        ResultColumn resultColumn = resultSetModel.Columns[i];

                        if (resultColumn.ColumnObject is TResultColumn)
                        {
                            TResultColumn columnObject = ((TResultColumn)resultColumn.ColumnObject);

                            TAliasClause alias = columnObject.AliasClause;
                            if (alias != null && alias.AliasName != null)
                            {
                                ViewColumn viewColumn = ModelFactory.createViewColumn(viewModel, alias.AliasName, i);
                                DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                                relation.Target = new ViewColumnRelationElement(viewColumn);
                                relation.addSource(new ResultColumnRelationElement(resultColumn));
                            }
                            else if (columnObject.FieldAttr != null)
                            {
                                ViewColumn viewColumn = ModelFactory.createViewColumn(viewModel, columnObject.FieldAttr, i);
                                ResultColumn column = (ResultColumn)ModelBindingManager.getModel(resultColumn.ColumnObject);
                                if (column != null && column.StarLinkColumns.Count > 0)
                                {
                                    viewColumn.bindStarLinkColumns(column.StarLinkColumns);
                                }
                                DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                                relation.Target = new ViewColumnRelationElement(viewColumn);
                                relation.addSource(new ResultColumnRelationElement(resultColumn));
                            }
                            else if (resultColumn.Alias != null
                                && columnObject.Expr.ExpressionType == EExpressionType.sqlserver_proprietary_column_alias_t)
                            {
                                ViewColumn viewColumn = ModelFactory.createViewColumn(viewModel,
                                        columnObject.Expr
                                                .LeftOperand
                                                .ObjectOperand,
                                        i);
                                DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                                relation.Target = new ViewColumnRelationElement(viewColumn);
                                relation.addSource(new ResultColumnRelationElement(resultColumn));
                            }
                            else
                            {
                                Console.Error.WriteLine();
                                Console.Error.WriteLine("Can't handle view column, the view statement is");
                                Console.Error.WriteLine(stmt.ToString());
                                continue;
                            }
                        }
                        else if (resultColumn.ColumnObject is TObjectName)
                        {
                            ViewColumn viewColumn = ModelFactory.createViewColumn(viewModel, (TObjectName)resultColumn.ColumnObject, i);
                            DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                            relation.Target = new ViewColumnRelationElement(viewColumn);
                            relation.addSource(new ResultColumnRelationElement(resultColumn));
                        }
                    }
                }
            }
        }

        private void appendRelations(Element dlineageResult)
        {
            Relation[] relations = ModelBindingManager.Relations;
            appendRelation(dlineageResult, relations, typeof(DataFlowRelation));
            appendRecordSetRelation(dlineageResult, relations);
            appendRelation(dlineageResult, relations, typeof(RecordSetRelation));
            appendRelation(dlineageResult, relations, typeof(ImpactRelation));
        }

        private void appendRelation(Element dlineageResult, Relation[] relations, Type clazz)
        {
            for (int i = 0; i < relations.Length; i++)
            {
                AbstractRelation relation = (AbstractRelation)relations[i];

                object targetElement = relation.Target.Element;
                if (targetElement is ResultColumn)
                {
                    ResultColumn targetColumn = (ResultColumn)targetElement;
                    if (targetColumn.StarLinkColumns.Count > 0)
                    {
                        for (int j = 0; j < targetColumn.StarLinkColumns.Count; j++)
                        {
                            appendStarRelation(dlineageResult, relation, j);
                        }
                        continue;
                    }
                }
                else if (targetElement is ViewColumn)
                {
                    ViewColumn targetColumn = (ViewColumn)targetElement;
                    if (targetColumn.StarLinkColumns.Count > 0)
                    {
                        for (int j = 0; j < targetColumn.StarLinkColumns.Count; j++)
                        {
                            appendStarRelation(dlineageResult, relation, j);
                        }
                        continue;
                    }
                }
                else if (targetElement is TableColumn)
                {
                    TableColumn targetColumn = (TableColumn)targetElement;
                    if (targetColumn.StarLinkColumns.Count > 0)
                    {
                        for (int j = 0; j < targetColumn.StarLinkColumns.Count; j++)
                        {
                            appendStarRelation(dlineageResult, relation, j);
                        }
                        continue;
                    }
                }

                Element relationElement = new Element("relation");
                relationElement.Add(new XAttribute("type", Enum.GetName(typeof(RelationType), relation.RelationType)));
                relationElement.Add(new XAttribute("id", relation.Id.ToString()));

                string targetName = null;

                if (relation.GetType() == clazz)
                {
                    if (targetElement is ResultColumn)
                    {
                        ResultColumn targetColumn = (ResultColumn)targetElement;
                        Element target = new Element("target");
                        target.Add(new XAttribute("id", targetColumn.Id.ToString()));
                        target.Add(new XAttribute("column", targetColumn.Name));
                        target.Add(new XAttribute("parent_id", (targetColumn.ResultSet.Id).ToString()));
                        target.Add(new XAttribute("parent_name", getResultSetName(targetColumn.ResultSet)));
                        if (targetColumn.StartPosition != null && targetColumn.EndPosition != null)
                        {
                            target.Add(new XAttribute("coordinate", convertPosition(targetColumn.StartPosition + "," + targetColumn.EndPosition)));
                        }
                        if (relation is RecordSetRelation && ((RecordSetRelation)relation).AggregateFunction != null)
                        {
                            target.Add(new XAttribute("function", ((RecordSetRelation)relation).AggregateFunction));
                        }
                        targetName = targetColumn.Name;
                        relationElement.Add(target);
                    }
                    else if (targetElement is TableColumn)
                    {
                        TableColumn targetColumn = (TableColumn)targetElement;
                        Element target = new Element("target");
                        target.Add(new XAttribute("id", targetColumn.Id.ToString()));
                        target.Add(new XAttribute("column", targetColumn.Name));
                        target.Add(new XAttribute("parent_id", (targetColumn.Table.Id).ToString()));
                        target.Add(new XAttribute("parent_name", getTableName(targetColumn.Table)));
                        if (targetColumn.StartPosition != null && targetColumn.EndPosition != null)
                        {
                            target.Add(new XAttribute("coordinate", convertPosition(targetColumn.StartPosition + "," + targetColumn.EndPosition)));
                        }
                        if (relation is RecordSetRelation && ((RecordSetRelation)relation).AggregateFunction != null)
                        {
                            target.Add(new XAttribute("function", ((RecordSetRelation)relation).AggregateFunction));
                        }
                        targetName = targetColumn.Name;
                        relationElement.Add(target);
                    }
                    else if (targetElement is ViewColumn)
                    {
                        ViewColumn targetColumn = (ViewColumn)targetElement;
                        Element target = new Element("target");
                        target.Add(new XAttribute("id", targetColumn.Id.ToString()));
                        target.Add(new XAttribute("column", targetColumn.Name));
                        target.Add(new XAttribute("parent_id", (targetColumn.View.Id).ToString()));
                        target.Add(new XAttribute("parent_name", targetColumn.View.Name));
                        if (targetColumn.StartPosition != null && targetColumn.EndPosition != null)
                        {
                            target.Add(new XAttribute("coordinate", convertPosition(targetColumn.StartPosition + "," + targetColumn.EndPosition)));
                        }
                        if (relation is RecordSetRelation && ((RecordSetRelation)relation).AggregateFunction != null)
                        {
                            target.Add(new XAttribute("function", ((RecordSetRelation)relation).AggregateFunction));
                        }
                        targetName = targetColumn.Name;
                        relationElement.Add(target);
                    }
                    else
                    {
                        continue;
                    }

                    RelationElement[] sourceElements = relation.Sources;
                    if (sourceElements.Length == 0)
                    {
                        continue;
                    }

                    bool append = false;
                    for (int j = 0; j < sourceElements.Length; j++)
                    {
                        object sourceElement = sourceElements[j].Element;
                        if (sourceElement is ResultColumn)
                        {
                            ResultColumn sourceColumn = (ResultColumn)sourceElement;
                            if (sourceColumn.StarLinkColumns.Count > 0)
                            {
                                Element source = new Element("source");

                                if (targetElement is ViewColumn)
                                {
                                    source.Add(new XAttribute("id", sourceColumn.Id.ToString() + "_" + ((ViewColumn)targetElement).ColumnIndex));
                                    source.Add(new XAttribute("column", getColumnName(sourceColumn.StarLinkColumns[((ViewColumn)targetElement).ColumnIndex])));
                                    source.Add(new XAttribute("parent_id", (sourceColumn.ResultSet.Id).ToString()));
                                    source.Add(new XAttribute("parent_name", getResultSetName(sourceColumn.ResultSet)));
                                    if (sourceColumn.StartPosition != null && sourceColumn.EndPosition != null)
                                    {
                                        source.Add(new XAttribute("coordinate", convertPosition(sourceColumn.StartPosition + "," + sourceColumn.EndPosition)));
                                    }
                                    append = true;
                                    relationElement.Add(source);
                                }
                                else
                                {
                                    int index = getColumnIndex(sourceColumn.StarLinkColumns, targetName);
                                    if (index != -1)
                                    {
                                        source.Add(new XAttribute("id", sourceColumn.Id.ToString() + "_" + index));
                                        source.Add(new XAttribute("column", getColumnName(sourceColumn.StarLinkColumns[index])));
                                    }
                                    else
                                    {
                                        source.Add(new XAttribute("id", sourceColumn.Id.ToString()));
                                        source.Add(new XAttribute("column", sourceColumn.Name));
                                    }
                                    source.Add(new XAttribute("parent_id", (sourceColumn.ResultSet.Id).ToString()));
                                    source.Add(new XAttribute("parent_name", getResultSetName(sourceColumn.ResultSet)));
                                    if (sourceColumn.StartPosition != null && sourceColumn.EndPosition != null)
                                    {
                                        source.Add(new XAttribute("coordinate", convertPosition(sourceColumn.StartPosition + "," + sourceColumn.EndPosition)));
                                    }
                                    append = true;
                                    relationElement.Add(source);
                                }
                            }
                            else
                            {
                                Element source = new Element("source");
                                source.Add(new XAttribute("id", sourceColumn.Id.ToString()));
                                source.Add(new XAttribute("column", sourceColumn.Name));
                                source.Add(new XAttribute("parent_id", (sourceColumn.ResultSet.Id).ToString()));
                                source.Add(new XAttribute("parent_name", getResultSetName(sourceColumn.ResultSet)));
                                if (sourceColumn.StartPosition != null && sourceColumn.EndPosition != null)
                                {
                                    source.Add(new XAttribute("coordinate", convertPosition(sourceColumn.StartPosition + "," + sourceColumn.EndPosition)));
                                }
                                append = true;
                                relationElement.Add(source);
                            }
                        }
                        else if (sourceElement is TableColumn)
                        {
                            TableColumn sourceColumn = (TableColumn)sourceElement;
                            Element source = new Element("source");
                            source.Add(new XAttribute("id", sourceColumn.Id.ToString()));
                            source.Add(new XAttribute("column", sourceColumn.Name));
                            source.Add(new XAttribute("parent_id", (sourceColumn.Table.Id).ToString()));
                            source.Add(new XAttribute("parent_name", getTableName(sourceColumn.Table)));
                            if (sourceColumn.StartPosition != null && sourceColumn.EndPosition != null)
                            {
                                source.Add(new XAttribute("coordinate", convertPosition(sourceColumn.StartPosition + "," + sourceColumn.EndPosition)));
                            }
                            append = true;
                            relationElement.Add(source);
                        }
                    }

                    if (append)
                    {
                        dlineageResult.Add(relationElement);
                    }
                }
            }
        }

        private int getColumnIndex(IList<TObjectName> starLinkColumns, string targetName)
        {
            for (int i = 0; i < starLinkColumns.Count; i++)
            {
                if (getColumnName(starLinkColumns[i]).Equals(targetName))
                {
                    return i;
                }
            }
            return -1;
        }
        private void appendStarRelation(Element dlineageResult, AbstractRelation relation, int index)
        {
            object targetElement = relation.Target.Element;

            Element relationElement = new Element("relation");
            relationElement.Add(new XAttribute("type", Enum.GetName(typeof(RelationType), relation.RelationType)));
            relationElement.Add(new XAttribute("id", relation.Id.ToString() + "_" + index));

            string targetName = "";

            if (targetElement is ResultColumn)
            {
                ResultColumn targetColumn = (ResultColumn)targetElement;

                TObjectName linkTargetColumn = targetColumn.StarLinkColumns[index];
                targetName = getColumnName(linkTargetColumn);

                Element target = new Element("target");
                target.Add(new XAttribute("id", targetColumn.Id.ToString() + "_" + index));
                target.Add(new XAttribute("column", targetName));

                target.Add(new XAttribute("parent_id", (targetColumn.ResultSet.Id).ToString()));
                target.Add(new XAttribute("parent_name", getResultSetName(targetColumn.ResultSet)));
                if (targetColumn.StartPosition != null && targetColumn.EndPosition != null)
                {
                    target.Add(new XAttribute("coordinate", convertPosition(targetColumn.StartPosition + "," + targetColumn.EndPosition)));
                }
                relationElement.Add(target);
            }
            else if (targetElement is ViewColumn)
            {
                ViewColumn targetColumn = (ViewColumn)targetElement;

                TObjectName linkTargetColumn = targetColumn.StarLinkColumns[index];
                targetName = getColumnName(linkTargetColumn);

                Element target = new Element("target");
                target.Add(new XAttribute("id", targetColumn.Id.ToString() + "_" + index));
                target.Add(new XAttribute("column", targetName));
                target.Add(new XAttribute("parent_id", (targetColumn.View.Id).ToString()));
                target.Add(new XAttribute("parent_name", targetColumn.View.Name));
                if (targetColumn.StartPosition != null && targetColumn.EndPosition != null)
                {
                    target.Add(new XAttribute("coordinate", convertPosition(targetColumn.StartPosition + "," + targetColumn.EndPosition)));
                }
                relationElement.Add(target);
            }
            else if (targetElement is TableColumn)
            {
                TableColumn targetColumn = (TableColumn)targetElement;

                TObjectName linkTargetColumn = targetColumn.StarLinkColumns[index];
                targetName = getColumnName(linkTargetColumn);

                Element target = new Element("target");
                target.Add(new XAttribute("id", targetColumn.Id.ToString() + "_" + index));
                target.Add(new XAttribute("column", targetName));
                target.Add(new XAttribute("parent_id", (targetColumn.Table.Id).ToString()));
                target.Add(new XAttribute("parent_name", targetColumn.Table.Name));
                if (targetColumn.StartPosition != null && targetColumn.EndPosition != null)
                {
                    target.Add(new XAttribute("coordinate", targetColumn.StartPosition + "," + targetColumn.EndPosition));
                }
                relationElement.Add(target);
            }
            else
            {
                return;
            }

            RelationElement[] sourceElements = relation.Sources;
            if (sourceElements.Length == 0)
            {
                return;
            }

            for (int j = 0; j < sourceElements.Length; j++)
            {
                object sourceElement = sourceElements[j].Element;
                if (sourceElement is ResultColumn)
                {
                    ResultColumn sourceColumn = (ResultColumn)sourceElement;
                    if (sourceColumn.StarLinkColumns.Count > 0)
                    {
                        for (int k = 0; k < sourceColumn.StarLinkColumns.Count; k++)
                        {
                            TObjectName sourceName = sourceColumn.StarLinkColumns[k];
                            Element source = new Element("source");
                            source.Add(new XAttribute("id", sourceColumn.Id.ToString() + "_" + k));
                            source.Add(new XAttribute("column", getColumnName(sourceName)));
                            source.Add(new XAttribute("parent_id", (sourceColumn.ResultSet.Id).ToString()));
                            source.Add(new XAttribute("parent_name", getResultSetName(sourceColumn.ResultSet)));
                            if (sourceColumn.StartPosition != null && sourceColumn.EndPosition != null)
                            {
                                source.Add(new XAttribute("coordinate", convertPosition(sourceColumn.StartPosition + "," + sourceColumn.EndPosition)));
                            }
                            if (relation.RelationType == RelationType.dataflow)
                            {
                                if (!targetName.Equals(getColumnName(sourceName)))
                                {
                                    continue;
                                }
                            }
                            relationElement.Add(source);
                        }
                    }
                    else
                    {
                        Element source = new Element("source");
                        source.Add(new XAttribute("id", sourceColumn.Id.ToString()));
                        source.Add(new XAttribute("column", sourceColumn.Name));
                        source.Add(new XAttribute("parent_id", (sourceColumn.ResultSet.Id).ToString()));
                        source.Add(new XAttribute("parent_name", getResultSetName(sourceColumn.ResultSet)));
                        if (sourceColumn.StartPosition != null && sourceColumn.EndPosition != null)
                        {
                            source.Add(new XAttribute("coordinate", convertPosition(sourceColumn.StartPosition + "," + sourceColumn.EndPosition)));
                        }
                        if (relation.RelationType == RelationType.dataflow)
                        {
                            if (!targetName.Equals(sourceColumn.Name))
                            {
                                continue;
                            }
                        }
                        relationElement.Add(source);
                    }
                }
                else if (sourceElement is TableColumn)
                {
                    TableColumn sourceColumn = (TableColumn)sourceElement;
                    Element source = new Element("source");
                    source.Add(new XAttribute("id", sourceColumn.Id.ToString()));
                    source.Add(new XAttribute("column", sourceColumn.Name));
                    source.Add(new XAttribute("parent_id", (sourceColumn.Table.Id).ToString()));
                    source.Add(new XAttribute("parent_name", getTableName(sourceColumn.Table)));
                    if (sourceColumn.StartPosition != null && sourceColumn.EndPosition != null)
                    {
                        source.Add(new XAttribute("coordinate", convertPosition(sourceColumn.StartPosition + "," + sourceColumn.EndPosition)));
                    }
                    if (relation.RelationType == RelationType.dataflow)
                    {
                        if (!targetName.Equals(sourceColumn.Name))
                        {
                            continue;
                        }
                    }
                    relationElement.Add(source);
                }
            }

            dlineageResult.Add(relationElement);
        }

        private void appendRecordSetRelation(Element dlineageResult, Relation[] relations)
        {
            for (int i = 0; i < relations.Length; i++)
            {
                AbstractRelation relation = (AbstractRelation)relations[i];
                Element relationElement = new Element("relation");
                relationElement.Add(new XAttribute("type", Enum.GetName(typeof(RelationType), relation.RelationType)));
                relationElement.Add(new XAttribute("id", relation.Id.ToString()));

                if (relation is RecordSetRelation)
                {
                    RecordSetRelation recordCountRelation = (RecordSetRelation)relation;

                    object targetElement = recordCountRelation.Target.Element;
                    if (targetElement is ResultColumn)
                    {
                        ResultColumn targetColumn = (ResultColumn)targetElement;
                        Element target = new Element("target");
                        target.Add(new XAttribute("id", targetColumn.Id.ToString()));
                        target.Add(new XAttribute("column", targetColumn.Name));
                        if (recordCountRelation.AggregateFunction != null)
                        {
                            target.Add(new XAttribute("function", recordCountRelation.AggregateFunction));
                        }
                        target.Add(new XAttribute("parent_id", (targetColumn.ResultSet.Id).ToString()));
                        target.Add(new XAttribute("parent_name", getResultSetName(targetColumn.ResultSet)));
                        if (targetColumn.StartPosition != null && targetColumn.EndPosition != null)
                        {
                            target.Add(new XAttribute("coordinate", convertPosition(targetColumn.StartPosition + "," + targetColumn.EndPosition)));
                        }
                        relationElement.Add(target);
                    }
                    else if (targetElement is TableColumn)
                    {
                        TableColumn targetColumn = (TableColumn)targetElement;
                        Element target = new Element("target");
                        target.Add(new XAttribute("id", targetColumn.Id.ToString()));
                        target.Add(new XAttribute("column", targetColumn.Name));
                        if (recordCountRelation.AggregateFunction != null)
                        {
                            target.Add(new XAttribute("function", recordCountRelation.AggregateFunction));
                        }
                        target.Add(new XAttribute("parent_id", (targetColumn.Table.Id).ToString()));
                        target.Add(new XAttribute("parent_name", getTableName(targetColumn.Table)));
                        if (targetColumn.StartPosition != null && targetColumn.EndPosition != null)
                        {
                            target.Add(new XAttribute("coordinate", convertPosition(targetColumn.StartPosition + "," + targetColumn.EndPosition)));
                        }
                        relationElement.Add(target);
                    }
                    else
                    {
                        continue;
                    }

                    RelationElement[] sourceElements = recordCountRelation.Sources;
                    if (sourceElements.Length == 0)
                    {
                        continue;
                    }

                    bool append = false;
                    for (int j = 0; j < sourceElements.Length; j++)
                    {
                        object sourceElement = sourceElements[j].Element;
                        if (sourceElement is Table)
                        {
                            Table table = (Table)sourceElement;
                            Element source = new Element("source");
                            source.Add(new XAttribute("source_id", table.Id.ToString()));
                            source.Add(new XAttribute("source_name", getTableName(table)));
                            if (table.StartPosition != null && table.EndPosition != null)
                            {
                                source.Add(new XAttribute("coordinate", convertPosition(table.StartPosition + "," + table.EndPosition)));
                            }
                            append = true;
                            relationElement.Add(source);
                        }
                        else if (sourceElement is QueryTable)
                        {
                            QueryTable table = (QueryTable)sourceElement;
                            Element source = new Element("source");
                            source.Add(new XAttribute("source_id", table.Id.ToString()));
                            source.Add(new XAttribute("source_name", getResultSetName(table)));
                            if (table.StartPosition != null && table.EndPosition != null)
                            {
                                source.Add(new XAttribute("coordinate", convertPosition(table.StartPosition + "," + table.EndPosition)));
                            }
                            append = true;
                            relationElement.Add(source);
                        }
                    }

                    if (append)
                    {
                        dlineageResult.Add(relationElement);
                    }
                }
            }
        }

        private void appendResultSets(Element dlineageResult)
        {
            IList<TResultColumnList> selectResultSets = ModelBindingManager.SelectResultSets;
            IList<TTable> tableWithSelectSetResultSets = ModelBindingManager.TableWithSelectSetResultSets;
            IList<TSelectSqlStatement> selectSetResultSets = ModelBindingManager.SelectSetResultSets;
            IList<TCTE> ctes = ModelBindingManager.CTEs;
            IList<TParseTreeNode> mergeResultSets = ModelBindingManager.MergeResultSets;
            IList<TParseTreeNode> updateResultSets = ModelBindingManager.UpdateResultSets;

            IList<TParseTreeNode> resultSets = new List<TParseTreeNode>();
            ((List<TParseTreeNode>)resultSets).AddRange(selectResultSets);
            ((List<TParseTreeNode>)resultSets).AddRange(tableWithSelectSetResultSets);
            ((List<TParseTreeNode>)resultSets).AddRange(selectSetResultSets);
            ((List<TParseTreeNode>)resultSets).AddRange(ctes);
            ((List<TParseTreeNode>)resultSets).AddRange(mergeResultSets);
            ((List<TParseTreeNode>)resultSets).AddRange(updateResultSets);

            for (int i = 0; i < resultSets.Count; i++)
            {
                ResultSet resultSetModel = (ResultSet)ModelBindingManager.getModel(resultSets[i]);
                appendResultSet(dlineageResult, resultSetModel);
            }
        }



        private void appendResultSet(Element dlineageResult, ResultSet resultSetModel)
        {
            if (resultSetModel == null)
            {
                Console.Error.WriteLine("ResultSet Model should not be null.");
            }

            if (!appendResultSetList.Contains(resultSetModel))
            {
                appendResultSetList.Add(resultSetModel);
            }
            else
            {
                return;
            }

            Element resultSetElement = new Element("resultset");
            resultSetElement.Add(new XAttribute("id", resultSetModel.Id.ToString()));
            resultSetElement.Add(new XAttribute("name", getResultSetName(resultSetModel)));
            resultSetElement.Add(new XAttribute("type", getResultSetType(resultSetModel)));
            if (resultSetModel.StartPosition != null && resultSetModel.EndPosition != null)
            {
                resultSetElement.Add(new XAttribute("coordinate", convertPosition(resultSetModel.StartPosition + "," + resultSetModel.EndPosition)));
            }
            dlineageResult.Add(resultSetElement);

            IList<ResultColumn> columns = resultSetModel.Columns;
            for (int j = 0; j < columns.Count; j++)
            {
                ResultColumn columnModel = columns[j];
                if (columnModel.StarLinkColumns.Count > 0)
                {
                    for (int k = 0; k < columnModel.StarLinkColumns.Count; k++)
                    {
                        Element columnElement = new Element("column");
                        columnElement.Add(new XAttribute("id", columnModel.Id.ToString() + "_" + k));
                        columnElement.Add(new XAttribute("name", getColumnName(columnModel.StarLinkColumns[k])));
                        if (columnModel.StartPosition != null && columnModel.EndPosition != null)
                        {
                            columnElement.Add(new XAttribute("coordinate", convertPosition(columnModel.StartPosition + "," + columnModel.EndPosition)));
                        }
                        resultSetElement.Add(columnElement);
                    }
                }
                else
                {
                    Element columnElement = new Element("column");
                    columnElement.Add(new XAttribute("id", columnModel.Id.ToString()));
                    columnElement.Add(new XAttribute("name", columnModel.Name));
                    if (columnModel.StartPosition != null && columnModel.EndPosition != null)
                    {
                        columnElement.Add(new XAttribute("coordinate", convertPosition(columnModel.StartPosition + "," + columnModel.EndPosition)));
                    }
                    resultSetElement.Add(columnElement);
                }
            }
        }

        private string getResultSetType(ResultSet resultSetModel)
        {
            if (resultSetModel is QueryTable)
            {
                QueryTable table = (QueryTable)resultSetModel;
                if (table.TableObject.CTE != null)
                {
                    return "with_cte";
                }
            }

            if (resultSetModel is SelectSetResultSet)
            {
                ESetOperatorType type = ((SelectSetResultSet)resultSetModel).SetOperatorType;
                return "select_" + Enum.GetName(typeof(ESetOperatorType), type);
            }

            if (resultSetModel is SelectResultSet)
            {
                if (((SelectResultSet)resultSetModel).SelectStmt.ParentStmt is TInsertSqlStatement)
                {
                    return "insert-select";
                }
                if (((SelectResultSet)resultSetModel).SelectStmt.ParentStmt is TUpdateSqlStatement)
                {
                    return "update-set";
                }
            }

            if (resultSetModel.GspObject is TMergeUpdateClause)
            {
                return "merge-update";
            }

            if (resultSetModel.GspObject is TMergeInsertClause)
            {
                return "merge-insert";
            }

            if (resultSetModel.GspObject is TUpdateSqlStatement)
            {
                return "update-set";
            }

            return "select_list";
        }

        private string getTableName(Table tableModel)
        {
            string tableName;
            if (!string.ReferenceEquals(tableModel.FullName, null) && tableModel.FullName.Trim().Length > 0)
            {
                return tableModel.FullName;
            }
            if (!string.ReferenceEquals(tableModel.Alias, null) && tableModel.Alias.Trim().Length > 0)
            {
                tableName = "RESULT_OF_" + tableModel.Alias.Trim();

            }
            else
            {
                tableName = getResultSetDisplayId("RESULT_OF_SELECT-QUERY");
            }
            return tableName;
        }

        private string getResultSetName(ResultSet resultSetModel)
        {

            if (ResultSet.DISPLAY_NAME.ContainsKey(resultSetModel.Id))
            {
                return ResultSet.DISPLAY_NAME[resultSetModel.Id];
            }

            if (resultSetModel is QueryTable)
            {
                QueryTable table = (QueryTable)resultSetModel;
                if (!string.ReferenceEquals(table.Alias, null) && table.Alias.Trim().Length > 0)
                {
                    string resultName = "RESULT_OF_" + table.Alias.Trim();
                    ResultSet.DISPLAY_NAME[resultSetModel.Id] = resultName;
                    return resultName;
                }
                else if (table.TableObject.CTE != null)
                {
                    string resultName = "RESULT_OF_WITH-" + table.TableObject.CTE.TableName.ToString();
                    ResultSet.DISPLAY_NAME[table.Id] = resultName;
                    return resultName;
                }
            }

            if (resultSetModel is SelectResultSet)
            {
                if (((SelectResultSet)resultSetModel).SelectStmt.ParentStmt is TInsertSqlStatement)
                {
                    string resultName = getResultSetDisplayId("INSERT-SELECT");
                    ResultSet.DISPLAY_NAME[resultSetModel.Id] = resultName;
                    return resultName;
                }

                if (((SelectResultSet)resultSetModel).SelectStmt.ParentStmt is TUpdateSqlStatement)
                {
                    string resultName = getResultSetDisplayId("UPDATE-SET");
                    ResultSet.DISPLAY_NAME[resultSetModel.Id] = resultName;
                    return resultName;
                }
            }

            if (resultSetModel is SelectSetResultSet)
            {
                ESetOperatorType type = ((SelectSetResultSet)resultSetModel).SetOperatorType;
                string resultName = getResultSetDisplayId("RESULT_OF_" + Enum.GetName(typeof(ESetOperatorType), type).ToUpper());
                ResultSet.DISPLAY_NAME[resultSetModel.Id] = resultName;
                return resultName;
            }

            if (resultSetModel.GspObject is TMergeUpdateClause)
            {
                string resultName = getResultSetDisplayId("MERGE-UPDATE");
                ResultSet.DISPLAY_NAME[resultSetModel.Id] = resultName;
                return resultName;
            }

            if (resultSetModel.GspObject is TMergeInsertClause)
            {
                string resultName = getResultSetDisplayId("MERGE-INSERT");
                ResultSet.DISPLAY_NAME[resultSetModel.Id] = resultName;
                return resultName;
            }

            if (resultSetModel.GspObject is TUpdateSqlStatement)
            {
                string resultName = getResultSetDisplayId("UPDATE-SET");
                ResultSet.DISPLAY_NAME[resultSetModel.Id] = resultName;
                return resultName;
            }

            string name = getResultSetDisplayId("RESULT_OF_SELECT-QUERY");
            ResultSet.DISPLAY_NAME[resultSetModel.Id] = name;
            return name;
        }

        private string getResultSetDisplayId(string type)
        {
            if (!ResultSet.DISPLAY_ID.ContainsKey(type))
            {
                ResultSet.DISPLAY_ID[type] = 1;
                return type;
            }
            else
            {
                int id = ResultSet.DISPLAY_ID[type].Value;
                ResultSet.DISPLAY_ID[type] = id + 1;
                return type + "-" + id;
            }
        }

        private void appendViews(Element dlineageResult)
        {
            IList<TCreateViewSqlStatement> views = ModelBindingManager.Views;
            for (int i = 0; i < views.Count; i++)
            {
                View viewModel = (View)ModelBindingManager.getViewModel(views[i]);
                Element viewElement = new Element("view");
                viewElement.Add(new XAttribute("id", viewModel.Id.ToString()));
                viewElement.Add(new XAttribute("name", viewModel.Name));
                viewElement.Add(new XAttribute("type", "view"));

                if (viewModel.StartPosition != null && viewModel.EndPosition != null)
                {
                    viewElement.Add(new XAttribute("coordinate", convertPosition(viewModel.StartPosition + "," + viewModel.EndPosition)));
                }
                dlineageResult.Add(viewElement);

                IList<ViewColumn> columns = viewModel.Columns;
                for (int j = 0; j < columns.Count; j++)
                {
                    ViewColumn columnModel = columns[j];
                    if (columnModel.StarLinkColumns.Count > 0)
                    {
                        for (int k = 0; k < columnModel.StarLinkColumns.Count; k++)
                        {
                            Element columnElement = new Element("column");
                            columnElement.Add(new XAttribute("id", columnModel.Id.ToString() + "_" + k));
                            columnElement.Add(new XAttribute("name", getColumnName(columnModel.StarLinkColumns[k])));
                            if (columnModel.StartPosition != null && columnModel.EndPosition != null)
                            {
                                columnElement.Add(new XAttribute("coordinate", convertPosition(columnModel.StartPosition + "," + columnModel.EndPosition)));
                            }
                            viewElement.Add(columnElement);
                        }
                    }
                    else
                    {
                        Element columnElement = new Element("column");
                        columnElement.Add(new XAttribute("id", columnModel.Id.ToString()));
                        columnElement.Add(new XAttribute("name", columnModel.Name));
                        if (columnModel.StartPosition != null && columnModel.EndPosition != null)
                        {
                            columnElement.Add(new XAttribute("coordinate", convertPosition(columnModel.StartPosition + "," + columnModel.EndPosition)));
                        }
                        viewElement.Add(columnElement);
                    }
                }
            }
        }

        private void appendTables(Element dlineageResult)
        {
            IList<TTable> tables = ModelBindingManager.BaseTables;
            for (int i = 0; i < tables.Count; i++)
            {
                object model = ModelBindingManager.getModel(tables[i]);
                if (model is Table)
                {
                    Table tableModel = (Table)model;
                    Element tableElement = new Element("table");
                    tableElement.Add(new XAttribute("id", tableModel.Id.ToString()));
                    tableElement.Add(new XAttribute("name", tableModel.FullName));
                    tableElement.Add(new XAttribute("type", "table"));
                    if (!string.ReferenceEquals(tableModel.Alias, null) && tableModel.Alias.Trim().Length > 0)
                    {
                        tableElement.Add(new XAttribute("alias", tableModel.Alias));
                    }
                    if (tableModel.StartPosition != null && tableModel.EndPosition != null)
                    {
                        tableElement.Add(new XAttribute("coordinate", convertPosition(tableModel.StartPosition + "," + tableModel.EndPosition)));
                    }
                    dlineageResult.Add(tableElement);

                    IList<TableColumn> columns = tableModel.Columns;
                    for (int j = 0; j < columns.Count; j++)
                    {
                        TableColumn columnModel = columns[j];
                        if (columnModel.StarLinkColumns.Count > 0)
                        {
                            for (int k = 0; k < columnModel.StarLinkColumns.Count; k++)
                            {
                                Element columnElement = new Element("column");
                                columnElement.Add(new XAttribute("id", columnModel.Id.ToString() + "_" + k));
                                columnElement.Add(new XAttribute("name", getColumnName(columnModel.StarLinkColumns[k])));
                                if (columnModel.StartPosition != null && columnModel.EndPosition != null)
                                {
                                    columnElement.Add(new XAttribute("coordinate", columnModel.StartPosition + "," + columnModel.EndPosition));
                                }
                                tableElement.Add(columnElement);
                            }
                        }
                        else
                        {
                            Element columnElement = new Element("column");
                            columnElement.Add(new XAttribute("id", columnModel.Id.ToString()));
                            columnElement.Add(new XAttribute("name", columnModel.Name));
                            if (columnModel.StartPosition != null && columnModel.EndPosition != null)
                            {
                                columnElement.Add(new XAttribute("coordinate", columnModel.StartPosition + "," + columnModel.EndPosition));
                            }
                            tableElement.Add(columnElement);
                        }
                    }
                }
                else if (model is QueryTable)
                {
                    appendResultSet(dlineageResult, (QueryTable)model);
                }
            }
        }

        private string convertPosition(string position)
        {
            return position.Replace("(", "[").Replace(")", "]");
        }

        private void analyzeSelectStmt(TSelectSqlStatement stmt)
        {
            if (!accessedStatements.Contains(stmt))
            {
                accessedStatements.Add(stmt);
            }
            else
            {
                return;
            }

            if (stmt.SetOperatorType != ESetOperatorType.none)
            {
                analyzeSelectStmt(stmt.LeftStmt);
                analyzeSelectStmt(stmt.RightStmt);

                stmtStack.Push(stmt);
                SelectSetResultSet resultSet = ModelFactory.createSelectSetResultSet(stmt);

                if (resultSet.Columns == null
                    || resultSet.Columns.Count == 0)
                {
                    if (stmt.LeftStmt.ResultColumnList != null)
                    {
                        createSelectSetResultColumns(resultSet, stmt.LeftStmt);
                    }
                    else if (stmt.RightStmt.ResultColumnList != null)
                    {
                        createSelectSetResultColumns(resultSet, stmt.RightStmt);
                    }
                }

                IList<ResultColumn> columns = resultSet.Columns;
                for (int i = 0; i < columns.Count; i++)
                {
                    DataFlowRelation relation = ModelFactory.createDataFlowRelation();
                    relation.Target = new ResultColumnRelationElement(columns[i]);

                    if (stmt.LeftStmt.ResultColumnList != null)
                    {
                        ResultSet sourceResultSet = (ResultSet)ModelBindingManager.getModel(stmt.LeftStmt.ResultColumnList);
                        if (sourceResultSet.Columns.Count > i)
                        {
                            relation.addSource(new ResultColumnRelationElement(sourceResultSet.Columns[i]));
                        }
                    }
                    else
                    {
                        ResultSet sourceResultSet = (ResultSet)ModelBindingManager.getModel(stmt.LeftStmt);
                        if (sourceResultSet != null && sourceResultSet.Columns.Count > i)
                        {
                            relation.addSource(new ResultColumnRelationElement(sourceResultSet.Columns[i]));
                        }
                    }

                    if (stmt.RightStmt.ResultColumnList != null)
                    {
                        ResultSet sourceResultSet = (ResultSet)ModelBindingManager.getModel(stmt.RightStmt.ResultColumnList);
                        if (sourceResultSet.Columns.Count > i)
                        {
                            relation.addSource(new ResultColumnRelationElement(sourceResultSet.Columns[i]));
                        }
                    }
                    else
                    {
                        ResultSet sourceResultSet = (ResultSet)ModelBindingManager.getModel(stmt.RightStmt);
                        if (sourceResultSet != null && sourceResultSet.Columns.Count > i)
                        {
                            relation.addSource(new ResultColumnRelationElement(sourceResultSet.Columns[i]));
                        }
                    }
                }

                stmtStack.Pop();
            }
            else
            {
                stmtStack.Push(stmt);

                TTableList fromTables = stmt.tables;
                for (int i = 0; i < fromTables.size(); i++)
                {
                    TTable table = fromTables.getTable(i);

                    if (table.Subquery != null)
                    {
                        QueryTable queryTable = ModelFactory.createQueryTable(table);
                        TSelectSqlStatement subquery = table.Subquery;
                        analyzeSelectStmt(subquery);

                        if (subquery.SetOperatorType != ESetOperatorType.none)
                        {
                            SelectSetResultSet selectSetResultSetModel = (SelectSetResultSet)ModelBindingManager.getModel(subquery);
                            for (int j = 0; j < selectSetResultSetModel.Columns.Count; j++)
                            {
                                ResultColumn sourceColumn = selectSetResultSetModel.Columns[j];
                                ResultColumn targetColumn = ModelFactory.createSelectSetResultColumn(queryTable, sourceColumn);
                                DataFlowRelation selectSetRalation = ModelFactory.createDataFlowRelation();
                                selectSetRalation.Target = new ResultColumnRelationElement(targetColumn);
                                selectSetRalation.addSource(new ResultColumnRelationElement(sourceColumn));
                            }
                        }
                    }
                    else if (table.CTE != null && ModelBindingManager.getModel(table.CTE) == null)
                    {
                        QueryTable queryTable = ModelFactory.createQueryTable(table);
                        TSelectSqlStatement subquery = table.CTE.Subquery;
                        if (subquery != null)
                        {
                            analyzeSelectStmt(subquery);

                            if (subquery.SetOperatorType != ESetOperatorType.none)
                            {
                                SelectSetResultSet selectSetResultSetModel = (SelectSetResultSet)ModelBindingManager.getModel(subquery);
                                for (int j = 0; j < selectSetResultSetModel.Columns.Count; j++)
                                {
                                    ResultColumn sourceColumn = selectSetResultSetModel.Columns[j];
                                    ResultColumn targetColumn = ModelFactory.createSelectSetResultColumn(queryTable, sourceColumn);
                                    DataFlowRelation selectSetRalation = ModelFactory.createDataFlowRelation();
                                    selectSetRalation.Target = new ResultColumnRelationElement(targetColumn);
                                    selectSetRalation.addSource(new ResultColumnRelationElement(sourceColumn));
                                }
                            }
                            else
                            {
                                ResultSet resultSetModel = (ResultSet)ModelBindingManager.getModel(subquery);
                                for (int j = 0; j < resultSetModel.Columns.Count; j++)
                                {
                                    ResultColumn sourceColumn = resultSetModel.Columns[j];
                                    ResultColumn targetColumn = ModelFactory.createSelectSetResultColumn(queryTable,
                                            sourceColumn);
                                    DataFlowRelation selectSetRalation = ModelFactory.createDataFlowRelation();
                                    selectSetRalation.Target = new ResultColumnRelationElement(targetColumn);
                                    selectSetRalation.addSource(new ResultColumnRelationElement(sourceColumn));
                                }
                            }

                        }
                        else if (table.CTE.UpdateStmt != null)
                        {
                            analyzeCustomSqlStmt(table.CTE.UpdateStmt);
                        }
                        else if (table.CTE.InsertStmt != null)
                        {
                            analyzeCustomSqlStmt(table.CTE.InsertStmt);
                        }
                        else if (table.CTE.DeleteStmt != null)
                        {
                            analyzeCustomSqlStmt(table.CTE.DeleteStmt);
                        }
                    }
                    else if (table.ObjectNameReferences != null && table.ObjectNameReferences.size() > 0)
                    {
                        Table tableModel = ModelFactory.createTable(table);
                        for (int j = 0; j < table.ObjectNameReferences
                                .size(); j++)
                        {
                            TObjectName @object = table.ObjectNameReferences.getObjectName(j);
                            if (!isFunctionName(@object))
                            {
                                ModelFactory.createTableColumn(tableModel, @object);
                            }
                        }
                    }
                }

                if (stmt.ResultColumnList != null)
                {
                    Object queryModel = ModelBindingManager.getModel(stmt.ResultColumnList);

                    if (queryModel == null)
                    {
                        TSelectSqlStatement parentStmt = getParentSetSelectStmt(stmt);
                        if (stmt.ParentStmt == null || parentStmt == null)
                        {
                            SelectResultSet resultSetModel = ModelFactory.createResultSet(stmt);
                            for (int i = 0; i < stmt.ResultColumnList.size(); i++)
                            {
                                TResultColumn column = stmt.ResultColumnList.getResultColumn(i);

								if (column.Expr.ComparisonType == EComparisonType.equalsTo
									&& column.Expr
											.LeftOperand
											.ObjectOperand != null)
								{
									TObjectName columnObject = column.Expr
											.LeftOperand
											.ObjectOperand;

									ResultColumn resultColumn = ModelFactory.createResultColumn(resultSetModel,
											columnObject);

									columnsInExpr visitor = new columnsInExpr(this);
									column.Expr
											.RightOperand
											.inOrderTraverse(visitor);

									IList<TObjectName> objectNames = visitor.objectNames;
									analyzeDataFlowRelation(resultColumn,
											objectNames);
								}
								else
								{
									ResultColumn resultColumn = ModelFactory.createResultColumn(resultSetModel,
											column);

									if ("*".Equals(column.ColumnNameOnly))
									{
										TObjectName columnObject = column.FieldAttr;
										TTable sourceTable = columnObject.SourceTable;
										if (columnObject.TableToken != null
												&& sourceTable != null)
										{
											TObjectName[] columns = ModelBindingManager.getTableColumns(sourceTable);
											for (int j = 0; j < columns.Length; j++)
											{
												TObjectName columnName = columns[j];
												if ("*".Equals(getColumnName(columnName)))
												{
													continue;
												}
												resultColumn.bindStarLinkColumn(columnName);
											}
										}
										else
										{
											TTableList tables = stmt.tables;
											for (int k = 0; k < tables.size(); k++)
											{
												TTable table = tables.getTable(k);
												TObjectName[] columns = ModelBindingManager.getTableColumns(table);
												for (int j = 0; j < columns.Length; j++)
												{
													TObjectName columnName = columns[j];
													if ("*".Equals(getColumnName(columnName)))
													{
														continue;
													}
													resultColumn.bindStarLinkColumn(columnName);
												}
											}
										}
									}
									analyzeResultColumn(column);
								}
							}
                        }

                        TSelectSqlStatement parent = getParentSetSelectStmt(stmt);
                        if (parent != null && parent.SetOperatorType != ESetOperatorType.none)
                        {
                            SelectResultSet resultSetModel = ModelFactory.createResultSet(stmt);
                            for (int i = 0; i < stmt.ResultColumnList.size(); i++)
                            {
                                TResultColumn column = stmt.ResultColumnList.getResultColumn(i);
                                ResultColumn resultColumn = ModelFactory.createResultColumn(resultSetModel, column);
                                if ("*".Equals(column.ColumnNameOnly))
                                {
                                    TObjectName columnObject = column.FieldAttr;
                                    TTable sourceTable = columnObject.SourceTable;
                                    if (columnObject.TableToken != null && sourceTable != null)
                                    {
                                        TObjectName[] columns = ModelBindingManager.getTableColumns(sourceTable);
                                        for (int j = 0; j < columns.Length; j++)
                                        {
                                            TObjectName columnName = columns[j];
                                            if ("*".Equals(getColumnName(columnName)))
                                            {
                                                continue;
                                            }
                                            resultColumn.bindStarLinkColumn(columnName);
                                        }
                                    }
                                    else
                                    {
                                        TTableList tables = stmt.tables;
                                        for (int k = 0; k < tables.size(); k++)
                                        {
                                            TTable table = tables.getTable(k);
                                            TObjectName[] columns = ModelBindingManager.getTableColumns(table);
                                            for (int j = 0; j < columns.Length; j++)
                                            {
                                                TObjectName columnName = columns[j];
                                                if ("*".Equals(getColumnName(columnName)))
                                                {
                                                    continue;
                                                }
                                                resultColumn.bindStarLinkColumn(columnName);
                                            }
                                        }
                                    }
                                }
                                analyzeResultColumn(column);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < stmt.ResultColumnList.size(); i++)
                        {
                            TResultColumn column = stmt.ResultColumnList.getResultColumn(i);

                            ResultColumn resultColumn;

                            if (queryModel is QueryTable)
                            {
                                resultColumn = ModelFactory.createResultColumn((QueryTable)queryModel,
                                        column);
                            }
                            else if (queryModel is ResultSet)
                            {
                                resultColumn = ModelFactory.createResultColumn((ResultSet)queryModel,
                                        column);
                            }
                            else
                            {
                                continue;
                            }

                            if ("*".Equals(column.ColumnNameOnly))
                            {
                                TObjectName columnObject = column.FieldAttr;
                                TTable sourceTable = columnObject.SourceTable;
                                if (columnObject.TableToken != null && sourceTable != null)
                                {
                                    TObjectName[] columns = ModelBindingManager.getTableColumns(sourceTable);
                                    for (int j = 0; j < columns.Length; j++)
                                    {
                                        TObjectName columnName = columns[j];
                                        if ("*".Equals(getColumnName(columnName)))
                                        {
                                            continue;
                                        }
                                        resultColumn.bindStarLinkColumn(columnName);
                                    }
                                }
                                else
                                {
                                    TTableList tables = stmt.tables;
                                    for (int k = 0; k < tables.size(); k++)
                                    {
                                        TTable table = tables.getTable(k);
                                        TObjectName[] columns = ModelBindingManager.getTableColumns(table);
                                        for (int j = 0; j < columns.Length; j++)
                                        {
                                            TObjectName columnName = columns[j];
                                            if ("*".Equals(getColumnName(columnName)))
                                            {
                                                continue;
                                            }
                                            resultColumn.bindStarLinkColumn(columnName);
                                        }
                                    }
                                }
                            }

                            analyzeResultColumn(column);
                        }
                    }
                }

                if (stmt.joins != null && stmt.joins.size() > 0)
                {
                    for (int i = 0; i < stmt.joins.size(); i++)
                    {
                        TJoin join = stmt.joins.getJoin(i);
                        if (join.JoinItems != null)
                        {
                            for (int j = 0; j < join.JoinItems.size(); j++)
                            {
                                TJoinItem joinItem = join.JoinItems.getJoinItem(j);
                                TExpression expr = joinItem.OnCondition;
                                if (expr != null)
                                {
                                    analyzeFilterCondtion(expr);
                                }
                            }
                        }
                    }
                }

                if (stmt.WhereClause != null)
                {
                    TExpression expr = stmt.WhereClause.Condition;
                    if (expr != null)
                    {
                        analyzeFilterCondtion(expr);
                    }
                }

                if (stmt.GroupByClause != null)
                {
                    TGroupByItemList groupByList = stmt.GroupByClause.Items;
                    for (int i = 0; i < groupByList.size(); i++)
                    {
                        TGroupByItem groupBy = groupByList.getGroupByItem(i);
                        TExpression expr = groupBy.Expr;
                        analyzeAggregate(expr);
                    }

                    if (stmt.GroupByClause.HavingClause != null)
                    {
                        analyzeAggregate(stmt.GroupByClause.HavingClause);
                    }
                }

                stmtStack.Pop();
            }
        }

        private TSelectSqlStatement getParentSetSelectStmt(TSelectSqlStatement stmt)
        {
            TCustomSqlStatement parent = stmt.ParentStmt;
            if (parent == null)
            {
                return null;
            }
            if (parent.Statements != null)
            {
                for (int i = 0; i < parent.Statements.size(); i++)
                {
                    TCustomSqlStatement temp = parent.Statements.get(i);
                    if (temp is TSelectSqlStatement)
                    {
                        TSelectSqlStatement select = (TSelectSqlStatement)temp;
                        if (select.LeftStmt == stmt || select.RightStmt == stmt)
                        {
                            return select;
                        }
                    }
                }
            }
            if (parent is TSelectSqlStatement)
            {
                TSelectSqlStatement select = (TSelectSqlStatement)parent;
                if (select.LeftStmt == stmt || select.RightStmt == stmt)
                {
                    return select;
                }
            }
            return null;
        }

        private void createSelectSetResultColumns(SelectSetResultSet resultSet, TSelectSqlStatement stmt)
        {
            if (stmt.SetOperatorType != ESetOperatorType.none)
            {
                createSelectSetResultColumns(resultSet, stmt.LeftStmt);
            }
            else
            {
                TResultColumnList columnList = stmt.ResultColumnList;
                for (int i = 0; i < columnList.size(); i++)
                {
                    TResultColumn column = columnList.getResultColumn(i);
                    ResultColumn resultColumn = ModelFactory.createSelectSetResultColumn(resultSet, column);

                    if (resultColumn.ColumnObject is TResultColumn)
                    {
                        TResultColumn columnObject = (TResultColumn)resultColumn.ColumnObject;
                        if (columnObject.FieldAttr != null)
                        {
                            if ("*".Equals(getColumnName(columnObject.FieldAttr)))
                            {
                                TObjectName fieldAttr = columnObject.FieldAttr;
                                TTable sourceTable = fieldAttr.SourceTable;
                                if (fieldAttr.TableToken != null && sourceTable != null)
                                {
                                    TObjectName[] columns = ModelBindingManager.getTableColumns(sourceTable);
                                    for (int j = 0; j < columns.Length; j++)
                                    {
                                        TObjectName columnName = columns[j];
                                        if ("*".Equals(getColumnName(columnName)))
                                        {
                                            continue;
                                        }
                                        resultColumn.bindStarLinkColumn(columnName);
                                    }
                                }
                                else
                                {
                                    TTableList tables = stmt.tables;
                                    for (int k = 0; k < tables.size(); k++)
                                    {
                                        TTable tableElement = tables.getTable(k);
                                        TObjectName[] columns = ModelBindingManager.getTableColumns(tableElement);
                                        for (int j = 0; j < columns.Length; j++)
                                        {
                                            TObjectName columnName = columns[j];
                                            if ("*".Equals(getColumnName(columnName)))
                                            {
                                                continue;
                                            }
                                            resultColumn.bindStarLinkColumn(columnName);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void analyzeResultColumn(TResultColumn column)
        {
            TExpression expression = column.Expr;
            columnsInExpr visitor = new columnsInExpr(this);
            expression.inOrderTraverse(visitor);
            IList<TObjectName> objectNames = visitor.ObjectNames;

            IList<TConstant> constants = visitor.Constants;
            IList<TFunctionCall> functions = visitor.Functions;

            analyzeDataFlowRelation(column, objectNames);
            analyzeRecordSetRelation(column, functions);
        }

        private void analyzeRecordSetRelation(TResultColumn column, IList<TFunctionCall> functions)
        {
            if (functions == null || functions.Count == 0)
            {
                return;
            }

            RecordSetRelation relation = ModelFactory.createRecordSetRelation();
            relation.Target = new ResultColumnRelationElement((ResultColumn)ModelBindingManager.getModel(column));

            for (int i = 0; i < functions.Count; i++)
            {
                TFunctionCall function = functions[i];
                if (stmtStack.Peek().tables.size() == 1)
                {
                    object tableObject = ModelBindingManager.getModel(stmtStack.Peek().tables.getTable(0));
                    if (tableObject is Table)
                    {
                        Table tableModel = (Table)tableObject;
                        relation.addSource(new TableRelationElement(tableModel));
                        relation.AggregateFunction = function.FunctionName.ToString();
                    }
                    else if (tableObject is QueryTable)
                    {
                        QueryTable tableModel = (QueryTable)tableObject;
                        relation.addSource(new QueryTableRelationElement(tableModel));
                        relation.AggregateFunction = function.FunctionName.ToString();
                    }
                }
            }
        }

        private void analyzeDataFlowRelation(TParseTreeNode gspObject, IList<TObjectName> objectNames)
        {
            object columnObject = ModelBindingManager.getModel(gspObject);
            analyzeDataFlowRelation(columnObject, objectNames);
        }

        private void analyzeDataFlowRelation(object modelObject, IList<TObjectName> objectNames)
        {
            if (objectNames == null || objectNames.Count == 0)
            {
                return;
            }

            DataFlowRelation relation = ModelFactory.createDataFlowRelation();

            if (modelObject is ResultColumn)
            {
                relation.Target = new ResultColumnRelationElement((ResultColumn)modelObject);
            }
            else if (modelObject is TableColumn)
            {
                relation.Target = new TableColumnRelationElement((TableColumn)modelObject);
            }
            else if (modelObject is ViewColumn)
            {
                relation.Target = new ViewColumnRelationElement((ViewColumn)modelObject);
            }
            else
            {
                throw new System.NotSupportedException();
            }

            for (int i = 0; i < objectNames.Count; i++)
            {
                TObjectName columnName = objectNames[i];

                IList<TTable> tables = new List<TTable>();
                {
                    TCustomSqlStatement stmt = stmtStack.Peek();

                    TTable table = columnName.SourceTable;

                    if (table == null)
                    {
                        table = ModelBindingManager.getTable(stmt, columnName);
                    }

                    if (table == null)
                    {
                        if (columnName.TableToken != null || !"*".Equals(getColumnName(columnName)))
                        {
                            table = columnName.SourceTable;
                        }
                    }

                    if (table == null)
                    {
                        if (stmt.tables != null)
                        {
                            for (int j = 0; j < stmt.tables.size(); j++)
                            {
                                if (table != null)
                                {
                                    break;
                                }

                                TTable tTable = stmt.tables.getTable(j);
                                if (tTable.ObjectNameReferences != null
                                    && tTable.ObjectNameReferences.size() > 0)
                                {
                                    for (int z = 0; z < tTable.ObjectNameReferences.size(); z++)
                                    {
                                        TObjectName refer = tTable.ObjectNameReferences.getObjectName(z);
                                        if ("*".Equals(getColumnName(refer)))
                                        {
                                            continue;
                                        }
                                        if (refer == columnName)
                                        {
                                            table = tTable;
                                            break;
                                        }
                                    }
                                }
                                else if (columnName.TableToken != null
                                  && (columnName.TableToken.astext.Equals(tTable.Name)
                                  || columnName.TableToken.astext.Equals(tTable.AliasName)))
                                {
                                    table = tTable;
                                    break;
                                }
                            }
                        }
                    }

                    if (table != null)
                    {
                        tables.Add(table);
                    }
                    else if (columnName.TableToken == null && "*".Equals(getColumnName(columnName)))
                    {
                        if (stmt.tables != null)
                        {
                            for (int j = 0; j < stmt.tables.size(); j++)
                            {
                                tables.Add(stmt.tables.getTable(j));
                            }
                        }
                    }
                }

                for (int k = 0; k < tables.Count; k++)
                {
                    TTable table = tables[k];
                    if (table != null)
                    {
                        if (ModelBindingManager.getModel(table) is Table)
                        {
                            Table tableModel = (Table)ModelBindingManager.getModel(table);
                            if (tableModel != null)
                            {
                                if (getColumnName(columnName).Equals("*"))
                                {
                                    TObjectName[] columns = ModelBindingManager.getTableColumns(table);
                                    for (int j = 0; j < columns.Length; j++)
                                    {
                                        TObjectName objectName = columns[j];
                                        if ("*".Equals(getColumnName(objectName)))
                                        {
                                            continue;
                                        }
                                        TableColumn columnModel = ModelFactory.createTableColumn(tableModel, objectName);
                                        relation.addSource(new TableColumnRelationElement(columnModel));
                                    }
                                }
                                else
                                {
                                    TableColumn columnModel = ModelFactory.createTableColumn(tableModel, columnName);
                                    relation.addSource(new TableColumnRelationElement(columnModel));
                                }
                            }
                        }
                        else if (ModelBindingManager.getModel(table) is QueryTable)
                        {
                            QueryTable queryTable = (QueryTable)ModelBindingManager.getModel(table);
                            TSelectSqlStatement subquery = null;
                            if (queryTable.TableObject.CTE != null)
                            {
                                subquery = queryTable.TableObject.CTE.Subquery;
                            }
                            else
                            {
                                subquery = queryTable.TableObject.Subquery;
                            }


                            if (subquery != null && subquery.SetOperatorType != ESetOperatorType.none)
                            {
                                SelectSetResultSet selectSetResultSetModel = (SelectSetResultSet)ModelBindingManager.getModel(subquery);
                                if (selectSetResultSetModel != null)
                                {
                                    for (int j = 0; j < selectSetResultSetModel.Columns.Count; j++)
                                    {
                                        ResultColumn sourceColumn = selectSetResultSetModel.Columns[j];
                                        if (sourceColumn.Name.Equals(columnName.ColumnNameOnly))
                                        {
                                            ResultColumn targetColumn = ModelFactory.createSelectSetResultColumn(queryTable, sourceColumn);
                                            relation.addSource(new ResultColumnRelationElement(targetColumn));
                                            break;
                                        }
                                    }
                                }
                            }

                            IList<ResultColumn> columns = queryTable.Columns;
                            if (getColumnName(columnName).Equals("*"))
                            {
                                for (int j = 0; j < queryTable.Columns.Count; j++)
                                {
                                    relation.addSource(new ResultColumnRelationElement(queryTable.Columns[j]));
                                }
                            }
                            else
                            {
                                if (table.CTE != null)
                                {
                                    for (k = 0; k < columns.Count; k++)
                                    {
                                        ResultColumn column = columns[k];
                                        if (SQLUtil.trimObjectName(columnName.ColumnNameOnly).Equals(SQLUtil.trimObjectName(column.Name)))
                                        {
                                            if (!column.Equals(modelObject))
                                            {
                                                relation.addSource(new ResultColumnRelationElement(column));
                                            }
                                            break;
                                        }
                                    }
                                }
                                else if (table.Subquery != null)
                                {
                                    if (columnName.SourceColumn != null)
                                    {
                                        object model = ModelBindingManager.getModel(columnName.SourceColumn);
                                        if (model is ResultColumn)
                                        {
                                            ResultColumn resultColumn = (ResultColumn)model;
                                            relation.addSource(new ResultColumnRelationElement(resultColumn));
                                        }
                                    }
                                    else if (columnName.SourceTable != null)
                                    {
                                        object tablModel = ModelBindingManager.getModel(columnName.SourceTable);
                                        if (tablModel is Table)
                                        {
                                            object model = ModelBindingManager.getModel(new Tuple<Table, TObjectName>((Table)tablModel, columnName));
                                            if (model is TableColumn)
                                            {
                                                relation.addSource(new TableColumnRelationElement((TableColumn)model));
                                            }
                                        }
                                        else if (tablModel is QueryTable)
                                        {
                                            IList<ResultColumn> queryColumns = ((QueryTable)tablModel).Columns;
                                            for (int l = 0; l < queryColumns.Count; l++)
                                            {
                                                ResultColumn column = queryColumns[l];
                                                if (getColumnName(columnName).Equals(column.Name,StringComparison.OrdinalIgnoreCase))
                                                {
                                                    if (!column.Equals(modelObject))
                                                    {
                                                        relation.addSource(new ResultColumnRelationElement(column));
                                                    }
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void analyzeAggregate(TExpression expr)
        {
            if (expr == null)
            {
                return;
            }

            TCustomSqlStatement stmt = stmtStack.Peek();

            columnsInExpr visitor = new columnsInExpr(this);
            expr.inOrderTraverse(visitor);
            IList<TObjectName> objectNames = visitor.ObjectNames;

            TResultColumnList columns = stmt.ResultColumnList;
            for (int i = 0; i < columns.size(); i++)
            {
                TResultColumn column = columns.getResultColumn(i);
                AbstractRelation relation;
                if (isAggregateFunction(column.Expr.FunctionCall))
                {
                    relation = ModelFactory.createRecordSetRelation();
                    relation.Target = new ResultColumnRelationElement((ResultColumn)ModelBindingManager.getModel(column));
                    ((RecordSetRelation)relation).AggregateFunction = column.Expr.FunctionCall.FunctionName.ToString();
                }
                else
                {
                    relation = ModelFactory.createImpactRelation();
                    relation.Target = new ResultColumnRelationElement((ResultColumn)ModelBindingManager.getModel(column));
                }

                for (int j = 0; j < objectNames.Count; j++)
                {
                    TObjectName columnName = objectNames[j];

                    TTable table = ModelBindingManager.getTable(stmt, columnName);
                    if (table != null)
                    {
                        if (ModelBindingManager.getModel(table) is Table)
                        {
                            Table tableModel = (Table)ModelBindingManager.getModel(table);
                            if (tableModel != null)
                            {
                                TableColumn columnModel = ModelFactory.createTableColumn(tableModel, columnName);
                                relation.addSource(new TableColumnRelationElement(columnModel));
                            }
                        }
                        else if (ModelBindingManager.getModel(table) is QueryTable)
                        {
                            ResultColumn resultColumn = (ResultColumn)ModelBindingManager.getModel(columnName.SourceColumn);
                            if (resultColumn != null)
                            {
                                relation.addSource(new ResultColumnRelationElement(resultColumn));
                            }
                        }
                    }
                }
            }
        }

        private void analyzeFilterCondtion(TExpression expr)
        {
            if (expr == null)
            {
                return;
            }

            TCustomSqlStatement stmt = stmtStack.Peek();

            columnsInExpr visitor = new columnsInExpr(this);
            expr.inOrderTraverse(visitor);
            IList<TObjectName> objectNames = visitor.ObjectNames;

            TResultColumnList columns = stmt.ResultColumnList;
            if (columns != null)
            {
                for (int i = 0; i < columns.size(); i++)
                {
                    TResultColumn column = columns.getResultColumn(i);

                    AbstractRelation relation;
                    if (isAggregateFunction(column.Expr.FunctionCall))
                    {
                        relation = ModelFactory.createRecordSetRelation();
                        relation.Target = new ResultColumnRelationElement((ResultColumn)ModelBindingManager.getModel(column));
                        ((RecordSetRelation)relation).AggregateFunction = column.Expr.FunctionCall.FunctionName.ToString();
                    }
                    else
                    {
                        relation = ModelFactory.createImpactRelation();
                        relation.Target = new ResultColumnRelationElement((ResultColumn)ModelBindingManager.getModel(column));
                    }

                    for (int j = 0; j < objectNames.Count; j++)
                    {
                        TObjectName columnName = objectNames[j];

                        TTable table = ModelBindingManager.getTable(stmt, columnName);
                        if (table != null)
                        {
                            if (ModelBindingManager.getModel(table) is Table)
                            {
                                Table tableModel = (Table)ModelBindingManager.getModel(table);
                                if (tableModel != null)
                                {
                                    TableColumn columnModel = ModelFactory.createTableColumn(tableModel, columnName);
                                    relation.addSource(new TableColumnRelationElement(columnModel));
                                }
                            }
                            else if (ModelBindingManager.getModel(table) is QueryTable)
                            {
                                ResultColumn resultColumn = (ResultColumn)ModelBindingManager.getModel(columnName.SourceColumn);
                                if (resultColumn != null)
                                {
                                    relation.addSource(new ResultColumnRelationElement(resultColumn));
                                }
                            }
                        }
                    }
                }
            }
        }

        private string getColumnName(TObjectName column)
        {
            string name = column.ColumnNameOnly;
            if (name == null || "".Equals(name.Trim()))
            {
                return column.ToString();
            }
            else
                return name.Trim();
        }

        internal class columnsInExpr : IExpressionVisitor
        {
            private readonly DataFlowAnalyzer outerInstance;

            public columnsInExpr(DataFlowAnalyzer outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            internal IList<TConstant> constants = new List<TConstant>();
            internal IList<TObjectName> objectNames = new List<TObjectName>();
            internal IList<TFunctionCall> functions = new List<TFunctionCall>();

            public virtual IList<TFunctionCall> Functions
            {
                get
                {
                    return functions;
                }
            }

            public virtual IList<TConstant> Constants
            {
                get
                {
                    return constants;
                }
            }

            public virtual IList<TObjectName> ObjectNames
            {
                get
                {
                    return objectNames;
                }
            }

            public virtual bool exprVisit(TParseTreeNode pNode, bool isLeafNode)
            {
                TExpression lcexpr = (TExpression)pNode;
                if (lcexpr.ExpressionType == EExpressionType.simple_constant_t)
                {
                    if (lcexpr.ConstantOperand != null)
                    {
                        constants.Add(lcexpr.ConstantOperand);
                    }
                }
                else if (lcexpr.ExpressionType == EExpressionType.simple_object_name_t)
                {
                    if (lcexpr.ObjectOperand != null && !outerInstance.isFunctionName(lcexpr.ObjectOperand))
                    {
                        objectNames.Add(lcexpr.ObjectOperand);
                    }
                }
                else if (lcexpr.ExpressionType == EExpressionType.function_t)
                {
                    TFunctionCall func = lcexpr.FunctionCall;
                    if (outerInstance.isAggregateFunction(func))
                    {
                        functions.Add(func);
                    }

                    if (func.Args != null)
                    {
                        for (int k = 0; k < func.Args.size(); k++)
                        {
                            TExpression expr = func.Args.getExpression(k);
                            if (expr != null)
                            {
                                expr.inOrderTraverse(this);
                            }
                        }
                    }

                    if (func.TrimArgument != null)
                    {
                        TTrimArgument args = func.TrimArgument;
                        TExpression expr = args.StringExpression;
                        if (expr != null)
                        {
                            expr.inOrderTraverse(this);
                        }
                        expr = args.TrimCharacter;
                        if (expr != null)
                        {
                            expr.inOrderTraverse(this);
                        }
                    }

                    if (func.AgainstExpr != null)
                    {
                        func.AgainstExpr.inOrderTraverse(this);
                    }
                    if (func.BetweenExpr != null)
                    {
                        func.BetweenExpr.inOrderTraverse(this);
                    }
                    if (func.Expr1 != null)
                    {
                        func.Expr1.inOrderTraverse(this);
                    }
                    if (func.Expr2 != null)
                    {
                        func.Expr2.inOrderTraverse(this);
                    }
                    if (func.Expr3 != null)
                    {
                        func.Expr3.inOrderTraverse(this);
                    }
                    if (func.Parameter != null)
                    {
                        func.Parameter.inOrderTraverse(this);
                    }
                }
                else if (lcexpr.ExpressionType == EExpressionType.case_t)
                {
                    TCaseExpression expr = lcexpr.CaseExpression;
                    TExpression defaultExpr = expr.Else_expr;
                    if (defaultExpr != null)
                    {
                        defaultExpr.inOrderTraverse(this);
                    }
                    TWhenClauseItemList list = expr.WhenClauseItemList;
                    for (int i = 0; i < list.size(); i++)
                    {
                        TWhenClauseItem element = (TWhenClauseItem)list.getElement(i);
                        (((TWhenClauseItem)element).Return_expr).inOrderTraverse(this);
                    }
                }
                else if (lcexpr.ExpressionType == EExpressionType.subquery_t)
                {
                    TSelectSqlStatement select = lcexpr.SubQuery;
                    outerInstance.analyzeSelectStmt(select);
                    inOrderTraverse(select, this);
                }
                return true;
            }

            private void inOrderTraverse(TSelectSqlStatement select,
                columnsInExpr columnsInExpr)
            {
                if (select.ResultColumnList != null)
                {
                    for (int i = 0; i < select.ResultColumnList.size(); i++)
                    {
                        select.ResultColumnList
                                .getResultColumn(i)
                                .Expr
                                .inOrderTraverse(columnsInExpr);
                    }
                }
                else if (select.SetOperatorType != ESetOperatorType.none)
                {
                    inOrderTraverse(select.LeftStmt, columnsInExpr);
                    inOrderTraverse(select.RightStmt, columnsInExpr);
                }
            }
        }

        public virtual bool isFunctionName(TObjectName @object)
        {
            if (@object == null || @object.Gsqlparser == null)
            {
                return false;
            }
            EDbVendor vendor = @object.Gsqlparser.DbVendor;
            if (vendor == EDbVendor.dbvteradata)
            {
                bool result = TERADATA_BUILTIN_FUNCTIONS.Contains(@object.ToString());
                if (result)
                {
                    return true;
                }
            }

            List<string> versions = functionChecker.getAvailableDbVersions(vendor);
            if (versions != null && versions.Count > 0)
            {
                for (int i = 0; i < versions.Count; i++)
                {
                    bool isFunction = functionChecker.isBuiltInFunction(@object.ToString(), @object.Gsqlparser.DbVendor, versions[i]);
                    if (!isFunction)
                    {
                        return false;
                    }
                }

                bool result = TERADATA_BUILTIN_FUNCTIONS.Contains(@object.ToString());
                if (result)
                {
                    return true;
                }
            }

            return false;
        }

        public virtual bool isAggregateFunction(TFunctionCall func)
        {
            if (func == null)
            {
                return false;
            }
            return new List<string>(new string[] { "AVG", "COUNT", "MAX", "MIN", "SUM", "COLLECT", "CORR", "COVAR_POP", "COVAR_SAMP", "CUME_DIST", "DENSE_RANK", "FIRST", "GROUP_ID", "GROUPING", "GROUPING_ID", "LAST", "LISTAGG", "MEDIAN", "PERCENT_RANK", "PERCENTILE_CONT", "PERCENTILE_DISC", "RANK", "STATS_BINOMIAL_TEST", "STATS_CROSSTAB", "STATS_F_TEST", "STATS_KS_TEST", "STATS_MODE", "STATS_MW_TEST", "STATS_ONE_WAY_ANOVA", "STATS_WSR_TEST", "STDDEV", "STDDEV_POP", "STDDEV_SAMP", "SYS_XMLAGG", "VAR_ POP", "VAR_ SAMP", "VARI ANCE", "XMLAGG" }).Contains(func.FunctionName.ToString());
        }

        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: DataFlowAnalyzer [/f <path_to_sql_file>] [/d <path_to_directory_includes_sql_files>] [/t <database type>] [/o <output file path>]");
                Console.WriteLine("/f: Option, specify the sql file path to analyze dataflow relation.");
                Console.WriteLine("/d: Option, specify the sql directory path to analyze dataflow relation.");
                Console.WriteLine("/t: Option, set the database type. Support oracle, mysql, mssql, db2, netezza, teradata, informix, sybase, postgresql, hive, greenplum and redshift, the default type is oracle");
                Console.WriteLine("/o: Option, write the output stream to the specified file.");
                Console.WriteLine("/log: Option, generate a dataflow.log file to log information.");
                return;
            }

            FileInfo sqlFiles = null;

            IList<string> argList = new List<string>(args);
            if (argList.IndexOf("/f") != -1 && argList.Count > argList.IndexOf("/f") + 1)
            {
                sqlFiles = new FileInfo(args[argList.IndexOf("/f") + 1]);
                if (!sqlFiles.Exists || sqlFiles.Attributes.HasFlag(FileAttributes.Directory))
                {
                    Console.WriteLine(sqlFiles + " is not a valid file.");
                    return;
                }
            }
            else if (argList.IndexOf("/d") != -1 && argList.Count > argList.IndexOf("/d") + 1)
            {
                sqlFiles = new FileInfo(args[argList.IndexOf("/d") + 1]);
                if (!sqlFiles.Attributes.HasFlag(FileAttributes.Directory))
                {
                    Console.WriteLine(sqlFiles + " is not a valid directory.");
                    return;
                }
            }

            string outputFile = null;

            int index = argList.IndexOf("/o");
            if (index != -1 && args.Length > index + 1)
            {
                outputFile = args[index + 1];
            }

            System.IO.FileStream writer = null;
            StreamWriter sw = null;

            if (!string.ReferenceEquals(outputFile, null))
            {
                try
                {
                    writer = new System.IO.FileStream(outputFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    sw = new StreamWriter(writer);
                    Console.SetOut(sw);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }

            DataFlowAnalyzer dlineage = new DataFlowAnalyzer(sqlFiles, Common.GetEDbVendor(args));

            StringBuilder errorBuffer = new StringBuilder();

            string result = dlineage.generateDataFlow(errorBuffer);

            bool log = argList.IndexOf("/log") != -1;

            TextWriter pw = null;
            StringBuilder errsw = null;
            TextWriter systemSteam = Console.Error;

            try
            {
                errsw = new StringBuilder();
                pw = new StringWriter(errsw);
                Console.SetError(pw);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

            if (!string.ReferenceEquals(result, null))
            {
                Console.WriteLine(result);

                if (writer != null && result.Length < 1024 * 1024)
                {
                    Console.Error.WriteLine(result);
                }
            }

            if (errorBuffer.Length > 0)
            {
                Console.Error.WriteLine("Error log:\n" + errorBuffer.ToString());
            }

            try
            {
                if (sw != null && writer != null)
                {
                    sw.Close();
                    writer.Close();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

            if (pw != null)
            {
                pw.Close();
            }

            if (errsw != null)
            {
                string errorMessage = errsw.ToString().Trim();
                if (errorMessage.Length > 0)
                {
                    if (log)
                    {
                        try
                        {
                            pw = new StreamWriter(new FileInfo("./dlineageRelation.log").FullName);
                            pw.WriteLine(errorMessage);
                            pw.Close();

                        }
                        catch (FileNotFoundException e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.Write(e.StackTrace);
                        }
                    }

                    Console.SetError(systemSteam);
                    Console.Error.WriteLine(errorMessage);
                }
            }
        }
    }

}