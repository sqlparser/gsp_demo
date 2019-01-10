using System;
using System.Collections.Generic;
using System.Text;

namespace gudusoft.gsqlparser.demos.columnImpact
{
    using Document = System.Xml.Linq.XDocument;
    using Element = System.Xml.Linq.XElement;
    using System.Drawing;
    using System.IO;
    using gudusoft.gsqlparser.demos.util;
    using System.Xml.Linq;
    using System.Text.RegularExpressions;
    using gudusoft.gsqlparser.nodes;
    using gudusoft.gsqlparser.stmt;
    using gudusoft.gsqlparser;

    public class ColumnImpact
    {

        public enum ClauseType
        {
            connectby,
            groupby,
            join,
            orderby,
            select,
            startwith,
            undefine,
            @where
        }

        internal class columnsInExpr : IExpressionVisitor
        {
            private readonly ColumnImpact outerInstance;


            internal IList<TColumn> columns;
            internal TExpression expr;
            internal ColumnImpact impact;
            internal int level;
            internal TCustomSqlStatement stmt;
            internal bool collectExpr;
            internal ClauseType clauseType;
            internal TAlias parentAlias;

            public columnsInExpr(ColumnImpact outerInstance, ColumnImpact impact, TExpression expr, IList<TColumn> columns, TCustomSqlStatement stmt, int level, bool collectExpr, ClauseType clauseType, TAlias parentAlias)
            {
                this.outerInstance = outerInstance;
                this.stmt = stmt;
                this.impact = impact;
                this.expr = expr;
                this.columns = columns;
                this.level = level;
                this.collectExpr = collectExpr;
                this.clauseType = clauseType;
                this.parentAlias = parentAlias;
            }

            internal virtual void addColumnToList(TParseTreeNodeList list, TCustomSqlStatement stmt)
            {
                if (list != null)
                {
                    for (int i = 0; i < list.size(); i++)
                    {
                        IList<TExpression> exprList = new List<TExpression>();
                        object element = list.getElement(i);

                        if (element is TGroupByItem)
                        {
                            if (!outerInstance.traceView && !outerInstance.isColumnLevel)
                            {
                                exprList.Add(((TGroupByItem)element).Expr);
                            }
                        }
                        if (element is TOrderByItem)
                        {
                            if (!outerInstance.traceView && !outerInstance.isColumnLevel)
                            {
                                exprList.Add(((TOrderByItem)element).SortKey);
                            }
                        }
                        else if (element is TExpression)
                        {
                            exprList.Add((TExpression)element);
                        }
                        else if (element is TWhenClauseItem)
                        {
                            if (!outerInstance.traceView && !outerInstance.isColumnLevel)
                            {
                                exprList.Add(((TWhenClauseItem)element).Comparison_expr);
                            }
                            exprList.Add(((TWhenClauseItem)element).Return_expr);
                        }

                        foreach (TExpression expr in exprList)
                        {
                            if (expr != null)
                            {
                                expr.inOrderTraverse(this);
                            }
                        }
                    }
                }
            }

            public virtual bool exprVisit(TParseTreeNode pNode, bool isLeafNode)
            {
                TExpression lcexpr = (TExpression)pNode;
                if (lcexpr.ExpressionType == EExpressionType.simple_object_name_t)
                {
                    columns.Add(impact.attrToColumn(lcexpr, stmt, expr, collectExpr, clauseType, parentAlias));
                }
                else if (lcexpr.ExpressionType == EExpressionType.between_t)
                {
                    columns.Add(impact.attrToColumn(lcexpr.BetweenOperand, stmt, expr, collectExpr, clauseType, parentAlias));
                }
                else if (lcexpr.ExpressionType == EExpressionType.function_t)
                {
                    TFunctionCall func = (TFunctionCall)lcexpr.FunctionCall;
                    if (func.FunctionType == EFunctionType.trim_t)
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
                    else if (func.FunctionType == EFunctionType.cast_t)
                    {
                        TExpression expr = func.Expr1;
                        if (expr != null)
                        {
                            expr.inOrderTraverse(this);
                        }
                    }
                    else if (func.FunctionType == EFunctionType.convert_t)
                    {
                        TExpression expr = func.Expr1;
                        if (expr != null)
                        {
                            expr.inOrderTraverse(this);
                        }
                        expr = func.Expr2;
                        if (expr != null)
                        {
                            expr.inOrderTraverse(this);
                        }
                        expr = func.Parameter;
                        if (expr != null)
                        {
                            expr.inOrderTraverse(this);
                        }
                    }
                    else if (func.FunctionType == EFunctionType.contains_t || func.FunctionType == EFunctionType.freetext_t)
                    {
                        TExpression expr = func.Expr1;
                        if (expr != null)
                        {
                            expr.inOrderTraverse(this);
                        }
                        TInExpr inExpr = func.InExpr;
                        if (inExpr.ExprList != null)
                        {
                            for (int k = 0; k < inExpr.ExprList.size(); k++)
                            {
                                expr = inExpr.ExprList.getExpression(k);
                                expr.inOrderTraverse(this);
                            }
                            if (expr != null)
                            {
                                expr.inOrderTraverse(this);
                            }
                        }
                        expr = inExpr.Func_expr;
                        if (expr != null)
                        {
                            expr.inOrderTraverse(this);
                        }
                    }
                    else if (func.FunctionType == EFunctionType.extractxml_t)
                    {
                        TExpression expr = func.XMLType_Instance;
                        if (expr != null)
                        {
                            expr.inOrderTraverse(this);
                        }
                        expr = func.XPath_String;
                        if (expr != null)
                        {
                            expr.inOrderTraverse(this);
                        }
                        expr = func.Namespace_String;
                        if (expr != null)
                        {
                            expr.inOrderTraverse(this);
                        }
                    }
                    else if (func.FunctionType == EFunctionType.rank_t)
                    {
                        TOrderByItemList orderByList = func.OrderByList;
                        for (int k = 0; k < orderByList.size(); k++)
                        {
                            TExpression expr = orderByList.getOrderByItem(k).SortKey;
                            if (expr != null)
                            {
                                expr.inOrderTraverse(this);
                            }
                        }
                    }
                    else if (func.Args != null)
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
                    if (func.AnalyticFunction != null)
                    {
                        TParseTreeNodeList list = func.AnalyticFunction.PartitionBy_ExprList;
                        addColumnToList(list, stmt);

                        if (func.AnalyticFunction.OrderBy != null)
                        {
                            list = func.AnalyticFunction.OrderBy.Items;
                            addColumnToList(list, stmt);
                        }
                    }
                    else if (func.WindowDef != null)
                    {
                        if (func.WindowDef.PartitionClause != null)
                        {
                            TParseTreeNodeList list = func.WindowDef.PartitionClause.ExpressionList;
                            addColumnToList(list, stmt);
                        }
                        if (func.WindowDef.orderBy != null)
                        {
                            TParseTreeNodeList list = func.WindowDef.orderBy.Items;
                            addColumnToList(list, stmt);
                        }
                    }

                }
                else if (lcexpr.ExpressionType == EExpressionType.subquery_t)
                {
                    impact.impactSqlFromStatement(lcexpr.SubQuery, level + 1);
                }
                else if (lcexpr.ExpressionType == EExpressionType.case_t)
                {
                    TCaseExpression expr = lcexpr.CaseExpression;
                    TExpression conditionExpr = expr.Input_expr;
                    if (conditionExpr != null)
                    {
                        conditionExpr.inOrderTraverse(this);
                    }
                    TExpression defaultExpr = expr.Else_expr;
                    if (defaultExpr != null)
                    {
                        defaultExpr.inOrderTraverse(this);
                    }
                    TWhenClauseItemList list = expr.WhenClauseItemList;
                    addColumnToList(list, stmt);
                }
                return true;
            }

            public virtual void searchColumn()
            {
                this.expr.inOrderTraverse(this);
            }
        }

        internal class Table
        {
            private readonly ColumnImpact outerInstance;

            public Table(ColumnImpact outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public string prefixName;
            public string tableAlias;
            public string tableName;
        }

        internal class TAlias
        {
            private readonly ColumnImpact outerInstance;

            public TAlias(ColumnImpact outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public string alias;
            public string column;
            public Point location;
            public TExpression columnExpr;
        }

        private IList<TColumn> columnCollection = new List<TColumn>();

        public class TColumn
        {

            public string viewName;
            public string expression = "";
            public string columnName;
            public string columnPrex;
            public string orignColumn;
            public Tuple<long, long> location;
            public IList<string> tableNames = new List<string>();
            public IList<string> tableFullNames = new List<string>();
            public ClauseType clauseType;
            public string alias;

            internal TColumn(ColumnImpact impact)
            {
                if (impact.CollectColumnInfo)
                {
                    impact.columnCollection.Add(this);
                }
            }

            public virtual string getFullName(string tableName)
            {
                if (!string.ReferenceEquals(tableName, null))
                {
                    return tableName + "." + columnName;
                }
                else
                {
                    return columnName;
                }
            }

            public virtual string OrigName
            {
                get
                {
                    if (!string.ReferenceEquals(columnPrex, null))
                    {
                        return columnPrex + "." + columnName;
                    }
                    else
                    {
                        return columnName;
                    }
                }
            }

        }

        internal class TResultEntry
        {
            private readonly ColumnImpact outerInstance;


            public ClauseType clause;

            public string targetColumn;
            public TTable targetTable;
            public Tuple<long, long> location;
            public TColumn columnObject;

            public TResultEntry(ColumnImpact outerInstance, TTable table, string viewName, string column, ClauseType clause, Tuple<long, long> location)
            {
                this.outerInstance = outerInstance;
                this.targetTable = table;
                this.targetColumn = column;
                this.clause = clause;
                this.location = location;
                columnObject = new TColumn(outerInstance);
                columnObject.columnName = "*";
                columnObject.viewName = viewName;
                updateColumnTableFullName(table, columnObject);
            }

