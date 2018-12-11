using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.antiSQLInjection
{

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using EExpressionType = gudusoft.gsqlparser.EExpressionType;
    using ESqlStatementType = gudusoft.gsqlparser.ESqlStatementType;
    using TBaseType = gudusoft.gsqlparser.TBaseType;
    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using TSourceToken = gudusoft.gsqlparser.TSourceToken;
    using IExpressionVisitor = gudusoft.gsqlparser.nodes.IExpressionVisitor;
    using TExpression = gudusoft.gsqlparser.nodes.TExpression;
    using TMultiTarget = gudusoft.gsqlparser.nodes.TMultiTarget;
    using TMultiTargetList = gudusoft.gsqlparser.nodes.TMultiTargetList;
    using TParseTreeNode = gudusoft.gsqlparser.nodes.TParseTreeNode;
    using TResultColumnList = gudusoft.gsqlparser.nodes.TResultColumnList;
    using TInsertSqlStatement = gudusoft.gsqlparser.stmt.TInsertSqlStatement;
    using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;
    using TUpdateSqlStatement = gudusoft.gsqlparser.stmt.TUpdateSqlStatement;
    using System.Text.RegularExpressions;
    using System;
    using System.IO;

    /// <summary>
    /// This is the classed used to check sql injection, it can detect following type
    /// of sql injection
    /// <para>
    /// <ul>
    /// <li>syntax error</li>
    /// <li>always_true_condition</li>
    /// <li>always_false_condition</li>
    /// <li>comment_at_the_end_of_statement</li>
    /// <li>stacking_queries</li>
    /// <li>not_in_allowed_statement</li>
    /// <li>union_set</li>
    /// </uL>
    /// </para>
    /// <para>
    /// </para>
    /// </summary>

    public class TAntiSQLInjection
    {

        private TGSqlParser sqlParser = null;
        private string sqlText = null;
        private FileInfo sqlFile = null;
        private List<TSQLInjection> sqlInjections = null;
        private List<ESqlStatementType> enabledStatements = null;

        private bool e_always_true_condition = true;
        private bool e_always_false_condition = true;
        private bool e_comment_at_the_end_of_statement = true;
        private bool e_stacking_queries = true;
        private bool e_not_in_allowed_statement = true;
        private bool e_union_set = true;
        private bool e_piggybacked_statement = true;
        private bool e_syntax_error = true;

        /// <summary>
        /// turn on/off the check of ESQLInjectionType.union_set default is on
        /// </summary>
        /// <param name="on"> </param>
        public virtual void check_union_set(bool on)
        {
            this.e_union_set = on;
        }

        /// <summary>
        /// turn on/off the check of ESQLInjectionType.not_in_allowed_statement
        /// default is on
        /// </summary>
        /// <param name="on"> </param>
        public virtual void check_not_in_allowed_statement(bool on)
        {
            this.e_not_in_allowed_statement = on;
        }

        /// <summary>
        /// turn on/off the check of ESQLInjectionType.stacking_queries default is on
        /// </summary>
        /// <param name="on"> </param>
        public virtual void check_stacking_queries(bool on)
        {
            this.e_stacking_queries = on;
        }

        /// <summary>
        /// turn on/off the check of
        /// ESQLInjectionType.comment_at_the_end_of_statement default is on
        /// </summary>
        /// <param name="on"> </param>
        public virtual void check_comment_at_the_end_of_statement(bool on)
        {
            this.e_comment_at_the_end_of_statement = on;
        }

        /// <summary>
        /// turn on/off the check of ESQLInjectionType.always_false_condition default
        /// is on
        /// </summary>
        /// <param name="on"> </param>
        public virtual void check_always_false_condition(bool on)
        {
            this.e_always_false_condition = on;
        }

        /// <summary>
        /// turn on/off the check of ESQLInjectionType.always_true_condition default
        /// is on
        /// </summary>
        /// <param name="on"> </param>
        public virtual void check_always_true_condition(bool on)
        {
            this.e_always_true_condition = on;
        }

        /// <summary>
        /// turn on/off the check of ESQLInjectionType.piggybacked_statement default
        /// is on
        /// </summary>
        /// <param name="on"> </param>
        public virtual void check_piggybacked_statement(bool on)
        {
            this.e_piggybacked_statement = on;
        }

        /// <summary>
        /// turn on/off the check of ESQLInjectionType.piggybacked_statement default
        /// is on
        /// </summary>
        /// <param name="on"> </param>
        public virtual void check_syntax_error(bool on)
        {
            this.e_syntax_error = on;
        }

        public virtual List<TSQLInjection> SqlInjections
        {
            get
            {
                if (this.sqlInjections == null)
                {
                    this.sqlInjections = new List<TSQLInjection>();
                }
                return sqlInjections;
            }
        }

        public TAntiSQLInjection(EDbVendor dbVendor)
        {
            this.sqlParser = new TGSqlParser(dbVendor);
            this.enabledStatements = new List<ESqlStatementType>();
            this.enabledStatements.Add(ESqlStatementType.sstselect);
        }

        /// <summary>
        /// add a type of sql statement that allowed to be executed in database.
        /// </summary>
        /// <param name="sqltype"> </param>
        public virtual void enableStatement(ESqlStatementType sqltype)
        {
            this.enabledStatements.Add(sqltype);
        }

        /// <summary>
        /// get a list of sql statement type that allowed to be executed in database.
        /// 
        /// @return
        /// </summary>
        public virtual List<ESqlStatementType> EnabledStatements
        {
            get
            {
                return enabledStatements;
            }
        }

        /// <summary>
        /// disable a type of sql statement that allowed to be executed in database.
        /// </summary>
        /// <param name="sqltype"> </param>
        public virtual void disableStatement(ESqlStatementType sqltype)
        {
            for (int i = this.enabledStatements.Count - 1; i >= 0; i--)
            {
                if (this.enabledStatements[i] == sqltype)
                {
                    this.enabledStatements.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Check is sql was injected or not.
        /// </summary>
        /// <param name="sql"> </param>
        /// <returns> if return true, use this.getSqlInjections() to get detailed
        ///         information about sql injection. </returns>
        public virtual bool isInjected(string sql)
        {
            bool ret = false;
            this.sqlText = sql;
            this.sqlParser.sqltext = this.sqlText;
            this.SqlInjections.Clear();
            int i = this.sqlParser.parse();
            if (i == 0)
            {
                ret = ret | Injected_always_false_condition;
                ret = ret | Injected_always_true_condition;
                ret = ret | Injected_comment_at_the_end_statement;
                ret = ret | Injected_stacking_queries;
                ret = ret | Injected_allowed_statement;
                ret = ret | Injected_union_set;
                ret = ret | Injected_piggybacked_statement;
            }
            else if (e_syntax_error)
            {
                TSQLInjection s = new TSQLInjection(ESQLInjectionType.syntax_error);
                s.Description = this.sqlParser.Errormessage;
                this.SqlInjections.Add(s);
                ret = true;
            }

            return ret;
        }

        public virtual bool isInjected(FileInfo file)
        {
            bool ret = false;
            this.sqlFile = file;
            this.sqlParser.sqlfilename = this.sqlFile.FullName;
            this.SqlInjections.Clear();
            int i = this.sqlParser.parse();
            if (i == 0)
            {
                ret = ret | Injected_always_false_condition;
                ret = ret | Injected_always_true_condition;
                ret = ret | Injected_comment_at_the_end_statement;
                ret = ret | Injected_stacking_queries;
                ret = ret | Injected_allowed_statement;
                ret = ret | Injected_union_set;
                ret = ret | Injected_piggybacked_statement;
            }
            else if (e_syntax_error)
            {
                TSQLInjection s = new TSQLInjection(ESQLInjectionType.syntax_error);
                s.Description = this.sqlParser.Errormessage;
                this.SqlInjections.Add(s);
                ret = true;
            }

            return ret;
        }

        private bool Injected_always_true_condition
        {
            get
            {
                bool ret = false;
                if (!this.e_always_true_condition)
                {
                    return false;
                }
                if (this.sqlParser.sqlstatements.size() == 0)
                {
                    return ret;
                }
                for (int i = 0; i < this.sqlParser.sqlstatements.Count; i++)
                {
                    if (this.sqlParser.sqlstatements.get(i).WhereClause != null)
                    {
                        TExpression condition = this.sqlParser.sqlstatements.get(i).WhereClause.Condition;
                        GEval e = new GEval();
                        object t = e.value(condition, null);
                        if (t is bool?)
                        {
                            if (((bool?)t).Value == true)
                            {
                                this.SqlInjections.Add(new TSQLInjection(ESQLInjectionType.always_true_condition));
                                ret = true;
                            }
                        }
                    }
                }
                return ret;
            }
        }

        private bool Injected_always_false_condition
        {
            get
            {
                bool ret = false;
                if (!this.e_always_false_condition)
                {
                    return false;
                }
                if (this.sqlParser.sqlstatements.size() == 0)
                {
                    return ret;
                }
                for (int i = 0; i < this.sqlParser.sqlstatements.Count; i++)
                {
                    if (this.sqlParser.sqlstatements.get(i).WhereClause != null)
                    {
                        TExpression condition = this.sqlParser.sqlstatements.get(i).WhereClause.Condition;
                        GEval e = new GEval();
                        object t = e.value(condition, null);
                        if (t is bool?)
                        {
                            if (((bool?)t).Value == false)
                            {
                                this.SqlInjections.Add(new TSQLInjection(ESQLInjectionType.always_false_condition));
                                ret = true;
                            }
                        }
                    }
                }
                return ret;
            }
        }

        private bool Injected_comment_at_the_end_statement
        {
            get
            {
                bool ret = false;
                if (!this.e_comment_at_the_end_of_statement)
                {
                    return false;
                }
                TSourceToken st = this.sqlParser.sourcetokenlist.get(this.sqlParser.sourcetokenlist.size() - 1);
                if ((st.tokencode == TBaseType.cmtdoublehyphen) || (st.tokencode == TBaseType.cmtslashstar))
                {
                    this.SqlInjections.Add(new TSQLInjection(ESQLInjectionType.comment_at_the_end_of_statement));
                    ret = true;
                }
                return ret;
            }
        }

        private bool Injected_stacking_queries
        {
            get
            {
                bool ret = false;
                if (!this.e_stacking_queries)
                {
                    return false;
                }
                if (this.sqlParser.sqlstatements.size() > 1)
                {
                    this.SqlInjections.Add(new TSQLInjection(ESQLInjectionType.stacking_queries));
                    ret = true;
                }
                return ret;
            }
        }

        private bool Injected_allowed_statement
        {
            get
            {
                bool ret = false;
                if (!this.e_not_in_allowed_statement)
                {
                    return false;
                }
                for (int j = 0; j < this.sqlParser.sqlstatements.size(); j++)
                {
                    if (!this.isAllowedStatement(this.sqlParser.sqlstatements.get(j).sqlstatementtype))
                    {
                        TSQLInjection s = new TSQLInjection(ESQLInjectionType.not_in_allowed_statement);
                        s.Description = this.sqlParser.sqlstatements.get(j).sqlstatementtype.ToString();
                        this.SqlInjections.Add(s);

                        ret = ret | true;
                    };

                }
                return ret;
            }
        }

        private bool Injected_piggybacked_statement
        {
            get
            {
                bool ret = false;
                if (!this.e_piggybacked_statement)
                {
                    return false;
                }
                if (this.sqlParser.sqlstatements.size() == 0)
                {
                    return ret;
                }
                if (this.sqlParser.DbVendor != EDbVendor.dbvmysql)
                {
                    return ret;
                }
                for (int k = 0; k < this.sqlParser.sqlstatements.Count; k++)
                {
                    if (this.sqlParser.sqlstatements.get(k).WhereClause != null)
                    {
                        if (this.sqlParser.sqlstatements.get(k) is TInsertSqlStatement)
                        {
                            TMultiTargetList values = ((TInsertSqlStatement)this.sqlParser.sqlstatements.get(k)).Values;
                            if (values != null)
                            {
                                for (int i = 0; i < values.size(); i++)
                                {
                                    TMultiTarget target = values.getMultiTarget(i);
                                    if (target == null)
                                    {
                                        continue;
                                    }
                                    TResultColumnList columns = target.ColumnList;
                                    for (int j = 0; j < columns.size(); j++)
                                    {
                                        TExpression expression = columns.getResultColumn(j).Expr;
                                        if (expression.ExpressionType == EExpressionType.contains_t)
                                        {
                                            continue;
                                        }
                                        if (isInjectedPiggybackedExpression(expression))
                                        {
                                            this.SqlInjections.Add(new TSQLInjection(ESQLInjectionType.piggybacked_statement));
                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                        else if (this.sqlParser.sqlstatements.get(k) is TUpdateSqlStatement)
                        {
                            TResultColumnList columns = ((TUpdateSqlStatement)this.sqlParser.sqlstatements.get(k)).ResultColumnList;
                            for (int j = 0; j < columns.size(); j++)
                            {
                                TExpression expression = columns.getResultColumn(j).Expr;
                                if (expression.ExpressionType == EExpressionType.assignment_t)
                                {
                                    if (isInjectedPiggybackedExpression(expression.RightOperand))
                                    {
                                        this.SqlInjections.Add(new TSQLInjection(ESQLInjectionType.piggybacked_statement));
                                        return true;
                                    }
                                }
                            }
                        }
                        else if (this.sqlParser.sqlstatements.get(k).WhereClause != null)
                        {
                            if (isInjectedPiggybackedExpression(this.sqlParser.sqlstatements.get(k).WhereClause.Condition))
                            {
                                this.SqlInjections.Add(new TSQLInjection(ESQLInjectionType.piggybacked_statement));
                                return true;
                            }
                        }
                    }
                }
                return ret;
            }
        }
        internal class piggybackedExpr : IExpressionVisitor
        {
            private readonly TAntiSQLInjection outerInstance;

            public piggybackedExpr(TAntiSQLInjection outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            internal bool piggybacked = false;

            public virtual bool exprVisit(TParseTreeNode pNode, bool isLeafNode)
            {
                if (pNode is TExpression)
                {
                    TExpression expr = (TExpression)pNode;
                    if ((Regex.Match(expr.ToString(), "'\\s*'").Success || Regex.Match(expr.ToString(), "\"\\s*\"").Success) && (expr.ParentExpr != null && isLogicExpression(expr.ParentExpr)))
                    {
                        piggybacked = true;
                    }
                }
                return true;
            }

            internal virtual bool isLogicExpression(TExpression expression)
            {
                EExpressionType exprType = expression.ExpressionType;
                return exprType == EExpressionType.logical_and_t || exprType == EExpressionType.logical_not_t || exprType == EExpressionType.logical_or_t || exprType == EExpressionType.logical_t || exprType == EExpressionType.logical_xor_t;
            }

            public virtual piggybackedExpr checkExpression(TExpression expression)
            {
                expression.inOrderTraverse(this);
                return this;
            }

            public virtual bool Piggybacked
            {
                get
                {
                    return piggybacked;
                }
            }

        }

        private bool isInjectedPiggybackedExpression(TExpression expression)
        {
            return (new piggybackedExpr(this)).checkExpression(expression).Piggybacked;
        }

        private bool Injected_union_set
        {
            get
            {
                bool ret = false;
                if (!this.e_union_set)
                {
                    return false;
                }
                if (this.sqlParser.sqlstatements.size() == 0)
                {
                    return ret;
                }
                for (int k = 0; k < this.sqlParser.sqlstatements.Count; k++)
                {
                    TCustomSqlStatement stmt = this.sqlParser.sqlstatements.get(k);
                    if (stmt.sqlstatementtype != ESqlStatementType.sstselect)
                    {
                        return ret;
                    }
                    TSelectSqlStatement select = (TSelectSqlStatement)stmt;
                    if (select.CombinedQuery)
                    {
                        this.SqlInjections.Add(new TSQLInjection(ESQLInjectionType.union_set));
                        ret = true;
                    }
                }
                return ret;
            }
        }

        private bool isAllowedStatement(ESqlStatementType pType)
        {
            bool ret = false;
            for (int i = 0; i < this.enabledStatements.Count; i++)
            {
                if (this.enabledStatements[i] == pType)
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

    }

}