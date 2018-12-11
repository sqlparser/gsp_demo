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
    public class testCaseExpression
	{
        [TestMethod()]
        public void testCaseWhenThenInSameLine()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "SELECT productnumber,\n" + "       name,\n" + "       'Price Range' = CASE                          WHEN listprice = 0                          THEN 'Mfg item - not for resale' \n" + "                         WHEN listprice < 50                          THEN 'Under $50' \n" + "                         WHEN listprice >= 50                               AND listprice < 250 \n" + "                         THEN 'Under $250'                          WHEN listprice >= 250 \n" + "                              AND listprice < 1000                          THEN 'Under $1000'                        ELSE 'Over $1000' END \n" + "FROM   production.product \n" + "ORDER  BY productnumber;  ";

			 sqlparser.parse();
			option.caseWhenThenInSameLine = true;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT   productnumber,\n" + "         name,\n" + "         'Price Range' = CASE\n" + "                           WHEN listprice = 0 THEN 'Mfg item - not for resale'\n" + "                           WHEN listprice < 50 THEN 'Under $50'\n" + "                           WHEN listprice >= 50\n" + "                                AND listprice < 250 THEN 'Under $250'\n" + "                           WHEN listprice >= 250\n" + "                                AND listprice < 1000 THEN 'Under $1000'\n" + "                           ELSE 'Over $1000'\n" + "                         END\n" + "FROM     production.product\n" + "ORDER BY productnumber;", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

        [TestMethod()]
        public void testIndent_CaseFromSwitch()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "SELECT productnumber,\n" + "       name,\n" + "       'Price Range' = CASE                          WHEN listprice = 0                          THEN 'Mfg item - not for resale' \n" + "                         WHEN listprice < 50                          THEN 'Under $50' \n" + "                         WHEN listprice >= 50                               AND listprice < 250 \n" + "                         THEN 'Under $250'                          WHEN listprice >= 250 \n" + "                              AND listprice < 1000                          THEN 'Under $1000'                        ELSE 'Over $1000' END \n" + "FROM   production.product \n" + "ORDER  BY productnumber;  ";

			 sqlparser.parse();
			option.caseWhenThenInSameLine = false;
			option.indentCaseFromSwitch = 4;
			option.indentCaseThen = 2;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT   productnumber,\n" + "         name,\n" + "         'Price Range' = CASE\n" + "                             WHEN listprice = 0\n" + "                               THEN 'Mfg item - not for resale'\n" + "                             WHEN listprice < 50\n" + "                               THEN 'Under $50'\n" + "                             WHEN listprice >= 50\n" + "                                  AND listprice < 250\n" + "                               THEN 'Under $250'\n" + "                             WHEN listprice >= 250\n" + "                                  AND listprice < 1000\n" + "                               THEN 'Under $1000'\n" + "                             ELSE 'Over $1000'\n" + "                         END\n" + "FROM     production.product\n" + "ORDER BY productnumber;", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}
	}

}