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
    public class testGroupByClause
	{

        [TestMethod()]
        public void testSelect_Groupby_Style()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			 sqlparser.sqltext = "SELECT e.employee_id,\n" + "       d.locatioin_id \n" + "FROM   employees e,departments d \n" + "group by e.employee_id,d.locatioin_id,d.locatioin_id2 ";

			 sqlparser.parse();
			 option.selectColumnlistStyle = TAlignStyle.AsStacked;
			 string result = FormatterFactory.pp(sqlparser, option);

			 Assert.IsTrue(result.Trim().Equals("SELECT   e.employee_id,\n" + "         d.locatioin_id\n" + "FROM     employees e,\n" + "         departments d\n" + "GROUP BY e.employee_id,\n" + "         d.locatioin_id,\n" + "         d.locatioin_id2", StringComparison.OrdinalIgnoreCase));
			 //System.out.println(result);
		}

        [TestMethod()]
        public void testSelect_Columnlist_Comma()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			 sqlparser.sqltext = "SELECT e.employee_id,\n" + "       d.locatioin_id \n" + "FROM   employees e,departments d \n" + "group by e.employee_id,d.locatioin_id,d.locatioin_id2 ";

			 sqlparser.parse();
			 option.selectColumnlistStyle = TAlignStyle.AsStacked;
			 option.selectColumnlistComma = TLinefeedsCommaOption.LfbeforeCommaWithSpace;
			 string result = FormatterFactory.pp(sqlparser, option);
			 Assert.IsTrue(result.Trim().Equals("SELECT   e.employee_id\n" + "         , d.locatioin_id\n" + "FROM     employees e,\n" + "         departments d\n" + "GROUP BY e.employee_id\n" + "         , d.locatioin_id\n" + "         , d.locatioin_id2", StringComparison.OrdinalIgnoreCase));

			 //System.out.println(result);
		}

        [TestMethod()]
        public void testSelectItemInNewLine()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			 sqlparser.sqltext = "SELECT e.employee_id,\n" + "       d.locatioin_id \n" + "FROM   employees e,departments d \n" + "group by e.employee_id,d.locatioin_id,d.locatioin_id2 ";

			 sqlparser.parse();
			 option.selectItemInNewLine = true;
			 string result = FormatterFactory.pp(sqlparser, option);
			 Assert.IsTrue(result.Trim().Equals("SELECT  \n" + "  e.employee_id,\n" + "  d.locatioin_id\n" + "FROM     employees e,\n" + "         departments d\n" + "GROUP BY\n" + "  e.employee_id,\n" + "  d.locatioin_id,\n" + "  d.locatioin_id2", StringComparison.OrdinalIgnoreCase));
			 //System.out.println(result);
		}

	}

}