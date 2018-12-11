using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.IO;
using gudusoft.gsqlparser.stmt;
using gudusoft.gsqlparser.nodes;

namespace gudusoft.gsqlparser.test
{
    /// <summary>
    /// UnitTestOracle 的摘要说明
    /// </summary>
    [TestClass]
    public class UTClientDag
    {
        TGSqlParser parser;

        public UTClientDag()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
            parser = new TGSqlParser(EDbVendor.dbvoracle);
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
        public void testExtractXML()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT warehouse_name,\n" + "   EXTRACT(warehouse_spec, '/Warehouse/Docks')\n" + "   \"Number of Docks\"\n" + "   FROM warehouses\n" + "   WHERE warehouse_name = 'San Francisco';";
            Assert.IsTrue(sqlparser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TResultColumn column = select.ResultColumnList.getResultColumn(1);
            TExpression expr = column.Expr;
            TFunctionCall f = expr.FunctionCall;
            Assert.IsTrue(f.FunctionType == EFunctionType.extractxml_t);
            Assert.IsTrue(f.XMLType_Instance.ToString().Equals("warehouse_spec", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(f.XPath_String.ToString().Equals("'/Warehouse/Docks'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testTreat()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT name, TREAT(VALUE(p) AS employee_t).salary salary \n" + "   FROM persons p;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TResultColumn column = select.ResultColumnList.getResultColumn(1);
            TExpression expr = column.Expr;
            TFunctionCall f = expr.FunctionCall;
            Assert.IsTrue(expr.ExpressionType == EExpressionType.object_access_t);
        }

        [TestMethod]
        public void testTranslate()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "create view wm$all_locks_view as select t.table_owner, t.table_name,\n" + "       decode(sys.lt_ctx_pkg.getltlockinfo(translate(t.info using char_cs),'row_lockmode'), 'e', 'exclusive', 's', 'shared') lock_mode,\n" + "       sys.lt_ctx_pkg.getltlockinfo(translate(t.info using char_cs),'row_lockuser') lock_owner,\n" + "       sys.lt_ctx_pkg.getltlockinfo(translate(t.info using char_cs),'row_lockstate') locking_state\n" + "from (select table_owner, table_name, info from\n" + "      table( cast(sys.ltadm.get_lock_table() as wm$lock_table_type))) t\n" + "with read only";
            Assert.IsTrue(sqlparser.parse() == 0);
        }

        [TestMethod]
        public void testXmlAgg()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT\n" + "   O.OBJECT_ID,\n" + "   '|' || RTRIM (XMLAGG (XMLELEMENT (K, O.KEY_1 || '|')).EXTRACT ('//text()'),\n" + "'|') || '|' AS TEXT_KEY\n" + "FROM DAG_OBJECT_FACT O";
            Assert.IsTrue(sqlparser.parse() == 0);

            TResultColumn rc = ((TSelectSqlStatement)sqlparser.sqlstatements.get(0)).ResultColumnList.getResultColumn(1);
            TExpression expression = rc.Expr.LeftOperand.RightOperand;
            TFunctionCall functionCall = expression.FunctionCall;
            TExpression xmlaggExpr = functionCall.Args.getExpression(0);
            Assert.IsTrue(xmlaggExpr.ExpressionType == EExpressionType.object_access_t);
            TObjectAccess objectAccess = xmlaggExpr.ObjectAccess;
            TFunctionCall xmlelement = objectAccess.ObjectExpr.FunctionCall.Args.getExpression(0).FunctionCall;
            //Assert.IsTrue(xmlelement.getArgs().size() == 1);
            Assert.IsTrue(xmlelement.XMLElementNameExpr.ToString().Equals("K", StringComparison.CurrentCultureIgnoreCase));
            TResultColumn resultColumn = xmlelement.XMLElementValueExprList.getResultColumn(0);
            TExpression expression1 = resultColumn.Expr;
            Assert.IsTrue(expression1.ToString().Equals("O.KEY_1 || '|'", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testKeepKeyword()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "create view rc_backup_set\n" + "as\n" + "select decode(keep_options, 0, 'no', 'yes') keep\n" + "from db, bs\n" + "where db.db_key = bs.db_key";
            Assert.IsTrue(sqlparser.parse() == 0);
        }

        [TestMethod]
        public  void testMultiSetOperator()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT CAST( MULTISET( SELECT empno, empname FROM emp) AS emp_tab_t ) emptab\n" + "FROM DUAL;";
            Assert.IsTrue(sqlparser.parse() == 0);
        }

    }
}
