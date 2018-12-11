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
    public class testDeclare
	{
        [TestMethod()]
        public void testLinebreakAfterDeclare()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "DECLARE @s  VARCHAR(1000),        @s2 VARCHAR(10)";

			 sqlparser.parse();
			 option.linebreakAfterDeclare = true;
			option.indentLen = 2;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Equals("DECLARE\n" + "  @s  VARCHAR(1000),\n" + "  @s2 VARCHAR(10)", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}
	}

}