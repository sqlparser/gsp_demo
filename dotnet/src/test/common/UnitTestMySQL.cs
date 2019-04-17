using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.IO;
using gudusoft.gsqlparser.stmt.mysql;
using gudusoft.gsqlparser.stmt;
using gudusoft.gsqlparser.nodes;
using gudusoft.gsqlparser.nodes.mysql;

namespace gudusoft.gsqlparser.test
{
    /// <summary>
    /// UnitTestOracle 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTestMySQL
    {
        TGSqlParser parser;

        public UnitTestMySQL()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
            parser = new TGSqlParser(EDbVendor.dbvmysql);
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
        public void TestMySQLFiles()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"mysql\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public void TestMySQLFiles2()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"new_dotnet\mysql\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            List<string> excludeFiles = new List<string> { "altertable_comment_change_column.sql" };
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                if (UnitTestCommon.excludeFile(info.Name, excludeFiles))
                {
                    continue;
                }

                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }
        [TestMethod]
        public void testCall1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CALL test2()";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMySQLCallStmt callStmt = (TMySQLCallStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(callStmt.ProcedureName.ToString().Equals("test2", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCall2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CALL test2(@`a`:=1)";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMySQLCallStmt callStmt = (TMySQLCallStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(callStmt.ProcedureName.ToString().Equals("test2", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(callStmt.Parameters.getExpression(0).LeftOperand.ToString().Equals("@`a`", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateFucntion1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE FUNCTION `func1`(p1 INT) RETURNS varchar(8000)\n" + "READS SQL DATA\n" + "DETERMINISTIC\n" + "BEGIN\n" + "DECLARE v_LIMITSTR VARCHAR(20);\n" + "IF p1 = -1 THEN\n" + "SET v_LIMITSTR = \"\";\n" + "ELSE\n" + "SET v_LIMITSTR = CONCAT(\"LIMIT \",p1);\n" + "END IF;\n" + "RETURN CONCAT(\"SELECT t1.c1, t2.c1, CONCAT('v=',t2.c1,';f=',t1.c1), CONCAT('v=',t2.c7,';f=',t0.c1),\n" + "t0.c2 AS `Test` FROM t0, t1, t2, t3\n" + "WHERE t0.c1 = t1.c6 AND\n" + "t1.c7 = t2.c7 AND\n" + "t1.c6 = t3.c6 AND\n" + "t0.c2 >= t3.c8 AND\n" + "t0.c3='weekend' AND\n" + "t0.c4='chickenrun' AND\n" + "t0.c5 = (SELECT MAX(c5) FROM t4 WHERE c3='weekend' AND c4='chickenrun')\n" + "ORDER BY t0.c2 DESC\", v_LIMITSTR, \";\") ;\n" + "END;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TCustomSqlStatement sqlStatement = sqlparser.sqlstatements.get(0);
            Assert.IsTrue(sqlStatement.ToString().Trim().Equals("CREATE FUNCTION `func1`(p1 INT) RETURNS varchar(8000)\n" + "READS SQL DATA\n" + "DETERMINISTIC\n" + "BEGIN\n" + "DECLARE v_LIMITSTR VARCHAR(20);\n" + "IF p1 = -1 THEN\n" + "SET v_LIMITSTR = \"\";\n" + "ELSE\n" + "SET v_LIMITSTR = CONCAT(\"LIMIT \",p1);\n" + "END IF;\n" + "RETURN CONCAT(\"SELECT t1.c1, t2.c1, CONCAT('v=',t2.c1,';f=',t1.c1), CONCAT('v=',t2.c7,';f=',t0.c1),\n" + "t0.c2 AS `Test` FROM t0, t1, t2, t3\n" + "WHERE t0.c1 = t1.c6 AND\n" + "t1.c7 = t2.c7 AND\n" + "t1.c6 = t3.c6 AND\n" + "t0.c2 >= t3.c8 AND\n" + "t0.c3='weekend' AND\n" + "t0.c4='chickenrun' AND\n" + "t0.c5 = (SELECT MAX(c5) FROM t4 WHERE c3='weekend' AND c4='chickenrun')\n" + "ORDER BY t0.c2 DESC\", v_LIMITSTR, \";\") ;\n" + "END;", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateTableTableOption1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE testtable (\n" + "testcolumn date default NULL\n" + ") ENGINE=InnoDB DEFAULT CHARSET=utf8 COMMENT='This is a test!!';";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            //        for(int i=0;i<createTableSqlStatement.getMySQLTableOptionList().size();i++){
            //            TMySQLCreateTableOption option = createTableSqlStatement.getMySQLTableOptionList().getElement(i);
            //            System.out.printf("name=%s, value=%s\n",option.getOptionName(),option.getOptionValue());
            //        }

            TMySQLCreateTableOption option0 = createTableSqlStatement.MySQLTableOptionList[0];
            TMySQLCreateTableOption option1 = createTableSqlStatement.MySQLTableOptionList[1];
            TMySQLCreateTableOption option2 = createTableSqlStatement.MySQLTableOptionList[2];

            //Assert.IsTrue(string.Equals(option0.OptionName, "ENGINE",StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(option0.tableOption == ETableOption.EO_ENGINE);
            Assert.IsTrue(string.Equals(option0.OptionValue,"InnoDB", StringComparison.CurrentCultureIgnoreCase));

            //Assert.IsTrue(string.Equals(option1.OptionName,"charset", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(option1.tableOption == ETableOption.EO_CHARACTER_SET);
            Assert.IsTrue(string.Equals(option1.OptionValue,"utf8", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(string.Equals(option1.characterSet.ToString(), "utf8", StringComparison.CurrentCultureIgnoreCase));

            //Assert.IsTrue(string.Equals(option2.OptionName,"COMMENT", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(option2.tableOption == ETableOption.EO_COMMENT);
            Assert.IsTrue(string.Equals(option2.OptionValue,"'This is a test!!'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateTableTableOption2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE total ( a INT NOT NULL AUTO_INCREMENT, message CHAR(20), INDEX(a)) ENGINE=MERGE UNION=(m1,m2)";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);

            TMySQLCreateTableOption option0 = createTableSqlStatement.MySQLTableOptionList[0];
            TMySQLCreateTableOption option1 = createTableSqlStatement.MySQLTableOptionList[1];

            Assert.IsTrue(option0.tableOption == ETableOption.EO_ENGINE);
            Assert.IsTrue(string.Equals(option0.OptionValue, "MERGE", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(option1.tableOption == ETableOption.EO_UNION);
            Assert.IsTrue(string.Equals(option1.ValueList.getObjectName(0).ToString(), "m1", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testCreateTableTableOptionCompress()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE t30 (    c1 INT ) engine `InnoDB`, COMPRESSION 'Zlib';";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);

            TMySQLCreateTableOption option0 = createTableSqlStatement.MySQLTableOptionList[0];
            TMySQLCreateTableOption option1 = createTableSqlStatement.MySQLTableOptionList[1];

            Assert.IsTrue(option0.tableOption == ETableOption.EO_ENGINE);
            Assert.IsTrue(string.Equals(option0.OptionValue, "`InnoDB`", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(option1.tableOption == ETableOption.EO_COMPRESSION);
            Assert.IsTrue(string.Equals(option1.OptionValue, "'Zlib'", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testCreateTableTableOptionConnection()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE t30 (    c1 INT ) CONNECTION 'connect_string';";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);

            TMySQLCreateTableOption option0 = createTableSqlStatement.MySQLTableOptionList[0];

            Assert.IsTrue(option0.tableOption == ETableOption.EO_CONNECTION);
            Assert.IsTrue(string.Equals(option0.OptionValue, "'connect_string'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateTableTableOptionEncryption()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE t30 (    c1 INT ) ENCRYPTION 'Y'";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);

            TMySQLCreateTableOption option0 = createTableSqlStatement.MySQLTableOptionList[0];

            Assert.IsTrue(option0.tableOption == ETableOption.EO_ENCRYPTION);
            Assert.IsTrue(string.Equals(option0.OptionValue, "'Y'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateTableTableOptionKeyBlockSize()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE t30 (    c1 INT ) KEY_BLOCK_SIZE 123";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);

            TMySQLCreateTableOption option0 = createTableSqlStatement.MySQLTableOptionList[0];

            Assert.IsTrue(option0.tableOption == ETableOption.EO_KEY_BLOCK_SIZE);
            Assert.IsTrue(string.Equals(option0.OptionValue, "123", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateTableTableOptionRowFormat()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE t30 (    c1 INT ) ROW_FORMAT COMPRESSED";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);

            TMySQLCreateTableOption option0 = createTableSqlStatement.MySQLTableOptionList[0];

            Assert.IsTrue(option0.tableOption == ETableOption.EO_ROW_FORMAT);
            Assert.IsTrue(string.Equals(option0.OptionValue, "COMPRESSED", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateTableTableOptionStatsAutoRecalc()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE t30 (    c1 INT ) STATS_AUTO_RECALC=1";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);

            TMySQLCreateTableOption option0 = createTableSqlStatement.MySQLTableOptionList[0];

            Assert.IsTrue(option0.tableOption == ETableOption.EO_STATS_AUTO_RECALC);
            Assert.IsTrue(string.Equals(option0.OptionValue, "1", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateTableTableOptionStatsPersistent()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE t30 (    c1 INT ) STATS_PERSISTENT=0;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);

            TMySQLCreateTableOption option0 = createTableSqlStatement.MySQLTableOptionList[0];

            Assert.IsTrue(option0.tableOption == ETableOption.EO_STATS_PERSISTENT);
            Assert.IsTrue(string.Equals(option0.OptionValue, "0", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateTableTableOptionStatsSamplePages()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE t30 (    c1 INT ) STATS_SAMPLE_PAGES=5;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);

            TMySQLCreateTableOption option0 = createTableSqlStatement.MySQLTableOptionList[0];

            Assert.IsTrue(option0.tableOption == ETableOption.EO_STATS_SAMPLE_PAGES);
            Assert.IsTrue(string.Equals(option0.OptionValue, "5", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateTableTableOptionCollate()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE t30 (    c1 INT ) DEFAULT CHARACTER SET latin2 COLLATE latin2_bin";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);

            TMySQLCreateTableOption option0 = createTableSqlStatement.MySQLTableOptionList[0];
            TMySQLCreateTableOption option1 = createTableSqlStatement.MySQLTableOptionList[1];

            Assert.IsTrue(option0.tableOption == ETableOption.EO_CHARACTER_SET);
            Assert.IsTrue(string.Equals(option0.characterSet.ToString(), "latin2", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(option1.tableOption == ETableOption.EO_COLLATE);
            Assert.IsTrue(string.Equals(option1.OptionValue, "latin2_bin", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testDatatype1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE test (\n" + "    column1 BOOLEAN\n" + ");";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition cd = createTable.ColumnList.getColumn(0);
            Assert.IsTrue(cd.Datatype.DataType == EDataType.boolean_t);
        }

        [TestMethod]
        public void testDatatype2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE test (\n" + "    column1 TINYINT\n" + ");";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition cd = createTable.ColumnList.getColumn(0);
            Assert.IsTrue(cd.Datatype.DataType == EDataType.tinyint_t);
        }

        [TestMethod]
        public void testDatatypeZeroFill()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE t (  c INT UNSIGNED ZEROFILL);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition cd = createTable.ColumnList.getColumn(0);
            Assert.IsTrue(cd.Datatype.DataType == EDataType.int_t);
            Assert.IsTrue(string.Equals(cd.Datatype.startToken.ToString(), "INT", StringComparison.CurrentCultureIgnoreCase));
            if (cd.Datatype.zeroFillToken != null)
            {
                Assert.IsTrue(string.Equals(cd.Datatype.zeroFillToken.ToString(), "zerofill", StringComparison.CurrentCultureIgnoreCase));
            }

            if (cd.Datatype.signedToken != null)
            {
                Assert.IsTrue(string.Equals(cd.Datatype.signedToken.ToString(), "UNSIGNED", StringComparison.CurrentCultureIgnoreCase));
            }
        }

        [TestMethod]
        public void testDatatype3()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE `test` (\n" + "  `id` int(11) NOT NULL AUTO_INCREMENT,\n" + "  `name` varchar(20) CHARACTER SET gbk NOT NULL,\n" + "  `create_time` timestamp NOT NULL DEFAULT '0000-00-00 00:00:00',\n" + "  `updated_time` timestamp NOT NULL DEFAULT '0000-00-00 00:00:00' ON UPDATE CURRENT_TIMESTAMP,\n" + "  PRIMARY KEY (`id`)\n" + ") ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition cd = createTable.ColumnList.getColumn(1);
            Assert.IsTrue(cd.Datatype.DataType == EDataType.varchar_t);
            Assert.IsTrue(string.Equals(cd.Datatype.CharsetName,"gbk",StringComparison.CurrentCultureIgnoreCase));

            TMySQLCreateTableOption option = createTable.MySQLTableOptionList[0];
            //            Assert.IsTrue(string.Equals(option.OptionName,"ENGINE",StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(option.tableOption == ETableOption.EO_ENGINE);
            Assert.IsTrue(string.Equals(option.OptionValue,"InnoDB",StringComparison.CurrentCultureIgnoreCase));

            option = createTable.MySQLTableOptionList[1];
            //Assert.IsTrue(string.Equals(option.OptionName,"AUTO_INCREMENT",StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(option.tableOption == ETableOption.EO_AUTO_INCREMENT);
            Assert.IsTrue(string.Equals(option.OptionValue,"2",StringComparison.CurrentCultureIgnoreCase));

            option = createTable.MySQLTableOptionList[2];
            //Assert.IsTrue(string.Equals(option.OptionName,"CHARSET",StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(option.tableOption == ETableOption.EO_CHARACTER_SET);
            Assert.IsTrue(string.Equals(option.OptionValue,"utf8",StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testColumnCollation()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE t14(    c1 CHAR(10) CHARACTER SET latin1 COLLATE latin1_german1_ci)";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition cd = createTable.ColumnList.getColumn(0);
            Assert.IsTrue(cd.Datatype.DataType == EDataType.char_t);
            Assert.IsTrue(string.Equals(cd.Datatype.CollationName, "latin1_german1_ci", StringComparison.CurrentCultureIgnoreCase));

            sqlparser.sqltext = @"CREATE TABLE `SQLDBM_EMPLOYEES`.`DEPT_MANAGER_TBL` 
                                    ( `EMP_NO` INT(4) unsigned zerofill NOT NULL DEFAULT 1000 COMMENT 'comment emp\_no' , 
                                    `DEPT_NO` CHAR(4) COLLATE latin1_german1_ci NOT NULL , 
                                    `TO_DATE` GEOMETRY NOT NULL , 
                                    `FROM_DATE` DATE NOT NULL , 
                                    PRIMARY KEY (`EMP_NO`, `DEPT_NO`), 
                                    SPATIAL KEY `Ind_86` (`TO_DATE`), 
                                    FULLTEXT KEY `Ind_129` (`DEPT_NO`) 
                                    ) COLLATE=utf8_unicode_ci;";
            Assert.IsTrue(sqlparser.parse() == 0);

             createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
             cd = createTable.ColumnList.getColumn(1);
            Assert.IsTrue(cd.Datatype.DataType == EDataType.char_t);
            Assert.IsTrue(string.Equals(cd.Datatype.CollationName, "latin1_german1_ci", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void TestColumnCollation2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);

            sqlparser.sqltext = @"
                                    CREATE TABLE `test`
                                    (
                                    `varchar(45)` enum('t','f') COLLATE latin1_german1_ci NOT NULL
                                    )";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable =
                (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition cd = createTable.ColumnList.getColumn(0);
            Console.WriteLine(cd.Datatype.CollationName);
            Assert.IsTrue(string.Equals(cd.Datatype.CollationName,
                                        "latin1_german1_ci", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testColumnStorage()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"CREATE TABLE t5
                                    (
                                        c1 CHAR(10),
                                        c2 CHAR(10) STORAGE DISK,
                                        c3 CHAR(10) STORAGE MEMORY,
                                        c4 CHAR(10) STORAGE DEFAULT
                                    );";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition cd0 = createTable.ColumnList.getColumn(0);
            Assert.IsTrue(cd0.columnStorage == EColumnStorage.csNotSpecified);

            TColumnDefinition cd = createTable.ColumnList.getColumn(1);
            Assert.IsTrue(cd.Datatype.DataType == EDataType.char_t);
            Assert.IsTrue(cd.columnStorage == EColumnStorage.csDisk);

        }

        [TestMethod]
        public void testColumnFormat()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"CREATE TABLE p30 (
                                      c1 INT,
                                      c2 INT COLUMN_FORMAT DYNAMIC
                                    );";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition cd = createTable.ColumnList.getColumn(1);
            Assert.IsTrue(cd.Datatype.DataType == EDataType.int_t);
            Assert.IsTrue(cd.columnFormat == EColumnFormat.cfDynamic);

        }

        [TestMethod]
        public void testColumnCalculateExpr()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"CREATE TABLE triangle (
                                      sidea DOUBLE,
                                      sideb DOUBLE,
                                      sidec DOUBLE AS (SQRT(sidea * sidea + sideb * sideb))
                                    );";

            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition cd = createTable.ColumnList.getColumn(2);
            Assert.IsTrue(cd.Datatype.DataType == EDataType.double_t);
            
            Assert.IsTrue(cd.calculatedExpr.ToString().Equals("SQRT(sidea * sidea + sideb * sideb)", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testColumnGeneratedStoreType()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"CREATE TABLE `Calculated_Column` ( 
                                    `virtual` CHAR(41) GENERATED ALWAYS AS (1-1) VIRTUAL , 
                                    `id` INT NOT NULL , 
                                    `stored` CHAR(41) GENERATED ALWAYS AS (1*2) STORED , 
                                    PRIMARY KEY (`id`) 
                                    );";

            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition cd = createTable.ColumnList.getColumn(0);
            Assert.IsTrue(cd.generatedColumnStoreType == EGeneratedColumnStoreType.gctVirtual);
            cd = createTable.ColumnList.getColumn(2);
            Assert.IsTrue(cd.generatedColumnStoreType == EGeneratedColumnStoreType.gctStored);
        }

        [TestMethod]
        public void testColumnComment()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"CREATE TABLE `world`.`Master2`
                                    (
                                     `MasterId` INT NOT NULL COMMENT 'MasterId',
                                     `MasterId2` INT NOT NULL,
                                     `MasterId3` INT NOT NULL,
                                     `Name` VARCHAR(40) NOT NULL,
                                     `Name2` VARCHAR(40) NOT NULL,
                                     `Name3` VARCHAR(40) NOT NULL,
                                      PRIMARY KEY (`MasterId` ASC, `MasterId2` DESC, `MasterId3`) COMMENT '!!!!!!',
                                      UNIQUE KEY `AK1_Master2_Name` (`Name` ASC, `Name2` DESC) COMMENT '!!!!!!'
                                    );";


            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition cd = createTable.ColumnList.getColumn(0);
            Assert.IsTrue(cd.Datatype.DataType == EDataType.int_t);
            Assert.IsTrue(cd.columnComment.ToString().Equals("'MasterId'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createTable.TableConstraints.size() == 2);
            TConstraint tableConstraint = createTable.TableConstraints.getConstraint(0);
            Assert.IsTrue(tableConstraint.Constraint_type == EConstraintType.primary_key);
            TMySQLIndexOption mySQLIndexOption = tableConstraint.IndexOptionList[0];
            Assert.IsTrue(mySQLIndexOption.indexOptionType == EIndexOptionType.iotComment);
            Assert.IsTrue(mySQLIndexOption.indexComment.ToString().Equals("'!!!!!!'", StringComparison.CurrentCultureIgnoreCase));
            //Assert.IsTrue(tableConstraint.constraintComment.ToString().Equals("'!!!!!!'", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testDate()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "select date from foo;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TResultColumn column = select.ResultColumnList.getResultColumn(0);
            Assert.IsTrue(column.Expr.ExpressionType == EExpressionType.simple_object_name_t);
            TObjectName objectName = column.Expr.ObjectOperand;
            Assert.IsTrue(objectName.ColumnToken.ToString().EndsWith("date", StringComparison.Ordinal));
        }

        [TestMethod]
        public void testDescribe()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "describe table1";
            Assert.IsTrue(sqlparser.parse() == 0);

            TDescribeStmt stmt = (TDescribeStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.TableName.ToString().Equals("table1", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testDropTable()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "DROP TABLE schema_n.table_n";
            Assert.IsTrue(sqlparser.parse() == 0);
            TDropTableSqlStatement dropTableSqlStatement = (TDropTableSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(string.Equals( dropTableSqlStatement.TableNameList.getObjectName(0).SchemaString,"schema_n", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(string.Equals(dropTableSqlStatement.TableNameList.getObjectName(0).ObjectString,"table_n", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(string.Equals(dropTableSqlStatement.TableNameList.getObjectName(0).ToString(),"schema_n.table_n", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testMultitables()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "SELECT * FROM t1 LEFT JOIN (t2, t3, t4) ON (t2.a=t1.a AND t3.b=t1.b AND t4.c=t1.c)";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TJoinItem joinItem = select.joins.getJoin(0).JoinItems.getJoinItem(0);
            Assert.IsTrue(joinItem.Kind == TBaseType.join_source_table);
            Assert.IsTrue(joinItem.Table.FromTableList.size() == 3);
            Assert.IsTrue(joinItem.Table.FromTableList.getFromTable(0).ToString().Equals("t2", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(joinItem.Table.FromTableList.getFromTable(1).ToString().Equals("t3", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(joinItem.Table.FromTableList.getFromTable(2).ToString().Equals("t4", StringComparison.CurrentCultureIgnoreCase));


        }

        [TestMethod]
        public virtual void testGetTable()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "                       SELECT * FROM (" + "                               SELECT tax_rates.* FROM" + "                                       wp_woocommerce_tax_rates as tax_rates" + "                               LEFT OUTER JOIN" + "                                       wp_woocommerce_tax_rate_locations as locations ON tax_rates.tax_rate_id = locations.tax_rate_id" + "                               LEFT OUTER JOIN" + "                                       wp_woocommerce_tax_rate_locations as locations2 ON tax_rates.tax_rate_id = locations2.tax_rate_id" + "                               WHERE" + "                                       tax_rate_country IN ( 'GB', '' )" + "                                       AND tax_rate_state IN ( '', '' )" + "                                       AND tax_rate_class = ''" + "                                       AND" + "                                       (" + "                                               (" + "                                                       locations.location_type IS NULL" + "                                               )" + "                                               OR" + "                                               (" + "                                                       locations.location_type = 'postcode'" + "                                                       AND locations.location_code IN ('*','')" + "                                                       AND locations2.location_type = 'city'" + "                                                       AND locations2.location_code = ''" + "                                               )" + "                                               OR" + "                                               (" + "                                                       locations.location_type = 'postcode'" + "                                                       AND locations.location_code IN ('*','')" + "                                                       AND 0 = (" + "                                                               SELECT COUNT(*) FROM wp_woocommerce_tax_rate_locations as sublocations" + "                                                               WHERE sublocations.location_type = 'city'" + "                                                               AND sublocations.tax_rate_id = tax_rates.tax_rate_id" + "                                                       )" + "                                               )" + "                                               OR" + "                                               (" + "                                                       locations.location_type = 'city'" + "                                                       AND locations.location_code = ''" + "                                                       AND 0 = (" + "                                                               SELECT COUNT(*) FROM wp_woocommerce_tax_rate_locations as sublocations" + "                                                               WHERE sublocations.location_type = 'postcode'" + "                                                               AND sublocations.tax_rate_id = tax_rates.tax_rate_id" + "                                                       )" + "                                               )" + "                                       )" + "                               GROUP BY" + "                                       tax_rate_id" + "                               ORDER BY" + "                                       tax_rate_priority, tax_rate_order" + "                       ) as ordered_taxes" + "                       GROUP BY" + "                               tax_rate_priority";

            int ret = sqlparser.parse();

            if (ret == 0)
            {
                StringBuilder tables = new StringBuilder();
                TTableList tableList = sqlparser.sqlstatements.get(0).tables;
                for (int j = 0; j < tableList.size(); j++)
                {
                    TTable table = tableList.getTable(j);
                    switch (table.TableType)
                    {
                        case  ETableSource.objectname:
                            //System.out.println("Table = " + table.getName());
                            break;
                        case ETableSource.subquery:
                            //System.out.println(table.getSubquery().toString());
                            break;
                        default:
                            break;
                    }
                }
            }

        }

        [TestMethod]
        public virtual void testIdentifierStartWithNumber()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "create table 9t(1a int)";
            Assert.IsTrue(sqlparser.parse() == 0);

        }

        [TestMethod]
        public void testIndexStorageType1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "ALTER TABLE jr_story ADD INDEX INK01_jr_story (story_no) using BTREE";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTableStatement = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            TAlterTableOption alterTableOption = alterTableStatement.AlterTableOptionList.getAlterTableOption(0);
            TMySQLIndexOption indexOption = alterTableOption.IndexOptionList[0];
            Assert.IsTrue(indexOption.indexOptionType == EIndexOptionType.iotIndexTypeBtree);
            //TMySQLIndexStorageType indexStorageType = indexOption.IndexStorageType;
            //Assert.IsTrue(indexStorageType.TypeToken.ToString().Equals("BTREE", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testIndexStorageType2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "ALTER TABLE jr_story ADD INDEX INK02_jr_story (story_no) using HASH";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTableStatement = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            TAlterTableOption alterTableOption = alterTableStatement.AlterTableOptionList.getAlterTableOption(0);
            TMySQLIndexOption indexOption = alterTableOption.IndexOptionList[0];
            Assert.IsTrue(indexOption.indexOptionType == EIndexOptionType.iotIndexTypeHash);
            //TMySQLIndexStorageType indexStorageType = indexOption.IndexStorageType;
            //Assert.IsTrue(indexStorageType.TypeToken.ToString().Equals("HASH", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testIndexType1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "ALTER TABLE jr_story ADD UNIQUE INDEX INK03_jr_story (story_no)";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTableStatement = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            TAlterTableOption alterTableOption = alterTableStatement.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(alterTableOption.MySQLIndexTypeToken.ToString().Equals("UNIQUE", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testIndexType2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "ALTER TABLE jr_story ADD FULLTEXT INDEX INK04_jr_story (story_no);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTableStatement = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            TAlterTableOption alterTableOption = alterTableStatement.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(alterTableOption.MySQLIndexTypeToken.ToString().Equals("FULLTEXT", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testIndexType3()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "ALTER TABLE jr_story ADD SPATIAL INDEX INK05_jr_story (story_no);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTableStatement = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            TAlterTableOption alterTableOption = alterTableStatement.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(alterTableOption.MySQLIndexTypeToken.ToString().Equals("SPATIAL", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testInsertIgnore()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "INSERT IGNORE INTO schema1.table1 (col1) VALUES('val1')";
            Assert.IsTrue(sqlparser.parse() == 0);
            TInsertSqlStatement insertSqlStatement = (TInsertSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(insertSqlStatement.Ignore.ToString().Equals("IGNORE", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testLeftRightShift()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "SELECT 12 << 3,12 >> 3 FROM whatever";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement selectSqlStatement = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TExpression expression = selectSqlStatement.ResultColumnList.getResultColumn(0).Expr;
            Assert.IsTrue(expression.ExpressionType == EExpressionType.left_shift_t);
            Assert.IsTrue(expression.LeftOperand.ToString().Equals("12", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(expression.RightOperand.ToString().Equals("3", StringComparison.CurrentCultureIgnoreCase));

            expression = selectSqlStatement.ResultColumnList.getResultColumn(1).Expr;
            Assert.IsTrue(expression.ExpressionType == EExpressionType.right_shift_t);
            Assert.IsTrue(expression.LeftOperand.ToString().Equals("12", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(expression.RightOperand.ToString().Equals("3", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testLimitClause1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "SELECT * FROM whatever LIMIT 40, 5";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement selectSqlStatement = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TLimitClause limitClause = selectSqlStatement.LimitClause;
            Assert.IsTrue(limitClause.Offset.ToString().Equals("40", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(limitClause.Row_count.ToString().Equals("5", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testLimitClause2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "SELECT * FROM whatever LIMIT 5";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement selectSqlStatement = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TLimitClause limitClause = selectSqlStatement.LimitClause;
            Assert.IsTrue(limitClause.Row_count.ToString().Equals("5", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testOnDuplicateUpdate()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "insert into mydb.mytable (c1,c2,c3) select 8,10,20 from mydb.t1 on duplicate key update c1=8";
            Assert.IsTrue(sqlparser.parse() == 0);

            TInsertSqlStatement insertSqlStatement = (TInsertSqlStatement)sqlparser.sqlstatements.get(0);
            TResultColumnList resultColumnList = insertSqlStatement.OnDuplicateKeyUpdate;
            TResultColumn resultColumn = resultColumnList.getResultColumn(0);

            Assert.IsTrue(resultColumn.Expr.ExpressionType == EExpressionType.assignment_t);
            Assert.IsTrue(resultColumn.Expr.LeftOperand.ToString().Equals("c1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(resultColumn.Expr.RightOperand.ToString().Equals("8", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testPrepareStmt()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "prepare stmnt from 'insert into Dept values(?,?,?,?,?)';";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMySQLPrepareStmt stmt = (TMySQLPrepareStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.StmtName.ToString().Equals("stmnt", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(string.Equals(stmt.PreparableStmtStr,"insert into Dept values(?,?,?,?,?)",StringComparison.CurrentCultureIgnoreCase));
            if (stmt.PreparableStmt != null)
            {
                Assert.IsTrue(stmt.PreparableStmt.sqlstatementtype == ESqlStatementType.sstinsert);
                TInsertSqlStatement insert = (TInsertSqlStatement)stmt.PreparableStmt;
                Assert.IsTrue(insert.TargetTable.ToString().Equals("Dept", StringComparison.CurrentCultureIgnoreCase));
                Assert.IsTrue(insert.Values.getMultiTarget(0).ColumnList.size() == 5);
            }
        }

        [TestMethod]
        public void testQuoteInLiteral1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "INSERT INTO umResponse (reservationId, timestamp, cmd, response) VALUES ('1234',       20120221171116,      'ADD',      '<HTML>\n" + "<HEAD>\n" + "<TITLE>500 Internal Server Error</TITLE>\n" + "</HEAD><BODY>\n" + "<H1>Internal Server Error</H1>\n" + "The server encountered an internal error or\n" + "misconfiguration and was unable to complete\n" + "your request.<P>\n" + "Please contact the server administrator to inform of the time the error occurred\n" + "and of anything you might have done that may have\n" + "caused the error.<P>\n" + "More information about this error may be available\n" + "in the server error log.<P>\n" + "<HR>\n" + "<ADDRESS>\n" + "Web Server at website.com\n" + "</ADDRESS>\n" + "</BODY>\n" + "</HTML>\n" + "\n" + "<!--\n" + "   - Unfortunately, Microsoft has added a clever new\n" + "   - \"feature\" to Internet Explorer. If the text of\n" + "   - an error\\'s message is \"too small\", specifically\n" + "   - less than 512 bytes, Internet Explorer returns\n" + "   - its own error message. You can turn that off,\n" + "   - but it\\'s pretty tricky to find switch called\n" + "   - \"smart error messages\". That means, of course,\n" + "   - that short error messages are censored by default.\n" + "   - IIS always returns error messages that are long\n" + "   - enough to make Internet Explorer happy. The\n" + "   - workaround is pretty simple: pad the error\n" + "   - message with a big comment like this to push it\n" + "   - over the five hundred and twelve bytes minimum.\n" + "   - Of course, that\\'s exactly what you\\'re reading\n" + "   - right now.\n" + "   -->\n" + "')";

            Assert.IsTrue(sqlparser.parse() == 0);
        }

        [TestMethod]
        public void testQuoteInLiteral2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "INSERT INTO umResponse (reservationId, timestamp, cmd, response)" + " VALUES ('1234'," + "       20120221171116," + "      'ADD'," + "      '<HTML>\n<HEAD>\n<TITLE>500 Internal Server Error</TITLE>\n</HEAD><BODY>\n<H1>Internal Server Error</H1>\nThe server encountered an internal error or\nmisconfiguration and was unable to complete\nyour request.<P>\nPlease contact the server administrator to inform of the time the error occurred\nand of anything you might have done that may have\ncaused the error.<P>\nMore information about this error may be available\nin the server error log.<P>\n<HR>\n<ADDRESS>\nWeb Server at website.com\n</ADDRESS>\n</BODY>\n</HTML>\n\n\n')";

            Assert.IsTrue(sqlparser.parse() == 0);
        }

        [TestMethod]
        public void testRollupModifier1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "SELECT year, SUM(profit) FROM sales GROUP BY year WITH ROLLUP;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement selectSqlStatement = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TGroupBy groupBy = selectSqlStatement.GroupByClause;
            Assert.IsTrue(groupBy.RollupModifier);
        }

        [TestMethod]
        public void testSet1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "SET sort_buffer_size=10000;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMySQLSet set = (TMySQLSet)sqlparser.sqlstatements.get(0);
            TExpression expression = set.Assignments[0].Expression;
            //System.out.println(expression.getExpressionType());
            Assert.IsTrue(expression.ExpressionType == EExpressionType.assignment_t);
            Assert.IsTrue(expression.LeftOperand.ToString().Equals("sort_buffer_size", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(expression.RightOperand.ToString().Equals("10000", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testSet2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "SET GLOBAL sort_buffer_size=1000000, SESSION sort_buffer_size=1000000;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMySQLSet set = (TMySQLSet)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(set.SetStatementType == ESetStatementType.variable);
            TSetAssignment assignment = set.Assignments[0];
            Assert.IsTrue(assignment.SetScope == ESetScope.global);
            assignment = set.Assignments[1];
            Assert.IsTrue(assignment.SetScope == ESetScope.session);

            TExpression expression = set.Assignments[0].Expression;
            Assert.IsTrue(expression.ExpressionType == EExpressionType.assignment_t);
            Assert.IsTrue(expression.LeftOperand.ToString().Equals("sort_buffer_size", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(expression.RightOperand.ToString().Equals("1000000", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testSet3()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "SET PASSWORD FOR 'bob'@'%.example.org' = PASSWORD('cleartext password');";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMySQLSet set = (TMySQLSet)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(set.SetStatementType == ESetStatementType.password);

            Assert.IsTrue(set.UserName.ToString().Equals("'bob'@'%.example.org'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(set.Password.ToString().Equals("'cleartext password'", StringComparison.CurrentCultureIgnoreCase));
        }
        [TestMethod]
        public void testSet4()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE PROCEDURE proc1(p1 INT)\n" + "    READS SQL DATA\n" + "    DETERMINISTIC\n" + "BEGIN\n" + "IF p1 = -1 THEN\n" + "  SET p1 = 10;\n" + "END IF;\n" + "END;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMySQLCreateProcedure createProcedure = (TMySQLCreateProcedure)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createProcedure.BodyStatements.size() == 1);
            Assert.IsTrue(createProcedure.BodyStatements.get(0).sqlstatementtype == ESqlStatementType.sstmysqlifstmt);
            TMySQLIfStmt ifStmt = (TMySQLIfStmt)createProcedure.BodyStatements.get(0);
            Assert.IsTrue(ifStmt.Condition.ToString().Equals("p1 = -1", StringComparison.CurrentCultureIgnoreCase));
            TMySQLSet set = (TMySQLSet)ifStmt.ThenStmts.get(0);
            Assert.IsTrue(set.SetStatementType == ESetStatementType.variable);
            TSetAssignment setAssignment = set.Assignments[0];
            TExpression expression = setAssignment.Expression;
            Assert.IsTrue(expression.ExpressionType == EExpressionType.assignment_t);
            Assert.IsTrue(expression.LeftOperand.ToString().Equals("p1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(expression.RightOperand.ToString().Equals("10", StringComparison.CurrentCultureIgnoreCase));
            //System.out.println(ifStmt.getThenStmts().size());
        }
        [TestMethod]
        public virtual void testSPParameter1()
        {
            //System.out.println(TBaseType.versionid);
            //System.out.println(TBaseType.releaseDate);
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE DEFINER=`sa`@`%` PROCEDURE `test2`(IN `in` VARCHAR(255), OUT `out` tinyint, INOUT `inout` tinyint) BEGIN SELECT city, phone FROM offices WHERE country = `in`; END";

            //      sqlparser.sqltext = "CREATE PROCEDURE `test2`(IN `in` VARCHAR(255), OUT `out` tinyint, INOUT `inout` tinyint) \n" +
            //              "BEGIN \n" +
            //              "SELECT city, phone FROM offices WHERE country = `in`; \n" +
            //              "END";

            int ret = sqlparser.parse();
            Assert.IsTrue(ret == 0);
            if (ret == 0)
            {
                TCustomSqlStatement sql = sqlparser.sqlstatements.get(0);
                //System.out.println("SQL Statement: " + sql.sqlstatementtype);
                Assert.IsTrue(sql.sqlstatementtype == ESqlStatementType.sstmysqlcreateprocedure);

                TMySQLCreateProcedure procedure = (TMySQLCreateProcedure)sql;
                //System.out.println("Procedure name: " + procedure.getProcedureName().toString());
                //System.out.println("Parameters:");

                TParameterDeclaration param = null;
                for (int i = 0; i < procedure.ParameterDeclarations.size(); i++)
                {
                    param = procedure.ParameterDeclarations.getParameterDeclarationItem(i);
                    // System.out.println("\tName:" + param.getParameterName().toString());
                    // System.out.println("\tDatatype:" + param.getDataType().toString());
                    // System.out.println("\tIN/OUT:" + param.getMode());
                }
            }
        }

        [TestMethod]
        public void testTimestampOnUpdate()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE TABLE test_table (pk_id bigint(10) unsigned NOT NULL auto_increment, \n" + "last_update timestamp NOT NULL default CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP, \n" + "PRIMARY KEY (pk_id)) ENGINE=InnoDB";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition columnDefinition = createTableSqlStatement.ColumnList.getColumn(1);
            Assert.IsTrue(columnDefinition.ColumnName.ToString().Equals("last_update", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(columnDefinition.Constraints.getConstraint(0).Constraint_type == EConstraintType.notnull);
            Assert.IsTrue(columnDefinition.Constraints.getConstraint(0).ToString().Equals("NOT NULL", StringComparison.CurrentCultureIgnoreCase));
            TConstraint constraint = columnDefinition.Constraints.getConstraint(1);
            Assert.IsTrue(constraint.Constraint_type == EConstraintType.default_value);
            Assert.IsTrue(constraint.DefaultExpression.ToString().Equals("CURRENT_TIMESTAMP", StringComparison.CurrentCultureIgnoreCase));
            TAutomaticProperty automaticProperty0 = constraint.AutomaticProperties[0];
            //        Assert.IsTrue(automaticProperty0.toString().equalsIgnoreCase("CURRENT_TIMESTAMP"));
            //        TAutomaticProperty automaticProperty1 = constraint.getAutomaticProperties().getElement(1);
            Assert.IsTrue(automaticProperty0.ToString().Equals("on update CURRENT_TIMESTAMP", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public  void testTruncateTable2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "truncate table a;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TTruncateStatement statement = (TTruncateStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(statement.TableName.ToString().Equals("a", StringComparison.CurrentCultureIgnoreCase));

            sqlparser.sqltext = "TRUNCATE  tbl_overridedescriptionrule;";
            Assert.IsTrue(sqlparser.parse() == 0);

            statement = (TTruncateStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(statement.TableName.ToString().Equals("tbl_overridedescriptionrule", StringComparison.CurrentCultureIgnoreCase));
            //Assert.IsTrue(statement.getTargetTable().toString().equalsIgnoreCase("a"));
        }

        [TestMethod]
        public  void testUpdateTargetTable1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "UPDATE table1 a\n" + "  INNER JOIN table2 b ON(a.field0=b.field0)\n" + "    SET a.field1 = 20120221\n" + "  WHERE b.field1 = 'D'\n" + "        AND b.field2 BETWEEN 20120217 and 20120219\n" + "        AND b.field3 != 0";
            Assert.IsTrue(sqlparser.parse() == 0);

            TUpdateSqlStatement updateSqlStatement = (TUpdateSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(string.Equals(updateSqlStatement.TargetTable.FullName,"table1",StringComparison.CurrentCultureIgnoreCase));
            TJoinItem joinItem = updateSqlStatement.joins.getJoin(0).JoinItems.getJoinItem(0);
            Assert.IsTrue(joinItem.JoinType.ToString().Equals("inner", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(joinItem.Table.ToString().Equals("table2", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(joinItem.OnCondition.ToString().Equals("(a.field0=b.field0)", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testUseDatabase()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "use dbname";
            Assert.IsTrue(sqlparser.parse() == 0);

            TUseDatabase use = (TUseDatabase)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(use.DatabaseName.ToString().Equals("dbname", StringComparison.CurrentCultureIgnoreCase));

        }



        [TestMethod]
        public void testCreateDatabaseCharacterSet()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE DATABASE db1 DEFAULT COLLATE latin1_german1_ci";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateDatabaseSqlStatement stmt = (TCreateDatabaseSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.DatabaseName.ToString().Equals("db1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(stmt.collationName.ToString().Equals("latin1_german1_ci", StringComparison.CurrentCultureIgnoreCase));

            sqlparser.sqltext = "CREATE SCHEMA db1 DEFAULT COLLATE latin1_german1_ci";
            Assert.IsTrue(sqlparser.parse() == 0);

            stmt = (TCreateDatabaseSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.DatabaseName.ToString().Equals("db1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(stmt.collationName.ToString().Equals("latin1_german1_ci", StringComparison.CurrentCultureIgnoreCase));

            sqlparser.sqltext = "CREATE SCHEMA `test2` COLLATE=utf16_bin;";
            Assert.IsTrue(sqlparser.parse() == 0);

            stmt = (TCreateDatabaseSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.DatabaseName.ToString().Equals("`test2`", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(stmt.collationName.ToString().Equals("utf16_bin", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateIndexComment()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE INDEX `Idx5` ON `Table` (`Name`) COMMENT 'test comment';";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateIndexSqlStatement createIndex = (TCreateIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createIndex.IndexName.ToString().Equals("`Idx5`", StringComparison.CurrentCultureIgnoreCase));
            List <TMySQLIndexOption> indexOptions = createIndex.IndexOptionList;
            Assert.IsTrue(indexOptions.Count == 1);
            TMySQLIndexOption indexOption = indexOptions[0];
            Assert.IsTrue(indexOption.indexOptionType == EIndexOptionType.iotComment);
            Assert.IsTrue(indexOption.indexComment.ToString().Equals("'test comment'", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testCreateIndexKeyBlockSize()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE INDEX `Idx5` ON `Table` (`Name`) KEY_BLOCK_SIZE = 123;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateIndexSqlStatement createIndex = (TCreateIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createIndex.IndexName.ToString().Equals("`Idx5`", StringComparison.CurrentCultureIgnoreCase));
            List<TMySQLIndexOption> indexOptions = createIndex.IndexOptionList;
            Assert.IsTrue(indexOptions.Count == 1);
            TMySQLIndexOption indexOption = indexOptions[0];
            Assert.IsTrue(indexOption.indexOptionType == EIndexOptionType.iotKeyBlockSize);
            Assert.IsTrue(indexOption.keyBlockSize.ToString().Equals("123", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testCreateIndexWithParser()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE FULLTEXT INDEX `Idx5` ON `Table` (`Name`) WITH PARSER `Parser1`;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateIndexSqlStatement createIndex = (TCreateIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createIndex.IndexName.ToString().Equals("`Idx5`", StringComparison.CurrentCultureIgnoreCase));
            List<TMySQLIndexOption> indexOptions = createIndex.IndexOptionList;
            Assert.IsTrue(indexOptions.Count == 1);
            TMySQLIndexOption indexOption = indexOptions[0];
            Assert.IsTrue(indexOption.indexOptionType == EIndexOptionType.iotWithParser);
            Assert.IsTrue(indexOption.parserName.Equals("`Parser1`", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testCreateIndexUsingBtree()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE INDEX `Idx5` ON `Table` (`Name`) USING BTREE;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateIndexSqlStatement createIndex = (TCreateIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createIndex.IndexName.ToString().Equals("`Idx5`", StringComparison.CurrentCultureIgnoreCase));
            List<TMySQLIndexOption> indexOptions = createIndex.IndexOptionList;
            Assert.IsTrue(indexOptions.Count == 1);
            TMySQLIndexOption indexOption = indexOptions[0];
            Assert.IsTrue(indexOption.indexOptionType == EIndexOptionType.iotIndexTypeBtree);

        }

        [TestMethod]
        public void testCreateIndexUsingHash()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "CREATE INDEX `Idx5` ON `Table` (`Name`) USING HASH";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateIndexSqlStatement createIndex = (TCreateIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createIndex.IndexName.ToString().Equals("`Idx5`", StringComparison.CurrentCultureIgnoreCase));
            List<TMySQLIndexOption> indexOptions = createIndex.IndexOptionList;
            Assert.IsTrue(indexOptions.Count == 1);
            TMySQLIndexOption indexOption = indexOptions[0];
            Assert.IsTrue(indexOption.indexOptionType == EIndexOptionType.iotIndexTypeHash);

        }

        [TestMethod]
        public void testCreateTableColumnComment()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"CREATE TABLE t28 (
                                     c1 varchar(5),
                                     c2 varchar(5) DEFAULT ""abc"",
                                     c3 varchar(5) DEFAULT 'abc',
                                     c4 INT COMMENT 'the comment'
                                    ); ";

            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition column = createTableSqlStatement.ColumnList.getColumn(3);

            Assert.IsTrue(string.Equals(column.ColumnName.ToString(), "c4", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(string.Equals(column.columnComment.ToString(), "'the comment'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateTableColumnKeyConstraint()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"CREATE TABLE `table`
                                    (
                                        Id int KEY,
                                        c1 int UNIQUE
                                    ); ";

            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition column0 = createTableSqlStatement.ColumnList.getColumn(0);
            Assert.IsTrue(string.Equals(column0.ColumnName.ToString(), "Id", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(column0.Constraints.getConstraint(0).Constraint_type == EConstraintType.key);
            TColumnDefinition column1 = createTableSqlStatement.ColumnList.getColumn(1);
            Assert.IsTrue(string.Equals(column1.ColumnName.ToString(), "c1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(column1.Constraints.getConstraint(0).Constraint_type == EConstraintType.unique);
        }

        [TestMethod]
        public void testTableConstraint()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"CREATE TABLE `TableWithAllOptions`
                                    (
                                     `PkId1` INT unsigned zerofill NOT NULL AUTO_INCREMENT ,
                                     `PkId2` INT NOT NULL ,
                                     `Name` VARCHAR(45) NOT NULL ,
                                     `NullableCol` VARCHAR(45) ,
                                     `IxCol1` BIGINT NOT NULL ,
                                     `IxCol2` BIGINT NOT NULL ,
                                     `Age` INT NOT NULL ,
                                     `Address` VARCHAR(55) NOT NULL ,
                                     `Email` VARCHAR(100) NOT NULL ,
                                     `Geolocation` GEOMETRY NOT NULL ,

                                    PRIMARY KEY (`PkId1`, `PkId2`),
                                    UNIQUE KEY `UX_Email` (`Email`) KEY_BLOCK_SIZE=21 USING HASH,
                                    KEY `IX_MultipleCols` (`IxCol1`, `IxCol2`) KEY_BLOCK_SIZE=11 USING BTREE,
                                    FULLTEXT KEY `FX_Address` (`Address`),
                                    KEY `IX_Age_FastSearch` (`Age`),
                                    SPATIAL KEY `SX_Geolocation` (`Geolocation`)
                                    );";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTable = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createTable.TableConstraints.size() == 6);
            TConstraint uniqueKey = createTable.TableConstraints[1];
            Assert.IsTrue(uniqueKey.Constraint_type == EConstraintType.unique);
            List<TMySQLIndexOption> mySQLIndexOptions = uniqueKey.IndexOptionList;
            Assert.IsTrue(uniqueKey.IndexOptionList.Count == 2);
            TMySQLIndexOption mySQLIndexOption = uniqueKey.IndexOptionList[0];
            Assert.IsTrue(mySQLIndexOption.indexOptionType == EIndexOptionType.iotKeyBlockSize);
            Assert.IsTrue(string.Equals(mySQLIndexOption.keyBlockSize.ToString(), "21", StringComparison.CurrentCultureIgnoreCase));
            mySQLIndexOption = uniqueKey.IndexOptionList[1];
            Assert.IsTrue(mySQLIndexOption.indexOptionType == EIndexOptionType.iotIndexTypeHash);

            TConstraint fulltextKey = createTable.TableConstraints[3];
            Assert.IsTrue(fulltextKey.Constraint_type == EConstraintType.fulltextKey);
        }

        [TestMethod]
        public void testAlterTableAlterColumn()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = "ALTER TABLE `world`.`t1` ALTER `c1` SET DEFAULT 'abc';";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTableStatement = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            TAlterTableOption alterTableOption = alterTableStatement.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(alterTableOption.OptionType == EAlterTableOptionType.AlterColumn);
            Assert.IsTrue(alterTableOption.DefaultExpr.ToString().Equals("'abc'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testAlterTableAddForeignKey()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"ALTER TABLE `world`.`Detail2`
                                    ADD CONSTRAINT `FK_Detail2_Master2` FOREIGN KEY (`MasterId`, `MasterId2`, `MasterId3`)
                                        REFERENCES `world`.`Master2` (`MasterId`, `MasterId2`, `MasterId3`)";

            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTableStatement = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            TAlterTableOption alterTableOption = alterTableStatement.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(alterTableOption.OptionType == EAlterTableOptionType.AddConstraintFK);
            Assert.IsTrue(alterTableOption.ConstraintName.ToString().Equals("`FK_Detail2_Master2`", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(alterTableOption.ColumnNameList.getObjectName(0).ToString().Equals("`MasterId`", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(alterTableOption.ReferencedObjectName.ToString().Equals("`world`.`Master2`", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(alterTableOption.ReferencedColumnList.getObjectName(0).ToString().Equals("`MasterId`", StringComparison.CurrentCultureIgnoreCase));
        }

    }
}
