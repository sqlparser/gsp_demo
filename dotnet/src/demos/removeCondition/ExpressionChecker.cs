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

	public class ExpressionChecker : IExpressionVisitor
	{

		private LinkedHashMap<string, string> conditionMap;
		private removeCondition removeCondition;

		internal StringBuilder buffer = new StringBuilder();

		internal StringBuilder tokenBuffer = new StringBuilder();

		public ExpressionChecker(removeCondition removeCondition)
		{
			this.removeCondition = removeCondition;
		}

		private bool checkCondition(TExpression[] expression)
		{
            string[] conditions = new string[0];
            if (conditionMap != null && conditionMap.Count > 0)
            {
                conditions = Enumerable.ToArray(conditionMap.Keys);
            }

            string expr = expression[0].ToScript();
			if (string.ReferenceEquals(expr, null))
			{
				return false;
			}
            Regex preservePattern = new Regex("\\$[^$]+\\$");
            MatchCollection matcher = preservePattern.Matches(expr);
            buffer.Remove(0, buffer.Length);
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
                        buffer.Append(expr.Substring(last, m.Index - last));
                        buffer.Append(conditionMap[conditions[i]]);
                        break;
					}
				}
				if (!flag)
				{
					return false;
				}
                last = m.Index + m.Length;
            }
            buffer.Append(expr.Substring(last));
			if (!expr.Equals(buffer.ToString()))
			{
                expression[0] = expression[0].Gsqlparser.parseExpression(buffer.ToString());
			}
			return true;
		}

		public virtual void checkExpression(TExpression expr, LinkedHashMap<string, string> conditionMap)
		{
			this.conditionMap = conditionMap;
			expr.postOrderTraverse(this);
		}

		public virtual void checkFunctionCall(TFunctionCall func, LinkedHashMap<string, string> conditionMap)
		{
			if (func.Args != null)
			{
				for (int k = 0; k < func.Args.size(); k++)
				{
					TExpression expr = func.Args.getExpression(k);
					if (expr.SubQuery != null)
					{
						expr.SubQuery.String = removeCondition.remove(expr.SubQuery, conditionMap);
					}
				}
			}
			if (func.AnalyticFunction != null)
			{
				TExpressionList list = func.AnalyticFunction.PartitionBy_ExprList;
				if (list != null && list.size() > 0)
				{
					for (int i = 0; i < list.size(); i++)
					{
						TExpression expr = list.getExpression(i);
						if (expr.SubQuery != null)
						{
							expr.SubQuery.String = removeCondition.remove(expr.SubQuery, conditionMap);
						}
					}
				}
				if (func.AnalyticFunction.OrderBy != null)
				{
					TOrderByItemList orderByItemList = func.AnalyticFunction.OrderBy.Items;
					if (orderByItemList != null && orderByItemList.size() > 0)
					{
						for (int i = 0; i < orderByItemList.size(); i++)
						{
							TExpression sortKey = orderByItemList.getOrderByItem(i).SortKey;
							if (sortKey.SubQuery != null)
							{
								sortKey.SubQuery.String = removeCondition.remove(sortKey.SubQuery, conditionMap);
							}
						}
					}
				}
			}
		}

		public virtual bool exprVisit(TParseTreeNode pnode, bool pIsLeafNode)
		{
			TExpression expression = (TExpression) pnode;
			if (is_compare_condition(expression.ExpressionType))
			{
				TExpression leftExpr = (TExpression) expression.LeftOperand;
				TExpression rightExpr = (TExpression) expression.RightOperand;

                TExpression[] leftExprs = new TExpression[] { leftExpr };
                TExpression[] rightExprs = new TExpression[] { rightExpr };
                if (leftExpr != null && !checkCondition(leftExprs))
				{
                    removeExpression(expression);
				}

                expression.LeftOperand = leftExprs[0];

                if (rightExpr != null && !checkCondition(rightExprs))
				{
					removeExpression(expression);
				}

                expression.RightOperand = rightExprs[0];

                if ((expression.LeftOperand != null && string.ReferenceEquals(toExprString(expression.LeftOperand.ToScript()), null)) || (expression.RightOperand != null && string.ReferenceEquals(toExprString(expression.RightOperand.ToScript()), null)))
				{
					removeExpression(expression);
				}
               

            }
            if (expression.ExpressionType == EExpressionType.between_t)
            {
                TExpression[] exprs = new TExpression[] { expression };
                if (!checkCondition(exprs))
                {
                    removeExpression(expression);
                }
                if (expression.OperatorToken != null && string.ReferenceEquals(toExprString(expression.OperatorToken.ToString()), null))
                {
                    removeExpression(expression);
                }

            }
            if (expression.ExpressionType == EExpressionType.pattern_matching_t)
            {
                TExpression[] exprs = new TExpression[] { expression };
                if (!checkCondition(exprs))
                {
                    removeExpression(expression);
                }
                if (expression.OperatorToken != null && string.ReferenceEquals(toExprString(expression.OperatorToken.ToString()), null))
                {
                    removeExpression(expression);
                }
            }
            if (expression.ExpressionType == EExpressionType.in_t)
			{
				TExpression left = expression.LeftOperand;
                TExpression[] leftExprs = new TExpression[] { left };
                if (!checkCondition(leftExprs))
				{
					removeExpression(expression);
					return true;
				}
                expression.LeftOperand = leftExprs[0];

                TExpression right = expression.RightOperand;
                TExpression[] rightExprs = new TExpression[] { right };
                if (right.SubQuery != null)
				{
					right.SubQuery.String = removeCondition.remove(right.SubQuery, conditionMap);
				}
				else if (!checkCondition(rightExprs))
				{
					removeExpression(expression);
				}
                expression.RightOperand = rightExprs[0];

                if (expression.OperatorToken != null && string.ReferenceEquals(toExprString(expression.OperatorToken.ToString()), null))
				{
					removeExpression(expression);
				}
			}
			if (expression.FunctionCall != null)
			{
				TFunctionCall func = (TFunctionCall) expression.FunctionCall;
				checkFunctionCall(func, conditionMap);
			}
			if (expression.SubQuery is TCustomSqlStatement)
			{
				expression.SubQuery.String = removeCondition.remove(expression.SubQuery, conditionMap);
			}
			if (expression.CaseExpression != null)
			{
				TCaseExpression expr = expression.CaseExpression;
				TExpression conditionExpr = expr.Input_expr;
				if (conditionExpr != null)
				{
					if (conditionExpr.SubQuery != null)
					{
						conditionExpr.SubQuery.String = removeCondition.remove(conditionExpr.SubQuery, conditionMap);
					}
				}
				TExpression defaultExpr = expr.Else_expr;
				if (defaultExpr != null)
				{
					if (defaultExpr.SubQuery != null)
					{
						defaultExpr.SubQuery.String = removeCondition.remove(defaultExpr.SubQuery, conditionMap);
					}
				}
				TStatementList defaultStatList = expr.Else_statement_list;
				if (defaultStatList != null && defaultStatList.size() > 0)
				{
					for (int i = 0; i < defaultStatList.size(); i++)
					{
						TCustomSqlStatement stmt = defaultStatList.get(i);
						stmt.String = removeCondition.remove(stmt, conditionMap);
					}
				}

				TWhenClauseItemList list = expr.WhenClauseItemList;
				if (list != null && list.size() > 0)
				{
					for (int i = 0; i < list.size(); i++)
					{
						TWhenClauseItem item = list.getWhenClauseItem(i);
						if (item.Comparison_expr != null)
						{
							if (item.Comparison_expr.SubQuery != null)
							{
								item.Comparison_expr.SubQuery.String = removeCondition.remove(item.Comparison_expr.SubQuery, conditionMap);
							}
						}
						if (item.Return_expr != null)
						{
							if (item.Return_expr.SubQuery != null)
							{
								item.Return_expr.SubQuery.String = removeCondition.remove(item.Return_expr.SubQuery, conditionMap);
							}
						}
						TStatementList statList = expr.Else_statement_list;
						if (statList != null && statList.size() > 0)
						{
							for (int j = 0; j < statList.size(); j++)
							{
								TCustomSqlStatement stmt = statList.get(j);
								stmt.String = removeCondition.remove(statList.get(j), conditionMap);
							}
						}
					}
				}

				if (expression.OperatorToken != null && string.ReferenceEquals(toExprString(expression.OperatorToken.ToString()), null))
				{
					removeExpression(expression);
				}
			}

			if (expression.LeftOperand == null)
			{
                TExpression[] exprs = new TExpression[] { expression };
                if (!checkCondition(exprs))
				{
                    removeExpression(expression);
                }
			}
			return true;
		}

		private string toExprString(string text)
		{
			if (string.ReferenceEquals(text, null) || text.Length == 0)
			{
				return null;
			}
			text = text.Trim();
			if (text.StartsWith("(", StringComparison.Ordinal) && text.EndsWith(")", StringComparison.Ordinal))
			{
				text = text.Substring(1, (text.Length - 1) - 1);
				return toExprString(text.Trim());
			}
			return text;
		}

		internal virtual bool is_compare_condition(EExpressionType t)
		{
			return ((t == EExpressionType.simple_comparison_t) || (t == EExpressionType.group_comparison_t));
		}

		private void removeExpression(TExpression expression)
		{
            expression.remove();

        }
	}

}