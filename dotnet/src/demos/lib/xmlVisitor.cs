using System;
using System.Collections;
using System.Text;

namespace gudusoft.gsqlparser.demos.lib
{
    using System.IO;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.nodes;
    using gudusoft.gsqlparser.stmt;
    using gudusoft.gsqlparser.stmt.mssql;
    using gudusoft.gsqlparser.stmt.oracle;
    using gudusoft.gsqlparser.stmt.mdx;
    using gudusoft.gsqlparser.nodes.mdx;

    public class xmlVisitor : TParseTreeVisitor
    {
        public const string TAG_SQLSCRIPT = "sqlscript";
        public const string TAG_SELECT_STATEMENT = "select_statement";
        public const string TAG_COLUMN_REFERENCED_EXPR = "column_referenced_expr";
        public const string TAG_CONSTANT_EXPR = "constant_expr";
        public const string TAG_FUNCTIONCALL_EXPR = "functionCall_expr";
        public const string TAG_FUNCTIONCALL = "functionCall";
        public const string TAG_FUNCTIONNAME = "functionName";
        public const string TAG_FUNCTIONARGS = "functionArgs";
        public const string TAG_GENERIC_FUNCTION = "generic_function";
        public const string TAG_CAST_FUNCTION = "cast_function";
        public const string TAG_CONVERT_FUNCTION = "convert_function";
        public const string TAG_TREAT_FUNCTION = "treat_function";
        public const string TAG_CONTAINS_FUNCTION = "contains_function";
        public const string TAG_FREETEXT_FUNCTION = "freetext_function";
        public const string TAG_TRIM_FUNCTION = "trim_function";
        public const string TAG_EXTRACT_FUNCTION = "extract_function";
        public const string TAG_SUBQUERY_EXPR = "subquery_expr";
        public const string TAG_EXISTS_EXPR = "exists_expr";
        public const string TAG_COMPARISON_EXPR = "comparison_expr";
        public const string TAG_LIST_EXPR = "list_expr";
        public const string TAG_IN_EXPR = "in_expr";
        public const string TAG_LIKE_EXPR = "like_expr";
        public const string TAG_BETWEEN_EXPR = "between_expr";
        public const string TAG_NOT_EXPR = "not_expr";
        public const string TAG_UNARY_EXPR = "unary_expr";
        public const string TAG_BINARY_EXPR = "binary_expr";
        public const string TAG_PARENTHESIS_EXPR = "parenthesis_expr";
        public const string TAG_NULL_EXPR = "null_expr";
        public const string TAG_ROW_CONSTRUCTOR_EXPR = "row_constructor_expr";
        public const string TAG_ARRAY_CONSTRUCTOR_EXPR = "array_constructor_expr";
        public const string TAG_CASE_EXPR = "case_expr";
        public const string TAG_ASSIGNMENT_EXPR = "assignment_expr";
        public const string TAG_UNKNOWN_EXPR = "unknown_expr";


        public const string TAG_EXPRESSION = "expression";
        public const string TAG_OBJECTNAME = "objectName";
        public const string TAG_FULLNAME = "full_name";
        public const string TAG_LITERAL = "literal";
        public const string TAG_STATEMENT_LIST = "statement_list";

        public const int TOP_STATEMENT = 1;

        public const string ATTR_EXPR_TYPE = "expr_type";

        private XDocument xmldoc = null;
        private XElement e_parent = null;
        private Stack elementStack = null;
        private string current_expression_tag = null;
        private string current_expression_list_tag = null;
        private string current_objectName_tag = null;
        private string current_statement_list_tag = null;
        private string current_objectName_list_tag = null;
        private string current_query_expression_tag = null;
        private string current_functionCall_tag = null;
        private string current_table_reference_tag = null;
        private string current_join_table_reference_tag = null;
        private string current_parameter_declaration_tag = null;
        private string current_datatype_tag = null;
        private EDbVendor dbVendor;
        private XNamespace defaultNamespace = XNamespace.Get("http://www.sqlparser.com/xml/sqlschema/1.0");

        public virtual void run(TGSqlParser sqlParser)
        {
            dbVendor = sqlParser.DbVendor;
            XElement e_sqlScript = new XElement(defaultNamespace + "sqlscript");
            XNamespace xsiNs = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
            e_sqlScript.Add(new XAttribute("dbvendor", sqlParser.DbVendor.ToString()));
            e_sqlScript.Add(new XAttribute("stmt_count", sqlParser.sqlstatements.Count.ToString()));
            e_sqlScript.Add(new XAttribute(XNamespace.Xmlns + "xsi", xsiNs.NamespaceName));
            xmldoc.Add(e_sqlScript);

            elementStack.Push(e_sqlScript);
            for (int i = 0; i < sqlParser.sqlstatements.Count; i++)
            {
                XElement e_statement = new XElement(defaultNamespace + "statement");
                e_statement.Add(new XAttribute("type", sqlParser.sqlstatements.get(i).sqlstatementtype.ToString()));
                e_sqlScript.Add(e_statement);
                elementStack.Push(e_statement);
                sqlParser.sqlstatements.get(i).DummyTag = TOP_STATEMENT;
                sqlParser.sqlstatements.get(i).accept(this);
                elementStack.Pop();
            }
            elementStack.Pop();
        }

