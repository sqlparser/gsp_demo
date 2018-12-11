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
    public class testFromClause
	{
        [TestMethod()]
        public void testSelect_fromclause_Style()
		{
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);

			sqlparser.sqltext = "SELECT last_name,\n" + "       department_name dept_name \n" + "FROM   employees,\n" + "       departments;  ";

			sqlparser.parse();
			option.selectFromclauseStyle = TAlignStyle.AsWrapped;
			string result = FormatterFactory.pp(sqlparser, option);
			Console.WriteLine(result);
			Assert.IsTrue(result.Trim().Equals("SELECT last_name,\n" + "       department_name dept_name\n" + "FROM   employees, departments;", StringComparison.OrdinalIgnoreCase));
		}

        [TestMethod()]
        public void testSelect_fromclause_Comma()
		{
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);

			sqlparser.sqltext = "SELECT last_name,\n" + "       department_name dept_name \n" + "FROM   employees,\n" + "       departments;  ";


			sqlparser.parse();
			option.selectFromclauseComma = TLinefeedsCommaOption.LfbeforeCommaWithSpace;
			string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT last_name,\n" + "       department_name dept_name\n" + "FROM   employees\n" + "       , departments;", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

        [TestMethod()]
        public void testFromClauseInNewLine()
		{
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);

			sqlparser.sqltext = "SELECT last_name,\n" + "       department_name dept_name \n" + "FROM   employees,\n" + "       departments;  ";


			sqlparser.parse();
			option.fromClauseInNewLine = true;
			string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT last_name,\n" + "       department_name dept_name\n" + "FROM  \n" + "  employees,\n" + "  departments;", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);

		}

        [TestMethod()]
        public void testSelect_FromclauseJoinOnInNewline()
		{
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);

			sqlparser.sqltext = "SELECT p.name AS product,\n" + "       p.listprice AS 'List Price',\n" + "       p.discount AS 'discount' \n" + "FROM   \n" + "  production.product p \n" + "  JOIN production.productsubcategory s \n" + "    ON p.productsubcategoryid = s.productsubcategoryid \n" + "WHERE  s.name LIKE @product \n" + "       AND p.listprice < @maxprice;";


			sqlparser.parse();
			option.selectFromclauseJoinOnInNewline = false;
			string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT p.name      AS product,\n" + "       p.listprice AS 'List Price',\n" + "       p.discount  AS 'discount'\n" + "FROM   production.product p\n" + "       JOIN production.productsubcategory s ON p.productsubcategoryid = s.productsubcategoryid\n" + "WHERE  s.name LIKE @product\n" + "       AND p.listprice < @maxprice;", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);

		}

        [TestMethod()]
        public void testAlignJoinWithFromKeyword()
		{
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);

			sqlparser.sqltext = "SELECT p.name AS product,\n" + "       p.listprice AS 'List Price',\n" + "       p.discount AS 'discount' \n" + "FROM   \n" + "  production.product p \n" + "  JOIN production.productsubcategory s \n" + "    ON p.productsubcategoryid = s.productsubcategoryid \n" + "WHERE  s.name LIKE @product \n" + "       AND p.listprice < @maxprice;";


			sqlparser.parse();
			option.alignJoinWithFromKeyword = true;
			string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("SELECT p.name      AS product,\n" + "       p.listprice AS 'List Price',\n" + "       p.discount  AS 'discount'\n" + "FROM   production.product p\n" + "JOIN   production.productsubcategory s\n" + "       ON p.productsubcategoryid = s.productsubcategoryid\n" + "WHERE  s.name LIKE @product\n" + "       AND p.listprice < @maxprice;", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);

		}

	}

}