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
    public class testInsertList
	{
        [TestMethod()]
        public void testInsert_Columnlist_Style()
		{
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);
			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);

			sqlparser.sqltext = "INSERT INTO employees\n" + "            (employee_id,\n" + "             first_name,\n" + "             department_id) VALUES     (113,            NULL,            100);";

			option.insertColumnlistStyle = TAlignStyle.AsWrapped;
			sqlparser.parse();
			string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("INSERT INTO employees\n" + "            (employee_id, first_name, department_id)\n" + "VALUES      (113,\n" + "             NULL,\n" + "             100);", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

        [TestMethod()]
        public void testInsert_Valuelist_Style()
		{
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);
			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);

			sqlparser.sqltext = "INSERT INTO employees\n" + "            (employee_id,\n" + "             first_name,\n" + "             department_id) VALUES     (113,            NULL,            100);";

			option.insertValuelistStyle = TAlignStyle.AsWrapped;
			sqlparser.parse();
			string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("INSERT INTO employees\n" + "            (employee_id,\n" + "             first_name,\n" + "             department_id)\n" + "VALUES      (113, NULL, 100);", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

        [TestMethod()]
        public void testDefaultCommaOption()
		{
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);
			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);

			sqlparser.sqltext = "INSERT INTO employees\n" + "            (employee_id,\n" + "             first_name,\n" + "             department_id) VALUES     (113,            NULL,            100);";

			option.defaultCommaOption = TLinefeedsCommaOption.LfbeforeCommaWithSpace;
			option.insertColumnlistStyle = TAlignStyle.AsStacked;
			option.insertValuelistStyle = TAlignStyle.AsStacked;
			sqlparser.parse();
			string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("INSERT INTO employees\n" + "            (employee_id\n" + "             , first_name\n" + "             , department_id)\n" + "VALUES      (113\n" + "             , NULL\n" + "             , 100);", StringComparison.OrdinalIgnoreCase));
		   // System.out.println(result);
		}

	}

}