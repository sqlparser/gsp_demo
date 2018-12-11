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
    using TCaseOption = gudusoft.gsqlparser.pp.para.styleenums.TCaseOption;
    using FormatterFactory = gudusoft.gsqlparser.pp.stmtformatter.FormatterFactory;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass()]
    public class testCapitalisation
	{

        [TestMethod()]
        public void testDefault()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "select department_id,\n" + "       min( salary ) \n" + "from   employees \n" + "group  by department_id";

			 sqlparser.parse();
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT   department_id,\n" + "         Min(salary)\n" + "FROM     employees\n" + "GROUP BY department_id", StringComparison.OrdinalIgnoreCase));
		   //  System.out.println(result);
		}

        [TestMethod()]
        public void testAllUpper()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "select department_id,\n" + "       min( salary ) \n" + "from   employees \n" + "group  by department_id";

			 sqlparser.parse();
			option.caseDatatype = TCaseOption.CoUppercase;
			option.caseFuncname = TCaseOption.CoUppercase;
			option.caseIdentifier = TCaseOption.CoUppercase;
			option.caseKeywords = TCaseOption.CoUppercase;
			option.caseQuotedIdentifier = TCaseOption.CoUppercase;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT   DEPARTMENT_ID,\n" + "         MIN(SALARY)\n" + "FROM     EMPLOYEES\n" + "GROUP BY DEPARTMENT_ID", StringComparison.OrdinalIgnoreCase));
			 //System.out.println(result);
		}

        [TestMethod()]
        public void testAllLower()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "Select department_id,\n" + "       min( salary ) \n" + "from   employees \n" + "group  by department_id";

			 sqlparser.parse();
			option.caseDatatype = TCaseOption.CoLowercase;
			option.caseFuncname = TCaseOption.CoLowercase;
			option.caseIdentifier = TCaseOption.CoLowercase;
			option.caseKeywords = TCaseOption.CoLowercase;
			option.caseQuotedIdentifier = TCaseOption.CoLowercase;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("select   department_id,\n" + "         min(salary)\n" + "from     employees\n" + "group by department_id", StringComparison.OrdinalIgnoreCase));
			 //System.out.println(result);
		}

        [TestMethod()]
        public void testAllUnchanged()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "Select department_id,\n" + "       miN( Salary ) \n" + "from   employees \n" + "GROUP  by department_id";

			 sqlparser.parse();
			option.caseDatatype = TCaseOption.CoNoChange;
			option.caseFuncname = TCaseOption.CoNoChange;
			option.caseIdentifier = TCaseOption.CoNoChange;
			option.caseKeywords = TCaseOption.CoNoChange;
			option.caseQuotedIdentifier = TCaseOption.CoNoChange;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("Select   department_id,\n" + "         miN(Salary)\n" + "from     employees\n" + "GROUP by department_id", StringComparison.OrdinalIgnoreCase));
			 //System.out.println(result);
		}

	}

}