        internal class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb) { }

            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }

        public virtual string FormattedXml
        {
            get
            {
                StringBuilder buffer = new StringBuilder();
                using (StringWriter writer = new Utf8StringWriter(buffer))
                {
                    xmldoc.Save(writer, SaveOptions.None);
                }
                return buffer.ToString().Trim();
            }
        }

        public virtual void writeToFile(string filename)
        {

            try
            {
                StreamWriter @out = new System.IO.StreamWriter(new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write));
                try
                {
                    @out.Write(FormattedXml);
                }
                finally
                {
                    @out.Close();
                }

                //                // write(testFile,buffer.ToString());
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

        }

        public const string crlf = "\r\n";
        internal StringBuilder sb;

        public xmlVisitor()
        {
            sb = new StringBuilder(1024);
            elementStack = new Stack();
            xmldoc = new XDocument();
            XDeclaration declaration = new XDeclaration("1.0", "utf-8", "no");
            xmldoc.Declaration = declaration;
        }


        internal virtual void appendEndTag(string tagName)
        {
            sb.Append(string.Format("</{0}>" + crlf, tagName));
        }

        internal virtual void appendStartTag(string tagName)
        {
            sb.Append(string.Format("<{0}>" + crlf, tagName));
        }

        internal virtual string getTagName(TParseTreeNode node)
        {
            return node.GetType().Name;
        }


        internal virtual void appendStartTag(TParseTreeNode node)
        {

            if (node is TStatementList)
            {
                appendStartTagWithCount(node, ((TStatementList)node).Count);
            }
            else if (node is TParseTreeNodeList)
            {
                appendStartTagWithCount(node, ((TParseTreeNodeList)node).Count);
            }
            else
            {
                sb.Append(string.Format("<{0}>" + crlf, getTagName(node)));
            }
        }

        internal virtual void appendStartTagWithIntProperty(TParseTreeNode node, string propertyName, int propertyValue)
        {
            sb.Append(string.Format("<{0} " + propertyName + "='{1:D}'>" + crlf, getTagName(node), propertyValue));
        }

        internal virtual void appendStartTagWithIntProperty(TParseTreeNode node, string propertyName, EExpressionType propertyValue)
        {
            sb.Append(string.Format("<{0} " + propertyName + "='{1}'>" + crlf, getTagName(node), Enum.GetName(typeof(EExpressionType), propertyValue)));
        }

        internal virtual void appendStartTagWithIntProperty(TParseTreeNode node, string propertyName, string propertyValue)
        {
            sb.Append(string.Format("<{0} " + propertyName + "='{1}'>" + crlf, getTagName(node), propertyValue));
        }

        internal virtual void appendStartTagWithIntProperty(TParseTreeNode node, string propertyName, int propertyValue, string propertyName2, string propertyValue2)
        {
            sb.Append(string.Format("<{0} " + propertyName + "='{1:D}' " + propertyName2 + "='{2}'" + ">" + crlf, getTagName(node), propertyValue, propertyValue2));
        }

        internal virtual void appendStartTagWithIntProperty(TParseTreeNode node, string propertyName, string propertyValue, string propertyName2, string propertyValue2)
        {
            sb.Append(string.Format("<{0} " + propertyName + "='{1}' " + propertyName2 + "='{2}'" + ">" + crlf, getTagName(node), propertyValue, propertyValue2));
        }

        internal virtual void appendStartTagWithIntProperty(TParseTreeNode node, string propertyName, string propertyValue, string propertyName2, string propertyValue2, string propertyName3, string propertyValue3)
        {
            sb.Append(string.Format("<{0} " + propertyName + "='{1}' " + propertyName2 + "='{2}' " + propertyName3 + "='{3}'" + ">" + crlf, getTagName(node), propertyValue, propertyValue2, propertyValue3));
        }

        internal virtual void appendStartTagWithProperties(TParseTreeNode node, string propertyName, string propertyValue, string propertyName2, string propertyValue2)
        {
            sb.Append(string.Format("<{0} " + propertyName + "='{1}' " + propertyName2 + "='{2}'" + ">" + crlf, getTagName(node), propertyValue, propertyValue2));
        }

        internal virtual void appendEndTag(TParseTreeNode node)
        {
            sb.Append(string.Format("</{0}>" + crlf, getTagName(node)));
        }

        internal virtual void appendStartTagWithCount(TParseTreeNode node, int count)
        {
            appendStartTagWithIntProperty(node, "size", count);
        }

        // process parse tree nodes




        public override void preVisit(TConstant node)
        {
            XElement e_literal = new XElement(defaultNamespace + TAG_LITERAL);
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_literal);

            XElement e_value = new XElement(defaultNamespace + "value");
            e_literal.Add(e_value);
            e_value.Value = node.ToString();
            //e_literal.
            //appendStartTag(node);
            //sb.append(node.ToString());
        }
        public override void postVisit(TConstant node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TTopClause node)
        {
            XElement e_top_clause = new XElement(defaultNamespace + "top_clause");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_top_clause);
            elementStack.Push(e_top_clause);
            e_top_clause.Add(new XAttribute("percent", node.Percent.ToString()));
            e_top_clause.Add(new XAttribute("with_ties", node.Withties.ToString()));

            if (node.Expr != null)
            {
                node.Expr.accept(this);
            }

            elementStack.Pop();
        }

        public override void postVisit(TTopClause node)
        {
            appendEndTag(node);
        }


        public override void preVisit(TSelectSqlStatement node)
        {
            //        sb.append(String.format("<%s setOperator='%d'>"+crlf,getTagName(node),node.getSetOperator()) );
            //            appendStartTagWithProperties(node, "setOperator",node.getSetOperatorType().ToString()
            //                    , "isAll", String.valueOf(node.isAll())) ;

            XElement e_query_expression;

            if (node.DummyTag == TOP_STATEMENT)
            {
                //if (elementStack.Count == 2){ // tag stack: sqlscript/statement
                e_parent = (XElement)elementStack.Peek();
                XElement e_select = new XElement(defaultNamespace + "select_statement");
                e_parent.Add(e_select);
                elementStack.Push(e_select);

                if (node.CteList != null)
                {
                    node.CteList.accept(this);
                }
            }

            string query_expression_tag = "query_expression";
            if (!string.ReferenceEquals(current_query_expression_tag, null))
            {
                query_expression_tag = current_query_expression_tag;
                current_query_expression_tag = null;
            }
            e_parent = (XElement)elementStack.Peek();
            e_query_expression = new XElement(defaultNamespace + query_expression_tag);
            e_query_expression.Add(new XAttribute("is_parenthesis", (node.ParenthesisCount > 0).ToString()));
            e_parent.Add(e_query_expression);
            elementStack.Push(e_query_expression);

            if (node.CombinedQuery)
            {

                XElement e_binary_query_expression = new XElement(defaultNamespace + "binary_query_expression");
                e_binary_query_expression.Add(new XAttribute("set_operator", node.SetOperatorType.ToString()));
                e_binary_query_expression.Add(new XAttribute("is_all", node.All.ToString()));

                e_query_expression.Add(e_binary_query_expression);
                elementStack.Push(e_binary_query_expression);
                current_query_expression_tag = "first_query_expression";
                node.LeftStmt.accept(this);
                current_query_expression_tag = "second_query_expression";
                node.RightStmt.accept(this);
                elementStack.Pop();

                if (node.OrderbyClause != null)
                {
                    node.OrderbyClause.accept(this);
                }

                if (node.LimitClause != null)
                {
                    node.LimitClause.accept(this);
                }

                if (node.ForUpdateClause != null)
                {
                    node.ForUpdateClause.accept(this);
                }

                if (node.ComputeClause != null)
                {
                    node.ComputeClause.accept(this);
                }

                //this.postVisit(node);
                elementStack.Pop();

                return;
            }

            e_parent = (XElement)elementStack.Peek();

            XElement e_query_specification = new XElement(defaultNamespace + "query_specification");
            e_parent.Add(e_query_specification);
            e_parent = e_query_specification;
            elementStack.Push(e_query_specification);

            //        if (node.getCteList() != null){
            //            node.getCteList().accept(this);
            //        }

            if (node.ValueClause != null)
            {
                // DB2 values constructor

                elementStack.Pop(); // query specification
                elementStack.Pop(); // query expression

                return;
            }
            if (node.TopClause != null)
            {
                node.TopClause.accept(this);
            }

            if (node.ResultColumnList != null)
            {
                XElement e_select_list = new XElement(defaultNamespace + "select_list");
                e_parent.Add(e_select_list);
                elementStack.Push(e_select_list);
                for (int i = 0; i < node.ResultColumnList.Count; i++)
                {
                    node.ResultColumnList.getResultColumn(i).accept(this);
                }
                elementStack.Pop();
            }
            else
            {
                // hive transform clause with no select list
            }

            if (node.IntoClause != null)
            {
                node.IntoClause.accept(this);
            }

            if (node.joins.Count > 0)
            {
                XElement e_from_clause = new XElement(defaultNamespace + "from_clause");
                e_parent = (XElement)elementStack.Peek();
                e_parent.Add(e_from_clause);
                elementStack.Push(e_from_clause);
                node.joins.accept(this);
                elementStack.Pop();
            }

            if (node.WhereClause != null)
            {
                node.WhereClause.accept(this);
            }

            if (node.HierarchicalClause != null)
            {
                node.HierarchicalClause.accept(this);
            }

            if (node.GroupByClause != null)
            {
                node.GroupByClause.accept(this);
            }

            if (node.QualifyClause != null)
            {
                node.QualifyClause.accept(this);
            }

            if (node.OrderbyClause != null)
            {
                node.OrderbyClause.accept(this);
            }

            if (node.LimitClause != null)
            {
                node.LimitClause.accept(this);
            }

            if (node.ForUpdateClause != null)
            {
                node.ForUpdateClause.accept(this);
            }

            if (node.ComputeClause != null)
            {
                node.ComputeClause.accept(this);
            }

            elementStack.Pop(); // query specification
            elementStack.Pop(); // query expression

        }

        public override void postVisit(TSelectSqlStatement node)
        {
            if (node.DummyTag == TOP_STATEMENT)
            {
                //if (elementStack.Count == 3) {
                XElement tmp = (XElement)elementStack.Peek();
                if (tmp.Name.LocalName.Equals("select_statement", StringComparison.OrdinalIgnoreCase))
                {
                    elementStack.Pop(); // tag stack: sqlscript/statement/select_statement
                }
            }
            appendEndTag(node);
        }

        public override void preVisit(TResultColumnList node)
        {
            appendStartTag(node);
            for (int i = 0; i < node.Count; i++)
            {
                node.getResultColumn(i).accept(this);
            }
        }

        public override void postVisit(TResultColumnList node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TResultColumn node)
        {
            // appendStartTag(node);
            e_parent = (XElement)elementStack.Peek();
            XElement e_result_column = new XElement(defaultNamespace + "result_column");
            e_parent.Add(e_result_column);
            elementStack.Push(e_result_column);

            node.Expr.accept(this);
            if (node.AliasClause != null)
            {
                node.AliasClause.accept(this);
            }
            elementStack.Pop();
        }

        public override void postVisit(TResultColumn node)
        {

            //appendEndTag(node);
        }

        public override void preVisit(TExpression node)
        {
            string tag_name = TAG_EXPRESSION;
            if (!string.ReferenceEquals(current_expression_tag, null))
            {
                tag_name = current_expression_tag;
                current_expression_tag = null;
            }
            XElement e_expression = new XElement(defaultNamespace + tag_name);

            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_expression);
            elementStack.Push(e_expression);
            e_expression.Add(new XAttribute(ATTR_EXPR_TYPE, node.ExpressionType.ToString()));

            switch (node.ExpressionType)
            {
                case EExpressionType.simple_object_name_t:
                    e_expression = new XElement(defaultNamespace + TAG_COLUMN_REFERENCED_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    current_objectName_tag = null;
                    node.ObjectOperand.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.simple_constant_t:
                    e_expression = new XElement(defaultNamespace + TAG_CONSTANT_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    node.ConstantOperand.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.new_structured_type_t:
                case EExpressionType.type_constructor_t:
                case EExpressionType.function_t:
                    e_expression = new XElement(defaultNamespace + TAG_FUNCTIONCALL_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    node.FunctionCall.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.cursor_t:
                case EExpressionType.multiset_t:
                case EExpressionType.subquery_t:
                    e_expression = new XElement(defaultNamespace + TAG_SUBQUERY_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    node.SubQuery.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.exists_t:
                    e_expression = new XElement(defaultNamespace + TAG_EXISTS_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    node.SubQuery.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.assignment_t:
                    e_expression = new XElement(defaultNamespace + TAG_ASSIGNMENT_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    current_expression_tag = "first_expr";
                    node.LeftOperand.accept(this);
                    current_expression_tag = "second_expr";
                    node.RightOperand.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.simple_comparison_t:
                    e_expression = new XElement(defaultNamespace + TAG_COMPARISON_EXPR);
                    e_expression.Add(new XAttribute("type", node.ComparisonOperator.ToString()));
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);

                    if (node.SubQuery != null)
                    {
                        node.ExprList.accept(this);
                        node.SubQuery.accept(this);
                    }
                    else
                    {
                        current_expression_tag = "first_expr";
                        node.LeftOperand.accept(this);
                        current_expression_tag = "second_expr";
                        node.RightOperand.accept(this);
                    }
                    elementStack.Pop();
                    break;
                case EExpressionType.group_comparison_t:
                    e_expression = new XElement(defaultNamespace + TAG_COMPARISON_EXPR);
                    e_expression.Add(new XAttribute("type", node.ComparisonOperator.ToString()));
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);


                    if (node.Quantifier != null)
                    {
                        e_expression.Add(new XAttribute("quantifier", node.Quantifier.ToString()));
                    }

                    current_expression_tag = "first_expr";
                    if (node.ExprList != null)
                    {
                        node.ExprList.accept(this);
                    }
                    else
                    {
                        node.LeftOperand.accept(this);
                    }

                    current_expression_tag = "second_expr";
                    node.RightOperand.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.in_t:
                    e_expression = new XElement(defaultNamespace + TAG_IN_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    e_expression.Add(new XAttribute("not", (node.NotToken != null) ? "true" : "false"));
                    elementStack.Push(e_expression);
                    current_expression_tag = "first_expr";
                    if (node.ExprList != null)
                    {
                        node.ExprList.accept(this);
                    }
                    else
                    {
                        node.LeftOperand.accept(this);
                    }

                    current_expression_tag = "second_expr";
                    node.RightOperand.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.collection_constructor_list_t:
                case EExpressionType.collection_constructor_multiset_t:
                case EExpressionType.collection_constructor_set_t:
                case EExpressionType.list_t:
                    e_expression = new XElement(defaultNamespace + TAG_LIST_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);

                    if (node.ExprList != null)
                    {
                        node.ExprList.accept(this);
                    }
                    elementStack.Pop();
                    break;
                case EExpressionType.pattern_matching_t:
                    e_expression = new XElement(defaultNamespace + TAG_LIKE_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    current_expression_tag = "first_expr";
                    node.LeftOperand.accept(this);
                    current_expression_tag = "second_expr";
                    node.RightOperand.accept(this);
                    if (node.LikeEscapeOperand != null)
                    {
                        current_expression_tag = "third_expr";
                        node.LikeEscapeOperand.accept(this);
                    }
                    elementStack.Pop();
                    break;
                case EExpressionType.between_t:
                    e_expression = new XElement(defaultNamespace + TAG_BETWEEN_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    current_expression_tag = "first_expr";
                    node.BetweenOperand.accept(this);
                    current_expression_tag = "second_expr";
                    node.LeftOperand.accept(this);
                    current_expression_tag = "third_expr";
                    node.RightOperand.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.logical_not_t:
                    e_expression = new XElement(defaultNamespace + TAG_NOT_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    current_expression_tag = "first_expr";
                    node.RightOperand.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.null_t:
                    e_expression = new XElement(defaultNamespace + TAG_NULL_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    e_expression.Add(new XAttribute("not", (node.NotToken != null) ? "true" : "false"));
                    elementStack.Push(e_expression);
                    current_expression_tag = "first_expr";
                    node.LeftOperand.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.parenthesis_t:
                    e_expression = new XElement(defaultNamespace + TAG_PARENTHESIS_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    current_expression_tag = "first_expr";
                    node.LeftOperand.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.at_local_t:
                case EExpressionType.day_to_second_t:
                case EExpressionType.year_to_month_t:
                case EExpressionType.floating_point_t:
                case EExpressionType.is_of_type_t:
                case EExpressionType.typecast_t:
                case EExpressionType.unary_factorial_t:
                    e_expression = new XElement(defaultNamespace + TAG_UNARY_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    current_expression_tag = "first_expr";
                    node.LeftOperand.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.unary_plus_t:
                case EExpressionType.unary_minus_t:
                case EExpressionType.unary_prior_t:
                case EExpressionType.unary_connect_by_root_t:
                case EExpressionType.unary_binary_operator_t:
                case EExpressionType.unary_squareroot_t:
                case EExpressionType.unary_cuberoot_t:
                case EExpressionType.unary_factorialprefix_t:
                case EExpressionType.unary_absolutevalue_t:
                case EExpressionType.unary_bitwise_not_t:
                    e_expression = new XElement(defaultNamespace + TAG_UNARY_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    current_expression_tag = "first_expr";
                    node.RightOperand.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.arithmetic_plus_t:
                case EExpressionType.arithmetic_minus_t:
                case EExpressionType.arithmetic_times_t:
                case EExpressionType.arithmetic_divide_t:
                case EExpressionType.power_t:
                case EExpressionType.range_t:
                case EExpressionType.concatenate_t:
                case EExpressionType.period_ldiff_t:
                case EExpressionType.period_rdiff_t:
                case EExpressionType.period_p_intersect_t:
                case EExpressionType.period_p_normalize_t:
                case EExpressionType.contains_t:
                case EExpressionType.arithmetic_modulo_t:
                case EExpressionType.bitwise_exclusive_or_t:
                case EExpressionType.bitwise_or_t:
                case EExpressionType.bitwise_and_t:
                case EExpressionType.bitwise_xor_t:
                case EExpressionType.exponentiate_t:
                case EExpressionType.scope_resolution_t:
                case EExpressionType.at_time_zone_t:
                case EExpressionType.member_of_t:
                case EExpressionType.logical_and_t:
                case EExpressionType.logical_or_t:
                case EExpressionType.logical_xor_t:
                case EExpressionType.is_t:
                case EExpressionType.collate_t:
                case EExpressionType.left_join_t:
                case EExpressionType.right_join_t:
                case EExpressionType.ref_arrow_t:
                case EExpressionType.left_shift_t:
                case EExpressionType.right_shift_t:
                case EExpressionType.bitwise_shift_left_t:
                case EExpressionType.bitwise_shift_right_t:
                case EExpressionType.multiset_union_t:
                case EExpressionType.multiset_union_distinct_t:
                case EExpressionType.multiset_intersect_t:
                case EExpressionType.multiset_intersect_distinct_t:
                case EExpressionType.multiset_except_t:
                case EExpressionType.multiset_except_distinct_t:
                case EExpressionType.json_get_text:
                case EExpressionType.json_get_text_at_path:
                case EExpressionType.json_get_object:
                case EExpressionType.json_get_object_at_path:
                case EExpressionType.json_left_contain:
                case EExpressionType.json_right_contain:
                case EExpressionType.json_exist:
                case EExpressionType.json_any_exist:
                case EExpressionType.json_all_exist:
                case EExpressionType.sqlserver_proprietary_column_alias_t:
                    e_expression = new XElement(defaultNamespace + TAG_BINARY_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    if (node.OperatorToken != null)
                    {
                        e_expression.Add(new XAttribute("operator", node.OperatorToken.ToString()));
                    }
                    elementStack.Push(e_expression);
                    current_expression_tag = "first_expr";
                    node.LeftOperand.accept(this);
                    current_expression_tag = "second_expr";
                    node.RightOperand.accept(this);
                    elementStack.Pop();
                    break;
                case EExpressionType.row_constructor_t:
                    e_expression = new XElement(defaultNamespace + TAG_ROW_CONSTRUCTOR_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);

                    if (node.ExprList != null)
                    {
                        node.ExprList.accept(this);
                    }
                    elementStack.Pop();
                    break;
                case EExpressionType.array_constructor_t:
                    e_expression = new XElement(defaultNamespace + TAG_ARRAY_CONSTRUCTOR_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    elementStack.Push(e_expression);
                    if (node.SubQuery != null)
                    {
                        node.SubQuery.accept(this);
                    }

                    if (node.ExprList != null)
                    {
                        node.ExprList.accept(this);
                    }
                    elementStack.Pop();
                    break;
                case EExpressionType.case_t:
                    node.CaseExpression.accept(this);
                    break;
                case EExpressionType.arrayaccess_t:
                    e_expression = new XElement(defaultNamespace + TAG_UNKNOWN_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    e_expression.Value = node.ToString();
                    //node.getArrayAccess().accept(this);
                    break;
                case EExpressionType.interval_t:
                    e_expression = new XElement(defaultNamespace + TAG_UNKNOWN_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    e_expression.Value = node.ToString();
                    //node.getIntervalExpr().accept(this);
                    break;
                default:
                    e_expression = new XElement(defaultNamespace + TAG_UNKNOWN_EXPR);
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_expression);
                    e_expression.Value = node.ToString();
                    break;
            }

            elementStack.Pop();
        }

        public override void postVisit(TExpression node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TAliasClause node)
        {
            XElement e_alias_clause = new XElement(defaultNamespace + "alias_clause");
            e_alias_clause.Add(new XAttribute("with_as", (node.AsToken != null) ? "true" : "false"));
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_alias_clause);
            elementStack.Push(e_alias_clause);
            current_objectName_tag = "object_name";
            node.AliasName.accept(this);
            current_objectName_tag = null;
            elementStack.Pop();
            //sb.append(node.ToString());
        }


        public override void preVisit(TInExpr node)
        {
            appendStartTag(node);
            if (node.SubQuery != null)
            {
                node.SubQuery.accept(this);
            }
            else if (node.GroupingExpressionItemList != null)
            {
                node.GroupingExpressionItemList.accept(this);
            }
            else
            {
                sb.Append(node.ToString());
            }
        }

        public override void postVisit(TInExpr node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TExpressionList node)
        {
            //appendStartTag(node);
            XElement e_expression_list;
            e_parent = (XElement)elementStack.Peek();
            if (string.ReferenceEquals(current_expression_list_tag, null))
            {
                e_expression_list = new XElement(defaultNamespace + "expression_list");
            }
            else
            {
                e_expression_list = new XElement(defaultNamespace + current_expression_list_tag);
                current_expression_list_tag = null;
            }

            e_parent.Add(e_expression_list);

            elementStack.Push(e_expression_list);

            for (int i = 0; i < node.Count; i++)
            {
                node.getExpression(i).accept(this);
            }
            elementStack.Pop();
        }

        public override void postVisit(TExpressionList node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TGroupingExpressionItem node)
        {
            appendStartTag(node);
            if (node.Expr != null)
            {
                node.Expr.accept(this);
            }
            else if (node.ExprList != null)
            {
                node.ExprList.accept(this);
            }
        }

        public override void postVisit(TGroupingExpressionItem node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TGroupingExpressionItemList node)
        {
            appendStartTag(node);
        }

        public override void postVisit(TGroupingExpressionItemList node)
        {
            appendEndTag(node);
        }

        public override void postVisit(TAliasClause node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TJoin node)
        {

            switch (node.Kind)
            {
                case TBaseType.join_source_fake:
                    node.Table.accept(this);
                    break;
                case TBaseType.join_source_table:
                case TBaseType.join_source_join:

                    string tag_name = "table_reference";

                    if (!string.ReferenceEquals(current_join_table_reference_tag, null))
                    {
                        tag_name = current_join_table_reference_tag;
                        current_join_table_reference_tag = null;
                    }

                    XElement e_table_reference = new XElement(defaultNamespace + tag_name);
                    e_table_reference.Add(new XAttribute("type", "join"));
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_table_reference);
                    elementStack.Push(e_table_reference);

                    int nest = 0;
                    for (int i = node.JoinItems.Count - 1; i >= 0; i--)
                    {
                        TJoinItem joinItem = node.JoinItems.getJoinItem(i);
                        XElement e_joined_table_reference = new XElement(defaultNamespace + "joined_table");
                        e_joined_table_reference.Add(new XAttribute("type", joinItem.JoinType.ToString()));
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_joined_table_reference);
                        elementStack.Push(e_joined_table_reference);
                        nest++;

                        XElement e_first_table_reference = null;
                        if (i != 0)
                        {
                            e_first_table_reference = new XElement(defaultNamespace + "first_table_reference");
                            e_first_table_reference.Add(new XAttribute("type", "join"));
                            e_parent = (XElement)elementStack.Peek();
                            e_parent.Add(e_first_table_reference);

                        }
                        else
                        {
                            if (node.Kind == TBaseType.join_source_table)
                            {
                                current_table_reference_tag = "first_table_reference";
                                node.Table.accept(this);
                            }
                            else if (node.Kind == TBaseType.join_source_join)
                            {
                                current_join_table_reference_tag = "first_table_reference";
                                preVisit(node.Join);
                            }
                        }

                        if (joinItem.Table != null)
                        {
                            current_table_reference_tag = "second_table_reference";
                            joinItem.Table.accept(this);
                        }
                        else if (joinItem.Join != null)
                        {
                            current_join_table_reference_tag = "second_table_reference";
                            preVisit(joinItem.Join);
                        }

                        if (joinItem.OnCondition != null)
                        {
                            current_expression_tag = "join_condition";
                            joinItem.OnCondition.accept(this);
                        }

                        if (i != 0)
                        {
                            elementStack.Push(e_first_table_reference);
                            nest++;
                        }

                    }

                    for (int i = 0; i < nest; i++)
                    {
                        elementStack.Pop();
                    }


                    elementStack.Pop(); // e_table_reference
                    break;
                    //                case TBaseType.join_source_join:
                    //                    node.getJoin().accept(this);
                    //                    node.getJoinItems().accept(this);
                    //                    break;
            }

            //            if (node.getAliasClause() != null){
            //                node.getAliasClause().accept(this);
            //            }



        }


        public override void preVisit(TJoinList node)
        {
            for (int i = 0; i < node.Count; i++)
            {
                node.getJoin(i).accept(this);
            }
        }


        public override void preVisit(TJoinItem node)
        {
            appendStartTagWithIntProperty(node, "jointype", node.JoinType.ToString());
            if (node.Kind == TBaseType.join_source_table)
            {
                node.Table.accept(this);
            }
            else if (node.Kind == TBaseType.join_source_join)
            {
                node.Join.accept(this);
            }

            if (node.OnCondition != null)
            {
                node.OnCondition.accept(this);
            }

            if (node.UsingColumns != null)
            {
                node.UsingColumns.accept(this);
            }
        }

        public override void postVisit(TJoinItem node)
        {
            appendEndTag(node);
        }
        public override void preVisit(TJoinItemList node)
        {
            appendStartTag(node);
            for (int i = 0; i < node.Count; i++)
            {
                node.getJoinItem(i).accept(this);
            }
        }


        public override void preVisit(TUnpivotInClauseItem node)
        {
            appendStartTag(node);
            outputNodeData(node);
        }


        public override void preVisit(TUnpivotInClause node)
        {
            appendStartTag(node);
            for (int i = 0; i < node.Items.Count; i++)
            {
                node.Items[i].accept(this);
            }

        }


        public override void preVisit(TPivotInClause node)
        {
            appendStartTag(node);
            if (node.Items != null)
            {
                node.Items.accept(this);
            }
            if (node.SubQuery != null)
            {
                node.SubQuery.accept(this);
            }

        }


        public override void preVisit(TPivotedTable node)
        {
            //appendStartTag(node);
            //node.getJoins().accept(this);
            XElement e_pivoted_table_reference, e_table_reference;
            TPivotClause pivotClause;

            e_pivoted_table_reference = new XElement(defaultNamespace + "pivoted_table_reference");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_pivoted_table_reference);
            elementStack.Push(e_pivoted_table_reference);


            for (int i = node.PivotClauseList.Count - 1; i >= 0; i--)
            {
                pivotClause = node.PivotClauseList[i];
                if (pivotClause.AliasClause != null)
                {
                    pivotClause.AliasClause.accept(this);
                }
                pivotClause.accept(this);

                if (i == 0)
                {
                    e_table_reference = new XElement(defaultNamespace + "table_reference");
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_table_reference);
                    elementStack.Push(e_table_reference);

                    node.TableSource.accept(this);
                    elementStack.Pop();
                }
                else
                {
                    e_table_reference = new XElement(defaultNamespace + "table_reference");
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_table_reference);
                    elementStack.Push(e_table_reference);


                    e_pivoted_table_reference = new XElement(defaultNamespace + "pivoted_table_reference");
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_pivoted_table_reference);
                    elementStack.Push(e_pivoted_table_reference);

                }

            }


            for (int i = node.PivotClauseList.Count - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                }
                else
                {
                    elementStack.Pop();
                    elementStack.Pop();
                }
            }
            elementStack.Pop();


        }


        public override void preVisit(TPivotClause node)
        {
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(new XAttribute("type", (node.Type == TPivotClause.pivot) ? "pivot" : "unpivot"));
            if (node.Aggregation_function != null)
            {
                //            XElement e_aggregate_function = new XElement(defaultNamespace +  "aggregateFunction");
                //            e_parent = (XElement)elementStack.Peek();
                //            e_parent.Add(e_aggregate_function);
                //            elementStack.Push(e_aggregate_function);
                current_functionCall_tag = "aggregateFunction";
                node.Aggregation_function.accept(this);
                //            elementStack.Pop();
            }
            if (node.ValueColumn != null)
            {
                current_objectName_tag = "valueColumn";
                node.ValueColumn.accept(this);
            }
            if (node.ValueColumnList != null)
            {
                for (int i = 0; i < node.ValueColumnList.Count; i++)
                {
                    node.ValueColumnList.getObjectName(i).accept(this);
                }
            }
            if (node.PivotColumn != null)
            {
                current_objectName_tag = "pivotColumn";
                node.PivotColumn.accept(this);
            }
            if (node.PivotColumnList != null)
            {
                current_objectName_list_tag = "inColumns";
                node.PivotColumnList.accept(this);
                //            for(int i=0;i<node.getPivotColumnList().Count;i++){
                //                node.getPivotColumnList().getObjectName(i).accept(this);
                //            }
            }

            if (node.Aggregation_function_list != null)
            {
                node.Aggregation_function_list.accept(this);
            }

            if (node.In_result_list != null)
            {
                node.In_result_list.accept(this);
            }

            if (node.PivotInClause != null)
            {
                node.PivotInClause.accept(this);
            }

            if (node.UnpivotInClause != null)
            {
                node.UnpivotInClause.accept(this);
            }

            //        if (node.getAliasClause() != null){
            //            node.getAliasClause().accept(this);
            //        }

        }


        public override void preVisit(TTable node)
        {
            //        appendStartTagWithIntProperty(node,"type",node.getTableType().ToString());
            string tag_name = "table_reference";
            if (!string.ReferenceEquals(current_table_reference_tag, null))
            {
                tag_name = current_table_reference_tag;
                current_table_reference_tag = null;
            }
            XElement e_table_reference = new XElement(defaultNamespace + tag_name);
            e_table_reference.Add(new XAttribute("type", node.TableType.ToString()));
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_table_reference);
            elementStack.Push(e_table_reference);

            switch (node.TableType)
            {
                case ETableSource.objectname:
                    {
                        XElement e_named_table_reference = new XElement(defaultNamespace + "named_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_named_table_reference);
                        elementStack.Push(e_named_table_reference);
                        current_objectName_tag = "table_name";
                        node.TableName.accept(this);
                        current_objectName_tag = null;

                        if (node.AliasClause != null)
                        {
                            node.AliasClause.accept(this);
                        }

                        elementStack.Pop();
                        //sb.append(node.ToString().replace(">","&#62;").replace("<","&#60;"));
                        break;
                    }
                case ETableSource.tableExpr:
                    {
                        e_table_reference = new XElement(defaultNamespace + "expr_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);

                        current_expression_tag = "table_expr";
                        node.TableExpr.accept(this);

                        if (node.AliasClause != null)
                        {
                            node.AliasClause.accept(this);
                        }

                        elementStack.Pop();
                        break;
                    }
                case ETableSource.subquery:
                    {
                        e_table_reference = new XElement(defaultNamespace + "query_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        node.Subquery.accept(this);
                        if (node.AliasClause != null)
                        {
                            node.AliasClause.accept(this);
                        }
                        elementStack.Pop();
                        break;
                    }
                case ETableSource.function:
                    {
                        e_table_reference = new XElement(defaultNamespace + "functionCall_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        current_functionCall_tag = "func_expr";
                        node.FuncCall.accept(this);
                        if (node.AliasClause != null)
                        {
                            node.AliasClause.accept(this);
                        }
                        elementStack.Pop();
                        break;
                    }
                case ETableSource.pivoted_table:
                    {
                        node.PivotedTable.accept(this);
                        break;
                    }
                case ETableSource.output_merge:
                    {
                        e_table_reference = new XElement(defaultNamespace + "not_decode_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        //node.getOutputMerge().accept(this);
                        e_table_reference.Value = node.OutputMerge.ToString();
                        elementStack.Pop();
                        break;
                    }
                case ETableSource.containsTable:
                    {
                        e_table_reference = new XElement(defaultNamespace + "named_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        node.ContainsTable.accept(this);
                        elementStack.Pop();

                        break;
                    }

                case ETableSource.openrowset:
                    {
                        e_table_reference = new XElement(defaultNamespace + "named_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        node.OpenRowSet.accept(this);
                        elementStack.Pop();
                        break;
                    }

                case ETableSource.openxml:
                    {
                        e_table_reference = new XElement(defaultNamespace + "named_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        node.OpenXML.accept(this);
                        elementStack.Pop();
                        break;
                    }

                case ETableSource.opendatasource:
                    {
                        e_table_reference = new XElement(defaultNamespace + "named_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        node.OpenDatasource.accept(this);
                        elementStack.Pop();
                        break;
                    }

                case ETableSource.openquery:
                    {
                        e_table_reference = new XElement(defaultNamespace + "named_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        node.Openquery.accept(this);
                        elementStack.Pop();
                        break;
                    }

                case ETableSource.datachangeTable:
                    {
                        e_table_reference = new XElement(defaultNamespace + "named_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        node.DatachangeTable.accept(this);
                        elementStack.Pop();
                        break;
                    }
                case ETableSource.rowList:
                    {
                        e_table_reference = new XElement(defaultNamespace + "named_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        node.RowList.accept(this);
                        elementStack.Pop();
                        break;
                    }
                case ETableSource.xmltable:
                    {
                        e_table_reference = new XElement(defaultNamespace + "named_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        node.XmlTable.accept(this);
                        elementStack.Pop();
                        break;
                    }

                case ETableSource.informixOuter:
                    {
                        e_table_reference = new XElement(defaultNamespace + "named_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        node.OuterClause.accept(this);
                        elementStack.Pop();
                        break;
                    }

                case ETableSource.table_ref_list:
                    {
                        e_table_reference = new XElement(defaultNamespace + "named_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        node.FromTableList.accept(this);
                        elementStack.Pop();
                        break;
                    }
                case ETableSource.hiveFromQuery:
                    {
                        e_table_reference = new XElement(defaultNamespace + "named_table_reference");
                        e_parent = (XElement)elementStack.Peek();
                        e_parent.Add(e_table_reference);
                        elementStack.Push(e_table_reference);
                        node.HiveFromQuery.accept(this);
                        elementStack.Pop();
                        break;
                    }
                default:
                    e_table_reference = new XElement(defaultNamespace + "named_table_reference");
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_table_reference);
                    elementStack.Push(e_table_reference);
                    sb.Append(node.ToString().Replace(">", "&#62;").Replace("<", "&#60;"));
                    elementStack.Pop();
                    break;

            }



            if (node.TableHintList != null)
            {
                appendStartTag("tablehints");
                for (int i = 0; i < node.TableHintList.Count; i++)
                {
                    TTableHint tableHint = node.TableHintList[i];
                    tableHint.accept(this);
                }
                appendEndTag("tablehints");
            }

            elementStack.Pop();
        }

        public override void postVisit(TTable node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TTableHint node)
        {
            appendStartTag(node);
            sb.Append(node.ToString());
        }
        public override void postVisit(TTableHint node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TObjectName node)
        {
            XElement e_object_name;
            string tag_name = TAG_OBJECTNAME;
            e_parent = (XElement)elementStack.Peek();
            if (!string.ReferenceEquals(current_objectName_tag, null))
            {
                tag_name = current_objectName_tag;
                current_objectName_tag = null;
            }
            e_object_name = new XElement(defaultNamespace + tag_name);
            e_parent.Add(e_object_name);

            XElement e_identifier = new XElement(defaultNamespace + TAG_FULLNAME);
            e_object_name.Add(e_identifier);
            e_object_name.Add(new XAttribute("object_type", node.DbObjectType.ToString()));
            e_identifier.Value = node.ToString();
            if (node.ServerToken != null)
            {
                XElement e_server = new XElement(defaultNamespace + "server_name");
                e_object_name.Add(e_server);
                e_server.Value = node.ServerToken.ToString();
            }
            if (node.DatabaseToken != null)
            {
                XElement e_database = new XElement(defaultNamespace + "database_name");
                e_object_name.Add(e_database);
                e_database.Value = node.DatabaseToken.ToString();
            }
            if (node.SchemaToken != null)
            {
                XElement e_schema = new XElement(defaultNamespace + "schema_name");
                e_object_name.Add(e_schema);
                e_schema.Value = node.SchemaToken.ToString();
            }
            if (node.ObjectToken != null)
            {
                XElement e_object = new XElement(defaultNamespace + "object_name");
                e_object_name.Add(e_object);
                e_object.Value = node.ObjectToken.ToString();
            }
            if (node.PartToken != null)
            {
                XElement e_part = new XElement(defaultNamespace + "part_name");
                e_object_name.Add(e_part);
                e_part.Value = node.PartToken.ToString();
            }
        }

        public override void postVisit(TObjectName node)
        {
            appendEndTag(node);
        }
        public override void preVisit(TObjectNameList node)
        {
            XElement e_objectName_list;
            e_parent = (XElement)elementStack.Peek();
            if (string.ReferenceEquals(current_objectName_list_tag, null))
            {
                e_objectName_list = new XElement(defaultNamespace + "objectName_list");
            }
            else
            {
                e_objectName_list = new XElement(defaultNamespace + current_objectName_list_tag);
                current_objectName_list_tag = null;
            }

            e_parent.Add(e_objectName_list);

            elementStack.Push(e_objectName_list);

            for (int i = 0; i < node.Count; i++)
            {
                node.getObjectName(i).accept(this);
            }
            elementStack.Pop();
        }


        public override void preVisit(TWhereClause node)
        {
            //appendStartTag(node);
            XElement e_where = new XElement(defaultNamespace + "where_clause");
            e_parent = (XElement)elementStack.Peek();
            //        if (current_objectName_list_tag == null){
            //            e_objectName_list = new XElement(defaultNamespace +  "objectName_list");
            //        }else{
            //            e_objectName_list = new XElement(defaultNamespace +  current_objectName_list_tag);
            //        }

            e_parent.Add(e_where);

            elementStack.Push(e_where);
            current_expression_tag = "condition";
            node.Condition.accept(this);
            current_expression_tag = null;
            elementStack.Pop();
        }
        public override void postVisit(TWhereClause node)
        {
            appendEndTag(node);
        }

        public override void preVisit(THierarchical node)
        {

            XElement e_hierarchical = new XElement(defaultNamespace + "hierarchial_clause");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_hierarchical);
            elementStack.Push(e_hierarchical);

            if (node.ConnectByList != null)
            {
                for (int i = 0; i < node.ConnectByList.Count; i++)
                {
                    node.ConnectByList[i].accept(this);
                }
            }

            if (node.StartWithClause != null)
            {
                XElement e_start_with = new XElement(defaultNamespace + "start_with_clause");
                e_hierarchical.Add(e_start_with);
                elementStack.Push(e_start_with);
                node.StartWithClause.accept(this);
                elementStack.Pop();
            }

            elementStack.Pop();

        }

        public override void preVisit(TConnectByClause node)
        {
            XElement e_connect_by = new XElement(defaultNamespace + "connect_by_clause");
            e_connect_by.Add(new XAttribute("nocycle", node.NoCycle.ToString()));
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_connect_by);
            elementStack.Push(e_connect_by);
            node.Condition.accept(this);
            elementStack.Pop();

        }

        public override void preVisit(TRollupCube node)
        {
            current_expression_list_tag = "rollup_list";
            if (node.Operation == TRollupCube.cube)
            {
                current_expression_list_tag = "cube_list";
            }
            node.Items.accept(this);

        }


        public override void preVisit(TGroupBy node)
        {
            appendStartTag(node);
            if (node.Items != null)
            {
                XElement e_group_by = new XElement(defaultNamespace + "group_by_clause");
                e_parent = (XElement)elementStack.Peek();
                e_parent.Add(e_group_by);
                elementStack.Push(e_group_by);
                node.Items.accept(this);
                elementStack.Pop();
            }
            if (node.HavingClause != null)
            {
                current_expression_tag = "having_clause";
                node.HavingClause.accept(this);
            }
        }

        public override void preVisit(TGroupByItem node)
        {
            XElement e_group_by = new XElement(defaultNamespace + "grouping_element");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_group_by);
            elementStack.Push(e_group_by);

            if (node.Expr != null)
            {
                TExpression ge = node.Expr;
                if ((ge.ExpressionType == EExpressionType.list_t) && (ge.ExprList == null))
                {
                    XElement e_grand_total = new XElement(defaultNamespace + "grand_total");
                    e_group_by.Add(e_grand_total);

                }
                else
                {
                    current_expression_tag = "grouping_expression";
                    ge.accept(this);
                }

                //current_expression_tag = "grouping_expression";
                //node.getExpr().accept(this);
            }
            else if (node.GroupingSet != null)
            {
                node.GroupingSet.accept(this);
            }
            else if (node.RollupCube != null)
            {
                node.RollupCube.accept(this);
            }

            elementStack.Pop();
        }

        //
        public override void preVisit(TGroupingSet node)
        {
            XElement e_grouping_sets = new XElement(defaultNamespace + "grouping_sets_specification");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_grouping_sets);
            elementStack.Push(e_grouping_sets);
            XElement e_grouping_set_item;
            for (int i = 0; i < node.Items.Count; i++)
            {
                e_grouping_set_item = new XElement(defaultNamespace + "grouping_set_item");
                e_parent = (XElement)elementStack.Peek();
                e_parent.Add(e_grouping_set_item);
                elementStack.Push(e_grouping_set_item);

                if (node.Items.getGroupingSetItem(i).Grouping_expression != null)
                {
                    TExpression ge = node.Items.getGroupingSetItem(i).Grouping_expression;
                    if ((ge.ExpressionType == EExpressionType.list_t) && (ge.ExprList == null))
                    {
                        XElement e_grand_total = new XElement(defaultNamespace + "grand_total");
                        e_grouping_set_item.Add(e_grand_total);

                    }
                    else
                    {
                        current_expression_tag = "grouping_expression";
                        ge.accept(this);
                    }
                }
                else if (node.Items.getGroupingSetItem(i).RollupCubeClause != null)
                {
                    TRollupCube rollupCube = node.Items.getGroupingSetItem(i).RollupCubeClause;
                    rollupCube.accept(this);
                }
                elementStack.Pop();
            }
            elementStack.Pop();
        }
        public override void postVisit(TGroupingSet node)
        {
        }

        public override void preVisit(TGroupByItemList node)
        {
            // appendStartTag(node);
            for (int i = 0; i < node.Count; i++)
            {
                node.getGroupByItem(i).accept(this);
            }
        }
        public override void postVisit(TGroupByItemList node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TOrderBy node)
        {
            XElement e_order_by = new XElement(defaultNamespace + "order_by_clause");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_order_by);
            elementStack.Push(e_order_by);
            node.Items.accept(this);
            elementStack.Pop();

        }

        public override void preVisit(TOrderByItem node)
        {
            XElement e_order_by_item = new XElement(defaultNamespace + "order_by_item");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_order_by_item);
            elementStack.Push(e_order_by_item);
            if (node.SortKey != null)
            {
                current_expression_tag = "sort_key";
                node.SortKey.accept(this);
            }
            e_order_by_item.Add(new XAttribute("sort_order", node.SortOrder.ToString()));
            //            if (node.getSortOrder() != ESortType.none){
            //                XElement e_sort_order = new XElement(defaultNamespace +  "sort_order");
            //                e_sort_order.setTextContent(node.getSortOrder().ToString());
            //                e_order_by_item.Add(e_sort_order);
            //            }
            elementStack.Pop();
        }



        public override void preVisit(TOrderByItemList node)
        {
            appendStartTag(node);
            for (int i = 0; i < node.Count; i++)
            {
                node.getOrderByItem(i).accept(this);
            }
        }
        public override void postVisit(TOrderByItemList node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TForUpdate node)
        {
        }


        public override void preVisit(TStatementList node)
        {
            //appendStartTag(node);

            XElement e_statement_list;
            e_parent = (XElement)elementStack.Peek();
            if (string.ReferenceEquals(current_statement_list_tag, null))
            {
                e_statement_list = new XElement(defaultNamespace + TAG_STATEMENT_LIST);
            }
            else
            {
                e_statement_list = new XElement(defaultNamespace + current_statement_list_tag);
                current_statement_list_tag = null;
            }
            e_statement_list.Add(new XAttribute("count", node.Count.ToString()));
            e_parent.Add(e_statement_list);

            //elementStack.Push(e_statement_list);
            for (int i = 0; i < node.Count; i++)
            {
                XElement e_statement = new XElement(defaultNamespace + "statement");
                e_statement.Add(new XAttribute("type", node.get(i).sqlstatementtype.ToString()));
                e_statement_list.Add(e_statement);
                elementStack.Push(e_statement);
                node.get(i).DummyTag = TOP_STATEMENT;
                node.get(i).accept(this);
                elementStack.Pop();
            }

        }

        internal virtual void doDeclare_Body_Exception(TCommonStoredProcedureSqlStatement node)
        {

            if (node.DeclareStatements != null)
            {
                appendStartTag("declare");
                node.DeclareStatements.accept(this);
                appendEndTag("declare");
            }

            if (node.BodyStatements != null)
            {
                appendStartTag("body");
                node.BodyStatements.accept(this);
                appendEndTag("body");
            }

            if (node.ExceptionClause != null)
            {
                node.ExceptionClause.accept(this);
            }

        }

        public override void preVisit(TPlsqlCreatePackage node)
        {
            XElement e_create_package = null;
            switch (node.Kind)
            {
                case TBaseType.kind_define:
                case TBaseType.kind_create:
                    e_create_package = new XElement(defaultNamespace + "create_package_statement");
                    break;
                case TBaseType.kind_create_body:
                    e_create_package = new XElement(defaultNamespace + "create_package_body_statement");
                    break;
            }

            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_create_package);
            elementStack.Push(e_create_package);
            current_objectName_tag = "package_name";
            node.PackageName.accept(this);
            if (node.EndLabelName != null)
            {
                current_objectName_tag = "end_package_name";
                node.EndLabelName.accept(this);
            }
            if (node.DeclareStatements.Count > 0)
            {
                current_statement_list_tag = "declare_section";
                node.DeclareStatements.accept(this);
            }

            elementStack.Pop();

            //            if (node.getParameterDeclarations() != null) node.getParameterDeclarations().accept(this);
            //            if ( node.getBodyStatements().Count > 0) node.getBodyStatements().accept(this);
            //            if (node.getExceptionClause() != null) node.getExceptionClause().accept(this);

        }


        public override void preVisit(TMssqlCreateFunction node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_function = new XElement(defaultNamespace + "create_function_statement");
            e_parent.Add(e_function);
            elementStack.Push(e_function);
            // doFunctionSpecification(node);

            e_parent = (XElement)elementStack.Peek();
            XElement e_function_spec = new XElement(defaultNamespace + "function_specification_statement");
            e_parent.Add(e_function_spec);
            elementStack.Push(e_function_spec);
            current_objectName_tag = "function_name";
            node.FunctionName.accept(this);
            if (node.EndLabelName != null)
            {
                current_objectName_tag = "end_function_name";
                node.EndLabelName.accept(this);
            }

            if (node.ReturnDataType != null)
            {
                current_datatype_tag = "return_datatype";
                node.ReturnDataType.accept(this);
            }

            if (node.ParameterDeclarations != null)
            {
                node.ParameterDeclarations.accept(this);
            }

            if (node.BodyStatements.Count > 0)
            {
                current_statement_list_tag = "body_statement_list";
                node.BodyStatements.accept(this);
            }

            elementStack.Pop();

            elementStack.Pop();

        }


        public override void preVisit(TCreateDatabaseSqlStatement stmt)
        {
            XElement e_use_database = new XElement(defaultNamespace + "create_database_statement");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_use_database);
            elementStack.Push(e_use_database);
            current_objectName_tag = "database_name";
            stmt.DatabaseName.accept(this);
            elementStack.Pop();
        }

        public override void preVisit(TCreateSchemaSqlStatement stmt)
        {
            XElement e_create_schema = new XElement(defaultNamespace + "create_schema_statement");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_create_schema);
            elementStack.Push(e_create_schema);
            current_objectName_tag = "schema_name";
            stmt.SchemaName.accept(this);
            elementStack.Pop();
        }

        public override void preVisit(TUseDatabase stmt)
        {
            XElement e_use_database = new XElement(defaultNamespace + "use_database_statement");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_use_database);
            elementStack.Push(e_use_database);
            current_objectName_tag = "database_name";
            stmt.DatabaseName.accept(this);
            elementStack.Pop();
        }

        public override void preVisit(TMssqlBlock node)
        {
            XElement e_block = new XElement(defaultNamespace + "block_statement");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_block);
            elementStack.Push(e_block);

            if (node.BodyStatements.Count > 0)
            {
                current_statement_list_tag = "body_statement_list";
                node.BodyStatements.accept(this);
            }

            elementStack.Pop();

        }

        private void doProcedureSpecification(TPlsqlCreateProcedure node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_procedure_spec = new XElement(defaultNamespace + "procedure_specification_statement");
            e_parent.Add(e_procedure_spec);
            elementStack.Push(e_procedure_spec);
            current_objectName_tag = "procedure_name";
            node.ProcedureName.accept(this);

            if (node.EndLabelName != null)
            {
                current_objectName_tag = "end_procedure_name";
                node.EndLabelName.accept(this);
            }

            if (node.ParameterDeclarations != null)
            {
                node.ParameterDeclarations.accept(this);
            }
            if (node.InnerStatements.Count > 0)
            {
                node.InnerStatements.accept(this);
            }
            if (node.DeclareStatements.Count > 0)
            {
                current_statement_list_tag = "declaration_section";
                node.DeclareStatements.accept(this);
            }

            if (node.BodyStatements.Count > 0)
            {
                current_statement_list_tag = "body_statement_list";
                node.BodyStatements.accept(this);
            }

            if (node.ExceptionClause != null)
            {
                node.ExceptionClause.accept(this);
            }

            elementStack.Pop();

        }

        public override void preVisit(TPlsqlCreateProcedure node)
        {
            XElement e_create_procedure = null;
            e_parent = (XElement)elementStack.Peek();

            switch (node.Kind)
            {
                case TBaseType.kind_define:
                    doProcedureSpecification(node);
                    break;
                case TBaseType.kind_declare:
                    XElement e_procedure_declare = new XElement(defaultNamespace + "procedure_declare_statement");
                    e_parent.Add(e_procedure_declare);
                    elementStack.Push(e_procedure_declare);
                    current_objectName_tag = "procedure_name";
                    node.ProcedureName.accept(this);
                    if (node.ParameterDeclarations != null)
                    {
                        node.ParameterDeclarations.accept(this);
                    }
                    elementStack.Pop();

                    break;
                case TBaseType.kind_create:
                    e_create_procedure = new XElement(defaultNamespace + "create_procedure_statement");
                    e_parent.Add(e_create_procedure);
                    elementStack.Push(e_create_procedure);
                    doProcedureSpecification(node);
                    elementStack.Pop();
                    break;
            }
        }

        private void doFunctionSpecification(TPlsqlCreateFunction node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_function_spec = new XElement(defaultNamespace + "function_specification_statement");
            e_parent.Add(e_function_spec);
            elementStack.Push(e_function_spec);
            current_objectName_tag = "function_name";
            node.FunctionName.accept(this);
            if (node.EndLabelName != null)
            {
                current_objectName_tag = "end_function_name";
                node.EndLabelName.accept(this);
            }
            current_datatype_tag = "return_datatype";
            node.ReturnDataType.accept(this);

            if (node.ParameterDeclarations != null)
            {
                node.ParameterDeclarations.accept(this);
            }
            if (node.DeclareStatements.Count > 0)
            {
                current_statement_list_tag = "declaration_section";
                node.DeclareStatements.accept(this);
            }
            if (node.BodyStatements.Count > 0)
            {
                current_statement_list_tag = "body_statement_list";
                node.BodyStatements.accept(this);
            }
            if (node.ExceptionClause != null)
            {
                node.ExceptionClause.accept(this);
            }
            elementStack.Pop();
        }

        public override void preVisit(TPlsqlCreateFunction node)
        {
            XElement e_function = null;
            e_parent = (XElement)elementStack.Peek();
            switch (node.Kind)
            {
                case TBaseType.kind_create:
                    e_function = new XElement(defaultNamespace + "create_function_statement");
                    e_parent.Add(e_function);
                    elementStack.Push(e_function);
                    doFunctionSpecification(node);
                    elementStack.Pop();
                    break;
                case TBaseType.kind_declare:
                    XElement e_function_declare = new XElement(defaultNamespace + "function_declare_statement");
                    e_parent.Add(e_function_declare);
                    elementStack.Push(e_function_declare);
                    current_objectName_tag = "function_name";
                    node.FunctionName.accept(this);
                    if (node.ParameterDeclarations != null)
                    {
                        node.ParameterDeclarations.accept(this);
                    }
                    elementStack.Pop();
                    break;
                case TBaseType.kind_define:
                    doFunctionSpecification(node);
                    break;
            }
        }


        public override void preVisit(TCommonBlock node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_block_stmt = new XElement(defaultNamespace + "plsql_block_statement");
            e_parent.Add(e_block_stmt);
            elementStack.Push(e_block_stmt);
            if (node.LabelName != null)
            {
                current_objectName_tag = "label_name";
                node.LabelName.accept(this);
            }
            // doDeclare_Body_Exception(node);
            current_statement_list_tag = "declaration_section";
            if (node.DeclareStatements.Count > 0)
            {
                node.DeclareStatements.accept(this);
            }
            current_statement_list_tag = "body_statement_list";
            if (node.BodyStatements.Count > 0)
            {
                node.BodyStatements.accept(this);
            }

            if (node.ExceptionClause != null)
            {
                node.ExceptionClause.accept(this);
            }
            elementStack.Pop();

        }


        public override void preVisit(TExceptionClause node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_exception_clause = new XElement(defaultNamespace + "exception_clause");
            e_parent.Add(e_exception_clause);
            elementStack.Push(e_exception_clause);
            node.Handlers.accept(this);
            elementStack.Pop();
        }

        public override void preVisit(TExceptionHandler node)
        {
            XElement e_exception_handler;
            e_parent = (XElement)elementStack.Peek();
            e_exception_handler = new XElement(defaultNamespace + "exception_handler");
            e_parent.Add(e_exception_handler);
            elementStack.Push(e_exception_handler);
            current_objectName_list_tag = "exception_name_list";
            node.ExceptionNames.accept(this);
            node.Statements.accept(this);
            elementStack.Pop();
        }

        public override void preVisit(TExceptionHandlerList node)
        {
            for (int i = 0; i < node.Count; i++)
            {
                node.getExceptionHandler(i).accept(this);
            }

        }

        public override void preVisit(TAlterTableOption node)
        {
            //appendStartTag(node);
            XElement e_alter_table_option = new XElement(defaultNamespace + "alter_table_option");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_alter_table_option);
            elementStack.Push(e_alter_table_option);
            e_alter_table_option.Add(new XAttribute("alter_type", node.OptionType.ToString()));

            //appendStartTagWithIntProperty(node, "alterOption", node.getOptionType().ToString());
            XElement e_option = null;
            switch (node.OptionType)
            {
                case EAlterTableOptionType.AddColumn:
                    e_option = new XElement(defaultNamespace + "add_column_option");
                    e_alter_table_option.Add(e_option);
                    elementStack.Push(e_option);
                    node.ColumnDefinitionList.accept(this);
                    elementStack.Pop();
                    break;
                case EAlterTableOptionType.AlterColumn:
                    e_option = new XElement(defaultNamespace + "alter_column_option");
                    e_alter_table_option.Add(e_option);
                    elementStack.Push(e_option);
                    node.ColumnName.accept(this);
                    elementStack.Pop();
                    break;
                case EAlterTableOptionType.ChangeColumn:
                    e_option = new XElement(defaultNamespace + "change_column_option");
                    e_alter_table_option.Add(e_option);
                    elementStack.Push(e_option);
                    node.ColumnName.accept(this);
                    elementStack.Pop();
                    break;
                case EAlterTableOptionType.DropColumn:
                    e_option = new XElement(defaultNamespace + "drop_column_option");
                    e_alter_table_option.Add(e_option);
                    elementStack.Push(e_option);
                    for (int i = 0; i < node.ColumnNameList.Count; i++)
                    {
                        current_objectName_tag = "column_name";
                        node.ColumnNameList.getObjectName(i).accept(this);
                    }
                    elementStack.Pop();

                    break;
                case EAlterTableOptionType.ModifyColumn:
                    e_option = new XElement(defaultNamespace + "modify_column_option");
                    e_alter_table_option.Add(e_option);
                    elementStack.Push(e_option);
                    node.ColumnDefinitionList.accept(this);
                    elementStack.Pop();
                    break;
                case EAlterTableOptionType.RenameColumn:
                    e_option = new XElement(defaultNamespace + "rename_column_option");
                    e_alter_table_option.Add(e_option);
                    elementStack.Push(e_option);
                    current_objectName_tag = "column_name";
                    node.ColumnName.accept(this);
                    current_objectName_tag = "new_column_name";
                    node.NewColumnName.accept(this);
                    elementStack.Pop();
                    break;
                case EAlterTableOptionType.AddConstraint:
                    e_option = new XElement(defaultNamespace + "add_constraint_option");
                    e_alter_table_option.Add(e_option);
                    elementStack.Push(e_option);
                    node.ConstraintList.accept(this);
                    elementStack.Pop();
                    break;
                case EAlterTableOptionType.switchPartition:
                    e_option = new XElement(defaultNamespace + "switch_partition_option");
                    e_alter_table_option.Add(e_option);
                    elementStack.Push(e_option);
                    current_objectName_tag = "new_table_name";
                    node.NewTableName.accept(this);
                    if (node.PartitionExpression1 != null)
                    {
                        current_expression_tag = "source_partition_number";
                        node.PartitionExpression1.accept(this);
                    }
                    if (node.PartitionExpression2 != null)
                    {
                        current_expression_tag = "target_partition_number";
                        node.PartitionExpression2.accept(this);
                    }
                    elementStack.Pop();
                    break;
                default:
                    e_option = new XElement(defaultNamespace + "not_implemented_option");
                    e_alter_table_option.Add(e_option);
                    elementStack.Push(e_option);
                    e_option.Value = node.ToString();
                    elementStack.Pop();
                    break;
            }

            elementStack.Pop();
        }

        public override void preVisit(TAlterTableStatement stmt)
        {

            XElement e_alter_table = new XElement(defaultNamespace + "alter_table_statement");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_alter_table);
            elementStack.Push(e_alter_table);

            current_objectName_tag = "table_name";
            stmt.TableName.accept(this);


            if (stmt.AlterTableOptionList != null)
            {
                XElement e_alter_table_option_list = new XElement(defaultNamespace + "alter_table_option_list");
                e_alter_table_option_list.Add(new XAttribute("count", (stmt.AlterTableOptionList.Count).ToString()));
                e_alter_table.Add(e_alter_table_option_list);
                elementStack.Push(e_alter_table_option_list);
                for (int i = 0; i < stmt.AlterTableOptionList.Count; i++)
                {
                    stmt.AlterTableOptionList.getAlterTableOption(i).accept(this);
                }
                elementStack.Pop();
            }
            //
            //            if (stmt.getMySQLTableOptionList() != null){
            //                stmt.getMySQLTableOptionList().accept(this);
            //            }

            elementStack.Pop();

        }



        public override void preVisit(TTypeName node)
        {
            string tag_name = "datatype";
            e_parent = (XElement)elementStack.Peek();
            if (!string.ReferenceEquals(current_datatype_tag, null))
            {
                tag_name = current_datatype_tag;
                current_datatype_tag = null;
            }
            XElement e_datatype = new XElement(defaultNamespace + tag_name);
            e_datatype.Add(new XAttribute("type", node.DataType.ToString()));
            e_parent.Add(e_datatype);
            XElement e_value = new XElement(defaultNamespace + "value");
            e_value.Value = node.ToString();
            e_datatype.Add(e_value);

        }


        public override void preVisit(TColumnDefinition node)
        {

            XElement e_column = new XElement(defaultNamespace + "column_definition");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_column);
            elementStack.Push(e_column);
            e_column.Add(new XAttribute("nullable", node.Null.ToString()));
            current_objectName_tag = "column_name";
            node.ColumnName.accept(this);

            if (node.Datatype != null)
            {
                node.Datatype.accept(this);
            }

            if ((node.Constraints != null) && (node.Constraints.Count > 0))
            {
                XElement e_constraint_list = new XElement(defaultNamespace + "column_constraint_list");
                e_column.Add(e_constraint_list);
                elementStack.Push(e_constraint_list);
                node.Constraints.accept(this);
                elementStack.Pop();
            }


            elementStack.Pop();
        }


        public override void preVisit(TColumnDefinitionList node)
        {
            appendStartTag(node);
            for (int i = 0; i < node.Count; i++)
            {
                node.getColumn(i).accept(this);
            }
        }

        public override void preVisit(TMergeWhenClause node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_merge_action = new XElement(defaultNamespace + "merge_action");
            e_parent.Add(e_merge_action);
            elementStack.Push(e_merge_action);


            if (node.Condition != null)
            {
                current_expression_tag = "search_condition";
                node.Condition.accept(this);
            }

            if (node.UpdateClause != null)
            {
                node.UpdateClause.accept(this);
            }

            if (node.InsertClause != null)
            {
                node.InsertClause.accept(this);
            }

            if (node.DeleteClause != null)
            {
                node.DeleteClause.accept(this);
            }

            elementStack.Pop();

        }

        public override void preVisit(TMergeUpdateClause node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_merge_update_action = new XElement(defaultNamespace + "merge_update_action");
            e_parent.Add(e_merge_update_action);
            elementStack.Push(e_merge_update_action);

            if (node.UpdateColumnList != null)
            {
                // node.getUpdateColumnList().accept(this);
                for (int i = 0; i < node.UpdateColumnList.Count; i++)
                {
                    current_expression_tag = "assignment_set_clause";
                    node.UpdateColumnList.getResultColumn(i).Expr.accept(this);
                }
            }

            if (node.UpdateWhereClause != null)
            {
                node.UpdateWhereClause.accept(this);
            }

            if (node.DeleteWhereClause != null)
            {
                node.DeleteWhereClause.accept(this);
            }
            elementStack.Pop();

        }


        public override void preVisit(TMergeInsertClause node)
        {
            //appendStartTag(node);
            e_parent = (XElement)elementStack.Peek();
            XElement e_merge_insert_action = new XElement(defaultNamespace + "merge_insert_action");
            e_parent.Add(e_merge_insert_action);
            elementStack.Push(e_merge_insert_action);

            if (node.ColumnList != null)
            {
                current_objectName_list_tag = "column_list_reference";
                node.ColumnList.accept(this);
            }

            if (node.Valuelist != null)
            {
                e_parent = (XElement)elementStack.Peek();
                XElement e_row_values = new XElement(defaultNamespace + "row_values");
                e_parent.Add(e_row_values);
                elementStack.Push(e_row_values);
                for (int i = 0; i < node.Valuelist.Count; i++)
                {
                    node.Valuelist.getResultColumn(i).Expr.accept(this);
                }
                //node.getValuelist().accept(this);
                elementStack.Pop();
            }

            if (node.InsertWhereClause != null)
            {
                node.InsertWhereClause.accept(this);
            }

            elementStack.Pop();

        }


        public override void preVisit(TMergeDeleteClause node)
        {
            //appendStartTag(node);
            e_parent = (XElement)elementStack.Peek();
            XElement e_merge_delete_action = new XElement(defaultNamespace + "merge_delete_action");
            e_parent.Add(e_merge_delete_action);
            elementStack.Push(e_merge_delete_action);


            elementStack.Pop();

        }


        public override void preVisit(TConstraint node)
        {

            XElement e_constraint = new XElement(defaultNamespace + "constraint");
            if (node.ConstraintName != null)
            {
                e_constraint.Add(new XAttribute("name", node.ConstraintName.ToString()));
            }

            e_constraint.Add(new XAttribute("type", node.Constraint_type.ToString()));
            e_constraint.Add(new XAttribute("clustered", node.Clustered.ToString()));
            e_constraint.Add(new XAttribute("nonclustered", node.NonClustered.ToString()));

            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_constraint);
            elementStack.Push(e_constraint);


            // appendStartTagWithIntProperty(node,"type",node.getConstraint_type().ToString(),"name",(node.getConstraintName() != null) ? node.getConstraintName().ToString():"");
            switch (node.Constraint_type)
            {
                case EConstraintType.notnull:
                    break;
                case EConstraintType.unique:
                    e_constraint.Add(new XAttribute("xsi:type", "unqiue_constriant_type"));
                    e_constraint.Add(new XAttribute("is_primary_key", "false"));
                    if (node.ColumnList != null)
                    {
                        for (int i = 0; i < node.ColumnList.Count; i++)
                        {
                            XElement e_column = new XElement(defaultNamespace + "column");
                            e_constraint.Add(e_column);
                            e_column.Add(new XAttribute("name", node.ColumnList.getObjectName(i).ToString()));
                        }
                    }

                    break;
                case EConstraintType.check:
                    e_constraint.Add(new XAttribute("xsi:type", "check_constriant_type"));
                    current_expression_tag = "check_condition";
                    if (node.CheckCondition != null)
                    {
                        node.CheckCondition.accept(this);
                    }
                    else
                    {
                        //db2 functional dependency
                    }

                    break;
                case EConstraintType.primary_key:
                    e_constraint.Add(new XAttribute("xsi:type", "unqiue_constriant_type"));
                    e_constraint.Add(new XAttribute("is_primary_key", "true"));
                    if (node.ColumnList != null)
                    {
                        for (int i = 0; i < node.ColumnList.Count; i++)
                        {
                            XElement e_column = new XElement(defaultNamespace + "column");
                            e_constraint.Add(e_column);
                            e_column.Add(new XAttribute("name", node.ColumnList.getObjectName(i).ToString()));
                        }
                    }
                    else if (node.IndexCols != null)
                    {
                        foreach (TIndexColName indexCol in node.IndexCols)
                        {
                            XElement e_column = new XElement(defaultNamespace + "column");
                            e_constraint.Add(e_column);
                            e_column.Add(new XAttribute("name", indexCol.ColumnName.ToString()));
                        }
                    }
                    break;
                case EConstraintType.foreign_key:
                case EConstraintType.reference:
                    e_constraint.Add(new XAttribute("xsi:type", "foreign_key_constriant_type"));
                    if (node.ColumnList != null)
                    {
                        for (int i = 0; i < node.ColumnList.Count; i++)
                        {
                            XElement e_column = new XElement(defaultNamespace + "column");
                            e_constraint.Add(e_column);
                            e_column.Add(new XAttribute("name", node.ColumnList.getObjectName(i).ToString()));
                        }
                    }
                    if (node.ReferencedObject != null)
                    {
                        //                        XElement e_referenced_table = new XElement(defaultNamespace +  "referenced_table");
                        //                        e_constraint.Add(e_referenced_table);
                        //                        elementStack.Push(e_referenced_table);
                        current_objectName_tag = "referenced_table";
                        node.ReferencedObject.accept(this);
                        //                        elementStack.Pop();
                    }
                    if (node.ReferencedColumnList != null)
                    {
                        for (int i = 0; i < node.ReferencedColumnList.Count; i++)
                        {
                            XElement e_column = new XElement(defaultNamespace + "referenced_column");
                            e_constraint.Add(e_column);
                            e_column.Add(new XAttribute("name", node.ReferencedColumnList.getObjectName(i).ToString()));
                        }
                    }
                    break;
                case EConstraintType.default_value:
                    e_constraint.Add(new XAttribute("xsi:type", "default_constriant_type"));
                    current_expression_tag = "default_value";
                    node.DefaultExpression.accept(this);
                    break;
                default:
                    break;
            }

            elementStack.Pop();
        }

        public override void preVisit(TConstraintList node)
        {
            //appendStartTag(node);
            for (int i = 0; i < node.Count; i++)
            {
                node.getConstraint(i).accept(this);
            }

        }

        public override void preVisit(TCreateViewSqlStatement stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_create_view = new XElement(defaultNamespace + "create_view_statement");
            e_parent.Add(e_create_view);
            elementStack.Push(e_create_view);
            current_objectName_tag = "view_name";
            stmt.ViewName.accept(this);

            if (stmt.ViewAliasClause != null)
            {
                XElement e_column_list = new XElement(defaultNamespace + "column_list");
                e_create_view.Add(e_column_list);
                elementStack.Push(e_column_list);
                for (int i = 0; i < stmt.ViewAliasClause.ViewAliasItemList.Count; i++)
                {
                    TViewAliasItem viewAliasItem = stmt.ViewAliasClause.ViewAliasItemList.getViewAliasItem(i);
                    if (viewAliasItem.Alias == null)
                    {
                        continue;
                    }
                    viewAliasItem.Alias.accept(this);
                }
                elementStack.Pop();
            }
            stmt.Subquery.DummyTag = TOP_STATEMENT;
            stmt.Subquery.accept(this);
            elementStack.Pop();
        }

        public override void postVisit(TCreateViewSqlStatement stmt)
        {

        }

        public override void preVisit(TMssqlCreateTrigger stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_create_trigger = new XElement(defaultNamespace + "create_trigger_statement");
            e_parent.Add(e_create_trigger);
            elementStack.Push(e_create_trigger);
            current_objectName_tag = "trigger_name";
            stmt.TriggerName.accept(this);
            current_table_reference_tag = "onTable";
            stmt.OnTable.accept(this);
            XElement e_timing_point = new XElement(defaultNamespace + "timing_point");
            e_timing_point.Value = stmt.TimingPoint.ToString();
            e_create_trigger.Add(e_timing_point);

            foreach (int typeCode in Enum.GetValues(typeof(ETriggerDmlType)))
            {
                XElement e_dmltype = new XElement(defaultNamespace + "dml_type");
                e_dmltype.Value = Enum.GetName(typeof(ETriggerDmlType), typeCode);
                e_create_trigger.Add(e_dmltype);
            }

            //e_create_trigger.Add(new XAttribute("dmlType",stmt.getDmlTypes().);

            current_statement_list_tag = "body_statement_list";
            if (stmt.BodyStatements.Count > 0)
            {
                stmt.BodyStatements.accept(this);
            }

            elementStack.Pop();
        }

        public override void preVisit(TCreateSequenceStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_create_sequence = new XElement(defaultNamespace + "create_sequence_statement");
            e_parent.Add(e_create_sequence);
            elementStack.Push(e_create_sequence);
            current_objectName_tag = "sequence_name";
            stmt.SequenceName.accept(this);
            for (int i = 0; i < stmt.Options.Count; i++)
            {
                TSequenceOption sequenceOption = stmt.Options[i];
                switch (sequenceOption.SequenceOptionType)
                {
                    case ESequenceOptionType.start:
                    case ESequenceOptionType.startWith:
                        e_create_sequence.Add(new XAttribute("start_with", sequenceOption.OptionValue.ToString()));
                        break;
                    case ESequenceOptionType.restart:
                    case ESequenceOptionType.restartWith:
                        e_create_sequence.Add(new XAttribute("restart_with", sequenceOption.OptionValue.ToString()));
                        break;
                    case ESequenceOptionType.increment:
                    case ESequenceOptionType.incrementBy:
                        e_create_sequence.Add(new XAttribute("increment_by", sequenceOption.OptionValue.ToString()));
                        break;
                    case ESequenceOptionType.minValue:
                        e_create_sequence.Add(new XAttribute("min_value", sequenceOption.OptionValue.ToString()));
                        break;
                    case ESequenceOptionType.maxValue:
                        e_create_sequence.Add(new XAttribute("max_value", sequenceOption.OptionValue.ToString()));
                        break;
                    case ESequenceOptionType.cycle:
                        e_create_sequence.Add(new XAttribute("cycle", "true"));
                        break;
                    case ESequenceOptionType.noCycle:
                        e_create_sequence.Add(new XAttribute("nocycle", "true"));
                        break;
                    case ESequenceOptionType.cache:
                        e_create_sequence.Add(new XAttribute("cache", sequenceOption.OptionValue.ToString()));
                        break;
                    case ESequenceOptionType.noCache:
                        e_create_sequence.Add(new XAttribute("nocache", "true"));
                        break;
                    case ESequenceOptionType.order:
                        e_create_sequence.Add(new XAttribute("order", "true"));
                        break;
                    case ESequenceOptionType.noOrder:
                        e_create_sequence.Add(new XAttribute("noorder", "true"));
                        break;
                    default:
                        break;
                }

            }
            elementStack.Pop();
        }

        public override void preVisit(TCreateSynonymStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_create_synonym = new XElement(defaultNamespace + "create_synonym_statement");
            e_parent.Add(e_create_synonym);
            elementStack.Push(e_create_synonym);
            current_objectName_tag = "synonym_name";
            stmt.SynonymName.accept(this);
            current_objectName_tag = "for_name";
            stmt.ForName.accept(this);
            elementStack.Pop();
        }

        public override void preVisit(TExecParameter node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_exec_parameter = new XElement(defaultNamespace + "exec_parameter");
            e_parent.Add(e_exec_parameter);
            elementStack.Push(e_exec_parameter);
            if (node.ParameterName != null)
            {
                current_objectName_tag = "parameter_name";
                node.ParameterName.accept(this);
            }
            current_expression_tag = "parameter_value";
            node.ParameterValue.accept(this);
            elementStack.Pop();
        }

        public override void preVisit(TMssqlExecute stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_execute = new XElement(defaultNamespace + "execute_statement");
            e_parent.Add(e_execute);
            elementStack.Push(e_execute);

            switch (stmt.ExecType)
            {
                case TBaseType.metExecSp:
                    current_objectName_tag = "module_name";
                    stmt.ModuleName.accept(this);
                    if (stmt.Parameters != null)
                    {
                        for (int i = 0; i < stmt.Parameters.Count; i++)
                        {
                            stmt.Parameters.getExecParameter(i).accept(this);
                        }
                    }
                    break;
                default:
                    break;
            }

            elementStack.Pop();
        }

        public override void preVisit(TMssqlDeclare stmt)
        {

            e_parent = (XElement)elementStack.Peek();
            XElement e_declare_varaible = new XElement(defaultNamespace + "declare_variable_statement");
            e_parent.Add(e_declare_varaible);
            elementStack.Push(e_declare_varaible);
            switch (stmt.DeclareType)
            {
                case EDeclareType.variable:
                    if (stmt.DeclareType == EDeclareType.variable)
                    {
                        stmt.Variables.accept(this);
                    }
                    break;
                default:
                    //if (stmt.getSubquery() != null) stmt.getSubquery().accept(this);
                    break;
            }
            elementStack.Pop();

        }




        public override void preVisit(TMssqlSet stmt)
        {
            XElement e_set_command;
            e_parent = (XElement)elementStack.Peek();
            switch (stmt.SetType)
            {
                case TBaseType.mstUnknown:
                    e_set_command = new XElement(defaultNamespace + "mssql_set_command");
                    e_parent.Add(e_set_command);
                    elementStack.Push(e_set_command);
                    e_set_command.Value = stmt.ToString();
                    elementStack.Pop();
                    break;
                case TBaseType.mstLocalVar:
                    XElement e_set_variable = new XElement(defaultNamespace + "mssql_set_variable_statement");
                    e_parent.Add(e_set_variable);
                    elementStack.Push(e_set_variable);
                    current_objectName_tag = "variable_name";
                    stmt.VarName.accept(this);
                    current_expression_tag = "variable_value";
                    stmt.VarExpr.accept(this);
                    elementStack.Pop();
                    break;
                case TBaseType.mstLocalVarCursor:
                    e_set_command = new XElement(defaultNamespace + "mssql_set_command");
                    e_parent.Add(e_set_command);
                    elementStack.Push(e_set_command);
                    e_set_command.Value = stmt.ToString();
                    elementStack.Pop();
                    break;
                case TBaseType.mstSetCmd:
                    e_set_command = new XElement(defaultNamespace + "mssql_set_command");
                    e_parent.Add(e_set_command);
                    elementStack.Push(e_set_command);
                    e_set_command.Value = stmt.ToString();
                    elementStack.Pop();
                    break;
                case TBaseType.mstXmlMethod:
                    e_set_command = new XElement(defaultNamespace + "mssql_set_command");
                    e_parent.Add(e_set_command);
                    elementStack.Push(e_set_command);
                    e_set_command.Value = stmt.ToString();
                    elementStack.Pop();
                    break;
                case TBaseType.mstSybaseLocalVar:
                    e_set_command = new XElement(defaultNamespace + "mssql_set_command");
                    e_parent.Add(e_set_command);
                    elementStack.Push(e_set_command);
                    e_set_command.Value = stmt.ToString();
                    elementStack.Pop();
                    break;
                default:
                    e_set_command = new XElement(defaultNamespace + "mssql_set_command");
                    e_parent.Add(e_set_command);
                    elementStack.Push(e_set_command);
                    e_set_command.Value = stmt.ToString();
                    elementStack.Pop();
                    break;

            }


            //            appendStartTagWithIntProperty(stmt,"type",stmt.getSetType());
            //            if (stmt.getSetType() == TBaseType.mstLocalVar){
            //
            //                appendStartTagWithIntProperty(stmt,
            //                        "variableName",
            //                        stmt.getVarName().ToString(),
            //                        "value",
            //                        stmt.getVarExpr().ToString());
            //
            //            }
        }


        public override void preVisit(TMergeSqlStatement stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_merge = new XElement(defaultNamespace + "merge_statement");
            e_parent.Add(e_merge);
            elementStack.Push(e_merge);

            if (stmt.CteList != null)
            {
                stmt.CteList.accept(this);
            }

            current_table_reference_tag = "target_table";
            stmt.TargetTable.accept(this);

            current_table_reference_tag = "source_table";
            stmt.UsingTable.accept(this);

            current_expression_tag = "search_condition";
            stmt.Condition.accept(this);

            //            if (stmt.getColumnList() != null) stmt.getColumnList().accept(this);
            if (stmt.WhenClauses != null)
            {
                for (int i = 0; i < stmt.WhenClauses.Count; i++)
                {
                    TMergeWhenClause whenClause = stmt.WhenClauses[i];
                    whenClause.accept(this);
                }
                //stmt.getWhenClauses().accept(this);
            }
            //            if (stmt.getOutputClause() != null) stmt.getOutputClause().accept(this);
            //            if (stmt.getErrorLoggingClause() != null) stmt.getErrorLoggingClause().accept(this);
            elementStack.Pop();
        }


        public override void preVisit(TCreateIndexSqlStatement stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_create_index = new XElement(defaultNamespace + "create_index_statement");
            e_parent.Add(e_create_index);
            elementStack.Push(e_create_index);
            //e_create_index.Add(new XAttribute("clustered",stmt.get);
            if (stmt.IndexName != null)
            {
                current_objectName_tag = "index_name";
                stmt.IndexName.accept(this);
            }
            else
            {
                // teradata allow empty index name
            }
            current_objectName_tag = "on_name";
            stmt.TableName.accept(this);

            XElement e_column_list = new XElement(defaultNamespace + "column_with_sort_list");
            e_create_index.Add(e_column_list);
            for (int i = 0; i < stmt.ColumnNameList.Count; i++)
            {
                TOrderByItem orderByItem = stmt.ColumnNameList.getOrderByItem(i);
                XElement e_column = new XElement(defaultNamespace + "column_with_sort");
                e_column.Add(new XAttribute("sort_order", orderByItem.SortOrder.ToString()));
                e_column_list.Add(e_column);
                elementStack.Push(e_column);
                current_expression_tag = "column_expr";
                orderByItem.SortKey.accept(this);
                elementStack.Pop();
            }

            elementStack.Pop();
        }

        public override void preVisit(TCreateTableSqlStatement stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_create_table = new XElement(defaultNamespace + "create_table_statement");
            e_parent.Add(e_create_table);
            elementStack.Push(e_create_table);
            current_table_reference_tag = "table_name";
            stmt.TargetTable.accept(this);

            XElement e_column_list = new XElement(defaultNamespace + "column_definition_list");
            e_create_table.Add(e_column_list);
            elementStack.Push(e_column_list);
            stmt.ColumnList.accept(this);
            elementStack.Pop();

            if ((stmt.TableConstraints != null) && (stmt.TableConstraints.Count > 0))
            {
                XElement e_constraint_list = new XElement(defaultNamespace + "table_constraint_list");
                e_create_table.Add(e_constraint_list);
                elementStack.Push(e_constraint_list);
                stmt.TableConstraints.accept(this);
                elementStack.Pop();
            }
            //            if (stmt.getSubQuery() != null){
            //                stmt.getSubQuery().accept(this);
            //            }

            elementStack.Pop();
        }


        public override void preVisit(TDropIndexSqlStatement stmt)
        {
        }

        public override void preVisit(TDropTableSqlStatement stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_drop = new XElement(defaultNamespace + "drop_table_statement");
            e_parent.Add(e_drop);
            elementStack.Push(e_drop);
            current_objectName_tag = "table_name";
            stmt.TableName.accept(this);
            elementStack.Pop();
        }

        public override void preVisit(TTruncateStatement stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_truncate = new XElement(defaultNamespace + "truncate_table_statement");
            e_parent.Add(e_truncate);
            elementStack.Push(e_truncate);
            current_objectName_tag = "table_name";
            stmt.TableName.accept(this);
            elementStack.Pop();
        }



        public override void preVisit(TDropViewSqlStatement stmt)
        {
            appendStartTagWithIntProperty(stmt, "name", stmt.ViewName.ToString());
        }

        public override void preVisit(TDeleteSqlStatement stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_delete = new XElement(defaultNamespace + "delete_statement");
            e_parent.Add(e_delete);
            elementStack.Push(e_delete);


            if (stmt.CteList != null)
            {
                stmt.CteList.accept(this);
            }

            if (stmt.TopClause != null)
            {
                stmt.TopClause.accept(this);
            }

            current_table_reference_tag = "target_table";
            stmt.TargetTable.accept(this);

            if (stmt.joins.Count > 0)
            {

                XElement e_from_clause = new XElement(defaultNamespace + "from_clause");
                e_parent = (XElement)elementStack.Peek();
                e_parent.Add(e_from_clause);
                elementStack.Push(e_from_clause);
                stmt.joins.accept(this);
                elementStack.Pop();

            }

            if (stmt.OutputClause != null)
            {
                stmt.OutputClause.accept(this);
            }

            if (stmt.WhereClause != null)
            {
                stmt.WhereClause.accept(this);
            }

            if (stmt.ReturningClause != null)
            {
                stmt.ReturningClause.accept(this);
            }

            elementStack.Pop();

        }
        public override void postVisit(TDeleteSqlStatement stmt)
        {

        }

        public override void preVisit(TUpdateSqlStatement stmt)
        {

            e_parent = (XElement)elementStack.Peek();
            XElement e_update = new XElement(defaultNamespace + "update_statement");
            e_parent.Add(e_update);
            elementStack.Push(e_update);

            if (stmt.CteList != null)
            {
                stmt.CteList.accept(this);
            }

            if (stmt.TopClause != null)
            {
                stmt.TopClause.accept(this);
            }

            current_table_reference_tag = "target_table";
            stmt.TargetTable.accept(this);

            for (int i = 0; i < stmt.ResultColumnList.Count; i++)
            {
                current_expression_tag = "set_clause";
                stmt.ResultColumnList.getResultColumn(i).Expr.accept(this);
            }

            if (stmt.joins.Count > 0)
            {
                XElement e_from_clause = new XElement(defaultNamespace + "from_clause");
                e_parent = (XElement)elementStack.Peek();
                e_parent.Add(e_from_clause);
                elementStack.Push(e_from_clause);
                stmt.joins.accept(this);
                elementStack.Pop();
            }

            if (stmt.WhereClause != null)
            {
                stmt.WhereClause.accept(this);
            }

            if (stmt.OrderByClause != null)
            {
                stmt.OrderByClause.accept(this);
            }

            if (stmt.LimitClause != null)
            {
                stmt.LimitClause.accept(this);
            }

            if (stmt.OutputClause != null)
            {
                stmt.OutputClause.accept(this);
            }

            if (stmt.ReturningClause != null)
            {
                stmt.ReturningClause.accept(this);
            }

            elementStack.Pop();

        }


        public override void preVisit(TFunctionCall node)
        {
            string tag_name = TAG_FUNCTIONCALL;
            if (!string.ReferenceEquals(current_functionCall_tag, null))
            {
                tag_name = current_functionCall_tag;
                current_functionCall_tag = null;
            }
            e_parent = (XElement)elementStack.Peek();
            XElement e_functionCall = new XElement(defaultNamespace + tag_name);
            e_parent.Add(e_functionCall);
            e_functionCall.Add(new XAttribute("type", node.FunctionType.ToString()));
            e_functionCall.Add(new XAttribute("aggregateType", node.AggregateType.ToString()));
            e_functionCall.Add(new XAttribute("builtIn", (node.isBuiltIn(dbVendor)) ? "true" : "false"));

            elementStack.Push(e_functionCall);
            current_objectName_tag = TAG_FUNCTIONNAME;
            node.FunctionName.accept(this);
            current_objectName_tag = null;

            current_expression_list_tag = TAG_FUNCTIONARGS;
            XElement e_function = null;

            switch (node.FunctionType)
            {
                case EFunctionType.unknown_t:
                    e_function = new XElement(defaultNamespace + TAG_GENERIC_FUNCTION);
                    e_functionCall.Add(e_function);
                    elementStack.Push(e_function);
                    if (node.Args != null)
                    {
                        node.Args.accept(this);
                    }
                    elementStack.Pop();
                    break;
                case EFunctionType.udf_t:
                case EFunctionType.case_n_t:
                case EFunctionType.chr_t:
                    e_function = new XElement(defaultNamespace + TAG_GENERIC_FUNCTION);
                    e_functionCall.Add(e_function);
                    elementStack.Push(e_function);
                    if (node.Args != null)
                    {
                        node.Args.accept(this);
                    }
                    if (node.AnalyticFunction != null)
                    {
                        node.AnalyticFunction.accept(this);
                    }
                    elementStack.Pop();
                    break;
                case EFunctionType.cast_t:
                    e_function = new XElement(defaultNamespace + TAG_CAST_FUNCTION);
                    e_functionCall.Add(e_function);
                    elementStack.Push(e_function);
                    node.Expr1.accept(this);
                    node.Typename.accept(this);
                    elementStack.Pop();
                    break;
                case EFunctionType.convert_t:
                    e_function = new XElement(defaultNamespace + TAG_CONVERT_FUNCTION);
                    e_functionCall.Add(e_function);
                    elementStack.Push(e_function);
                    node.Expr1.accept(this);
                    if (node.Typename != null)
                    {
                        node.Typename.accept(this);
                    }
                    else
                    {
                        // convert in MySQL have no datatype argument
                    }

                    elementStack.Pop();

                    break;
                case EFunctionType.trim_t:
                    e_function = new XElement(defaultNamespace + TAG_TRIM_FUNCTION);
                    e_functionCall.Add(e_function);
                    elementStack.Push(e_function);
                    if (node.TrimArgument != null)
                    {
                        //node.getTrimArgument().accept(this);
                        TTrimArgument trimArgument = node.TrimArgument;
                        if (trimArgument.Both_trailing_leading != null)
                        {
                            XElement e_trim_style = new XElement(defaultNamespace + "style");
                            e_trim_style.Value = trimArgument.Both_trailing_leading.ToString();
                            e_function.Add(e_trim_style);
                        }
                        if (trimArgument.TrimCharacter != null)
                        {
                            current_expression_tag = "char_expr";
                            trimArgument.TrimCharacter.accept(this);
                        }
                        current_expression_tag = "source_expr";
                        trimArgument.StringExpression.accept(this);
                    }
                    elementStack.Pop();

                    break;
                case EFunctionType.extract_t:
                    e_function = new XElement(defaultNamespace + TAG_EXTRACT_FUNCTION);
                    e_functionCall.Add(e_function);
                    elementStack.Push(e_function);
                    if (node.Args != null)
                    { // extract xml
                        current_expression_list_tag = "functionArgs";
                        node.Args.accept(this);
                    }
                    else
                    {
                        XElement e_time = new XElement(defaultNamespace + "time");
                        e_time.Value = node.Extract_time_token.ToString();
                        e_function.Add(e_time);

                        if (node.Expr1 != null)
                        {
                            node.Expr1.accept(this);
                        }
                    }

                    elementStack.Pop();
                    break;
                case EFunctionType.treat_t:
                    e_function = new XElement(defaultNamespace + TAG_TREAT_FUNCTION);
                    e_functionCall.Add(e_function);
                    elementStack.Push(e_function);
                    node.Expr1.accept(this);
                    node.Typename.accept(this);
                    elementStack.Pop();
                    break;
                case EFunctionType.contains_t:
                    e_function = new XElement(defaultNamespace + TAG_CONTAINS_FUNCTION);
                    e_functionCall.Add(e_function);
                    elementStack.Push(e_function);
                    current_expression_tag = "column_reference";
                    current_expression_list_tag = null;
                    node.Expr1.accept(this);
                    current_expression_tag = "value_expression";
                    node.Expr2.accept(this);
                    elementStack.Pop();
                    break;
                case EFunctionType.freetext_t:
                    e_function = new XElement(defaultNamespace + TAG_CONTAINS_FUNCTION);
                    e_functionCall.Add(e_function);
                    elementStack.Push(e_function);
                    current_expression_tag = "column_reference";
                    current_expression_list_tag = null;
                    node.Expr1.accept(this);
                    current_expression_tag = "value_expression";
                    node.Expr2.accept(this);
                    elementStack.Pop();
                    break;
                case EFunctionType.range_n_t:
                case EFunctionType.position_t:
                case EFunctionType.substring_t:
                case EFunctionType.xmlquery_t:
                case EFunctionType.xmlcast_t:
                case EFunctionType.match_against_t:
                case EFunctionType.adddate_t:
                case EFunctionType.date_add_t:
                case EFunctionType.subdate_t:
                case EFunctionType.date_sub_t:
                case EFunctionType.timestampadd_t:
                case EFunctionType.timestampdiff_t:
                    XElement e_not_support = new XElement(defaultNamespace + "not_decode_function");
                    e_functionCall.Add(e_not_support);
                    e_not_support.Value = node.ToString();
                    break;
                default:
                    e_function = new XElement(defaultNamespace + TAG_GENERIC_FUNCTION);
                    e_functionCall.Add(e_function);
                    elementStack.Push(e_function);
                    if (node.Args != null)
                    {
                        node.Args.accept(this);
                    }
                    elementStack.Pop();
                    break;
            }

            if (node.WindowDef != null)
            {
                node.WindowDef.accept(this);
            }

            current_expression_list_tag = null;
            elementStack.Pop();

        }

        public override void preVisit(TWithinGroup withinGroup)
        {
            XElement e_functionCall = (XElement)elementStack.Peek();
            XElement e_within_group = new XElement(defaultNamespace + "within_group");
            e_functionCall.Add(e_within_group);
            elementStack.Push(e_within_group);
            withinGroup.orderBy.accept(this);
            elementStack.Pop();

        }

        public override void preVisit(TKeepDenseRankClause keepDenseRankClause)
        {
            XElement e_functionCall = (XElement)elementStack.Peek();
            XElement e_keepDenseRank = new XElement(defaultNamespace + "keep_dense_rank");
            e_functionCall.Add(e_keepDenseRank);
            e_keepDenseRank.Add(new XAttribute("first", keepDenseRankClause.First.ToString()));
            e_keepDenseRank.Add(new XAttribute("last", keepDenseRankClause.Last.ToString()));
            elementStack.Push(e_keepDenseRank);
            keepDenseRankClause.OrderBy.accept(this);
            elementStack.Pop();
        }
        public override void preVisit(TWindowDef windowDef)
        {
            //TWindowDef windowDef = node.getWindowDef();
            XElement e_functionCall = (XElement)elementStack.Peek();

            if (windowDef.withinGroup != null)
            {
                windowDef.withinGroup.accept(this);
            }

            if (windowDef.keepDenseRankClause != null)
            {
                windowDef.keepDenseRankClause.accept(this);
            }

            if (windowDef.includingOverClause)
            {
                XElement e_overClause = new XElement(defaultNamespace + "over_clause");
                elementStack.Push(e_overClause);
                e_functionCall.Add(e_overClause);
                if (windowDef.PartitionClause != null)
                {
                    XElement e_partition = new XElement(defaultNamespace + "partition_clause");
                    e_overClause.Add(e_partition);
                    elementStack.Push(e_partition);
                    current_expression_list_tag = "partitions";
                    windowDef.PartitionClause.ExpressionList.accept(this);
                    elementStack.Pop();
                }

                if (windowDef.orderBy != null)
                {
                    windowDef.orderBy.accept(this);
                }

                if (windowDef.WindowFrame != null)
                {
                    TWindowFrame windowFrame = windowDef.WindowFrame;
                    XElement e_winFrame = new XElement(defaultNamespace + "window_frame");
                    e_overClause.Add(e_winFrame);
                    e_winFrame.Add(new XAttribute("type", windowFrame.WindowExpressionType.ToString()));
                    elementStack.Push(e_winFrame);
                    windowFrame.StartBoundary.accept(this);
                    if (windowFrame.EndBoundary != null)
                    {
                        windowFrame.EndBoundary.accept(this);
                    }
                    elementStack.Pop();
                }

                elementStack.Pop();//e_overClause
            }

        }

        public override void preVisit(TWindowFrameBoundary boundary)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_boundary = new XElement(defaultNamespace + "window_frame_boundary");
            e_parent.Add(e_boundary);
            e_boundary.Add(new XAttribute("type", boundary.BoundaryType.ToString()));
            if (boundary.BoundaryNumber != null)
            {
                e_boundary.Add(new XAttribute("offset_value", boundary.BoundaryNumber.ToString()));
            }

            //elementStack.Push(e_insert);
        }


        public override void preVisit(TInsertSqlStatement stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_insert = new XElement(defaultNamespace + "insert_statement");
            e_parent.Add(e_insert);
            e_insert.Add(new XAttribute("insertSource", stmt.InsertSource.ToString()));

            elementStack.Push(e_insert);

            if (stmt.CteList != null)
            {
                stmt.CteList.accept(this);
            }

            if (stmt.TargetTable != null)
            {
                current_table_reference_tag = "target_table";
                stmt.TargetTable.accept(this);
            }
            else
            {
                // hive insert may have no target table
            }

            if (stmt.ColumnList != null)
            {
                current_objectName_list_tag = "column_list";
                stmt.ColumnList.accept(this);
            }




            //
            //            if (stmt.getTopClause() != null){
            //                stmt.getTopClause().accept(this);
            //            }
            //
            //
            //
            //            if (stmt.getOutputClause() != null){
            //                stmt.getOutputClause().accept(this);
            //            }
            //
            switch (stmt.InsertSource)
            {
                case EInsertSource.values:
                    XElement e_insert_values = new XElement(defaultNamespace + "insert_values");
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_insert_values);
                    elementStack.Push(e_insert_values);
                    XElement e_row_values;
                    for (int i = 0; i < stmt.Values.Count; i++)
                    {
                        e_row_values = new XElement(defaultNamespace + "row_values");
                        e_insert_values.Add(e_row_values);
                        elementStack.Push(e_row_values);
                        TMultiTarget multiTarget = stmt.Values.getMultiTarget(i);

                        for (int j = 0; j < multiTarget.ColumnList.Count; j++)
                        {
                            if (multiTarget.ColumnList.getResultColumn(j).PlaceHolder)
                            {
                                continue; // teradata allow empty value
                            }
                            multiTarget.ColumnList.getResultColumn(j).Expr.accept(this);
                        }

                        elementStack.Pop(); //e_row_values
                    }
                    elementStack.Pop(); // e_insert_values

                    break;
                case EInsertSource.subquery:
                    current_query_expression_tag = "insert_query";
                    stmt.SubQuery.accept(this);
                    break;
                case EInsertSource.values_empty:
                    break;
                case EInsertSource.values_function:
                    //stmt.getFunctionCall().accept(this);
                    break;
                case EInsertSource.values_oracle_record:
                    //stmt.getRecordName().accept(this);
                    break;
                case EInsertSource.set_column_value:
                    //stmt.getSetColumnValues().accept(this);
                    break;
                case EInsertSource.execute:
                    XElement e_insert_execute = new XElement(defaultNamespace + "insert_execute");
                    e_parent = (XElement)elementStack.Peek();
                    e_parent.Add(e_insert_execute);
                    elementStack.Push(e_insert_execute);
                    stmt.ExecuteStmt.accept(this);
                    elementStack.Pop();
                    break;
                default:
                    break;
            }
            //
            //            if (stmt.getReturningClause() != null){
            //                stmt.getReturningClause().accept(this);
            //            }
            elementStack.Pop();
        }
        public override void postVisit(TInsertSqlStatement stmt)
        {
            appendEndTag(stmt);
        }

        public override void preVisit(TMultiTarget node)
        {
            appendStartTag(node);
            if (node.ColumnList != null)
            {
                node.ColumnList.accept(this);
            }

            if (node.SubQuery != null)
            {
                node.SubQuery.accept(this);
            }
        }


        public override void preVisit(TMultiTargetList node)
        {
            appendStartTag(node);
            for (int i = 0; i < node.Count; i++)
            {
                node.getMultiTarget(i).accept(this);
            }
        }

        public override void postVisit(TMultiTargetList node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TCTE node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_cte = new XElement(defaultNamespace + "cte");
            e_parent.Add(e_cte);
            elementStack.Push(e_cte);
            current_objectName_tag = "expression_name";
            node.TableName.accept(this);

            if (node.ColumnList != null)
            {
                current_objectName_list_tag = "column_list";
                node.ColumnList.accept(this);
            }
            if (node.Subquery != null)
            {
                node.Subquery.DummyTag = TOP_STATEMENT;
                node.Subquery.accept(this);
            }
            else if (node.UpdateStmt != null)
            {
                node.UpdateStmt.accept(this);
            }
            else if (node.InsertStmt != null)
            {
                node.InsertStmt.accept(this);
            }
            else if (node.DeleteStmt != null)
            {
                node.DeleteStmt.accept(this);
            }

            elementStack.Pop();
        }

        public override void postVisit(TCTE node)
        {

        }

        public override void preVisit(TCTEList node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_cte_list = new XElement(defaultNamespace + "cte_list");
            e_parent.Add(e_cte_list);
            elementStack.Push(e_cte_list);
            for (int i = 0; i < node.Count; i++)
            {
                node.getCTE(i).accept(this);
            }
            elementStack.Pop();
        }
        public override void postVisit(TCTEList node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TAssignStmt node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_assign_stmt = new XElement(defaultNamespace + "assignment_statement");
            e_parent.Add(e_assign_stmt);
            elementStack.Push(e_assign_stmt);
            current_expression_tag = "left";
            node.Left.accept(this);
            current_expression_tag = "right";
            node.Expression.accept(this);
            elementStack.Pop();
        }
        public override void postVisit(TAssignStmt node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TIfStmt node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_if_stmt = new XElement(defaultNamespace + "if_statement");
            e_parent.Add(e_if_stmt);
            elementStack.Push(e_if_stmt);
            current_expression_tag = "condition";
            node.Condition.accept(this);
            current_statement_list_tag = "then_statement_list";
            node.ThenStatements.accept(this);
            if (node.ElseifStatements.Count > 0)
            {
                XElement e_elsif_cause_list = new XElement(defaultNamespace + "elsif_clause_list");
                e_if_stmt.Add(e_elsif_cause_list);
                elementStack.Push(e_elsif_cause_list);
                for (int i = 0; i < node.ElseifStatements.Count; i++)
                {
                    TElsifStmt elsifStmt = (TElsifStmt)node.ElseifStatements.get(i);

                    XElement e_elsif_cause = new XElement(defaultNamespace + "elsif_clause");
                    e_elsif_cause_list.Add(e_elsif_cause);
                    elementStack.Push(e_elsif_cause);
                    current_expression_tag = "condition";
                    elsifStmt.Condition.accept(this);
                    elsifStmt.ThenStatements.accept(this);
                    elementStack.Pop();
                }

                elementStack.Pop();
            }
            if (node.ElseStatements.Count > 0)
            {
                current_statement_list_tag = "else_statement_list";
                node.ElseStatements.accept(this);
            }

            elementStack.Pop();
        }

        public override void preVisit(TMssqlIfElse node)
        {
            e_parent = (XElement)elementStack.Peek();

            XElement e_if_stmt = new XElement(defaultNamespace + "if_statement");
            e_parent.Add(e_if_stmt);
            elementStack.Push(e_if_stmt);
            if (node.Condition != null)
            {
                current_expression_tag = "condition";
                node.Condition.accept(this);
            }

            current_statement_list_tag = "then_statement_list";
            TStatementList ifList = new TStatementList();
            ifList.add(node.Stmt);
            ifList.accept(this);

            if (node.ElseStmt != null)
            {
                current_statement_list_tag = "else_statement_list";
                TStatementList elseList = new TStatementList();
                elseList.add(node.ElseStmt);
                elseList.accept(this);
            }
            elementStack.Pop();
        }

        public override void preVisit(TBasicStmt node)
        {
            //appendStartTag(node);
            //outputNodeData(node);
            e_parent = (XElement)elementStack.Peek();
            XElement e_basic_stmt = new XElement(defaultNamespace + "plsql_basic_statement");
            e_parent.Add(e_basic_stmt);
            elementStack.Push(e_basic_stmt);
            node.Expr.accept(this);
            elementStack.Pop();

        }

        public override void preVisit(TCaseStmt node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_case_stmt = new XElement(defaultNamespace + "case_stmt");
            e_parent.Add(e_case_stmt);
            if (node.CaseExpr != null)
            {
                elementStack.Push(e_case_stmt);
                node.CaseExpr.accept(this);
                elementStack.Pop();
            }
        }

        public override void preVisit(TCaseExpression node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_case_expr = new XElement(defaultNamespace + TAG_CASE_EXPR);
            e_parent.Add(e_case_expr);
            elementStack.Push(e_case_expr);


            if (node.Input_expr != null)
            {
                current_expression_tag = "input_expression";
                node.Input_expr.accept(this);
            }

            XElement e_when_then; // = new XElement(defaultNamespace +  "when_then_clause");
            for (int i = 0; i < node.WhenClauseItemList.Count; i++)
            {
                e_when_then = new XElement(defaultNamespace + "when_then_clause");
                e_case_expr.Add(e_when_then);
                elementStack.Push(e_when_then);
                current_expression_tag = "search_expression";
                node.WhenClauseItemList.getWhenClauseItem(i).Comparison_expr.accept(this);
                current_expression_tag = "result_expression";
                if (node.WhenClauseItemList.getWhenClauseItem(i).Return_expr != null)
                {
                    node.WhenClauseItemList.getWhenClauseItem(i).Return_expr.accept(this);
                }
                else if (node.WhenClauseItemList.getWhenClauseItem(i).Statement_list != null)
                {
                    XElement result_expression = new XElement(defaultNamespace + current_expression_tag);
                    e_when_then.Add(result_expression);
                    elementStack.Push(result_expression);
                    node.WhenClauseItemList.getWhenClauseItem(i).Statement_list.accept(this);
                    elementStack.Pop();
                }
                elementStack.Pop();
            }

            if (node.Else_expr != null)
            {
                current_expression_tag = "else_expression";
                node.Else_expr.accept(this);
            }

            if (node.Else_statement_list.Count > 0)
            {
                node.Else_statement_list.accept(this);
            }
            elementStack.Pop();
        }

        public override void preVisit(TWhenClauseItemList node)
        {
            appendStartTag(node);
            for (int i = 0; i < node.Count; i++)
            {
                node.getWhenClauseItem(i).accept(this);
            }

        }
        public override void postVisit(TWhenClauseItemList node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TWhenClauseItem node)
        {
            appendStartTag(node);
            node.Comparison_expr.accept(this);
            if (node.Return_expr != null)
            {
                node.Return_expr.accept(this);
            }
            else if (node.Statement_list.Count > 0)
            {
                node.Statement_list.accept(this);
            }
        }


        public override void preVisit(TCloseStmt node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_close_stmt = new XElement(defaultNamespace + "close_statement");
            e_parent.Add(e_close_stmt);
            elementStack.Push(e_close_stmt);
            current_objectName_tag = "cursor_name";
            node.CursorName.accept(this);
            elementStack.Pop();
        }

        public override void postVisit(TCloseStmt node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TPlsqlCreateTrigger node)
        {
            appendStartTagWithIntProperty(node, "name", node.TriggerName.ToString());
            node.EventClause.accept(this);
            if (node.FollowsTriggerList != null)
            {
                node.FollowsTriggerList.accept(this);
            }
            if (node.WhenCondition != null)
            {
                node.WhenCondition.accept(this);
            }
            node.TriggerBody.accept(this);

        }



        public override void preVisit(TTypeAttribute node)
        {
            appendStartTag(node);
            node.AttributeName.accept(this);
            node.Datatype.accept(this);
        }

        public override void postVisit(TTypeAttribute node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TTypeAttributeList node)
        {
            appendStartTag(node);
            for (int i = 0; i < node.Count; i++)
            {
                node.getAttributeItem(i).accept(this);
            }
        }

        public override void postVisit(TTypeAttributeList node)
        {
            appendEndTag(node);
        }


        public override void preVisit(TPlsqlCreateTypeBody stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_create_type_body = new XElement(defaultNamespace + "create_type_body_statement");
            e_parent.Add(e_create_type_body);
            elementStack.Push(e_create_type_body);
            current_objectName_tag = "type_name";
            stmt.TypeName.accept(this);
            stmt.BodyStatements.accept(this);
            elementStack.Pop();
        }


        public override void preVisit(TPlsqlVarrayTypeDefStmt node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_varray = new XElement(defaultNamespace + "declare_varray_type");
            e_parent.Add(e_varray);
            elementStack.Push(e_varray);
            e_varray.Add(new XAttribute("notnull", node.NotNull.ToString()));
            current_objectName_tag = "type_name";
            node.TypeName.accept(this);
            current_datatype_tag = "element_type";
            node.ElementDataType.accept(this);
            XElement e_size_limit = new XElement(defaultNamespace + "size_limit");
            e_varray.Add(e_size_limit);
            e_size_limit.Value = node.SizeLimit.ToString();

            elementStack.Pop();
        }

        public override void preVisit(TPlsqlTableTypeDefStmt node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_nested_table = new XElement(defaultNamespace + "declare_nested_table_type");
            e_parent.Add(e_nested_table);
            elementStack.Push(e_nested_table);
            current_objectName_tag = "type_name";
            node.TypeName.accept(this);
            current_datatype_tag = "element_type";
            node.ElementDataType.accept(this);
            elementStack.Pop();
        }

        public override void preVisit(TPlsqlCreateType node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_object_type = new XElement(defaultNamespace + "declare_object_type");
            e_parent.Add(e_object_type);
            elementStack.Push(e_object_type);

            current_objectName_tag = "type_name";
            node.TypeName.accept(this);

            if (node.Attributes != null)
            {
                for (int i = 0; i < node.Attributes.Count; i++)
                {
                    XElement e_attribute_type = new XElement(defaultNamespace + "attribute_type");
                    e_object_type.Add(e_attribute_type);
                    elementStack.Push(e_attribute_type);
                    current_objectName_tag = "attribute_name";
                    node.Attributes.getAttributeItem(i).AttributeName.accept(this);
                    node.Attributes.getAttributeItem(i).Datatype.accept(this);
                    elementStack.Pop();
                }
            }
            elementStack.Pop();
        }

        public override void preVisit(TPlsqlCreateType_Placeholder node)
        {
            TPlsqlCreateType createType = null;
            e_parent = (XElement)elementStack.Peek();
            XElement e_create_type = new XElement(defaultNamespace + "oracle_create_type_statement");
            e_parent.Add(e_create_type);
            elementStack.Push(e_create_type);
            e_create_type.Add(new XAttribute("createdType", node.CreatedType.ToString()));

            switch (node.CreatedType)
            {
                case EOracleCreateType.octIncomplete:
                    createType = node.ObjectStatement;
                    XElement e_imcomplelte_object_type = new XElement(defaultNamespace + "declare_incomplete_object_type");
                    e_create_type.Add(e_imcomplelte_object_type);
                    elementStack.Push(e_imcomplelte_object_type);
                    current_objectName_tag = "type_name";
                    createType.TypeName.accept(this);
                    elementStack.Pop();
                    break;
                case EOracleCreateType.octObject:
                    node.ObjectStatement.accept(this);
                    break;
                case EOracleCreateType.octNestedTable:
                    node.NestedTableStatement.accept(this);
                    break;
                case EOracleCreateType.octVarray:
                    node.VarrayStatement.accept(this);
                    break;
                default:
                    break;

            }

            //            switch(node.getKind()){
            //                case TBaseType.kind_define:
            //                case TBaseType.kind_create:
            //                    createType = node.getObjectStatement();
            //                    current_objectName_tag="type_name";
            //                    createType.getTypeName().accept(this);
            //
            //                    e_object_type = new XElement(defaultNamespace +  "object_type");
            //                    e_create_type.Add(e_object_type);
            //                    elementStack.Push(e_object_type);
            //                    if (createType.getAttributes() != null){
            //                        for(int i=0;i<createType.getAttributes().Count;i++){
            //                            XElement e_attribute_type = new XElement(defaultNamespace +  "attribute_type");
            //                            e_object_type.Add(e_attribute_type);
            //                            elementStack.Push(e_attribute_type);
            //                            current_objectName_tag="attribute_name";
            //                            createType.getAttributes().getAttributeItem(i).getAttributeName().accept(this);
            //                            createType.getAttributes().getAttributeItem(i).getDatatype().accept(this);
            //                            elementStack.Pop();
            //                        }
            //                    }
            //                    elementStack.Pop();
            //
            //                    break;
            //                case TBaseType.kind_create_incomplete:
            //                    createType = node.getObjectStatement();
            //                    current_objectName_tag="type_name";
            //                    createType.getTypeName().accept(this);
            //                    XElement e_imcomplelte_object_type = new XElement(defaultNamespace +  "incomplete_type");
            //                    e_create_type.Add(e_imcomplelte_object_type);
            //                    break;
            //                case TBaseType.kind_create_varray:
            //                    node.getVarrayStatement().accept(this);
            //                    break;
            //                case TBaseType.kind_create_nested_table:
            //                    node.getNestedTableStatement().accept(this);
            //                    break;
            //            }


            elementStack.Pop();
        }


        internal virtual void outputNodeData(TParseTreeNode node)
        {
            sb.Append(node.ToString());
        }

        public override void preVisit(TMssqlCommit node)
        {
            if (node.TransactionName != null)
            {
                appendStartTagWithIntProperty(node, "transactionName", node.TransactionName.ToString());
            }
            else
            {
                appendStartTag(node);
            }
            sb.Append(node.ToString());
        }

        public override void postVisit(TMssqlCommit node)
        {
            appendEndTag(node);
        }


        public override void preVisit(TMssqlRollback node)
        {
            if (node.TransactionName != null)
            {
                appendStartTagWithIntProperty(node, "transactionName", node.TransactionName.ToString());
            }
            else
            {
                appendStartTag(node);
            }
            sb.Append(node.ToString());
        }



        public override void preVisit(TMssqlSaveTran node)
        {
            if (node.TransactionName != null)
            {
                appendStartTagWithIntProperty(node, "transactionName", node.TransactionName.ToString());
            }
            else
            {
                appendStartTag(node);
            }
            sb.Append(node.ToString());
        }

        public override void postVisit(TMssqlSaveTran node)
        {
            appendEndTag(node);
        }


        public override void preVisit(TMssqlGo node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_go = new XElement(defaultNamespace + "go_statement");
            e_parent.Add(e_go);
        }

        public override void preVisit(TMssqlPrint node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_print = new XElement(defaultNamespace + "print_statement");
            e_parent.Add(e_print);
            elementStack.Push(e_print);
            node.Messages.accept(this);
            elementStack.Pop();
        }

        public override void preVisit(TMssqlCreateProcedure node)
        {

            e_parent = (XElement)elementStack.Peek();
            XElement e_create_procedure = new XElement(defaultNamespace + "create_procedure_statement");
            e_parent.Add(e_create_procedure);
            elementStack.Push(e_create_procedure);

            XElement e_procedure_spec = new XElement(defaultNamespace + "procedure_specification_statement");
            e_create_procedure.Add(e_procedure_spec);
            elementStack.Push(e_procedure_spec);
            current_objectName_tag = "procedure_name";
            node.ProcedureName.accept(this);


            if (node.ParameterDeclarations != null)
            {
                node.ParameterDeclarations.accept(this);
            }


            if (node.BodyStatements.Count > 0)
            {
                current_statement_list_tag = "body_statement_list";
                node.BodyStatements.accept(this);
            }

            elementStack.Pop();
            elementStack.Pop();
        }



        public override void preVisit(TParameterDeclarationList list)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_parameter_list = new XElement(defaultNamespace + "parameter_declaration_list");
            e_parent.Add(e_parameter_list);
            elementStack.Push(e_parameter_list);
            for (int i = 0; i < list.Count; i++)
            {
                list.getParameterDeclarationItem(i).accept(this);
            }
            elementStack.Pop();
        }

        public override void preVisit(TParameterDeclaration node)
        {
            //appendStartTag(node);
            string tag_name = "parameter_declaration";
            if (!string.ReferenceEquals(current_parameter_declaration_tag, null))
            {
                tag_name = current_parameter_declaration_tag;
                current_parameter_declaration_tag = null;
            }
            e_parent = (XElement)elementStack.Peek();
            XElement e_parameter = new XElement(defaultNamespace + tag_name);
            e_parent.Add(e_parameter);
            elementStack.Push(e_parameter);
            current_objectName_tag = "name";
            node.ParameterName.accept(this);
            node.DataType.accept(this);
            if (node.DefaultValue != null)
            {
                current_expression_tag = "default_value";
                node.DefaultValue.accept(this);
            }
            if (node.ParameterMode != EParameterMode.defaultValue)
            {
                XElement e_mode = new XElement(defaultNamespace + "mode");
                e_mode.Value = node.ParameterMode.ToString();
                e_parameter.Add(e_mode);
            }
            elementStack.Pop();
        }


        public override void preVisit(TMssqlCreateType stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_variable = new XElement(defaultNamespace + "mssql_create_type");
            e_parent.Add(e_variable);
            elementStack.Push(e_variable);
            current_objectName_tag = "type_name";
            stmt.Type_name.accept(this);
            if (stmt.Base_type != null)
            {
                current_datatype_tag = "base_type";
                stmt.Base_type.accept(this);
            }
            if (stmt.ExternalName != null)
            {
                current_objectName_tag = "external_name";
                stmt.ExternalName.accept(this);
            }
            elementStack.Pop();
        }

        public override void preVisit(TDeclareVariable node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_variable = new XElement(defaultNamespace + "variable");
            e_parent.Add(e_variable);
            elementStack.Push(e_variable);

            current_objectName_tag = "variable_name";
            node.VariableName.accept(this);
            if (node.Datatype != null)
            {
                node.Datatype.accept(this);
            }

            elementStack.Pop();

        }

        public override void postVisit(TDeclareVariable node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TDeclareVariableList node)
        {

            for (int i = 0; i < node.Count; i++)
            {
                node.getDeclareVariable(i).accept(this);
            }

        }

        public override void postVisit(TDeclareVariableList node)
        {
            appendEndTag(node);
        }

        public override void preVisit(TVarDeclStmt node)
        {
            XElement e_var_decl_stmt = null;
            e_parent = (XElement)elementStack.Peek();

            switch (node.DeclareType)
            {
                case EDeclareType.constant:
                    e_var_decl_stmt = new XElement(defaultNamespace + "constant_declaration_statement");
                    e_parent.Add(e_var_decl_stmt);
                    elementStack.Push(e_var_decl_stmt);
                    current_objectName_tag = "constant_name";
                    node.ElementName.accept(this);
                    node.DataType.accept(this);
                    current_expression_tag = "default_value";
                    node.DefaultValue.accept(this);
                    elementStack.Pop();
                    break;
                case EDeclareType.variable:
                    e_var_decl_stmt = new XElement(defaultNamespace + "variable_declaration_statement");
                    e_parent.Add(e_var_decl_stmt);
                    elementStack.Push(e_var_decl_stmt);
                    current_objectName_tag = "variable_name";
                    node.ElementName.accept(this);
                    node.DataType.accept(this);
                    if (node.DefaultValue != null)
                    {
                        current_expression_tag = "default_value";
                        node.DefaultValue.accept(this);
                    }
                    elementStack.Pop();
                    break;
                case EDeclareType.exception:
                    e_var_decl_stmt = new XElement(defaultNamespace + "exception_declaration_statement");
                    e_parent.Add(e_var_decl_stmt);
                    elementStack.Push(e_var_decl_stmt);
                    current_objectName_tag = "exception_name";
                    node.ElementName.accept(this);
                    elementStack.Pop();
                    break;
                case EDeclareType.subtype:
                    e_var_decl_stmt = new XElement(defaultNamespace + "subtype_definition_statement");
                    e_parent.Add(e_var_decl_stmt);
                    elementStack.Push(e_var_decl_stmt);
                    current_objectName_tag = "subtype_name";
                    node.ElementName.accept(this);
                    node.DataType.accept(this);
                    elementStack.Pop();
                    break;
                default:
                    e_var_decl_stmt = new XElement(defaultNamespace + "var_decl_stmt");
                    e_parent.Add(e_var_decl_stmt);
                    e_var_decl_stmt.Add(new XAttribute("type", node.DeclareType.ToString()));
                    e_var_decl_stmt.Value = node.ToString();
                    break;
            }

            //   elementStack.Pop();
        }

        public override void postVisit(TVarDeclStmt node)
        {
            appendEndTag(node);
        }


        public override void preVisit(TRaiseStmt node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_raise_stmt = new XElement(defaultNamespace + "raise_statement");
            e_parent.Add(e_raise_stmt);
            elementStack.Push(e_raise_stmt);
            if (node.ExceptionName != null)
            {
                current_objectName_tag = "exception_name";
                node.ExceptionName.accept(this);
            }
            elementStack.Pop();
        }

        public override void preVisit(TReturnStmt node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_return_stmt = new XElement(defaultNamespace + "return_statement");
            e_parent.Add(e_return_stmt);
            elementStack.Push(e_return_stmt);
            if (node.Expression != null)
            {
                current_objectName_tag = "expression";
                node.Expression.accept(this);
            }
            elementStack.Pop();
        }

        public override void preVisit(TMssqlReturn node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_return_stmt = new XElement(defaultNamespace + "return_statement");
            e_parent.Add(e_return_stmt);
            elementStack.Push(e_return_stmt);
            if (node.ReturnExpr != null)
            {
                current_objectName_tag = "expression";
                node.ReturnExpr.accept(this);
            }
            elementStack.Pop();
        }

        public override void preVisit(TPlsqlRecordTypeDefStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_record_type_stmt = new XElement(defaultNamespace + "record_type_definition_statement");
            e_parent.Add(e_record_type_stmt);
            elementStack.Push(e_record_type_stmt);
            current_objectName_tag = "type_name";
            stmt.TypeName.accept(this);

            XElement e_field_declarations = new XElement(defaultNamespace + "field_declaration_list");
            e_record_type_stmt.Add(e_field_declarations);
            elementStack.Push(e_field_declarations);
            for (int i = 0; i < stmt.FieldDeclarations.Count; i++)
            {
                current_parameter_declaration_tag = "record_field_declaration";
                stmt.FieldDeclarations.getParameterDeclarationItem(i).accept(this);
            }
            elementStack.Pop();

            elementStack.Pop();
        }

        public override void preVisit(TSqlplusCmdStatement stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_raise_stmt = new XElement(defaultNamespace + "sqlplus_command");
            e_parent.Add(e_raise_stmt);
        }

        public override void preVisit(TCursorDeclStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            switch (stmt.Kind)
            {
                case TCursorDeclStmt.kind_ref_cursor_type_definition:
                    XElement e_stmt = new XElement(defaultNamespace + "ref_cursor_type_definition_statement");
                    e_parent.Add(e_stmt);
                    elementStack.Push(e_stmt);
                    current_objectName_tag = "type_name";
                    stmt.CursorTypeName.accept(this);
                    if (stmt.Rowtype != null)
                    {
                        stmt.Rowtype.accept(this);
                    }
                    elementStack.Pop();
                    break;
                case TCursorDeclStmt.kind_cursor_declaration:
                    XElement e_cursor_stmt = new XElement(defaultNamespace + "cursor_declaration_statement");
                    e_parent.Add(e_cursor_stmt);
                    elementStack.Push(e_cursor_stmt);
                    current_objectName_tag = "cursor_name";
                    stmt.CursorName.accept(this);
                    if (stmt.CursorParameterDeclarations != null)
                    {
                        stmt.CursorParameterDeclarations.accept(this);
                    }
                    if (stmt.Rowtype != null)
                    {
                        current_datatype_tag = "return_type";
                        stmt.Rowtype.accept(this);
                    }
                    stmt.Subquery.DummyTag = TOP_STATEMENT;
                    stmt.Subquery.accept(this);
                    elementStack.Pop();
                    break;
                default:
                    XElement e_cursor_decl_stmt = new XElement(defaultNamespace + "cursor_decl_stmt");
                    e_parent.Add(e_cursor_decl_stmt);
                    e_cursor_decl_stmt.Value = stmt.ToString();
                    break;
            }
        }


        public override void preVisit(TLoopStmt stmt)
        {
            XElement e_stmt = null;
            e_parent = (XElement)elementStack.Peek();
            switch (stmt.Kind)
            {
                case TLoopStmt.basic_loop:
                    e_stmt = new XElement(defaultNamespace + "loop_statement");
                    break;
                case TLoopStmt.cursor_for_loop:
                    e_stmt = new XElement(defaultNamespace + "cursor_for_loop_statement");
                    break;
                case TLoopStmt.for_loop:
                    e_stmt = new XElement(defaultNamespace + "for_loop_statement");
                    break;
                case TLoopStmt.while_loop:
                    e_stmt = new XElement(defaultNamespace + "while_statement");
                    break;
            }

            if (e_stmt != null)
            {
                e_parent.Add(e_stmt);
                elementStack.Push(e_stmt);
                if (stmt.RecordName != null)
                {
                    current_objectName_tag = "record";
                    stmt.RecordName.accept(this);
                    
                    if (stmt.Subquery != null)
                    {
                        current_query_expression_tag = "select_statement";
                        stmt.Subquery.accept(this);
                    }
                    else if (stmt.CursorName != null)
                    {
                        current_objectName_tag = "cursor";
                        stmt.CursorName.accept(this);
                        if (stmt.CursorParameterNames != null)
                        {
                            current_expression_list_tag = "cursor_parameter_list";
                            stmt.CursorParameterNames.accept(this);
                        }
                    }
                }
                if (stmt.Condition != null)
                {
                    current_expression_tag = "condition";
                    stmt.Condition.accept(this);
                }
                if (stmt.BodyStatements != null)
                {
                    current_statement_list_tag = "loop";
                    stmt.BodyStatements.accept(this);
                }
                elementStack.Pop();
            }
        }

        public override void preVisit(TPlsqlContinue stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_continue_stmt = new XElement(defaultNamespace + "continue_statement");
            e_parent.Add(e_continue_stmt);
            elementStack.Push(e_continue_stmt);
            if (stmt.LabelName != null)
            {
                current_objectName_tag = "label_name";
                stmt.LabelName.accept(this);
            }
            if (stmt.Condition != null)
            {
                current_expression_tag = "condition";
                stmt.Condition.accept(this);
            }

            elementStack.Pop();

        }

        public override void preVisit(TPlsqlExecImmeStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_execute_immediate_stmt = new XElement(defaultNamespace + "execute_immediate_statement");
            e_parent.Add(e_execute_immediate_stmt);
            elementStack.Push(e_execute_immediate_stmt);

            if (stmt.DynamicSQL != null)
            {
                XElement e_dynamic_sql = new XElement(defaultNamespace + "dynamic_sql_stmt");
                e_dynamic_sql.Value = stmt.DynamicSQL;
                e_execute_immediate_stmt.Add(e_dynamic_sql);
            }

            if (stmt.DynamicStringExpr != null)
            {
                XElement e_dynamic_string = new XElement(defaultNamespace + "dynamic_string");
                elementStack.Push(e_dynamic_string);
                e_execute_immediate_stmt.Add(e_dynamic_string);
                preVisit(stmt.DynamicStringExpr);
                elementStack.Pop();
            }

            if (stmt.DynamicStatements != null)
            {
                preVisit(stmt.DynamicStatements);
            }

            elementStack.Pop();
        }

        public override void preVisit(TExitStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_continue_stmt = new XElement(defaultNamespace + "exit_statement");
            e_parent.Add(e_continue_stmt);
            elementStack.Push(e_continue_stmt);
            if (stmt.ExitlabelName != null)
            {
                current_objectName_tag = "label_name";
                stmt.ExitlabelName.accept(this);
            }
            current_expression_tag = "condition";
            if (stmt.WhenCondition != null)
            {
                stmt.WhenCondition.accept(this);
            }

            elementStack.Pop();

        }

        public override void preVisit(TFetchStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_continue_stmt = new XElement(defaultNamespace + "fetch_statement");
            e_parent.Add(e_continue_stmt);
            elementStack.Push(e_continue_stmt);
            current_objectName_tag = "cursor_name";
            stmt.CursorName.accept(this);
            current_expression_list_tag = "into_list";
            stmt.VariableNames.accept(this);

            elementStack.Pop();

        }
        public override void preVisit(TPlsqlGotoStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_goto_stmt = new XElement(defaultNamespace + "goto_statement");
            e_parent.Add(e_goto_stmt);
            elementStack.Push(e_goto_stmt);
            current_objectName_tag = "label_name";
            stmt.GotolabelName.accept(this);

        }

        public override void preVisit(TPlsqlNullStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_full_stmt = new XElement(defaultNamespace + "plsql_null_statement");
            e_parent.Add(e_full_stmt);
            e_full_stmt.Value = stmt.ToString();
        }

        public override void preVisit(TOracleCommentOnSqlStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_comment_on_stmt = new XElement(defaultNamespace + "comment_on_statement");
            e_parent.Add(e_comment_on_stmt);
            e_comment_on_stmt.Add(new XAttribute("object_type", Enum.GetName(typeof(EDbObjectType), stmt.DbObjType)));
            elementStack.Push(e_comment_on_stmt);
            current_objectName_tag = "object_name";
            stmt.ObjectName.accept(this);
            elementStack.Pop();
            XElement e_string = new XElement(defaultNamespace + "comment_message");
            e_comment_on_stmt.Add(e_string);
            e_string.Value = stmt.Message.String;
        }

        public override void preVisit(TOpenStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_open_stmt = new XElement(defaultNamespace + "open_statement");
            e_parent.Add(e_open_stmt);
            elementStack.Push(e_open_stmt);
            current_objectName_tag = "cursor_name";
            stmt.CursorName.accept(this);
            if (stmt.CursorParameterNames != null)
            {
                current_expression_list_tag = "parameter_list";
                stmt.CursorParameterNames.accept(this);
            }
            elementStack.Pop();
        }

        public override void preVisit(TOpenforStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_open_for_stmt = new XElement(defaultNamespace + "open_for_statement");
            e_parent.Add(e_open_for_stmt);
            elementStack.Push(e_open_for_stmt);
            current_objectName_tag = "variable_name";
            stmt.CursorVariableName.accept(this);
            if (stmt.Subquery != null)
            {
                stmt.Subquery.DummyTag = TOP_STATEMENT;
                stmt.Subquery.accept(this);
            }
            if (stmt.Dynamic_string != null)
            {
                XElement e_string = new XElement(defaultNamespace + "dynamic_string");
                e_open_for_stmt.Add(e_string);
                e_string.Value = stmt.Dynamic_string.ToString();
            }

            elementStack.Pop();
        }

        public override void preVisit(TPlsqlForallStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_forall_stmt = new XElement(defaultNamespace + "forall_statement");
            e_parent.Add(e_forall_stmt);
            elementStack.Push(e_forall_stmt);
            current_objectName_tag = "index_name";
            stmt.IndexName.accept(this);

            XElement e_bounds_clause = new XElement(defaultNamespace + "bounds_clause");
            e_forall_stmt.Add(e_bounds_clause);
            elementStack.Push(e_bounds_clause);
            switch (stmt.Bound_clause_kind)
            {
                case TPlsqlForallStmt.bound_clause_kind_normal:
                    e_bounds_clause.Add(new XAttribute("type", "normal"));
                    break;
                case TPlsqlForallStmt.bound_clause_kind_indices_of:
                    e_bounds_clause.Add(new XAttribute("type", "indeces_of"));
                    break;
                case TPlsqlForallStmt.bound_clause_kind_values_of:
                    e_bounds_clause.Add(new XAttribute("type", "values_of"));
                    break;
            }
            if (stmt.Lower_bound != null)
            {
                current_expression_tag = "lower_bound";
                stmt.Lower_bound.accept(this);
            }
            if (stmt.Upper_bound != null)
            {
                current_expression_tag = "upper_bound";
                stmt.Upper_bound.accept(this);
            }
            if (stmt.CollectionName != null)
            {
                current_objectName_tag = "collection_name";
                stmt.CollectionName.accept(this);
            }
            if (stmt.CollecitonNameExpr != null)
            {
                current_expression_tag = "collection_expr";
                stmt.CollecitonNameExpr.accept(this);
            }
            elementStack.Pop();


            XElement e_statement = new XElement(defaultNamespace + "statement");
            e_statement.Add(new XAttribute("type", stmt.Statement.sqlstatementtype.ToString()));
            e_forall_stmt.Add(e_statement);
            elementStack.Push(e_statement);
            stmt.Statement.DummyTag = TOP_STATEMENT;
            stmt.Statement.accept(this);
            elementStack.Pop();
            elementStack.Pop();

        }

        public override void preVisit(TRollbackStmt stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_rollback_stmt = new XElement(defaultNamespace + "rollback_statement");
            e_parent.Add(e_rollback_stmt);
           // e_rollback_stmt.Value = stmt.ToString();
        } 
        
        public override void preVisit(TCallStatement stmt)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_call_stmt = new XElement(defaultNamespace + "call_statement");
            e_parent.Add(e_call_stmt);
            elementStack.Push(e_call_stmt);
            current_objectName_tag = "routine_name";
            stmt.RoutineName.accept(this);
            if (stmt.Args != null)
            {
                current_expression_list_tag = "parameter_list";
                stmt.Args.accept(this);
            }
            elementStack.Pop();

        }

        public override void preVisit(TMdxSelect node)
        {
            e_parent = (XElement)elementStack.Peek();
            XElement e_select = new XElement(defaultNamespace + "mdx_select");
            e_parent.Add(e_select);
            elementStack.Push(e_select);

            if (node.Withs != null)
            {
                for (int i = 0; i < node.Withs.Count; i++)
                {
                    node.Withs[i].accept(this);
                }
            }

            if (node.Axes != null)
            {
                for (int i = 0; i < node.Axes.Count; i++)
                {
                    TMdxAxisNode mdxAxis = node.Axes[i];
                    mdxAxis.accept(this);
                }
            }

            if (node.Cube != null)
            {
                XElement e_cube_clause = new XElement(defaultNamespace + "cube_clause");
                e_select.Add(e_cube_clause);
                elementStack.Push(e_cube_clause);

                XElement e_cube_name = new XElement(defaultNamespace + "cube_name");
                e_cube_clause.Add(e_cube_name);
                e_cube_name.Value = node.Cube.Segments[0].ToString();
               
                elementStack.Pop();
            }

            if (node.Where != null)
            {
                node.Where.accept(this);
            }

            elementStack.Pop();
        }

        public override void preVisit(TMdxWithNode node)
        {
            XElement e_with_clause = new XElement(defaultNamespace + "with_clause");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_with_clause);
            elementStack.Push(e_with_clause);

            if (node is TMdxWithMemberNode)
            {
                preVisit((TMdxWithMemberNode)node);
            }
            else if (node is TMdxWithSetNode)
            {
                preVisit((TMdxWithSetNode)node);
            }

            elementStack.Pop();
        }

        private void preVisit(TMdxWithMemberNode node)
        {
            XElement e_mdx_with_member = new XElement(defaultNamespace + "mdx_with_member");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_mdx_with_member);
            elementStack.Push(e_mdx_with_member);

            if (node.NameNode != null)
            {
                XElement e_member_name = new XElement(defaultNamespace + "member_name");
                e_mdx_with_member.Add(e_member_name);
                e_member_name.Value = node.NameNode.ToString();
            }

            if (node.ExprNode != null)
            {
                XElement e_value_expr = new XElement(defaultNamespace + "value_expr");
                e_mdx_with_member.Add(e_value_expr);
                elementStack.Push(e_value_expr);
                handleMdxExpr(node.ExprNode);
                elementStack.Pop();
            }

            elementStack.Pop();
        }

        private void preVisit(TMdxWithSetNode node)
        {
            XElement e_mdx_set_member = new XElement(defaultNamespace + "mdx_with_set");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_mdx_set_member);
            elementStack.Push(e_mdx_set_member);

            if (node.NameNode != null)
            {
                XElement e_set_name = new XElement(defaultNamespace + "set_name");
                e_mdx_set_member.Add(e_set_name);
                e_set_name.Value = (node.NameNode.ToString());
            }

            if (node.ExprNode != null)
            {
                XElement e_value_expr = new XElement(defaultNamespace + "value_expr");
                e_mdx_set_member.Add(e_value_expr);
                elementStack.Push(e_value_expr);
                handleMdxExpr(node.ExprNode);
                elementStack.Pop();
            }

            elementStack.Pop();
        }

        public override void preVisit(TMdxWhereNode node)
        {
            XElement e_where_clause = new XElement(defaultNamespace + "where_clause");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_where_clause);
            elementStack.Push(e_where_clause);

            XElement e_expr = new XElement(defaultNamespace + "expr");
            e_where_clause.Add(e_expr);
            elementStack.Push(e_expr);

            if (node.Filter != null)
            {
                handleMdxExpr(node.Filter);
            }

            elementStack.Pop();
            elementStack.Pop();
        }

        public override void preVisit(TMdxAxisNode node)
        {
            XElement e_axis_clause = new XElement(defaultNamespace + "axis_clause");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_axis_clause);
            elementStack.Push(e_axis_clause);

            XElement e_expr = new XElement(defaultNamespace + "expr");
            e_axis_clause.Add(e_expr);
            elementStack.Push(e_expr);

            if (node.ExpNode != null)
            {
                handleMdxExpr(node.ExpNode);
            }

            elementStack.Pop();

            if (node.Name_OR_Number != null)
            {
                XElement e_on_axis = new XElement(defaultNamespace + "on_axis");
                e_axis_clause.Add(e_on_axis);
                elementStack.Push(e_on_axis);

                XElement e_string_value = new XElement(defaultNamespace + "string_value");
                e_string_value.Value = node.Name_OR_Number.ToString();
                e_on_axis.Add(e_string_value);
                elementStack.Pop();
            }

            elementStack.Pop();
        }

        public override void preVisit(TMdxFunctionNode node)
        {
            e_parent = (XElement)elementStack.Peek();

            XElement e_mdx_function = new XElement(defaultNamespace + "mdx_function");
            e_parent.Add(e_mdx_function);
            elementStack.Push(e_mdx_function);

            XElement e_function_name = new XElement(defaultNamespace + "function_name");
            e_mdx_function.Add(e_function_name);
            elementStack.Push(e_function_name);

            XElement e_segment = new XElement(defaultNamespace + "segment");
            e_function_name.Add(e_segment);
            elementStack.Push(e_segment);
            preVisit(node.FunctionSegment);
            elementStack.Pop();

            elementStack.Pop();

            if (node.Arguments.Count > 0)
            {
                XElement e_function_args = new XElement(defaultNamespace + "function_args");
                e_mdx_function.Add(e_function_args);
                elementStack.Push(e_function_args);

                for (int i = 0; i < node.Arguments.Count; i++)
                {
                    if (node.ExpSyntax.Equals(EMdxExpSyntax.Method) && i == 0)
                    {
                        continue;
                    }
                    TMdxExpNode XElement = node.Arguments[i];
                    XElement e_mdx_expr = new XElement(defaultNamespace + "mdx_expr");
                    e_function_args.Add(e_mdx_expr);
                    elementStack.Push(e_mdx_expr);
                    handleMdxExpr(XElement);
                    elementStack.Pop();
                }

                elementStack.Pop();
            }

            if (node.ExpSyntax.Equals(EMdxExpSyntax.Method))
            {
                TMdxExpNode XElement = node.Arguments[0];
                XElement e_mdx_expr = new XElement(defaultNamespace + "object_expr");
                e_mdx_function.Add(e_mdx_expr);
                elementStack.Push(e_mdx_expr);
                handleMdxExpr(XElement);
                elementStack.Pop();
            }

            e_mdx_function.Add(new XAttribute("expr_syntax", Enum.GetName(typeof(EMdxExpSyntax), node.ExpSyntax)));

            elementStack.Pop();
        }

        private void handleMdxExpr(TMdxExpNode element)
        {
            if (element is TMdxUnaryOpNode)
            {
                ((TMdxUnaryOpNode)element).accept(this);
            }
            else if (element is TMdxBinOpNode)
            {
                ((TMdxBinOpNode)element).accept(this);
            }
            else
            {
                e_parent = (XElement)elementStack.Peek();
                XElement e_mdx_value_primary_expr = new XElement(defaultNamespace + "mdx_value_primary_expr");
                e_parent.Add(e_mdx_value_primary_expr);
                elementStack.Push(e_mdx_value_primary_expr);
                element.accept(this);
                elementStack.Pop();
            }
        }

        public override void preVisit(TMdxPropertyNode node)
        {
            e_parent = (XElement)elementStack.Peek();

            XElement e_mdx_function = new XElement(defaultNamespace + "mdx_property");
            e_parent.Add(e_mdx_function);
            elementStack.Push(e_mdx_function);

            XElement e_function_name = new XElement(defaultNamespace + "function_name");
            e_function_name.Value = node.FunctionName;
            e_mdx_function.Add(e_function_name);

            if (node.Arguments.Count > 0)
            {
                XElement e_function_args = new XElement(defaultNamespace + "function_args");
                e_mdx_function.Add(e_function_args);
                elementStack.Push(e_function_args);

                for (int i = 0; i < node.Arguments.Count ; i++)
                {
                    TMdxExpNode XElement = node.Arguments[i];
                    XElement e_mdx_expr = new XElement(defaultNamespace + "mdx_expr");
                    e_function_args.Add(e_mdx_expr);
                    elementStack.Push(e_mdx_expr);
                    handleMdxExpr(XElement);
                    elementStack.Pop();
                }

                elementStack.Pop();
            }

            e_mdx_function.Add(new XAttribute("expr_syntax", Enum.GetName(typeof(EMdxExpSyntax), node.ExpSyntax)));

            elementStack.Pop();
        }

        public override void preVisit(TMdxSetNode node)
        {
            e_parent = (XElement)elementStack.Peek();

            XElement e_mdx_set = new XElement(defaultNamespace + "mdx_set");
            e_parent.Add(e_mdx_set);
            elementStack.Push(e_mdx_set);

            XElement e_mdx_exprs = new XElement(defaultNamespace + "mdx_exprs");
            e_mdx_set.Add(e_mdx_exprs);
            elementStack.Push(e_mdx_exprs);

            for (int i = 0; i < node.TupleList.Count; i++)
            {
                TMdxExpNode element = node.TupleList[i];
                XElement e_mdx_expr = new XElement(defaultNamespace + "mdx_expr");
                e_mdx_exprs.Add(e_mdx_expr);
                elementStack.Push(e_mdx_expr);
                handleMdxExpr(element);
                elementStack.Pop();
            }

            elementStack.Pop();
            elementStack.Pop();
        }

        public override void preVisit(TMdxTupleNode node)
        {
            e_parent = (XElement)elementStack.Peek();

            XElement e_mdx_tuple = new XElement(defaultNamespace + "mdx_tuple");
            e_parent.Add(e_mdx_tuple);
            elementStack.Push(e_mdx_tuple);

            XElement e_mdx_exprs = new XElement(defaultNamespace + "mdx_members");
            e_mdx_tuple.Add(e_mdx_exprs);
            elementStack.Push(e_mdx_exprs);

            for (int i = 0; i < node.ExprList.Count; i++)
            {
                TMdxExpNode element = node.ExprList[i];
                XElement e_mdx_expr = new XElement(defaultNamespace + "mdx_expr");
                e_mdx_exprs.Add(e_mdx_expr);
                elementStack.Push(e_mdx_expr);
                handleMdxExpr(element);
                elementStack.Pop();
            }

            elementStack.Pop();
            elementStack.Pop();
        }

        public override void preVisit(TMdxBinOpNode node)
        {
            e_parent = (XElement)elementStack.Peek();

            XElement e_mdx_binary_expr = new XElement(defaultNamespace + "mdx_binary_expr");
            e_parent.Add(e_mdx_binary_expr);
            elementStack.Push(e_mdx_binary_expr);

            XElement e_left_expr = new XElement(defaultNamespace + "left_expr");
            e_mdx_binary_expr.Add(e_left_expr);
            elementStack.Push(e_left_expr);
            handleMdxExpr(node.LeftExprNode);
            elementStack.Pop();

            XElement e_right_expr = new XElement(defaultNamespace + "right_expr");
            e_mdx_binary_expr.Add(e_right_expr);
            elementStack.Push(e_right_expr);
            handleMdxExpr(node.RightExprNode);
            elementStack.Pop();

            e_mdx_binary_expr.Add(new XAttribute("operator", node.Operator.ToString()));

            elementStack.Pop();
        }

        public override void preVisit(TMdxUnaryOpNode node)
        {
            e_parent = (XElement)elementStack.Peek();

            XElement e_mdx_unary_expr = new XElement(defaultNamespace + "mdx_unary_expr");
            e_parent.Add(e_mdx_unary_expr);
            elementStack.Push(e_mdx_unary_expr);

            XElement e_expr = new XElement(defaultNamespace + "expr");
            e_mdx_unary_expr.Add(e_expr);
            elementStack.Push(e_expr);
            handleMdxExpr(node.ExpNode);
            elementStack.Pop();

            e_mdx_unary_expr.Add(new XAttribute("operator", node.Operator.ToString()));

            elementStack.Pop();
        }

        public override void preVisit(TMdxCaseNode node)
        {
            e_parent = (XElement)elementStack.Peek();

            XElement e_mdx_case = new XElement(defaultNamespace + "mdx_case");
            e_parent.Add(e_mdx_case);
            elementStack.Push(e_mdx_case);

            if (node.Condition != null)
            {
                XElement e_condition_expr = new XElement(defaultNamespace + "condition_expr");
                e_mdx_case.Add(e_condition_expr);
                elementStack.Push(e_condition_expr);
                handleMdxExpr(node.Condition);
                elementStack.Pop();
            }

            if (node.WhenList != null)
            {
                XElement e_when_then_list = new XElement(defaultNamespace + "when_then_list");
                e_mdx_case.Add(e_when_then_list);
                elementStack.Push(e_when_then_list);
                for (int i = 0; i < node.WhenList.Count; i++)
                {
                    TMdxWhenNode whenNode = (TMdxWhenNode)node.WhenList[i];
                    whenNode.accept(this);
                }
                elementStack.Pop();
            }

            if (node.ElseExpr != null)
            {
                XElement e_else_value = new XElement(defaultNamespace + "else_value");
                e_mdx_case.Add(e_else_value);
                elementStack.Push(e_else_value);
                handleMdxExpr(node.ElseExpr);
                elementStack.Pop();
            }

            elementStack.Pop();
        }

        public override void preVisit(TMdxWhenNode node)
        {
            XElement e_mdx_when_then = new XElement(defaultNamespace + "mdx_when_then");
            e_parent = (XElement)elementStack.Peek();
            e_parent.Add(e_mdx_when_then);
            elementStack.Push(e_mdx_when_then);

            if (node.WhenExpr != null)
            {
                XElement e_when_expr = new XElement(defaultNamespace + "when_expr");
                e_mdx_when_then.Add(e_when_expr);
                elementStack.Push(e_when_expr);
                handleMdxExpr(node.WhenExpr);
                elementStack.Pop();
            }

            if (node.ThenExpr != null)
            {
                XElement e_then_value = new XElement(defaultNamespace + "then_value");
                e_mdx_when_then.Add(e_then_value);
                elementStack.Push(e_then_value);
                handleMdxExpr(node.WhenExpr);
                elementStack.Pop();
            }

            elementStack.Pop();
        }

        public override void preVisit(TMdxIdentifierNode node)
        {
            e_parent = (XElement)elementStack.Peek();

            XElement e_mdx_identifier = new XElement(defaultNamespace + "mdx_identifier");
            e_parent.Add(e_mdx_identifier);
            elementStack.Push(e_mdx_identifier);

            for (int i = 0; i < node.Segments.Count; i++)
            {
                XElement e_segment = new XElement(defaultNamespace + "segment");
                e_mdx_identifier.Add(e_segment);
                elementStack.Push(e_segment);

                IMdxIdentifierSegment segment = node.Segments[i];
                preVisit(segment);

                elementStack.Pop();
            }
            elementStack.Pop();
        }

        public override void preVisit(TMdxStringConstNode node)
        {
            e_parent = (XElement)elementStack.Peek();

            XElement e_mdx_constant = new XElement(defaultNamespace + "mdx_constant");
            e_parent.Add(e_mdx_constant);
            elementStack.Push(e_mdx_constant);

            XElement e_string_value = new XElement(defaultNamespace + "string_value");
            e_string_value.Value = node.ToString();
            e_mdx_constant.Add(e_string_value);

            e_mdx_constant.Add(new XAttribute("kind", "String"));

            elementStack.Pop();
        }

        public override void preVisit(TMdxIntegerConstNode node)
        {
            e_parent = (XElement)elementStack.Peek();

            XElement e_mdx_constant = new XElement(defaultNamespace + "mdx_constant");
            e_parent.Add(e_mdx_constant);
            elementStack.Push(e_mdx_constant);

            XElement e_string_value = new XElement(defaultNamespace + "string_value");
            e_string_value.Value = node.ToString();
            e_mdx_constant.Add(e_string_value);

            e_mdx_constant.Add(new XAttribute("kind", "Integer"));

            elementStack.Pop();
        }

        public override void preVisit(TMdxFloatConstNode node)
        {
            e_parent = (XElement)elementStack.Peek();

            XElement e_mdx_constant = new XElement(defaultNamespace + "mdx_constant");
            e_parent.Add(e_mdx_constant);
            elementStack.Push(e_mdx_constant);

            XElement e_string_value = new XElement(defaultNamespace + "string_value");
            e_string_value.Value = node.ToString();
            e_mdx_constant.Add(e_string_value);

            e_mdx_constant.Add(new XAttribute("kind", "Float"));

            elementStack.Pop();
        }

        private void preVisit(IMdxIdentifierSegment segment)
        {
            e_parent = (XElement)elementStack.Peek();

            if (segment.Name != null)
            {
                XElement e_name_segment = new XElement(defaultNamespace + "name_segment");
                e_parent.Add(e_name_segment);
                e_name_segment.Add(new XAttribute("value", segment.Name));
                if (segment.Quoting != null)
                {
                    e_name_segment.Add(new XAttribute("quoting", Enum.GetName(typeof(EMdxQuoting), segment.Quoting)));
                }
            }

            if (segment.KeyParts != null)
            {
                XElement e_key_segment = new XElement(defaultNamespace + "key_segment");
                e_parent.Add(e_key_segment);
                elementStack.Push(e_key_segment);

                for (int j = 0; j < segment.KeyParts.Count; j++)
                {
                    preVisit((IMdxIdentifierSegment)segment.KeyParts[j]);
                }
                elementStack.Pop();
            }

        }

    }
}