using System;
using System.Collections.Generic;
using System.Text;

namespace gudusoft.gsqlparser.demos.dlineage.columnImpact
{

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using EExpressionType = gudusoft.gsqlparser.EExpressionType;
    using ESetOperatorType = gudusoft.gsqlparser.ESetOperatorType;
    using ETableSource = gudusoft.gsqlparser.ETableSource;
    using IMetaDatabase = gudusoft.gsqlparser.IMetaDatabase;
    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using TSourceToken = gudusoft.gsqlparser.TSourceToken;
    using IExpressionVisitor = gudusoft.gsqlparser.nodes.IExpressionVisitor;
    using TCTE = gudusoft.gsqlparser.nodes.TCTE;
    using TCaseExpression = gudusoft.gsqlparser.nodes.TCaseExpression;
    using TColumnDefinition = gudusoft.gsqlparser.nodes.TColumnDefinition;
    using TExpression = gudusoft.gsqlparser.nodes.TExpression;
    using TExpressionList = gudusoft.gsqlparser.nodes.TExpressionList;
    using TFunctionCall = gudusoft.gsqlparser.nodes.TFunctionCall;
    using TGroupByItem = gudusoft.gsqlparser.nodes.TGroupByItem;
    using TGroupingExpressionItem = gudusoft.gsqlparser.nodes.TGroupingExpressionItem;
    using TInExpr = gudusoft.gsqlparser.nodes.TInExpr;
    using TJoin = gudusoft.gsqlparser.nodes.TJoin;
    using TJoinItem = gudusoft.gsqlparser.nodes.TJoinItem;
    using TMergeWhenClause = gudusoft.gsqlparser.nodes.TMergeWhenClause;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using TOrderBy = gudusoft.gsqlparser.nodes.TOrderBy;
    using TOrderByItem = gudusoft.gsqlparser.nodes.TOrderByItem;
    using TOrderByItemList = gudusoft.gsqlparser.nodes.TOrderByItemList;
    using TParseTreeNode = gudusoft.gsqlparser.nodes.TParseTreeNode;
    using TResultColumn = gudusoft.gsqlparser.nodes.TResultColumn;
    using TResultColumnList = gudusoft.gsqlparser.nodes.TResultColumnList;
    using TTable = gudusoft.gsqlparser.nodes.TTable;
    using TTableList = gudusoft.gsqlparser.nodes.TTableList;
    using TTrimArgument = gudusoft.gsqlparser.nodes.TTrimArgument;
    using TViewAliasItem = gudusoft.gsqlparser.nodes.TViewAliasItem;
    using TWhenClauseItem = gudusoft.gsqlparser.nodes.TWhenClauseItem;
    using TWhenClauseItemList = gudusoft.gsqlparser.nodes.TWhenClauseItemList;
    using TCreateTableSqlStatement = gudusoft.gsqlparser.stmt.TCreateTableSqlStatement;
    using TCreateViewSqlStatement = gudusoft.gsqlparser.stmt.TCreateViewSqlStatement;
    using TInsertSqlStatement = gudusoft.gsqlparser.stmt.TInsertSqlStatement;
    using TMergeSqlStatement = gudusoft.gsqlparser.stmt.TMergeSqlStatement;
    using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;
    using TUpdateSqlStatement = gudusoft.gsqlparser.stmt.TUpdateSqlStatement;



    using Document = System.Xml.Linq.XDocument;
    using Element = System.Xml.Linq.XElement;

    using DDLParser = gudusoft.gsqlparser.demos.dlineage.metadata.DDLParser;
    using MetaDB = gudusoft.gsqlparser.demos.dlineage.metadata.MetaDB;
    using ColumnMetaData = gudusoft.gsqlparser.demos.dlineage.model.metadata.ColumnMetaData;
    using TableMetaData = gudusoft.gsqlparser.demos.dlineage.model.metadata.TableMetaData;
    using AliasModel = gudusoft.gsqlparser.demos.dlineage.model.view.AliasModel;
    using Clause = gudusoft.gsqlparser.demos.dlineage.model.view.Clause;
    using ColumnImpactModel = gudusoft.gsqlparser.demos.dlineage.model.view.ColumnImpactModel;
    using ColumnModel = gudusoft.gsqlparser.demos.dlineage.model.view.ColumnModel;
    using FieldModel = gudusoft.gsqlparser.demos.dlineage.model.view.FieldModel;
    using ReferenceModel = gudusoft.gsqlparser.demos.dlineage.model.view.ReferenceModel;
    using TableModel = gudusoft.gsqlparser.demos.dlineage.model.view.TableModel;
    using XML2Model = gudusoft.gsqlparser.demos.dlineage.model.xml.XML2Model;
    using columnImpactResult = gudusoft.gsqlparser.demos.dlineage.model.xml.columnImpactResult;
    using sourceColumn = gudusoft.gsqlparser.demos.dlineage.model.xml.sourceColumn;
    using table = gudusoft.gsqlparser.demos.dlineage.model.xml.table;
    using targetColumn = gudusoft.gsqlparser.demos.dlineage.model.xml.targetColumn;
    using SQLUtil = gudusoft.gsqlparser.demos.dlineage.util.SQLUtil;
    using demos.util;
    using gudusoft.gsqlparser.nodes;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.IO;
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.stmt;

    //import gudusoft.sqldrill.columnImpact.model.AliasModel;

    public class ColumnImpact
    {
        public enum ClauseType
        {
            undefine,
            connectby,
            groupby,
            join,
            orderby,
            select,
            startwith,
            @where,
            topselect,
            createview,
            createtable,
            insert,
            casewhen,
            casethen,
            update,
            updateset,
            assign,
            mergematch,
            mergenotmatch,
            mergeset,
            merge
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

            internal virtual void AddColumnToList(TParseTreeNodeList list, TCustomSqlStatement stmt)
            {
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        TParseTreeNode element = list.getElement(i);

                        columnsInExpr visitor = new columnsInExpr(outerInstance, this.impact, this.expr, this.columns, this.stmt, this.level, this.collectExpr, this.clauseType, parentAlias);

                        if (element is TGroupByItem)
                        {
                            visitor.clauseType = ClauseType.groupby;
                            (((TGroupByItem)element).Expr).inOrderTraverse(visitor);
                        }
                        if (element is TOrderByItem)
                        {
                            visitor.clauseType = ClauseType.orderby;
                            (((TOrderByItem)element).SortKey).inOrderTraverse(visitor);
                        }
                        else if (element is TWhenClauseItem)
                        {
                            if (visitor.clauseType == ClauseType.select)
                            {
                                (((TWhenClauseItem)element).Return_expr).inOrderTraverse(visitor);
                            }
                            else
                            {
                                visitor.clauseType = ClauseType.casethen;
                                (((TWhenClauseItem)element).Return_expr).inOrderTraverse(visitor);
                            }
                            if (!impact.analyzeDlineage)
                            {
                                visitor.clauseType = ClauseType.casewhen;
                                (((TWhenClauseItem)element).Comparison_expr).inOrderTraverse(visitor);
                            }
                        }
                        else if (element is TExpression)
                        {
                            ((TExpression)element).inOrderTraverse(visitor);
                        }
                    }
                }
            }

