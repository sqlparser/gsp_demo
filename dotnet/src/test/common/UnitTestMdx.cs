using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.IO;
using gudusoft.gsqlparser.stmt.mdx;
using gudusoft.gsqlparser.nodes.mdx;

namespace gudusoft.gsqlparser.test
{
    /// <summary>
    /// UnitTestOracle 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTestMDX
    {
        TGSqlParser parser;

        public UnitTestMDX()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
            parser = new TGSqlParser(EDbVendor.dbvmdx);
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
            parser.sqltext = @"WITH 
   MEMBER [Measures].[Special Discount] AS
   [Measures].[Discount Amount] * 1.5
SELECT 
   [Measures].[Special Discount] on COLUMNS,
   NON EMPTY [Product].[Product].MEMBERS  ON Rows
FROM [Adventure Works]";
            int ret = parser.parse();
            Assert.IsTrue(ret == 0, parser.Errormessage);
        }

        [TestMethod]
        public void TestMdxFiles()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"mdx\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }
        [TestMethod]
        public void testMdxTokenlizer1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmdx);
            sqlparser.sqltext = "SELECT Measures.MEMBERS ON COLUMNS,\n" + "\n" + "Product.Style.CHILDREN ON ROWS\n" + "\n" + "FROM [Adventure Works] \n" + ";";
            sqlparser.tokenizeSqltext();
            string outstr = "";
            for (int i = 0; i < sqlparser.sourcetokenlist.size(); i++)
            {
                TSourceToken st = sqlparser.sourcetokenlist.get(i);
                //System.out.printf("%s,type:%s,code:%d\n",st.tokentype == ETokenType.ttreturn?"linebreak":st.toString(),st.tokentype,st.tokencode);
                outstr += string.Format("{0},type:{1},code:{2:D}\n", st.tokentype == ETokenType.ttreturn ? "linebreak" : st.ToString(), st.tokentype, st.tokencode);
            }
            // System.out.println(outstr);
            Assert.IsTrue(outstr.Trim().Equals("SELECT,type:ttkeyword,code:301\n" + " ,type:ttwhitespace,code:259\n" + "Measures,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "MEMBERS,type:ttkeyword,code:630\n" + " ,type:ttwhitespace,code:259\n" + "ON,type:ttkeyword,code:323\n" + " ,type:ttwhitespace,code:259\n" + "COLUMNS,type:ttkeyword,code:559\n" + ",,type:ttcomma,code:44\n" + "linebreak,type:ttreturn,code:260\n" + "Product,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "Style,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "CHILDREN,type:ttkeyword,code:555\n" + " ,type:ttwhitespace,code:259\n" + "ON,type:ttkeyword,code:323\n" + " ,type:ttwhitespace,code:259\n" + "ROWS,type:ttkeyword,code:661\n" + "linebreak,type:ttreturn,code:260\n" + "FROM,type:ttkeyword,code:329\n" + " ,type:ttwhitespace,code:259\n" + "[Adventure Works],type:ttidentifier,code:282\n" + " ,type:ttwhitespace,code:259\n" + "linebreak,type:ttreturn,code:260\n" + ";,type:ttsemicolon,code:59", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testMdxTokenlizer2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmdx);
            sqlparser.sqltext = "SELECT Measures.MEMBERS ON COLUMNS,\n" + "Product.Style.CHILDREN ON ROWS\n" + "FROM [Total Profit [Domestic]]]\t\n" + ";";
            sqlparser.tokenizeSqltext();
            string outstr = "";
            for (int i = 0; i < sqlparser.sourcetokenlist.size(); i++)
            {
                TSourceToken st = sqlparser.sourcetokenlist.get(i);
                outstr += string.Format("{0},type:{1},code:{2:D}\n", st.tokentype == ETokenType.ttreturn ? "linebreak" : st.ToString(), st.tokentype, st.tokencode);
            }
            // System.out.println(outstr);
            Assert.IsTrue(outstr.Trim().Equals("SELECT,type:ttkeyword,code:301\n" + " ,type:ttwhitespace,code:259\n" + "Measures,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "MEMBERS,type:ttkeyword,code:630\n" + " ,type:ttwhitespace,code:259\n" + "ON,type:ttkeyword,code:323\n" + " ,type:ttwhitespace,code:259\n" + "COLUMNS,type:ttkeyword,code:559\n" + ",,type:ttcomma,code:44\n" + "linebreak,type:ttreturn,code:260\n" + "Product,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "Style,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "CHILDREN,type:ttkeyword,code:555\n" + " ,type:ttwhitespace,code:259\n" + "ON,type:ttkeyword,code:323\n" + " ,type:ttwhitespace,code:259\n" + "ROWS,type:ttkeyword,code:661\n" + "linebreak,type:ttreturn,code:260\n" + "FROM,type:ttkeyword,code:329\n" + " ,type:ttwhitespace,code:259\n" + "[Total Profit [Domestic]]],type:ttidentifier,code:282\n" + "\t,type:ttwhitespace,code:259\n" + "linebreak,type:ttreturn,code:260\n" + ";,type:ttsemicolon,code:59", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testMdxTokenlizer3()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmdx);
            sqlparser.sqltext = "SELECT Measures.MEMBERS^ ON COLUMNS,\n" + "Product.Style.CHILDREN ON ROWS\n" + "FROM [Total Profit [Domestic]]]\t\n" + ";";
            sqlparser.tokenizeSqltext();
            string outstr = "";
            for (int i = 0; i < sqlparser.sourcetokenlist.size(); i++)
            {
                TSourceToken st = sqlparser.sourcetokenlist.get(i);
                outstr += string.Format("{0},type:{1},code:{2:D}\n", st.tokentype == ETokenType.ttreturn ? "linebreak" : st.ToString(), st.tokentype, st.tokencode);
            }
            // System.out.println(outstr);
            Assert.IsTrue(outstr.Trim().Equals("SELECT,type:ttkeyword,code:301\n" + " ,type:ttwhitespace,code:259\n" + "Measures,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "MEMBERS,type:ttkeyword,code:630\n" + "^,type:ttcaret,code:94\n" + " ,type:ttwhitespace,code:259\n" + "ON,type:ttkeyword,code:323\n" + " ,type:ttwhitespace,code:259\n" + "COLUMNS,type:ttkeyword,code:559\n" + ",,type:ttcomma,code:44\n" + "linebreak,type:ttreturn,code:260\n" + "Product,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "Style,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "CHILDREN,type:ttkeyword,code:555\n" + " ,type:ttwhitespace,code:259\n" + "ON,type:ttkeyword,code:323\n" + " ,type:ttwhitespace,code:259\n" + "ROWS,type:ttkeyword,code:661\n" + "linebreak,type:ttreturn,code:260\n" + "FROM,type:ttkeyword,code:329\n" + " ,type:ttwhitespace,code:259\n" + "[Total Profit [Domestic]]],type:ttidentifier,code:282\n" + "\t,type:ttwhitespace,code:259\n" + "linebreak,type:ttreturn,code:260\n" + ";,type:ttsemicolon,code:59", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testMdxTokenlizer4()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmdx);
            sqlparser.sqltext = "SELECT Measures.MEMBERS ON COLUMNS,\n" + "Product.Style.CHILDREN ON ROWS\n" + "FROM [Adventure Works] \n" + "where Product.Style.CHILDREN >= 1\n" + ";";

            sqlparser.tokenizeSqltext();
            string outstr = "";
            for (int i = 0; i < sqlparser.sourcetokenlist.size(); i++)
            {
                TSourceToken st = sqlparser.sourcetokenlist.get(i);
                outstr += string.Format("{0},type:{1},code:{2:D}\n", st.tokentype == ETokenType.ttreturn ? "linebreak" : st.ToString(), st.tokentype, st.tokencode);
            }
            // System.out.println(outstr);
            Assert.IsTrue(outstr.Trim().Equals("SELECT,type:ttkeyword,code:301\n" + " ,type:ttwhitespace,code:259\n" + "Measures,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "MEMBERS,type:ttkeyword,code:630\n" + " ,type:ttwhitespace,code:259\n" + "ON,type:ttkeyword,code:323\n" + " ,type:ttwhitespace,code:259\n" + "COLUMNS,type:ttkeyword,code:559\n" + ",,type:ttcomma,code:44\n" + "linebreak,type:ttreturn,code:260\n" + "Product,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "Style,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "CHILDREN,type:ttkeyword,code:555\n" + " ,type:ttwhitespace,code:259\n" + "ON,type:ttkeyword,code:323\n" + " ,type:ttwhitespace,code:259\n" + "ROWS,type:ttkeyword,code:661\n" + "linebreak,type:ttreturn,code:260\n" + "FROM,type:ttkeyword,code:329\n" + " ,type:ttwhitespace,code:259\n" + "[Adventure Works],type:ttidentifier,code:282\n" + " ,type:ttwhitespace,code:259\n" + "linebreak,type:ttreturn,code:260\n" + "where,type:ttkeyword,code:317\n" + " ,type:ttwhitespace,code:259\n" + "Product,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "Style,type:ttidentifier,code:264\n" + ".,type:ttperiod,code:46\n" + "CHILDREN,type:ttkeyword,code:555\n" + " ,type:ttwhitespace,code:259\n" + ">=,type:ttmulticharoperator,code:293\n" + " ,type:ttwhitespace,code:259\n" + "1,type:ttnumber,code:263\n" + "linebreak,type:ttreturn,code:260\n" + ";,type:ttsemicolon,code:59", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testMdxTokenlizer5()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmdx);
            sqlparser.sqltext = "// This member returns the gross profit margin for product types\n" + "// and reseller types crossjoined by year.\n" + "SELECT \n" + "    [Date].[Calendar Time].[Calendar Year].Members *\n" + "      [Reseller].[Reseller Type].Children ON 0,\n" + "    [Product].[Category].[Category].Members ON 1\n" + "FROM // Select from the Adventure Works cube.\n" + "    [Adventure Works]\n" + "WHERE\n" + "    [Measures].[Gross Profit Margin]\n" + ";";

            sqlparser.tokenizeSqltext();
            string outstr = "";
            for (int i = 0; i < sqlparser.sourcetokenlist.size(); i++)
            {
                TSourceToken st = sqlparser.sourcetokenlist.get(i);
                outstr += string.Format("{0},type:{1},code:{2:D}\n", st.tokentype == ETokenType.ttreturn ? "linebreak" : st.ToString(), st.tokentype, st.tokencode);
            }
            // System.out.println(outstr);
            Assert.IsTrue(outstr.Trim().Equals("// This member returns the gross profit margin for product types,type:ttCPPComment,code:258\n" + "linebreak,type:ttreturn,code:260\n" + "// and reseller types crossjoined by year.,type:ttCPPComment,code:258\n" + "linebreak,type:ttreturn,code:260\n" + "SELECT,type:ttkeyword,code:301\n" + " ,type:ttwhitespace,code:259\n" + "linebreak,type:ttreturn,code:260\n" + "[Date],type:ttidentifier,code:282\n" + ".,type:ttperiod,code:46\n" + "[Calendar Time],type:ttidentifier,code:282\n" + ".,type:ttperiod,code:46\n" + "[Calendar Year],type:ttidentifier,code:282\n" + ".,type:ttperiod,code:46\n" + "Members,type:ttkeyword,code:630\n" + " ,type:ttwhitespace,code:259\n" + "*,type:ttasterisk,code:42\n" + "linebreak,type:ttreturn,code:260\n" + "[Reseller],type:ttidentifier,code:282\n" + ".,type:ttperiod,code:46\n" + "[Reseller Type],type:ttidentifier,code:282\n" + ".,type:ttperiod,code:46\n" + "Children,type:ttkeyword,code:555\n" + " ,type:ttwhitespace,code:259\n" + "ON,type:ttkeyword,code:323\n" + " ,type:ttwhitespace,code:259\n" + "0,type:ttnumber,code:263\n" + ",,type:ttcomma,code:44\n" + "linebreak,type:ttreturn,code:260\n" + "[Product],type:ttidentifier,code:282\n" + ".,type:ttperiod,code:46\n" + "[Category],type:ttidentifier,code:282\n" + ".,type:ttperiod,code:46\n" + "[Category],type:ttidentifier,code:282\n" + ".,type:ttperiod,code:46\n" + "Members,type:ttkeyword,code:630\n" + " ,type:ttwhitespace,code:259\n" + "ON,type:ttkeyword,code:323\n" + " ,type:ttwhitespace,code:259\n" + "1,type:ttnumber,code:263\n" + "linebreak,type:ttreturn,code:260\n" + "FROM,type:ttkeyword,code:329\n" + " ,type:ttwhitespace,code:259\n" + "// Select from the Adventure Works cube.,type:ttCPPComment,code:258\n" + "linebreak,type:ttreturn,code:260\n" + "[Adventure Works],type:ttidentifier,code:282\n" + "linebreak,type:ttreturn,code:260\n" + "WHERE,type:ttkeyword,code:317\n" + "linebreak,type:ttreturn,code:260\n" + "[Measures],type:ttidentifier,code:282\n" + ".,type:ttperiod,code:46\n" + "[Gross Profit Margin],type:ttidentifier,code:282\n" + "linebreak,type:ttreturn,code:260\n" + ";,type:ttsemicolon,code:59", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testTokenlizer6()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmdx);
            sqlparser.sqltext = "SELECT \n" + "[Date].[Calendar Year].&[2004] ON 0\n" + "FROM [Adventure Works];";

            sqlparser.tokenizeSqltext();
            string outstr = "";
            for (int i = 0; i < sqlparser.sourcetokenlist.size(); i++)
            {
                TSourceToken st = sqlparser.sourcetokenlist.get(i);
                outstr += string.Format("{0},type:{1},code:{2:D}\n", st.tokentype == ETokenType.ttreturn ? "linebreak" : st.ToString(), st.tokentype, st.tokencode);
            }
            // System.out.println(outstr);
            Assert.IsTrue(outstr.Trim().Equals("SELECT,type:ttkeyword,code:301\n" + " ,type:ttwhitespace,code:259\n" + "linebreak,type:ttreturn,code:260\n" + "[Date],type:ttidentifier,code:282\n" + ".,type:ttperiod,code:46\n" + "[Calendar Year],type:ttidentifier,code:282\n" + ".,type:ttperiod,code:46\n" + "&[2004],type:ttidentifier,code:285\n" + " ,type:ttwhitespace,code:259\n" + "ON,type:ttkeyword,code:323\n" + " ,type:ttwhitespace,code:259\n" + "0,type:ttnumber,code:263\n" + "linebreak,type:ttreturn,code:260\n" + "FROM,type:ttkeyword,code:329\n" + " ,type:ttwhitespace,code:259\n" + "[Adventure Works],type:ttidentifier,code:282\n" + ";,type:ttsemicolon,code:59", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testTokenlizer7()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmdx);
            sqlparser.sqltext = "SELECT \n" + "[x].&foo&[1]&bar.[y] ON 0\n" + "FROM [Adventure Works];";

            sqlparser.tokenizeSqltext();
            string outstr = "";
            for (int i = 0; i < sqlparser.sourcetokenlist.size(); i++)
            {
                TSourceToken st = sqlparser.sourcetokenlist.get(i);
                outstr += string.Format("{0},type:{1},code:{2:D}\n", st.tokentype == ETokenType.ttreturn ? "linebreak" : st.ToString(), st.tokentype, st.tokencode);
            }
            // System.out.println(outstr);
            Assert.IsTrue(outstr.Trim().Equals("SELECT,type:ttkeyword,code:301\n" + " ,type:ttwhitespace,code:259\n" + "linebreak,type:ttreturn,code:260\n" + "[x],type:ttidentifier,code:282\n" + ".,type:ttperiod,code:46\n" + "&foo,type:ttidentifier,code:286\n" + "&[1],type:ttidentifier,code:285\n" + "&bar,type:ttidentifier,code:286\n" + ".,type:ttperiod,code:46\n" + "[y],type:ttidentifier,code:282\n" + " ,type:ttwhitespace,code:259\n" + "ON,type:ttkeyword,code:323\n" + " ,type:ttwhitespace,code:259\n" + "0,type:ttnumber,code:263\n" + "linebreak,type:ttreturn,code:260\n" + "FROM,type:ttkeyword,code:329\n" + " ,type:ttwhitespace,code:259\n" + "[Adventure Works],type:ttidentifier,code:282\n" + ";,type:ttsemicolon,code:59", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testRawStatements()
        {
            
            string rootdir = UnitTestCommon.BASE_SQL_DIR() + @"mdx\";
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmdx);

            sqlparser.sqlfilename = rootdir + "case.sql";
            sqlparser.getrawsqlstatements();
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstmdxselect);
            Assert.IsTrue(sqlparser.sqlstatements.get(1).sqlstatementtype == ESqlStatementType.sstmdxselect);

            sqlparser.sqlfilename = rootdir + "createmember.sql";
            sqlparser.getrawsqlstatements();
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstmdxcreatemember);

            sqlparser.sqlfilename = rootdir + "createsessioncube.sql";
            sqlparser.getrawsqlstatements();
            Assert.IsTrue(sqlparser.sqlstatements.size() == 1);
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstmdxcreatesessioncube);

            sqlparser.sqlfilename = rootdir + "createsubcube.sql";
            sqlparser.getrawsqlstatements();
            Assert.IsTrue(sqlparser.sqlstatements.size() == 6);
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstmdxcreatesubcube);
            Assert.IsTrue(sqlparser.sqlstatements.get(1).sqlstatementtype == ESqlStatementType.sstmdxselect);
            Assert.IsTrue(sqlparser.sqlstatements.get(2).sqlstatementtype == ESqlStatementType.sstmdxcreatesubcube);
            Assert.IsTrue(sqlparser.sqlstatements.get(3).sqlstatementtype == ESqlStatementType.sstmdxselect);
            Assert.IsTrue(sqlparser.sqlstatements.get(4).sqlstatementtype == ESqlStatementType.sstmdxcreatesubcube);
            Assert.IsTrue(sqlparser.sqlstatements.get(5).sqlstatementtype == ESqlStatementType.sstmdxselect);

            sqlparser.sqlfilename = rootdir + "drillthrough.sql";
            sqlparser.getrawsqlstatements();
            Assert.IsTrue(sqlparser.sqlstatements.size() == 1);
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstmdxdrillthrough);

            sqlparser.sqlfilename = rootdir + "scope.sql";
            sqlparser.getrawsqlstatements();
            Assert.IsTrue(sqlparser.sqlstatements.size() == 2);
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstmdxscope);
            Assert.IsTrue(sqlparser.sqlstatements.get(1).sqlstatementtype == ESqlStatementType.sstmdxscope);
        }

        [TestMethod]
        public void testIIF()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmdx);
            sqlparser.sqltext = "select \n" + "iif([Measures].[Orders Count]<>0, \n" + "    [Measures].[Unit Price BAD],\n" + "    0) on 1\n" + "from t";
            int i = sqlparser.parse();
            Assert.IsTrue(i == 0);

            TMdxSelect select = (TMdxSelect)sqlparser.sqlstatements.get(0);
            TMdxAxisNode axisNode = select.Axes[0];
            TMdxFunctionNode functionNode = (TMdxFunctionNode)axisNode.ExpNode;
            Assert.IsTrue(functionNode.Arguments.Count == 3);
            for (int j = 0; j < functionNode.Arguments.Count; j++)
            {
                TMdxExpNode expNode = functionNode.Arguments[j];
                // System.out.println(expNode.toString());
            }

        }
        [TestMethod]
        public void testCreateMember()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmdx);
            sqlparser.sqltext = "CREATE MEMBER CURRENTCUBE.Measures.[_Internet Current Quarter Sales Performance Status] \n" + "AS 'Case When IsEmpty(KpiValue(\"Internet Current Quarter Sales Performance\")) Then Null When KpiValue(\"Internet Current Quarter Sales Performance\") < 1 Then -1 When KpiValue(\"Internet Current Quarter Sales Performance\") >= 1 And KpiValue(\"Internet Current Quarter Sales Performance\") < 1.07 Then 0 Else 1 End', \n" + "ASSOCIATED_MEASURE_GROUP = 'Internet Sales';";
            int i = sqlparser.parse();
            Assert.IsTrue(i == 0);
            TMdxCreateMember createMember = (TMdxCreateMember)sqlparser.sqlstatements.get(0);
            TMdxWithMemberNode withMemberNode = createMember.Specification;

            string newQuery = "select " + TBaseType.getStringInsideLiteral(withMemberNode.ExprNode.ToString()) + " on 1 from t";
            sqlparser.sqltext = newQuery;
            i = sqlparser.parse();
            Assert.IsTrue(i == 0);
            //System.out.println(sqlparser.sqlstatements.get(0).sqlstatementtype);
            TMdxSelect select = (TMdxSelect)sqlparser.sqlstatements.get(0);
            TMdxAxisNode axisNode = select.Axes[0];
            TMdxCaseNode caseNode = (TMdxCaseNode)axisNode.ExpNode;

            Assert.IsTrue(caseNode.WhenList.Count == 3);

        }
    }
}
