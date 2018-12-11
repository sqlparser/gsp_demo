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
    public class testComment
	{
        [TestMethod()]
        public void testremove_comment()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);
			option.removeComment = true;

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "select department_id,\n" + "       min( salary ) -- single line comment \n" + "from   employees \n" + "group  by department_id";

			 sqlparser.parse();
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT   department_id,\n" + "         Min(salary) \n" + "FROM     employees\n" + "GROUP BY department_id", StringComparison.OrdinalIgnoreCase));
			//Assert.IsTrue("remove_comment is not supported",false);
			 Console.WriteLine(result);
		}

        [TestMethod()]
        public void testbegin_no_format()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "CREATE PROCEDURE uspnresults\n" + "AS\n" + "SELECT COUNT(contactid) FROM person.contact\n" + "--begin_no_format\n" + "SELECT COUNT(customerid) FROM \n" + "sales.customer;\n" + "--end_no_format\n" + "GO";

			 sqlparser.parse();
			 string result = FormatterFactory.pp(sqlparser, option);
			 //System.out.println(result);
			Assert.IsTrue(result.Trim().Equals("CREATE PROCEDURE uspnresults \n" + "AS \n" + "  SELECT Count(contactid)\n" + "  FROM   person.contact \n" + "--begin_no_format\n" + "SELECT Count(customerid) FROM \n" + "sales.customer;\n" + "--end_no_format \n" + "GO", StringComparison.OrdinalIgnoreCase));

		}


	}

}