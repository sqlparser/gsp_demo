using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using gudusoft.gsqlparser.demos.util;
using gudusoft.gsqlparser;
using gudusoft.gsqlparser.nodes;
using gudusoft.gsqlparser.stmt;
using System.Linq;

namespace gudusoft.gsqlparser.demos.removeCondition
{

	public class removeCondition
	{

		public static void Main(string[] args)
		{
			string sql = "SELECT SUM (d.amt) \r\n" + "FROM   summit.cntrb_detail d \r\n" + "WHERE" + " d.fund_coll_attrb IN ( '$Institute$' ) \r\n" + "AND d.fund_acct IN ( '$Fund$' ) \r\n" + "AND d.cntrb_date >= '$From_Date$' \r\n" + "AND d.cntrb_date <= '$Thru_Date$' \r\n" + "GROUP  BY d.id; ";
			LinkedHashMap<string, string> conditionMap = new LinkedHashMap<string, string>();
			conditionMap["Institute"] = "ShanXi University";
			conditionMap["Fund"] = "Eclipse.org";
            removeCondition remove = new removeCondition(new FileInfo("C:\\1.txt"), EDbVendor.dbvoracle, conditionMap);

			Console.WriteLine(remove.RemoveResult);
        }

        private string result;

		private StringBuilder conditionBuffer = new StringBuilder();
		private StringBuilder trimBuffer = new StringBuilder();
		private StringBuilder suffixBuffer = new StringBuilder();
		private StringBuilder prefixBuffer = new StringBuilder();
		private StringBuilder replaceBuffer = new StringBuilder();
		private StringBuilder tokenBuffer = new StringBuilder();

        public removeCondition(FileInfo sqlFile, EDbVendor vendor, LinkedHashMap<string, string> conditionMap)
		{
			TGSqlParser sqlparser = new TGSqlParser(vendor);
            sqlparser.sqlfilename = sqlFile.FullName;
			remove(sqlparser, conditionMap);
		}

		private string leftParenthese = null;
		private string rightParenthese = null;

		public removeCondition(string sql, EDbVendor vendor, LinkedHashMap<string, string> conditionMap)
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
			remove(sqlparser, conditionMap);

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

		private void getParserString(TCustomSqlStatement query, LinkedHashMap<string, string> conditionMap)
		{
			string oldString = query.ToScript();
			if (string.ReferenceEquals(oldString, null))
			{
				return;
			}
			string newString = remove(query, conditionMap);
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
		

		internal virtual string remove(TCustomSqlStatement stat, LinkedHashMap<string, string> conditionMap)
		{
			if (stat.ResultColumnList != null)
			{
				for (int j = 0; j < stat.ResultColumnList.size(); j++)
				{
					TResultColumn column = stat.ResultColumnList.getResultColumn(j);
					if (column.Expr != null && column.Expr.SubQuery is TCustomSqlStatement)
					{
						TCustomSqlStatement query = (TCustomSqlStatement) column.Expr.SubQuery;
						getParserString(query, conditionMap);
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
						getParserString(cte.Subquery, conditionMap);
					}
					if (cte.InsertStmt != null)
					{
						getParserString(cte.InsertStmt, conditionMap);
					}
					if (cte.UpdateStmt != null)
					{
						getParserString(cte.UpdateStmt, conditionMap);
					}
					if (cte.PreparableStmt != null)
					{
						getParserString(cte.PreparableStmt, conditionMap);
					}
					if (cte.DeleteStmt != null)
					{
						getParserString(cte.DeleteStmt, conditionMap);
					}
				}
			}

			if (stat is TSelectSqlStatement && ((TSelectSqlStatement) stat).SetOperator != TSelectSqlStatement.setOperator_none)
			{
				TSelectSqlStatement select = ((TSelectSqlStatement) stat);
				getParserString(select.LeftStmt, conditionMap);
				getParserString(select.RightStmt, conditionMap);
				return select.ToScript();
			}

			if (stat.Statements != null && stat.Statements.size() > 0)
			{
				for (int i = 0; i < stat.Statements.size(); i++)
				{
					getParserString(stat.Statements.get(i), conditionMap);
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
							getParserString(stat.ReturningClause.ColumnValueList.getExpression(i).SubQuery, conditionMap);
						}
					}
				}
				if (stat.ReturningClause.VariableList != null)
				{
					for (int i = 0; i < stat.ReturningClause.VariableList.size(); i++)
					{
						if (stat.ReturningClause.VariableList.getExpression(i).SubQuery != null)
						{
							getParserString(stat.ReturningClause.VariableList.getExpression(i).SubQuery, conditionMap);
						}
					}
				}
			}
			if (stat is TSelectSqlStatement)
			{
				TTableList list = ((TSelectSqlStatement) stat).tables;
				for (int i = 0; i < list.size(); i++)
				{
					TTable table = list.getTable(i);
					if (table.Subquery != null)
					{
						getParserString(table.Subquery, conditionMap);
					}
					if (table.FuncCall != null)
					{
						ExpressionChecker w = new ExpressionChecker(this);
						w.checkFunctionCall(table.FuncCall, conditionMap);
					}
				}
			}

