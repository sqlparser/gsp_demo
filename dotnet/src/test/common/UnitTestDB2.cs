using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.IO;
using gudusoft.gsqlparser.stmt;
using gudusoft.gsqlparser.stmt.db2;
using gudusoft.gsqlparser.nodes;

namespace gudusoft.gsqlparser.test
{
    /// <summary>
    /// UnitTestOracle 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTestDB2
    {
        TGSqlParser parser;

        public UnitTestDB2()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
            parser = new TGSqlParser(EDbVendor.dbvdb2);
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
        public void TestDB2Files()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"db2/", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public void TestDB2Files2()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"new_dotnet\db2\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public  void testDB2_1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvdb2);

            sqlparser.sqltext = "CALL test2()";

            Assert.IsTrue(sqlparser.parse() == 0);
        }

        [TestMethod]
        public  void testCreateDatabase()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvdb2);

            sqlparser.sqltext = "CREATE DATABASE MYDB\n" + "   BUFFERPOOL BP1\n" + "   INDEXBP BP2\n" + "   CCSID EBCDIC\n" + "   STOGROUP B0SG0100;";

            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateDatabaseSqlStatement db = (TCreateDatabaseSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(db.DatabaseName.ToString().Equals("MYDB", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public  void testCreateProcedure()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvdb2);

            sqlparser.sqltext = "CREATE PROCEDURE CREATE_T_EMP()\n" + "   LANGUAGE SQL\n" + "BEGIN\n" + "DECLARE SQLCODE INT;\n" + "DECLARE l_sqlcode INT DEFAULT 0;\n" + "\n" + "  DECLARE CONTINUE HANDLER FOR NOT FOUND\n" + "    SET l_sqlcode = SQLCODE; \n" + "\n" + "        INSERT INTO PROJECT (PROJNO, PROJNAME, DEPTNO, RESPEMP, PRSTDATE) \n" + "        VALUES('HG0023', 'NEW NETWORK', 'E11', '200280', CURRENT DATE); \n" + "END";

            Assert.IsTrue(sqlparser.parse() == 0);

            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstdb2createprocedure);
            TDb2CreateProcedure procedure = (TDb2CreateProcedure)sqlparser.sqlstatements.get(0);
            // System.out.print(procedure.getBodyStatements().size());
            Assert.IsTrue(procedure.DeclareStatements.size() == 3);
            Assert.IsTrue(procedure.DeclareStatements.get(2).ToString().Equals("DECLARE CONTINUE HANDLER FOR NOT FOUND\n" + "    SET l_sqlcode = SQLCODE", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(procedure.BodyStatements.size() == 1);

            Assert.IsTrue(procedure.BodyStatements.get(0).sqlstatementtype == ESqlStatementType.sstinsert);
            TInsertSqlStatement insert = (TInsertSqlStatement)procedure.BodyStatements.get(0);
            // System.out.print(insert.toString());
            // Assert.IsTrue(insert.toString() != null);
            Assert.IsTrue(insert.ToString().Equals("INSERT INTO PROJECT (PROJNO, PROJNAME, DEPTNO, RESPEMP, PRSTDATE) \n" + "        VALUES('HG0023', 'NEW NETWORK', 'E11', '200280', CURRENT DATE)", StringComparison.CurrentCultureIgnoreCase));


        }

        [TestMethod]
        public virtual void testParameter()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvdb2);

            sqlparser.sqltext = "CREATE PROCEDURE \"SA\".\"TEST2\"(parm1 INT DEFAULT -1, parm2 INT DEFAULT -3)\n" + "BEGIN\n" + "\tselect a from b;\n" + "END";

            Assert.IsTrue(sqlparser.parse() == 0);
        }

        [TestMethod]
        public virtual void testWithCheckOption()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvdb2);
            sqlparser.sqltext = "CREATE VIEW V2 AS SELECT COL1 FROM V1 WITH check option";
            Assert.IsTrue(sqlparser.parse() == 0);
            TCreateViewSqlStatement createView = (TCreateViewSqlStatement)sqlparser.sqlstatements.get(0);
            TRestrictionClause restrictionClause = createView.RestrictionClause;
            Assert.IsTrue(restrictionClause.Type == TRestrictionClause.with_check_option);
        }

        [TestMethod]
        public  void testCreateIndex()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvdb2);
            sqlparser.sqltext = "CREATE UNIQUE INDEX \"A\".\"IB\" ON \"A\".\"B\"\n" + "             (\"A\" ASC,\n" + "             \"B\" ASC )\n" + "             ALLOW REVERSE SCANS; --Error";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateIndexSqlStatement createIndexSqlStatement = (TCreateIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createIndexSqlStatement.IndexName.ToString().Equals("\"A\".\"IB\"", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndexSqlStatement.TableName.ToString().Equals("\"A\".\"B\"", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndexSqlStatement.ColumnNameList.getOrderByItem(0).SortKey.ToString().Equals("\"A\"", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndexSqlStatement.ColumnNameList.getOrderByItem(0).SortType == 1);
        }


        [TestMethod]
        public void testCTEMainQueryToString()
        {
            String varname1 = "";
            varname1 = varname1 + "WITH prep1 " + "\n";
            varname1 = varname1 + "     AS ( SELECT 'First' AS colval, " + "\n";
            varname1 = varname1 + "                 1       AS joincol " + "\n";
            varname1 = varname1 + "          FROM " + "\n";
            varname1 = varname1 + "            SYSIBM.sysdummy1 ) , prep2 " + "\n";
            varname1 = varname1 + "     AS ( SELECT 'Second' AS colval, " + "\n";
            varname1 = varname1 + "                 1        AS joincol " + "\n";
            varname1 = varname1 + "          FROM " + "\n";
            varname1 = varname1 + "            SYSIBM.sysdummy1 ) , prep3 " + "\n";
            varname1 = varname1 + "     AS ( SELECT 'Third' AS colval, " + "\n";
            varname1 = varname1 + "                 1       AS joincol " + "\n";
            varname1 = varname1 + "          FROM " + "\n";
            varname1 = varname1 + "            SYSIBM.sysdummy1 ) " + "\n";
            varname1 = varname1 + "SELECT prep1.colval AS firstcolval, " + "\n";
            varname1 = varname1 + "       prep2.colval AS secondcolval, " + "\n";
            varname1 = varname1 + "       prep3.colval " + "\n";
            varname1 = varname1 + "FROM " + "\n";
            varname1 = varname1 + "  prep1 " + "\n";
            varname1 = varname1 + "  INNER JOIN prep2 " + "\n";
            varname1 = varname1 + "  ON prep1.joincol = prep2.joincol " + "\n";
            varname1 = varname1 + "  INNER JOIN prep3 " + "\n";
            varname1 = varname1 + "  ON prep1.joincol = prep3.joincol";

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvdb2);
            sqlparser.sqltext = varname1;
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TSourceToken st = select.startToken; // keep origin start token
            select.startToken  = select.SelectToken;
            //Console.WriteLine(select.ToString());
            select.startToken = st; // restore to origin start token
            //Console.WriteLine(select.ToString());
        }

    }
}
