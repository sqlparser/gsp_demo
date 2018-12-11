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
    using TCompactMode = gudusoft.gsqlparser.pp.para.styleenums.TCompactMode;
    using FormatterFactory = gudusoft.gsqlparser.pp.stmtformatter.FormatterFactory;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass()]
    public class testCompactMode
	{
        [TestMethod()]
        /// <summary>
        /// turn sql into a single line, or multiple lines with a fixed line width
        /// no need to format this sql
        /// be careful when there are single line comment in SQL statement.
        /// </summary>
        public void testSingleLine()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "select department_id,\n" + "       min( salary ) -- single line comment \n" + "from   employees \n" + "group  by department_id";

			 sqlparser.parse();
			option.compactMode = TCompactMode.Cpmugly;
			option.lineWidth = 60;
			 string result = FormatterFactory.pp(sqlparser, option);
			 Assert.IsTrue(result.Trim().Equals("SELECT department_id, Min( salary ) \n" + "/* -- single line comment */ FROM employees GROUP BY \n" + "department_id", StringComparison.OrdinalIgnoreCase));

			 //System.out.println(result);
		}

	}

}