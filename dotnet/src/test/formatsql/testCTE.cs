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
    public class testCTE
	{
        [TestMethod()]
        public void testCTE_NewlineBeforeAs()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "WITH mycte(x)     AS (SELECT x = Convert( VARCHAR(1000), 'hello' )         UNION ALL \n" + "         SELECT Convert( VARCHAR(1000), x + 'a' )          FROM   mycte \n" + "         WHERE  Len( x ) < 10         UNION ALL          SELECT Convert( VARCHAR(1000), x + 'b' ) \n" + "         FROM   mycte          WHERE  Len( x ) < 10)\n" + "SELECT x FROM   mycte ORDER  BY Len( x ),          x;  ";

			 sqlparser.parse();
			 option.cteNewlineBeforeAs = false;
			 string result = FormatterFactory.pp(sqlparser, option);
            //Console.WriteLine(result);
            Assert.IsTrue(result.Trim().Equals("WITH mycte(x) AS (SELECT x = CONVERT(VARCHAR(1000), 'hello')\n                  UNION ALL\n                  SELECT CONVERT(VARCHAR(1000), x + 'a')\n                  FROM   mycte\n                  WHERE  len(x) < 10\n                  UNION ALL\n                  SELECT CONVERT(VARCHAR(1000), x + 'b')\n                  FROM   mycte\n                  WHERE  len(x) < 10) \n  SELECT   x\n  FROM     mycte\n  ORDER BY len(x),\n           x;", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}
	}

}