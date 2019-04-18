using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.IO;
using gudusoft.gsqlparser.stmt;

namespace gudusoft.gsqlparser.test
{
    /// <summary>
    /// UnitTestOracle 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTestTeradata
    {
        TGSqlParser parser;

        public UnitTestTeradata()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
            parser = new TGSqlParser(EDbVendor.dbvteradata);
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
        public void TestTeradataFiles()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"teradata\verified\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public void TestTeradataFiles2()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"new_dotnet\teradata\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public void TestTeradataRenameTable()
        {
            parser.sqltext = "RENAME TABLE renA TO renX;";
            int ret = parser.parse();
            Assert.IsTrue(ret == 0, parser.Errormessage);
            Assert.IsTrue(parser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstrename);
            TRenameStmt renameTable = (TRenameStmt)parser.sqlstatements.get(0);
            Assert.IsTrue(renameTable.NewName.ToString().Equals("renX", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(renameTable.OldName.ToString().Equals("renA", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void TestTeradataCommentOn()
        {
            parser.sqltext = "comment on table db1.TableA is 'Hello world';";
            int ret = parser.parse();
            Assert.IsTrue(ret == 0, parser.Errormessage);
            Assert.IsTrue(parser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstCommentOn);
            TCommentOnSqlStmt commentOn = (TCommentOnSqlStmt)parser.sqlstatements.get(0);
            Assert.IsTrue(commentOn.objectName.ToString().Equals("db1.TableA", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(commentOn.dbObjectType == EDbObjectType.table);
            Assert.IsTrue(commentOn.objectName.DbObjectType == EDbObjectType.table);
        }
    }
}
