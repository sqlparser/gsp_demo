using System;
using System.Collections;

namespace gudusoft.gsqlparser.demos.antiSQLInjection
{

    using TBaseType = gudusoft.gsqlparser.TBaseType;
    using ENodeType = gudusoft.gsqlparser.nodes.ENodeType;
    using IExpressionVisitor = gudusoft.gsqlparser.nodes.IExpressionVisitor;
    using TConstant = gudusoft.gsqlparser.nodes.TConstant;
    using TExpression = gudusoft.gsqlparser.nodes.TExpression;
    using TParseTreeNode = gudusoft.gsqlparser.nodes.TParseTreeNode;
    using TWhereClause = gudusoft.gsqlparser.nodes.TWhereClause;
    using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;

    using Expr = org.boris.expr.Expr;
    using ExprBoolean = org.boris.expr.ExprBoolean;
    using ExprEvaluatable = org.boris.expr.ExprEvaluatable;
    using ExprParser = org.boris.expr.parser.ExprParser;
    using Exprs = org.boris.expr.util.Exprs;
    using SimpleEvaluationContext = org.boris.expr.util.SimpleEvaluationContext;
    using gudusoft.gsqlparser;
    using System.Collections.Generic;

    /// <summary>
    /// GEval used to evaluate condition in where clause
    /// <para>
    /// Usage:
    /// </para>
    /// <para>
    /// GEval e = new GEval()
    /// </para>
    /// <para>
    /// e.value(condition,context)
    /// </para>
    /// <para>
    /// This class help to find out expression that always return true or false which
    /// will be
    /// </para>
    /// <para>
    /// used as a sql injection.
    /// </para>
    /// <para>
    /// If expression can't be evaluated, then an unknown value was returned.
    /// </para>
    /// <para>
    /// </para>
    /// <para>
    /// How this Evaluator works:
    /// </para>
    /// <para>
    /// This Evaluator use Postfix expression evaluation to calculate value of an
    /// expression
    /// </para>
    /// <para>
    /// TExpression.postOrderTraverse function traverses the expression in post fix
    /// order, and GEval work
    /// </para>
    /// <para>
    /// as a tree visitor to evaluate value of this expression
    /// </para>
    /// <para>
    /// Check this article to found out how postfix expression evaluation this works:
    /// </para>
    /// <para>
    /// http://scriptasylum.com/tutorials/infix_postfix/algorithms/postfix-evaluation
    /// /index.htm
    /// </para>
    /// <para>
    /// </para>
    /// <para>
    /// Supported expression syntax:
    /// <ul>
    /// <li>column-name > 1, an unknown value was returned.</li>
    /// <li>column-name > 1 or 1=1, always return true.</li>
    /// <li>column-name > 1 and 1=2, always return false</li>
    /// <li>column-name > 1 and 1+2-8/4 = 1, always return true</li>
    /// <li>column-name > 1 or 2 between 1 and 3, always return true</li>
    /// <li>column-name > 1 or 'abc' like 'ab%', always return true</li>
    /// <li>null is null, always return true</li>
    /// <li>exists (select 1 from tab where 1<2), always return true</li>
    /// </ul>
    /// </para>
    /// <para>
    /// </para>
    /// <para>
    /// In condition was not supported yet, so
    /// </para>
    /// <para>
    /// 1 in (1,2,3), will return unknown value
    /// </para>
    /// <para>
    /// </para>
    /// <para>
    /// you can modify this evaluator to meet your own requirements.
    /// 
    /// </para>
    /// </summary>

    public class GEval
    {

        public GEval()
        {
        }

        /// <summary>
        /// Evaluate a expression.
        /// </summary>
        /// <param name="expr">
        ///            , condition need to be evaluated. </param>
        /// <param name="context">
        ///            , not used in current version
        /// @return </param>

        public virtual object value(TExpression expr, GContext context)
        {
            evalVisitor ev = new evalVisitor(context);
            expr.postOrderTraverse(ev);
            return ev.Value;
        }

    }

    internal class evalVisitor : IExpressionVisitor
    {

        public evalVisitor(GContext context)
        {
            this.exprs = new Stack();
            this.context = context;
        }

        private object value;

        public virtual object Value
        {
            get
            {
                if (value == null)
                {
                    return ((TExpression)exprs.Pop()).Val;
                }
                return value;
            }
        }