            public TResultEntry(ColumnImpact outerInstance, TTable table, TColumn columnObject, string column, ClauseType clause, Tuple<long, long> location)
            {
                this.outerInstance = outerInstance;
                this.targetTable = table;
                this.targetColumn = column;
                this.clause = clause;
                this.location = location;
                this.columnObject = columnObject;
                updateColumnTableFullName(table, this.columnObject);
            }

            internal virtual void updateColumnTableFullName(TTable table, TColumn column)
            {
                IList<string> fullNames = column.tableFullNames;
                if (fullNames != null)
                {
                    for (int i = 0; i < fullNames.Count; i++)
                    {
                        string tableName = table.Name;
                        string fullName = fullNames[i];
                        if (!string.ReferenceEquals(tableName, null))
                        {
                            fullName = string.ReferenceEquals(fullName, null) ? "" : fullName.Trim();
                            if (!tableName.Equals(fullName, StringComparison.OrdinalIgnoreCase))
                            {
                                if (!fullNames.Contains(table.FullName))
                                {
                                    fullNames.RemoveAt(i);
                                    fullNames.Insert(i, table.FullName);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal class TSourceColumn
        {
            private readonly ColumnImpact outerInstance;

            public TSourceColumn(ColumnImpact outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public IList<ClauseType> clauses = new List<ClauseType>();
            public string name;
            public string tableName;
            public string tableOwner;
            public LinkedHashMap<ClauseType, IList<Tuple<long, long>>> locations = new LinkedHashMap<ClauseType, IList<Tuple<long, long>>>();
        }

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: ColumnImpact [/f <script file>] [/d]/[/s [/xml] [/c]]/[/v] [/o <output file path>] [/t <database type>]");
                Console.WriteLine("/s: Option, display the analysis result simply.");
                Console.WriteLine("/c: Option, display the analysis result simply in column level.");
                Console.WriteLine("/d: Option, display the analysis result in detail.");
                Console.WriteLine("/xml: Option, export the analysis results to XML format, it's valid only if /s is specified");
                Console.WriteLine("/v: Option, trace data lineage in views.");
                Console.WriteLine("/o: Option, write the output stream to the specified file.");
                Console.WriteLine("/t: Option, set the database type. Support oracle, mysql, mssql and db2, the default type is oracle");
                return;
            }

            string sqltext = @"SELECT e.last_name AS name,
                                e.commission_pct comm,
                                e.salary * 12 ""Annual Salary""
                                FROM scott.employees AS e
                                WHERE e.salary > 1000 or 1=1
                                ORDER BY
                                e.first_name,
                                e.last_name;";

            IList<string> argList = new List<string>(args);

            bool traceView = argList.IndexOf("/v") != -1;

            bool simply = traceView || argList.IndexOf("/s") != -1;

            bool isXML = !traceView && simply && argList.IndexOf("/xml") != -1;

            bool isColumnLevel = !traceView && simply && argList.IndexOf("/c") != -1;

            string outputFile = null;

            int index = argList.IndexOf("/o");

            if (index != -1 && args.Length > index + 1)
            {
                outputFile = args[index + 1];
            }

            System.IO.StreamWriter writer = null;
            if (!string.ReferenceEquals(outputFile, null))
            {
                try
                {
                    writer = new StreamWriter(outputFile);
                    Console.SetOut(writer);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }

            EDbVendor vendor = Common.GetEDbVendor(args);

            FileInfo file = null;

            index = argList.IndexOf("/f");
            if (index != -1 && args.Length > index + 1)
            {
                file = new FileInfo(args[index + 1]);
            }

            ColumnImpact impact = null;
            if (file != null)
            {
                impact = new ColumnImpact(file, vendor, simply, isXML, isColumnLevel, traceView, null);
            }
            else
            {
                impact = new ColumnImpact(sqltext, vendor, simply, isXML, isColumnLevel, traceView, null);
            }
            
            impact.CollectColumnInfo = false;
            impact.impactSQL();
            Console.Write(impact.ImpactResult);

            if (!simply)
            {
                Console.WriteLine("\r\nYou can add /s directive to display the analysis result in a simple format.");
            }

            try
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
        } // main

        internal virtual TColumn attrToColumn(TExpression lcexpr, TCustomSqlStatement stmt, TExpression expr, bool collectExpr, ClauseType clause, TAlias parentAlias)
        {
            TColumn column = attrToColumn(lcexpr, stmt, clause, parentAlias);
            if (column == null)
            {
                return null;
            }
            if (collectExpr)
            {
                column.expression = new Regex("\n+").Replace(expr.ToString().Replace("\r\n", "\n"), " ");
                if (column.expression.Trim().Length > 0)
                {
                    List<TParseTreeNode> tokens = expr.startToken.nodesStartFromThisToken;
                    if (tokens != null)
                    {
                        for (int i = 0; i < tokens.Count; i++)
                        {
                            TParseTreeNode node = tokens[i];
                            if (node is TResultColumn)
                            {
                                TResultColumn field = (TResultColumn)node;
                                if (field.AliasClause != null)
                                {
                                    column.alias = field.AliasClause.ToString();
                                }
                            }
                        }
                    }
                }
            }
            return column;
        }

        /* store the relations of alias to column */
        private IList<TAlias> aliases = new List<TAlias>();
        private StringBuilder buffer = new StringBuilder();
        private LinkedHashMap<string, TCustomSqlStatement> cteMap = new LinkedHashMap<string, TCustomSqlStatement>();
        private LinkedHashMap<string, LinkedHashMap<TCustomSqlStatement, bool>> accessMap = new LinkedHashMap<string, LinkedHashMap<TCustomSqlStatement, bool>>();
        private LinkedHashMap<TCustomSqlStatement, ClauseType> currentClauseMap = new LinkedHashMap<TCustomSqlStatement, ClauseType>();
        private string currentSource = null;
        /* store the dependency relations */
        private LinkedHashMap<string, IList<TResultEntry>> dependMap = new LinkedHashMap<string, IList<TResultEntry>>();
        private IMetaDatabase filter;
        private bool isXML = false;
        private bool isColumnLevel = false;
        private bool traceView = false;
        private bool simply = false;
        private int columnNumber = 0;
        private TCustomSqlStatement subquery = null;
        private string viewName;
        private bool collectColumnInfo = true;
        private TGSqlParser sqlparser;

        public virtual bool CollectColumnInfo
        {
            get
            {
                return collectColumnInfo;
            }
            set
            {
                this.collectColumnInfo = value;
            }
        }


        public ColumnImpact(FileInfo file, EDbVendor dbVendor, bool? simply, bool? isXML)
        {
            this.simply = simply.Value;
            this.isXML = isXML.Value;
            sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqlfilename = file.FullName;
        }

        public ColumnImpact(FileInfo file, EDbVendor dbVendor, bool? simply, bool? isXML, IMetaDatabase filter)
        {
            this.simply = simply.Value;
            this.isXML = isXML.Value;
            this.filter = filter;
            sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqlfilename = file.FullName;
        }

        public ColumnImpact(string sql, EDbVendor dbVendor, bool? simply, bool? isXML)
        {
            this.simply = simply.Value;
            this.isXML = isXML.Value;
            sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqltext = sql;
        }

        public ColumnImpact(string sql, EDbVendor dbVendor, bool? simply, bool? isXML, IMetaDatabase filter)
        {
            this.simply = simply.Value;
            this.isXML = isXML.Value;
            this.filter = filter;
            sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqltext = sql;
        }

        public ColumnImpact(FileInfo file, EDbVendor dbVendor, bool simply, bool isXML, bool isColumnLevel, IMetaDatabase filter)
        {
            this.simply = simply;
            this.isXML = isXML;
            this.isColumnLevel = isColumnLevel;
            this.filter = filter;
            sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqlfilename = file.FullName;
        }

        public ColumnImpact(FileInfo file, EDbVendor dbVendor, bool simply, bool isXML, bool isColumnLevel, bool traceView, IMetaDatabase filter)
        {
            if (traceView)
            {
                this.traceView = true;
                this.simply = true;
                this.isColumnLevel = true;
            }
            else
            {
                this.simply = simply;
                this.isXML = isXML;
                this.isColumnLevel = isColumnLevel;
            }
            this.filter = filter;
            sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqlfilename = file.FullName;
        }

        public ColumnImpact(string sql, EDbVendor dbVendor, bool? simply, bool? isXML, bool isColumnLevel, IMetaDatabase filter)
        {
            this.simply = simply.Value;
            this.isXML = isXML.Value;
            this.isColumnLevel = isColumnLevel;
            this.filter = filter;
            sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqltext = sql;
        }

        public ColumnImpact(string sql, EDbVendor dbVendor, bool simply, bool isXML, bool isColumnLevel, bool traceView, IMetaDatabase filter)
        {
            if (traceView)
            {
                this.traceView = true;
                this.simply = true;
                this.isColumnLevel = true;
            }
            else
            {
                this.simply = simply;
                this.isXML = isXML;
                this.isColumnLevel = isColumnLevel;
            }
            this.filter = filter;
            sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqltext = sql;
        }

        private TColumn attrToColumn(TExpression attr, TCustomSqlStatement stmt, ClauseType clauseType, TAlias parentAlias)
        {

            if (sqlparser.DbVendor == EDbVendor.dbvteradata)
            {
                if (clauseType == ClauseType.select && parentAlias != null)
                {
                    string columnName = removeQuote(attr.ObjectOperand.endToken.ToString());
                    TResultColumn resultColumn = getResultColumnByAlias(stmt, columnName);
                    if (resultColumn != null)
                    {
                        if (resultColumn.AliasClause != null && !parentAlias.alias.Equals(resultColumn.ColumnAlias, StringComparison.OrdinalIgnoreCase))
                        {
                            linkFieldToTables(parentAlias, resultColumn, stmt, 0);
                        }
                        return null;
                    }
                }
            }

            TColumn column = new TColumn(this);
            column.clauseType = clauseType;
            if (!string.ReferenceEquals(viewName, null))
            {
                column.viewName = viewName;
            }
            column.columnName = removeQuote(attr.ObjectOperand.endToken.ToString());
            column.location = new Tuple<long, long>(attr.ObjectOperand.endToken.lineNo, attr.endToken.columnNo);

            List<TParseTreeNode> tokens = attr.ObjectOperand.startToken.nodesStartFromThisToken;
            if (tokens != null)
            {
                for (int i = 0; i < tokens.Count; i++)
                {
                    TParseTreeNode node = tokens[i];
                    if (node is TResultColumn)
                    {
                        TResultColumn field = (TResultColumn)node;
                        if (field.AliasClause != null)
                        {
                            column.alias = field.AliasClause.ToString();
                        }
                    }
                }
            }

            if (attr.ToString().IndexOf(".", StringComparison.Ordinal) > 0)
            {
                column.columnPrex = removeQuote(attr.ToString().Substring(0, attr.ToString().LastIndexOf(".", StringComparison.Ordinal)));

                string tableName = removeQuote(column.columnPrex);
                if (tableName.IndexOf(".", StringComparison.Ordinal) > 0)
                {
                    tableName = removeQuote(tableName.Substring(tableName.LastIndexOf(".", StringComparison.Ordinal) + 1));
                }
                if (!column.tableNames.Contains(tableName))
                {
                    column.tableNames.Add(tableName);
                    if (!column.tableFullNames.Contains(tableName))
                    {
                        column.tableFullNames.Add(tableName);
                    }
                }
            }
            else
            {
                TTableList tables = stmt.tables;
                for (int i = 0; i < tables.size(); i++)
                {
                    TTable lztable = tables.getTable(i);
                    Table table = TLzTaleToTable(lztable);
                    if (!column.tableNames.Contains(table.tableName))
                    {
                        column.tableNames.Add(table.tableName);
                        if (!column.tableFullNames.Contains(lztable.FullName))
                        {
                            column.tableFullNames.Add(lztable.FullName);
                        }
                    }
                }
            }

            column.orignColumn = column.columnName;

            return column;
        }

        private TResultColumn getResultColumnByAlias(TCustomSqlStatement stmt, string columnName)
        {
            TResultColumnList columns = stmt.ResultColumnList;
            if (columns != null)
            {
                for (int i = 0; i < columns.size(); i++)
                {
                    TResultColumn column = columns.getResultColumn(i);
                    if (column.AliasClause != null && columnName.Equals(column.AliasClause.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return column;
                    }
                }
            }
            return null;
        }

        private string buildString(string @string, int level)
        {
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < level; i++)
            {
                buffer.Append(@string);
            }
            return buffer.ToString();
        }

        private TCustomSqlStatement containClasuse(LinkedHashMap<TCustomSqlStatement, ClauseType> currentClauseMap, TCustomSqlStatement select)
        {
            if (currentClauseMap.ContainsKey(select))
            {
                return select;
            }
            else if (select.ParentStmt is TCustomSqlStatement)
            {
                return containClasuse(currentClauseMap, (TCustomSqlStatement)select.ParentStmt);
            }
            else
            {
                return null;
            }
        }

        private IList<TColumn> exprToColumn(TExpression expr, TCustomSqlStatement stmt, int level, ClauseType clauseType)
        {
            IList<TColumn> columns = new List<TColumn>();

            columnsInExpr c = new columnsInExpr(this, this, expr, columns, stmt, level, false, clauseType, null);
            c.searchColumn();

            return columns;
        }

        private IList<TColumn> exprToColumn(TExpression expr, TCustomSqlStatement stmt, int level, ClauseType clauseType, TAlias parentAlias)
        {
            IList<TColumn> columns = new List<TColumn>();

            columnsInExpr c = new columnsInExpr(this, this, expr, columns, stmt, level, false, clauseType, parentAlias);
            c.searchColumn();

            return columns;
        }

        private IList<TColumn> exprToColumn(TExpression expr, TCustomSqlStatement stmt, int level, bool collectExpr, ClauseType clauseType, TAlias parentAlias)
        {
            IList<TColumn> columns = new List<TColumn>();

            columnsInExpr c = new columnsInExpr(this, this, expr, columns, stmt, level, collectExpr, clauseType, parentAlias);
            c.searchColumn();

            return columns;
        }

        private bool findColumnInSubQuery(TSelectSqlStatement select, string columnName, int level, Tuple<long, long> originLocation)
        {
            bool ret = false;
            if (accessMap.ContainsKey(columnName) && accessMap[columnName] != null && accessMap[columnName].ContainsKey(select))
            {
                return accessMap[columnName][select];
            }
            else
            {
                if (!accessMap.ContainsKey(columnName))
                {
                    accessMap[columnName] = new LinkedHashMap<TCustomSqlStatement, bool>();
                }
                accessMap[columnName][select] = false;
            }
            if (select.SetOperator != TSelectSqlStatement.setOperator_none)
            {
                bool left = findColumnInSubQuery(select.LeftStmt, columnName, level, originLocation);
                bool right = findColumnInSubQuery(select.RightStmt, columnName, level, originLocation);
                ret = left && right;
            }
            else if (select.ResultColumnList != null)
            {
                // check colum name in select list of subquery
                TResultColumn columnField = null;
                if (!"*".Equals(columnName))
                {
                    for (int i = 0; i < select.ResultColumnList.size(); i++)
                    {
                        TResultColumn field = select.ResultColumnList.getResultColumn(i);
                        if (field.AliasClause != null)
                        {
                            if (field.AliasClause.ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase))
                            {
                                columnField = field;
                                break;
                            }
                        }
                        else
                        {
                            if (field.Expr.ExpressionType == EExpressionType.simple_object_name_t)
                            {
                                TColumn column = attrToColumn(field.Expr, select, ClauseType.select, null);
                                if (!string.ReferenceEquals(columnName, null) && columnName.Equals(column.columnName, StringComparison.OrdinalIgnoreCase))
                                {
                                    columnField = field;
                                    break;
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < select.ResultColumnList.size(); i++)
                {
                    TResultColumn field = select.ResultColumnList.getResultColumn(i);
                    if (columnField != null && !field.Equals(columnField))
                    {
                        continue;
                    }
                    if (field.AliasClause != null)
                    {
                        ret = "*".Equals(columnName) || field.AliasClause.ToString().Equals(columnName, StringComparison.OrdinalIgnoreCase);
                        if (ret)
                        {
                            // let's check where this column come from?
                            if (!simply)
                            {
                                buffer.Append(buildString(" ", level) + "--> " + field.AliasClause.ToString() + "(alias)\r\n");
                            }
                            linkFieldToTables(null, field, select, level);
                        }
                    }
                    else
                    {
                        if (field.Expr.ExpressionType == EExpressionType.simple_object_name_t)
                        {
                            TColumn column = attrToColumn(field.Expr, select, ClauseType.select, null);
                            ret = "*".Equals(columnName) || (!string.ReferenceEquals(columnName, null) && columnName.Equals(column.columnName, StringComparison.OrdinalIgnoreCase));
                            if (ret || "*".Equals(column.columnName))
                            {
                                findColumnInTables(column, select, level, ret == false ? columnName : null, originLocation);
                                findColumnsFromClauses(select, level + 1);
                            }
                        }
                    }

                    if (ret && !"*".Equals(columnName))
                    {
                        break;
                    }
                }
            }

            LinkedHashMap<TCustomSqlStatement, bool> stmts = accessMap[columnName];
            if (stmts != null)
            {
                stmts[select] = ret;
            }

            return ret;
        } // findColumnInSubQuery

        private bool findColumnInTables(TColumn column, TCustomSqlStatement select, int level, string columnName, Tuple<long, long> originLocation)
        {
            bool ret = false;
            foreach (string tableName in column.tableNames)
            {
                if (!string.ReferenceEquals(columnName, null) && filter != null)
                {
                    int dotIndex = tableName.LastIndexOf(".", StringComparison.Ordinal);
                    string tableOwner = null;
                    string tableRealName = null;
                    if (dotIndex >= 0)
                    {
                        tableOwner = tableName.Substring(0, dotIndex);
                        tableRealName = tableName.Replace(tableOwner + ".", "");
                    }
                    else
                    {
                        tableRealName = tableName;
                    }
                    if (filter.checkColumn(null, null, tableOwner, tableRealName, columnName))
                    {
                        column.columnName = columnName;
                        if (originLocation != null)
                        {
                            column.location = originLocation;
                        }
                        // column.orignColumn = "*";
                        ret |= findColumnInTables(column, tableName, select, level);
                    }
                    else
                    {
                        ret |= false;
                    }
                }
                else
                {
                    ret |= findColumnInTables(column, tableName, select, level);
                }
            }
            return ret;
        }

        private bool findColumnInTables(TColumn column, string tableName, TCustomSqlStatement select, int level)
        {
            return findColumnInTables(column, tableName, select, level, ClauseType.undefine);
        }

        private bool findColumnInTables(TColumn column, string tableName, TCustomSqlStatement select, int level, ClauseType clause)
        {
            bool ret = false;
            TTableList tables = select.tables;

            if (tables.size() == 1)
            {
                TTable lzTable = tables.getTable(0);
                // buffer.AppendLine(lzTable.AsText);
                if ((lzTable.TableType == ETableSource.objectname) && (string.ReferenceEquals(tableName, null) || (!string.ReferenceEquals(tableName, null) && lzTable.AliasClause == null && getTableName(lzTable).Equals(tableName, StringComparison.OrdinalIgnoreCase)) || (!string.ReferenceEquals(tableName, null) && lzTable.AliasClause != null && lzTable.AliasClause.ToString().Equals(tableName, StringComparison.OrdinalIgnoreCase))))
                {
                    ret = true;

                    if (!simply)
                    {
                        buffer.Append(buildString(" ", level) + "--> " + getTableName(lzTable) + "." + column.columnName + "\r\n");
                    }
                    if (cteMap.ContainsKey(getTableName(lzTable)))
                    {
                        if (!simply)
                        {
                            buffer.Append(buildString(" ", level) + "--> WITH CTE\r\n");
                        }
                        ret = findColumnInSubQuery((TSelectSqlStatement)cteMap[getTableName(lzTable)], column.columnName, level, column.location);
                    }
                    else
                    {
                        if (!string.ReferenceEquals(currentSource, null) && dependMap.ContainsKey(currentSource))
                        {
                            TCustomSqlStatement stmt = containClasuse(currentClauseMap, select);
                            if (stmt != null)
                            {
                                dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, (ClauseType)currentClauseMap[stmt], column.location));
                            }
                            else if (select is TSelectSqlStatement)
                            {
                                if (ClauseType.undefine.Equals(clause))
                                {
                                    dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, ClauseType.select, column.location));
                                }
                                else
                                {
                                    dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, clause, column.location));
                                }
                            }
                            else
                            {
                                dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, ClauseType.undefine, column.location));
                            }
                        }
                    }
                }
                else if (select.ParentStmt is TSelectSqlStatement)
                {
                    subquery = select;
                    ret = findColumnInTables(column, tableName, select.ParentStmt, level, clause);
                    subquery = null;
                }
            }

            if (ret)
            {
                return ret;
            }

            for (int x = 0; x < tables.size(); x++)
            {
                TTable lzTable = tables.getTable(x);
                switch (lzTable.TableType)
                {
                    case ETableSource.objectname:
                        Table table = TLzTaleToTable(lzTable);
                        string alias = table.tableAlias;
                        if (!string.ReferenceEquals(alias, null))
                        {
                            alias = alias.Trim();
                        }
                        if ((!string.ReferenceEquals(tableName, null)) && ((tableName.Equals(alias, StringComparison.OrdinalIgnoreCase) || tableName.Equals(table.tableName, StringComparison.OrdinalIgnoreCase))))
                        {
                            if (!simply)
                            {
                                buffer.Append(buildString(" ", level) + "--> " + table.tableName + "." + column.columnName + "\r\n");
                            }
                            if (cteMap.ContainsKey(getTableName(lzTable)))
                            {
                                if (!simply)
                                {
                                    buffer.Append(buildString(" ", level) + "--> WITH CTE\r\n");
                                }
                                ret = findColumnInSubQuery((TSelectSqlStatement)cteMap[getTableName(lzTable)], column.columnName, level, column.location);
                            }
                            else
                            {
                                if (dependMap.ContainsKey(currentSource))
                                {
                                    string columnName = column.orignColumn;
                                    if ("*".Equals(columnName))
                                    {
                                        columnName = column.columnName;
                                    }
                                    if (currentClauseMap.ContainsKey(select))
                                    {
                                        dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, columnName, (ClauseType)currentClauseMap[select], column.location));
                                    }
                                    else if (select is TSelectSqlStatement)
                                    {
                                        if (ClauseType.undefine.Equals(clause))
                                        {
                                            dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, ClauseType.select, column.location));
                                        }
                                        else
                                        {
                                            dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, clause, column.location));
                                        }
                                    }
                                    else
                                    {
                                        dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, columnName, ClauseType.undefine, column.location));
                                    }
                                }
                                ret = true;
                            }
                        }
                        break;
                    case ETableSource.subquery:
                        for (int i = 0; i < column.tableNames.Count; i++)
                        {
                            string name = column.tableNames[i];
                            TSelectSqlStatement selectStat = (TSelectSqlStatement)lzTable.Subquery;

                            if (selectStat == subquery)
                            {
                                continue;
                            }

                            if (string.ReferenceEquals(name, null))
                            {
                                ret = findColumnInSubQuery(selectStat, column.columnName, level, column.location);
                                break;
                            }

                            if (lzTable.AliasClause != null && getTableAliasName(lzTable).Equals(name, StringComparison.OrdinalIgnoreCase))
                            {
                                ret = findColumnInSubQuery(selectStat, column.columnName, level, column.location);
                                break;
                            }

                            bool flag = false;
                            for (int j = 0; j < selectStat.tables.size(); j++)
                            {
                                if (selectStat.tables.getTable(j).AliasClause != null)
                                {
                                    if (getTableAliasName(selectStat.tables.getTable(j)).Equals(name, StringComparison.OrdinalIgnoreCase))
                                    {
                                        ret = findColumnInSubQuery(selectStat, column.columnName, level, column.location);
                                        flag = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (selectStat.tables.getTable(j).TableName.ToString().Equals(name, StringComparison.OrdinalIgnoreCase))
                                    {
                                        ret = findColumnInSubQuery(selectStat, column.columnName, level, column.location);
                                        flag = true;
                                        break;
                                    }
                                }
                            }
                            if (flag)
                            {
                                break;
                            }
                        }
                        break;
                    default:
                        break;
                }
                if (ret)
                {
                    break;
                }
            }

            if (!ret && select.ParentStmt is TSelectSqlStatement)
            {
                subquery = select;
                ret = findColumnInTables(column, tableName, select.ParentStmt, level, clause);
                subquery = null;
            }

            return ret;
        }

        private string getTableAliasName(TTable lztable)
        {
            return removeQuote(lztable.AliasClause.AliasName.ToString());
        }

        private string getTableName(TTable lzTable)
        {
            return removeQuote(lzTable.Name);
        }



        private void findColumnsFromClauses(TCustomSqlStatement select, int level)
        {
            currentClauseMap[select] = ClauseType.undefine;
            LinkedHashMap<TExpression, ClauseType> clauseTable = new LinkedHashMap<TExpression, ClauseType>();
            if (select is TSelectSqlStatement)
            {

                TSelectSqlStatement statement = (TSelectSqlStatement)select;

                if (statement.OrderbyClause != null)
                {
                    TOrderBy sortList = statement.OrderbyClause;
                    for (int i = 0; i < sortList.Items.size(); i++)
                    {
                        TOrderByItem orderBy = sortList.Items.getOrderByItem(i);
                        TExpression expr = orderBy.SortKey;
                        clauseTable[expr] = ClauseType.orderby;
                    }
                }

                if (statement.WhereClause != null)
                {
                    clauseTable[statement.WhereClause.Condition] = ClauseType.@where;
                }
                if (statement.HierarchicalClause != null && statement.HierarchicalClause.ConnectByList != null)
                {
                    for (int i = 0; i < statement.HierarchicalClause.ConnectByList.Count; i++)
                    {
                        clauseTable[statement.HierarchicalClause.ConnectByList[i].Condition] = ClauseType.connectby;
                    }
                }
                if (statement.HierarchicalClause != null && statement.HierarchicalClause.StartWithClause != null)
                {
                    clauseTable[statement.HierarchicalClause.StartWithClause] = ClauseType.startwith;
                }
                if (statement.joins != null)
                {
                    for (int i = 0; i < statement.joins.size(); i++)
                    {
                        TJoin join = statement.joins.getJoin(i);
                        if (join.JoinItems != null)
                        {
                            for (int j = 0; j < join.JoinItems.size(); j++)
                            {
                                TJoinItem joinItem = join.JoinItems.getJoinItem(j);
                                TExpression expr = joinItem.OnCondition;
                                if (expr != null)
                                {
                                    clauseTable[expr] = ClauseType.join;
                                }
                            }
                        }
                    }
                }
            }
            else if (select is TUpdateSqlStatement)
            {
                TUpdateSqlStatement statement = (TUpdateSqlStatement)select;
                if (statement.OrderByClause != null)
                {
                    TOrderByItemList sortList = statement.OrderByClause.Items;
                    for (int i = 0; i < sortList.size(); i++)
                    {
                        TOrderByItem orderBy = sortList.getOrderByItem(i);
                        TExpression expr = orderBy.SortKey;
                        clauseTable[expr] = ClauseType.orderby;
                    }
                }
                if (statement.WhereClause != null)
                {
                    clauseTable[statement.WhereClause.Condition] = ClauseType.@where;
                }

                if (statement.joins != null)
                {
                    for (int i = 0; i < statement.joins.size(); i++)
                    {
                        TJoin join = statement.joins.getJoin(i);
                        if (join.JoinItems != null)
                        {
                            for (int j = 0; j < join.JoinItems.size(); j++)
                            {
                                TJoinItem joinItem = join.JoinItems.getJoinItem(j);
                                TExpression expr = joinItem.OnCondition;
                                if (expr != null)
                                {
                                    clauseTable[expr] = ClauseType.join;
                                }
                            }
                        }
                    }
                }
            }

            foreach (TExpression expr in clauseTable.Keys)
            {
                currentClauseMap[select] = clauseTable[expr];

                if (!simply)
                {
                    switch ((ClauseType)currentClauseMap[select])
                    {
                        case ClauseType.where:
                            buffer.Append(buildString(" ", level) + "--> Where Clause\r\n");
                            break;
                        case ClauseType.connectby:
                            buffer.Append(buildString(" ", level) + "--> Connect By Clause\r\n");
                            break;
                        case ClauseType.startwith:
                            buffer.Append(buildString(" ", level) + "--> Start With Clause\r\n");
                            break;
                        case ClauseType.orderby:
                            buffer.Append(buildString(" ", level) + "--> Order By Clause\r\n");
                            break;
                        case ClauseType.join:
                            buffer.Append(buildString(" ", level) + "--> Join\r\n");
                            break;
                    }

                }

                IList<TColumn> columns = exprToColumn(expr, select, level, clauseTable[expr]);
                foreach (TColumn column1 in columns)
                {
                    foreach (string tableName in column1.tableNames)
                    {
                        if (!simply)
                        {

                            switch ((ClauseType)currentClauseMap[select])
                            {
                                case ClauseType.where:
                                    buffer.Append(buildString(" ", level + 1) + "--> " + column1.getFullName(tableName) + "(Where)\r\n");
                                    break;
                                case ClauseType.connectby:
                                    buffer.Append(buildString(" ", level + 1) + "--> " + column1.getFullName(tableName) + "(Connect By)\r\n");
                                    break;
                                case ClauseType.startwith:
                                    buffer.Append(buildString(" ", level + 1) + "--> " + column1.getFullName(tableName) + "(Start With)\r\n");
                                    break;
                                case ClauseType.orderby:
                                    buffer.Append(buildString(" ", level + 1) + "--> " + column1.getFullName(tableName) + "(Order By)\r\n");
                                    break;
                                case ClauseType.join:
                                    buffer.Append(buildString(" ", level + 1) + "--> " + column1.getFullName(tableName) + "(Join)\r\n");
                                    break;
                            }

                        }
                        findColumnInTables(column1, tableName, select, level + 2, column1.clauseType);
                    }

                }
            }
            currentClauseMap.Remove(select);

            // check order by clause
            findColumnsFromGroupBy(select, level);
        }

        private void findColumnsFromGroupBy(TCustomSqlStatement select, int level)
        {
            if (select is TSelectSqlStatement && ((TSelectSqlStatement)select).GroupByClause != null)
            {
                for (int j = 0; j < ((TSelectSqlStatement)select).GroupByClause.Items.size(); j++)
                {
                    TGroupByItem i = ((TSelectSqlStatement)select).GroupByClause.Items.getGroupByItem(j);

                    IList<TColumn> columns1;
                    try
                    {
                        if (i.Expr == null)
                        {
                            return;
                        }
                        int index = int.Parse(i.Expr.ToString());
                        columns1 = exprToColumn(select.ResultColumnList.getResultColumn(index - 1).Expr, select, level, ClauseType.groupby);
                    }
                    catch (System.FormatException)
                    {
                        columns1 = exprToColumn(i.Expr, select, level, ClauseType.groupby);
                    }

                    if (columns1.Count > 0)
                    {
                        TColumn column1 = columns1[0];
                        foreach (string tableName in column1.tableNames)
                        {
                            if (!simply)
                            {
                                buffer.Append(buildString(" ", level) + "--> " + column1.getFullName(tableName) + "(group by)\r\n");
                            }
                            findColumnInTables(column1, tableName, select, level + 1, ClauseType.groupby);
                        }
                    }
                }

            }
        }

        private void findColumnsFromList(TCustomSqlStatement select, int level, TParseTreeNodeList list, ClauseType clauseType)
        {
            if (list == null)
            {
                return;
            }

            for (int i = 0; i < list.size(); i++)
            {
                object element = list.getElement(i);
                TExpression lcexpr = null;
                if (element is TGroupByItem)
                {
                    lcexpr = ((TGroupByItem)element).Expr;
                }
                else if (element is TOrderByItem)
                {
                    lcexpr = ((TOrderByItem)element).SortKey;
                }
                else if (element is TExpression)
                {
                    lcexpr = (TExpression)element;
                }

                if (lcexpr != null)
                {
                    IList<TColumn> columns = exprToColumn(lcexpr, select, level, clauseType);
                    foreach (TColumn column1 in columns)
                    {
                        findColumnInTables(column1, select, level + 1, null, null);
                        findColumnsFromClauses(select, level + 2);
                    }
                }
            }
        }

        public virtual string ImpactResult
        {
            get
            {
                return buffer.ToString();
            }
        }

        public virtual IList<TColumn> ColumnInfos
        {
            get
            {
                return columnCollection;
            }
        }

        public virtual bool impactSQL()
        {
            int ret = sqlparser.parse();

            if (ret != 0)
            {
                buffer.Append(sqlparser.Errormessage + "\r\n");
                return false;
            }
            else
            {
                Document doc = null;
                Element columnImpactResult = null;
                if (simply && isXML)
                {
                    doc = new Document();
                    XDeclaration declaration = new XDeclaration("1.0", "utf-8", "no");
                    doc.Declaration = declaration;
                    columnImpactResult = new XElement("columnImpactResult");
                    doc.Add(columnImpactResult);
                }

                columnCollection.Clear();

                for (int k = 0; k < sqlparser.sqlstatements.size(); k++)
                {
                    if (sqlparser.sqlstatements.get(k) is TCustomSqlStatement)
                    {
                        dependMap.Clear();
                        aliases.Clear();
                        currentSource = null;
                        cteMap.Clear();
                        currentClauseMap.Clear();
                        accessMap.Clear();

                        TCustomSqlStatement select = (TCustomSqlStatement)sqlparser.sqlstatements.get(k);
                        initCTEMap(select);

                        columnNumber = 0;
                        impactSqlFromStatement(select);

                        if (traceView)
                        {
                            if (select is TCreateViewSqlStatement)
                            {
                                TSelectSqlStatement stmt = ((TCreateViewSqlStatement)select).Subquery;
                                if (stmt.WhereClause != null)
                                {
                                    buffer.Append("rt=vWhere\tview=" + viewName + "\twhere=").Append(new Regex("\n+").Replace(stmt.WhereClause.Condition.ToString().Replace("\r\n", "\n"), " ")).AppendLine();
                                }
                                IList<TTable> tableList = new List<TTable>();
                                checkStmtTables(stmt, tableList);
                                if (tableList.Count > 0)
                                {
                                    StringBuilder tableBuffer = new StringBuilder();
                                    IList<string> list = new List<string>();
                                    for (int i = 0; i < tableList.Count; i++)
                                    {
                                        IList<string> tables = new List<string>();
                                        getTableNames(tables, tableList[i]);
                                        if (tables != null)
                                        {
                                            for (int j = 0; j < tables.Count; j++)
                                            {
                                                bool exist = false;
                                                for (int z = 0; z < list.Count; z++)
                                                {
                                                    if (list[z].Equals(tables[j], StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        exist = true;
                                                        break;
                                                    }
                                                }
                                                if (!exist)
                                                {
                                                    list.Add(tables[j]);
                                                }
                                            }
                                        }
                                    }

                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        tableBuffer.Append(list[i]);
                                        if (i < list.Count - 1)
                                        {
                                            tableBuffer.Append(", ");
                                        }
                                    }
                                    buffer.Append("rt=vTable\tview=" + viewName + "\ttables=").Append(tableBuffer.ToString()).AppendLine();

                                }
                                if (stmt.joins != null)
                                {
                                    for (int i = 0; i < stmt.joins.size(); i++)
                                    {
                                        if (stmt.joins.getJoin(i).JoinItems.size() > 0)
                                        {
                                            buffer.Append("rt=vJoin\tview=" + viewName + "\tjoin=").Append(new Regex("\n+").Replace(stmt.joins.getJoin(i).JoinItems.ToString().Replace("\r\n", "\n"), " ")).AppendLine();
                                        }
                                    }
                                }
                            }

                            LinkedHashMap<string, string> bufferMap = new LinkedHashMap<string, string>();
                            LinkedHashMap<string, string> exprMap = new LinkedHashMap<string, string>();

                            TCreateViewSqlStatement createView = null;
                            if (select is TCreateViewSqlStatement)
                            {
                                createView = (TCreateViewSqlStatement)select;
                            }
                            foreach (TAlias alias in aliases)
                            {
                                if (dependMap.ContainsKey(alias.alias))
                                {
                                    IList<TResultEntry> results = (IList<TResultEntry>)dependMap[alias.alias];
                                    IList<string> nullRealColumns = new List<string>();
                                    foreach (TResultEntry result in results)
                                    {
                                        TColumn columnObject = result.columnObject;
                                        if (columnObject == null || string.ReferenceEquals(columnObject.viewName, null))
                                        {
                                            continue;
                                        }

                                        if (result.clause != ClauseType.select)
                                        {
                                            continue;
                                        }

                                        string column = null;

                                        if (!string.ReferenceEquals(result.columnObject.columnName, null))
                                        {
                                            if (result.targetTable.FullName == null)
                                            {
                                                continue;
                                            }
                                            if ("*".Equals(result.targetColumn))
                                            {
                                                column = removeQuote(result.targetTable.FullName.ToLower());
                                            }
                                            else
                                            {
                                                column = removeQuote((result.targetTable.FullName + "." + result.targetColumn).ToLower());
                                            }
                                        }
                                        else
                                        {
                                            if (nullRealColumns.Contains(removeQuote(result.columnObject.expression)))
                                            {
                                                continue;
                                            }
                                            else
                                            {
                                                nullRealColumns.Add(removeQuote(result.columnObject.expression));
                                            }

                                        }

                                        string columnAlias = null;
                                        if (createView != null && createView.ViewAliasClause != null)
                                        {
                                            columnAlias = createView.ViewAliasClause.ViewAliasItemList.getViewAliasItem(aliases.IndexOf(alias)).Alias.ToString();
                                        }
                                        else if (!alias.alias.Equals(alias.column))
                                        {
                                            columnAlias = alias.alias;
                                        }
                                        else
                                        {
                                            columnAlias = alias.column;
                                            if (alias.columnExpr != null)
                                            {
                                                if (alias.columnExpr.ExpressionType == EExpressionType.simple_object_name_t)
                                                {
                                                    if (columnAlias.IndexOf('.') != -1)
                                                    {
                                                        columnAlias = columnAlias.Substring(columnAlias.LastIndexOf('.') + 1);
                                                    }
                                                }
                                            }
                                        }

                                        string temp = ("rt=col\tview=" + columnObject.viewName + "\t" + "column=" + columnAlias + "\t" + (!string.ReferenceEquals(column, null) ? ("source=" + column + "\t") : "") + "expression=");

                                        if (!bufferMap.ContainsKey(temp.ToUpper()))
                                        {
                                            bufferMap[temp.ToUpper()] = temp;
                                        }
                                        if (!string.ReferenceEquals(columnObject.expression, null) && columnObject.expression.Trim().Length > 0)
                                        {
                                            if (!exprMap.ContainsKey(temp.ToUpper()))
                                            {
                                                exprMap[temp.ToUpper()] = columnObject.expression;
                                            }
                                            else
                                            {
                                                string expr = exprMap[temp.ToUpper()];
                                                Regex regex = new Regex(",\\s*" + Regex.Escape(columnObject.expression) + "\\s*,", RegexOptions.IgnoreCase);
                                                if (!regex.Match(("," + expr + ",")).Success)
                                                {
                                                    expr += (", " + columnObject.expression);
                                                }
                                                exprMap[temp.ToUpper()] = expr;
                                            }
                                        }
                                    }
                                }
                            }
                            IEnumerator<string> iter = bufferMap.Keys.GetEnumerator();
                            while (iter.MoveNext())
                            {
                                string temp = bufferMap[iter.Current];
                                buffer.Append(temp);
                                string expr = exprMap[temp.ToUpper()];
                                if (string.ReferenceEquals(expr, null))
                                {
                                    expr = "";
                                }
                                buffer.Append(expr + "\r\n");
                            }
                        }
                        else if (simply)
                        {
                            if (!isXML)
                            {
                                foreach (TAlias alias in aliases)
                                {

                                    buffer.Append(alias.alias + " depends on: ");

                                    IList<string> collections = new List<string>();

                                    if (dependMap.ContainsKey(alias.alias))
                                    {
                                        IList<TResultEntry> results = (IList<TResultEntry>)dependMap[alias.alias];
                                        foreach (TResultEntry result in results)
                                        {
                                            if (result.columnObject == null)
                                            {
                                                continue;
                                            }
                                            if (string.ReferenceEquals(result.columnObject.columnName, null))
                                            {
                                                continue;
                                            }

                                            string column = null;
                                            if (isColumnLevel && result.clause != ClauseType.select)
                                            {
                                                continue;
                                            }
                                            if (result.targetTable.FullName == null)
                                            {
                                                continue;
                                            }

                                            if ("*".Equals(result.targetColumn))
                                            {
                                                if (result.targetTable.FullName == null)
                                                {
                                                    continue;
                                                }
                                                column = removeQuote(result.targetTable.FullName.ToLower());
                                            }
                                            else
                                            {
                                                column = removeQuote((result.targetTable.FullName + "." + result.targetColumn).ToLower());
                                            }
                                            if (!collections.Contains(column))
                                            {
                                                collections.Add(column);
                                            }
                                        }
                                    }

                                    IList<string> list = new List<string>(collections);
                                    for (int i = 0; i < list.Count; i++)
                                    {
                                        if (i < collections.Count - 1)
                                        {
                                            buffer.Append(list[i] + ", ");
                                        }
                                        else
                                        {
                                            buffer.Append(list[i]);
                                        }
                                    }

                                    buffer.AppendLine();

                                }
                            }
                            else
                            {

                                foreach (TAlias alias in aliases)
                                {
                                    Element targetColumn = new Element("targetColumn");
                                    if (!alias.alias.Equals(alias.column))
                                    {
                                        targetColumn.Add(new XAttribute("alias", alias.alias));
                                    }
                                    targetColumn.Add(new XAttribute("coordinate", alias.location.X + "," + alias.location.Y));
                                    targetColumn.Add(new XAttribute("name", alias.column));

                                    columnImpactResult.Add(targetColumn);

                                    LinkedHashMap<string, TSourceColumn> collections = new LinkedHashMap<string, TSourceColumn>();

                                    if (dependMap.ContainsKey(alias.alias))
                                    {
                                        IList<TResultEntry> results = (IList<TResultEntry>)dependMap[alias.alias];
                                        foreach (TResultEntry result in results)
                                        {
                                            if (result.columnObject == null)
                                            {
                                                continue;
                                            }
                                            if (string.ReferenceEquals(result.columnObject.columnName, null))
                                            {
                                                continue;
                                            }

                                            if (isColumnLevel && result.clause != ClauseType.select)
                                            {
                                                continue;
                                            }
                                            if (result.targetTable.FullName == null)
                                            {
                                                continue;
                                            }

                                            string key = null;
                                            if ("*".Equals(result.targetColumn))
                                            {
                                                key = removeQuote(result.targetTable.FullName.ToLower());
                                            }
                                            else
                                            {
                                                key = removeQuote((result.targetTable.FullName.ToLower() + "." + result.targetColumn).ToLower());
                                            }

                                            TSourceColumn sourceColumn = null;
                                            if (collections.ContainsKey(key))
                                            {
                                                sourceColumn = (TSourceColumn)collections[key];
                                                if (!sourceColumn.clauses.Contains(result.clause))
                                                {
                                                    sourceColumn.clauses.Add(result.clause);
                                                }

                                                if (result.location != null)
                                                {
                                                    if (!sourceColumn.locations.ContainsKey(result.clause))
                                                    {
                                                        sourceColumn.locations[result.clause] = new List<Tuple<long, long>>();
                                                    }
                                                    IList<Tuple<long, long>> ys = sourceColumn.locations[result.clause];
                                                    if (!ys.Contains(result.location))
                                                    {
                                                        ys.Add(result.location);
                                                    }
                                                }

                                            }
                                            else
                                            {
                                                sourceColumn = new TSourceColumn(this);
                                                collections[key] = sourceColumn;
                                                sourceColumn.tableOwner = removeQuote(result.targetTable.TableName.SchemaString);
                                                sourceColumn.tableName = removeQuote(result.targetTable.Name);
                                                if (!"*".Equals(result.targetColumn))
                                                {
                                                    sourceColumn.name = result.targetColumn;
                                                }
                                                if (!sourceColumn.clauses.Contains(result.clause))
                                                {
                                                    sourceColumn.clauses.Add(result.clause);
                                                }
                                                if (result.location != null)
                                                {
                                                    if (!sourceColumn.locations.ContainsKey(result.clause))
                                                    {
                                                        sourceColumn.locations[result.clause] = new List<Tuple<long, long>>();
                                                    }
                                                    IList<Tuple<long, long>> ys = sourceColumn.locations[result.clause];
                                                    if (!ys.Contains(result.location))
                                                    {
                                                        ys.Add(result.location);
                                                    }
                                                }
                                            }
                                        }

                                        IEnumerator<string> iter = collections.Keys.GetEnumerator();

                                        while (iter.MoveNext())
                                        {
                                            TSourceColumn sourceColumn = (TSourceColumn)collections[iter.Current];
                                            if (sourceColumn.clauses.Count > 0)
                                            {
                                                for (int j = 0; j < sourceColumn.clauses.Count; j++)
                                                {
                                                    ClauseType clause = sourceColumn.clauses[j];
                                                    Element element = new Element("sourceColumn");
                                                    {
                                                        StringBuilder buffer = new StringBuilder();
                                                        switch (clause)
                                                        {
                                                            case demos.columnImpact.ColumnImpact.ClauseType.@where:
                                                                buffer.Append("where");
                                                                break;
                                                            case demos.columnImpact.ColumnImpact.ClauseType.connectby:
                                                                buffer.Append("connect by");
                                                                break;
                                                            case demos.columnImpact.ColumnImpact.ClauseType.startwith:
                                                                buffer.Append("start with");
                                                                break;
                                                            case demos.columnImpact.ColumnImpact.ClauseType.orderby:
                                                                buffer.Append("order by");
                                                                break;
                                                            case demos.columnImpact.ColumnImpact.ClauseType.join:
                                                                buffer.Append("join");
                                                                break;
                                                            case demos.columnImpact.ColumnImpact.ClauseType.select:
                                                                buffer.Append("select");
                                                                break;
                                                            case demos.columnImpact.ColumnImpact.ClauseType.groupby:
                                                                buffer.Append("group by");
                                                                break;
                                                        }
                                                        if (buffer.ToString().Length != 0)
                                                        {
                                                            element.Add(new XAttribute("clause", buffer.ToString()));
                                                        }
                                                    }
                                                    {
                                                        StringBuilder buffer = new StringBuilder();
                                                        buildLocationString(sourceColumn, clause, buffer);
                                                        if (buffer.ToString().Length != 0)
                                                        {
                                                            element.Add(new XAttribute("coordinate", buffer.ToString()));
                                                        }
                                                    }
                                                    if (!string.ReferenceEquals(sourceColumn.name, null))
                                                    {
                                                        element.Add(new XAttribute("name", sourceColumn.name));
                                                    }
                                                    if (!string.ReferenceEquals(sourceColumn.tableName, null))
                                                    {
                                                        element.Add(new XAttribute("tableName", sourceColumn.tableName));
                                                    }
                                                    if (!string.ReferenceEquals(sourceColumn.tableOwner, null))
                                                    {
                                                        element.Add(new XAttribute("tableOwner", sourceColumn.tableOwner));
                                                    }
                                                    targetColumn.Add(element);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (doc != null)
                {
                    try
                    {
                        StringBuilder xmlBuffer = new StringBuilder();

                        using (StringWriter writer = new Utf8StringWriter(xmlBuffer))
                        {
                            doc.Save(writer, SaveOptions.None);
                        }

                        buffer.Append(xmlBuffer.ToString().Trim());
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }
                }
            }
            return true;
        }

        internal class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb) { }

            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        private void checkStmtTables(TSelectSqlStatement stmt, IList<TTable> tableList)
        {
            if (stmt.SetOperator != TSelectSqlStatement.setOperator_none)
            {
                checkStmtTables(stmt.LeftStmt, tableList);
                checkStmtTables(stmt.RightStmt, tableList);
            }
            else
            {
                if (stmt.tables != null)
                {
                    for (int i = 0; i < stmt.tables.size(); i++)
                    {
                        TTable table = stmt.tables.getTable(i);
                        if (!tableList.Contains(table))
                        {
                            tableList.Add(table);
                        }
                    }
                }
            }
        }

        private void initCTEMap(TCustomSqlStatement select)
        {
            if (select.Statements != null && select.Statements.size() > 0)
            {
                for (int i = 0; i < select.Statements.size(); i++)
                {
                    initCTEMap(select.Statements.get(i));
                }
            }
            if (select.CteList != null && select.CteList.size() > 0)
            {
                for (int i = 0; i < select.CteList.size(); i++)
                {
                    TCTE expression = select.CteList.getCTE(i);
                    cteMap[removeQuote(expression.TableName.ToString())] = expression.Subquery;
                }
            }
        }

        private void getTableNames(IList<string> tableNames, TTable table)
        {
            if (table.Subquery != null)
            {
                for (int i = 0; i < table.Subquery.tables.size(); i++)
                {
                    getTableNames(tableNames, table.Subquery.tables.getTable(i));
                }
            }
            else
            {
                tableNames.Add(removeQuote(table.FullName));
            }
        }

        private void buildLocationString(TSourceColumn sourceColumn, ClauseType clauseType, StringBuilder locationBuffer)
        {
            IList<Tuple<long, long>> ys = sourceColumn.locations[clauseType];
            if (ys != null)
            {
                for (int z = 0; z < ys.Count; z++)
                {
                    locationBuffer.Append(ys[z].Item1 + "," + ys[z].Item2);
                    if (z < ys.Count - 1)
                    {
                        locationBuffer.Append(";");
                    }
                }
            }
        }

        private void impactSqlFromStatement(TCustomSqlStatement select, int baseLevel)
        {
            if (select is TSelectSqlStatement)
            {
                TSelectSqlStatement stmt = (TSelectSqlStatement)select;
                if (stmt.SetOperator != TSelectSqlStatement.setOperator_none)
                {
                    impactSqlFromStatement(stmt.LeftStmt, baseLevel);
                    impactSqlFromStatement(stmt.RightStmt, baseLevel);
                }
                else
                {
                    for (int i = 0; i < select.ResultColumnList.size(); i++)
                    {
                        linkFieldToTables(null, select.ResultColumnList.getResultColumn(i), select, baseLevel);
                    }
                }
            }
            else if (select is TInsertSqlStatement && ((TInsertSqlStatement)select).SubQuery != null)
            {
                impactSqlFromStatement(((TInsertSqlStatement)select).SubQuery, baseLevel);
            }
            else if (select is TCreateViewSqlStatement)
            {
                viewName = ((TCreateViewSqlStatement)select).ViewName.ToString();
                impactSqlFromStatement(((TCreateViewSqlStatement)select).Subquery, baseLevel);
            }
            else
            {
                if (select.ResultColumnList != null)
                {
                    for (int i = 0; i < select.ResultColumnList.size(); i++)
                    {
                        linkFieldToTables(null, select.ResultColumnList.getResultColumn(i), select, baseLevel);
                    }
                }
            }
        }

        private void impactSqlFromStatement(TCustomSqlStatement select)
        {
            if (select is TSelectSqlStatement)
            {
                TSelectSqlStatement stmt = (TSelectSqlStatement)select;
                if (stmt.SetOperator != TSelectSqlStatement.setOperator_none)
                {
                    impactSqlFromStatement(stmt.LeftStmt);
                    impactSqlFromStatement(stmt.RightStmt);
                }
                else
                {
                    for (int i = 0; i < select.ResultColumnList.size(); i++)
                    {
                        linkFieldToTables(null, select.ResultColumnList.getResultColumn(i), select, 0);
                    }
                }
            }
            else if (select is TInsertSqlStatement && ((TInsertSqlStatement)select).SubQuery != null)
            {
                impactSqlFromStatement(((TInsertSqlStatement)select).SubQuery);
            }
            else if (select is TCreateViewSqlStatement)
            {
                viewName = ((TCreateViewSqlStatement)select).ViewName.ToString();
                impactSqlFromStatement(((TCreateViewSqlStatement)select).Subquery);
            }
            else if (select.ResultColumnList != null)
            {
                for (int i = 0; i < select.ResultColumnList.size(); i++)
                {
                    linkFieldToTables(null, select.ResultColumnList.getResultColumn(i), select, 0);
                }
            }
            else if (select.Statements != null)
            {
                for (int i = 0; i < select.Statements.size(); i++)
                {
                    impactSqlFromStatement(select.Statements.get(i));
                }
            }
        }

        private bool isPseudocolumn(string column)
        {
            if (string.ReferenceEquals(column, null))
            {
                return false;
            }
            if ("rownum".Equals(column.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if ("rowid".Equals(column.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if ("nextval".Equals(column.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else if ("sysdate".Equals(column.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        private bool linkFieldToTables(TAlias parentAlias, TResultColumn field, TCustomSqlStatement select, int level)
        {
            if (level == 0)
            {
                accessMap.Clear();
            }
            bool ret = false;
            // all items in select list was represented by a TLzField Objects
            switch (field.Expr.ExpressionType)
            {
                case EExpressionType.simple_object_name_t:
                    TColumn column = attrToColumn(field.Expr, select, ClauseType.select, parentAlias);
                    bool isPseudocolumn = select.dbvendor == EDbVendor.dbvoracle && this.isPseudocolumn(column.columnName);
                    if (level == 0 || parentAlias != null)
                    {
                        TAlias columnAlias = null;
                        if (parentAlias != null)
                        {
                            columnAlias = parentAlias;
                        }
                        else
                        {
                            columnAlias = new TAlias(this);
                            columnAlias.column = removeQuote(field.ToString());
                            columnAlias.columnExpr = field.Expr;
                            columnAlias.alias = removeQuote(field.ToString());
                            columnAlias.location = new Point((int)field.startToken.lineNo, (int)field.startToken.columnNo);
                            if (field.AliasClause != null)
                            {
                                columnAlias.alias = removeQuote(field.AliasClause.ToString());
                                columnAlias.column = removeQuote(field.ToString());
                                columnAlias.columnExpr = field.Expr;
                                TSourceToken startToken = field.AliasClause.AliasName.startToken;
                                columnAlias.location = new Point((int)startToken.lineNo, (int)startToken.columnNo);
                            }
                            aliases.Add(columnAlias);
                        }
                        currentSource = columnAlias.alias;
                        if (!dependMap.ContainsKey(currentSource))
                        {
                            dependMap[currentSource] = new List<TResultEntry>();
                        }

                        if (!simply && parentAlias == null)
                        {
                            if (!columnAlias.alias.Equals(column.OrigName, StringComparison.OrdinalIgnoreCase))
                            {
                                buffer.Append("\r\nSearch " + columnAlias.alias + (level == 0 ? (" <<column_" + (++columnNumber) + ">>") : "") + "\r\n");
                                buffer.Append("--> " + column.OrigName + (!isPseudocolumn && column.tableNames.Count > 1 ? (" <<GUESS>>") : "") + "\r\n");
                            }
                            else
                            {
                                buffer.Append("\r\nSearch " + column.OrigName + (level == 0 ? (" <<column_" + (++columnNumber) + ">>") : "") + (!isPseudocolumn && column.tableNames.Count > 1 ? (" <<GUESS>>") : "") + "\r\n");
                                level -= 1;
                            }
                        }

                    }
                    if (isPseudocolumn)
                    {
                        break;
                    }
                    ret = findColumnInTables(column, select, level + 1, null, null);
                    findColumnsFromClauses(select, level + 2);
                    break;
                case EExpressionType.subquery_t:
                    TAlias alias1 = new TAlias(this);
                    alias1.column = removeQuote(field.ToString());
                    alias1.columnExpr = field.Expr;
                    alias1.alias = removeQuote(field.ToString());
                    alias1.location = new Point((int)field.startToken.lineNo, (int)field.startToken.columnNo);
                    if (field.AliasClause != null)
                    {
                        alias1.alias = removeQuote(field.AliasClause.ToString());
                        TSourceToken startToken = field.AliasClause.AliasName.startToken;
                        alias1.column = removeQuote(field.ToString());
                        alias1.columnExpr = field.Expr;
                        alias1.location = new Point((int)startToken.lineNo, (int)startToken.columnNo);
                    }

                    if (level == 0)
                    {
                        aliases.Add(alias1);
                        if (!simply)
                        {
                            buffer.Append("\r\nSearch " + alias1.alias + (level == 0 ? (" <<column_" + (++columnNumber) + ">>") : "") + "\r\n");
                            // buffer.append( "--> "
                            // + field.getExpr( ).getSubQuery( )
                            // + "\r\n" );
                        }
                    }
                    TSelectSqlStatement stmt = (TSelectSqlStatement)field.Expr.SubQuery;
                    IList<TSelectSqlStatement> stmtList = new List<TSelectSqlStatement>();
                    getSelectSqlStatements(stmt, stmtList);
                    for (int i = 0; i < stmtList.Count; i++)
                    {
                        linkFieldToTables(alias1, stmtList[i].ResultColumnList.getResultColumn(0), stmtList[i], level - 1 < 0 ? 0 : level - 1);
                    }
                    break;
                default:
                    TAlias alias = parentAlias;
                    if (level == 0)
                    {
                        alias = new TAlias(this);

                        if (select is TUpdateSqlStatement)
                        {
                            TExpression expression = field.Expr.LeftOperand;
                            alias.column = removeQuote(expression.ToString());
                            alias.columnExpr = expression;
                            alias.alias = alias.column;
                            alias.location = new Point((int)expression.startToken.lineNo, (int)expression.startToken.columnNo);
                        }
                        else
                        {
                            alias.column = removeQuote(field.ToString());
                            alias.columnExpr = field.Expr;
                            alias.alias = alias.column;
                            alias.location = new Point((int)field.startToken.lineNo, (int)field.startToken.columnNo);

                        }
                        if (alias != null && parentAlias == null)
                        {
                            if (field.AliasClause != null)
                            {
                                alias.alias = removeQuote(field.AliasClause.ToString());
                                alias.column = removeQuote(field.ToString());
                                alias.columnExpr = field.Expr;
                                TSourceToken startToken = field.AliasClause.AliasName.startToken;
                                alias.location = new Point((int)startToken.lineNo, (int)startToken.columnNo);
                            }
                            aliases.Add(alias);
                            if (!simply)
                            {
                                buffer.Append("\r\n" + "Search " + alias.alias + (level == 0 ? (" <<column_" + (++columnNumber) + ">>") : "") + "\r\n");
                            }

                            currentSource = alias.alias;
                            if (!dependMap.ContainsKey(currentSource))
                            {
                                dependMap[currentSource] = new List<TResultEntry>();
                            }
                        }
                    }

                    IList<TColumn> columns = exprToColumn(field.Expr, select, level, true, ClauseType.select, alias);
                    if (columns.Count == 0 && traceView)
                    {
                        TColumn nullColumn = new TColumn(this);
                        nullColumn.expression = field.Expr.ToString();
                        nullColumn.viewName = this.viewName;
                        TTableList tables = select.tables;
                        for (int i = 0; i < tables.size(); i++)
                        {
                            TTable lztable = tables.getTable(i);
                            Table table = TLzTaleToTable(lztable);
                            if (!nullColumn.tableNames.Contains(table.tableName))
                            {
                                nullColumn.tableNames.Add(table.tableName);
                                if (!nullColumn.tableFullNames.Contains(lztable.FullName))
                                {
                                    nullColumn.tableFullNames.Add(lztable.FullName);
                                }
                            }
                        }
                        columns.Add(nullColumn);
                    }
                    if (select is TUpdateSqlStatement)
                    {
                        while (columns.Count > 1)
                        {
                            columns.RemoveAt(columns.Count - 1);
                        }
                    }
                    if (!simply)
                    {
                        foreach (TColumn column1 in columns)
                        {
                            if (column1 == null)
                            {
                                continue;
                            }
                            if (level == 0)
                            {
                                buffer.Append(buildString(" ", level) + "--> " + column1.OrigName + "\r\n");
                            }
                        }
                    }

                    foreach (TColumn column1 in columns)
                    {
                        if (column1 == null)
                        {
                            continue;
                        }

                        if (level == 0)
                        {
                            if (!simply)
                            {
                                buffer.Append("\r\n" + "Search " + column1.OrigName + "\r\n");
                            }
                        }
                        if (!(select is TUpdateSqlStatement))
                        {
                            findColumnInTables(column1, select, level + 1, null, null);
                        }
                        findColumnsFromClauses(select, level + 2);
                    }

                    if (field.Expr.ExpressionType == EExpressionType.function_t)
                    {
                        TFunctionCall func = (TFunctionCall)field.Expr.FunctionCall;
                        // buffer.AppendLine("function name {0}",
                        // func.funcname.AsText);
                        if (func.FunctionName.ToString().Equals("count", StringComparison.OrdinalIgnoreCase) || func.FunctionName.ToString().Equals("sum", StringComparison.OrdinalIgnoreCase) || func.FunctionName.ToString().Equals("row_number", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!simply)
                            {
                                buffer.Append(buildString(" ", level + 1) + "--> aggregate function " + func.ToString() + "\r\n");
                                for (int i = 0; i < select.tables.size(); i++)
                                {
                                    if (select.tables.getTable(i).Subquery == null)
                                    {
                                        buffer.Append(buildString(" ", level + 1) + "--> table " + removeQuote(select.tables.getTable(i).FullNameWithAliasString) + "\r\n");
                                    }
                                    else
                                    {
                                        buffer.Append(buildString(" ", level + 1) + "--> table " + select.tables.getTable(i).ToString() + (select.tables.getTable(i).AliasClause != null ? (" " + select.tables.getTable(i).AliasClause.ToString()) : "") + "\r\n");
                                    }
                                }
                            }
                            // check column in function arguments
                            int argCount = 0;
                            if (func.Args != null)
                            {
                                for (int k = 0; k < func.Args.size(); k++)
                                {
                                    TExpression expr = func.Args.getExpression(k);
                                    if (expr.ToString().Trim().Equals("*"))
                                    {
                                        continue;
                                    }
                                    IList<TColumn> columns1 = exprToColumn(expr, select, level + 1, ClauseType.select, parentAlias);
                                    foreach (TColumn column1 in columns1)
                                    {
                                        findColumnInTables(column1, select, level + 1, null, null);
                                        findColumnsFromClauses(select, level + 2);
                                    }
                                    argCount++;
                                }
                            }

                            if (argCount == 0 && !"ROW_NUMBER".Equals(func.FunctionName.ToString(), StringComparison.OrdinalIgnoreCase))
                            {

                                Tuple<long, long> point = new Tuple<long, long>(func.endToken.lineNo, func.endToken.columnNo);
                                if (func.Args != null && func.Args.size() > 0)
                                {
                                    for (int k = 0; k < func.Args.size(); k++)
                                    {
                                        TExpression expr = func.Args.getExpression(k);
                                        if (expr.ToString().Trim().Equals("*"))
                                        {
                                            point = new Tuple<long, long>(expr.startToken.lineNo, expr.startToken.columnNo);
                                            break;
                                        }
                                    }
                                }
                                if (dependMap.ContainsKey(currentSource))
                                {

                                    if (currentClauseMap.ContainsKey(select))
                                    {
                                        dependMap[currentSource].Add(new TResultEntry(this, select.tables.getTable(0), viewName, "*", (ClauseType)currentClauseMap[select], point));
                                    }
                                    else if (select is TSelectSqlStatement)
                                    {
                                        dependMap[currentSource].Add(new TResultEntry(this, select.tables.getTable(0), viewName, "*", ClauseType.select, point));
                                    }
                                    else
                                    {
                                        dependMap[currentSource].Add(new TResultEntry(this, select.tables.getTable(0), viewName, "*", ClauseType.undefine, point));
                                    }
                                }
                            }

                            if (func.AnalyticFunction != null)
                            {
                                TParseTreeNodeList list = func.AnalyticFunction.PartitionBy_ExprList;
                                findColumnsFromList(select, level + 1, list, ClauseType.select);

                                if (func.AnalyticFunction.OrderBy != null)
                                {
                                    list = func.AnalyticFunction.OrderBy.Items;
                                    findColumnsFromList(select, level + 1, list, ClauseType.select);
                                }
                            }

                            findColumnsFromClauses(select, level + 2);

                        }
                    }
                    break;
            }

            return ret;
        }

        private void getSelectSqlStatements(TSelectSqlStatement select, IList<TSelectSqlStatement> stmtList)
        {
            if (select.SetOperator != TSelectSqlStatement.setOperator_none)
            {
                getSelectSqlStatements(select.LeftStmt, stmtList);
                getSelectSqlStatements(select.RightStmt, stmtList);
            }
            else
            {
                stmtList.Add(select);
            }
        }

        private Table TLzTaleToTable(TTable lztable)
        {
            Table table = new Table(this);
            if (lztable.Subquery == null && lztable.TableName != null)
            {
                table.tableName = removeQuote(getTableName(lztable));
                if (lztable.TableName.ToString().IndexOf(".", StringComparison.Ordinal) > 0)
                {
                    table.prefixName = removeQuote(lztable.TableName.ToString().Substring(0, lztable.FullName.IndexOf('.')));
                }
            }

            if (lztable.AliasClause != null)
            {
                table.tableAlias = removeQuote(lztable.AliasClause.ToString());
            }
            return table;
        }

        private string removeQuote(string @string)
        {
            if (string.ReferenceEquals(@string, null))
            {
                return @string;
            }

            if (@string.IndexOf('.') != -1
                && @string.Length < 128)
            {
                IList<string> splits = parseNames(@string);
                StringBuilder buffer = new StringBuilder();
                for (int i = 0; i < splits.Count; i++)
                {
                    buffer.Append(splits[i]);
                    if (i < splits.Count - 1)
                    {
                        buffer.Append(".");
                    }
                }
                @string = buffer.ToString();
            }
            else
            {
                if (@string.StartsWith("\"", StringComparison.Ordinal) && @string.EndsWith("\"", StringComparison.Ordinal))
                {
                    return @string.Substring(1, (@string.Length - 1) - 1);
                }

                if (@string.StartsWith("[", StringComparison.Ordinal) && @string.EndsWith("]", StringComparison.Ordinal))
                {
                    return @string.Substring(1, (@string.Length - 1) - 1);
                }
            }
            return @string;
        }

        public static IList<string> parseNames(string nameString)
        {
            String name = nameString.Trim();
            List<String> names = new List<String>();
            string[] splits = nameString.ToUpper().Split(new char[] { '.' });
            if ((name.StartsWith("\"") && name.EndsWith("\""))
                    || (name.StartsWith("[") && name.EndsWith("]")))
            {
                for (int i = 0; i < splits.Length; i++)
                {
                    string split = splits[i].Trim();
                    if (split.StartsWith("[", StringComparison.Ordinal) && !split.EndsWith("]", StringComparison.Ordinal))
                    {
                        StringBuilder buffer = new StringBuilder();
                        buffer.Append(splits[i]);
                        while (!(split = splits[++i].Trim()).EndsWith("]", StringComparison.Ordinal))
                        {
                            buffer.Append(".");
                            buffer.Append(splits[i]);
                        }

                        buffer.Append(".");
                        buffer.Append(splits[i]);

                        names.Add(buffer.ToString());
                        continue;
                    }
                    if (split.StartsWith("\"", StringComparison.Ordinal) && !split.EndsWith("\"", StringComparison.Ordinal))
                    {
                        StringBuilder buffer = new StringBuilder();
                        buffer.Append(splits[i]);
                        while (!(split = splits[++i].Trim()).EndsWith("\"", StringComparison.Ordinal))
                        {
                            buffer.Append(".");
                            buffer.Append(splits[i]);
                        }

                        buffer.Append(".");
                        buffer.Append(splits[i]);

                        names.Add(buffer.ToString());
                        continue;
                    }
                    names.Add(splits[i]);
                }
            }
            else
            {
                names.AddRange(splits);
            }
            return names;
        }
    }

}