			if (stat is TSelectSqlStatement)
			{
				TJoinList list = ((TSelectSqlStatement) stat).joins;
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
								getParserString(joinItem.Table.Subquery, conditionMap);
							}
							if (joinItem.Table.FuncCall != null)
							{
								ExpressionChecker w = new ExpressionChecker(this);
								w.checkFunctionCall(joinItem.Table.FuncCall, conditionMap);
							}
							if (joinItem.OnCondition != null)
                        	{
                            	ExpressionChecker w = new ExpressionChecker(this);
                            	w.checkExpression(joinItem.OnCondition, conditionMap);
                        	}
						}
					}
				}
			}

			if (stat is TSelectSqlStatement)
			{
				TSelectSqlStatement select = (TSelectSqlStatement) stat;
				for (int i = 0; i < select.ResultColumnList.size(); i++)
				{
					TResultColumn field = select.ResultColumnList.getResultColumn(i);
					TExpression expr = field.Expr;
					if (expr != null && expr.ExpressionType == EExpressionType.subquery_t)
					{
						getParserString(expr.SubQuery, conditionMap);
					}
				}
			}

			if (stat.WhereClause != null && stat.WhereClause.Condition!=null && stat.WhereClause.Condition.ToScript().Trim().Length>0)
			{
				TExpression whereExpression = stat.Gsqlparser.parseExpression(stat.WhereClause.Condition.ToScript());
				if (string.ReferenceEquals(whereExpression.ToString(), null))
				{
					removeCondition removeCondition = new removeCondition(stat.ToString(), stat.dbvendor, conditionMap);
					return removeCondition.result;
				}
				else
				{
					string oldString = stat.ToScript();
					conditionBuffer.Remove(0, conditionBuffer.Length);
					ExpressionChecker w = new ExpressionChecker(this);
					w.checkExpression(whereExpression, conditionMap);
                    stat.WhereClause.Condition = stat.Gsqlparser.parseExpression(whereExpression.ToScript());
				}
			}
			if ((stat is TSelectSqlStatement) && ((TSelectSqlStatement) stat).GroupByClause != null && ((TSelectSqlStatement) stat).GroupByClause.HavingClause != null)
			{

				TExpression havingExpression = ((TSelectSqlStatement) stat).GroupByClause.HavingClause;

				if (havingExpression == null)
				{
					removeCondition removeCondition = new removeCondition(stat.ToScript(), stat.dbvendor, conditionMap);
					return removeCondition.result;
				}
				else
				{
					string oldString = stat.ToScript();
					conditionBuffer.Remove(0, conditionBuffer.Length);
					ExpressionChecker w = new ExpressionChecker(this);
					w.checkExpression(havingExpression, conditionMap);
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

		internal virtual void remove(TGSqlParser sqlparser, LinkedHashMap<string, string> conditionMap)
		{
			int i = sqlparser.parse();
			if (i == 0)
			{
				TCustomSqlStatement stat = sqlparser.sqlstatements.get(0);
				getParserString(stat, conditionMap);
				result = stat.ToScript();
				if (!string.ReferenceEquals(result, null))
				{
					result = replaceCondition(result, conditionMap);
				}
			}
			else
			{
				Console.Error.WriteLine(sqlparser.Errormessage);
			}
		}

		private string replaceCondition(string content, LinkedHashMap<string, string> conditionMap)
		{
			string[] conditions = new string[0];
			if (conditionMap != null && conditionMap.Count > 0)
			{
                conditions = Enumerable.ToArray(conditionMap.Keys);
            }

            Regex preservePattern = new Regex("\\$[^$]+\\$");
            MatchCollection matcher = preservePattern.Matches(content);

            StringBuilder sb = new StringBuilder();
            int last = 0;
            foreach (Match m in matcher)
            {
                string match = m.Groups[0].Value;
                string condition = match.Replace("$", "").Trim();
                bool flag = false;
                for (int i = 0; i < conditions.Length; i++)
                {
                    if (conditions[i].Equals(condition, StringComparison.CurrentCultureIgnoreCase) && !string.ReferenceEquals(conditionMap[conditions[i]], null))
                    {
                        flag = true;
                        sb.Append(content.Substring(last, m.Index - last));
                        sb.Append(conditionMap[conditions[i]]);
                        break;
                    }
                }

                if (!flag) {
                    sb.Append(content.Substring(last, m.Index - last));
                }
                last = m.Index + m.Length;
            }
            sb.Append(content.Substring(last));
            return sb.ToString();
		}
	}

}