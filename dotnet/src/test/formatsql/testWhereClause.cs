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
	using FormatterFactory = gudusoft.gsqlparser.pp.stmtformatter.FormatterFactory;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass()]
    public class testWhereClause
	{
        [TestMethod()]
        public void testAndOrUnderWhere()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			 sqlparser.sqltext = "SELECT e.employee_id,\n" + "       d.locatioin_id\n" + "FROM   employees e,\n" + "       departments d\n" + "WHERE  e.department_id = d.department_id\n" + "   AND e.last_name = 'Matos' and exists(\n" + "\t\t\t\tSELECT e.employee_id\n" + "\t\t\t\tFROM   employees e,\n" + "\t\t\t\t       departments d\n" + "\t\t\t\tWHERE  e.department_id = d.department_id\n" + "\t\t\t\t   AND e.last_name   \n" + "   );";

			 sqlparser.parse();
			option.andOrUnderWhere = true;
			 string result = FormatterFactory.pp(sqlparser, option);
			 Assert.IsTrue(result.Trim().Equals("SELECT e.employee_id,\n" + "       d.locatioin_id\n" + "FROM   employees e,\n" + "       departments d\n" + "WHERE  e.department_id = d.department_id\n" + "   AND e.last_name = 'Matos'\n" + "   AND EXISTS( SELECT e.employee_id\n" + "               FROM   employees e,\n" + "                      departments d\n" + "               WHERE  e.department_id = d.department_id\n" + "                  AND e.last_name  );", StringComparison.OrdinalIgnoreCase));
		   //  System.out.println(result);
		}

	}

}