            public virtual bool exprVisit(TParseTreeNode pNode, bool isLeafNode)
            {
                TExpression lcexpr = (TExpression)pNode;
                if (lcexpr.ExpressionType == EExpressionType.simple_constant_t)
                {
                    TColumn tempColumn = new TColumn();
                    tempColumn.columnName = lcexpr.ToString();
                    tempColumn.location = new Tuple<long, long>(lcexpr.getEndToken().lineNo,
                            lcexpr.getEndToken().columnNo);
                    tempColumn.offset = lcexpr.getEndToken().offset;
                    tempColumn.length = lcexpr.getEndToken().astext.Length;
                    tempColumn.tableNames.Add(SQLUtil.TABLE_CONSTANT);
                    tempColumn.tableFullNames.Add(SQLUtil.TABLE_CONSTANT
                            + "."
                            + tempColumn.columnName);
                    tempColumn.clauseType = clauseType;
                    columns.Add(tempColumn);
                }
                else if (lcexpr.ExpressionType == EExpressionType.simple_object_name_t)
                {
                    TColumn column;
                    if (!outerInstance.accessExpressions.ContainsKey(lcexpr.ToString() + lcexpr.startToken.posinlist + lcexpr.endToken.posinlist))
                    {
                        column = impact.attrToColumn(lcexpr,
                                stmt,
                                expr,
                                collectExpr,
                                clauseType,
                                parentAlias);
                        outerInstance.accessExpressions[lcexpr.ToString()+ lcexpr.startToken.posinlist+lcexpr.endToken.posinlist] = column;
                    }
                    else
                    {
                        column = outerInstance.accessExpressions[lcexpr.ToString() + lcexpr.startToken.posinlist + lcexpr.endToken.posinlist];
                    }
                    if (column != null)
                    {
                        columns.Add(column);
                    }
                }
                else if (lcexpr.ExpressionType == EExpressionType.multiset_t)
                {
                    if (lcexpr.SubQuery != null)
                    {
                        for (int i = 0; i < lcexpr.SubQuery.ResultColumnList.Count; i++)
                        {
                            impact.linkFieldToTables(parentAlias, lcexpr.SubQuery.ResultColumnList.getResultColumn(i), lcexpr.SubQuery, level + 1, ClauseType.select);
                        }
                    }
                }
                else if (lcexpr.ExpressionType == EExpressionType.between_t)
                {
                    lcexpr.BetweenOperand.inOrderTraverse(this);
                }
                else if (lcexpr.ExpressionType == EExpressionType.function_t)
                {
                    TFunctionCall func = lcexpr.FunctionCall;
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
                    if (func.InExpr != null)
                    {
                        TInExpr inExpr = func.InExpr;
                        if (inExpr.ExprList != null)
                        {
                            for (int k = 0; k < inExpr.ExprList.Count; k++)
                            {
                                inExpr.ExprList.getExpression(k).inOrderTraverse(this);
                            }
                        }
                        if (inExpr.Func_expr != null)
                        {
                            inExpr.Func_expr.inOrderTraverse(this);
                        }
                        if (inExpr.GroupingExpressionItemList != null)
                        {
                            for (int k = 0; k < inExpr.GroupingExpressionItemList.Count; k++)
                            {
                                TGroupingExpressionItem item = inExpr.GroupingExpressionItemList.getGroupingExpressionItem(k);
                                if (item.Expr != null)
                                {
                                    item.Expr.inOrderTraverse(this);
                                }
                                if (item.ExprList != null)
                                {
                                    item.ExprList.getExpression(k).inOrderTraverse(this);
                                }
                            }
                        }
                    }
                    if (func.RangeSize != null)
                    {
                        func.RangeSize.inOrderTraverse(this);
                    }
                    if (func.XMLElementNameExpr != null)
                    {
                        func.XMLElementNameExpr.inOrderTraverse(this);
                    }
                    if (func.XMLType_Instance != null)
                    {
                        func.XMLType_Instance.inOrderTraverse(this);
                    }
                    if (func.XPath_String != null)
                    {
                        func.XPath_String.inOrderTraverse(this);
                    }
                    if (func.Namespace_String != null)
                    {
                        func.Namespace_String.inOrderTraverse(this);
                    }
                    if (func.Args != null)
                    {
                        for (int k = 1; k < func.Args.Count; k++)
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
                    if (func.OrderByList != null)
                    {
                        TOrderByItemList orderByList = func.OrderByList;
                        for (int k = 0; k < orderByList.Count; k++)
                        {
                            TExpression expr = orderByList.getOrderByItem(k).SortKey;
                            if (expr != null)
                            {
                                expr.inOrderTraverse(this);
                            }
                        }
                    }
                    if (func.Args != null)
                    {
                        for (int k = 0; k < func.Args.Count; k++)
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
                        AddColumnToList(list, stmt);

                        if (func.AnalyticFunction.OrderBy != null)
                        {
                            list = func.AnalyticFunction.OrderBy.Items;
                            AddColumnToList(list, stmt);
                        }
                    }
                    else if (func.WindowDef != null)
                    {
                        if (func.WindowDef.PartitionClause != null)
                        {
                            TParseTreeNodeList list = func.WindowDef.PartitionClause.ExpressionList;
                            AddColumnToList(list, stmt);
                        }
                        if (func.WindowDef.orderBy != null)
                        {
                            TParseTreeNodeList list = func.WindowDef.orderBy.Items;
                            AddColumnToList(list, stmt);
                        }
                    }
                }
                else if (lcexpr.ExpressionType == EExpressionType.subquery_t)
                {
                    TSelectSqlStatement select = lcexpr.SubQuery;
                    handleSubquery(select);
                }
                else if (lcexpr.ExpressionType == EExpressionType.case_t)
                {
                    TCaseExpression expr = lcexpr.CaseExpression;
                    TExpression conditionExpr = expr.Input_expr;
                    if (!impact.analyzeDlineage)
                    {
                        if (conditionExpr != null)
                        {
                            conditionExpr.inOrderTraverse(this);
                        }
                    }
                    TExpression defaultExpr = expr.Else_expr;
                    if (defaultExpr != null)
                    {
                        defaultExpr.inOrderTraverse(this);
                    }
                    TWhenClauseItemList list = expr.WhenClauseItemList;
                    AddColumnToList(list, stmt);
                }
                else if (lcexpr.ExpressionType == EExpressionType.interval_t)
                {
                    TIntervalExpression expression = lcexpr.IntervalExpr;
                    if (expression != null)
                    {
                        expression.Expr.inOrderTraverse(this);
                    }
                }
                return true;
            }

            private void handleSubquery(TSelectSqlStatement select)
            {
                if (select.ResultColumnList != null)
                {
                    for (int i = 0; i < select.ResultColumnList.size(); i++)
                    {
                        impact.linkFieldToTables(null,
                                select.ResultColumnList.getResultColumn(i),
                                select,
                                level + 1,
                                ClauseType.select);
                    }
                }
                else if (select.SetOperatorType != ESetOperatorType.none)
                {
                    handleSubquery(select.LeftStmt);
                    handleSubquery(select.RightStmt);
                }
            }

            public virtual void searchColumn()
            {
                if (this.expr != null)
                {
                    this.expr.inOrderTraverse(this);
                }
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


            internal string alias;
            internal string column;
            internal string aliasDisplayName;
            internal string columnDisplayName;
            public Tuple<long, long> location;
            public Tuple<long, long> fieldLocation;
            public string columnHighlightInfo;
            public string aliasHighlightInfo;

            public virtual string Column
            {
                get
                {
                    return column;
                }
                set
                {
                    this.columnDisplayName = value;
                    this.column = SQLUtil.trimObjectName(value);
                }
            }


            public virtual string Alias
            {
                get
                {
                    return alias;
                }
                set
                {
                    this.aliasDisplayName = value;
                    this.alias = SQLUtil.trimObjectName(value);
                }
            }


            public virtual string AliasDisplayName
            {
                get
                {
                    return aliasDisplayName;
                }
            }

            public virtual string ColumnDisplayName
            {
                get
                {
                    return columnDisplayName;
                }
            }


            public override int GetHashCode()
            {
                const int prime = 31;
                int result = 1;
                result = prime * result;
                result = prime
                        * result
                        + ((alias == null) ? 0 : alias.GetHashCode());
                result = prime
                        * result
                        + ((aliasDisplayName == null) ? 0
                                : aliasDisplayName.GetHashCode());
                result = prime
                        * result
                        + ((aliasHighlightInfo == null) ? 0
                                : aliasHighlightInfo.GetHashCode());
                result = prime
                        * result
                        + ((column == null) ? 0 : column.GetHashCode());
                result = prime
                        * result
                        + ((columnDisplayName == null) ? 0
                                : columnDisplayName.GetHashCode());
                result = prime
                        * result
                        + ((columnHighlightInfo == null) ? 0
                                : columnHighlightInfo.GetHashCode());
                result = prime
                        * result
                        + ((fieldLocation == null) ? 0
                                : fieldLocation.GetHashCode());
                result = prime
                        * result
                        + ((location == null) ? 0 : location.GetHashCode());
                return result;
            }

            public override bool Equals(Object obj)
            {
                if (this == obj)
                    return true;
                if (obj == null)
                    return false;
                if (GetType() != obj.GetType())
                    return false;
                TAlias other = (TAlias)obj;

                if (alias == null)
                {
                    if (other.alias != null)
                        return false;
                }
                else if (!alias.Equals(other.alias))
                    return false;
                if (aliasDisplayName == null)
                {
                    if (other.aliasDisplayName != null)
                        return false;
                }
                else if (!aliasDisplayName.Equals(other.aliasDisplayName))
                    return false;
                if (aliasHighlightInfo == null)
                {
                    if (other.aliasHighlightInfo != null)
                        return false;
                }
                else if (!aliasHighlightInfo.Equals(other.aliasHighlightInfo))
                    return false;
                if (column == null)
                {
                    if (other.column != null)
                        return false;
                }
                else if (!column.Equals(other.column))
                    return false;
                if (columnDisplayName == null)
                {
                    if (other.columnDisplayName != null)
                        return false;
                }
                else if (!columnDisplayName.Equals(other.columnDisplayName))
                    return false;
                if (columnHighlightInfo == null)
                {
                    if (other.columnHighlightInfo != null)
                        return false;
                }
                else if (!columnHighlightInfo.Equals(other.columnHighlightInfo))
                    return false;
                if (fieldLocation == null)
                {
                    if (other.fieldLocation != null)
                        return false;
                }
                else if (!fieldLocation.Equals(other.fieldLocation))
                    return false;
                if (location == null)
                {
                    if (other.location != null)
                        return false;
                }
                else if (!location.Equals(other.location))
                    return false;
                return true;
            }
        }

        public class TColumn
        {

            public string expression = "";
            public string columnName;
            public string columnPrex;
            public string orignColumn;
            public Tuple<long, long> location;
            public long offset;
            public long length;
            public IList<string> tableNames = new List<string>();
            public IList<string> tableFullNames = new List<string>();
            public ClauseType clauseType;
            public string alias;
            public string linkColumnName;
            internal bool isOrphan;

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
            public bool isConstant = false;

            public TResultEntry(ColumnImpact outerInstance, TColumn columnObject, String column,
                ClauseType clause, Tuple<long, long> location)
            {
                this.outerInstance = outerInstance;
                this.targetColumn = column;
                this.clause = clause;
                this.location = location;
                this.columnObject = columnObject;
                this.isConstant = true;
            }

            public TResultEntry(ColumnImpact outerInstance, TTable table, string column, ClauseType clause, Tuple<long, long> location)
            {
                this.outerInstance = outerInstance;
                this.targetTable = table;
                this.targetColumn = column;
                this.clause = clause;
                this.location = location;
                columnObject = new TColumn();
                columnObject.columnName = "*";
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
                            if (!tableName.Equals(fullName, StringComparison.CurrentCultureIgnoreCase))
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
            public string TableOwner;
            public LinkedHashMap<ClauseType, IList<Tuple<long, long>>> locations = new LinkedHashMap<ClauseType, IList<Tuple<long, long>>>();
            public ISet<Tuple<long, long>> highlightInfos = new HashSet<Tuple<long, long>>();
            public bool isNotOrphan = false;
        }

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
        private HashSet<TAlias> aliases = new HashSet<TAlias>();
        private StringBuilder buffer = new StringBuilder();
        private StringBuilder errorBuffer = new StringBuilder();
        private bool isSucess = true;
        private LinkedHashMap<string, TCustomSqlStatement> cteMap = new LinkedHashMap<string, TCustomSqlStatement>();
        private LinkedHashMap<string, LinkedHashMap<TCustomSqlStatement, bool>> accessMap = new LinkedHashMap<string, LinkedHashMap<TCustomSqlStatement, bool>>();
        private LinkedHashMap<String, TColumn> accessExpressions = new LinkedHashMap<String, TColumn>();
        private LinkedHashMap<TResultColumn, List<String>> accessColumns = new LinkedHashMap<TResultColumn, List<String>>();
        private LinkedHashMap<TCustomSqlStatement, ClauseType> currentClauseMap = new LinkedHashMap<TCustomSqlStatement, ClauseType>();
        private string currentSource = null;
        /* store the dependency relations */
        private LinkedHashMap<string, IList<TResultEntry>> dependMap = new LinkedHashMap<string, IList<TResultEntry>>();
        private bool debug = false;
        private bool showUIInfo = false;
        private int columnNumber = 0;
        private TCustomSqlStatement subquery = null;
        private TGSqlParser sqlparser;
        private IMetaDatabase metaDB;
        private Element fileNode;
        private string database = null;
        private IDictionary<TableMetaData, IList<ColumnMetaData>> tableColumns;
        private EDbVendor vendor;
        private bool strict = false;
        private bool traceErrorMessage = true;
        private bool analyzeDlineage = false;
        //JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
        private bool ignoreTopSelect_Renamed = false;
        private string virtualTable = null;

        public virtual string VirtualTableName
        {
            set
            {
                this.virtualTable = value;
            }
        }

        public virtual void ignoreTopSelect(bool ingore)
        {
            ignoreTopSelect_Renamed = ingore;
        }

        public virtual bool TraceErrorMessage
        {
            set
            {
                traceErrorMessage = value;
            }
        }

        public virtual bool ShowUIInfo
        {
            set
            {
                this.showUIInfo = value;
            }
        }

        public virtual bool Debug
        {
            set
            {
                this.debug = value;
            }
        }

        public virtual bool Sucess
        {
            get
            {
                return isSucess;
            }
            set
            {
                this.isSucess = value;
            }
        }


        public virtual bool AnalyzeDlineage
        {
            set
            {
                this.analyzeDlineage = value;
            }
        }

        public ColumnImpact(Element fileNode, EDbVendor dbVendor, IDictionary<TableMetaData, IList<ColumnMetaData>> tableColumns, bool strict)
        {
            this.vendor = dbVendor;
            this.strict = strict;
            this.tableColumns = tableColumns;
            MetaDB metaDB = new MetaDB(tableColumns, strict);
            sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqltext = removeParenthesis(SQLUtil.getFileContent(fileNode.Attribute("name").Value).ToUpper());
            //sqlparser.MetaDatabase = metaDB;
            this.metaDB = metaDB;
            this.fileNode = fileNode;
        }

        public ColumnImpact(string sqlText, EDbVendor dbVendor, IDictionary<TableMetaData, IList<ColumnMetaData>> tableColumns, bool strict)
        {
            this.vendor = dbVendor;
            this.strict = strict;
            this.tableColumns = tableColumns;
            MetaDB metaDB = new MetaDB(tableColumns, strict);
            sqlparser = new TGSqlParser(dbVendor);
            if (string.ReferenceEquals(sqlText, null))
            {
                sqlText = "";
            }
            sqlparser.sqltext = removeParenthesis(sqlText.ToUpper());
            //sqlparser.MetaDatabase = metaDB;
            this.metaDB = metaDB;
        }

        private string removeParenthesis(string sql)
        {
            String temp = sql.Trim();
            if (temp.StartsWith("(") && temp.EndsWith(")"))
            {
                temp = temp.Substring(1, temp.Length - 2);
                return removeParenthesis(temp);
            }
            return sql;
        }

        private TColumn attrToColumn(TExpression attr, TCustomSqlStatement stmt, ClauseType clauseType, TAlias parentAlias)
        {
            if (stmt is TSelectSqlStatement)
            {
                TSelectSqlStatement select = (TSelectSqlStatement)stmt;
                if (select.tables != null && select.tables.size() == 1)
                {
                    TTable table = select.tables.getTable(0);
                    if (table.TableName == null
                            && table.AliasClause == null
                            && table.Subquery != null)
                    {
                        stmt = table.Subquery;
                    }
                }
            }
            if (sqlparser.DbVendor == EDbVendor.dbvteradata)
            {
                if ((clauseType == ClauseType.select
                    || clauseType == ClauseType.casewhen || clauseType == ClauseType.casethen)
                    && parentAlias != null)
                {
                    if (!fromStmtTable(stmt, attr.ObjectOperand))
                    {
                        string columnName = SQLUtil.trimObjectName(attr.ObjectOperand.endToken.ToString());

                        TResultColumn resultColumn = getResultColumnByAlias(stmt,
                                columnName);

                        if (resultColumn != null)
                        {
                            if (resultColumn.AliasClause != null
                                    && !parentAlias.AliasDisplayName
                                            .Equals(resultColumn.ColumnAlias, StringComparison.CurrentCultureIgnoreCase))
                            {
                                linkFieldToTables(parentAlias,
                                        resultColumn,
                                        stmt,
                                        0,
                                        clauseType);
                                return null;
                            }
                        }
                    }
                }
            }

            TColumn column = new TColumn();
            column.clauseType = clauseType;
            column.columnName = SQLUtil.trimObjectName(attr.ObjectOperand.endToken.ToString());
            column.location = new Tuple<long, long>((int)attr.ObjectOperand.endToken.lineNo, (int)attr.endToken.columnNo);
            column.offset = attr.endToken.offset;
            column.length = attr.endToken.astext.Length;

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

            IList<String> segements = SQLUtil.parseNames(attr.ToString());
            if (segements.Count > 1)
            {
                column.columnPrex = SQLUtil.trimObjectName(attr.ToString()
                    .Substring(0,
                            attr.ToString().Length - ("."
                                    + segements[segements.Count - 1]).Length));

                string tableName = SQLUtil.trimObjectName(column.columnPrex);
                if (tableName.IndexOf(".", StringComparison.Ordinal) > 0)
                {
                    tableName = SQLUtil.trimObjectName(tableName.Substring(tableName.LastIndexOf(".", StringComparison.Ordinal) + 1));
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
                bool containColumn = false;
                if (stmt is TSelectSqlStatement)
                {
                    for (int i = 0; i < tables.Count; i++)
                    {
                        TTable lztable = tables.getTable(i);
                        Table table = TLzTaleToTable(lztable);
                        string tableName = lztable.FullName;
                        if (metaDB != null && !string.ReferenceEquals(tableName, null))
                        {
                            int dotIndex = tableName.LastIndexOf(".", StringComparison.Ordinal);
                            string TableOwner = null;
                            string tableRealName = null;
                            if (dotIndex >= 0)
                            {
                                TableOwner = lztable.TableName.DatabaseString;
                                if (!isNotEmpty(TableOwner))
                                {
                                    tableRealName = tableName;
                                }
                                else
                                {
                                    tableRealName = tableName.Replace(TableOwner + ".", "");
                                }
                            }
                            else
                            {
                                tableRealName = tableName;
                            }
                            string tableDatabaseString = null;
                            string tableSchemaString = null;
                            string tableNameString = null;
                            string[] split = tableRealName.Split(new char[] { '.' });
                            if (split.Length == 3)
                            {
                                tableDatabaseString = split[0];
                                tableSchemaString = split[1];
                                tableNameString = split[2];
                            }
                            else if (split.Length == 2)
                            {
                                tableDatabaseString = TableOwner;
                                tableSchemaString = split[0];
                                tableNameString = split[1];
                            }
                            else if (split.Length == 1)
                            {
                                tableDatabaseString = TableOwner;
                                tableNameString = split[0];
                            }
                            if (metaDB.checkColumn(null, tableDatabaseString, tableSchemaString, tableNameString, column.OrigName))
                            {
                                containColumn = true;
                                column.tableNames.Add(table.tableName);
                                if (!column.tableFullNames.Contains(lztable.FullName))
                                {
                                    column.tableFullNames.Add(lztable.FullName);
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < tables.size(); i++)
                {
                    TTable lztable = tables.getTable(i);

                    bool[] result = new bool[]{
                    false
                    };
                    bool flag = checkSubqueryTableColumns(parentAlias,
                            clauseType,
                            column,
                            lztable,
                            result);
                    if (result[0])
                    {
                        return null;
                    }
                    if (flag)
                    {
                        containColumn = true;
                        continue;
                    }

                    Table table = TLzTaleToTable(lztable);

                    if (lztable.Subquery != null)
                    {
                        TObjectNameList linkColumnNames = lztable.LinkedColumns;
                        for (int j = 0; j < linkColumnNames.size(); j++)
                        {
                            if (linkColumnNames.getObjectName(j).ColumnNameOnly
                                    .ToString()
                                    .Equals(column.columnName))
                            {
                                TObjectName name = linkColumnNames.getObjectName(j);
                                if (name.SourceColumn != null)
                                {
                                    linkFieldToTables(parentAlias, name, name.SourceColumn, lztable.Subquery, clauseType);
                                }
                                return null;
                            }
                        }
                    }

                    TObjectNameList cteColumnNames = lztable.CteColomnReferences;
                    if (cteColumnNames != null && cteColumnNames.size() > 0)
                    {
                        for (int j = 0; j < cteColumnNames.size(); j++)
                        {
                            if (cteColumnNames.getObjectName(j).ColumnNameOnly
                                    .Equals(column.columnName))
                            {
                                TObjectName name = cteColumnNames.getObjectName(j);
                                TCustomSqlStatement cteStmt = lztable.CTE.PreparableStmt;
                                if (cteStmt != null
                                        && cteStmt.ResultColumnList != null
                                        && cteStmt.ResultColumnList.size() > j)
                                {
                                    linkFieldToTables(parentAlias, name,
                                            cteStmt.ResultColumnList
                                                    .getResultColumn(j),
                                            cteStmt,
                                            clauseType);
                                }
                                return null;
                            }
                        }
                    }

                    TObjectNameList columnNames = lztable.LinkedColumns;
                    for (int j = 0; j < columnNames.size(); j++)
                    {
                        if (columnNames.getObjectName(j).ColumnNameOnly
                                .ToString()
                                .Equals(column.columnName))
                        {
                            column.tableNames.Add(table.tableName);
                            column.tableFullNames.Add(lztable.FullName);
                            containColumn = true;
                        }
                    }
                }

                if (!containColumn)
                {
                    if (tables.Count > 0)
                    {
                        TTable lztable = tables.getTable(0);
                        Table table = TLzTaleToTable(lztable);
                        if (!column.tableNames.Contains(table.tableName))
                        {
                            column.tableNames.Add(table.tableName);
                            if (!column.tableFullNames.Contains(lztable.FullName))
                            {
                                column.tableFullNames.Add(lztable.FullName);
                            }
                        }

                        if (tables.Count > 1 && stmt is TSelectSqlStatement)
                        {
                            if (fileNode != null)
                            {
                                errorBuffer.Append("Orphan column: " + attr.ObjectOperand.endToken.ToString() + ", at line: " + column.location.Item1 + ", column:" + column.location.Item2 + ".");
                                errorBuffer.Append(" File: ").Append(fileNode.Attribute("name").Value);
                                errorBuffer.AppendLine();
                            }
                            column.isOrphan = true;
                        }
                        else if (column.tableNames.Count == 0)
                        {
                            column.tableNames.Add(SQLUtil.TABLE_CONSTANT);
                            column.tableFullNames.Add(SQLUtil.TABLE_CONSTANT);
                        }
                    }
                }
            }

            column.orignColumn = column.columnName;
            return column;
        }

        private bool checkSubqueryTableColumns(TAlias parentAlias,
            ClauseType clauseType, TColumn column, TSelectSqlStatement stmt,
            bool[] result)
        {
            if (stmt.SetOperatorType != ESetOperatorType.none)
            {
                return checkSubqueryTableColumns(parentAlias,
                        clauseType,
                        column,
                        stmt.LeftStmt,
                        result)
                        || checkSubqueryTableColumns(parentAlias,
                                clauseType,
                                column,
                                stmt.LeftStmt,
                                result);
            }
            else
            {
                TTableList tables = stmt.tables;
                bool flag = false;
                for (int i = 0; i < tables.size(); i++)
                {
                    if (checkSubqueryTableColumns(parentAlias,
                            clauseType,
                            column,
                            tables.getTable(i),
                            result))
                    {
                        flag = true;
                    }
                }
                return flag;
            }
        }

        private bool checkSubqueryTableColumns(TAlias parentAlias,
                ClauseType clauseType, TColumn column, TTable lztable,
                bool[] result)
        {
            bool flag = false;

            TObjectNameList columnNames = lztable.LinkedColumns;
            for (int j = 0; j < columnNames.size(); j++)
            {
                if (SQLUtil.trimObjectName(columnNames.getObjectName(j)
                        .ColumnNameOnly)
                        .Equals(column.columnName))
                {
                    TObjectName name = columnNames.getObjectName(j);

                    if (lztable.Equals(name.SourceTable))
                    {
                        if (!SQLUtil.isEmpty(name.SourceTable.Name))
                        {
                            column.tableNames.Add(name.SourceTable.Name);
                            column.tableFullNames.Add(name.SourceTable
                                    .FullName);
                            return true;
                        }
                        else if (name.SourceTable.Subquery != null
                                && name.SourceColumn != null)
                        {
                            linkFieldToTables(parentAlias,
                                    name,
                                    name.SourceColumn,
                                    name.SourceTable.Subquery,
                                    clauseType);
                            result[0] = true;
                            return true;
                        }
                    }
                }
            }

            if (lztable.Subquery != null)
            {
                flag = checkSubqueryTableColumns(parentAlias,
                        clauseType,
                        column,
                        lztable.Subquery,
                        result);
            }

            return flag;
        }
        private bool fromStmtTable(TCustomSqlStatement stmt,
        TObjectName objectName)
        {
            if (objectName.SourceTable != null && stmt.tables != null)
            {
                for (int i = 0; i < stmt.tables.size(); i++)
                {
                    if (stmt.tables.getTable(i).Equals(objectName.SourceTable))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private TResultColumn getResultColumnByAlias(TCustomSqlStatement stmt, string columnName)
        {
            TResultColumnList columns = stmt.ResultColumnList;
            if (columns != null)
            {
                for (int i = 0; i < columns.Count; i++)
                {
                    TResultColumn column = columns.getResultColumn(i);
                    if (column.AliasClause != null && columnName.Equals(column.AliasClause.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        return column;
                    }
                    if (column.ColumnNameOnly != null
                        && columnName.Equals(column.ColumnNameOnly, StringComparison.CurrentCultureIgnoreCase))
                        return column;
                }
            }
            return null;
        }

        private TColumn attrToColumn(TAlias parentAlias, TObjectName objectName, TCustomSqlStatement stmt, ClauseType clauseType)
        {
            TColumn column = new TColumn();
            column.clauseType = clauseType;
            column.columnName = SQLUtil.trimObjectName(objectName.endToken.ToString());
            column.location = new Tuple<long, long>((int)objectName.endToken.lineNo, (int)objectName.endToken.columnNo);
            column.offset = objectName.endToken.offset;
            column.length = objectName.endToken.astext.Length;

            List<TParseTreeNode> tokens = objectName.startToken.nodesStartFromThisToken;

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

            IList<String> segements = SQLUtil.parseNames(objectName.ToString());
            if (segements.Count > 1)
            {
                column.columnPrex = SQLUtil.trimObjectName(objectName.ToString()
                    .Substring(0,
                            objectName.ToString().Length - ("."
                                    + segements[segements.Count - 1]).Length));

                string tableName = SQLUtil.trimObjectName(column.columnPrex);
                if (tableName.IndexOf(".", StringComparison.Ordinal) > 0)
                {
                    tableName = SQLUtil.trimObjectName(tableName.Substring(tableName.LastIndexOf(".", StringComparison.Ordinal) + 1));
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
                bool containColumn = false;
                if (stmt is TSelectSqlStatement)
                {
                    for (int i = 0; i < tables.Count; i++)
                    {
                        TTable lztable = tables.getTable(i);
                        Table table = TLzTaleToTable(lztable);
                        string tableName = lztable.FullName;
                        if (metaDB != null && !string.ReferenceEquals(tableName, null))
                        {
                            int dotIndex = tableName.LastIndexOf(".", StringComparison.Ordinal);
                            string TableOwner = null;
                            string tableRealName = null;
                            if (dotIndex >= 0)
                            {
                                TableOwner = lztable.TableName.DatabaseString;
                                if (!isNotEmpty(TableOwner))
                                {
                                    tableRealName = tableName;
                                }
                                else
                                {
                                    tableRealName = tableName.Replace(TableOwner + ".", "");
                                }
                            }
                            else
                            {
                                tableRealName = tableName;
                            }
                            string tableDatabaseString = null;
                            string tableSchemaString = null;
                            string tableNameString = null;
                            string[] split = tableRealName.Split(new char[] { '.' });
                            if (split.Length == 3)
                            {
                                tableDatabaseString = split[0];
                                tableSchemaString = split[1];
                                tableNameString = split[2];
                            }
                            else if (split.Length == 2)
                            {
                                tableDatabaseString = TableOwner;
                                tableSchemaString = split[0];
                                tableNameString = split[1];
                            }
                            else if (split.Length == 1)
                            {
                                tableDatabaseString = TableOwner;
                                tableNameString = split[0];
                            }
                            if (metaDB.checkColumn(null, tableDatabaseString, tableSchemaString, tableNameString, column.OrigName))
                            {
                                containColumn = true;
                                column.tableNames.Add(table.tableName);
                                if (!column.tableFullNames.Contains(lztable.FullName))
                                {
                                    column.tableFullNames.Add(lztable.FullName);
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < tables.size(); i++)
                {
                    TTable lztable = tables.getTable(i);

                    bool[] result = new bool[]{
                        false
                    };
                    bool flag = checkSubqueryTableColumns(parentAlias,
                            clauseType,
                            column,
                            lztable,
                            result);
                    if (result[0])
                    {
                        return null;
                    }
                    if (flag)
                    {
                        containColumn = true;
                        continue;
                    }

                    Table table = TLzTaleToTable(lztable);

                    if (lztable.Subquery != null)
                    {
                        TObjectNameList linkColumnNames = lztable.LinkedColumns;
                        for (int j = 0; j < linkColumnNames.size(); j++)
                        {
                            if (linkColumnNames.getObjectName(j).ColumnNameOnly
                                    .ToString()
                                    .Equals(column.columnName))
                            {
                                TObjectName name = linkColumnNames.getObjectName(j);
                                if (name.SourceColumn != null)
                                {
                                    linkFieldToTables(parentAlias, name, name.SourceColumn, lztable.Subquery, clauseType);
                                }
                                return null;
                            }
                        }
                    }

                    TObjectNameList cteColumnNames = lztable.CteColomnReferences;
                    if (cteColumnNames != null && cteColumnNames.size() > 0)
                    {
                        for (int j = 0; j < cteColumnNames.size(); j++)
                        {
                            if (cteColumnNames.getObjectName(j).ColumnNameOnly
                                    .Equals(column.columnName))
                            {
                                TObjectName name = cteColumnNames.getObjectName(j);
                                TCustomSqlStatement cteStmt = lztable.CTE.PreparableStmt;
                                if (cteStmt != null
                                        && cteStmt.ResultColumnList != null
                                        && cteStmt.ResultColumnList.size() > j)
                                {
                                    linkFieldToTables(parentAlias, name,
                                            cteStmt.ResultColumnList
                                                    .getResultColumn(j),
                                            cteStmt,
                                            clauseType);
                                }
                                return null;
                            }
                        }
                    }

                    TObjectNameList columnNames = lztable.LinkedColumns;
                    for (int j = 0; j < columnNames.size(); j++)
                    {
                        if (columnNames.getObjectName(j).ColumnNameOnly
                                .ToString()
                                .Equals(column.columnName))
                        {
                            column.tableNames.Add(table.tableName);
                            column.tableFullNames.Add(lztable.FullName);
                            containColumn = true;
                        }
                    }
                }

                if (!containColumn)
                {
                    for (int i = 0; i < stmt.ResultColumnList.size(); i++)
                    {
                        TResultColumn column1 = stmt.ResultColumnList
                                .getResultColumn(i);
                        if (column1.AliasClause != null
                                && column1.AliasClause.ToString()
                                        .Equals(objectName.ColumnNameOnly))
                        {
                            IList<TColumn> columns = exprToColumn(column1.Expr,
                                    stmt,
                                    0,
                                    clauseType);
                            if (columns != null && columns.Count>0)
                            {
                                containColumn = true;
                                column.orignColumn = column1.Expr.ToString();
                                return column;
                            }
                        }
                    }
                }

                if (!containColumn && tables.Count>0)
                {
                    TTable lztable = tables.getTable(0);
                    Table table = TLzTaleToTable(lztable);
                    if (!column.tableNames.Contains(table.tableName))
                    {
                        column.tableNames.Add(table.tableName);
                        if (!column.tableFullNames.Contains(lztable.FullName))
                        {
                            column.tableFullNames.Add(lztable.FullName);
                        }
                    }

                    if (tables.Count > 1 && stmt is TSelectSqlStatement)
                    {
                        if (fileNode != null)
                        {
                            errorBuffer.Append("Orphan column: " + objectName.endToken.ToString() + ", at line: " + column.location.Item1 + ", column:" + column.location.Item2 + ".");
                            errorBuffer.Append(" File: ").Append(fileNode.Attribute("name").Value);
                            errorBuffer.AppendLine();
                        }
                        column.isOrphan = true;
                    }
                }
            }

            column.orignColumn = column.columnName;
            return column;
        }

        private static bool isNotEmpty(string str)
        {
            return !string.ReferenceEquals(str, null) && str.Trim().Length > 0;
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
                return containClasuse(currentClauseMap, select.ParentStmt);
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
            return findColumnInSubQuery(select, null, columnName, level, originLocation);
        }

        private bool findColumnInSubQuery(TSelectSqlStatement select, TAliasClause tableAlias, string columnName, int level, Tuple<long, long> originLocation)
        {
            bool ret = false;
            String key = (tableAlias != null ? tableAlias.ToString() : "")
                + ":"
                + columnName;
            if (accessMap.ContainsKey(key) && accessMap[key] != null && accessMap[key].ContainsKey(select))
            {
                return accessMap[key][select];
            }
            else
            {
                if (!accessMap.ContainsKey(key))
                {
                    accessMap[key] = new LinkedHashMap<TCustomSqlStatement, bool>();
                }
                accessMap[key][select] = false;
            }
            if (select.SetOperatorType != ESetOperatorType.none)
            {
                bool left = findColumnInSubQuery(select.LeftStmt, tableAlias, columnName, level, originLocation);
                bool right = findColumnInSubQuery(select.RightStmt, tableAlias, columnName, level, originLocation);
                ret = left && right;
            }
            else if (select.ResultColumnList != null)
            {
                // check colum name in select list of subquery
                TResultColumn columnField = null;
                if (!"*".Equals(columnName))
                {
                    for (int i = 0; i < select.ResultColumnList.Count; i++)
                    {
                        TResultColumn field = select.ResultColumnList.getResultColumn(i);
                        if (field.AliasClause != null)
                        {
                            if (field.AliasClause.ToString().Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
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
                                if (column == null)
                                {
                                    continue;
                                }
                                if (!string.ReferenceEquals(columnName, null) && columnName.Equals(column.columnName, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    columnField = field;
                                    break;
                                }
                                else if ("*".Equals(column.columnName) && select.ResultColumnList.Count == 1)
                                {
                                    if (select.tables.getTable(0) != null)
                                    {
                                        TObjectNameList columns = select.tables.getTable(0).LinkedColumns;
                                        for (int j = 0; j < columns.size(); j++)
                                        {
                                            TObjectName queryColumn = columns.getObjectName(j);
                                            if (tableAlias != null
                                                    && queryColumn.TableString != null
                                                    && tableAlias.ToString()
                                                            .Equals(queryColumn.TableString, StringComparison.CurrentCultureIgnoreCase))
                                            {
                                                if (columnName.Equals(queryColumn.ColumnNameOnly, StringComparison.CurrentCultureIgnoreCase))
                                                {
                                                    column.columnName = queryColumn.ColumnNameOnly;
                                                    findColumnInTables(column,
                                                            select,
                                                            level,
                                                            ret == false ? columnName
                                                                    : null,
                                                            originLocation);
                                                    findColumnsFromClauses(select,
                                                            level + 1);
                                                    return true;
                                                }
                                            }

                                            if (tableAlias == null
                                                    && queryColumn.TableString == null)
                                            {
                                                if (columnName.Equals(queryColumn.ColumnNameOnly, StringComparison.CurrentCultureIgnoreCase))
                                                {
                                                    column.columnName = queryColumn.ColumnNameOnly;
                                                    findColumnInTables(column,
                                                            select,
                                                            level,
                                                            ret == false ? columnName
                                                                    : null,
                                                            originLocation);
                                                    findColumnsFromClauses(select,
                                                            level + 1);
                                                    return true;
                                                }
                                            }
                                        }
                                    }
                                    columnField = field;
                                    break;
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < select.ResultColumnList.Count; i++)
                {
                    TResultColumn field = select.ResultColumnList.getResultColumn(i);
                    if (columnField != null && !field.Equals(columnField))
                    {
                        continue;
                    }
                    if (field.AliasClause != null)
                    {
                        ret = "*".Equals(columnName) || field.AliasClause.ToString().Equals(columnName, StringComparison.CurrentCultureIgnoreCase);
                        if (ret)
                        {
                            // let's check where this column come from?
                            if (debug)
                            {
                                buffer.Append(buildString(" ", level) + "--> " + field.AliasClause.ToString() + "(alias)\r\n");
                            }
                            linkFieldToTables(null, field, select, level, ClauseType.select);
                        }
                    }
                    else
                    {
                        if (field.Expr.ExpressionType == EExpressionType.simple_object_name_t)
                        {
                            TColumn column = attrToColumn(field.Expr, select, ClauseType.select, null);
                            if (column == null)
                            {
                                continue;
                            }
                            ret = "*".Equals(columnName) || (!string.ReferenceEquals(columnName, null) && columnName.Equals(column.columnName, StringComparison.CurrentCultureIgnoreCase));
                            if (ret || "*".Equals(column.columnName))
                            {
                                if ("*".Equals(column.columnName))
                                {
                                    column.columnName = columnName;
                                }

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

            if (!accessMap.ContainsKey(key))
            {
                accessMap[key] = new LinkedHashMap<TCustomSqlStatement, bool>();
            }
            LinkedHashMap<TCustomSqlStatement, bool> stmts = accessMap[key];
            stmts[select] = ret;

            return ret;
        } // findColumnInSubQuery

        private bool findColumnInTables(TColumn column, string tableName, TCustomSqlStatement select, int level)
        {
            if (column.clauseType != ClauseType.undefine)
            {
                return findColumnInTables(column, tableName, select, level, column.clauseType);
            }
            else
            {
                return findColumnInTables(column, tableName, select, level, ClauseType.undefine);
            }
        }

        private bool findColumnInTables(TColumn column, string tableName, TCustomSqlStatement select, int level, ClauseType clause)
        {
            bool ret = false;

            if (SQLUtil.TABLE_CONSTANT.Equals(tableName)
                && currentSource != null)
            {
                dependMap[currentSource].Add(new TResultEntry(this, column,
                        column.columnName,
                        clause,
                        column.location));
                return true;
            }

            TTableList tables = select.tables;

            // merge using

            if (tables.Count == 1)
            {
                TTable lzTable = tables.getTable(0);
                // buffer.AppendLine(lzTable.AsText);
                if ((lzTable.TableType == ETableSource.objectname) && (string.ReferenceEquals(tableName, null) || (!string.ReferenceEquals(tableName, null) && lzTable.AliasClause == null && getTableName(lzTable).Equals(SQLUtil.trimObjectName(tableName), StringComparison.CurrentCultureIgnoreCase)) || (!string.ReferenceEquals(tableName, null) && lzTable.AliasClause != null && lzTable.AliasClause.ToString().Equals(tableName, StringComparison.CurrentCultureIgnoreCase))))
                {
                    ret = true;

                    if (debug)
                    {
                        buffer.Append(buildString(" ", level) + "--> " + getTableName(lzTable) + "." + column.columnName + "\r\n");
                    }
                    if (cteMap.ContainsKey(getTableName(lzTable)))
                    {
                        if (debug)
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
                                dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, currentClauseMap[stmt], column.location));
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
                            else if (select is TInsertSqlStatement)
                            {
                                if (ClauseType.undefine.Equals(clause))
                                {
                                    dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, ClauseType.insert, column.location));
                                }
                                else
                                {
                                    dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, clause, column.location));
                                }
                            }
                            else if (select is TUpdateSqlStatement)
                            {
                                if (ClauseType.undefine.Equals(clause))
                                {
                                    dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, ClauseType.updateset, column.location));
                                }
                                else
                                {
                                    dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, clause, column.location));
                                }
                            }
                            else if (select is TMergeSqlStatement)
                            {
                                if (ClauseType.undefine.Equals(clause))
                                {
                                    dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, ClauseType.mergeset, column.location));
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

            for (int x = 0; x < tables.Count; x++)
            {
                TTable tempTable = tables.getTable(x);
                TTable lzTable = tempTable;
                if (tempTable.LinkTable != null)
                {
                    lzTable.TableName = tempTable.LinkTable.TableName;
                    TAliasClause alias = new TAliasClause();
                    alias.AliasName = tempTable.TableName;
                    lzTable.AliasClause = alias;
                }
                switch (lzTable.TableType)
                {
                    case ETableSource.objectname:
                        Table table = TLzTaleToTable(lzTable);
                        string alias = table.tableAlias;
                        if (!string.ReferenceEquals(alias, null))
                        {
                            alias = alias.Trim();
                        }
                        if ((!string.ReferenceEquals(tableName, null)) && ((tableName.Equals(alias, StringComparison.CurrentCultureIgnoreCase) || tableName.Equals(table.tableName, StringComparison.CurrentCultureIgnoreCase))))
                        {
                            if (debug)
                            {
                                buffer.Append(buildString(" ", level) + "--> " + table.tableName + "." + column.columnName + "\r\n");
                            }
                            if (cteMap.ContainsKey(getTableName(lzTable)))
                            {
                                if (debug)
                                {
                                    buffer.Append(buildString(" ", level) + "--> WITH CTE\r\n");
                                }
                                ret = findColumnInSubQuery((TSelectSqlStatement)cteMap[getTableName(lzTable)], column.columnName, level, column.location);
                            }
                            else
                            {
                                if (currentSource!=null && dependMap.ContainsKey(currentSource))
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
                                    else if (select is TInsertSqlStatement)
                                    {
                                        if (ClauseType.undefine.Equals(clause))
                                        {
                                            dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, ClauseType.insert, column.location));
                                        }
                                        else
                                        {
                                            dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, clause, column.location));
                                        }
                                    }
                                    else if (select is TUpdateSqlStatement)
                                    {
                                        if (ClauseType.undefine.Equals(clause))
                                        {
                                            dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, ClauseType.update, column.location));
                                        }
                                        else
                                        {
                                            dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, clause, column.location));
                                        }
                                    }
                                    else if (select is TMergeSqlStatement)
                                    {
                                        if (ClauseType.undefine.Equals(clause))
                                        {
                                            dependMap[currentSource].Add(new TResultEntry(this, lzTable, column, column.columnName, ClauseType.merge, column.location));
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

                            if (lzTable.AliasClause != null && getTableAliasName(lzTable).Equals(name, StringComparison.CurrentCultureIgnoreCase))
                            {
                                ret = findColumnInSubQuery(selectStat, lzTable.AliasClause, column.columnName, level, column.location);
                                break;
                            }

                            if (selectStat.SetOperatorType != ESetOperatorType.none)
                            {
                                ret = findColumnInSubQuery(selectStat,
                                        lzTable.AliasClause,
                                        column.columnName,
                                        level,
                                        column.location);
                                break;
                            }

                            bool flag = false;
                            for (int j = 0; j < selectStat.tables.Count; j++)
                            {
                                if (selectStat.tables.getTable(j).AliasClause != null)
                                {
                                    TTable tableItem = selectStat.tables.getTable(j);
                                    if (getTableAliasName(tableItem).Equals(name,StringComparison.CurrentCultureIgnoreCase)
                                            || (tableItem.Name != null && name.Equals(tableItem.Name, StringComparison.CurrentCultureIgnoreCase)))
                                    {
                                        ret = findColumnInSubQuery(selectStat, column.columnName, level, column.location);
                                        flag = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (selectStat.tables.getTable(j).Subquery != null)
                                    {
                                        ret = findColumnInSubQuery(selectStat.tables.getTable(j).Subquery, column.columnName, level, column.location);
                                        flag = true;
                                        break;
                                    }
                                    else if (selectStat.tables.getTable(j).TableName != null && selectStat.tables.getTable(j).TableName.ToString().Equals(name, StringComparison.CurrentCultureIgnoreCase))
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

            if (!ret && select.ParentStmt != null)
            {
                subquery = select;
                ret = findColumnInTables(column, tableName, select.ParentStmt, level, clause);
                subquery = null;
            }

            return ret;
        }

        private string getTableAliasName(TTable lztable)
        {
            return SQLUtil.trimObjectName(lztable.AliasClause.AliasName.ToString());
        }

        private string getTableName(TTable lzTable)
        {
            return SQLUtil.trimObjectName(lzTable.Name);
        }

        private bool findColumnInTables(TColumn column, TCustomSqlStatement select, int level, string columnName, Tuple<long, long> originLocation)
        {
            bool ret = false;
            foreach (string tableName in column.tableNames)
            {
                if (!string.ReferenceEquals(columnName, null) && metaDB != null && tableName != null)
                {
                    int dotIndex = tableName.LastIndexOf(".", StringComparison.Ordinal);
                    string TableOwner = null;
                    string tableRealName = null;
                    if (dotIndex >= 0)
                    {
                        TableOwner = tableName.Substring(0, dotIndex);
                        tableRealName = tableName.Replace(TableOwner + ".", "");
                    }
                    else
                    {
                        tableRealName = tableName;
                    }
                    if (metaDB.checkColumn(null, null, TableOwner, tableRealName, columnName))
                    {
                        column.columnName = columnName;
                        if (originLocation != null)
                        {
                            column.location = originLocation;
                        }
                        // column.orignColumn = "*";
                        ret |= findColumnInTables(column, tableName, select, level);
                    }
                    else if (column.tableNames.Count == 1)
                    {
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

        private void findColumnsFromClauses(TCustomSqlStatement select, int level)
        {
            if (analyzeDlineage)
            {
                return;
            }
            currentClauseMap[select] = ClauseType.undefine;
            LinkedHashMap<TExpression, ClauseType> clauseTable = new LinkedHashMap<TExpression, ClauseType>();
            if (select is TSelectSqlStatement)
            {

                TSelectSqlStatement statement = (TSelectSqlStatement)select;

                if (statement.OrderbyClause != null)
                {
                    TOrderBy sortList = statement.OrderbyClause;
                    for (int i = 0; i < sortList.Items.Count; i++)
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
                    for (int i = 0; i < statement.joins.Count; i++)
                    {
                        TJoin join = statement.joins.getJoin(i);
                        if (join.JoinItems != null)
                        {
                            for (int j = 0; j < join.JoinItems.Count; j++)
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
                    for (int i = 0; i < sortList.Count; i++)
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
                    for (int i = 0; i < statement.joins.Count; i++)
                    {
                        TJoin join = statement.joins.getJoin(i);
                        if (join.JoinItems != null)
                        {
                            for (int j = 0; j < join.JoinItems.Count; j++)
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
            else if (select is TInsertSqlStatement)
            {
                TInsertSqlStatement statement = (TInsertSqlStatement)select;

                if (statement.WhereClause != null)
                {
                    clauseTable[statement.WhereClause.Condition] = ClauseType.@where;
                }

                if (statement.joins != null)
                {
                    for (int i = 0; i < statement.joins.Count; i++)
                    {
                        TJoin join = statement.joins.getJoin(i);
                        if (join.JoinItems != null)
                        {
                            for (int j = 0; j < join.JoinItems.Count; j++)
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
            else if (select is TMergeSqlStatement)
            {
                TMergeSqlStatement statement = (TMergeSqlStatement)select;

                if (statement.WhereClause != null)
                {
                    clauseTable[statement.WhereClause.Condition] = ClauseType.@where;
                }

                if (statement.joins != null)
                {
                    for (int i = 0; i < statement.joins.Count; i++)
                    {
                        TJoin join = statement.joins.getJoin(i);
                        if (join.JoinItems != null)
                        {
                            for (int j = 0; j < join.JoinItems.Count; j++)
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

                if (statement.Condition != null)
                {
                    clauseTable[statement.Condition] = ClauseType.mergematch;
                }

                if (statement.MatchedSearchCondition != null)
                {
                    clauseTable[statement.MatchedSearchCondition] = ClauseType.mergematch;
                }

                if (statement.NotMatchedSearchCondition != null)
                {
                    clauseTable[statement.NotMatchedSearchCondition] = ClauseType.mergenotmatch;
                }

                for (int i = 0; i < statement.WhenClauses.Count; i++)
                {
                    TMergeWhenClause whenClause = statement.WhenClauses[i];
                    if (whenClause.UpdateClause != null && whenClause.UpdateClause.UpdateWhereClause != null)
                    {
                        clauseTable[whenClause.UpdateClause.UpdateWhereClause] = ClauseType.@where;
                    }
                    else if (whenClause.InsertClause != null && whenClause.InsertClause.InsertWhereClause != null)
                    {
                        clauseTable[whenClause.InsertClause.InsertWhereClause] = ClauseType.@where;
                    }
                    else if (whenClause.DeleteClause != null)
                    {

                    }
                }

            }

            foreach (TExpression expr in clauseTable.Keys)
            {
                currentClauseMap[select] = clauseTable[expr];

                if (debug)
                {
                    switch (currentClauseMap[select])
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
                        default:
                            break;
                    }

                }

                IList<TColumn> columns = exprToColumn(expr, select, level, clauseTable[expr]);
                foreach (TColumn column1 in columns)
                {
                    foreach (string tableName in column1.tableNames)
                    {
                        if (debug)
                        {

                            switch (currentClauseMap[select])
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
                                default:
                                    break;
                            }

                        }
                        findColumnInTables(column1, tableName, select, level + 2);
                    }

                }
            }
            currentClauseMap.Remove(select);
        }

        private void findColumnsFromList(TCustomSqlStatement select, int level, TParseTreeNodeList list, ClauseType clauseType)
        {
            if (list == null)
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                TParseTreeNode element = list.getElement(i);
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

        public virtual string ErrorMessage
        {
            get
            {
                return errorBuffer.ToString();
            }
        }

        public virtual void impactSQL()
        {
            int ret = 0;
            try
            {
                ret = sqlparser.parse();
            }
            catch (Exception e1)
            {
                errorBuffer.Append(e1.Message + "\r\n")
                        .Append(e1.StackTrace);
                return;
            }

            if (ret != 0)
            {
                errorBuffer.Append(sqlparser.Errormessage + "\r\n");
                isSucess = false;
            }
            else
            {
                Document doc = null;
                Element columnImpactResult = null;
                if (fileNode != null)
                {
                    doc = fileNode.Document;
                }
                else
                {
                    doc = new Document();
                    XDeclaration declaration = new XDeclaration("1.0", "utf-8", "no");
                    doc.Declaration = declaration;
                    columnImpactResult = new Element("columnImpactResult");
                    doc.Add(columnImpactResult);
                }
                for (int k = 0; k < sqlparser.sqlstatements.Count; k++)
                {
                    if (sqlparser.sqlstatements.get(k) is TCustomSqlStatement)
                    {
                        dependMap.Clear();
                        aliases.Clear();
                        currentSource = null;
                        cteMap.Clear();
                        currentClauseMap.Clear();
                        accessMap.Clear();
                        accessColumns.Clear();
                        accessExpressions.Clear();

                        TCustomSqlStatement select = sqlparser.sqlstatements.get(k);

                        if (select.ToString().Trim().StartsWith("USE", StringComparison.Ordinal))
                        {
                            database = (new DDLParser(null, null, select.dbvendor, select.ToString().Trim().ToUpper() + ";", true, null)).Database;
                            continue;
                        }

                        initCTEMap(select);

                        columnNumber = 0;
                        impactSqlFromStatement(select);

                        IList<TTable> tableCollections = new List<TTable>();
                        foreach (TAlias alias in aliases)
                        {
                            Element targetColumn = new Element("targetColumn");
                            targetColumn.Add(new XAttribute("name", alias.ColumnDisplayName));
                            targetColumn.Add(new XAttribute("coordinate", alias.fieldLocation.Item1 + "," + alias.fieldLocation.Item2));
                            if (!alias.Alias.Equals(alias.Column))
                            {
                                targetColumn.Add(new XAttribute("alias", alias.AliasDisplayName));
                                if (showUIInfo)
                                {
                                    targetColumn.Add(new XAttribute("aliasHighlightInfo", "" + alias.aliasHighlightInfo));
                                }
                                targetColumn.Add(new XAttribute("aliasCoordinate", "" + alias.location.Item1 + "," + alias.location.Item2));
                            }
                            if (showUIInfo)
                            {
                                targetColumn.Add(new XAttribute("columnHighlightInfo", "" + alias.columnHighlightInfo));
                            }

                            if (columnImpactResult != null)
                            {
                                columnImpactResult.Add(targetColumn);
                            }
                            else if (fileNode != null)
                            {
                                fileNode.Add(targetColumn);
                            }

                            LinkedHashMap<string, TSourceColumn> collections = new LinkedHashMap<string, TSourceColumn>();

                            if (dependMap.ContainsKey(alias.Alias))
                            {
                                IList<TResultEntry> results = dependMap[alias.Alias];
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

                                    if (!result.isConstant
                                        && result.targetTable.FullName == null)
                                    {
                                        continue;
                                    }

                                    string key = null;
                                    if (result.isConstant)
                                    {
                                        key = SQLUtil.trimObjectName((SQLUtil.TABLE_CONSTANT.ToLower()
                                                + "." + result.targetColumn).ToLower());
                                    }
                                    else if ("*".Equals(result.targetColumn))
                                    {
                                        if (!string.ReferenceEquals(result.columnObject.linkColumnName, null))
                                        {
                                            key = SQLUtil.trimObjectName(result.targetTable.FullName.ToLower() + "." + result.columnObject.linkColumnName);
                                        }
                                        else
                                        {
                                            key = SQLUtil.trimObjectName(result.targetTable.FullName.ToLower());
                                        }
                                    }
                                    else
                                    {
                                        key = SQLUtil.trimObjectName((result.targetTable.FullName.ToLower() + "." + result.targetColumn).ToLower());
                                    }

                                    TSourceColumn sourceColumn = null;
                                    if (collections.ContainsKey(key))
                                    {
                                        sourceColumn = collections[key];
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

                                        if (result.columnObject != null && showUIInfo)
                                        {
                                            sourceColumn.highlightInfos.Add(new Tuple<long, long>((int)result.columnObject.offset, (int)result.columnObject.length));
                                        }

                                        if (result.columnObject != null && !sourceColumn.isNotOrphan)
                                        {
                                            sourceColumn.isNotOrphan = !result.columnObject.isOrphan;
                                        }

                                    }
                                    else
                                    {
                                        sourceColumn = new TSourceColumn(this);
                                        collections[key] = sourceColumn;
                                        if (result.isConstant)
                                        {
                                            sourceColumn.TableOwner = null;
                                        }
                                        else
                                        {
                                            sourceColumn.TableOwner = result.targetTable.TableName
                                                    .DatabaseString;
                                        }
                                        if (!isNotEmpty(sourceColumn.TableOwner))
                                        {
                                            sourceColumn.TableOwner = database;
                                        }
                                        sourceColumn.tableName = getTableNameFromMetadata(result);
                                        if (!tableCollections.Contains(result.targetTable))
                                        {
                                            tableCollections.Add(result.targetTable);
                                        }

                                        sourceColumn.name = getColumnNameFromMetadata(result);

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

                                        if (result.columnObject != null && showUIInfo)
                                        {
                                            sourceColumn.highlightInfos.Add(new Tuple<long, long>((int)result.columnObject.offset, (int)result.columnObject.length));
                                        }

                                        if (result.columnObject != null && !sourceColumn.isNotOrphan)
                                        {
                                            sourceColumn.isNotOrphan = !result.columnObject.isOrphan;
                                        }
                                    }
                                }

                                IEnumerator<string> iter = collections.Keys.GetEnumerator();

                                while (iter.MoveNext())
                                {
                                    TSourceColumn sourceColumn = collections[iter.Current];
                                    if (sourceColumn.clauses.Count > 0)
                                    {
                                        for (int j = 0; j < sourceColumn.clauses.Count; j++)
                                        {
                                            ClauseType clause = sourceColumn.clauses[j];
                                            if (clause == ClauseType.createview || clause == ClauseType.createtable || clause == ClauseType.insert || clause == ClauseType.updateset || clause == ClauseType.mergeset || clause == ClauseType.topselect)
                                            {
                                                Element element = new Element("linkTable");
                                                if (clause == ClauseType.topselect || clause == ClauseType.createtable)
                                                {
                                                    element.Add(new XAttribute("type", "select"));
                                                }
                                                else if (clause == ClauseType.createview)
                                                {
                                                    element.Add(new XAttribute("type", "view"));
                                                }
                                                else if (clause == ClauseType.insert)
                                                {
                                                    element.Add(new XAttribute("type", "insert"));
                                                }
                                                else if (clause == ClauseType.updateset)
                                                {
                                                    element.Add(new XAttribute("type", "update"));
                                                }
                                                else if (clause == ClauseType.mergeset)
                                                {
                                                    element.Add(new XAttribute("type", "merge"));
                                                }

                                                if (sourceColumn.TableOwner != null)
                                                {
                                                    element.Add(new XAttribute("tableOwner", sourceColumn.TableOwner));
                                                }
                                                if (sourceColumn.tableName != null)
                                                {
                                                    element.Add(new XAttribute("tableName", sourceColumn.tableName));
                                                }
                                                if (sourceColumn.name != null)
                                                {
                                                    element.Add(new XAttribute("name", sourceColumn.name));
                                                }
                                                {
                                                    StringBuilder buffer = new StringBuilder();
                                                    buildLocationString(sourceColumn, clause, buffer);
                                                    if (buffer.ToString().Length != 0)
                                                    {
                                                        element.Add(new XAttribute("coordinate", buffer.ToString()));
                                                    }
                                                }
                                                if (showUIInfo)
                                                {
                                                    StringBuilder buffer = new StringBuilder();
                                                    buildHighligthString(sourceColumn, buffer);
                                                    if (buffer.ToString().Length != 0)
                                                    {
                                                        element.Add(new XAttribute("highlightInfos", buffer.ToString()));
                                                    }
                                                }

                                                if (!sourceColumn.isNotOrphan)
                                                {
                                                    element.Add(new XAttribute("orphan", "true"));
                                                }
                                                if (!(select is TSelectSqlStatement))
                                                {
                                                    if (sourceColumn.TableOwner != null)
                                                    {
                                                        if (element.Attribute("tableOwner") == null)
                                                        {
                                                            element.Add(new XAttribute("tableOwner",
                                                                sourceColumn.TableOwner));
                                                        }
                                                        else
                                                        {
                                                            element.Attribute("tableOwner").SetValue(sourceColumn.TableOwner);
                                                        }
                                                    }
                                                    if (sourceColumn.tableName != null)
                                                    {
                                                        if (element.Attribute("tableName") == null)
                                                        {
                                                            element.Add(new XAttribute("tableName",
                                                                    sourceColumn.tableName));
                                                        }
                                                        else
                                                        {
                                                            element.Attribute("tableName").SetValue(sourceColumn.tableName);
                                                        }
                                                    }
                                                    if (sourceColumn.name != null)
                                                    {
                                                        if (element.Attribute("name") == null)
                                                        {
                                                            element.Add(new XAttribute("name",
                                                                    sourceColumn.name));
                                                        }
                                                        else
                                                        {
                                                            element.Attribute("name").SetValue(sourceColumn.name);
                                                        }
                                                    }
                                                    String aliasName = null;
                                                    if (sourceColumn.name != null)
                                                    {
                                                        aliasName = sourceColumn.name;
                                                    }
                                                    if (sourceColumn.tableName != null)
                                                    {
                                                        aliasName = sourceColumn.tableName
                                                                + "."
                                                                + aliasName;
                                                    }
                                                    if (sourceColumn.TableOwner != null)
                                                    {
                                                        aliasName = sourceColumn.TableOwner
                                                                + "."
                                                                + aliasName;
                                                    }
                                                    if (targetColumn.Attribute("name") == null)
                                                    {
                                                        targetColumn.Add(new XAttribute("name",
                                                            aliasName));
                                                    }
                                                    else
                                                    {
                                                        targetColumn.Attribute("name").SetValue(aliasName);
                                                    }
                                                }
                                                targetColumn.Add(element);
                                            }
                                            else
                                            {
                                                Element element = new Element("sourceColumn");
                                                if (sourceColumn.TableOwner != null)
                                                {
                                                    element.Add(new XAttribute("tableOwner", sourceColumn.TableOwner));
                                                }
                                                if (sourceColumn.tableName != null)
                                                {
                                                    element.Add(new XAttribute("tableName", sourceColumn.tableName));
                                                }
                                                if (sourceColumn.name != null)
                                                {
                                                    element.Add(new XAttribute("name", sourceColumn.name));
                                                }
                                                {
                                                    StringBuilder buffer = new StringBuilder();
                                                    switch (clause)
                                                    {
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.@where:
                                                            buffer.Append("where");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.connectby:
                                                            buffer.Append("connect by");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.startwith:
                                                            buffer.Append("start with");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.orderby:
                                                            buffer.Append("order by");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.join:
                                                            buffer.Append("join");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.select:
                                                            buffer.Append("select");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.update:
                                                            buffer.Append("update");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.assign:
                                                            buffer.Append("assign");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.merge:
                                                            buffer.Append("merge");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.mergematch:
                                                            buffer.Append("merge match");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.mergenotmatch:
                                                            buffer.Append("merge not match");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.groupby:
                                                            buffer.Append("group by");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.casethen:
                                                            buffer.Append("case then");
                                                            break;
                                                        case demos.dlineage.columnImpact.ColumnImpact.ClauseType.casewhen:
                                                            buffer.Append("case when");
                                                            break;
                                                        default:
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
                                                if (showUIInfo)
                                                {
                                                    StringBuilder buffer = new StringBuilder();
                                                    buildHighligthString(sourceColumn, buffer);
                                                    if (buffer.ToString().Length != 0)
                                                    {
                                                        element.Add(new XAttribute("highlightInfos", buffer.ToString()));
                                                    }
                                                }
                                                if (!sourceColumn.isNotOrphan)
                                                {
                                                    element.Add(new XAttribute("orphan", "true"));
                                                }
                                                targetColumn.Add(element);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (showUIInfo)
                        {
                            for (int i = 0; i < tableCollections.Count; i++)
                            {
                                TTable table = tableCollections[i];
                                Element element = new Element("table");
                                if (table != null)
                                {
                                    element.Add(new XAttribute("owner", handleBlank(table.TableName.SchemaString)));
                                    element.Add(new XAttribute("name", handleBlank(table.Name)));
                                    element.Add(new XAttribute("highlightInfo", table.startToken.offset + "," + (table.TableName != null ? (table.TableName.endToken.offset - table.startToken.offset + table.TableName.endToken.astext.Length) : (table.endToken.offset - table.startToken.offset + table.endToken.astext.Length))));
                                    element.Add(new XAttribute("coordinate", table.startToken.lineNo + "," + table.startToken.columnNo));
                                }
                                else
                                {
                                    element.Add(new XAttribute("name", SQLUtil.TABLE_CONSTANT));
                                }
                                if (columnImpactResult != null)
                                {
                                    columnImpactResult.Add(element);
                                }
                                else if (fileNode != null)
                                {
                                    fileNode.Add(element);
                                }
                            }
                        }
                    }
                }
                if (columnImpactResult != null)
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

            if (traceErrorMessage && ErrorMessage.Trim().Length > 0)
            {
                Console.Error.Write(ErrorMessage);
            }
        }

        private object handleBlank(string value)
        {
            return value == null ? "" : value;
        }

        internal class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb) { }

            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        private string getColumnNameFromMetadata(TResultEntry result)
        {
            string columnName = !string.ReferenceEquals(result.columnObject.linkColumnName, null) ? result.columnObject.linkColumnName : result.targetColumn;
            TableMetaData tableMetaData = null;
            if (result.isConstant)
            {
                tableMetaData = getConstantTableMetaData();
            }
            else
            {
                TTable table = result.targetTable;
                tableMetaData = getTableMetaData(table.TableName);
            }
            ColumnMetaData columnMetaData = getColumnMetaData(tableMetaData,
                    columnName);

            if (columnMetaData != null)
            {
                if (result.columnObject != null)
                {
                    columnMetaData.setOrphan(Convert.ToString(result.columnObject.isOrphan));
                }

                return columnMetaData.DisplayName;
            }
            return columnName;
        }

        private string getTableNameFromMetadata(TResultEntry result)
        {
            if (result.isConstant)
            {
                return getConstantTableMetaData().DisplayName;
            }
            else
            {
                TTable table = result.targetTable;
                TableMetaData tableMetaData = getTableMetaData(table.TableName);
                if (tableMetaData != null)
                {
                    if (tableMetaData.SchemaDisplayName != null)
                        return tableMetaData.SchemaDisplayName
                                + "."
                                + tableMetaData.DisplayName;
                    else
                        return tableMetaData.DisplayName;
                }
                if (isNotEmpty(table.TableName.DatabaseString))
                {
                    return table.FullName.Replace(table.TableName
                            .DatabaseString + ".",
                            "");
                }
                else
                    return table.FullName;
            }
        }

        private TableMetaData getConstantTableMetaData()
        {
            TableMetaData tableMetaData = new TableMetaData(vendor, strict);
            tableMetaData.Name = (SQLUtil.TABLE_CONSTANT);
            tableMetaData = getTableMetaData(tableMetaData);
            return tableMetaData;
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

            if (tableMetaData == null || !tableColumns.ContainsKey(tableMetaData) || tableColumns[tableMetaData] == null)
            {
                return null;
            }
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

        private void initCTEMap(TCustomSqlStatement select)
        {
            if (select.Statements != null && select.Statements.Count > 0)
            {
                for (int i = 0; i < select.Statements.Count; i++)
                {
                    initCTEMap(select.Statements.get(i));
                }
            }
            if (select.CteList != null && select.CteList.Count > 0)
            {
                for (int i = 0; i < select.CteList.Count; i++)
                {
                    TCTE expression = select.CteList.getCTE(i);
                    cteMap[SQLUtil.trimObjectName(expression.TableName.ToString())] = expression.Subquery;
                }
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

        private void buildHighligthString(TSourceColumn sourceColumn, StringBuilder highlightBuffer)
        {
            ISet<Tuple<long, long>> infos = sourceColumn.highlightInfos;
            if (infos != null)
            {
                IEnumerator<Tuple<long, long>> iter = infos.GetEnumerator();
                while (iter.MoveNext())
                {
                    Tuple<long, long> point = iter.Current;
                    highlightBuffer.Append(point.Item1 + "," + point.Item2).Append(";");
                }
            }
        }

        private void impactSqlFromStatement(TCustomSqlStatement select)
        {
            if (select is TSelectSqlStatement)
            {
                TSelectSqlStatement stmt = (TSelectSqlStatement)select;
                if (stmt.SetOperatorType != ESetOperatorType.none)
                {
                    impactSqlFromStatement(stmt.LeftStmt);
                    impactSqlFromStatement(stmt.RightStmt);
                }
                else
                {
                    for (int i = 0; i < select.ResultColumnList.Count; i++)
                    {
                        linkFieldToTables(null, select.ResultColumnList.getResultColumn(i), select, 0, ClauseType.select);
                    }
                }
            }
            else if (select is TInsertSqlStatement && ((TInsertSqlStatement)select).SubQuery != null)
            {
                impactSqlFromStatement(((TInsertSqlStatement)select).SubQuery);
            }
            else if (select is TCreateViewSqlStatement)
            {
                impactSqlFromStatement(((TCreateViewSqlStatement)select).Subquery);
            }
            else if (select is TCreateTableSqlStatement && ((TCreateTableSqlStatement)select).SubQuery != null)
            {
                impactSqlFromStatement(((TCreateTableSqlStatement)select).SubQuery);
            }
            else if (select is TMergeSqlStatement)
            {
                TMergeSqlStatement merge = (TMergeSqlStatement)select;
                for (int i = 0; i < merge.WhenClauses.Count; i++)
                {
                    TMergeWhenClause whenClause = merge.WhenClauses[i];
                    if (whenClause.UpdateClause != null)
                    {
                        for (int j = 0; j < whenClause.UpdateClause.UpdateColumnList.Count; j++)
                        {
                            linkFieldToTables(null, whenClause.UpdateClause.UpdateColumnList.getResultColumn(j), select, 0, ClauseType.merge);
                        }
                    }
                    else if (whenClause.InsertClause != null)
                    {
                        if (whenClause.InsertClause.Valuelist == null)
                            continue;

                        for (int j = 0; j < whenClause.InsertClause.Valuelist.Count; j++)
                        {
                            linkFieldToTables(null, whenClause.InsertClause.ColumnList.getObjectName(j), whenClause.InsertClause.Valuelist.getResultColumn(j), select, ClauseType.merge);
                        }
                    }
                    else if (whenClause.DeleteClause != null)
                    {

                    }
                }
            }
            else if (select.ResultColumnList != null)
            {
                for (int i = 0; i < select.ResultColumnList.Count; i++)
                {
                    linkFieldToTables(null, select.ResultColumnList.getResultColumn(i), select, 0, ClauseType.undefine);
                }
            }
            else if (select.Statements != null)
            {
                for (int i = 0; i < select.Statements.Count; i++)
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
            if ("rownum".Equals(column.Trim(), StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            else if ("rowid".Equals(column.Trim(), StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            else if ("nextval".Equals(column.Trim(), StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            else if ("sysdate".Equals(column.Trim(), StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

        private bool linkFieldToTables(TAlias parentAlias, TExpression field, TCustomSqlStatement select, int level, ClauseType clauseType)
        {
            if (select is TSelectSqlStatement
                && ((TSelectSqlStatement)select).SetOperatorType != ESetOperatorType.none)
            {
                TSelectSqlStatement stmt = (TSelectSqlStatement)select;
                bool leftResult = false;
                bool rightResult = false;
                if (stmt.LeftStmt != null)
                {
                    leftResult = linkFieldToTables(parentAlias,
                            field,
                            stmt.LeftStmt,
                            level,
                            clauseType);
                }
                if (stmt.RightStmt != null)
                {
                    leftResult = linkFieldToTables(parentAlias,
                            field,
                            stmt.RightStmt,
                            level,
                            clauseType);
                }
                return leftResult || rightResult;
            }

            if (level == 0)
            {
                accessMap.Clear();
            }
            bool ret = false;
            // all items in select list was represented by a TLzField Objects
            switch (field.ExpressionType)
            {
                case EExpressionType.simple_object_name_t:
                    {
                        TColumn column = attrToColumn(field, select, clauseType, parentAlias);
                        if (column == null)
                        {
                            break;
                        }
                        bool isPseudocolumn = select.dbvendor == EDbVendor.dbvoracle && this.isPseudocolumn(column.columnName);
                        if (level == 0 || parentAlias != null)
                        {
                            TAlias alias = null;
                            if (parentAlias != null)
                            {
                                alias = parentAlias;
                            }
                            else
                            {
                                alias = new TAlias(this);
                                alias.Column = field.ToString();
                                alias.Alias = field.ToString();
                                alias.location = new Tuple<long, long>((int)field.startToken.lineNo, (int)field.startToken.columnNo);
                                alias.fieldLocation = alias.location;

                                alias.columnHighlightInfo = field.startToken.offset + "," + (field.endToken.offset - field.startToken.offset + field.endToken.astext.Length);

                                aliases.Add(alias);
                            }
                            currentSource = alias.Alias;
                            if (currentSource!=null && !dependMap.ContainsKey(currentSource))
                            {
                                dependMap[currentSource] = new List<TResultEntry>();
                            }

                            if (debug && parentAlias == null)
                            {
                                if (!alias.Alias.Equals(column.OrigName, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    buffer.Append("\r\nSearch " + alias.AliasDisplayName + (level == 0 ? (" <<column_" + (++columnNumber) + ">>") : "") + "\r\n");
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
                    }
                    goto default;
                default:
                    break;
            }
            return ret;
        }

        private bool linkFieldToTables(TAlias parentAlias, TObjectName objectName, TResultColumn field, TCustomSqlStatement select, ClauseType clauseType)
        {
            if (select is TSelectSqlStatement
                && ((TSelectSqlStatement)select).SetOperatorType != ESetOperatorType.none)
            {
                TSelectSqlStatement stmt = (TSelectSqlStatement)select;
                bool leftResult = false;
                bool rightResult = false;
                if (stmt.LeftStmt != null)
                {
                    leftResult = linkFieldToTables(parentAlias, objectName,
                            field,
                            stmt.LeftStmt,
                            clauseType);
                }
                if (stmt.RightStmt != null)
                {
                    rightResult = linkFieldToTables(parentAlias, objectName,
                            field,
                            stmt.RightStmt,
                            clauseType);
                }
                return leftResult || rightResult;
            }

            TAlias alias = parentAlias;
            if (parentAlias == null)
            {
                alias = new TAlias(this);
                alias.Column = objectName.ToString();
                alias.Alias = objectName.ToString();
                alias.location = new Tuple<long, long>((int)objectName.startToken.lineNo, (int)objectName.startToken.columnNo);
                alias.fieldLocation = alias.location;
                alias.columnHighlightInfo = objectName.startToken.offset + "," + (objectName.endToken.offset - objectName.startToken.offset + objectName.endToken.astext.Length);
                aliases.Add(alias);
            }

            currentSource = alias.Alias;
            if (currentSource!=null && !dependMap.ContainsKey(currentSource))
            {
                dependMap[currentSource] = new List<TResultEntry>();
            }

            TColumn column = attrToColumn(alias, objectName, select, clauseType);
            if (column == null)
            {
                return false;
            }

            bool ret = false;

            if (column != null)
            {
                ret = findColumnInTables(column, select, 1, null, null);
            }

            findColumnsFromClauses(select, 2);

            clauseType = ClauseType.assign;
            linkFieldToTables(alias, field, select, 1, clauseType);

            if (select is TMergeSqlStatement)
            {
                TMergeSqlStatement merge = (TMergeSqlStatement)select;
                TTable table = new TTable();
                table.TableName = merge.TargetTable.TableName;
                TColumn linkColumn = new TColumn();
                linkColumn.linkColumnName = objectName.ColumnNameOnly.ToString();
                linkColumn.columnName = SQLUtil.trimObjectName(currentSource);

                linkColumn.location = new Tuple<long, long>((int)objectName.startToken.lineNo, (int)objectName.startToken.columnNo);

                linkColumn.offset = objectName.startToken.offset;
                linkColumn.length = objectName.ToString().Length;
                linkColumn.clauseType = ClauseType.mergeset;
                TResultEntry resultEntry = new TResultEntry(this, table, linkColumn, linkColumn.columnName, ClauseType.mergeset, linkColumn.location);
                dependMap[currentSource].Add(resultEntry);
            }

            return ret;
        }

        private bool linkFieldToTables(TAlias parentAlias, TResultColumn field, TCustomSqlStatement select, int level, ClauseType clauseType)
        {
            if (select is TSelectSqlStatement
                    && ((TSelectSqlStatement)select).SetOperatorType != ESetOperatorType.none)
            {
                TSelectSqlStatement stmt = (TSelectSqlStatement)select;
                bool leftResult = false;
                bool rightResult = false;
                if (stmt.LeftStmt != null)
                {
                    leftResult = linkFieldToTables(parentAlias,
                            field,
                            stmt.LeftStmt,
                            level,
                            clauseType);
                }
                if (stmt.RightStmt != null)
                {
                    leftResult = linkFieldToTables(parentAlias,
                            field,
                            stmt.RightStmt,
                            level,
                            clauseType);
                }
                return leftResult || rightResult;
            }

            if (!accessColumns.ContainsKey(field))
            {
                List<String> aliases = new List<String>();
                if (parentAlias != null)
                {
                    aliases.Add(parentAlias.alias);
                }
                else
                {
                    aliases.Add(null);
                }
                accessColumns[field] = aliases;
            }
            else
            {
                List<String> aliases = accessColumns[field];
                if (parentAlias != null)
                {
                    if (aliases.Contains(parentAlias.alias))
                    {
                        return true;
                    }
                    else
                    {
                        aliases.Add(parentAlias.alias);
                    }
                }
                else
                {
                    aliases.Add(null);
                }
            }

            if (level == 0)
            {
                accessMap.Clear();
            }
            bool ret = false;
            // all items in select list was represented by a TLzField Objects

            switch (field.Expr.ExpressionType)
            {
                case EExpressionType.simple_constant_t:
                    {
                        TColumn column = new TColumn();
                        column.alias = field.ColumnAlias;
                        column.columnName = field.Expr.ToString();
                        column.location = new Tuple<long, long>(field.Expr
                            .endToken.lineNo, (int)field.Expr
                            .endToken.columnNo);
                        column.offset = field.Expr.endToken.offset;
                        column.length = field.Expr.endToken.astext.Length;
                        column.tableNames.Add(SQLUtil.TABLE_CONSTANT);
                        column.tableFullNames.Add(SQLUtil.TABLE_CONSTANT
                            + "."
                            + column.columnName);
                        column.clauseType = clauseType;
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
                                columnAlias.Column = field.ToString();
                                columnAlias.Alias = field.ToString();
                                columnAlias.location = new Tuple<long, long>((int)field.startToken.lineNo, (int)field.startToken.columnNo);
                                columnAlias.fieldLocation = columnAlias.location;

                                columnAlias.columnHighlightInfo = field.startToken.offset + "," + (field.endToken.offset - field.startToken.offset + field.endToken.astext.Length);

                                if (field.AliasClause != null)
                                {
                                    columnAlias.Alias = field.AliasClause.ToString();
                                    columnAlias.Column = field.ToString();
                                    TSourceToken startToken = field.AliasClause.AliasName.startToken;
                                    columnAlias.location = new Tuple<long, long>((int)startToken.lineNo, (int)startToken.columnNo);
                                    columnAlias.aliasHighlightInfo = field.AliasClause.startToken.offset + "," + (field.AliasClause.endToken.offset - field.AliasClause.startToken.offset + field.AliasClause.endToken.astext.Length);

                                    columnAlias.columnHighlightInfo = field.startToken.offset + "," + (field.Expr.endToken.offset - field.startToken.offset + field.Expr.endToken.astext.Length);
                                }
                                aliases.Add(columnAlias);
                            }
                            currentSource = columnAlias.Alias;
                            if (currentSource!=null && !dependMap.ContainsKey(currentSource))
                            {
                                dependMap[currentSource] = new List<TResultEntry>();
                            }

                            if (debug && parentAlias == null)
                            {
                                if (!columnAlias.Alias.Equals(column.OrigName, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    buffer.Append("\r\nSearch " + columnAlias.AliasDisplayName + (level == 0 ? (" <<column_" + (++columnNumber) + ">>") : "") + "\r\n");
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
                    }
                    break;
                case EExpressionType.simple_object_name_t:
                    {
                        TColumn column = attrToColumn(field.Expr,
                                    select,
                                    clauseType,
                                    parentAlias);
                        findColumnsFromClauses(select, 2);

                        if (column == null)
                        {
                            return false;
                        }

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
                                columnAlias.Column = field.ToString();
                                columnAlias.Alias = field.ToString();
                                columnAlias.location = new Tuple<long, long>((int)field.startToken.lineNo, (int)field.startToken.columnNo);
                                columnAlias.fieldLocation = columnAlias.location;

                                columnAlias.columnHighlightInfo = field.startToken.offset + "," + (field.endToken.offset - field.startToken.offset + field.endToken.astext.Length);

                                if (field.AliasClause != null)
                                {
                                    columnAlias.Alias = field.AliasClause.ToString();
                                    columnAlias.Column = field.ToString();
                                    TSourceToken startToken = field.AliasClause.AliasName.startToken;
                                    columnAlias.location = new Tuple<long, long>((int)startToken.lineNo, (int)startToken.columnNo);
                                    columnAlias.aliasHighlightInfo = field.AliasClause.startToken.offset + "," + (field.AliasClause.endToken.offset - field.AliasClause.startToken.offset + field.AliasClause.endToken.astext.Length);

                                    columnAlias.columnHighlightInfo = field.startToken.offset + "," + (field.Expr.endToken.offset - field.startToken.offset + field.Expr.endToken.astext.Length);
                                }
                                aliases.Add(columnAlias);
                            }
                            currentSource = columnAlias.Alias;
                            if (!dependMap.ContainsKey(currentSource))
                            {
                                dependMap[currentSource] = new List<TResultEntry>();
                            }

                            if (debug && parentAlias == null)
                            {
                                if (!columnAlias.Alias.Equals(column.OrigName, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    buffer.Append("\r\nSearch " + columnAlias.AliasDisplayName + (level == 0 ? (" <<column_" + (++columnNumber) + ">>") : "") + "\r\n");
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
                    }
                    break;
                case EExpressionType.subquery_t:
                    TAlias alias1 = new TAlias(this);
                    alias1.Column = field.ToString();
                    alias1.Alias = field.ToString();
                    alias1.location = new Tuple<long, long>((int)field.startToken.lineNo, (int)field.startToken.columnNo);
                    alias1.fieldLocation = alias1.location;
                    alias1.columnHighlightInfo = field.startToken.offset + "," + (field.endToken.offset - field.startToken.offset + field.endToken.astext.Length);

                    if (field.AliasClause != null)
                    {
                        alias1.Alias = field.AliasClause.ToString();
                        TSourceToken startToken = field.AliasClause.AliasName.startToken;
                        alias1.Column = field.ToString();
                        alias1.location = new Tuple<long, long>((int)startToken.lineNo, (int)startToken.columnNo);

                        alias1.aliasHighlightInfo = field.AliasClause.startToken.offset + "," + (field.AliasClause.endToken.offset - field.AliasClause.startToken.offset + field.AliasClause.endToken.astext.Length);

                        alias1.columnHighlightInfo = field.startToken.offset + "," + (field.Expr.endToken.offset - field.startToken.offset + field.Expr.endToken.astext.Length);
                    }

                    if (level == 0)
                    {
                        aliases.Add(alias1);
                        if (debug)
                        {
                            buffer.Append("\r\nSearch " + alias1.AliasDisplayName + (level == 0 ? (" <<column_" + (++columnNumber) + ">>") : "") + "\r\n");
                            // buffer.append( "--> "
                            // + field.getExpr( ).getSubQuery( )
                            // + "\r\n" );
                        }
                    }
                    TSelectSqlStatement stmt = field.Expr.SubQuery;
                    IList<TSelectSqlStatement> stmtList = new List<TSelectSqlStatement>();
                    getSelectSqlStatements(stmt, stmtList);
                    for (int i = 0; i < stmtList.Count; i++)
                    {
                        linkFieldToTables(alias1, stmtList[i].ResultColumnList.getResultColumn(0), stmtList[i], level - 1 < 0 ? 0 : level - 1, clauseType);
                    }
                    break;
                default:
                    TAlias alias = parentAlias;

                    if (level == 0)
                    {

                        if (select is TMergeSqlStatement)
                        {
                            TExpression expression = field.Expr.LeftOperand;
                            if (expression != null)
                            {
                                linkFieldToTables(null, expression, select, level, ClauseType.merge);
                            }
                        }
                        else if (select is TUpdateSqlStatement)
                        {
                            TExpression expression = field.Expr.LeftOperand;
                            if (expression.ExpressionType == EExpressionType.list_t)
                            {
                                TExpression setExpression = field.Expr.RightOperand;
                                for (int i = 0; i < expression.ExprList.Count; i++)
                                {
                                    linkFieldToTables(null, expression.ExprList.getExpression(i), select, level, ClauseType.update);
                                    if (setExpression != null && setExpression.SubQuery != null)
                                    {
                                        TSelectSqlStatement query = setExpression.SubQuery;
                                        if (query.ResultColumnList.Count > i)
                                        {
                                            linkFieldToTables(null, query.ResultColumnList.getResultColumn(i), query, level + 1, clauseType);
                                        }
                                    }
                                }
                                break;
                            }
                            else
                            {
                                linkFieldToTables(null, getColumnExpression(expression), select, level, ClauseType.update);
                            }

                        }
                        else if (alias == null)
                        {
                            alias = new TAlias(this);
                            alias.Column = field.ToString();
                            alias.Alias = alias.ColumnDisplayName;
                            alias.location = new Tuple<long, long>((int)field.startToken.lineNo, (int)field.startToken.columnNo);
                            alias.fieldLocation = alias.location;
                            alias.columnHighlightInfo = field.startToken.offset + "," + (field.endToken.offset - field.startToken.offset + field.endToken.astext.Length);

                        }

                        if (alias != null && parentAlias == null)
                        {
                            if (field.AliasClause != null)
                            {
                                alias.Alias = field.AliasClause.ToString();
                                alias.Column = field.ToString();
                                TSourceToken startToken = field.AliasClause.AliasName.startToken;
                                alias.location = new Tuple<long, long>((int)startToken.lineNo, (int)startToken.columnNo);

                                alias.aliasHighlightInfo = field.AliasClause.startToken.offset + "," + (field.AliasClause.endToken.offset - field.AliasClause.startToken.offset + field.AliasClause.endToken.astext.Length);

                                alias.columnHighlightInfo = field.startToken.offset + "," + (field.Expr.endToken.offset - field.startToken.offset + field.Expr.endToken.astext.Length);
                            }
                            aliases.Add(alias);

                            if (debug)
                            {
                                buffer.Append("\r\n" + "Search " + alias.AliasDisplayName + (level == 0 ? (" <<column_" + (++columnNumber) + ">>") : "") + "\r\n");
                            }

                            currentSource = alias.Alias;
                            if (!dependMap.ContainsKey(currentSource))
                            {
                                dependMap[currentSource] = new List<TResultEntry>();
                            }
                        }
                    }

                    if (select is TUpdateSqlStatement)
                    {
                        clauseType = ClauseType.assign;
                    }
                    else if (select is TMergeSqlStatement)
                    {
                        clauseType = ClauseType.assign;
                    }

                    IList<TColumn> columns = exprToColumn(field.Expr, select, level, true, clauseType, alias);

                    if (select is TUpdateSqlStatement)
                    {
                        if (columns.Count > 0)
                        {
                            columns.RemoveAt(0);
                        }
                    }
                    else if (select is TMergeSqlStatement)
                    {
                        if (columns.Count > 0)
                        {
                            columns.RemoveAt(0);
                        }
                    }
                    if (debug)
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
                            if (debug)
                            {
                                buffer.Append("\r\n" + "Search " + column1.OrigName + "\r\n");
                            }
                        }

                        findColumnInTables(column1, select, level + 1, null, null);
                        findColumnsFromClauses(select, level + 2);
                    }

                    if (field.Expr.ExpressionType == EExpressionType.function_t)
                    {
                        TFunctionCall func = field.Expr.FunctionCall;
                        // buffer.AppendLine("function name {0}",
                        // func.funcname.AsText);
                        if (func.FunctionName.ToString().Equals("count", StringComparison.CurrentCultureIgnoreCase) || func.FunctionName.ToString().Equals("sum", StringComparison.CurrentCultureIgnoreCase) || func.FunctionName.ToString().Equals("row_number", StringComparison.CurrentCultureIgnoreCase))
                        {
                            if (debug)
                            {
                                buffer.Append(buildString(" ", level + 1) + "--> aggregate function " + func.ToString() + "\r\n");
                                for (int i = 0; i < select.tables.Count; i++)
                                {
                                    if (select.tables.getTable(i).Subquery == null)
                                    {
                                        buffer.Append(buildString(" ", level + 1) + "--> table " + SQLUtil.trimObjectName(select.tables.getTable(i).FullNameWithAliasString) + "\r\n");
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
                                for (int k = 0; k < func.Args.Count; k++)
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

                            if (argCount == 0 && !"ROW_NUMBER".Equals(func.FunctionName.ToString(), StringComparison.CurrentCultureIgnoreCase))
                            {

                                Tuple<long, long> point = new Tuple<long, long>((int)func.endToken.lineNo, (int)func.endToken.columnNo);
                                if (func.Args != null && func.Args.Count > 0)
                                {
                                    for (int k = 0; k < func.Args.Count; k++)
                                    {
                                        TExpression expr = func.Args.getExpression(k);
                                        if (expr.ToString().Trim().Equals("*"))
                                        {
                                            point = new Tuple<long, long>((int)expr.startToken.lineNo, (int)expr.startToken.columnNo);
                                            break;
                                        }
                                    }
                                }
                                if (currentSource!=null && dependMap.ContainsKey(currentSource))
                                {

                                    if (currentClauseMap.ContainsKey(select))
                                    {
                                        dependMap[currentSource].Add(new TResultEntry(this, select.tables.getTable(0), "*", currentClauseMap[select], point));
                                    }
                                    else if (select is TSelectSqlStatement)
                                    {
                                        dependMap[currentSource].Add(new TResultEntry(this, select.tables.getTable(0), "*", ClauseType.select, point));
                                    }
                                    else
                                    {
                                        dependMap[currentSource].Add(new TResultEntry(this, select.tables.getTable(0), "*", ClauseType.undefine, point));
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
            if (level == 0)
            {
                if (currentSource == null)
                {
                    currentSource = getCurrentSource(field.Expr);
                    if (currentSource!=null && !dependMap.ContainsKey(currentSource))
                        dependMap[currentSource] = new List<TResultEntry>();
                }

                if (isTopSelectStmt(select) && !ignoreTopSelect_Renamed)
                {
                    TTable table = new TTable();
                    table.TableName = getVirtualTable(select);
                    TColumn column = new TColumn();
                    if (parentAlias != null)
                    {
                        column.linkColumnName = parentAlias.AliasDisplayName;
                        column.columnName = SQLUtil.trimObjectName(currentSource);
                    }
                    else
                    {
                        column.linkColumnName = field.AliasClause != null ? field.AliasClause.ToString() : (isNotEmpty(field.ColumnNameOnly) ? field.ColumnNameOnly : field.ToString());
                        column.columnName = SQLUtil.trimObjectName(currentSource);
                    }
                    if (parentAlias != null)
                    {
                        column.location = parentAlias.location;

                        column.offset = long.Parse(parentAlias.columnHighlightInfo.Split(new char[] { ',' })[0]);
                        column.length = long.Parse(parentAlias.columnHighlightInfo.Split(new char[] { ',' })[1]);
                    }
                    else if (field.AliasClause != null)
                    {
                        column.location = new Tuple<long, long>((int)field.AliasClause.startToken.lineNo, (int)field.AliasClause.startToken.columnNo);

                        column.offset = field.AliasClause.startToken.offset;
                        column.length = field.AliasClause.AliasName.ToString().Length;
                    }
                    else if (isNotEmpty(field.ColumnNameOnly))
                    {
                        column.location = new Tuple<long, long>((int)field.FieldAttr.startToken.lineNo, (int)field.FieldAttr.startToken.columnNo);

                        column.offset = field.FieldAttr.startToken.offset;
                        column.length = field.FieldAttr.ToString().Length;
                    }
                    else
                    {
                        column.location = new Tuple<long, long>((int)field.startToken.lineNo, (int)field.startToken.columnNo);

                        column.offset = field.startToken.offset;
                        column.length = field.ToString().Length;
                    }

                    column.clauseType = ClauseType.topselect;
                    TResultEntry resultEntry = new TResultEntry(this, table, column, column.columnName, ClauseType.topselect, column.location);
                    dependMap[currentSource].Add(resultEntry);
                }
                else if (select.ParentStmt is TCreateViewSqlStatement)
                {
                    TCreateViewSqlStatement createView = (TCreateViewSqlStatement)select.ParentStmt;
                    TTable table = new TTable();
                    table.TableName = createView.ViewName;
                    TColumn column = new TColumn();
                    if (parentAlias != null)
                    {
                        column.linkColumnName = parentAlias.AliasDisplayName;
                        column.columnName = SQLUtil.trimObjectName(currentSource);
                    }
                    else
                    {
                        column.linkColumnName = field.AliasClause != null ? field.AliasClause.ToString() : (isNotEmpty(field.ColumnNameOnly) ? field.ColumnNameOnly : field.ToString());
                        column.columnName = SQLUtil.trimObjectName(currentSource);
                    }
                    if (parentAlias != null)
                    {
                        column.location = parentAlias.location;
                    }
                    else if (field.AliasClause != null)
                    {
                        column.location = new Tuple<long, long>((int)field.AliasClause.startToken.lineNo, (int)field.AliasClause.startToken.columnNo);
                    }
                    else if (createView.ViewAliasClause != null)
                    {
                        for (int i = 0; i < select.ResultColumnList.Count; i++)
                        {
                            if (select.ResultColumnList.getResultColumn(i).Equals(field))
                            {
                                TViewAliasItem item = createView.ViewAliasClause.ViewAliasItemList.getViewAliasItem(i);
                                column.location = new Tuple<long, long>((int)item.startToken.lineNo, (int)item.startToken.columnNo);
                            }
                        }
                    }
                    else if (isNotEmpty(field.ColumnNameOnly))
                    {
                        column.location = new Tuple<long, long>((int)field.FieldAttr.endToken.lineNo, (int)field.FieldAttr.endToken.columnNo);
                    }
                    else
                    {
                        column.location = new Tuple<long, long>((int)createView.ViewName.endToken.lineNo, (int)createView.ViewName.endToken.columnNo);
                    }
                    column.offset = createView.ViewName.endToken.offset;
                    column.length = createView.ViewName.endToken.astext.Length;
                    column.clauseType = ClauseType.createview;
                    TResultEntry resultEntry = new TResultEntry(this, table, column, column.columnName, ClauseType.createview, column.location);
                    dependMap[currentSource].Add(resultEntry);
                }
                else if (select.ParentStmt is TCreateTableSqlStatement && ((TCreateTableSqlStatement)select.ParentStmt).SubQuery != null)
                {
                    TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)select.ParentStmt;
                    TTable table = new TTable();
                    table.TableName = createTable.TableName;
                    TColumn column = new TColumn();
                    if (parentAlias != null)
                    {
                        column.linkColumnName = parentAlias.AliasDisplayName;
                        column.columnName = SQLUtil.trimObjectName(currentSource);
                    }
                    else
                    {
                        column.linkColumnName = field.AliasClause != null ? field.AliasClause.ToString() : (isNotEmpty(field.ColumnNameOnly) ? field.ColumnNameOnly : field.ToString());
                        column.columnName = SQLUtil.trimObjectName(currentSource);
                    }
                    if (parentAlias != null)
                    {
                        column.location = parentAlias.location;
                    }
                    else if (field.AliasClause != null)
                    {
                        column.location = new Tuple<long, long>((int)field.AliasClause.startToken.lineNo, (int)field.AliasClause.startToken.columnNo);
                    }
                    else if (createTable.ColumnList != null && createTable.ColumnList.Count > 0)
                    {
                        for (int i = 0; i < select.ResultColumnList.Count; i++)
                        {
                            if (select.ResultColumnList.getResultColumn(i).Equals(field))
                            {
                                TColumnDefinition item = createTable.ColumnList.getColumn(i);
                                column.location = new Tuple<long, long>((int)item.startToken.lineNo, (int)item.startToken.columnNo);
                            }
                        }
                    }
                    else if (isNotEmpty(field.ColumnNameOnly))
                    {
                        column.location = new Tuple<long, long>((int)field.FieldAttr.endToken.lineNo, (int)field.FieldAttr.endToken.columnNo);
                    }
                    else
                    {
                        column.location = new Tuple<long, long>((int)createTable.TableName.endToken.lineNo, (int)createTable.TableName.endToken.columnNo);
                    }
                    column.offset = createTable.TableName.endToken.offset;
                    column.length = createTable.TableName.endToken.astext.Length;
                    column.clauseType = ClauseType.createtable;
                    TResultEntry resultEntry = new TResultEntry(this, table, column, column.columnName, ClauseType.createtable, column.location);
                    dependMap[currentSource].Add(resultEntry);
                }
                else if (select is TUpdateSqlStatement && (field.Expr.LeftOperand == null || field.Expr.LeftOperand.ExprList == null))
                {
                    TUpdateSqlStatement update = (TUpdateSqlStatement)select;
                    TTable table = new TTable();
                    if (update.TargetTable.LinkTable != null)
                    {
                        table.TableName = update.TargetTable
                                .LinkTable
                                .TableName;
                        TAliasClause alias = new TAliasClause();
                        alias.AliasName = update.TargetTable.TableName;
                        table.AliasClause = (alias);
                    }
                    else
                    {
                        table.TableName = update.TargetTable.TableName;
                    }
                    TColumn column = new TColumn();
                    TExpression columnExpression = getColumnExpression(field.Expr);
                    if (parentAlias != null)
                    {
                        column.linkColumnName = parentAlias.AliasDisplayName;
                        column.columnName = SQLUtil.trimObjectName(currentSource);
                    }
                    else
                    {
                        column.linkColumnName = columnExpression.ObjectOperand != null ? columnExpression.ObjectOperand
                            .ColumnNameOnly
                            : columnExpression.ToString();
                        column.columnName = SQLUtil.trimObjectName(currentSource);
                    }
                    if (parentAlias != null)
                    {
                        column.location = parentAlias.location;

                        column.offset = long.Parse(parentAlias.columnHighlightInfo.Split(new char[] { ',' })[0]);
                        column.length = long.Parse(parentAlias.columnHighlightInfo.Split(new char[] { ',' })[1]);
                    }
                    else
                    {
                        column.location = new Tuple<long, long>(columnExpression.getStartToken().lineNo,
                            columnExpression.getStartToken().columnNo);

                        column.offset = columnExpression.getStartToken().offset;
                        column.length = columnExpression.ToString().Length;
                    }
                    column.clauseType = ClauseType.updateset;
                    TResultEntry resultEntry = new TResultEntry(this, table, column, column.columnName, ClauseType.updateset, column.location);
                    dependMap[currentSource].Add(resultEntry);
                }
                else if (select is TMergeSqlStatement)
                {
                    TMergeSqlStatement merge = (TMergeSqlStatement)select;
                    TTable table = new TTable();
                    table.TableName = findTableName(merge.TargetTable);
                    TColumn column = new TColumn();
                    if (parentAlias != null)
                    {
                        column.linkColumnName = parentAlias.AliasDisplayName;
                        column.columnName = SQLUtil.trimObjectName(currentSource);
                    }
                    else
                    {
                        column.linkColumnName = field.Expr.LeftOperand.ObjectOperand != null ? field.Expr.LeftOperand.ObjectOperand.ColumnNameOnly.ToString() : field.Expr.LeftOperand.ToString();
                        column.columnName = SQLUtil.trimObjectName(currentSource);
                    }
                    if (parentAlias != null)
                    {
                        column.location = parentAlias.location;

                        column.offset = long.Parse(parentAlias.columnHighlightInfo.Split(new char[] { ',' })[0]);
                        column.length = long.Parse(parentAlias.columnHighlightInfo.Split(new char[] { ',' })[1]);
                    }
                    else
                    {
                        column.location = new Tuple<long, long>((int)field.Expr.LeftOperand.startToken.lineNo, (int)field.Expr.LeftOperand.startToken.columnNo);

                        column.offset = field.Expr.LeftOperand.startToken.offset;
                        column.length = field.Expr.LeftOperand.ToString().Length;
                    }
                    column.clauseType = ClauseType.mergeset;
                    TResultEntry resultEntry = new TResultEntry(this, table, column, column.columnName, ClauseType.mergeset, column.location);
                    dependMap[currentSource].Add(resultEntry);
                }
                else if (select.ParentStmt is TInsertSqlStatement)
                {
                    TInsertSqlStatement insertStmt = (TInsertSqlStatement)select.ParentStmt;

                    if (insertStmt.ColumnList != null && select.ResultColumnList != null)
                    {
                        if (insertStmt.ColumnList.Count == select.ResultColumnList.Count)
                        {
                            for (int i = 0; i < insertStmt.ColumnList.Count; i++)
                            {
                                if (select.ResultColumnList.getResultColumn(i).Equals(field))
                                {
                                    TTable table = new TTable();
                                    table.TableName = insertStmt.TargetTable.TableName;
                                    TColumn column = new TColumn();

                                    column.columnName = SQLUtil.trimObjectName(currentSource);

                                    TObjectName columnName = insertStmt.ColumnList.getObjectName(i);

                                    column.linkColumnName = (isNotEmpty(columnName.ColumnNameOnly) ? columnName.ColumnNameOnly : columnName.ToString());

                                    column.location = new Tuple<long, long>((int)columnName.startToken.lineNo, (int)columnName.startToken.columnNo);
                                    column.offset = columnName.startToken.offset;
                                    column.length = columnName.ToString().Length;
                                    column.clauseType = ClauseType.insert;
                                    TResultEntry resultEntry = new TResultEntry(this, table, column, column.columnName, ClauseType.insert, column.location);
                                    dependMap[currentSource].Add(resultEntry);
                                }
                            }
                        }
                        else if ("*".Equals(select.ResultColumnList.getResultColumn(0).ToString()))
                        {
                            for (int i = 0; i < insertStmt.ColumnList.Count; i++)
                            {
                                if (select.ResultColumnList.getResultColumn(0).Equals(field))
                                {
                                    TTable table = new TTable();
                                    table.TableName = insertStmt.TargetTable.TableName;
                                    TColumn column = new TColumn();

                                    column.columnName = SQLUtil.trimObjectName(currentSource);

                                    TObjectName columnName = insertStmt.ColumnList.getObjectName(i);

                                    column.linkColumnName = (isNotEmpty(columnName.ColumnNameOnly) ? columnName.ColumnNameOnly : columnName.ToString());

                                    column.location = new Tuple<long, long>((int)columnName.startToken.lineNo, (int)columnName.startToken.columnNo);
                                    column.offset = columnName.startToken.offset;
                                    column.length = columnName.ToString().Length;
                                    column.clauseType = ClauseType.insert;
                                    TResultEntry resultEntry = new TResultEntry(this, table, column, column.columnName, ClauseType.insert, column.location);
                                    dependMap[currentSource].Add(resultEntry);
                                }
                            }
                        }
                    }
                    else if (insertStmt.ColumnList == null)
                    {
                        for (int i = 0; i < select.ResultColumnList.Count; i++)
                        {
                            if (select.ResultColumnList.getResultColumn(i).Equals(field))
                            {
                                if (insertStmt.TargetTable.Subquery != null)
                                {
                                    continue;
                                }
                                TableMetaData tableMetaData = getTableMetaData(insertStmt.TargetTable.TableName);

                                IList<ColumnMetaData> columns = tableColumns[tableMetaData];
                                if (columns != null && columns.Count == select.ResultColumnList.Count)
                                {
                                    TTable table = new TTable();
                                    table.TableName = insertStmt.TargetTable.TableName;
                                    TColumn column = new TColumn();

                                    column.columnName = SQLUtil.trimObjectName(currentSource);

                                    column.linkColumnName = columns[i].Name;

                                    column.location = new Tuple<long, long>((int)field.startToken.lineNo, (int)field.startToken.columnNo);
                                    column.offset = field.startToken.offset;
                                    column.length = field.ToString().Length;
                                    column.clauseType = ClauseType.insert;
                                    TResultEntry resultEntry = new TResultEntry(this, table, column, column.columnName, ClauseType.insert, column.location);
                                    dependMap[currentSource].Add(resultEntry);

                                }
                                else if (columns == null || columns.Count == 0)
                                {
                                    TTable table = new TTable();
                                    table.TableName = insertStmt.TargetTable.TableName;
                                    TColumn column = new TColumn();

                                    column.columnName = SQLUtil.trimObjectName(currentSource);

                                    if (field.ColumnAlias != null && !"".Equals(field.ColumnAlias))
                                    {
                                        column.linkColumnName = field.ColumnAlias;
                                    }
                                    else if (field.ColumnNameOnly != null && !"".Equals(field.ColumnNameOnly))
                                    {
                                        column.linkColumnName = field.ColumnNameOnly;
                                    }
                                    else
                                    {
                                        continue;
                                    }

                                    column.location = new Tuple<long, long>(field.getStartToken().lineNo,
                                            field.getStartToken().columnNo);
                                    column.offset = field.getStartToken().offset;
                                    column.length = field.ToString().Length;
                                    column.clauseType = ClauseType.insert;
                                    TResultEntry resultEntry = new TResultEntry(this, table,
                                            column,
                                            column.columnName,
                                            ClauseType.insert,
                                            column.location);
                                    dependMap[currentSource]
                                            .Add(resultEntry);
                                }
                            }
                        }
                    }
                }
            }

            if (select.ParentStmt is TUpdateSqlStatement)
            {
                TUpdateSqlStatement update = (TUpdateSqlStatement)select.ParentStmt;
                TTable table = new TTable();
                table.TableName = update.TargetTable.TableName;
                TColumn column = new TColumn();

                for (int j = 0; j < update.ResultColumnList.Count; j++)
                {
                    TResultColumn resultColumn = update.ResultColumnList.getResultColumn(j);
                    TExpression leftExpr = resultColumn.Expr.LeftOperand;
                    TExpression rightExpr = resultColumn.Expr.RightOperand;
                    if (leftExpr.ExpressionType == EExpressionType.list_t)
                    {
                        TExpressionList setExpression = leftExpr.ExprList;

                        TCustomSqlStatement stmt = select;
                        if (rightExpr.ExpressionType == EExpressionType.subquery_t)
                        {
                            stmt = rightExpr.SubQuery;
                        }

                        for (int k = 0; k < stmt.ResultColumnList.size(); k++)
                        {
                            if (field == stmt.ResultColumnList.getResultColumn(k))
                            {
                                column.linkColumnName = setExpression.getExpression(k).ToString();
                                column.columnName = SQLUtil.trimObjectName(currentSource);

                                column.location = new Tuple<long, long>((int)setExpression.getExpression(k).startToken.lineNo, (int)setExpression.getExpression(k).startToken.columnNo);

                                column.offset = setExpression.getExpression(k).startToken.offset;
                                column.length = setExpression.getExpression(k).ToString().Length;
                                column.clauseType = ClauseType.updateset;
                                TResultEntry resultEntry = new TResultEntry(this, table, column, column.columnName, ClauseType.updateset, column.location);
                                dependMap[currentSource].Add(resultEntry);
                            }
                        }
                    }
                }
            }
            return ret;
        }

        private TObjectName findTableName(TTable targetTable)
        {
            if (targetTable.TableName != null)
                return targetTable.TableName;
            else if (targetTable.Subquery != null
                && targetTable.Subquery.tables != null
                && targetTable.Subquery.tables.Count > 0)
                return findTableName(targetTable.Subquery.tables.getTable(0));
            return null;
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

        private String getCurrentSource(TExpression expr)
        {
            if (expr.ExpressionType == EExpressionType.simple_object_name_t)
            {
                return expr.ToString();
            }
            else if (expr.LeftOperand != null)
            {
                return getCurrentSource(expr.LeftOperand);
            }
            else
                return null;
        }

        private bool isTopSelectStmt(TCustomSqlStatement select)
        {
            if (!(select is TSelectSqlStatement))
            {
                return false;
            }
            else
            {
                if (select.ParentStmt == null)
                {
                    return true;
                }
                TCustomSqlStatement parent = select.ParentStmt;
                if (!(parent is TSelectSqlStatement))
                {
                    return false;
                }
                TSelectSqlStatement parentSelectSqlStatement = (TSelectSqlStatement)parent;
                if (parentSelectSqlStatement.SetOperatorType == ESetOperatorType.none)
                {
                    return false;
                }
                if (parentSelectSqlStatement.LeftStmt == select || parentSelectSqlStatement.RightStmt == select)
                {
                    return isTopSelectStmt(parentSelectSqlStatement);
                }
                else
                {
                    return false;
                }
            }
        }

        private TCustomSqlStatement getTopSelect(TCustomSqlStatement select)
        {
            if (!(select is TSelectSqlStatement))
            {
                return null;
            }
            else
            {
                if (select.ParentStmt == null)
                {
                    return select;
                }
                TCustomSqlStatement parent = select.ParentStmt;
                if (!(parent is TSelectSqlStatement))
                {
                    return null;
                }
                TSelectSqlStatement parentSelectSqlStatement = (TSelectSqlStatement)parent;
                if (parentSelectSqlStatement.SetOperatorType == ESetOperatorType.none)
                {
                    return null;
                }
                if (parentSelectSqlStatement.LeftStmt == select || parentSelectSqlStatement.RightStmt == select)
                {
                    return getTopSelect(parentSelectSqlStatement);
                }
                else
                {
                    return null;
                }
            }
        }

        private TObjectName getVirtualTable(TCustomSqlStatement stmt)
        {
            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final gudusoft.gsqlparser.TSourceToken virtualToken = new gudusoft.gsqlparser.TSourceToken();
            TSourceToken virtualToken = new TSourceToken();
            virtualToken.String = string.ReferenceEquals(virtualTable, null) ? SQLUtil.generateVirtualTableName(getTopSelect(stmt)) : virtualTable;
            TObjectName objectName = new TObjectNameAnonymousInnerClass(this, virtualToken);

            objectName.init(virtualToken, null);
            return objectName;
        }

        private class TObjectNameAnonymousInnerClass : TObjectName
        {
            private readonly ColumnImpact outerInstance;

            private TSourceToken virtualToken;

            public TObjectNameAnonymousInnerClass(ColumnImpact outerInstance, TSourceToken virtualToken)
            {
                this.outerInstance = outerInstance;
                this.virtualToken = virtualToken;
            }


            public override string ToString()
            {
                return virtualToken.ToString();
            }
        }

        private void getSelectSqlStatements(TSelectSqlStatement select, IList<TSelectSqlStatement> stmtList)
        {
            if (select.SetOperatorType != ESetOperatorType.none)
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
            if (lztable.Subquery == null && lztable.TableName != null && lztable.FullName != null)
            {
                table.tableName = SQLUtil.trimObjectName(getTableName(lztable));
                if (lztable.TableName.ToString().IndexOf(".", StringComparison.Ordinal) > 0)
                {
                    table.prefixName = SQLUtil.trimObjectName(lztable.TableName.ToString().Substring(0, lztable.FullName.IndexOf('.')));
                }
            }

            if (lztable.AliasClause != null)
            {
                table.tableAlias = SQLUtil.trimObjectName(lztable.AliasClause.ToString());
            }
            return table;
        }

        public virtual ColumnImpactModel generateModel()
        {
            ColumnImpactModel model = new ColumnImpactModel();
            if (!Sucess || ImpactResult.Trim().Length == 0)
            {
                return model;
            }
            columnImpactResult result = XML2Model.loadXML(ImpactResult);
            targetColumn[] columns = result.columns;
            if (columns != null)
            {
                for (int i = 0; i < columns.Length; i++)
                {
                    targetColumn field = columns[i];
                    AliasModel alias = null;
                    FieldModel fieldModel = null;
                    if (!string.ReferenceEquals(field.alias, null))
                    {
                        alias = new AliasModel();
                        alias.Name = field.alias;

                        FieldModel aliasField = new FieldModel();
                        int index = field.name.LastIndexOf('.');
                        if (index != -1)
                        {
                            aliasField.Name = field.name.Substring(index + 1);
                            aliasField.Schema = field.name.Substring(0, index);
                        }
                        else
                        {
                            aliasField.Name = field.name;
                        }
                        alias.Field = aliasField;

                        try
                        {
                            alias.HighlightInfo = field.aliasHighlightInfo;
                            aliasField.HighlightInfo = field.columnHighlightInfo;
                            alias.Coordinate = field.aliasCoordinate;
                            aliasField.Coordinate = field.coordinate;
                        }
                        catch (Exception)
                        {

                        }

                        model.addAlias(alias);
                    }
                    else
                    {
                        fieldModel = new FieldModel();
                        int index = field.name.LastIndexOf('.');
                        if (index != -1)
                        {
                            fieldModel.Name = field.name.Substring(index + 1);
                            fieldModel.Schema = field.name.Substring(0, index);
                        }
                        else
                        {
                            fieldModel.Name = field.name;
                        }
                        fieldModel.HighlightInfo = field.columnHighlightInfo;
                        fieldModel.Coordinate = field.coordinate;
                        model.addField(fieldModel);
                    }
                    sourceColumn[] sources = field.columns;
                    for (int j = 0; sources != null && j < sources.Length; j++)
                    {
                        sourceColumn source = sources[j];

                        if ("true".Equals(source.orphan))
                        {
                            continue;
                        }

                        if (!model.containsTable(source.tableOwner, source.tableName))
                        {
                            TableModel tableModel = new TableModel();
                            tableModel.Schema = source.tableOwner;
                            tableModel.Name = source.tableName;
                            tableModel.HighlightInfo = getTableHighlightInfo(result, tableModel);
                            tableModel.Coordinate = getTableCoordinate(result, tableModel);
                            model.addTable(tableModel);
                        }
                        TableModel table = model.getTable(source.tableOwner, source.tableName);

                        if (string.ReferenceEquals(source.name, null) || "".Equals(source.name))
                        {
                            ReferenceModel @ref = new ReferenceModel();
                            @ref.Table = table;
                            if (!string.ReferenceEquals(source.clause, null) && source.clause.ToUpper().IndexOf("SELECT", StringComparison.Ordinal) != -1)
                            {
                                @ref.Clause = Clause.SELECT;
                            }
                            if (!string.ReferenceEquals(source.clause, null) && source.clause.ToUpper().IndexOf("UPDATE", StringComparison.Ordinal) != -1)
                            {
                                @ref.Clause = Clause.UPDATE;
                            }
                            if (!string.ReferenceEquals(source.clause, null) && source.clause.ToUpper().IndexOf("MERGE", StringComparison.Ordinal) != -1)
                            {
                                @ref.Clause = Clause.MERGE;
                            }
                            if (alias != null)
                            {
                                @ref.ReferenceType = ReferenceModel.TYPE_ALIAS_TABLE;
                                @ref.Alias = alias;
                                model.addReference(@ref);

                            }
                            else if (fieldModel != null)
                            {
                                @ref.ReferenceType = ReferenceModel.TYPE_FIELD_TABLE;
                                @ref.Field = fieldModel;
                                model.addReference(@ref);
                            }
                            ColumnModel column = new ColumnModel("*", table);
                            column.HighlightInfos = source.highlightInfos;
                            column.Coordinate = source.coordinate;
                            if (!table.containsColumn(column))
                            {
                                table.addColumn(column);
                            }
                            @ref.Column = column;
                        }
                        else
                        {
                            ColumnModel column = new ColumnModel(source.name, table);
                            column.HighlightInfos = source.highlightInfos;
                            column.Coordinate = source.coordinate;
                            if (!table.containsColumn(column))
                            {
                                table.addColumn(column);
                            }
                            ReferenceModel @ref = new ReferenceModel();
                            if (!string.ReferenceEquals(source.clause, null) && source.clause.ToUpper().IndexOf("SELECT", StringComparison.Ordinal) != -1)
                            {
                                @ref.Clause = Clause.SELECT;
                            }

                            if (!string.ReferenceEquals(source.clause, null) && source.clause.ToUpper().IndexOf("UPDATE", StringComparison.Ordinal) != -1)
                            {
                                @ref.Clause = Clause.UPDATE;
                            }

                            if (!string.ReferenceEquals(source.clause, null) && source.clause.ToUpper().IndexOf("MERGE", StringComparison.Ordinal) != -1)
                            {
                                @ref.Clause = Clause.MERGE;
                            }
                            @ref.Column = column;
                            if (alias != null)
                            {
                                @ref.ReferenceType = ReferenceModel.TYPE_ALIAS_COLUMN;
                                @ref.Alias = alias;
                                model.addReference(@ref);
                            }
                            else if (fieldModel != null)
                            {
                                @ref.ReferenceType = ReferenceModel.TYPE_FIELD_COLUMN;
                                @ref.Field = fieldModel;
                                model.addReference(@ref);
                            }
                        }
                    }
                }
            }
            return model;
        }

        private string getTableFullName(table table)
        {
            if (table.owner != null && table.owner.Trim().Length > 0)
            {
                return table.owner + "." + table.name;
            }
            else
            {
                return table.name;
            }
        }

        private string getTableHighlightInfo(columnImpactResult result, TableModel tableModel)
        {
            table[] tables = result.tables;
            for (int i = 0; i < tables.Length; i++)
            {
                table table = tables[i];
                if (getTableFullName(table).Equals(tableModel.FullName))
                {
                    return table.highlightInfo;
                }
            }
            return null;
        }

        private string getTableCoordinate(columnImpactResult result, TableModel tableModel)
        {
            table[] tables = result.tables;
            for (int i = 0; i < tables.Length; i++)
            {
                table table = tables[i];
                if (getTableFullName(table).Equals(tableModel.FullName))
                {
                    return table.coordinate;
                }
            }
            return null;
        }

    }

}