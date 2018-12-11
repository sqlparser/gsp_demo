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

    [TestClass]
    public class UTClientTosska
    {


        public UTClientTosska()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
            
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
        public void testOwnStmtOfExpr()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"DELETE FROM employee e WHERE       NOT EXISTS ( SELECT * FROM department d WHERE e.emp_dept = d.dpt_id)";
            Assert.IsTrue(sqlparser.parse() == 0);
            TDeleteSqlStatement delete = (TDeleteSqlStatement)sqlparser.sqlstatements.get(0);
            TExpression condition = delete.WhereClause.Condition;
            TExpression subqueryExpr = condition.RightOperand;
            TSelectSqlStatement subquery = subqueryExpr.SubQuery;
            condition = subquery.WhereClause.Condition;
            TCustomSqlStatement ownStmt = condition.OwnerStmt;
            Assert.IsTrue(ownStmt.sqlstatementtype == ESqlStatementType.sstselect);
        }

    }

}
