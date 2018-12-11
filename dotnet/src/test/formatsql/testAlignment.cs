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
    using FormatterFactory = gudusoft.gsqlparser.pp.stmtformatter.FormatterFactory;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass()]
    public class testAlignment
	{
        [TestMethod()]
        public void testSelect_keywords_alignOption()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "DELETE FROM job_history jh \n" + "WHERE  employee_id = (SELECT employee_id \n" + "FROM   employee e \n" + "WHERE  jh.employee_id = e.employee_id \n" + "AND start_date = (SELECT Min(start_date) \n" + "FROM   job_history jh \n" + "WHERE  jh.employee_id = e.employee_id) \n" + "AND 5 > (SELECT Count( * ) \n" + "FROM   job_history jh \n" + "WHERE  jh.employee_id = e.employee_id \n" + "GROUP  BY employee_id \n" + "HAVING Count( * ) >= 4)); ";

			 sqlparser.parse();
			option.selectKeywordsAlignOption = TAlignOption.AloRight;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("DELETE FROM job_history jh\n" + "      WHERE employee_id = (SELECT employee_id\n" + "                             FROM employee e\n" + "                            WHERE jh.employee_id = e.employee_id\n" + "                                  AND start_date = (SELECT Min(start_date)\n" + "                                                      FROM job_history jh\n" + "                                                     WHERE jh.employee_id = e.employee_id)\n" + "                                  AND 5 > (  SELECT Count(*)\n" + "                                               FROM job_history jh\n" + "                                              WHERE jh.employee_id = e.employee_id\n" + "                                           GROUP BY employee_id\n" + "                                           HAVING  Count(*) >= 4));", StringComparison.OrdinalIgnoreCase));
	//        Assert.IsTrue(result.trim().equalsIgnoreCase("DELETE FROM job_history jh\n" +
	//                "      WHERE employee_id = (SELECT employee_id\n" +
	//                "                             FROM employee e\n" +
	//                "                            WHERE jh.employee_id = e.employee_id\n" +
	//                "                                  AND start_date = (SELECT Min(start_date)\n" +
	//                "                                                      FROM job_history jh\n" +
	//                "                                                     WHERE jh.employee_id = e.employee_id)\n" +
	//                "                                  AND 5 > (  SELECT Count(*)\n" +
	//                "                                               FROM job_history jh\n" +
	//                "                                              WHERE jh.employee_id = e.employee_id\n" +
	//                "                                           GROUP BY employee_id  HAVING Count(*) >= 4));"));

		   // System.out.println(result);
		}

        [TestMethod()]
        public void testSelect_keywords_alignOption_delete()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "INSERT INTO employees\n" + "(employee_id,\n" + " first_name,\n" + " last_name,\n" + " email,\n" + " phone_number,\n" + " hire_date,\n" + " job_id,\n" + " salary,\n" + " commission_pct,\n" + " manager_id,\n" + " department_id) \n" + "VALUES(113,\n" + "'Louis',\n" + "'Popp',\n" + "'Ldd',\n" + "'515.124.222',\n" + "sysdate,\n" + "'Ac_account',\n" + "8900,\n" + "NULL,\n" + "205,\n" + "100)\n" + "            \n" + "                                      ";

			 sqlparser.parse();
			option.selectKeywordsAlignOption = TAlignOption.AloRight;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("INSERT INTO employees\n" + "            (employee_id,\n" + "             first_name,\n" + "             last_name,\n" + "             email,\n" + "             phone_number,\n" + "             hire_date,\n" + "             job_id,\n" + "             salary,\n" + "             commission_pct,\n" + "             manager_id,\n" + "             department_id)\n" + "     VALUES (113,\n" + "             'Louis',\n" + "             'Popp',\n" + "             'Ldd',\n" + "             '515.124.222',\n" + "             sysdate,\n" + "             'Ac_account',\n" + "             8900,\n" + "             NULL,\n" + "             205,\n" + "             100)", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

        [TestMethod()]
        public void testSelect_keywords_alignOption_update()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "UPDATE employees \n" + "SET department_id = 70 \n" + "WHERE employee_id = 113";

			 sqlparser.parse();
			option.selectKeywordsAlignOption = TAlignOption.AloRight;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("UPDATE employees\n" + "   SET department_id = 70\n" + " WHERE employee_id = 113", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

	}

}