using System;
using System.Text;
using gudusoft.gsqlparser;
using gudusoft.gsqlparser.nodes;
using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.removevars
{

    public class ExpressionChecker : IExpressionVisitor
    {
        private removevars removevars;

        internal StringBuilder buffer = new StringBuilder();

        internal StringBuilder tokenBuffer = new StringBuilder();

        private List<TExpression> removeExprs = new List<TExpression>();

        public ExpressionChecker(removevars removevars)
        {
            this.removevars = removevars;
        }

        private bool checkCondition(TExpression[] expression)
        {
            string expr = expression[0].ToScript();
            if (string.ReferenceEquals(expr, null))
            {
                return false;
            }

            if (expr.IndexOf("@") == -1)
                return true;

            TSourceToken start = expression[0].startToken;
            TSourceToken end = expression[0].endToken;
            TSourceTokenList stlist = start.container;
            //int t = 0;
            for (int k = start.posinlist; k <= end.posinlist; k++)
            {

                if (stlist[k].tokentype == ETokenType.ttsqlvar)
                {
                    return false;
                }
            }
            return true;
        }

        public virtual void checkExpression(TExpression expr)
        {
            expr.postOrderTraverse(this);
            for (int i = 0; i < removeExprs.Count; i++) {
                removeExprs[i].remove();
            }
            removeExprs.Clear();
        }

        public virtual void checkFunctionCall(TFunctionCall func)
        {
            if (func.Args != null)
            {
                for (int k = 0; k < func.Args.size(); k++)
                {
                    TExpression expr = func.Args.getExpression(k);
                    if (expr.SubQuery != null)
                    {
                        expr.SubQuery.String = removevars.remove(expr.SubQuery);
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
                            expr.SubQuery.String = removevars.remove(expr.SubQuery);
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
                                sortKey.SubQuery.String = removevars.remove(sortKey.SubQuery);
                            }
                        }
                    }
                }
            }
        }

        public virtual bool exprVisit(TParseTreeNode pnode, bool pIsLeafNode)
        {
            TExpression expression = (TExpression)pnode;
            if (expression.ExpressionType == EExpressionType.parenthesis_t)
            {
                expression = expression.LeftOperand;
            }
            if (is_compare_condition(expression.ExpressionType))
            {
                TExpression leftExpr = (TExpression)expression.LeftOperand;
                TExpression rightExpr = (TExpression)expression.RightOperand;

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
                    right.SubQuery.String = removevars.remove(right.SubQuery);
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
                TFunctionCall func = (TFunctionCall)expression.FunctionCall;
                checkFunctionCall(func);
            }
            if (expression.SubQuery is TCustomSqlStatement)
            {
                expression.SubQuery.String = removevars.remove(expression.SubQuery);
            }
            if (expression.CaseExpression != null)
            {
                TCaseExpression expr = expression.CaseExpression;
                TExpression conditionExpr = expr.Input_expr;
                if (conditionExpr != null)
                {
                    if (conditionExpr.SubQuery != null)
                    {
                        conditionExpr.SubQuery.String = removevars.remove(conditionExpr.SubQuery);
                    }
                }
                TExpression defaultExpr = expr.Else_expr;
                if (defaultExpr != null)
                {
                    if (defaultExpr.SubQuery != null)
                    {
                        defaultExpr.SubQuery.String = removevars.remove(defaultExpr.SubQuery);
                    }
                }
                TStatementList defaultStatList = expr.Else_statement_list;
                if (defaultStatList != null && defaultStatList.size() > 0)
                {
                    for (int i = 0; i < defaultStatList.size(); i++)
                    {
                        TCustomSqlStatement stmt = defaultStatList.get(i);
                        stmt.String = removevars.remove(stmt);
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
                                item.Comparison_expr.SubQuery.String = removevars.remove(item.Comparison_expr.SubQuery);
                            }
                        }
                        if (item.Return_expr != null)
                        {
                            if (item.Return_expr.SubQuery != null)
                            {
                                item.Return_expr.SubQuery.String = removevars.remove(item.Return_expr.SubQuery);
                            }
                        }
                        TStatementList statList = expr.Else_statement_list;
                        if (statList != null && statList.size() > 0)
                        {
                            for (int j = 0; j < statList.size(); j++)
                            {
                                TCustomSqlStatement stmt = statList.get(j);
                                stmt.String = removevars.remove(statList.get(j));
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
            removeExprs.Add(expression);
        }
    }

}