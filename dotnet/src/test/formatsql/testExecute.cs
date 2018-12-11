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
    public class testExecute
	{
        [TestMethod()]
        public void testLinebreakBeforeParamInExec()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "EXEC Sptrackmember   @p_member_id,  '2.2',  @p_weeknum   ";

			 sqlparser.parse();
			 option.linebreakBeforeParamInExec = false;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("EXEC sptrackmember @p_member_id, '2.2', @p_weeknum", StringComparison.OrdinalIgnoreCase));

			sqlparser.parse();
			option.linebreakBeforeParamInExec = true;
			result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("EXEC sptrackmember \n" + "  @p_member_id,\n" + "  '2.2',\n" + "  @p_weeknum", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}
	}

}