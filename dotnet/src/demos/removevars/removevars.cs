using System;
using gudusoft.gsqlparser;
using gudusoft.gsqlparser.nodes;
using gudusoft.gsqlparser.stmt;
using System.IO;
using System.Text;

namespace gudusoft.gsqlparser.demos.removevars
{
    public class removevars
    {
        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: removevars scriptfile");
                return;
            }

            FileInfo file = new FileInfo(args[0]);
            if (!file.Exists || file.Attributes == FileAttributes.Directory)
            {
                Console.WriteLine(file + " is not a valid file.");
                return;
            }

            removevars remove = new removevars(file, EDbVendor.dbvmssql);
            Console.WriteLine(remove.RemoveResult);
        }

       

        private string result;

        private StringBuilder conditionBuffer = new StringBuilder();
        private StringBuilder trimBuffer = new StringBuilder();
        private StringBuilder suffixBuffer = new StringBuilder();
        private StringBuilder prefixBuffer = new StringBuilder();
        private StringBuilder replaceBuffer = new StringBuilder();
        private StringBuilder tokenBuffer = new StringBuilder();

        public removevars(FileInfo sqlFile, EDbVendor vendor)
        {
            TGSqlParser sqlparser = new TGSqlParser(vendor);
            sqlparser.sqlfilename = sqlFile.FullName;
            remove(sqlparser);
        }

        private string leftParenthese = null;
        private string rightParenthese = null;

        public removevars(string sql, EDbVendor vendor)
        {
            TGSqlParser sqlparser = new TGSqlParser(vendor);
            string noquoteString = removeQuote(sql.Trim());
            int index = sql.IndexOf(noquoteString, StringComparison.Ordinal);
            if (index > 0)
            {
                leftParenthese = sql.Substring(0, index);
                rightParenthese = sql.Substring(index + noquoteString.Length);
            }
            sqlparser.sqltext = noquoteString;
            remove(sqlparser);

            if (!string.ReferenceEquals(leftParenthese, null) && !string.ReferenceEquals(rightParenthese, null))
            {
                result = (leftParenthese + result + rightParenthese);
            }
        }

        private string removeQuote(string sql)
        {
            if (sql.StartsWith("(", StringComparison.Ordinal) && sql.EndsWith(")", StringComparison.Ordinal))
            {
                sql = sql.Substring(1, (sql.Length - 1) - 1).Trim();
            }
            if (sql.StartsWith("(", StringComparison.Ordinal) && sql.EndsWith(")", StringComparison.Ordinal))
            {
                return removeQuote(sql);
            }
            return sql;
        }

        private void getParserString(TCustomSqlStatement query)
        {
            string oldString = query.ToScript();
            if (string.ReferenceEquals(oldString, null))
            {
                return;
            }
            string newString = remove(query);
            if (string.ReferenceEquals(newString, null))
            {
                return;
            }
            if (!string.ReferenceEquals(newString, null) && !oldString.Equals(newString))
            {
                query.String = newString;
                return;
            }
        }

        public virtual string RemoveResult
        {
            get
            {
                return result;
            }
        }


        internal virtual string remove(TCustomSqlStatement stat)
        {
            if (stat.ResultColumnList != null)
            {
                for (int j = 0; j < stat.ResultColumnList.size(); j++)
                {
                    TResultColumn column = stat.ResultColumnList.getResultColumn(j);
                    if (column.Expr != null && column.Expr.SubQuery is TCustomSqlStatement)
                    {
                        TCustomSqlStatement query = (TCustomSqlStatement)column.Expr.SubQuery;
                        getParserString(query);
                    }
                }
            }
            if (stat.CteList != null)
            {
                for (int i = 0; i < stat.CteList.size(); i++)
                {
                    TCTE cte = stat.CteList.getCTE(i);
                    if (cte.Subquery != null)
                    {
                        getParserString(cte.Subquery);
                    }
                    if (cte.InsertStmt != null)
                    {
                        getParserString(cte.InsertStmt);
                    }
                    if (cte.UpdateStmt != null)
                    {
                        getParserString(cte.UpdateStmt);
                    }
                    if (cte.PreparableStmt != null)
                    {
                        getParserString(cte.PreparableStmt);
                    }
                    if (cte.DeleteStmt != null)
                    {
                        getParserString(cte.DeleteStmt);
                    }
                }
            }

            if (stat is TSelectSqlStatement && ((TSelectSqlStatement)stat).SetOperator != TSelectSqlStatement.setOperator_none)
            {
                TSelectSqlStatement select = ((TSelectSqlStatement)stat);
                getParserString(select.LeftStmt);
                getParserString(select.RightStmt);
                return select.ToScript();
            }

            if (stat.Statements != null && stat.Statements.size() > 0)
            {
                for (int i = 0; i < stat.Statements.size(); i++)
                {
                    getParserString(stat.Statements.get(i));
                }
            }
            if (stat.ReturningClause != null)
            {
                if (stat.ReturningClause.ColumnValueList != null)
                {
                    for (int i = 0; i < stat.ReturningClause.ColumnValueList.size(); i++)
                    {
                        if (stat.ReturningClause.ColumnValueList.getExpression(i).SubQuery != null)
                        {
                            getParserString(stat.ReturningClause.ColumnValueList.getExpression(i).SubQuery);
                        }
                    }
                }
                if (stat.ReturningClause.VariableList != null)
                {
                    for (int i = 0; i < stat.ReturningClause.VariableList.size(); i++)
                    {
                        if (stat.ReturningClause.VariableList.getExpression(i).SubQuery != null)
                        {
                            getParserString(stat.ReturningClause.VariableList.getExpression(i).SubQuery);
                        }
                    }
                }
            }
            if (stat is TSelectSqlStatement)
            {
                TTableList list = ((TSelectSqlStatement)stat).tables;
                for (int i = 0; i < list.size(); i++)
                {
                    TTable table = list.getTable(i);
                    if (table.Subquery != null)
                    {
                        getParserString(table.Subquery);
                    }
                    if (table.FuncCall != null)
                    {
                        ExpressionChecker w = new ExpressionChecker(this);
                        w.checkFunctionCall(table.FuncCall);
                    }
                }
            }

            if (stat is TSelectSqlStatement)
            {
                TJoinList list = ((TSelectSqlStatement)stat).joins;
                for (int i = 0; i < list.size(); i++)
                {
                    TJoin join = list.getJoin(i);
                    for (int j = 0; j < join.JoinItems.size(); j++)
                    {
                        TJoinItem joinItem = join.JoinItems.getJoinItem(j);
                        if (joinItem.Table != null)
                        {
                            if (joinItem.Table.Subquery != null)
                            {
                                getParserString(joinItem.Table.Subquery);
                            }
                            if (joinItem.Table.FuncCall != null)
                            {
                                ExpressionChecker w = new ExpressionChecker(this);
                                w.checkFunctionCall(joinItem.Table.FuncCall);
                            }
                        }
                        if (joinItem.OnCondition != null)
                        {
                            ExpressionChecker w = new ExpressionChecker(this);
                            w.checkExpression(joinItem.OnCondition);
                        }
                    }
                }
            }

            if (stat is TSelectSqlStatement)
            {
                TSelectSqlStatement select = (TSelectSqlStatement)stat;
                for (int i = 0; i < select.ResultColumnList.size(); i++)
                {
                    TResultColumn field = select.ResultColumnList.getResultColumn(i);
                    TExpression expr = field.Expr;
                    if (expr != null && expr.ExpressionType == EExpressionType.subquery_t)
                    {
                        getParserString(expr.SubQuery);
                    }
                }
            }

            if (stat.WhereClause != null && stat.WhereClause.Condition != null && stat.WhereClause.Condition.ToScript().Trim().Length > 0)
            {
                TExpression whereExpression = stat.Gsqlparser.parseExpression(stat.WhereClause.Condition.ToScript());
                if (string.ReferenceEquals(whereExpression.ToString(), null))
                {
                    removevars removevars = new removevars(stat.ToString(), stat.dbvendor);
                    return removevars.result;
                }
                else
                {
                    string oldString = stat.ToScript();
                    conditionBuffer.Remove(0, conditionBuffer.Length);
                    ExpressionChecker w = new ExpressionChecker(this);
                    w.checkExpression(whereExpression);
                    stat.WhereClause.Condition = stat.Gsqlparser.parseExpression(whereExpression.ToScript());
                }
            }
            if ((stat is TSelectSqlStatement) && ((TSelectSqlStatement)stat).GroupByClause != null && ((TSelectSqlStatement)stat).GroupByClause.HavingClause != null)
            {

                TExpression havingExpression = ((TSelectSqlStatement)stat).GroupByClause.HavingClause;

                if (havingExpression == null)
                {
                    removevars removeCondition = new removevars(stat.ToScript(), stat.dbvendor);
                    return removeCondition.result;
                }
                else
                {
                    string oldString = stat.ToScript();
                    conditionBuffer.Remove(0, conditionBuffer.Length);
                    ExpressionChecker w = new ExpressionChecker(this);
                    w.checkExpression(havingExpression);
                    string newString = stat.ToScript();
                    if (!oldString.Equals(newString))
                    {
                        if (havingExpression != null && havingExpression.ToScript().Trim().Length == 0)
                        {
                            ((TSelectSqlStatement)stat).GroupByClause = null;
                        }
                    }
                }
            }
            return stat.ToScript();

        }

        internal virtual void remove(TGSqlParser sqlparser)
        {
            int i = sqlparser.parse();
            if (i == 0)
            {
                TCustomSqlStatement stat = sqlparser.sqlstatements.get(0);
                getParserString(stat);
                result = stat.ToScript();
            }
            else
            {
                Console.Error.WriteLine(sqlparser.Errormessage);
            }
        }
    }
}