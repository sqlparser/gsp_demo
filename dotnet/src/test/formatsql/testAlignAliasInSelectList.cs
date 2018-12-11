using System;

namespace gudusoft.gsqlparser.test.formatsql
{
    /*
	 * Date: 12-1-29
	 */

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using GFmtOpt = gudusoft.gsqlparser.pp.para.GFmtOpt;
    using GFmtOptFactory = gudusoft.gsqlparser.pp.para.GFmtOptFactory;
    using TAlignOption = gudusoft.gsqlparser.pp.para.styleenums.TAlignOption;
    using TLinefeedsCommaOption = gudusoft.gsqlparser.pp.para.styleenums.TLinefeedsCommaOption;
    using FormatterFactory = gudusoft.gsqlparser.pp.stmtformatter.FormatterFactory;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass()]
    public class testAlignAliasInSelectList
	{
        [TestMethod()]
        public void test1()
		{
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			sqlparser.sqltext = "SELECT\n" + "col1 AS mycolumn\n" + ", col2 AS yourcolumn\n" + ", Sum(col3) AS thesum\n" + ", CASE\n" + "WHEN Lower(a) = 23 THEN 'blue'\n" + "ELSE NULL\n" + "END AS mycase\n" + ", Trim(TRAILING FROM col1) AS trim_col\n" + "FROM\n" + "table1\n" + "INNER JOIN table2\n" + "ON col1=col2 AND col3=col4\n" + "WHERE col4 > col5\n" + "AND col6 = 1000";

			sqlparser.parse();


			option.selectColumnlistComma = TLinefeedsCommaOption.LfbeforeCommaWithSpace;
			option.fromClauseInNewLine = true;
			option.selectItemInNewLine = true;
			option.andOrUnderWhere = true;
			option.fromClauseInNewLine = true;
			option.caseWhenThenInSameLine = true;

			string result = FormatterFactory.pp(sqlparser, option);
//            Console.WriteLine(result);
			Assert.IsTrue(result.Trim().Equals("SELECT\n  col1                       AS mycolumn\n  , col2                     AS yourcolumn\n  , SUM(col3)                AS thesum\n  , CASE\n      WHEN lower(a) = 23 THEN 'blue'\n      ELSE NULL\n    END                      AS mycase\n  , TRIM(TRAILING FROM col1) AS trim_col\nFROM  \n  table1\n  INNER JOIN table2\n  ON col1 = col2\n     AND col3 = col4\nWHERE  col4 > col5\n   AND col6 = 1000", StringComparison.OrdinalIgnoreCase));

		}

	}

}