using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.IO;
using gudusoft.gsqlparser.stmt;
using gudusoft.gsqlparser.nodes;
using gudusoft.gsqlparser.nodes.hive;
using gudusoft.gsqlparser.stmt.hive;

namespace gudusoft.gsqlparser.test
{
    /// <summary>
    /// UnitTestOracle 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTestHive
    {
        TGSqlParser parser;

        public UnitTestHive()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
            parser = new TGSqlParser(EDbVendor.dbvhive);
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性: 
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestQuery()
        {
            parser.sqltext = "select f from t where f>1";
            int ret = parser.parse();
            Assert.IsTrue(ret == 0, parser.Errormessage);
        }

        [TestMethod]
        public void TestHiveFiles()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"new_dotnet\hive\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }


        [TestMethod]
        public void testWindowing()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT a, COUNT(b) OVER (PARTITION BY c)\n" + "FROM T;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 2);

            TExpression expression = select.ResultColumnList.getResultColumn(1).Expr;
            Assert.IsTrue(expression.ExpressionType == EExpressionType.function_t);

            Assert.IsTrue(select.ResultColumnList.getResultColumn(1).ToString().Equals("COUNT(b) OVER (PARTITION BY c)", StringComparison.CurrentCultureIgnoreCase));
            TFunctionCall functionCall = expression.FunctionCall;
            Assert.IsTrue(functionCall.FunctionName.ToString().Equals("COUNT", StringComparison.CurrentCultureIgnoreCase));

            TWindowDef windowDef = functionCall.WindowDef;
            Assert.IsTrue(windowDef.PartitionClause.ExpressionList.size() == 1);
            Assert.IsTrue(windowDef.PartitionClause.ExpressionList.getExpression(0).ToString().Equals("c", StringComparison.CurrentCultureIgnoreCase));

