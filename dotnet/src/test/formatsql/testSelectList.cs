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
	using TAlignStyle = gudusoft.gsqlparser.pp.para.styleenums.TAlignStyle;
	using TLinefeedsCommaOption = gudusoft.gsqlparser.pp.para.styleenums.TLinefeedsCommaOption;
	using FormatterFactory = gudusoft.gsqlparser.pp.stmtformatter.FormatterFactory;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass()]
    public class testSelectList
	{
        [TestMethod()]
        public void testSelect_Columnlist_Style()
		{

			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);

			 sqlparser.sqltext = "select col1, col2,sum(col3) from table1";

			int ret = sqlparser.parse();
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			option.selectColumnlistStyle = TAlignStyle.AsWrapped;
			string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Equals("SELECT col1, col2,Sum(col3)\n" + "FROM   table1", StringComparison.OrdinalIgnoreCase));

			sqlparser.parse();
			option.selectColumnlistStyle = TAlignStyle.AsStacked;
			result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Equals("SELECT col1,\n" + "       col2,\n" + "       Sum(col3)\n" + "FROM   table1", StringComparison.OrdinalIgnoreCase));
		}

        [TestMethod()]
        public void testSelect_Columnlist_Comma()
		{
			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);

			sqlparser.sqltext = "select col1, col2,sum(col3) from table1";

			sqlparser.parse();
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			option.selectColumnlistComma = TLinefeedsCommaOption.LfbeforeCommaWithSpace;
			string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Equals("SELECT col1\n" + "       , col2\n" + "       , Sum(col3)\n" + "FROM   table1", StringComparison.OrdinalIgnoreCase));

		   // System.out.println(result);

		}

        [TestMethod()]
        public void testSelectItemInNewLine()
		{
			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);

			sqlparser.sqltext = "select col1, col2,sum(col3) from table1";

			sqlparser.parse();
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			option.selectItemInNewLine = true;
			string result = FormatterFactory.pp(sqlparser, option);
		   // System.out.println(result);
			Assert.IsTrue(result.Equals("SELECT\n" + "  col1,\n" + "  col2,\n" + "  Sum(col3)\n" + "FROM   table1", StringComparison.OrdinalIgnoreCase));

			TGSqlParser sqlparser2 = new TGSqlParser(EDbVendor.dbvmssql);
			sqlparser2.sqltext = "select top 10 col1 as b, col2222 as c,sum(col3) as d from table1";
			option.selectItemInNewLine = true;
			option.alignAliasInSelectList = true;
			sqlparser2.parse();
			result = FormatterFactory.pp(sqlparser2, option);

			//System.out.println(result);
			Assert.IsTrue(result.Equals("SELECT top 10\n" + "  col1      AS b,\n" + "  col2222   AS c,\n" + "  Sum(col3) AS d\n" + "FROM   table1", StringComparison.OrdinalIgnoreCase));
		}

        [TestMethod()]
        public void testAlignAliasInSelectList()
		{
			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);

			sqlparser.sqltext = "select col1 as b, col2222 as c,sum(col3) as d from table1";

			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			option.alignAliasInSelectList = false;
			sqlparser.parse();
			string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Equals("SELECT col1 AS b,\n" + "       col2222 AS c,\n" + "       Sum(col3) AS d\n" + "FROM   table1", StringComparison.OrdinalIgnoreCase));

			sqlparser.parse();
			option.alignAliasInSelectList = true;

			result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Equals("SELECT col1      AS b,\n" + "       col2222   AS c,\n" + "       Sum(col3) AS d\n" + "FROM   table1", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);

		}

        [TestMethod()]
        /// <summary>
        /// Not support  option.treatDistinctAsVirtualColumn yet
        /// </summary>
        public void testTreatDistinctAsVirtualColumn()
		{
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);
			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);

			sqlparser.sqltext = "select distinct col1 as b, col2222 as c,sum(col3) as d from table1";

			option.treatDistinctAsVirtualColumn = true;
			sqlparser.parse();
			string result = FormatterFactory.pp(sqlparser, option);
			// System.out.println(result);
		   Assert.IsTrue(result.Trim().Equals("SELECT DISTINCT \n" + "       col1      AS b,\n" + "       col2222   AS c,\n" + "       Sum(col3) AS d\n" + "FROM   table1", StringComparison.OrdinalIgnoreCase));
		}

	}

}