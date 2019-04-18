using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.IO;
using gudusoft.gsqlparser.stmt;
using gudusoft.gsqlparser.stmt.informix;

namespace gudusoft.gsqlparser.test
{
    /// <summary>
    /// UnitTestOracle 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTestInformix
    {
        TGSqlParser parser;

        public UnitTestInformix()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
            parser = new TGSqlParser(EDbVendor.dbvinformix);
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
        public void TestInformixFiles()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"informix\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public void testColumn()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvinformix);
            sqlparser.sqltext = "RENAME COLUMN customer.customer_num TO c_num;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TRenameStmt stmt = (TRenameStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.OldName.ToString().Equals("customer.customer_num", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(stmt.NewName.ToString().Equals("c_num", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(stmt.ObjectType == EDbObjectType.column);
        }

        [TestMethod]
        public void testIndex()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvinformix);
            sqlparser.sqltext = "RENAME index customer.customer_num TO c_num;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TRenameStmt stmt = (TRenameStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.OldName.ToString().Equals("customer.customer_num", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(stmt.NewName.ToString().Equals("c_num", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(stmt.ObjectType == EDbObjectType.index);
        }

        [TestMethod]
        public void testSequence()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvinformix);
            sqlparser.sqltext = "RENAME sequence customer.customer_num TO c_num;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TRenameStmt stmt = (TRenameStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.OldName.ToString().Equals("customer.customer_num", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(stmt.NewName.ToString().Equals("c_num", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(stmt.ObjectType == EDbObjectType.sequence);
        }

        [TestMethod]
        public void testTable()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvinformix);
            sqlparser.sqltext = "RENAME TABLE new_table TO items;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TRenameStmt stmt = (TRenameStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.OldName.ToString().Equals("new_table", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(stmt.NewName.ToString().Equals("items", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(stmt.ObjectType == EDbObjectType.table);
        }

        [TestMethod]
        public void testRowType()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvinformix);
            sqlparser.sqltext = "DROP ROW TYPE employee_t RESTRICT";
            Assert.IsTrue(sqlparser.parse() == 0);

            TInformixDropRowTypeStmt stmt = (TInformixDropRowTypeStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.RowTypeName.ToString().Equals("employee_t", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testSequence2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvinformix);
            sqlparser.sqltext = "DROP SEQUENCE Invoice_Numbers;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TDropSequenceStmt stmt = (TDropSequenceStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.SequenceName.ToString().Equals("Invoice_Numbers", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testSynonym()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvinformix);
            sqlparser.sqltext = "DROP SYNONYM cathyg.nj_cust;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TDropSynonymStmt stmt = (TDropSynonymStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.SynonymName.ToString().Equals("cathyg.nj_cust", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testIndex2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvinformix);
            sqlparser.sqltext = "DROP INDEX stores_demo:joed.o_num_ix;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TDropIndexSqlStatement stmt = (TDropIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.IndexName.ToString().Equals("stores_demo:joed.o_num_ix", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testTable2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvinformix);
            sqlparser.sqltext = "DROP TABLE stores_demo@accntg:joed.state;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TDropTableSqlStatement stmt = (TDropTableSqlStatement)sqlparser.sqlstatements.get(0);

            Assert.IsTrue(stmt.TableName.ToString().Equals("stores_demo@accntg:joed.state", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testView()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvinformix);
            sqlparser.sqltext = "DROP VIEW cust1";
            Assert.IsTrue(sqlparser.parse() == 0);

            TDropViewSqlStatement stmt = (TDropViewSqlStatement)sqlparser.sqlstatements.get(0);

            Assert.IsTrue(stmt.ViewName.ToString().Equals("cust1", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public virtual void testIndex3()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvinformix);
            sqlparser.sqltext = "ALTER INDEX ix_cust TO NOT CLUSTER;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterIndexStmt stmt = (TAlterIndexStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.IndexName.ToString().Equals("ix_cust", StringComparison.CurrentCultureIgnoreCase));
        }

    }
}
