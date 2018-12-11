using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace gudusoft.gsqlparser.demos.gettablecolumns
{
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.nodes;
    using gudusoft.gsqlparser.stmt;
    using gudusoft.gsqlparser.stmt.mssql;
    using gudusoft.gsqlparser.stmt.oracle;
    using System.IO;

    internal enum ClauseType
    {
        @where,
        connectby,
        startwith,
        orderby,
        casewhen,
        casethen
    }

    public class JoinCondition
    {

        public string lefttable, righttable, leftcolumn, rightcolumn,
            leftTableLocation, rightTableLocation, leftColumnLocation,
            rightColumnLocation;
        public IList<TCustomSqlStatement> sql = new List<TCustomSqlStatement>();

        public override int GetHashCode()
        {
            int hashCode = 0;
            if (!string.ReferenceEquals(lefttable, null))
            {
                hashCode += lefttable.GetHashCode();
            }
            if (!string.ReferenceEquals(righttable, null))
            {
                hashCode += righttable.GetHashCode();
            }
            if (!string.ReferenceEquals(leftcolumn, null))
            {
                hashCode += leftcolumn.GetHashCode();
            }
            if (!string.ReferenceEquals(rightcolumn, null))
            {
                hashCode += rightcolumn.GetHashCode();
            }

            foreach (TCustomSqlStatement stmt in sql)
            {
                hashCode += stmt.GetHashCode();
            }

            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (!(obj is JoinCondition))
            {
                return false;
            }

            JoinCondition join = (JoinCondition)obj;

            if (!string.ReferenceEquals(this.leftcolumn, null) && !this.leftcolumn.Equals(join.leftcolumn))
            {
                return false;
            }
            if (!string.ReferenceEquals(this.rightcolumn, null) && !this.rightcolumn.Equals(join.rightcolumn))
            {
                return false;
            }
            if (!string.ReferenceEquals(this.lefttable, null) && !this.lefttable.Equals(join.lefttable))
            {
                return false;
            }
            if (!string.ReferenceEquals(this.righttable, null) && !this.righttable.Equals(join.righttable))
            {
                return false;
            }

            if (!string.ReferenceEquals(join.righttable, null) && !join.righttable.Equals(this.righttable))
            {
                return false;
            }
            if (!string.ReferenceEquals(join.lefttable, null) && !join.lefttable.Equals(this.lefttable))
            {
                return false;
            }
            if (!string.ReferenceEquals(join.rightcolumn, null) && !join.rightcolumn.Equals(this.rightcolumn))
            {
                return false;
            }
            if (!string.ReferenceEquals(join.leftcolumn, null) && !join.leftcolumn.Equals(this.leftcolumn))
            {
                return false;
            }

            if (join.sql.Count != this.sql.Count)
            {
                return false;
            }
            for (int i = 0; i < join.sql.Count; i++)
            {
                if (!join.sql[i].Equals(this.sql[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class TColumn
    {

        public IList<string> tableNames = new List<string>();
        public string columnName;
        public string columnPrex;
        public string columnAlias;
        public string columnLocation;
        public string tableLocation;

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

    internal class TTable
    {

        public string tableName;
        public string prefixName;
        public string tableAlias;
        public string tableLocation;
    }

    internal class joinConditonsInExpr : IExpressionVisitor
    {

        private TExpression expr;
        private joinRelationAnalyze analysis;
        private TCustomSqlStatement stmt;

        public joinConditonsInExpr(joinRelationAnalyze analysis, TExpression expr, TCustomSqlStatement stmt)
        {
            this.stmt = stmt;
            this.analysis = analysis;
            this.expr = expr;
        }

        internal virtual bool is_compare_condition(EExpressionType t)
        {
            return ((t == EExpressionType.simple_comparison_t) || (t == EExpressionType.group_comparison_t) || (t == EExpressionType.in_t));
        }

        private string getExpressionTable(TExpression expr)
        {
            if (expr.ObjectOperand != null)
            {
                return expr.ObjectOperand.ObjectString;
            }
            else if (expr.LeftOperand != null && expr.LeftOperand.ObjectOperand != null)
            {
                return expr.LeftOperand.ObjectOperand.ObjectString;
            }
            else if (expr.RightOperand != null && expr.RightOperand.ObjectOperand != null)
            {
                return expr.RightOperand.ObjectOperand.ObjectString;
            }
            else
            {
                return null;
            }
        }

        private string getExpressionTableLocation(TExpression expr)
        {
            TSourceToken tableToken = null;
            if (expr.ObjectOperand != null)
            {
                tableToken = expr.ObjectOperand.TableToken;
            }
            else if (expr.LeftOperand != null
                    && expr.LeftOperand.ObjectOperand != null)
            {
                tableToken = expr.LeftOperand
                        .ObjectOperand
                        .TableToken;
            }
            else if (expr.RightOperand != null
                    && expr.RightOperand.ObjectOperand != null)
            {
                tableToken = expr.RightOperand
                        .ObjectOperand
                        .TableToken;
            }
            if (tableToken != null)
            {
                return "(" + tableToken.lineNo + "," + tableToken.columnNo + ")";
            }
            return null;
        }

        public virtual bool exprVisit(TParseTreeNode pnode, bool flag)
        {
            TExpression lcexpr = (TExpression)pnode;

            TExpression slexpr, srexpr, lc_expr = lcexpr;

            if (is_compare_condition(lc_expr.ExpressionType))
            {
                slexpr = lc_expr.LeftOperand;
                srexpr = lc_expr.RightOperand;

                if (srexpr.FunctionCall != null && srexpr.FunctionCall.FunctionName.ToString().Equals("ISNULL", StringComparison.CurrentCultureIgnoreCase))
                {
                    TExpressionList list = srexpr.FunctionCall.Args;
                    for (int i = 0; i < list.size(); i++)
                    {
                        dealCompareCondition(slexpr, list.getExpression(i));
                    }
                }
                else
                {
                    dealCompareCondition(slexpr, srexpr);
                }
            }

            if (lcexpr.ExpressionType == EExpressionType.function_t)
            {
                TFunctionCall func = (TFunctionCall)lcexpr.FunctionCall;
                if (func.Args != null)
                {
                    for (int k = 0; k < func.Args.size(); k++)
                    {
                        TExpression expr = func.Args.getExpression(k);
                        expr.inOrderTraverse(this);
                    }
                }
                if (func.AnalyticFunction != null)
                {
                    TParseTreeNodeList list = func.AnalyticFunction.PartitionBy_ExprList;
                    searchJoinInList(list, stmt);

                    if (func.AnalyticFunction.OrderBy != null)
                    {
                        list = func.AnalyticFunction.OrderBy.Items;
                        searchJoinInList(list, stmt);
                    }
                }

            }
            else if (lcexpr.ExpressionType == EExpressionType.subquery_t)
            {
                if (lcexpr.SubQuery is TSelectSqlStatement)
                {
                    TSelectSqlStatement query = lcexpr.SubQuery;
                    analysis.searchSubQuery(query);
                }
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
                searchJoinInList(list, stmt);
            }
            else if (lcexpr.ExpressionType == EExpressionType.exists_t)
            {
                if (lcexpr.RightOperand != null && lcexpr.RightOperand.SubQuery != null)
                {
                    TSelectSqlStatement query = lcexpr.RightOperand.SubQuery;
                    analysis.searchSubQuery(query);
                }
            }
            return true;
        }

        private void dealCompareCondition(TExpression slexpr, TExpression srexpr)
        {
            if (((slexpr.ExpressionType == EExpressionType.simple_object_name_t) || (slexpr.OracleOuterJoin) || (srexpr.OracleOuterJoin && slexpr.ExpressionType == EExpressionType.simple_constant_t)) && ((srexpr.ExpressionType == EExpressionType.simple_object_name_t) || (srexpr.OracleOuterJoin) || (slexpr.OracleOuterJoin && srexpr.ExpressionType == EExpressionType.simple_constant_t) || (slexpr.OracleOuterJoin && srexpr.ExpressionType == EExpressionType.case_t)) || (slexpr.ExpressionType == EExpressionType.simple_object_name_t && srexpr.ExpressionType == EExpressionType.subquery_t) || (slexpr.ExpressionType == EExpressionType.subquery_t && srexpr.ExpressionType == EExpressionType.simple_object_name_t))
            {
                TExpression lattr = null, rattr = null;
                JoinCondition jr = new JoinCondition();
                jr.sql.Add(stmt);

                if (slexpr.OracleOuterJoin)
                {
                    lattr = slexpr;
                    jr.lefttable = lattr != null ? getExpressionTable(lattr) : null;
                    jr.leftTableLocation = lattr != null ? getExpressionTableLocation(lattr)
                        : null;

                    TSourceToken columnToken = getBeforeToken(lattr.getEndToken());
                    jr.leftcolumn = lattr != null ? columnToken.ToString() : null;
                    if (columnToken != null)
                    {
                        jr.leftColumnLocation = "("
                                + columnToken.lineNo
                                + ","
                                + columnToken.columnNo
                                + ")";
                    }
                }
                else if (slexpr.ExpressionType == EExpressionType.simple_object_name_t)
                {
                    lattr = slexpr;
                    jr.lefttable = lattr != null ? getExpressionTable(lattr) : null;
                    jr.leftTableLocation = lattr != null ? getExpressionTableLocation(lattr)
                        : null;

                    jr.leftcolumn = lattr != null ? lattr.getEndToken().ToString()
                            : null;

                    TSourceToken columnToken = lattr.getEndToken();
                    if (columnToken != null)
                    {
                        jr.leftColumnLocation = "("
                                + columnToken.lineNo
                                + ","
                                + columnToken.columnNo
                                + ")";
                    }
                }

                if (srexpr.OracleOuterJoin)
                {
                    rattr = srexpr;
                    jr.righttable = rattr != null ? getExpressionTable(rattr) : null;
                    jr.rightTableLocation = rattr != null ? getExpressionTableLocation(rattr)
                        : null;

                    TSourceToken columnToken = getBeforeToken(rattr.getEndToken());
                    jr.rightcolumn = rattr != null ? columnToken.ToString() : null;
                    if (columnToken != null)
                    {
                        jr.rightColumnLocation = "("
                                + columnToken.lineNo
                                + ","
                                + columnToken.columnNo
                                + ")";
                    }
                    if (slexpr.ExpressionType != EExpressionType.subquery_t)
                    {
                        analysis.joinRelationSet.Add(jr);
                    }
                }
                else if (srexpr.ExpressionType == EExpressionType.simple_object_name_t)
                {
                    rattr = srexpr;
                    jr.righttable = rattr != null ? getExpressionTable(rattr) : null;
                    jr.rightTableLocation = rattr != null ? getExpressionTableLocation(rattr)
                        : null;
                    jr.rightcolumn = rattr != null ? rattr.getEndToken()
                            .ToString() : null;

                    TSourceToken columnToken = rattr.getEndToken();
                    if (columnToken != null)
                    {
                        jr.rightColumnLocation = "("
                                + columnToken.lineNo
                                + ","
                                + columnToken.columnNo
                                + ")";
                    }
                    if (slexpr.ExpressionType != EExpressionType.subquery_t)
                    {
                        analysis.joinRelationSet.Add(jr);
                    }
                }
                else if (srexpr.ExpressionType == EExpressionType.case_t)
                {
                    TCaseExpression expr = srexpr.CaseExpression;

                    TWhenClauseItemList list = expr.WhenClauseItemList;
                    for (int i = 0; i < list.size(); i++)
                    {
                        TExpression thenexpr = ((TWhenClauseItem)list.getWhenClauseItem(i)).Return_expr;
                        if (thenexpr.ExpressionType == EExpressionType.simple_object_name_t)
                        {
                            rattr = thenexpr;
                        }
                        JoinCondition condtion = new JoinCondition();
                        condtion.leftcolumn = jr.leftcolumn;
                        condtion.lefttable = jr.lefttable;
                        condtion.leftTableLocation = jr.leftTableLocation;
                        condtion.leftColumnLocation = jr.leftColumnLocation;
                        condtion.sql = jr.sql;
                        condtion.righttable = rattr != null ? getExpressionTable(rattr) : null;
                        condtion.rightTableLocation = rattr != null ? getExpressionTableLocation(rattr)
                            : null;

                        if (rattr != null)
                        {
                            if (rattr.OracleOuterJoin)
                            {
                                TSourceToken columnToken = getBeforeToken(rattr.getEndToken());
                                condtion.rightcolumn = columnToken.ToString();
                                if (columnToken != null)
                                {
                                    jr.rightColumnLocation = "("
                                            + columnToken.lineNo
                                            + ","
                                            + columnToken.columnNo
                                            + ")";
                                }
                            }
                            else
                            {
                                condtion.rightcolumn = rattr.endToken.ToString();
                                TSourceToken columnToken = rattr.getEndToken();
                                if (columnToken != null)
                                {
                                    jr.rightColumnLocation = "("
                                            + columnToken.lineNo
                                            + ","
                                            + columnToken.columnNo
                                            + ")";
                                }
                            }
                        }
                        else
                        {
                            condtion.rightcolumn = null;
                        }

                        analysis.joinRelationSet.Add(condtion);
                    }
                    if (expr.Else_expr != null)
                    {
                        TExpression elseexpr = expr.Else_expr;
                        if (elseexpr.ExpressionType == EExpressionType.simple_object_name_t)
                        {
                            rattr = elseexpr;
                        }

                        JoinCondition condtion = new JoinCondition();
                        condtion.leftcolumn = jr.leftcolumn;
                        condtion.lefttable = jr.lefttable;
                        condtion.leftColumnLocation = jr.leftColumnLocation;
                        condtion.leftTableLocation = jr.leftTableLocation;
                        condtion.sql = jr.sql;
                        condtion.righttable = rattr != null ? getExpressionTable(rattr) : null;
                        if (rattr != null)
                        {
                            if (rattr.OracleOuterJoin)
                            {
                                TSourceToken columnToken = getBeforeToken(rattr.getEndToken());
                                condtion.rightcolumn = columnToken.ToString();
                                if (columnToken != null)
                                {
                                    jr.rightColumnLocation = "("
                                            + columnToken.lineNo
                                            + ","
                                            + columnToken.columnNo
                                            + ")";
                                }
                            }
                            else
                            {
                                condtion.rightcolumn = rattr.endToken.ToString();
                                TSourceToken columnToken = rattr.getEndToken();
                                if (columnToken != null)
                                {
                                    jr.rightColumnLocation = "("
                                            + columnToken.lineNo
                                            + ","
                                            + columnToken.columnNo
                                            + ")";
                                }
                            }
                        }
                        else
                        {
                            condtion.rightcolumn = null;
                        }
                        analysis.joinRelationSet.Add(condtion);
                    }
                }

                if (srexpr.ExpressionType == EExpressionType.subquery_t)
                {
                    TSelectSqlStatement subquery = (TSelectSqlStatement)srexpr.SubQuery;
                    addSubqueryJoin(jr, subquery, false);
                }

                if (slexpr.ExpressionType == EExpressionType.subquery_t)
                {
                    TSelectSqlStatement subquery = (TSelectSqlStatement)slexpr.SubQuery;
                    addSubqueryJoin(jr, subquery, true);
                }
            }
        }

        private TSourceToken getBeforeToken(TSourceToken token)
        {
            TSourceTokenList tokens = token.container;
            int index = token.posinlist;

            for (int i = index - 1; i >= 0; i--)
            {
                TSourceToken currentToken = tokens.get(i);
                if (currentToken.ToString().Trim().Length == 0)
                {
                    continue;
                }
                else
                {
                    return currentToken;
                }
            }
            return token;
        }

        private void addSubqueryJoin(JoinCondition jr, TSelectSqlStatement subquery, bool? isLeft)
        {
            if (subquery.CombinedQuery)
            {
                addSubqueryJoin(jr, subquery.LeftStmt, isLeft);
                addSubqueryJoin(jr, subquery.RightStmt, isLeft);
            }
            else
            {
                for (int i = 0; i < subquery.ResultColumnList.size(); i++)
                {
                    TResultColumn field = subquery.ResultColumnList.getResultColumn(i);
                    TColumn column = analysis.attrToColumn(field, subquery);
                    foreach (string tableName in column.tableNames)
                    {
                        JoinCondition condtion = new JoinCondition();
                        if (isLeft.Value)
                        {
                            condtion.rightcolumn = jr.rightcolumn;
                            condtion.righttable = jr.righttable;
                            condtion.rightColumnLocation = jr.rightColumnLocation;
                            condtion.rightTableLocation = jr.rightTableLocation;
                            condtion.sql.Add(stmt);
                            condtion.sql.Add(subquery);
                            condtion.lefttable = tableName;
                            condtion.leftcolumn = column.columnName;
                            condtion.rightColumnLocation = jr.rightColumnLocation;
                            condtion.rightTableLocation = jr.rightTableLocation;
                        }
                        else
                        {
                            condtion.leftcolumn = jr.leftcolumn;
                            condtion.leftColumnLocation = jr.leftColumnLocation;
                            condtion.lefttable = jr.lefttable;
                            condtion.leftTableLocation = jr.leftTableLocation;
                            condtion.sql.Add(stmt);
                            condtion.sql.Add(subquery);
                            condtion.righttable = tableName;
                            condtion.rightcolumn = column.columnName;
                            condtion.rightColumnLocation = column.columnLocation;
                            condtion.rightTableLocation = column.tableLocation;
                        }
                        analysis.joinRelationSet.Add(condtion);
                    }
                }
            }
        }

        private void searchJoinInList(TParseTreeNodeList list, TCustomSqlStatement stmt)
        {
            if (list != null)
            {
                for (int i = 0; i < list.size(); i++)
                {
                    IList<TExpression> exprList = new List<TExpression>();

                    if (list.getElement(i) is TOrderByItem)
                    {
                        exprList.Add((TExpression)((TOrderByItem)list.getElement(i)).SortKey);
                    }
                    else if (list.getElement(i) is TExpression)
                    {
                        exprList.Add((TExpression)list.getElement(i));
                    }
                    else if (list.getElement(i) is TWhenClauseItem)
                    {
                        exprList.Add(((TWhenClauseItem)list.getElement(i)).Comparison_expr);
                        exprList.Add(((TWhenClauseItem)list.getElement(i)).Return_expr);
                    }

                    foreach (TExpression lcexpr in exprList)
                    {
                        lcexpr.inOrderTraverse(this);
                    }
                }
            }
        }

        public virtual void searchExpression()
        {
            this.expr.inOrderTraverse(this);
        }
    }

    public class joinRelationAnalyze
    {
        private StringBuilder buffer = new StringBuilder();
        private Hashtable cteMap = new Hashtable();
        private Hashtable tableAliasMap = new Hashtable();
        private IList<TCustomSqlStatement> searchInSubQuerys = new List<TCustomSqlStatement>();
        private IList<TCustomSqlStatement> searchInTables = new List<TCustomSqlStatement>();
        private IList<TCustomSqlStatement> searchInClauses = new List<TCustomSqlStatement>();
        public Hashtable queryAliasMap = new Hashtable();
        public HashSet<JoinCondition> joinRelationSet = new HashSet<JoinCondition>();
        private IList<JoinCondition> conditions = new List<JoinCondition>();

        public virtual string AnalysisResult
        {
            get
            {
                return buffer.ToString();
            }
        }

        public virtual IList<JoinCondition> JoinConditions
        {
            get
            {
                return conditions;
            }
        }

        public joinRelationAnalyze(string sql, EDbVendor dbVendor, bool showTitle)
        {
            TGSqlParser sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqltext = sql;
            analyzeSQL(sqlparser, false, showTitle);
        }

        public joinRelationAnalyze(FileInfo file, EDbVendor dbVendor, bool showTitle)
        {
            TGSqlParser sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqlfilename = file.FullName;
            analyzeSQL(sqlparser, false, showTitle);
        }

        public joinRelationAnalyze(TGSqlParser sqlparser, bool showLocation, bool showTitle)
        {
            analyzeSQL(sqlparser, showLocation, showTitle);
        }

        private void analyzeSQL(TGSqlParser sqlparser, bool showLocation, bool showTitle)
        {
            int ret = sqlparser.parse();

            if (ret != 0)
            {
                buffer.Append(sqlparser.Errormessage);
                return;
            }
            else
            {
                for (int j = 0; j < sqlparser.sqlstatements.size(); j++)
                {
                    TCustomSqlStatement select = (TCustomSqlStatement)sqlparser.sqlstatements.get(j);
                    analyzeStmt(select);
                }
            }

            if (showTitle)
            {
                buffer.Append("File\tJoinTable1\tJoinColumn1\tJoinTable2\tJoinColumn2\r\n");
            }

            conditions.Clear();

            foreach (JoinCondition join in joinRelationSet)
            {
                string lefttable = join.lefttable;
                string righttable = join.righttable;
                string leftcolumn = join.leftcolumn;
                string rightcolumn = join.rightcolumn;
                string leftColumnLocation = join.leftColumnLocation;
                string rightColumnLocation = join.rightColumnLocation;
                string leftTableLocation = join.leftTableLocation;
                string rightTableLocation = join.rightTableLocation;


                if ((string.ReferenceEquals(lefttable, null) || lefttable.Length == 0) && (string.ReferenceEquals(righttable, null) || righttable.Length == 0))
                {
                    continue;
                }

                IList<string[]> leftJoinNameList = getRealName(lefttable, leftcolumn, join.sql);
                IList<string[]> rightJoinNameList = getRealName(righttable, rightcolumn, join.sql);

                foreach (string[] leftJoinNames in leftJoinNameList)
                {
                    foreach (string[] rightJoinNames in rightJoinNameList)
                    {
                        if (!string.ReferenceEquals(leftJoinNames[0], null) && !string.ReferenceEquals(rightJoinNames[0], null) && !string.ReferenceEquals(leftJoinNames[1], null) && !string.ReferenceEquals(rightJoinNames[1], null))
                        {
                            JoinCondition condition = new JoinCondition();
                            condition.lefttable = leftJoinNames[0];
                            condition.righttable = rightJoinNames[0];
                            condition.leftcolumn = leftJoinNames[1];
                            condition.rightcolumn = rightJoinNames[1];
                            condition.leftColumnLocation = leftColumnLocation;
                            condition.rightColumnLocation = rightColumnLocation;
                            condition.leftTableLocation = leftTableLocation;
                            condition.rightTableLocation = rightTableLocation;

                            if (!conditions.Contains(condition))
                            {
                                conditions.Add(condition);
                                if (showLocation)
                                {
                                    buffer.Append(getFileName(sqlparser) + "\t" + fillString(condition.lefttable
                                            + condition.leftTableLocation)
                                            + "\t"
                                            + fillString(condition.leftcolumn
                                                    + condition.leftColumnLocation)
                                            + "\t"
                                            + fillString(condition.righttable
                                                    + condition.rightTableLocation)
                                            + "\t"
                                            + fillString(condition.rightcolumn
                                                    + condition.rightColumnLocation)
                                            + "\r\n");
                                }
                                else
                                {
                                    buffer.Append(getFileName(sqlparser) + "\t" + fillString(condition.lefttable)
                                            + "\t"
                                            + fillString(condition.leftcolumn)
                                            + "\t"
                                            + fillString(condition.righttable)
                                            + "\t"
                                            + fillString(condition.rightcolumn)
                                            + "\r\n");
                                }
                            }
                        }
                    }
                }
            }
        }

        private string getFileName(TGSqlParser sqlparser)
        {
            if (sqlparser.sqlfilename != null && sqlparser.sqlfilename.Length > 0)
            {
                return new FileInfo(sqlparser.sqlfilename).Name;
            }
            else return "N/A";
        }

        private void analyzeStmt(TCustomSqlStatement select)
        {
            if (select.CteList != null && select.CteList.size() > 0)
            {
                for (int i = 0; i < select.CteList.size(); i++)
                {
                    TCTE expression = (TCTE)select.CteList.getCTE(i);
                    cteMap[expression.TableName] = expression.Subquery;
                }
            }

            analyzeStatement(select);
        }

        private void analyzeStatement(TCustomSqlStatement select)
        {
            if (select is TSelectSqlStatement)
            {
                TSelectSqlStatement stmt = (TSelectSqlStatement)select;

                searchJoinFromStatement(stmt);

                if (stmt.CombinedQuery)
                {
                    analyzeStatement(stmt.LeftStmt);
                    analyzeStatement(stmt.RightStmt);
                }
                else
                {
                    for (int i = 0; i < select.ResultColumnList.size(); i++)
                    {
                        TResultColumn field = select.ResultColumnList.getResultColumn(i);
                        searchFields(field, select);
                    }
                }
            }
            else if (select is TPlsqlCreateProcedure)
            {
                TPlsqlCreateProcedure createProcedure = (TPlsqlCreateProcedure)select;
                if (createProcedure.BodyStatements != null)
                {
                    for (int i = 0; i < createProcedure.BodyStatements.size(); i++)
                    {
                        analyzeStmt(createProcedure.BodyStatements.get(i));
                    }
                }
            }
            else if (select is TMssqlCreateProcedure)
            {
                TMssqlCreateProcedure createProcedure = (TMssqlCreateProcedure)select;
                if (createProcedure.BodyStatements != null)
                {
                    for (int i = 0; i < createProcedure.BodyStatements.size(); i++)
                    {
                        analyzeStmt(createProcedure.BodyStatements.get(i));
                    }
                }
            }
            else if (select is TMssqlBlock)
            {
                TMssqlBlock block = (TMssqlBlock)select;
                if (block.BodyStatements != null)
                {
                    for (int i = 0; i < block.BodyStatements.size(); i++)
                    {
                        analyzeStmt(block.BodyStatements.get(i));
                    }
                }
            }
            else if (select.ResultColumnList != null)
            {
                for (int i = 0; i < select.ResultColumnList.size(); i++)
                {
                    TResultColumn field = select.ResultColumnList.getResultColumn(i);
                    searchFields(field, select);
                }
            }
        }

        private void searchJoinFromStatement(TSelectSqlStatement stmt)
        {
            if (stmt.joins != null)
            {
                for (int i = 0; i < stmt.joins.size(); i++)
                {
                    TJoin join = stmt.joins.getJoin(i);
                    handleJoin(stmt, join);
                }
            }
        }

        private void handleJoin(TSelectSqlStatement stmt, TJoin join)
        {
            if (join.Join != null)
            {
                handleJoin(stmt, join.Join);
            }
            if (join.JoinItems != null)
            {
                for (int j = 0; j < join.JoinItems.size(); j++)
                {
                    TJoinItem joinItem = join.JoinItems.getJoinItem(j);
                    TExpression expr = joinItem.OnCondition;
                    searchExpression(expr, stmt);
                }
            }
        }

        private IList<string[]> getRealName(string tableAlias, string columnAlias, IList<TCustomSqlStatement> stmtList)
        {
            IList<string[]> nameList = new List<string[]>();
            foreach (TCustomSqlStatement stmt in stmtList)
            {

                gudusoft.gsqlparser.nodes.TTable table = null;
                string columnName = columnAlias;
                if ((string.ReferenceEquals(tableAlias, null) || tableAlias.Length == 0) && stmt is TSelectSqlStatement && ((TSelectSqlStatement)stmt).tables.size() == 1 && ((TSelectSqlStatement)stmt).tables.getTable(0).AliasClause == null)
                {
                    table = ((TSelectSqlStatement)stmt).tables.getTable(0);
                    getTableNames(nameList, table, columnName);
                    continue;
                }
                else if (string.ReferenceEquals(tableAlias, null) || tableAlias.Length == 0)
                {
                    nameList.Add(new string[] { null, columnName });
                    continue;
                }

                if (tableAliasMap.ContainsKey(tableAlias.ToLower() + ":" + stmt.ToString()))
                {
                    table = (gudusoft.gsqlparser.nodes.TTable)tableAliasMap[tableAlias.ToLower() + ":" + stmt.ToString()];
                    getTableNames(nameList, table, columnName);
                    continue;
                }
                else if (tableAliasMap.ContainsKey(tableAlias.ToLower()) && !containsKey(tableAliasMap, tableAlias.ToLower() + ":"))
                {
                    table = (gudusoft.gsqlparser.nodes.TTable)tableAliasMap[tableAlias.ToLower()];
                    getTableNames(nameList, table, columnName);
                    continue;
                }
                else
                {
                    if (queryAliasMap.ContainsKey(tableAlias.ToLower()))
                    {
                        object value = queryAliasMap[tableAlias.ToLower()];
                        if (value is TSelectSqlStatement)
                        {
                            TSelectSqlStatement sql = (TSelectSqlStatement)value;
                            getRealNameFromSql(nameList, columnAlias, stmt, sql);
                        }
                        continue;
                    }
                    else if (stmt is TSelectSqlStatement)
                    {
                        findTableByAlias(nameList, (TSelectSqlStatement)stmt, tableAlias, columnAlias, new List<TSelectSqlStatement>());
                        continue;
                    }
                    continue;
                }
            }
            return nameList;
        }

        private void getTableNames(IList<string[]> nameList, gudusoft.gsqlparser.nodes.TTable table, string columnName)
        {
            if (!(table.Subquery is TSelectSqlStatement))
            {
                nameList.Add(new string[] { table.FullName, columnName });
            }
            else
            {
                TSelectSqlStatement stmt = (TSelectSqlStatement)table.Subquery;
                getRealNameFromSql(nameList, columnName, null, stmt);
            }
        }

        private void getRealNameFromSql(IList<string[]> nameList, string columnAlias, TCustomSqlStatement stmt, TSelectSqlStatement sql)
        {
            gudusoft.gsqlparser.nodes.TTable table = null;
            string columnName = null;

            if (sql.CombinedQuery)
            {
                getRealNameFromSql(nameList, columnAlias, stmt, sql.LeftStmt);
                getRealNameFromSql(nameList, columnAlias, stmt, sql.RightStmt);
            }
            else
            {
                for (int i = 0; i < sql.ResultColumnList.size(); i++)
                {
                    TResultColumn field = sql.ResultColumnList.getResultColumn(i);
                    switch (field.Expr.ExpressionType)
                    {
                        case EExpressionType.simple_object_name_t:
                            TColumn column = attrToColumn(field, sql);
                            if (((string.ReferenceEquals(column.columnAlias, null) || column.columnAlias.Length == 0) && columnAlias.Trim().Equals(column.columnName.Trim(), StringComparison.CurrentCultureIgnoreCase)) || ((!string.ReferenceEquals(column.columnAlias, null) && column.columnAlias.Length > 0) && columnAlias.Trim().Equals(column.columnAlias.Trim())) || column.columnName.Equals("*"))
                            {
                                if (!string.ReferenceEquals(column.columnPrex, null))
                                {
                                    if (stmt != null && tableAliasMap.ContainsKey(column.columnPrex.ToLower() + ":" + stmt.ToString()))
                                    {
                                        table = (gudusoft.gsqlparser.nodes.TTable)tableAliasMap[column.columnPrex.ToLower() + ":" + stmt.ToString()];
                                    }
                                    else if (tableAliasMap.ContainsKey(column.columnPrex.ToLower()))
                                    {
                                        table = (gudusoft.gsqlparser.nodes.TTable)tableAliasMap[column.columnPrex.ToLower()];
                                    }
                                }
                                else
                                {
                                    table = sql.tables.getTable(0);
                                }

                                if (column.columnName.Equals("*"))
                                {
                                    columnName = columnAlias;
                                }
                                else
                                {
                                    columnName = column.columnName;
                                }
                            }
                            break;
                    }
                }
                if (table != null)
                {
                    nameList.Add(new string[] { getTableName(table), columnName });
                }
            }
        }

        private string getTableName(gudusoft.gsqlparser.nodes.TTable table)
        {
            if (table.Subquery != null && table.Subquery.tables != null && table.Subquery.tables.size() > 0)
            {
                return getTableName(table.Subquery.tables.getTable(0));
            }
            return table.FullName;
        }

        private void findTableByAlias(IList<string[]> nameList, TSelectSqlStatement stmt, string tableAlias, string columnAlias, IList<TSelectSqlStatement> stats)
        {
            if (stats.Contains(stmt))
            {
                return;
            }
            else
            {
                stats.Add(stmt);
            }

            if (stmt.CombinedQuery)
            {
                findTableByAlias(nameList, stmt.LeftStmt, tableAlias, columnAlias, stats);
                findTableByAlias(nameList, stmt.RightStmt, tableAlias, columnAlias, stats);
            }
            else
            {
                for (int i = 0; i < stmt.tables.size(); i++)
                {
                    gudusoft.gsqlparser.nodes.TTable table = stmt.tables.getTable(i);
                    if (table.AliasClause != null && table.AliasClause.ToString().Length > 0)
                    {
                        if (table.AliasClause.ToString().Equals(tableAlias, StringComparison.CurrentCultureIgnoreCase))
                        {
                            nameList.Add(new string[] { table.TableName.ToString(), columnAlias });
                            return;
                        }
                    }
                    else if (table.TableName != null)
                    {
                        if (table.TableName.ToString().Equals(tableAlias, StringComparison.CurrentCultureIgnoreCase))
                        {
                            nameList.Add(new string[] { table.TableName.ToString(), columnAlias });
                            return;
                        }
                    }
                }
            }
            if (nameList.Count == 0 && stmt.ParentStmt is TSelectSqlStatement)
            {
                findTableByAlias(nameList, (TSelectSqlStatement)stmt.ParentStmt, tableAlias, columnAlias, stats);
            }

        }

        private bool containsKey(Hashtable tableAliasMap, string key)
        {
            ICollection collection = tableAliasMap.Keys;
            foreach (string str in collection)
            {
                if (str.ToLower().StartsWith(key.ToLower(), StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private string fillString(string text)
        {
            int tablength = 8;
        
            if (text.Length < tablength)
            {
                text += "\t";
            }
            return text;
        }

        public virtual void searchFields(TResultColumn field, TCustomSqlStatement select)
        {
            switch (field.Expr.ExpressionType)
            {
                case EExpressionType.simple_object_name_t:
                    searchTables(select);
                    searchClauses(select);
                    break;
                case EExpressionType.simple_constant_t:
                    searchExpression(field.Expr, select);
                    searchTables(select);
                    searchClauses(select);
                    break;
                case EExpressionType.case_t:
                    searchExpression(field.Expr, select);
                    searchTables(select);
                    searchClauses(select);
                    break;
                case EExpressionType.function_t:
                    searchExpression(field.Expr, select);
                    searchTables(select);
                    searchClauses(select);

                    TFunctionCall func = field.Expr.FunctionCall;
                    // buffer.AppendLine("function name {0}",
                    // func.funcname.AsText);

                    // check column : function arguments
                    if (func.Args != null)
                    {
                        for (int k = 0; k < func.Args.size(); k++)
                        {
                            TExpression expr = (TExpression)func.Args.getExpression(k);
                            searchExpression(expr, select);
                        }
                    }
                    else
                    {
                        if (select.tables.getTable(0).AliasClause != null)
                        {
                            string alias = select.tables.getTable(0).AliasClause.ToString();
                            if (!tableAliasMap.ContainsKey(alias.ToLower().Trim() + ":" + select.ToString()))
                            {
                                tableAliasMap[alias.ToLower().Trim() + ":" + select.ToString()] = select.tables.getTable(0);
                            }
                            if (!tableAliasMap.ContainsKey(alias.ToLower().Trim()))
                            {
                                tableAliasMap[alias.ToLower().Trim()] = select.tables.getTable(0);
                            }
                        }
                    }

                    if (func.AnalyticFunction != null)
                    {
                        TParseTreeNodeList list = func.AnalyticFunction.PartitionBy_ExprList;

                        searchExpressionList(select, list);

                        if (func.AnalyticFunction.OrderBy != null)
                        {
                            list = func.AnalyticFunction.OrderBy.Items;
                            searchExpressionList(select, list);
                        }
                    }

                    // check order by clause
                    // if (select instanceof TSelectSqlStatement &&
                    // ((TSelectSqlStatement)select).GroupbyClause != null)
                    // {
                    // for (int j = 0; j <
                    // ((TSelectSqlStatement)select).GroupbyClause.GroupItems.Count();
                    // j++)
                    // {
                    // TLzGroupByItem i =
                    // (TLzGroupByItem)((TSelectSqlStatement)select).GroupbyClause.GroupItems[j];
                    // searchExpression((TExpression)i._ndExpr, select);
                    // searchTables(select);
                    // }

                    // }

                    break;
                case EExpressionType.subquery_t:
                    if (field.Expr.SubQuery is TSelectSqlStatement)
                    {
                        searchSubQuery(field.Expr.SubQuery);
                    }
                    break;
                default:
                    //buffer.Append("searchFields of type: " + field.Expr.ExpressionType + " not implemented yet\r\n");
                    break;
            }
        }

        private void searchExpressionList(TCustomSqlStatement select, TParseTreeNodeList list)
        {
            if (list == null)
            {
                return;
            }

            for (int i = 0; i < list.size(); i++)
            {
                TExpression lcexpr = null;
                if (list.getElement(i) is TOrderByItem)
                {
                    lcexpr = (TExpression)((TOrderByItem)list.getElement(i)).SortKey;
                }
                else if (list.getElement(i) is TExpression)
                {
                    lcexpr = (TExpression)list.getElement(i);
                }

                if (lcexpr != null)
                {
                    searchExpression(lcexpr, select);
                }
            }
        }

        private void searchClauses(TCustomSqlStatement select)
        {
            if (!searchInClauses.Contains(select))
            {
                searchInClauses.Add(select);
            }
            else
            {
                return;
            }
            if (select is TSelectSqlStatement)
            {

                TSelectSqlStatement statement = (TSelectSqlStatement)select;
                Hashtable clauseTable = new Hashtable();

                // if (statement.SortClause != null)
                // {
                // TLzOrderByList sortList = (TLzOrderByList)statement.SortClause;
                // for (int i = 0; i < sortList.Count(); i++)
                // {
                // TLzOrderBy orderBy = sortList[i];
                // TExpression expr = orderBy.SortExpr;
                // clauseTable.add(expr, ClauseType.orderby);
                // }
                // }

                if (statement.WhereClause != null)
                {
                    clauseTable[(statement.WhereClause.Condition)] = ClauseType.@where;
                }
                // if (statement.ConnectByClause != null)
                // {
                // clauseTable.add((TExpression)statement.ConnectByClause,
                // ClauseType.connectby);
                // }
                // if (statement.StartwithClause != null)
                // {
                // clauseTable.add((TExpression)statement.StartwithClause,
                // ClauseType.startwith);
                // }
                foreach (TExpression expr in clauseTable.Keys)
                {
                    ClauseType type = (ClauseType)clauseTable[expr];
                    searchExpression(expr, select);
                    searchTables(select);

                }
            }
        }

        internal virtual void searchTables(TCustomSqlStatement select)
        {
            if (!searchInTables.Contains(select))
            {
                searchInTables.Add(select);
            }
            else
            {
                return;
            }

            TTableList tables = select.tables;

            if (tables.size() == 1)
            {
                gudusoft.gsqlparser.nodes.TTable lzTable = tables.getTable(0);
                if ((lzTable.TableType == ETableSource.objectname) && (lzTable.AliasClause == null || lzTable.AliasClause.ToString().Trim().Length == 0))
                {
                    if (cteMap.ContainsKey(lzTable.TableName.ToString()))
                    {
                        searchSubQuery((TSelectSqlStatement)cteMap[lzTable.TableName.ToString()]);
                    }
                    else
                    {
                        if (lzTable.AliasClause != null)
                        {
                            string alias = lzTable.AliasClause.ToString();
                            if (!tableAliasMap.ContainsKey(alias.ToLower().Trim() + ":" + select.ToString()))
                            {
                                tableAliasMap[alias.ToLower().Trim() + ":" + select.ToString()] = lzTable;
                            }
                            if (!tableAliasMap.ContainsKey(alias.ToLower().Trim()))
                            {
                                tableAliasMap[alias.ToLower().Trim()] = lzTable;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < tables.size(); i++)
            {
                gudusoft.gsqlparser.nodes.TTable lztable = tables.getTable(i);
                switch (lztable.TableType)
                {
                    case ETableSource.objectname:
                        TTable table = TLzTaleToTable(lztable);
                        string alias = table.tableAlias;
                        if (!string.ReferenceEquals(alias, null))
                        {
                            alias = alias.Trim();
                        }
                        else if (!string.ReferenceEquals(table.tableName, null))
                        {
                            alias = table.tableName.Trim();
                        }

                        if (cteMap.ContainsKey(lztable.TableName.ToString()))
                        {
                            searchSubQuery((TSelectSqlStatement)cteMap[lztable.TableName.ToString()]);
                        }
                        else
                        {
                            if (!string.ReferenceEquals(alias, null))
                            {
                                if (!tableAliasMap.ContainsKey(alias.ToLower().Trim() + ":" + select.ToString()))
                                {
                                    tableAliasMap[alias.ToLower().Trim() + ":" + select.ToString()] = lztable;
                                }
                                if (!tableAliasMap.ContainsKey(alias.ToLower().Trim()))
                                {
                                    tableAliasMap[alias.ToLower().Trim()] = lztable;
                                }
                            }
                        }
                        break;
                    case ETableSource.subquery:
                        if (lztable.AliasClause != null)
                        {
                            string tableAlias = lztable.AliasClause.ToString().Trim();
                            if (!queryAliasMap.ContainsKey(tableAlias.ToLower()))
                            {
                                queryAliasMap[tableAlias.ToLower()] = (TSelectSqlStatement)lztable.Subquery;
                            }
                        }
                        searchSubQuery((TSelectSqlStatement)lztable.Subquery);
                        break;
                    default:
                        break;
                }
            }
        }

        public virtual void searchSubQuery(TSelectSqlStatement select)
        {
            if (!searchInSubQuerys.Contains(select))
            {
                searchInSubQuerys.Add(select);
            }
            else
            {
                return;
            }

            searchJoinFromStatement(select);

            if (select.CombinedQuery)
            {
                searchSubQuery(select.LeftStmt);
                searchSubQuery(select.RightStmt);
            }
            else
            {
                for (int i = 0; i < select.ResultColumnList.size(); i++)
                {
                    TResultColumn field = select.ResultColumnList.getResultColumn(i);
                    searchFields(field, select);
                }
            }
        }

        public virtual TColumn attrToColumn(TResultColumn field, TCustomSqlStatement stmt)
        {
            TColumn column = new TColumn();

            TExpression attr = field.Expr;

            column.columnAlias = field.AliasClause == null ? null : field.AliasClause.ToString();
            TSourceToken columnToken = attr.getEndToken();
            column.columnName = columnToken.ToString();
            if (columnToken != null)
            {
                column.columnLocation = "("
                        + columnToken.lineNo
                        + ","
                        + columnToken.columnNo
                        + ")";
            }

            if (attr.ToString().IndexOf(".", StringComparison.Ordinal) > 0)
            {
                column.columnPrex = attr.ToString().Substring(0, attr.ToString().LastIndexOf(".", StringComparison.Ordinal));

                string tableName = column.columnPrex;
                if (tableName.IndexOf(".", StringComparison.Ordinal) > 0)
                {
                    tableName = tableName.Substring(tableName.LastIndexOf(".", StringComparison.Ordinal) + 1);
                }
                if (!column.tableNames.Contains(tableName))
                {
                    column.tableNames.Add(tableName);
                    if (attr.ObjectOperand != null
                        && attr.ObjectOperand.TableToken != null)
                    {
                        TSourceToken tableToken = attr.ObjectOperand
                                .TableToken;
                        if (tableToken != null)
                        {
                            column.tableLocation = "("
                                    + tableToken.lineNo
                                    + ","
                                    + tableToken.columnNo
                                    + ")";
                        }
                    }
                }
            }
            else
            {
                TTableList tables = stmt.tables;
                for (int i = 0; i < tables.size(); i++)
                {
                    gudusoft.gsqlparser.nodes.TTable lztable = tables.getTable(i);
                    TTable table = TLzTaleToTable(lztable);
                    if (!column.tableNames.Contains(table.tableName))
                    {
                        column.tableNames.Add(table.tableName);
                        column.tableLocation = table.tableLocation;
                    }
                }
            }

            return column;
        }

        internal virtual TTable TLzTaleToTable(gudusoft.gsqlparser.nodes.TTable lztable)
        {
            TTable table = new TTable();
            if (lztable.TableName != null)
            {
                table.tableName = lztable.Name;
                if (lztable.TableName.ToString().IndexOf(".", StringComparison.Ordinal) > 0)
                {
                    table.prefixName = lztable.TableName.ToString().Substring(0, lztable.FullName.IndexOf('.'));
                    table.tableLocation = lztable.TableName.coordinate();
                }
            }

            if (lztable.AliasClause != null)
            {
                table.tableAlias = lztable.AliasClause.ToString();
                if (table.tableLocation == null)
                {
                    table.tableLocation = lztable.AliasClause
                            .AliasName
                            .coordinate();
                }
            }
            return table;
        }

        internal virtual void searchExpression(TExpression expr, TCustomSqlStatement stmt)
        {
            joinConditonsInExpr c = new joinConditonsInExpr(this, expr, stmt);
            c.searchExpression();
        }

    }

}