            //System.out.println(windowSpecification.toString());

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_fake);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.objectname);

        }

        [TestMethod]
        public void testWindowing2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT a, SUM(b) OVER (PARTITION BY c ORDER BY d ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW)\n" + "FROM T;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 2);

            TExpression expression = select.ResultColumnList.getResultColumn(1).Expr;
            Assert.IsTrue(expression.ExpressionType == EExpressionType.function_t);

            TFunctionCall functionCall = expression.FunctionCall;
            Assert.IsTrue(functionCall.FunctionName.ToString().Equals("SUM", StringComparison.CurrentCultureIgnoreCase));

            TWindowDef windowSpecification = functionCall.WindowDef;
            Assert.IsTrue(windowSpecification.PartitionClause.ExpressionList.size() == 1);
            Assert.IsTrue(windowSpecification.PartitionClause.ExpressionList.getExpression(0).ToString().Equals("c", StringComparison.CurrentCultureIgnoreCase));

            TOrderByItem orderByItem = windowSpecification.orderBy.Items.getOrderByItem(0);
            Assert.IsTrue(orderByItem.ToString().Equals("d", StringComparison.CurrentCultureIgnoreCase));

            TWindowFrame windowFrame = windowSpecification.WindowFrame;
            Assert.IsTrue(windowFrame.WindowExpressionType == ELimitRowType.Rows);
            TWindowFrameBoundary start = windowFrame.StartBoundary;
            TWindowFrameBoundary end = windowFrame.EndBoundary;
            Assert.IsTrue(start.BoundaryType == EBoundaryType.ebtUnboundedPreceding);
            Assert.IsTrue(end.BoundaryType == EBoundaryType.ebtCurrentRow);
            //System.out.println(windowSpecification.toString());

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_fake);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.objectname);

        }

        [TestMethod]
        public void testWindowing3()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT a, AVG(b) OVER (PARTITION BY c ORDER BY d ROWS BETWEEN 3 PRECEDING AND 3 FOLLOWING)\n" + "FROM T;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 2);

            TExpression expression = select.ResultColumnList.getResultColumn(1).Expr;
            Assert.IsTrue(expression.ExpressionType == EExpressionType.function_t);

            TFunctionCall functionCall = expression.FunctionCall;
            Assert.IsTrue(functionCall.FunctionName.ToString().Equals("AVG", StringComparison.CurrentCultureIgnoreCase));

            TWindowDef windowSpecification = functionCall.WindowDef;
            Assert.IsTrue(windowSpecification.PartitionClause.ExpressionList.size() == 1);
            Assert.IsTrue(windowSpecification.PartitionClause.ExpressionList.getExpression(0).ToString().Equals("c", StringComparison.CurrentCultureIgnoreCase));

            TOrderByItem orderByItem = windowSpecification.orderBy.Items.getOrderByItem(0);
            Assert.IsTrue(orderByItem.ToString().Equals("d", StringComparison.CurrentCultureIgnoreCase));

            TWindowFrame windowFrame = windowSpecification.WindowFrame;
            Assert.IsTrue(windowFrame.WindowExpressionType == ELimitRowType.Rows);
            TWindowFrameBoundary start = windowFrame.StartBoundary;
            TWindowFrameBoundary end = windowFrame.EndBoundary;
            Assert.IsTrue(start.BoundaryType == EBoundaryType.ebtPreceding);
            Assert.IsTrue(start.BoundaryNumber.ToString().Equals("3", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(end.BoundaryType == EBoundaryType.ebtFollowing);
            Assert.IsTrue(end.BoundaryNumber.ToString().Equals("3", StringComparison.CurrentCultureIgnoreCase));
            //System.out.println(windowSpecification.toString());

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_fake);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.objectname);

        }

        [TestMethod]
        public void testWindowing4()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT a, SUM(b) OVER w\n" + "FROM T\n" + "WINDOW w AS (PARTITION BY c ORDER BY d ROWS UNBOUNDED PRECEDING)";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 2);

            TExpression expression = select.ResultColumnList.getResultColumn(1).Expr;
            Assert.IsTrue(expression.ExpressionType == EExpressionType.function_t);

            TFunctionCall functionCall = expression.FunctionCall;
            Assert.IsTrue(functionCall.FunctionName.ToString().Equals("SUM", StringComparison.CurrentCultureIgnoreCase));

            TWindowDef windowSpecification = functionCall.WindowDef;
            Assert.IsTrue(windowSpecification.Name.ToString().Equals("w", StringComparison.CurrentCultureIgnoreCase));
            //System.out.println(windowSpecification.toString());

            TWindowClause windowClause = select.WindowClause;
            Assert.IsTrue(windowClause.WindowDefs.Count == 1);
            TWindowDef windowDefinition = windowClause.WindowDefs[0];
            Assert.IsTrue(windowDefinition.Name.ToString().Equals("w", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(windowDefinition.PartitionClause.ExpressionList.getExpression(0).ToString().Equals("c", StringComparison.CurrentCultureIgnoreCase));

            TWindowFrame winframe = windowDefinition.WindowFrame;
            Assert.IsTrue(winframe.WindowExpressionType == ELimitRowType.Rows);
            Assert.IsTrue(winframe.StartBoundary.BoundaryType == EBoundaryType.ebtUnboundedPreceding);

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_fake);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.objectname);

        }

        [TestMethod]
        public void testUnionInSubquery()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT t3.col\n" + "FROM (\n" + "  SELECT a+b AS col\n" + "  FROM t1\n" + "  UNION ALL\n" + "  SELECT c-d AS col\n" + "  FROM t2\n" + ") t3;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 1);

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_fake);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.subquery);

            TSelectSqlStatement subquery = table.Subquery;
            Assert.IsTrue(subquery.CombinedQuery);
            TSelectSqlStatement left = subquery.LeftStmt;
            TSelectSqlStatement right = subquery.RightStmt;

            TResultColumn rs = left.ResultColumnList.getResultColumn(0);
            TExpression expr = rs.Expr;
            Assert.IsTrue(expr.ExpressionType == EExpressionType.arithmetic_plus_t);

            TAliasClause aliasClause = rs.AliasClause;
            // System.out.println(aliasClause.getAliasName().toString());
            Assert.IsTrue(aliasClause.AliasName.ToString().Equals("col", StringComparison.CurrentCultureIgnoreCase));

            rs = right.ResultColumnList.getResultColumn(0);
            expr = rs.Expr;
            Assert.IsTrue(expr.ExpressionType == EExpressionType.arithmetic_minus_t);


        }

        [TestMethod]
        public void testUnion1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT u.id, actions.date\n" + "FROM (\n" + "    SELECT av.uid AS uid \n" + "    FROM action_video av \n" + "    WHERE av.date = '2008-06-03'\n" + "    UNION ALL \n" + "    SELECT ac.uid AS uid \n" + "    FROM action_comment ac \n" + "    WHERE ac.date = '2008-06-03'\n" + " ) actions JOIN users u ON (u.id = actions.uid) ;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_table);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.subquery);
            TJoinItem joinItem = join.JoinItems.getJoinItem(0);
            Assert.IsTrue(joinItem.JoinType == EJoinType.join);
            Assert.IsTrue(String.Equals(joinItem.Table.FullName, "users", StringComparison.CurrentCultureIgnoreCase));

            TSelectSqlStatement subquery = table.Subquery;
            Assert.IsTrue(subquery.CombinedQuery);

            TSelectSqlStatement left = subquery.LeftStmt;
            TSelectSqlStatement right = subquery.RightStmt;

            TResultColumn rs = left.ResultColumnList.getResultColumn(0);
            TExpression expr = rs.Expr;
            Assert.IsTrue(expr.ExpressionType == EExpressionType.simple_object_name_t);
            Assert.IsTrue(expr.ToString().Equals("av.uid", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(rs.AliasClause.AliasName.ToString().Equals("uid", StringComparison.CurrentCultureIgnoreCase));

            rs = right.ResultColumnList.getResultColumn(0);
            expr = rs.Expr;
            Assert.IsTrue(expr.ExpressionType == EExpressionType.simple_object_name_t);
            Assert.IsTrue(expr.ToString().Equals("ac.uid", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(rs.AliasClause.AliasName.ToString().Equals("uid", StringComparison.CurrentCultureIgnoreCase));


        }

        [TestMethod]
        public void testUnion2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "FROM (\n" + "     FROM (\n" + "             FROM action_video av\n" + "             SELECT av.uid AS uid, av.id AS id, av.date AS date\n" + " \n" + "            UNION ALL\n" + " \n" + "             FROM action_comment ac\n" + "             SELECT ac.uid AS uid, ac.id AS id, ac.date AS date\n" + "     ) union_actions\n" + "     SELECT union_actions.uid, union_actions.id, union_actions.date\n" + "     CLUSTER BY union_actions.uid)  map;";
            Assert.IsTrue(sqlparser.parse() == 0);

            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.ssthiveFromQuery);
            THiveFromQuery fromQuery = (THiveFromQuery)sqlparser.sqlstatements.get(0);
            TTable table = fromQuery.tables.getTable(0);
            Assert.IsTrue(table.TableType == ETableSource.hiveFromQuery);
            Assert.IsTrue(table.AliasClause.AliasName.ToString().Equals("map", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(fromQuery.HiveBodyList.size() == 0);

            fromQuery = table.HiveFromQuery;
            TSelectSqlStatement select = (TSelectSqlStatement)fromQuery.HiveBodyList.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 3);
            THiveClusterBy clusterBy = select.HiveClusterBy;
            Assert.IsTrue(clusterBy.ExpressionList.getExpression(0).ToString().Equals("union_actions.uid", StringComparison.CurrentCultureIgnoreCase));

            table = fromQuery.tables.getTable(0);
            Assert.IsTrue(table.TableType == ETableSource.subquery);
            Assert.IsTrue(table.AliasClause.AliasName.ToString().Equals("union_actions", StringComparison.CurrentCultureIgnoreCase));
            select = table.Subquery;
            Assert.IsTrue(select.CombinedQuery);
            Assert.IsTrue(select.SetOperator == TSelectSqlStatement.setOperator_unionall);
            // hive from query: from...select will be translate into select statement in union operation
            TSelectSqlStatement left = select.LeftStmt;
            Assert.IsTrue(left.tables.getTable(0).TableName.ToString().Equals("action_video", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(left.tables.getTable(0).AliasClause.AliasName.ToString().Equals("av", StringComparison.CurrentCultureIgnoreCase));

            // hive from query: from...select will be translate into select statement in union operation
            TSelectSqlStatement right = select.RightStmt;
            Assert.IsTrue(right.tables.getTable(0).TableName.ToString().Equals("action_comment", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(right.tables.getTable(0).AliasClause.AliasName.ToString().Equals("ac", StringComparison.CurrentCultureIgnoreCase));

            //Assert.IsTrue(fromQuery.getHiveBodyList().size() == 0);

        }

        [TestMethod]
        public  void testGroupBy()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "select * from t1 group by c1 with rollup";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.GroupByClause.ToString().Equals("group by c1", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public  void testFromClause()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "select * from t1 join t2 on t1.c1 = t2.c2";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            //System.out.println(select.joins);
            Assert.IsTrue(select.joins.ToString().Equals("t1 join t2 on t1.c1 = t2.c2", StringComparison.CurrentCultureIgnoreCase));
            //Assert.IsTrue(select.getGroupByClause().toString().equalsIgnoreCase("group by c1"));

        }

        [TestMethod]
        public void testShow()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SHOW FORMATTED INDEX ON table03;";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveShow show = (THiveShow)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(show.ShowType == EHiveShowType.stIndexes);
            Assert.IsTrue(show.ShowOptions == EHiveDescOption.doFormatted);
            Assert.IsTrue(show.TableName.ToString().Equals("table03", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testSet1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "set system:xxx=5;";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveSet set = (THiveSet)sqlparser.sqlstatements.get(0);
            TExpression expression = set.Expr;
            Assert.IsTrue(expression.LeftOperand.ToString().Equals("system:xxx", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(expression.RightOperand.ToString().Equals("5", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testSet2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "set system:yyy=${system:xxx};";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveSet set = (THiveSet)sqlparser.sqlstatements.get(0);
            TExpression expression = set.Expr;
            Assert.IsTrue(expression.LeftOperand.ToString().Equals("system:yyy", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(expression.RightOperand.ToString().Equals("${system:xxx}", StringComparison.CurrentCultureIgnoreCase));
            TExpression right = expression.RightOperand;
            Assert.IsTrue(right.ExpressionType == EExpressionType.hive_variable_t);
            THiveVariable v = right.Hive_variable;
            Assert.IsTrue(v.VarName.ToString().Equals("system", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(v.VarProperty.ToString().Equals("xxx", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testSet3()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "set c=${hiveconf:${hiveconf:b}};";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveSet set = (THiveSet)sqlparser.sqlstatements.get(0);
            TExpression expression = set.Expr;
            Assert.IsTrue(expression.LeftOperand.ToString().Equals("c", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(expression.RightOperand.ToString().Equals("${hiveconf:${hiveconf:b}}", StringComparison.CurrentCultureIgnoreCase));
            TExpression right = expression.RightOperand;
            Assert.IsTrue(right.ExpressionType == EExpressionType.hive_variable_t);
            THiveVariable v = right.Hive_variable;
            Assert.IsTrue(v.VarName.ToString().Equals("hiveconf", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(v.VarProperty == null);
            Assert.IsTrue(v.NestedVar.ToString().Equals("${hiveconf:b}", StringComparison.CurrentCultureIgnoreCase));
            v = v.NestedVar;
            Assert.IsTrue(v.VarName.ToString().Equals("hiveconf", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(v.VarProperty.ToString().Equals("b", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testSelect1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT page_views.*\n" + "FROM page_views\n" + "WHERE page_views.date >= '2008-03-01' AND page_views.date <= '2008-03-31'";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 1);
            TResultColumn resultColumn = select.ResultColumnList.getResultColumn(0);
            TExpression expr = resultColumn.Expr;
            Assert.IsTrue(expr.ExpressionType == EExpressionType.simple_object_name_t);
            TObjectName objectName = expr.ObjectOperand;
            Assert.IsTrue(objectName.ToString().Equals("page_views.*", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_fake);
            Assert.IsTrue(join.Table.ToString().Equals("page_views", StringComparison.CurrentCultureIgnoreCase));

            TWhereClause whereClause = select.WhereClause;
            TExpression condition = whereClause.Condition;
            Assert.IsTrue(condition.ExpressionType == EExpressionType.logical_and_t);
            Assert.IsTrue(condition.ToString().Equals("page_views.date >= '2008-03-01' AND page_views.date <= '2008-03-31'", StringComparison.CurrentCultureIgnoreCase));
            TExpression left = condition.LeftOperand;
            Assert.IsTrue(left.ExpressionType == EExpressionType.simple_comparison_t);
            Assert.IsTrue(left.ComparisonOperator.ToString().Equals(">=", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(left.ComparisonOperator.tokencode == TBaseType.great_equal);
            TExpression right = condition.RightOperand;
            //System.out.println(left.getComparisonOperator().tokencode);
        }


        [TestMethod]
        public void testGroupBy2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT col1 FROM t1 GROUP BY col1 HAVING SUM(col2) > 10;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 1);
            TGroupBy groupBy = select.GroupByClause;
            Assert.IsTrue(groupBy.Items.size() == 1);
            TGroupByItem groupByItem = groupBy.Items.getGroupByItem(0);
            Assert.IsTrue(groupByItem.Expr.ToString().Equals("col1", StringComparison.CurrentCultureIgnoreCase));
            TExpression havingCondition = groupBy.HavingClause;
            Assert.IsTrue(havingCondition.ExpressionType == EExpressionType.simple_comparison_t);
            Assert.IsTrue(havingCondition.ToString().Equals("SUM(col2) > 10", StringComparison.CurrentCultureIgnoreCase));
            TExpression left = havingCondition.LeftOperand;
            TExpression right = havingCondition.RightOperand;
            Assert.IsTrue(left.ExpressionType == EExpressionType.function_t);
            TFunctionCall functionCall = left.FunctionCall;
            Assert.IsTrue(functionCall.FunctionName.ToString().Equals("SUM", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(functionCall.Args.getExpression(0).ToString().Equals("col2", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(right.ExpressionType == EExpressionType.simple_constant_t);
        }

        [TestMethod]
        public void testSubqueryInFromClause()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT col1 FROM (SELECT col11, SUM(col2) AS col2sum FROM t1 GROUP BY col1) t2 WHERE t2.col2sum > 10";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 1);

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_fake);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.subquery);

            TSelectSqlStatement subquery = table.Subquery;
            Assert.IsTrue(subquery.ResultColumnList.getResultColumn(0).ToString().Equals("col11", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testLimitClause()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT * FROM t1 LIMIT 5;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 1);

            TLimitClause limitClause = select.LimitClause;
            //System.out.println(limitClause.getOffset().toString());
            Assert.IsTrue(limitClause.Offset.ToString().Equals("5", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testSortBy()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT * FROM sales SORT BY amount DESC LIMIT 5;;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 1);

            THiveSortBy sortBy = select.SortBy;
            Assert.IsTrue(sortBy.Items.getOrderByItem(0).SortKey.ToString().Equals("amount", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sortBy.Items.getOrderByItem(0).SortType == TBaseType.srtDesc);

        }

        [TestMethod]
        public  void testAlias1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT TRANSFORM(stuff) \n" + "  USING 'script'\n" + "  AS (thing1 INT, thing2 INT) from b;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            THiveTransformClause transformClause = select.TransformClause;
            Assert.IsTrue(transformClause.TransformType == THiveTransformClause.ETransformType.ettSelect);
            Assert.IsTrue(transformClause.UsingString.ToString().Equals("'script'", StringComparison.CurrentCultureIgnoreCase));

            TAliasClause aliasClause = transformClause.AliasClause;
            Assert.IsTrue(aliasClause.ColumnNameTypeList.size() == 2);
            TColumnDefinition cd1 = aliasClause.ColumnNameTypeList.getColumn(0);
            Assert.IsTrue(cd1.ColumnName.ToString().Equals("thing1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(cd1.Datatype.DataType == EDataType.int_t);
            TColumnDefinition cd2 = aliasClause.ColumnNameTypeList.getColumn(1);
            Assert.IsTrue(cd2.ColumnName.ToString().Equals("thing2", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(cd2.Datatype.DataType == EDataType.int_t);

        }
        [TestMethod]
        public void testCreateTable1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "create table Addresses (\n" + "  name string,\n" + "  street string,\n" + "  city string,\n" + "  state string,\n" + "  zip int\n" + ") stored as orc tblproperties (\"orc.compress\"=\"NONE\");";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createTable.TableName.ToString().Equals("Addresses", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createTable.ColumnList.size() == 5);
            TColumnDefinition cd = createTable.ColumnList.getColumn(0);
            Assert.IsTrue(cd.ColumnName.ToString().Equals("name", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(cd.Datatype.DataType == EDataType.string_t);
            cd = createTable.ColumnList.getColumn(4);
            Assert.IsTrue(cd.ColumnName.ToString().Equals("zip", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(cd.Datatype.DataType == EDataType.int_t);

            THiveTableFileFormat tff = createTable.HiveTableFileFormat;
            Assert.IsTrue(tff.FileFormat == EHiveStoredFileFormat.sffFILEFORMAT_GENERIC);
            Assert.IsTrue(tff.GenericSpec.ToString().Equals("orc", StringComparison.CurrentCultureIgnoreCase));

            THiveTableProperties tp = createTable.HiveTableProperties;
            Assert.IsTrue(tp.TableProperties.Count == 1);
            THiveKeyValueProperty kv = tp.TableProperties[0];
            Assert.IsTrue(kv.KeyString.ToString().Equals("\"orc.compress\"", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(kv.ValueString.ToString().Equals("\"NONE\"", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testCreateTable2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "CREATE TABLE union_test(foo UNIONTYPE<int, double, array<string>, struct<a:int,b:string>>);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createTable.TableName.ToString().Equals("union_test", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createTable.ColumnList.size() == 1);
            TColumnDefinition cd = createTable.ColumnList.getColumn(0);
            Assert.IsTrue(cd.ColumnName.ToString().Equals("foo", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(cd.Datatype.DataType == EDataType.unionType_t);

            Assert.IsTrue(cd.Datatype.ColTypeList.Count == 4);
            TTypeName dataType = cd.Datatype.ColTypeList[0];
            Assert.IsTrue(dataType.DataType == EDataType.int_t);

            dataType = cd.Datatype.ColTypeList[1];
            Assert.IsTrue(dataType.DataType == EDataType.double_t);

            dataType = cd.Datatype.ColTypeList[2];
            Assert.IsTrue(dataType.DataType == EDataType.listType_t);
            Assert.IsTrue(dataType.TypeOfList.DataType == EDataType.string_t);

            dataType = cd.Datatype.ColTypeList[3];
            Assert.IsTrue(dataType.DataType == EDataType.structType_t);
            Assert.IsTrue(dataType.ColumnDefList.size() == 2);
            TColumnDefinition cd1 = dataType.ColumnDefList.getColumn(0);
            Assert.IsTrue(cd1.ColumnName.ToString().Equals("a", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(cd1.Datatype.DataType == EDataType.int_t);
            TColumnDefinition cd2 = dataType.ColumnDefList.getColumn(1);
            Assert.IsTrue(cd2.ColumnName.ToString().Equals("b", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(cd2.Datatype.DataType == EDataType.string_t);

        }

        [TestMethod]
        public void testCreateTable3()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "CREATE TABLE complex_json (\n" + "  DocId string,\n" + "  User struct<Id:int,\n" + "              Username:string,\n" + "              Name: string,\n" + "              ShippingAddress:struct<Address1:string,\n" + "                                     Address2:string,\n" + "                                     City:string,\n" + "                                     State:string>,\n" + "              Orders:array<struct<ItemId:int,\n" + "                                  OrderDate:string>>>\n" + ")\n" + "ROW FORMAT SERDE 'org.openx.data.jsonserde.JsonSerDe';";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createTable.TableName.ToString().Equals("complex_json", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createTable.ColumnList.size() == 2);
            TColumnDefinition cd = createTable.ColumnList.getColumn(0);
            Assert.IsTrue(cd.ColumnName.ToString().Equals("DocId", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(cd.Datatype.DataType == EDataType.string_t);

            cd = createTable.ColumnList.getColumn(1);
            Assert.IsTrue(cd.ColumnName.ToString().Equals("User", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(cd.Datatype.DataType == EDataType.structType_t);
            Assert.IsTrue(cd.Datatype.ColumnDefList.size() == 5);

            THiveRowFormat rowFormat = createTable.HiveRowFormat;
            Assert.IsTrue(rowFormat.RowFormatType == THiveRowFormat.ERowFormatType.serde);
            Assert.IsTrue(rowFormat.RowFormatName.ToString().Equals("'org.openx.data.jsonserde.JsonSerDe'", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testCreateTable4()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "CREATE TABLE page_view(viewTime INT, userid BIGINT,\n" + "page_url STRING, referrer_url STRING,\n" + "ip STRING COMMENT 'IP Address of the User')\n" + "COMMENT 'This is the page view table'\n" + "PARTITIONED BY(dt STRING, country STRING)\n" + "ROW FORMAT DELIMITED\n" + "FIELDS TERMINATED BY '1'\n" + "STORED AS SEQUENCEFILE;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createTable.TableName.ToString().Equals("page_view", StringComparison.CurrentCultureIgnoreCase));
            TColumnDefinition cd = createTable.ColumnList.getColumn(4);
            Assert.IsTrue(cd.ColumnName.ToString().Equals("ip", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(cd.Datatype.DataType == EDataType.string_t);
            Assert.IsTrue(cd.Comment.ToString().Equals("'IP Address of the User'", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(createTable.TableComment.ToString().Equals("'This is the page view table'", StringComparison.CurrentCultureIgnoreCase));
            THiveTablePartition tp = createTable.HiveTablePartition;
            Assert.IsTrue(tp.ColumnDefList.size() == 2);
            Assert.IsTrue(tp.ColumnDefList.getColumn(0).ColumnName.ToString().Equals("dt", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(tp.ColumnDefList.getColumn(1).Datatype.DataType == EDataType.string_t);

            THiveRowFormat rowFormat = createTable.HiveRowFormat;
            Assert.IsTrue(rowFormat.RowFormatType == THiveRowFormat.ERowFormatType.delimited);
            Assert.IsTrue(rowFormat.TableRowFormatFieldIdentifier.TerminateString.ToString().Equals("'1'", StringComparison.CurrentCultureIgnoreCase));

            THiveTableFileFormat tff = createTable.HiveTableFileFormat;
            Assert.IsTrue(tff.FileFormat == EHiveStoredFileFormat.sffTBLSEQUENCEFILE);

        }
        [TestMethod]
        public void testCreateIndex1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "CREATE INDEX table02_index ON TABLE table02 (column3) AS 'COMPACT' WITH DEFERRED REBUILD;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateIndexSqlStatement createIndex = (TCreateIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createIndex.IndexName.ToString().Equals("table02_index", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndex.TableName.ToString().Equals("table02", StringComparison.CurrentCultureIgnoreCase));
            TOrderByItemList viewColumns = createIndex.ColumnNameList;
            Assert.IsTrue(viewColumns.size() == 1);
            Assert.IsTrue(viewColumns.getOrderByItem(0).SortKey.ToString().Equals("column3", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndex.AsTypeName.ToString().Equals("'COMPACT'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndex.DeferredRebuildIndex);
        }
        [TestMethod]
        public void testCreateIndex2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "CREATE INDEX table08_index ON TABLE table08 (column9) AS 'COMPACT' TBLPROPERTIES (\"prop3\"=\"value3\", \"prop4\"=\"value4\");";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateIndexSqlStatement createIndex = (TCreateIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createIndex.IndexName.ToString().Equals("table08_index", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndex.TableName.ToString().Equals("table08", StringComparison.CurrentCultureIgnoreCase));
            TOrderByItemList viewColumns = createIndex.ColumnNameList;
            Assert.IsTrue(viewColumns.size() == 1);
            Assert.IsTrue(viewColumns.getOrderByItem(0).SortKey.ToString().Equals("column9", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndex.AsTypeName.ToString().Equals("'COMPACT'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(!createIndex.DeferredRebuildIndex);

            THiveIndexProperties indexProperties = createIndex.IndexProperties;
            Assert.IsTrue(indexProperties == null);

            THiveTableProperties tableProperties = createIndex.TableProperties;
            Assert.IsTrue(tableProperties.TableProperties.Count == 2);
        }

        [TestMethod]
        public void testCreateFunction1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "CREATE TEMPORARY FUNCTION function_name AS 'class_name';";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveCreateFunction createFunction = (THiveCreateFunction)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createFunction.FunctionName.ToString().Equals("function_name", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createFunction.AsName.ToString().Equals("'class_name'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testAnalyze1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "ANALYZE TABLE Table1 PARTITION(ds='2008-04-09', hr) COMPUTE STATISTICS noscan;";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveAnalyzeTable analyzeTable = (THiveAnalyzeTable)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(String.Equals(analyzeTable.Table.FullName,"Table1", StringComparison.CurrentCultureIgnoreCase));
            TTable table = analyzeTable.Table;
            TPartitionExtensionClause partition = table.PartitionExtensionClause;
            Assert.IsTrue(partition.KeyValues.size() == 2);
            Assert.IsTrue(partition.KeyValues.getExpression(0).LeftOperand.ToString().Equals("ds", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(partition.KeyValues.getExpression(0).RightOperand.ToString().Equals("'2008-04-09'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(partition.KeyValues.getExpression(1).ExpressionType == EExpressionType.simple_object_name_t);
            Assert.IsTrue(partition.KeyValues.getExpression(1).ObjectOperand.ToString().Equals("hr", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testSerde()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "alter table decimal_1 set serde 'org.apache.hadoop.hive.serde2.lazy.LazySimpleSerDe';";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTable = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(alterTable.TableName.ToString().Equals("decimal_1", StringComparison.CurrentCultureIgnoreCase));

            TAlterTableOption alterTableOption = alterTable.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(alterTableOption.OptionType == EAlterTableOptionType.serde);
            Assert.IsTrue(alterTableOption.SerdeName.ToString().Equals("'org.apache.hadoop.hive.serde2.lazy.LazySimpleSerDe'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testArchive()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "ALTER TABLE srcpart UNARCHIVE PARTITION(ds='2008-04-08', hr='12');";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTable = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(alterTable.TableName.ToString().Equals("srcpart", StringComparison.CurrentCultureIgnoreCase));

            TAlterTableOption alterTableOption = alterTable.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(alterTableOption.OptionType == EAlterTableOptionType.unArchive);
            Assert.IsTrue(alterTableOption.PartitionSpecList.Count == 1);
            TPartitionExtensionClause partitionSpec = alterTableOption.PartitionSpecList[0];
            Assert.IsTrue(partitionSpec.KeyValues.getExpression(0).ToString().Equals("ds='2008-04-08'", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testCreateView1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "CREATE VIEW V4 AS\n" + "  SELECT src1.key, src2.value as value1, src3.value as value2\n" + "  FROM V1 src1 JOIN V2 src2 on src1.key = src2.key JOIN src src3 ON src2.key = src3.key;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateViewSqlStatement createView = (TCreateViewSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createView.ViewName.ToString().Equals("V4", StringComparison.CurrentCultureIgnoreCase));

            TSelectSqlStatement select = createView.Subquery;
            Assert.IsTrue(select.ResultColumnList.size() == 3);
        }

        [TestMethod]
        public  void testDescribe1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "DESCRIBE EXTENDED TABLE1;";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveDescribe describe = (THiveDescribe)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(describe.DescribleType == EHiveDescribleType.dtTablePartition);
            Assert.IsTrue(describe.DescOption == EHiveDescOption.doExtended);
            THiveDescTablePartition tablePartition = describe.TablePartition;
            Assert.IsTrue(tablePartition.Partition == null);
            Assert.IsTrue(tablePartition.DescTabType.ToString().Equals("TABLE1", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public  void testDescribe2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "DESCRIBE EXTENDED TABLE1 PARTITION(ds='2008-04-09', hr=11);";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveDescribe describe = (THiveDescribe)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(describe.DescribleType == EHiveDescribleType.dtTablePartition);
            Assert.IsTrue(describe.DescOption == EHiveDescOption.doExtended);
            THiveDescTablePartition tablePartition = describe.TablePartition;
            TPartitionExtensionClause partition = tablePartition.Partition;
            Assert.IsTrue(partition.KeyValues.size() == 2);
            Assert.IsTrue(partition.KeyValues.getExpression(0).LeftOperand.ToString().Equals("ds", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(partition.KeyValues.getExpression(0).RightOperand.ToString().Equals("'2008-04-09'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(partition.KeyValues.getExpression(1).LeftOperand.ToString().Equals("hr", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(partition.KeyValues.getExpression(1).RightOperand.ToString().Equals("11", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(tablePartition.DescTabType.ToString().Equals("TABLE1", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public  void testDropIndex1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "DROP INDEX table02_index ON table02;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TDropIndexSqlStatement dropIndex = (TDropIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(dropIndex.IndexName.ToString().Equals("table02_index", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(dropIndex.TableName.ToString().Equals("table02", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testExplain1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "EXPLAIN\n" + "FROM src INSERT OVERWRITE TABLE dest_g1 SELECT src.key, sum(substr(src.value,4)) GROUP BY src.key;";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveExplain explain = (THiveExplain)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(explain.Stmt.sqlstatementtype == ESqlStatementType.ssthiveFromQuery);
            THiveFromQuery fromQuery = (THiveFromQuery)explain.Stmt;
            Assert.IsTrue(fromQuery.tables.getTable(0).TableName.ToString().Equals("src", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(fromQuery.HiveBodyList.size() == 1);
            TInsertSqlStatement insert = (TInsertSqlStatement)fromQuery.HiveBodyList.get(0);
            Assert.IsTrue(insert.HiveInsertType == EHiveInsertType.overwriteTable);
            Assert.IsTrue(insert.TargetTable.TableName.ToString().Equals("dest_g1", StringComparison.CurrentCultureIgnoreCase));
            TSelectSqlStatement select = (TSelectSqlStatement)insert.SubQuery;
            TGroupBy groupBy = select.GroupByClause;
            Assert.IsTrue(groupBy.Items.getGroupByItem(0).Expr.ToString().Equals("src.key", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testExplain2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "EXPLAIN DEPENDENCY\n" + "  SELECT key, count(1) FROM srcpart WHERE ds IS NOT NULL GROUP BY key;";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveExplain explain = (THiveExplain)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(explain.Stmt.sqlstatementtype == ESqlStatementType.sstselect);
            TSelectSqlStatement select = (TSelectSqlStatement)explain.Stmt;
            Assert.IsTrue(select.ResultColumnList.size() == 2);
            Assert.IsTrue(select.tables.getTable(0).TableName.ToString().Equals("srcpart", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(select.WhereClause.Condition.ToString().Equals("ds IS NOT NULL", StringComparison.CurrentCultureIgnoreCase));
            TGroupBy groupBy = select.GroupByClause;
            Assert.IsTrue(groupBy.Items.getGroupByItem(0).Expr.ToString().Equals("key", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public  void testExportTable1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "export table department to 'hdfs_exports_location/department';";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveExportTable export = (THiveExportTable)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(export.Table.ToString().Equals("department", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(export.Path.ToString().Equals("'hdfs_exports_location/department'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public  void testExportTable2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "export table employee partition (emp_country=\"in\", emp_state=\"ka\") to 'hdfs_exports_location/employee';";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveExportTable export = (THiveExportTable)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(export.Table.TableName.ToString().Equals("employee", StringComparison.CurrentCultureIgnoreCase));
            TPartitionExtensionClause p = export.Table.PartitionExtensionClause;
            Assert.IsTrue(p.KeyValues.size() == 2);
            Assert.IsTrue(p.KeyValues.getExpression(0).LeftOperand.ToString().Equals("emp_country", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(p.KeyValues.getExpression(0).RightOperand.ToString().Equals("\"in\"", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(p.KeyValues.getExpression(1).LeftOperand.ToString().Equals("emp_state", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(p.KeyValues.getExpression(1).RightOperand.ToString().Equals("\"ka\"", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(export.Path.ToString().Equals("'hdfs_exports_location/employee'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testFieldExpression1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT v['code'], COUNT(1) FROM www_access GROUP BY v['code'];";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TExpression expr = select.ResultColumnList.getResultColumn(0).Expr;
            Assert.IsTrue(expr.ExpressionType == EExpressionType.array_access_expr_t);
            Assert.IsTrue(expr.ToString().Equals("v['code']", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(expr.LeftOperand.ExpressionType == EExpressionType.simple_object_name_t);
            Assert.IsTrue(expr.LeftOperand.ToString().Equals("v", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(expr.RightOperand.ExpressionType == EExpressionType.simple_constant_t);
            Assert.IsTrue(expr.RightOperand.ToString().Equals("'code'", StringComparison.CurrentCultureIgnoreCase));


        }
        [TestMethod]
        public void testFromSelect1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "FROM pv_users \n" + "INSERT OVERWRITE TABLE pv_gender_sum\n" + "  SELECT pv_users.gender, count(DISTINCT pv_users.userid) \n" + "  GROUP BY pv_users.gender \n" + "INSERT OVERWRITE DIRECTORY '/user/facebook/tmp/pv_age_sum'\n" + "  SELECT pv_users.age, count(DISTINCT pv_users.userid) \n" + "  GROUP BY pv_users.age; \n" + "  ";
            Assert.IsTrue(sqlparser.parse() == 0);

            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.ssthiveFromQuery);

            THiveFromQuery select = (THiveFromQuery)sqlparser.sqlstatements.get(0);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_fake);
            Assert.IsTrue(String.Equals(join.Table.FullName,"pv_users", StringComparison.CurrentCultureIgnoreCase));

            //quick way to access table
            TTable t = select.tables.getTable(0);
            Assert.IsTrue(String.Equals(t.FullName,"pv_users", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(select.HiveBodyList.size() == 2);

            Assert.IsTrue(select.HiveBodyList.get(0).sqlstatementtype == ESqlStatementType.sstinsert);
            TInsertSqlStatement insert1 = (TInsertSqlStatement)select.HiveBodyList.get(0);
            Assert.IsTrue(insert1.HiveInsertType == EHiveInsertType.overwriteTable);
            Assert.IsTrue(String.Equals(insert1.TargetTable.FullName,"pv_gender_sum", StringComparison.CurrentCultureIgnoreCase));
            TSelectSqlStatement select1 = insert1.SubQuery;
            Assert.IsTrue(select1.GroupByClause.Items.getGroupByItem(0).ToString().Equals("pv_users.gender", StringComparison.CurrentCultureIgnoreCase));

            insert1 = (TInsertSqlStatement)select.HiveBodyList.get(1);
            Assert.IsTrue(insert1.HiveInsertType == EHiveInsertType.overwriteDirectory);
            // System.out.println(insert1.getDirectoryName().toString());
            //Assert.IsTrue(insert1.getDirectoryName().getEndToken() != null);
            Assert.IsTrue(insert1.DirectoryName.ToString().Equals("'/user/facebook/tmp/pv_age_sum'", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testFromSelect2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "  FROM (\n" + "    FROM pv_users\n" + "    MAP ( pv_users.userid, pv_users.date )\n" + "    USING 'map_script'\n" + "    AS c1, c2, c3\n" + "    DISTRIBUTE BY c2\n" + "    SORT BY c2, c1) map_output\n" + "  INSERT OVERWRITE TABLE pv_users_reduced\n" + "    REDUCE ( map_output.c1, map_output.c2, map_output.c3 )\n" + "    USING 'reduce_script'\n" + "    AS date, count;";

            Assert.IsTrue(sqlparser.parse() == 0);

            THiveFromQuery select = (THiveFromQuery)sqlparser.sqlstatements.get(0);

            Assert.IsTrue(select.HiveBodyList.size() == 1);
            TCustomSqlStatement sql = select.HiveBodyList.get(0);
            Assert.IsTrue(sql.sqlstatementtype == ESqlStatementType.sstinsert);
            TInsertSqlStatement insert = (TInsertSqlStatement)sql;
            Assert.IsTrue(insert.HiveInsertType == EHiveInsertType.overwriteTable);
            Assert.IsTrue(insert.tables.getTable(0).ToString().Equals("pv_users_reduced", StringComparison.CurrentCultureIgnoreCase));

            TSelectSqlStatement reduce = insert.SubQuery;
            Assert.IsTrue(reduce.TransformClause != null);
            THiveTransformClause transformClause = reduce.TransformClause;
            Assert.IsTrue(transformClause.TransformType == THiveTransformClause.ETransformType.ettReduce);
            Assert.IsTrue(transformClause.ExpressionList.size() == 3);
            Assert.IsTrue(transformClause.ExpressionList.getExpression(0).ToString().Equals("map_output.c1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(transformClause.ExpressionList.getExpression(1).ToString().Equals("map_output.c2", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(transformClause.ExpressionList.getExpression(2).ToString().Equals("map_output.c3", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(transformClause.UsingString.ToString().Equals("'reduce_script'", StringComparison.CurrentCultureIgnoreCase));

            TAliasClause aliasClause = transformClause.AliasClause;
            Assert.IsTrue(aliasClause.Columns.size() == 2);
            Assert.IsTrue(aliasClause.Columns.getObjectName(0).ToString().Equals("date", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(aliasClause.Columns.getObjectName(1).ToString().Equals("count", StringComparison.CurrentCultureIgnoreCase));


            TTable table = select.tables.getTable(0);
            Assert.IsTrue(table.TableType == ETableSource.hiveFromQuery);
            THiveFromQuery subquery = table.HiveFromQuery;
            Assert.IsTrue(subquery.HiveBodyList.size() == 1);
            Assert.IsTrue(table.AliasClause.ToString().Equals("map_output", StringComparison.CurrentCultureIgnoreCase));

            sql = subquery.HiveBodyList.get(0);
            Assert.IsTrue(sql.sqlstatementtype == ESqlStatementType.sstselect);
            TSelectSqlStatement select1 = (TSelectSqlStatement)sql;
            Assert.IsTrue(select1.TransformClause != null);
            transformClause = select1.TransformClause;
            Assert.IsTrue(transformClause.TransformType == THiveTransformClause.ETransformType.ettMap);

            Assert.IsTrue(transformClause.ExpressionList.size() == 2);
            Assert.IsTrue(transformClause.ExpressionList.getExpression(0).ToString().Equals("pv_users.userid", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(transformClause.ExpressionList.getExpression(1).ToString().Equals("pv_users.date", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(transformClause.UsingString.ToString().Equals("'map_script'", StringComparison.CurrentCultureIgnoreCase));

            aliasClause = transformClause.AliasClause;
            Assert.IsTrue(aliasClause.Columns.size() == 3);
            Assert.IsTrue(aliasClause.Columns.getObjectName(0).ToString().Equals("c1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(aliasClause.Columns.getObjectName(1).ToString().Equals("c2", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(aliasClause.Columns.getObjectName(2).ToString().Equals("c3", StringComparison.CurrentCultureIgnoreCase));

            THiveDistributeBy distributeBy = select1.DistributeBy;
            Assert.IsTrue(distributeBy.ExpressionList.size() == 1);
            Assert.IsTrue(distributeBy.ExpressionList.getExpression(0).ToString().Equals("c2", StringComparison.CurrentCultureIgnoreCase));

            THiveSortBy sortBy = select1.SortBy;
            Assert.IsTrue(sortBy.Items.size() == 2);
            Assert.IsTrue(sortBy.Items.getOrderByItem(0).SortKey.ToString().Equals("c2", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sortBy.Items.getOrderByItem(0).SortType == TBaseType.srtNone);
            Assert.IsTrue(sortBy.Items.getOrderByItem(1).SortKey.ToString().Equals("c1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sortBy.Items.getOrderByItem(0).SortType == TBaseType.srtNone);



            TTable table1 = subquery.tables.getTable(0);
            Assert.IsTrue(table1.TableName.ToString().Equals("pv_users", StringComparison.CurrentCultureIgnoreCase));

            //select = subquery.getHiveBodyList().get(0);

        }

        [TestMethod]
        public void testFromSelect3()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = " FROM (\n" + "    FROM pv_users\n" + "    MAP pv_users.userid, pv_users.date\n" + "    USING 'map_script'\n" + "    AS dt, uid\n" + "    CLUSTER BY dt) map_output\n" + "  INSERT OVERWRITE TABLE pv_users_reduced\n" + "    REDUCE map_output.dt, map_output.uid\n" + "    USING 'reduce_script'\n" + "    AS date, count;\n";

            Assert.IsTrue(sqlparser.parse() == 0);

            THiveFromQuery select = (THiveFromQuery)sqlparser.sqlstatements.get(0);
            TTable table = select.tables.getTable(0);
            Assert.IsTrue(table.AliasClause.ToString().Equals("map_output", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(select.HiveBodyList.size() == 1);
            TCustomSqlStatement sql = select.HiveBodyList.get(0);
            Assert.IsTrue(sql.sqlstatementtype == ESqlStatementType.sstinsert);

            TInsertSqlStatement insert = (TInsertSqlStatement)sql;
            Assert.IsTrue(insert.HiveInsertType == EHiveInsertType.overwriteTable);
            Assert.IsTrue(insert.tables.getTable(0).ToString().Equals("pv_users_reduced", StringComparison.CurrentCultureIgnoreCase));
            TSelectSqlStatement subquery = insert.SubQuery;

            Assert.IsTrue(subquery.TransformClause != null);

            THiveTransformClause transformClause = subquery.TransformClause;
            Assert.IsTrue(transformClause.TransformType == THiveTransformClause.ETransformType.ettReduce);
            Assert.IsTrue(transformClause.ExpressionList.size() == 2);
            Assert.IsTrue(transformClause.ExpressionList.getExpression(0).ToString().Equals("map_output.dt", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(transformClause.ExpressionList.getExpression(1).ToString().Equals("map_output.uid", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(transformClause.UsingString.ToString().Equals("'reduce_script'", StringComparison.CurrentCultureIgnoreCase));

            TAliasClause aliasClause = transformClause.AliasClause;
            Assert.IsTrue(aliasClause.Columns.size() == 2);
            Assert.IsTrue(aliasClause.Columns.getObjectName(0).ToString().Equals("date", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(aliasClause.Columns.getObjectName(1).ToString().Equals("count", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testFromSelect4()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "FROM (\n" + "    FROM src\n" + "    SELECT TRANSFORM(src.KEY, src.value) ROW FORMAT SERDE 'org.apache.hadoop.hive.contrib.serde2.TypedBytesSerDe'\n" + "    USING '/bin/cat'\n" + "    AS (tkey, tvalue) ROW FORMAT SERDE 'org.apache.hadoop.hive.contrib.serde2.TypedBytesSerDe'\n" + "    RECORDREADER 'org.apache.hadoop.hive.ql.exec.TypedBytesRecordReader'\n" + "  ) tmap\n" + "  INSERT OVERWRITE TABLE dest1 SELECT tkey, tvalue;";

            Assert.IsTrue(sqlparser.parse() == 0);

            THiveFromQuery select = (THiveFromQuery)sqlparser.sqlstatements.get(0);

            TTable table = select.tables.getTable(0);
            Assert.IsTrue(table.AliasClause.ToString().Equals("tmap", StringComparison.CurrentCultureIgnoreCase));
            TInsertSqlStatement insert = (TInsertSqlStatement)select.HiveBodyList.get(0);
            Assert.IsTrue(insert.HiveInsertType == EHiveInsertType.overwriteTable);
            Assert.IsTrue(insert.tables.getTable(0).ToString().Equals("dest1", StringComparison.CurrentCultureIgnoreCase));
            TSelectSqlStatement subquery = insert.SubQuery;
            Assert.IsTrue(subquery.ResultColumnList.size() == 2);
            Assert.IsTrue(subquery.ResultColumnList.getResultColumn(0).ToString().Equals("tkey", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(subquery.ResultColumnList.getResultColumn(1).ToString().Equals("tvalue", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(table.TableType == ETableSource.hiveFromQuery);
            THiveFromQuery subquery1 = table.HiveFromQuery;
            Assert.IsTrue(subquery1.tables.getTable(0).ToString().Equals("src", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(subquery1.HiveBodyList.size() == 1);

            TSelectSqlStatement select1 = (TSelectSqlStatement)subquery1.HiveBodyList.get(0);
            Assert.IsTrue(select1.TransformClause != null);
            THiveTransformClause transformClause = select1.TransformClause;
            Assert.IsTrue(transformClause.TransformType == THiveTransformClause.ETransformType.ettSelect);
            Assert.IsTrue(transformClause.ExpressionList.size() == 2);
            Assert.IsTrue(transformClause.ExpressionList.getExpression(0).ToString().Equals("src.KEY", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(transformClause.ExpressionList.getExpression(1).ToString().Equals("src.value", StringComparison.CurrentCultureIgnoreCase));

            THiveRowFormat inrf = transformClause.InRowFormat;
            Assert.IsTrue(inrf.RowFormatType == THiveRowFormat.ERowFormatType.serde);
            Assert.IsTrue(inrf.RowFormatName.ToString().Equals("'org.apache.hadoop.hive.contrib.serde2.TypedBytesSerDe'", StringComparison.CurrentCultureIgnoreCase));

            THiveRowFormat outrf = transformClause.OutRowFormat;
            Assert.IsTrue(outrf.RowFormatType == THiveRowFormat.ERowFormatType.serde);
            Assert.IsTrue(outrf.RowFormatName.ToString().Equals("'org.apache.hadoop.hive.contrib.serde2.TypedBytesSerDe'", StringComparison.CurrentCultureIgnoreCase));

            THiveRecordReader outrr = transformClause.OutRecordReader;
            Assert.IsTrue(outrr.StringLiteral.ToString().Equals("'org.apache.hadoop.hive.ql.exec.TypedBytesRecordReader'", StringComparison.CurrentCultureIgnoreCase));

            TAliasClause aliasClause = transformClause.AliasClause;
            Assert.IsTrue(aliasClause.Columns.size() == 2);
            Assert.IsTrue(aliasClause.Columns.getObjectName(0).ToString().Equals("tkey", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(transformClause.UsingString.ToString().Equals("'/bin/cat'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testFromSelect5()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "FROM page_view_stg pvs\n" + "INSERT OVERWRITE TABLE page_view PARTITION(dt='2008-06-08', country)\n" + "SELECT pvs.viewTime, pvs.userid, pvs.page_url, pvs.referrer_url, null, null, pvs.ip, pvs.country";

            Assert.IsTrue(sqlparser.parse() == 0);

            THiveFromQuery select = (THiveFromQuery)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.tables.getTable(0).TableName.ToString().Equals("page_view_stg", StringComparison.CurrentCultureIgnoreCase));

            TInsertSqlStatement insert = (TInsertSqlStatement)select.HiveBodyList.get(0);
            Assert.IsTrue(insert.HiveInsertType == EHiveInsertType.overwriteTable);
            TTable table = insert.TargetTable;
            TPartitionExtensionClause partition = table.PartitionExtensionClause;

            Assert.IsTrue(table.TableName.ToString().Equals("page_view", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(partition.KeyValues.getExpression(0).LeftOperand.ToString().Equals("dt", StringComparison.CurrentCultureIgnoreCase));
            TSelectSqlStatement subquery = insert.SubQuery;
            Assert.IsTrue(subquery.ResultColumnList.size() == 8);
        }

        [TestMethod]
        public void testFromSelect6()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "FROM pv_users\n" + "MAP pv_users.userid, pv_users.date\n" + "USING 'map_script'\n" + "AS dt, uid\n" + "CLUSTER BY dt;";

            Assert.IsTrue(sqlparser.parse() == 0);

            THiveFromQuery select = (THiveFromQuery)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.tables.getTable(0).TableName.ToString().Equals("pv_users", StringComparison.CurrentCultureIgnoreCase));
            TSelectSqlStatement map = (TSelectSqlStatement)select.HiveBodyList.get(0);
            Assert.IsTrue(map.TransformClause != null);
            THiveTransformClause transformClause = map.TransformClause;
            Assert.IsTrue(transformClause.TransformType == THiveTransformClause.ETransformType.ettMap);
            Assert.IsTrue(transformClause.ExpressionList.getExpression(0).ToString().Equals("pv_users.userid", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(transformClause.UsingString.ToString().Equals("'map_script'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(map.HiveClusterBy.ExpressionList.getExpression(0).ToString().Equals("dt", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testFunction1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT create_union(0, key), " + "create_union(if(key<100, 0, 1), 2.0, value), " + "create_union(1, \"a\", struct(2, \"b\")) FROM src LIMIT 2;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TExpression expr0 = select.ResultColumnList.getResultColumn(0).Expr;
            TExpression expr1 = select.ResultColumnList.getResultColumn(1).Expr;
            TExpression expr2 = select.ResultColumnList.getResultColumn(2).Expr;

            Assert.IsTrue(expr0.ExpressionType == EExpressionType.function_t);
            Assert.IsTrue(expr1.FunctionCall.FunctionName.ToString().Equals("create_union", StringComparison.CurrentCultureIgnoreCase));

            TExpression arg = expr1.FunctionCall.Args.getExpression(0);
            Assert.IsTrue(arg.ToString().Equals("if(key<100, 0, 1)", StringComparison.CurrentCultureIgnoreCase));
            TFunctionCall iff = arg.FunctionCall;
            Assert.IsTrue(iff.FunctionName.ToString().Equals("if", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(iff.Args.size() == 3);
            TExpression argofarg = iff.Args.getExpression(0);
            Assert.IsTrue(argofarg.ExpressionType == EExpressionType.simple_comparison_t);


        }

        [TestMethod]
        public void testCast()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "select cast(t as boolean) from decimal_2;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TExpression expr0 = select.ResultColumnList.getResultColumn(0).Expr;
            TFunctionCall f = expr0.FunctionCall;
            Assert.IsTrue(f.FunctionType == EFunctionType.cast_t);
            Assert.IsTrue(f.Expr1.ToString().Equals("t", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(f.Typename.DataType == EDataType.boolean_t);
        }

        [TestMethod]
        public void testGetFullTableName()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "INSERT OVERWRITE LOCAL DIRECTORY '/tmp/ttt' SELECT * from (select * from a) c";
            Assert.IsTrue(sqlparser.parse() == 0);

            TInsertSqlStatement insertSqlStatement = (TInsertSqlStatement)sqlparser.sqlstatements.get(0);
            TSelectSqlStatement select = insertSqlStatement.SubQuery;
            TTable table = select.tables.getTable(0);
            Console.WriteLine(table.FullName);
        }

        [TestMethod]
        public void testHint1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT /*+ STREAMTABLE(a) */ a.val, b.val, c.val FROM a JOIN b ON (a.key = b.key1) JOIN c ON (c.key = b.key1);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            THiveHintClause hint = select.HiveHintClause;
            Assert.IsTrue(hint.HintList[0].HintName.ToString().Equals("STREAMTABLE", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(hint.HintList[0].HintArgs.getObjectName(0).ToString().Equals("a", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testImportTable1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "import table department from 'hdfs_exports_location/department' \n" + "       location 'import_target_location/department';";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveImportTable importTable = (THiveImportTable)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(!importTable.External);
            Assert.IsTrue(importTable.Table.ToString().Equals("department", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(importTable.Path.ToString().Equals("'hdfs_exports_location/department'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(importTable.TableLocation.ToString().Equals("'import_target_location/department'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testImportTable2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "import from 'hdfs_exports_location/department';";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveImportTable importTable = (THiveImportTable)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(!importTable.External);
            Assert.IsTrue(importTable.Table == null);
            Assert.IsTrue(importTable.Path.ToString().Equals("'hdfs_exports_location/department'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(importTable.TableLocation == null);
        }

        [TestMethod]
        public void testInsert1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "INSERT OVERWRITE DIRECTORY 's3://bucketname/path/subpath/' SELECT * \n" + "FROM hiveTableName;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TInsertSqlStatement insert = (TInsertSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(insert.HiveInsertType == EHiveInsertType.overwriteDirectory);
            Assert.IsTrue(insert.DirectoryName.ToString().Equals("'s3://bucketname/path/subpath/'", StringComparison.CurrentCultureIgnoreCase));

            TSelectSqlStatement select = insert.SubQuery;
            Assert.IsTrue(select.ResultColumnList.getResultColumn(0).ToString().Equals("*", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(select.tables.getTable(0).ToString().Equals("hiveTableName", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testInsert2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "INSERT OVERWRITE TABLE hiveTableName SELECT * FROM s3_import;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TInsertSqlStatement insert = (TInsertSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(insert.HiveInsertType == EHiveInsertType.overwriteTable);
            Assert.IsTrue(insert.tables.getTable(0).ToString().Equals("hiveTableName", StringComparison.CurrentCultureIgnoreCase));

            TSelectSqlStatement select = insert.SubQuery;
            Assert.IsTrue(select.ResultColumnList.getResultColumn(0).ToString().Equals("*", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(select.tables.getTable(0).ToString().Equals("s3_import", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testInsert3()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "INSERT OVERWRITE TABLE pv_users\n" + "SELECT pv.*, u.gender, u.age\n" + "FROM user FULL OUTER JOIN page_view pv ON (pv.userid = u.id)\n" + "WHERE pv.date = '2008-03-03';";
            Assert.IsTrue(sqlparser.parse() == 0);

            TInsertSqlStatement insert = (TInsertSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(insert.HiveInsertType == EHiveInsertType.overwriteTable);
            Assert.IsTrue(insert.tables.getTable(0).ToString().Equals("pv_users", StringComparison.CurrentCultureIgnoreCase));

            TSelectSqlStatement select = insert.SubQuery;
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_table);
            Assert.IsTrue(join.Table.TableName.ToString().Equals("user", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(select.ToString().Equals("SELECT pv.*, u.gender, u.age\n" + "FROM user FULL OUTER JOIN page_view pv ON (pv.userid = u.id)\n" + "WHERE pv.date = '2008-03-03'", StringComparison.CurrentCultureIgnoreCase));
            TJoinItem joinItem = join.JoinItems.getJoinItem(0);
            Assert.IsTrue(joinItem.JoinType == EJoinType.fullouter);
            Assert.IsTrue(joinItem.Table.ToString().Equals("page_view", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(joinItem.OnCondition.ToString().Equals("(pv.userid = u.id)", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testJoin()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT page_views.*\n" + "FROM page_views JOIN dim_users\n" + "  ON (page_views.user_id = dim_users.id AND page_views.date >= '2008-03-01' AND page_views.date <= '2008-03-31')";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 1);

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_table);
            Assert.IsTrue(join.Table.ToString().Equals("page_views", StringComparison.CurrentCultureIgnoreCase));
            TJoinItem joinItem = join.JoinItems.getJoinItem(0);
            Assert.IsTrue(joinItem.JoinType == EJoinType.join);
            Assert.IsTrue(joinItem.Table.ToString().Equals("dim_users", StringComparison.CurrentCultureIgnoreCase));
            TExpression joinCondition = joinItem.OnCondition;
            Assert.IsTrue(joinCondition.ExpressionType == EExpressionType.parenthesis_t);
            joinCondition = joinCondition.LeftOperand;
            Assert.IsTrue(joinCondition.ExpressionType == EExpressionType.logical_and_t);
            //System.out.println(joinCondition.toString());
        }

        [TestMethod]
        public void testJoin1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT a.val, b.val, c.val FROM a JOIN b ON (a.key = b.key1) JOIN c ON (c.key = b.key2);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_table);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.objectname);
            Assert.IsTrue(String.Equals(table.FullName,"a", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(join.JoinItems.size() == 2);
            TJoinItem joinItem = join.JoinItems.getJoinItem(0);
            Assert.IsTrue(joinItem.Table.ToString().Equals("b", StringComparison.CurrentCultureIgnoreCase));
            TJoinItem joinItem2 = join.JoinItems.getJoinItem(1);
            Assert.IsTrue(joinItem2.JoinType == EJoinType.join);
            TExpression joinCondition = joinItem2.OnCondition;
            Assert.IsTrue(joinCondition.ExpressionType == EExpressionType.parenthesis_t);
            Assert.IsTrue(joinCondition.LeftOperand.ToString().Equals("c.key = b.key2", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testJoin2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "select /*+MAPJOIN(smallTableTwo)*/ idOne, idTwo, value FROM\n" + "  ( select /*+MAPJOIN(smallTableOne)*/ idOne, idTwo, value FROM\n" + "    bigTable JOIN smallTableOne on (bigTable.idOne = smallTableOne.idOne)                                                   \n" + "  ) firstjoin                                                             \n" + "  JOIN                                                                  \n" + "  smallTableTwo on (firstjoin.idTwo = smallTableTwo.idTwo)    ;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_table);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.subquery);
            TJoinItem joinItem = join.JoinItems.getJoinItem(0);
            Assert.IsTrue(joinItem.JoinType == EJoinType.join);
            Assert.IsTrue(String.Equals(joinItem.Table.FullName,"smallTableTwo", StringComparison.CurrentCultureIgnoreCase));
            // Assert.IsTrue(table.getFullName().equalsIgnoreCase("a"));

            TSelectSqlStatement subquery = table.Subquery;
            THiveHintClause hintClause = subquery.HiveHintClause;
            Assert.IsTrue(hintClause.HintList[0].HintName.ToString().Equals("MAPJOIN", StringComparison.CurrentCultureIgnoreCase));

            join = subquery.joins.getJoin(0);
            Assert.IsTrue(String.Equals(join.Table.FullName,"bigTable", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testLeftJoin()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT a.val1, a.val2, b.val, c.val\n" + "  FROM a\n" + "  JOIN b ON (a.KEY = b.KEY)\n" + "  LEFT OUTER JOIN c ON (a.KEY = c.KEY);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 4);

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_table);
            Assert.IsTrue(join.Table.ToString().Equals("a", StringComparison.CurrentCultureIgnoreCase));
            TJoinItem joinItem = join.JoinItems.getJoinItem(0);
            Assert.IsTrue(joinItem.JoinType == EJoinType.join);
            Assert.IsTrue(joinItem.Table.ToString().Equals("b", StringComparison.CurrentCultureIgnoreCase));
            TExpression joinCondition = joinItem.OnCondition;
            Assert.IsTrue(joinCondition.ExpressionType == EExpressionType.parenthesis_t);
            Assert.IsTrue(joinCondition.ToString().Equals("(a.KEY = b.KEY)", StringComparison.CurrentCultureIgnoreCase));
            joinCondition = joinCondition.LeftOperand;
            Assert.IsTrue(joinCondition.ExpressionType == EExpressionType.simple_comparison_t);
            Assert.IsTrue(joinCondition.ToString().Equals("a.KEY = b.KEY", StringComparison.CurrentCultureIgnoreCase));


            joinItem = join.JoinItems.getJoinItem(1);
            Assert.IsTrue(joinItem.JoinType == EJoinType.leftouter);
            Assert.IsTrue(joinItem.Table.ToString().Equals("c", StringComparison.CurrentCultureIgnoreCase));
            joinCondition = joinItem.OnCondition;
            Assert.IsTrue(joinCondition.ExpressionType == EExpressionType.parenthesis_t);
            joinCondition = joinCondition.LeftOperand;
            Assert.IsTrue(joinCondition.ExpressionType == EExpressionType.simple_comparison_t);
        }

        [TestMethod]
        public  void testLateralView1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT * FROM exampleTable\n" + "        LATERAL VIEW explode(col1) myTable1 AS myCol1\n" + "        LATERAL VIEW explode(myCol1) myTable2 AS myCol2;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);

            TTable table = select.tables.getTable(0);
            Assert.IsTrue(table.TableType == ETableSource.objectname);
            Assert.IsTrue(table.TableName.ToString().Equals("exampleTable", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(table.LateralViewList.Count == 2);

            THiveLateralView view = table.LateralViewList[0];
            TFunctionCall call = view.Udtf;
            Assert.IsTrue(call.FunctionName.ToString().Equals("explode", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(call.Args.getExpression(0).ToString().Equals("col1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(view.TableAlias.AliasName.ToString().Equals("myTable1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(view.ColumnAliasList.size() == 1);
            Assert.IsTrue(view.ColumnAliasList.getObjectName(0).ToString().Equals("myCol1", StringComparison.CurrentCultureIgnoreCase));

            view = table.LateralViewList[1];
            call = view.Udtf;
            Assert.IsTrue(call.FunctionName.ToString().Equals("explode", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(call.Args.getExpression(0).ToString().Equals("myCol1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(view.TableAlias.AliasName.ToString().Equals("myTable2", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(view.ColumnAliasList.size() == 1);
            Assert.IsTrue(view.ColumnAliasList.getObjectName(0).ToString().Equals("myCol2", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testlexer1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "A=B";
            sqlparser.tokenizeSqltext();
            Assert.IsTrue(sqlparser.sourcetokenlist.get(1).tokencode == TBaseType.hive_equal);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(1).ToString().Equals("=", StringComparison.CurrentCultureIgnoreCase));

            sqlparser.sqltext = "A==B";
            sqlparser.tokenizeSqltext();
            Assert.IsTrue(sqlparser.sourcetokenlist.get(1).tokencode == TBaseType.hive_equal);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(1).ToString().Equals("==", StringComparison.CurrentCultureIgnoreCase));

            sqlparser.sqltext = "A<=>B";
            sqlparser.tokenizeSqltext();
            Assert.IsTrue(sqlparser.sourcetokenlist.get(1).tokencode == TBaseType.hive_equal_ns);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(1).ToString().Equals("<=>", StringComparison.CurrentCultureIgnoreCase));

            sqlparser.sqltext = "123L=123S=123Y"; //big/small/tiny int
            sqlparser.tokenizeSqltext();
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).ToString().Equals("123L", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).tokencode == TBaseType.hive_BigintLiteral);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(2).ToString().Equals("123S", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(2).tokencode == TBaseType.hive_SmallintLiteral);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(4).ToString().Equals("123Y", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(4).tokencode == TBaseType.hive_TinyintLiteral);

            sqlparser.sqltext = "12.3BD=1.23e-10=10E+2"; //big/small/tiny int
            sqlparser.tokenizeSqltext();
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).ToString().Equals("12.3BD", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).tokencode == TBaseType.hive_DecimalLiteral);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(2).ToString().Equals("1.23e-10", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(2).tokencode == TBaseType.hive_number);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(4).ToString().Equals("10E+2", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(4).tokencode == TBaseType.hive_number);

            //
            sqlparser.sqltext = "12B=123b=10K"; //big/small/tiny int
            sqlparser.tokenizeSqltext();
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).ToString().Equals("12B", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).tokencode == TBaseType.hive_ByteLengthLiteral);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(2).ToString().Equals("123b", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(2).tokencode == TBaseType.hive_ByteLengthLiteral);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(4).ToString().Equals("10K", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(4).tokencode == TBaseType.hive_ByteLengthLiteral);
            //
            //
            sqlparser.sqltext = "`abc*`";
            sqlparser.tokenizeSqltext();
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).ToString().Equals("`abc*`", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).tokencode == TBaseType.ident);
            //
            sqlparser.sqltext = "0XA9F7";
            sqlparser.tokenizeSqltext();
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).ToString().Equals("0XA9F7", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).tokencode == TBaseType.hive_CharSetLiteral);
            //
            sqlparser.sqltext = "'a\\'b'"; //"'a\\'b"+""+"'";
            sqlparser.tokenizeSqltext();
            // System.out.println(sqlparser.sqltext);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).ToString().Equals("'a\\'b'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).tokencode == TBaseType.hive_StringLiteral);
            //
            sqlparser.sqltext = "\"a\\\"'b\""; //"a\"'b"
            sqlparser.tokenizeSqltext();
            //System.out.println(sqlparser.sqltext);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).ToString().Equals("\"a\\\"'b\"", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).tokencode == TBaseType.hive_StringLiteral);
            //
            sqlparser.sqltext = "_as-._:k";
            sqlparser.tokenizeSqltext();
            //System.out.println(sqlparser.sqltext);
            //System.out.println(sqlparser.sourcetokenlist.get(0).toString());
            //System.out.println(sqlparser.sourcetokenlist.get(1).toString());
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).ToString().Equals("_as-._:k", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).tokencode == TBaseType.hive_CharSetName);
            //
            sqlparser.sqltext = "asme=9ss=ax_x_";
            sqlparser.tokenizeSqltext();
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).ToString().Equals("asme", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).tokencode == TBaseType.ident);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(2).ToString().Equals("9ss", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(2).tokencode == TBaseType.ident);
            Assert.IsTrue(sqlparser.sourcetokenlist.get(4).ToString().Equals("ax_x_", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(4).tokencode == TBaseType.ident);
            //
            sqlparser.sqltext = "where";
            sqlparser.tokenizeSqltext();
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).ToString().Equals("where", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).tokencode == TBaseType.rrw_where);
            //
            sqlparser.sqltext = "-- where";
            sqlparser.tokenizeSqltext();
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).ToString().Equals("-- where", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sourcetokenlist.get(0).tokencode == TBaseType.cmtdoublehyphen);
        }

        [TestMethod]
        public  void testLoad1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "LOAD DATA LOCAL INPATH  '/tmp/simple.json' INTO TABLE json_table;";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveLoad load = (THiveLoad)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(load.Islocal);
            Assert.IsTrue(!load.Isoverwrite);
            Assert.IsTrue(load.Path.ToString().Equals("'/tmp/simple.json'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(load.Table.ToString().Equals("json_table", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public  void testLoad2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "LOAD DATA LOCAL INPATH '/tmp/pv_2008-06-08_us.txt' INTO TABLE page_view PARTITION(date='2008-06-08', country='US');";
            Assert.IsTrue(sqlparser.parse() == 0);

            THiveLoad load = (THiveLoad)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(load.Islocal);
            Assert.IsTrue(!load.Isoverwrite);
            Assert.IsTrue(load.Path.ToString().Equals("'/tmp/pv_2008-06-08_us.txt'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(load.Table.TableName.ToString().Equals("page_view", StringComparison.CurrentCultureIgnoreCase));
            TPartitionExtensionClause p = load.Table.PartitionExtensionClause;
            Assert.IsTrue(p.KeyValues.size() == 2);
            Assert.IsTrue(p.KeyValues.getExpression(0).LeftOperand.ToString().Equals("date", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(p.KeyValues.getExpression(0).RightOperand.ToString().Equals("'2008-06-08'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(p.KeyValues.getExpression(1).LeftOperand.ToString().Equals("country", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(p.KeyValues.getExpression(1).RightOperand.ToString().Equals("'US'", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public  void testParse1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "INSERT OVERWRITE TABLE pv_gender_sum\n" + "  SELECT pv_users.gender, count (DISTINCT pv_users.userid)\n" + "  FROM pv_users\n" + "  GROUP BY pv_users.gender;";
            Assert.IsTrue(sqlparser.parse() == 0);
        }

        [TestMethod]
        public  void testParse2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "  SELECT pv_users.gender, count (DISTINCT pv_users.userid)\n" + "  FROM pv_users\n" + "  GROUP BY pv_users.gender;";
            Assert.IsTrue(sqlparser.parse() == 0);
        }

        [TestMethod]
        public void testSampling()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT *\n" + "FROM source TABLESAMPLE(BUCKET 3 OUT OF 32 ON rand()) s;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 1);

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_fake);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.objectname);
            TAliasClause aliasClause = table.AliasClause;
            Assert.IsTrue(aliasClause.AliasName.ToString().Equals("s", StringComparison.CurrentCultureIgnoreCase));

            TTableSample tableSample = table.TableSample;
            Assert.IsTrue(tableSample.BucketNumber.ToString().Equals("3", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(tableSample.OutofNumber.ToString().Equals("32", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(tableSample.OnExprList.size() == 1);
            Assert.IsTrue(tableSample.OnExprList.getExpression(0).ToString().Equals("rand()", StringComparison.CurrentCultureIgnoreCase));
            //System.out.println(tableSample.toString());
        }

        [TestMethod]
        public void testSampling2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT *\n" + "FROM source TABLESAMPLE(0.1 PERCENT) s;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 1);

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_fake);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.objectname);
            TAliasClause aliasClause = table.AliasClause;
            Assert.IsTrue(aliasClause.AliasName.ToString().Equals("s", StringComparison.CurrentCultureIgnoreCase));

            TTableSample tableSample = table.TableSample;
            Assert.IsTrue(tableSample.Numerator.ToString().Equals("0.1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(tableSample.Percent.ToString().Equals("PERCENT", StringComparison.CurrentCultureIgnoreCase));
            //System.out.println(tableSample.toString());
        }

        [TestMethod]
        public void testSampling3()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT *\n" + "FROM source TABLESAMPLE(100M) s;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 1);

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_fake);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.objectname);
            TAliasClause aliasClause = table.AliasClause;
            Assert.IsTrue(aliasClause.AliasName.ToString().Equals("s", StringComparison.CurrentCultureIgnoreCase));

            TTableSample tableSample = table.TableSample;
            Assert.IsTrue(tableSample.Numerator.ToString().Equals("100M", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(tableSample.Percent == null);
            //System.out.println(tableSample.toString());
        }

        [TestMethod]
        public void testSampling4()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvhive);
            sqlparser.sqltext = "SELECT * FROM source TABLESAMPLE(10 ROWS);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.ResultColumnList.size() == 1);

            Assert.IsTrue(select.joins.size() == 1);
            TJoin join = select.joins.getJoin(0);
            Assert.IsTrue(join.Kind == TBaseType.join_source_fake);

            TTable table = join.Table;
            Assert.IsTrue(table.TableType == ETableSource.objectname);


            TTableSample tableSample = table.TableSample;
            Assert.IsTrue(tableSample.Numerator.ToString().Equals("10", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(tableSample.Percent.ToString().Equals("ROWS", StringComparison.CurrentCultureIgnoreCase));
            //System.out.println(tableSample.toString());
        }
    }
}
