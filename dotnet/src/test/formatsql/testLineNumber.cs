using System;

namespace gudusoft.gsqlparser.test.formatsql
{
	/*
	 * Date: 11-3-23
	 */

	using EDbVendor = gudusoft.gsqlparser.EDbVendor;
	using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
	using GFmtOpt = gudusoft.gsqlparser.pp.para.GFmtOpt;
	using GFmtOptFactory = gudusoft.gsqlparser.pp.para.GFmtOptFactory;
	using FormatterFactory = gudusoft.gsqlparser.pp.stmtformatter.FormatterFactory;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass()]
    public class testLineNumber
	{
        [TestMethod()]
        public void testlinenumber_enabled()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "select department_id,\n" + "       min( salary ) -- single line comment \n" + "from   employees \n" + "group  by department_id";

			 sqlparser.parse();
			option.linenumberEnabled = true;
			option.linenumberLeftMargin = 1;
			option.linenumberRightMargin = 4;
			 string result = FormatterFactory.pp(sqlparser, option);
			 Assert.IsTrue(result.Equals(" 1    SELECT   department_id,\n" + " 2             Min(salary) -- single line comment \n" + " 3    FROM     employees\n" + " 4    GROUP BY department_id", StringComparison.OrdinalIgnoreCase));
	//         System.out.println(result);
		}

	}

}