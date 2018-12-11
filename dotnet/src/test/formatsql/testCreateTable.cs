using System;

namespace gudusoft.gsqlparser.test.formatsql
{
    /*
	 * Date: 11-3-22
	 */

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using GFmtOpt = gudusoft.gsqlparser.pp.para.GFmtOpt;
    using GFmtOptFactory = gudusoft.gsqlparser.pp.para.GFmtOptFactory;
    using TAlignOption = gudusoft.gsqlparser.pp.para.styleenums.TAlignOption;
    using TAlignStyle = gudusoft.gsqlparser.pp.para.styleenums.TAlignStyle;
    using TLinefeedsCommaOption = gudusoft.gsqlparser.pp.para.styleenums.TLinefeedsCommaOption;
    using FormatterFactory = gudusoft.gsqlparser.pp.stmtformatter.FormatterFactory;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass()]
    public class testCreateTable
	{

        [TestMethod()]
        public void testBEStyle_createtable_leftBEOnNewline()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			 sqlparser.sqltext = "CREATE TABLE dept(deptno NUMBER(2),\n" + "                  dname  VARCHAR2(14),\n" + "                  loc    VARCHAR2(13)) ";

			 sqlparser.parse();
			option.beStyleCreatetableLeftBEOnNewline = true;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("CREATE TABLE dept\n" + "  (deptno NUMBER(2),\n" + "   dname  VARCHAR2(14),\n" + "   loc    VARCHAR2(13))", StringComparison.OrdinalIgnoreCase));
			 //System.out.println(result);
		}

        [TestMethod()]
        public void testBEStyle_createtable_rightBEOnNewline()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			 sqlparser.sqltext = "CREATE TABLE dept(deptno NUMBER(2),\n" + "                  dname  VARCHAR2(14),\n" + "                  loc    VARCHAR2(13)) ";

			 sqlparser.parse();
			option.beStyleCreatetableRightBEOnNewline = true;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("CREATE TABLE dept(deptno NUMBER(2),\n" + "                  dname  VARCHAR2(14),\n" + "                  loc    VARCHAR2(13)\n" + ")", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

        [TestMethod()]
        public void testCreatetable_ListitemInNewLine()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			 sqlparser.sqltext = "CREATE TABLE dept(deptno NUMBER(2),\n" + "                  dname  VARCHAR2(14),\n" + "                  loc    VARCHAR2(13)) ";

			 sqlparser.parse();
			option.createtableListitemInNewLine = true;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("CREATE TABLE dept(\n" + "  deptno NUMBER(2),\n" + "  dname  VARCHAR2(14),\n" + "  loc    VARCHAR2(13))", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

        [TestMethod()]
        public void testCreatetable_Fieldlist_Align_option()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			 sqlparser.sqltext = "CREATE TABLE dept(deptno NUMBER(2),\n" + "                  dname  VARCHAR2(14),\n" + "                  loc    VARCHAR2(13)) ";

			 sqlparser.parse();
			option.createtableFieldlistAlignOption = TAlignOption.AloRight;
			option.beStyleCreatetableLeftBEOnNewline = true;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("CREATE TABLE dept\n" + "  (deptno NUMBER(2),\n" + "    dname VARCHAR2(14),\n" + "      loc VARCHAR2(13))", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

        [TestMethod()]
        public void testDefaultAligntype()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			 sqlparser.sqltext = "CREATE TABLE dept(deptno NUMBER(2),\n" + "                  dname  VARCHAR2(14),\n" + "                  loc    VARCHAR2(13)) ";

			 sqlparser.parse();
			 option.defaultAligntype = TAlignStyle.AsWrapped;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("CREATE TABLE dept(deptno NUMBER(2), dname VARCHAR2(14), loc VARCHAR2(13))", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

        [TestMethod()]
        public void testDefaultCommaOption()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			 sqlparser.sqltext = "CREATE TABLE dept(deptno NUMBER(2),\n" + "                  dname  VARCHAR2(14),\n" + "                  loc    VARCHAR2(13)) ";

			 sqlparser.parse();
			 option.defaultAligntype = TAlignStyle.AsStacked;
			 option.defaultCommaOption = TLinefeedsCommaOption.LfbeforeCommaWithSpace;
			 string result = FormatterFactory.pp(sqlparser, option);
			 Assert.IsTrue(result.Trim().Equals("CREATE TABLE dept(deptno  NUMBER(2)\n" + "                  , dname VARCHAR2(14)\n" + "                  , loc   VARCHAR2(13))", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

	}

}