        private Stack exprs = null;
        private GContext context = null;

        public virtual bool exprVisit(TParseTreeNode pNode, bool isLeafNode)
        {
            if (value != null)
            {
                return true;
            }
            TExpression expr = (TExpression)pNode;
            switch ((expr.ExpressionType))
            {
                case EExpressionType.simple_source_token_t:
                    if (expr.SourcetokenOperand.ToString().Equals("null", StringComparison.CurrentCultureIgnoreCase))
                    {
                        expr.Val = null;
                    }
                    else
                    {
                        expr.Val = new UnknownValue();
                    }
                    break;
                case EExpressionType.simple_object_name_t:
                    // this.objectOperand.setObjectType(TObjectName.ttobjVariable);
                    expr.Val = new UnknownValue();
                    break;
                case EExpressionType.simple_constant_t:
                    try
                    {
                        expr.Val = eval_constant(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.arithmetic_plus_t:
                    try
                    {
                        expr.Val = eval_add(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.arithmetic_minus_t:
                    try
                    {
                        expr.Val = eval_subtract(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.arithmetic_times_t:
                    try
                    {
                        expr.Val = eval_mul(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.arithmetic_divide_t:
                    try
                    {
                        expr.Val = eval_divide(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.parenthesis_t:
                    expr.Val = ((TExpression)exprs.Pop()).Val;
                    // leftOperand.doParse(psql,plocation);
                    break;
                case EExpressionType.concatenate_t:
                    try
                    {
                        expr.Val = eval_concatenate(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.unary_plus_t:
                    expr.Val = ((TExpression)exprs.Pop()).Val;
                    break;
                case EExpressionType.unary_minus_t:
                    try
                    {
                        long l = Coercion.coerceLong(((TExpression)exprs.Pop()).Val);
                        expr.Val = -l;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.assignment_t:
                    try
                    {
                        eval_assignment(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.group_t:
                    if (expr.RightOperand.SubQuery != null)
                    {
                        checkSubquery(expr.RightOperand.SubQuery);
                    }
                    expr.Val = new UnknownValue();
                    // inExpr.doParse(psql,plocation);
                    break;
                case EExpressionType.list_t:
                    expr.Val = new UnknownValue();
                    // exprList.doParse(psql,plocation);
                    break;
                case EExpressionType.function_t:
                    expr.Val = computeFunction(expr);
                    break;
                case EExpressionType.new_structured_type_t:
                    // functionCall.doParse(psql,plocation);
                    expr.Val = new UnknownValue();
                    break;
                case EExpressionType.cursor_t:
                    expr.Val = new UnknownValue();
                    break;
                case EExpressionType.subquery_t:
                    checkSubquery(expr.SubQuery);
                    expr.Val = new UnknownValue();
                    break;
                case EExpressionType.case_t:
                    expr.Val = new UnknownValue();
                    break;
                case EExpressionType.pattern_matching_t:
                    try
                    {
                        expr.Val = eval_like(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.exists_t:
                    try
                    {
                        expr.Val = eval_exists_condition(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.new_variant_type_t:
                    expr.Val = new UnknownValue();
                    break;
                case EExpressionType.unary_prior_t:
                    try
                    {
                        expr.Val = eval_unknown_one_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    // expr.setVal(new UnknownValue());
                    break;
                case EExpressionType.unary_bitwise_not_t:
                    try
                    {
                        expr.Val = eval_unknown_one_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    // expr.setVal(new UnknownValue());
                    break;
                case EExpressionType.sqlserver_proprietary_column_alias_t:
                    try
                    {
                        expr.Val = eval_unknown_two_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    // expr.setVal(new UnknownValue());
                    break;
                case EExpressionType.arithmetic_modulo_t:
                    try
                    {
                        expr.Val = eval_mod(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.bitwise_exclusive_or_t:
                    try
                    {
                        expr.Val = eval_unknown_two_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    // expr.setVal(new UnknownValue());
                    break;
                case EExpressionType.bitwise_or_t:
                    try
                    {
                        expr.Val = eval_unknown_two_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.bitwise_and_t:
                    try
                    {
                        expr.Val = eval_unknown_two_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.bitwise_xor_t:
                    try
                    {
                        expr.Val = eval_unknown_two_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.exponentiate_t:
                    try
                    {
                        expr.Val = eval_unknown_two_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.scope_resolution_t:
                    // expr.setVal(new UnknownValue());
                    break;
                case EExpressionType.at_time_zone_t:
                    try
                    {
                        expr.Val = eval_unknown_two_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.at_local_t:
                    try
                    {
                        expr.Val = eval_unknown_one_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.day_to_second_t:
                    try
                    {
                        expr.Val = eval_unknown_one_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.year_to_month_t:
                    try
                    {
                        expr.Val = eval_unknown_one_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.simple_comparison_t:
                    try
                    {
                        expr.Val = eval_simple_comparison_conditions(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.group_comparison_t:
                    try
                    {
                        expr.Val = eval_group_comparison_conditions(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.in_t:
                    try
                    {
                        expr.Val = eval_in_conditions(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.floating_point_t:
                    try
                    {
                        expr.Val = eval_unknown_one_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.logical_and_t:
                    try
                    {
                        expr.Val = eval_logical_conditions_and(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.logical_or_t:
                    try
                    {
                        expr.Val = eval_logical_conditions_or(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.logical_not_t:
                    try
                    {
                        expr.Val = eval_logical_conditions_not(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.logical_xor_t:
                    try
                    {
                        expr.Val = eval_unknown_two_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.null_t:
                    try
                    {
                        expr.Val = eval_isnull(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    // leftOperand.doParse(psql,plocation);
                    break;
                case EExpressionType.between_t:
                    try
                    {
                        expr.Val = eval_between(expr);
                    }
                    catch (Exception)
                    {
                        expr.Val = new UnknownValue();
                    }
                    break;
                case EExpressionType.is_of_type_t:
                    try
                    {
                        expr.Val = eval_unknown_one_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.collate_t: // sql server
                    try
                    {
                        expr.Val = eval_unknown_two_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    // leftOperand.doParse(psql,plocation);
                    // rightOperand.doParse(psql,plocation);
                    break;
                case EExpressionType.left_join_t:
                    try
                    {
                        expr.Val = eval_unknown_two_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.right_join_t:
                    // leftOperand.doParse(psql,plocation);
                    // rightOperand.doParse(psql,plocation);
                    try
                    {
                        expr.Val = eval_unknown_two_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                case EExpressionType.ref_arrow_t:
                    try
                    {
                        expr.Val = eval_unknown_two_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    // leftOperand.doParse(psql,plocation);
                    // rightOperand.doParse(psql,plocation);
                    break;
                case EExpressionType.typecast_t:
                    try
                    {
                        expr.Val = eval_unknown_one_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    // leftOperand.doParse(psql,plocation);
                    break;
                case EExpressionType.arrayaccess_t:
                    expr.Val = new UnknownValue();
                    // arrayAccess.doParse(psql,plocation);
                    break;
                case EExpressionType.unary_connect_by_root_t:
                    try
                    {
                        expr.Val = eval_unknown_one_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    // rightOperand.doParse(psql,plocation);
                    break;
                case EExpressionType.interval_t:
                    expr.Val = new UnknownValue();
                    // intervalExpr.doParse(psql,plocation);
                    break;
                case EExpressionType.unary_binary_operator_t:
                    try
                    {
                        expr.Val = eval_unknown_one_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    // rightOperand.doParse(psql,plocation);
                    break;
                case EExpressionType.left_shift_t:
                case EExpressionType.right_shift_t:
                    try
                    {
                        expr.Val = eval_unknown_one_operand(expr);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace); // To change body of catch statement
                                                     // use
                                                     // File | Settings | File Templates.
                    }
                    break;
                default:
                    ;
                    break;
            } // switch

            exprs.Push(expr);
            return true;
        }

        private object computeFunction(TExpression expr)
        {
            if (expr != null && !string.ReferenceEquals(expr.ToString(), null))
            {
                return computeFunction(expr.ToString());
            }
            return new UnknownValue();
        }

        private object computeFunction(string expr)
        {
            try
            {
                SimpleEvaluationContext context = new SimpleEvaluationContext();
                Expr e = ExprParser.parse(expr);
                Exprs.toUpperCase(e);
                if (e is ExprEvaluatable)
                {
                    e = ((ExprEvaluatable)e).evaluate(context);
                }
                if (e != null)
                {
                    return e.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }
            return new UnknownValue();
        }

        private void checkSubquery(TSelectSqlStatement select)
        {
            if (select != null && select.WhereClause != null)
            {
                object value = (new GEval()).value(select.WhereClause.Condition, context);
                if (value is bool)
                {
                    this.value = value;
                }
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_unknown_one_operand(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_unknown_one_operand(TExpression expr)
        {
            exprs.Pop();
            return new UnknownValue();
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_unknown_two_operand(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_unknown_two_operand(TExpression expr)
        {
            exprs.Pop();
            exprs.Pop();
            return new UnknownValue();
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void eval_assignment(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual void eval_assignment(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            TExpression left = (TExpression)exprs.Pop();
            left.Val = right;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_constant(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_constant(TExpression expr)
        {
            object ret = null;
            TConstant constant = expr.ConstantOperand;
            if ((constant.NodeType == ENodeType.T_Constant.Id && isString(constant.ToString())) || constant.NodeType == ENodeType.T_Constant_String.Id)
            {
                if (constant.startToken.ToString().Equals("NULL", StringComparison.CurrentCultureIgnoreCase))
                {
                    ret = "NULL";
                }
                else
                {
                    string s = constant.startToken.ToString().Substring(1, (constant.startToken.ToString().Length - 1) - 1);
                    ret = s;
                }
            }
            else if ((constant.NodeType == ENodeType.T_Constant.Id && isInteger(constant.ToString())) || constant.NodeType == ENodeType.T_Constant_Integer.Id)
            {
                long v = Coercion.coerceInteger(constant.startToken.ToString());
                if (constant.Sign != null)
                {
                    if (constant.Sign.ToString().Equals("-", StringComparison.CurrentCultureIgnoreCase))
                    {
                        v = -v;
                    }
                }
                ret = v;
            }
            else if ((constant.NodeType == ENodeType.T_Constant.Id && isFloat(constant.ToString())) || constant.NodeType == ENodeType.T_Constant_Float.Id)
            {
                double d = Coercion.coerceDouble(constant.startToken.ToString());
                ret = d;
            }
            return ret;
        }

        private bool isString(string @string)
        {
            if (@string.Equals("NULL", StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
            else
            {
                if (@string.StartsWith("\'", StringComparison.Ordinal) && @string.EndsWith("\'", StringComparison.Ordinal))
                {
                    return true;
                }
            }
            return false;
        }

        private bool isInteger(string @string)
        {
            try
            {
                int.Parse(@string);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private bool isFloat(string @string)
        {
            try
            {
                double.Parse(@string);
            }
            catch
            {
                return false;
            }
            return true;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_exists_condition(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_exists_condition(TExpression expr)
        {
            // check condition in subquery
            TSelectSqlStatement select = expr.SubQuery;
            TWhereClause @where = select.WhereClause;
            if (@where == null)
            {
                return true;
            }

            TExpression condition = @where.Condition;
            GEval e = new GEval();
            return e.value(condition, null);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_like(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_like(TExpression expr)
        {
            if (expr.LikeEscapeOperand != null)
            {
                // exprs.pop();
            }
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if (right.ToString().StartsWith("%", StringComparison.Ordinal))
            {
                if (right.ToString().EndsWith("%", StringComparison.Ordinal))
                {
                    // 'abc' like '%abc%'
                    string c = right.ToString().Substring(1, (right.ToString().Length - 1) - 1);
                    if (left.ToString().Contains(c))
                    {
                        if (expr.OperatorToken.ToString().Equals("not", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (expr.OperatorToken.ToString().Equals("not", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    // 'abc' like '%abc'
                    string c = right.ToString().Substring(1, (right.ToString().Length) - 1);
                    if (left.ToString().EndsWith(c, StringComparison.Ordinal))
                    {
                        if (expr.OperatorToken.ToString().Equals("not", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (expr.OperatorToken.ToString().Equals("not", StringComparison.CurrentCultureIgnoreCase))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            else if (right.ToString().EndsWith("%", StringComparison.Ordinal))
            {
                // 'abc' like 'abc%'
                string c = right.ToString().Substring(0, right.ToString().Length - 1);
                if (left.ToString().StartsWith(c, StringComparison.Ordinal))
                {
                    if (expr.OperatorToken.ToString().Equals("not", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    if (expr.OperatorToken.ToString().Equals("not", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                // 'abc' like 'abc'
                if (right.ToString().Equals(left.ToString(), StringComparison.CurrentCultureIgnoreCase))
                {
                    if (expr.OperatorToken.ToString().Equals("not", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    if (expr.OperatorToken.ToString().Equals("not", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_notequal(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_notequal(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if (left == null && right == null)
            {
                /*
				 * first, the possibility that both *are* null
				 */

                return false;
            }
            else if (left == null || right == null)
            {
                /*
				 * otherwise, both aren't, so it's clear L != R
				 */
                return true;
            }
            else if (left.GetType().Equals(right.GetType()))
            {
                return (left.Equals(right)) ? false : true;
            }
            else if (left is float || left is double || right is float || right is double)
            {
                object result = computeFunction(left.ToString() + "<>" + right.ToString());
                return bool.Parse(result.ToString()) == true;
            }
            else if (left is ValueType || right is ValueType || left is char || right is char)
            {
                object result = computeFunction(left.ToString() + "<>" + right.ToString());
                return bool.Parse(result.ToString()) == true;
            }
            else if (left is bool || right is bool)
            {
                return (Coercion.coerceBoolean(left).Equals(Coercion.coerceBoolean(right))) ? false : true;
            }
            else if (left is string || right is string)
            {
                return (left.ToString().Equals(right.ToString())) ? false : true;
            }

            return (left.Equals(right)) ? false : true;

        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_ge(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_ge(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if (left == right)
            {
                return true;
            }
            else if ((left == null) || (right == null))
            {
                return false;
            }
            else if (Coercion.isFloatingPoint(left) || Coercion.isFloatingPoint(right))
            {
                object result = computeFunction(left.ToString() + ">=" + right.ToString());
                return bool.Parse(result.ToString()) == true;
            }
            else if (Coercion.isNumberable(left) || Coercion.isNumberable(right))
            {
                object result = computeFunction(left.ToString() + ">=" + right.ToString());
                return bool.Parse(result.ToString()) == true;
            }
            else if (left is string || right is string)
            {
                string leftString = left.ToString();
                string rightString = right.ToString();

                return leftString.CompareTo(rightString) >= 0 ? true : false;
            }
            else if (left is IComparable)
            {
                return ((IComparable)left).CompareTo(right) >= 0 ? true : false;
            }
            else if (right is IComparable)
            {
                return ((IComparable)right).CompareTo(left) <= 0 ? true : false;
            }

            throw new Exception("Invalid comparison : GE ");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_le(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_le(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if (left == right)
            {
                return true;
            }
            else if ((left == null) || (right == null))
            {
                return false;
            }
            else if (Coercion.isFloatingPoint(left) || Coercion.isFloatingPoint(right))
            {
                object result = computeFunction(left.ToString() + "<=" + right.ToString());
                return bool.Parse(result.ToString()) == true;
            }
            else if (Coercion.isNumberable(left) || Coercion.isNumberable(right))
            {
                object result = computeFunction(left.ToString() + "<=" + right.ToString());
                return bool.Parse(result.ToString()) == true;
            }
            else if (left is string || right is string)
            {
                string leftString = left.ToString();
                string rightString = right.ToString();

                return leftString.CompareTo(rightString) <= 0 ? true : false;
            }
            else if (left is IComparable)
            {
                return ((IComparable)left).CompareTo(right) <= 0 ? true : false;
            }
            else if (right is IComparable)
            {
                return ((IComparable)right).CompareTo(left) >= 0 ? true : false;
            }

            throw new Exception("Invalid comparison : LE ");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_lt(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_lt(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if ((left == right) || (left == null) || (right == null))
            {
                return false;
            }
            else if (Coercion.isFloatingPoint(left) || Coercion.isFloatingPoint(right))
            {
                object result = computeFunction(left.ToString() + "<" + right.ToString());
                return bool.Parse(result.ToString()) == true;
            }
            else if (Coercion.isNumberable(left) || Coercion.isNumberable(right))
            {
                object result = computeFunction(left.ToString() + "<" + right.ToString());
                return bool.Parse(result.ToString()) == true;
            }
            else if (left is string || right is string)
            {
                string leftString = left.ToString();
                string rightString = right.ToString();

                return leftString.CompareTo(rightString) < 0 ? true : false;
            }
            else if (left is IComparable)
            {
                return ((IComparable)left).CompareTo(right) < 0 ? true : false;
            }
            else if (right is IComparable)
            {
                return ((IComparable)right).CompareTo(left) > 0 ? true : false;
            }

            throw new Exception("Invalid comparison : LT ");
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_gt(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_gt(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if ((left == right) || (left == null) || (right == null))
            {
                return false;
            }
            else if (Coercion.isFloatingPoint(left) || Coercion.isFloatingPoint(right))
            {
                object result = computeFunction(left.ToString() + ">" + right.ToString());
                return bool.Parse(result.ToString()) == true;
            }
            else if (Coercion.isNumberable(left) || Coercion.isNumberable(right))
            {
                object result = computeFunction(left.ToString() + ">" + right.ToString());
                return bool.Parse(result.ToString()) == true;
            }
            else if (left is string || right is string)
            {
                string leftString = left.ToString();
                string rightString = right.ToString();

                return leftString.CompareTo(rightString) > 0 ? true : false;
            }
            else if (left is IComparable)
            {
                return ((IComparable)left).CompareTo(right) > 0 ? true : false;
            }
            else if (right is IComparable)
            {
                return ((IComparable)right).CompareTo(left) < 0 ? true : false;
            }

            throw new Exception("Invalid comparison : GT ");

        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_equal(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_equal(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }

            if (left == null && right == null)
            {
                /*
				 * if both are null L == R
				 */
                return true;
            }
            else if (left == null || right == null)
            {
                /*
				 * we know both aren't null, therefore L != R
				 */
                return false;
            }
            else if (left.GetType().Equals(right.GetType()))
            {
                return left.Equals(right) ? true : false;
            }
            else if (left is float || left is double || right is float || right is double)
            {
                object result = computeFunction(left.ToString() + "=" + right.ToString());
                return bool.Parse(result.ToString()) == true;
            }
            else if (left is ValueType || right is ValueType || left is char || right is char)
            {
                object result = computeFunction(left.ToString() + "=" + right.ToString());
                return bool.Parse(result.ToString()) == true;
            }
            else if (left is bool || right is bool)
            {
                return Coercion.coerceBoolean(left).Equals(Coercion.coerceBoolean(right)) ? true : false;
            }
            else if (left is string || right is string)
            {
                return left.ToString().Equals(right.ToString()) ? true : false;
            }

            return left.Equals(right) ? true : false;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_logical_conditions_not(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_logical_conditions_not(TExpression expr)
        {
            object left = ((TExpression)exprs.Pop()).Val;

            if (left is UnknownValue)
            {
                return new UnknownValue();
            }
            bool b = Coercion.coerceBoolean(left);

           
            return b ? false : true;
   
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_logical_conditions_or(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_logical_conditions_or(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            if (right is UnknownValue)
            {
                if (left is UnknownValue)
                {
                    return new UnknownValue();
                }
                else
                {
                    bool leftValue = Coercion.coerceBoolean(left);
                    if (leftValue)
                    {
                        return true;
                    }
                    else
                    {
                        return new UnknownValue();
                    }
                }
            }
            else if (left is UnknownValue)
            {
                bool rightValue = Coercion.coerceBoolean(right);
                if (rightValue)
                {
                    return true;
                }
                else
                {
                    return new UnknownValue();
                }
            }
            else
            {
                bool leftValue = Coercion.coerceBoolean(left);
                bool rightValue = Coercion.coerceBoolean(right);

                return (leftValue || rightValue) ? true : false;
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_logical_conditions_and(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_logical_conditions_and(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            if (right is UnknownValue)
            {
                if (left is UnknownValue)
                {
                    return new UnknownValue();
                }
                else
                {
                    bool leftValue = Coercion.coerceBoolean(left);
                    if (!leftValue)
                    {
                        return false;
                    }
                    else
                    {
                        return new UnknownValue();
                    }
                }
            }
            else if (left is UnknownValue)
            {
                bool rightValue = Coercion.coerceBoolean(right);
                if (!rightValue)
                {
                    return false;
                }
                else
                {
                    return new UnknownValue();
                }
            }
            else
            {
                bool leftValue = Coercion.coerceBoolean(left);
                bool rightValue = Coercion.coerceBoolean(right);

                return (leftValue && rightValue) ? true : false;
            }

        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_in_conditions(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_in_conditions(TExpression expr)
        {
            object result = computeFunction(expr);
            if (ExprBoolean.TRUE.ToString().Equals(result) || ExprBoolean.FALSE.ToString().Equals(result))
            {
                return bool.Parse(result.ToString()) == true;
            }
            else
            {
                return new UnknownValue();
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_group_comparison_conditions(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_group_comparison_conditions(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            return new UnknownValue();
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_simple_comparison_conditions(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_simple_comparison_conditions(TExpression expr)
        {
            TExpression topExpr = (TExpression)exprs.Peek();
            object right = topExpr.Val;
            List<Object> expressionList = new List<Object>(exprs.ToArray());
            expressionList.Reverse();
            int index = expressionList.IndexOf(topExpr);
            object left = ((TExpression)expressionList[index - 1]).Val;

            if ((left is UnknownValue) || (right is UnknownValue))
            {
                exprs.Pop();
                exprs.Pop();
                return new UnknownValue();
            }

            if (expr.ComparisonOperator.tokencode == (int)'=')
            {
                return eval_equal(expr);
            }
            else if (expr.ComparisonOperator.tokencode == TBaseType.not_equal)
            {
                return eval_notequal(expr);
            }
            else if (expr.ComparisonOperator.tokencode == (int)'>')
            {
                return eval_gt(expr);
            }
            else if (expr.ComparisonOperator.tokencode == (int)'<')
            {
                return eval_lt(expr);
            }
            else if (expr.ComparisonOperator.tokencode == TBaseType.less_equal)
            {
                return eval_le(expr);
            }
            else if (expr.ComparisonOperator.tokencode == TBaseType.great_equal)
            {
                return eval_ge(expr);
            }
            else
            {
                exprs.Pop();
                exprs.Pop();
                return new UnknownValue();
            }

        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_between(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_between(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;
            GEval v = new GEval();
            object between = v.value(expr.BetweenOperand, null);

            if ((between is UnknownValue) || (left is UnknownValue) || (right is UnknownValue))
            {
                return new UnknownValue();
            }
            else
            {
                return computeFunction(between.ToString() + ">=" + left.ToString() + "&&" + between.ToString() + "<=" + right.ToString());
            }

        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_isnull(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_isnull(TExpression expr)
        {
            object left = ((TExpression)exprs.Pop()).Val;
            if (left is UnknownValue)
            {
                return new UnknownValue();
            }
            else
            {
                if (left == null)
                {
                    if (expr.OperatorToken.ToString().Equals("not", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return false; // null is not null, return false
                    }
                    else
                    {
                        return true; // null is null, return true
                    }
                }
                else if (false.Equals(left))
                {
                    if (expr.OperatorToken.ToString().Equals("not", StringComparison.CurrentCultureIgnoreCase))
                    {
                        return true; // not null is not null, return false
                    }
                    else
                    {
                        return false; // not null is null, return false
                    }
                }
                else
                {
                    return new UnknownValue();
                }
            }
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_mod(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_mod(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            if (left == null && right == null)
            {
                return (byte)0;
            }

            return computeFunction(left.ToString() + "%" + right.ToString());
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_divide(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_divide(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;
            if (left == null && right == null)
            {
                return (byte)0;
            }

            return computeFunction(left.ToString() + "/" + right.ToString());

        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_mul(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_mul(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            /*
			 * the spec says 'and', I think 'or'
			 */
            if (left == null && right == null)
            {
                return (byte)0;
            }

            return computeFunction(left.ToString() + "*" + right.ToString());
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_subtract(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_subtract(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            /*
			 * the spec says 'and', I think 'or'
			 */
            if (left == null && right == null)
            {
                return (byte)0;
            }

            return computeFunction(left.ToString() + "-" + right.ToString());

        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_concatenate(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_concatenate(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            return left.ToString() + right.ToString();
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: Object eval_add(gudusoft.gsqlparser.nodes.TExpression expr) throws Exception
        internal virtual object eval_add(TExpression expr)
        {
            object right = ((TExpression)exprs.Pop()).Val;
            object left = ((TExpression)exprs.Pop()).Val;

            /*
			 * the spec says 'and'
			 */
            if (left == null && right == null)
            {
                return 0L;
            }

            return computeFunction(left.ToString() + "+" + right.ToString());
        }

    }

    internal class Coercion
    {

        /// <summary>
        /// Coerce to a Boolean.
        /// </summary>
        /// <param name="val">
        ///            Object to be coerced. </param>
        /// <returns> The Boolean coerced value, or null if none possible. </returns>
        public static bool coerceBoolean(object val)
        {
            if (val == null)
            {
                return false;
            }
            else if (val is bool)
            {
                return (bool)val;
            }
            else if (val is string)
            {
                return Convert.ToBoolean((string)val);
            }
            else
            {
                return Convert.ToBoolean(val.ToString());
            }
        }

        /// <summary>
        /// Coerce to a Integer.
        /// </summary>
        /// <param name="val">
        ///            Object to be coerced. </param>
        /// <returns> The Integer coerced value. </returns>
        /// <exception cref="Exception">
        ///             If Integer coercion fails. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static Nullable<long> coerceInteger(Object val) throws Exception
        public static long coerceInteger(object val)
        {
            if (val == null)
            {
                return 0L;
            }
            else if (val is string)
            {
                if ("".Equals(val))
                {
                    return 0L;
                }
                return long.Parse((string)val);
            }
            else if (val is char)
            {
                return (long)(((char)val));
            }
            else if (val is bool)
            {
                throw new Exception("Boolean->Integer coercion exception");
            }
            else if (val is ValueType)
            {
                return long.Parse(((ValueType)val).ToString());
            }

            throw new Exception("Integer coercion exception");
        }

        /// <summary>
        /// Coerce to a Long.
        /// </summary>
        /// <param name="val">
        ///            Object to be coerced. </param>
        /// <returns> The Long coerced value. </returns>
        /// <exception cref="Exception">
        ///             If Long coercion fails. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static Nullable<long> coerceLong(Object val) throws Exception
        public static long coerceLong(object val)
        {
            if (val == null)
            {
                return 0L;
            }
            else if (val is string)
            {
                if ("".Equals(val))
                {
                    return 0L;
                }
                return Convert.ToInt64((string)val);
            }
            else if (val is char)
            {
                return (long)(((char)val));
            }
            else if (val is bool)
            {
                throw new Exception("Boolean->Long coercion exception");
            }
            else if (val is ValueType)
            {
                return long.Parse(((ValueType)val).ToString());
            }

            throw new Exception("Long coercion exception");
        }

        /// <summary>
        /// Coerce to a Double.
        /// </summary>
        /// <param name="val">
        ///            Object to be coerced. </param>
        /// <returns> The Double coerced value. </returns>
        /// <exception cref="Exception">
        ///             If Double coercion fails. </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public static Nullable<double> coerceDouble(Object val) throws Exception
        public static double coerceDouble(object val)
        {
            if (val == null)
            {
                return 0d;
            }
            else if (val is string)
            {
                if ("".Equals(val))
                {
                    return 0d;
                }

                return Convert.ToDouble((string)val);
            }
            else if (val is char)
            {
                int i = ((char)val);

                return (double)i;
            }
            else if (val is bool)
            {
                throw new Exception("Boolean->Double coercion exception");
            }
            else if (val is double)
            {
                return (double)val;
            }
            else if (val is ValueType)
            {
                return double.Parse(val.ToString());
            }

            throw new Exception("Double coercion exception");
        }

        /// <summary>
        /// Is Object a floating point number.
        /// </summary>
        /// <param name="o">
        ///            Object to be analyzed. </param>
        /// <returns> true if it is a Float or a Double. </returns>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: public static boolean isFloatingPoint(final Object o)
        public static bool isFloatingPoint(object o)
        {
            return o is float || o is double;
        }

        /// <summary>
        /// Is Object a whole number.
        /// </summary>
        /// <param name="o">
        ///            Object to be analyzed. </param>
        /// <returns> true if Integer, Long, Byte, Short or Character. </returns>
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        //ORIGINAL LINE: public static boolean isNumberable(final Object o)
        public static bool isNumberable(object o)
        {
            return o is int || o is long || o is byte || o is short || o is char;
        }

    }

}