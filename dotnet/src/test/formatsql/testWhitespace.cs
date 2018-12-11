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
    public class testWhitespace
	{
        [TestMethod()]
        public void testWSPadding_OperatorArithmetic()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			 sqlparser.sqltext = "SELECT * \n" + "FROM   dual \n" + "WHERE  1=-1 \n" + "       AND(1!=2 \n" + "            OR 2^=3) \n" + "       AND 3<>4 \n" + "       AND 4>+5;  ";
			 sqlparser.parse();
			 option.wsPaddingOperatorArithmetic = true;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT *\n" + "FROM   dual\n" + "WHERE  1 = -1\n" + "       AND ( 1 != 2\n" + "             OR 2 ^= 3 )\n" + "       AND 3 <> 4\n" + "       AND 4 > +5;", StringComparison.OrdinalIgnoreCase));
		  //  System.out.println(result);
		}

        [TestMethod()]
        public void testWSPadding_ParenthesesInFunction()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "CREATE FUNCTION sales.Fn_salesbystore\n" + "  ( @storeid INT) \n" + "RETURNS TABLE \n" + "AS \n" + "  RETURN 0;";
			 sqlparser.parse();
			 option.wsPaddingParenthesesInFunction = true;
			option.beStyleFunctionRightBEOnNewline = false;
			 string result = FormatterFactory.pp(sqlparser, option);
			//System.out.println(result);
			Assert.IsTrue(result.Trim().Equals("CREATE FUNCTION sales.Fn_salesbystore ( @storeid INT ) \n" + "RETURNS TABLE \n" + "AS \n" + "  RETURN 0;", StringComparison.OrdinalIgnoreCase));

			sqlparser.parse();
			option.wsPaddingParenthesesInFunction = false;
		   option.beStyleFunctionRightBEOnNewline = false;
			result = FormatterFactory.pp(sqlparser, option);
		   // System.out.println(result);
			Assert.IsTrue(result.Trim().Equals("CREATE FUNCTION sales.Fn_salesbystore (@storeid INT) \n" + "RETURNS TABLE \n" + "AS \n" + "  RETURN 0;", StringComparison.OrdinalIgnoreCase));
		}

        [TestMethod()]
        public void testWSPadding_ParenthesesInExpression()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "SELECT ( ( ( a - b) - c)) FROM   t ";
			 sqlparser.parse();
			 option.wsPaddingParenthesesInExpression = true;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT ( ( ( a - b ) - c ) )\n" + "FROM   t", StringComparison.OrdinalIgnoreCase));

			//System.out.println(result);

			sqlparser.parse();
			option.wsPaddingParenthesesInExpression = false;
			result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT (((a - b) - c))\n" + "FROM   t", StringComparison.OrdinalIgnoreCase));

		   // System.out.println(result);
		}

        [TestMethod()]
        public void testWSPadding_ParenthesesOfSubQuery()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "SELECT last_name \n" + "FROM   employees \n" + "WHERE  salary > ( SELECT salary \n" + "                 FROM   employees \n" + "                 WHERE  last_name = 'Abel');";
			 sqlparser.parse();
			 option.wsPaddingParenthesesOfSubQuery = true;

			 string result = FormatterFactory.pp(sqlparser, option);

			Assert.IsTrue(result.Trim().Equals("SELECT last_name\n" + "FROM   employees\n" + "WHERE  salary > ( SELECT salary\n" + "                  FROM   employees\n" + "                  WHERE  last_name = 'Abel' );", StringComparison.OrdinalIgnoreCase));

			//System.out.println(result);

			sqlparser.parse();
			option.wsPaddingParenthesesOfSubQuery = false;
			result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT last_name\n" + "FROM   employees\n" + "WHERE  salary > (SELECT salary\n" + "                 FROM   employees\n" + "                 WHERE  last_name = 'Abel');", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

        [TestMethod()]
        public void testWSPadding_ParenthesesInFunctionCall()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "SELECT department_id,\n" + "       Min( salary) \n" + "FROM   employees \n" + "GROUP  BY department_id";
			 sqlparser.parse();

			option.wsPaddingParenthesesInFunctionCall = true;
			string result = FormatterFactory.pp(sqlparser, option);

			Assert.IsTrue(result.Trim().Equals("SELECT   department_id,\n" + "         Min( salary )\n" + "FROM     employees\n" + "GROUP BY department_id", StringComparison.OrdinalIgnoreCase));
			 //System.out.println(result);

			sqlparser.parse();
			option.wsPaddingParenthesesInFunctionCall = false;
			result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT   department_id,\n" + "         Min(salary)\n" + "FROM     employees\n" + "GROUP BY department_id", StringComparison.OrdinalIgnoreCase));

			//System.out.println(result);
		}

        [TestMethod()]
        public void testWSPadding_ParenthesesOfTypename()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "CREATE TABLE datatype \n" + "  (fld0 GENERICTYPE,\n" + "   fld1 CHAR( 2),\n" + "   fld3 NCHAR( 1)); ";

			sqlparser.parse();

			option.wsPaddingParenthesesOfTypename = true;
			string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("CREATE TABLE datatype(fld0 GENERICTYPE,\n" + "                      fld1 CHAR( 2 ),\n" + "                      fld3 NCHAR( 1 ));", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);

			sqlparser.parse();
			option.wsPaddingParenthesesOfTypename = false;
			result = FormatterFactory.pp(sqlparser, option);

			Assert.IsTrue(result.Trim().Equals("CREATE TABLE datatype(fld0 GENERICTYPE,\n" + "                      fld1 CHAR(2),\n" + "                      fld3 NCHAR(1));", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

	}

}