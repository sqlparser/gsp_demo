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
    public class testFunctionCall
    {

        [TestMethod()]
        public void testParameters()
        {
            GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);
            option.functionCallParametersStyle = TAlignStyle.AsStacked;
            option.functionCallParametersComma = TLinefeedsCommaOption.LfBeforeComma;

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "SET @a = dbo.Func1(@param1,                   @param2,                   @param3 + 1,\n" + "                   @param4)  ";

            sqlparser.parse();
            string result = FormatterFactory.pp(sqlparser, option);
            Assert.IsTrue(result.Trim().Equals("SET @a = dbo.Func1(@param1\n" + "                   ,@param2\n" + "                   ,@param3 + 1\n" + "                   ,@param4)", StringComparison.OrdinalIgnoreCase));
            //System.out.println(result);
        }

        [TestMethod()]
        public void testDecode()
        {
            GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);
            option.functionCallParametersStyle = TAlignStyle.AsStacked;
            option.functionCallParametersComma = TLinefeedsCommaOption.LfbeforeCommaWithSpace;

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "SELECT last_name,\n" + "       DECODE(job_id, 'It_prog', 1.10 * salary,                      'st_clerk', 1.15 * salary,                      'sa_rep', 1.20 * salary,\n" + "                      salary) revised_salary\n" + "FROM   employees;";

            sqlparser.parse();
            string result = FormatterFactory.pp(sqlparser, option);
            Assert.IsTrue(result.Trim().Equals("SELECT last_name,\n" + "       Decode(job_id\n" + "              , 'It_prog'\n" + "              , 1.10 * salary\n" + "              , 'st_clerk'\n" + "              , 1.15 * salary\n" + "              , 'sa_rep'\n" + "              , 1.20 * salary\n" + "              , salary) revised_salary\n" + "FROM   employees;", StringComparison.OrdinalIgnoreCase));
            // System.out.println(result);
        }

    }

}