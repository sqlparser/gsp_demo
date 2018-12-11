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
    public class testIndentation
	{
        [TestMethod()]
        public void testIndentLen()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "DECLARE @s  VARCHAR(1000),        @s2 VARCHAR(10)";

			 sqlparser.parse();
			 option.linebreakAfterDeclare = true;
			option.indentLen = 3;
			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("DECLARE\n" + "   @s  VARCHAR(1000),\n" + "   @s2 VARCHAR(10)", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

        [TestMethod()]
        public void testUseTab()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "DECLARE @s  VARCHAR(1000),        @s2 VARCHAR(10)";

			 sqlparser.parse();
			 option.linebreakAfterDeclare = true;
			option.indentLen = 2;
			option.useTab = true;
			option.tabSize = 3;
			 string result = FormatterFactory.pp(sqlparser, option);
			 Assert.IsTrue(result.Trim().Equals("DECLARE\n" + "   @s  VARCHAR(1000),\n" + "   @s2 VARCHAR(10)", StringComparison.OrdinalIgnoreCase));
			//System.out.println(result);
		}

        [TestMethod()]
        /// <summary>
        /// don't use BEStyle_Function_BodyIndent,  replace it by  beStyleBlockIndentSize
        /// </summary>
        public void testBEStyle_Function_BodyIndent()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "CREATE PROCEDURE humanresources.Uspgetallemployees \n" + "AS \n" + "SELECT lastname,\n" + "firstname,\n" + "jobtitle,\n" + "department \n" + "FROM   humanresources.vemployeedepartment; \n";

			 sqlparser.parse();
			 option.beStyleBlockIndentSize = 3;

			 string result = FormatterFactory.pp(sqlparser, option);
			 //System.out.println(result);
			Assert.IsTrue(result.Trim().Equals("CREATE PROCEDURE humanresources.uspgetallemployees \n" + "AS \n" + "   SELECT lastname,\n" + "          firstname,\n" + "          jobtitle,\n" + "          department\n" + "   FROM   humanresources.vemployeedepartment;", StringComparison.OrdinalIgnoreCase));
		}

        [TestMethod()]
        public void testBEStyle_Block_leftBEIndentSize()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "CREATE PROCEDURE humanresources.Uspgetallemployees \n" + "AS \n" + "BEGIN \n" + "SELECT lastname,\n" + "firstname,\n" + "jobtitle,\n" + "department \n" + "FROM   humanresources.vemployeedepartment; \n" + "END  \n";

			 sqlparser.parse();
			 option.beStyleBlockLeftBEOnNewline = true; // begin keyword in new line
			 option.beStyleBlockLeftBEIndentSize = 2; // offset of begin keyword
			 option.beStyleBlockIndentSize = 1; // code indentation inside begin/end block

			 string result = FormatterFactory.pp(sqlparser, option);
			 //System.out.println(result);
			Assert.IsTrue(result.Trim().Equals("CREATE PROCEDURE humanresources.uspgetallemployees \n" + "AS \n" + "  BEGIN \n" + "   SELECT lastname,\n" + "          firstname,\n" + "          jobtitle,\n" + "          department\n" + "   FROM   humanresources.vemployeedepartment; \n" + "  END", StringComparison.OrdinalIgnoreCase));
		}

        [TestMethod()]
        public void testBEStyle_Block_leftBEOnNewline()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "IF @cost <= @compareprice \n" + "BEGIN \n" + "PRINT 'These products can be purchased for less than ' \n" + "END \n" + "ELSE \n" + "PRINT 'The prices for all products in this category exceed '";

			 sqlparser.parse();
			 option.beStyleBlockLeftBEOnNewline = false; // begin keyword not in a new line
			 option.beStyleBlockRightBEIndentSize = 0; // used in indent end keyword, valid only when beStyleBlockLeftBEOnNewline = false, otherwise, use beStyleBlockLeftBEIndentSize to indent begin and end keyword
			 option.beStyleBlockIndentSize = 2;
			 option.beStyleIfElseSingleStmtIndentSize = 4;

			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("IF @cost <= @compareprice BEGIN \n" + "  PRINT 'These products can be purchased for less than ' \n" + "END \n" + "ELSE \n" + "    PRINT 'The prices for all products in this category exceed '", StringComparison.OrdinalIgnoreCase));
			//Assert.IsTrue("beStyleBlockLeftBEOnNewline,beStyleBlockLeftBEIndentSize,beStyleBlockIndentSize,beStyleIfElseSingleStmtIndentSize not implemented",false);
			// System.out.println(result);
		}

        [TestMethod()]
        public void testBEStyle_Block_leftBEOnNewline2()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "IF @cost <= @compareprice \n" + "BEGIN \n" + "PRINT 'These products can be purchased for less than ' \n" + "END \n" + "ELSE \n" + "PRINT 'The prices for all products in this category exceed '";

			 sqlparser.parse();
			 option.beStyleBlockLeftBEOnNewline = false;
			 option.beStyleBlockRightBEIndentSize = 2;
			 option.beStyleBlockIndentSize = 0;
			 option.beStyleIfElseSingleStmtIndentSize = 2;

			 string result = FormatterFactory.pp(sqlparser, option);
			 Assert.IsTrue(result.Trim().Equals("IF @cost <= @compareprice BEGIN \n" + "  PRINT 'These products can be purchased for less than ' \n" + "  END \n" + "ELSE \n" + "  PRINT 'The prices for all products in this category exceed '", StringComparison.OrdinalIgnoreCase));
		   //  System.out.println(result);
		}

        [TestMethod()]
        public void testBEStyle_Block_leftBEOnNewline3()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "IF @cost <= @compareprice \n" + "BEGIN \n" + "PRINT 'These products can be purchased for less than ' \n" + "END \n" + "ELSE \n" + "PRINT 'The prices for all products in this category exceed '";

			 sqlparser.parse();
			 option.beStyleBlockLeftBEOnNewline = true;
			 option.beStyleBlockLeftBEIndentSize = 0; // used to indent both begin and end keyword
			 option.beStyleBlockIndentSize = 2;
			 option.beStyleIfElseSingleStmtIndentSize = 2;

			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("IF @cost <= @compareprice \n" + "BEGIN \n" + "  PRINT 'These products can be purchased for less than ' \n" + "END \n" + "ELSE \n" + "  PRINT 'The prices for all products in this category exceed '", StringComparison.OrdinalIgnoreCase));
			// System.out.println(result);
		}

        [TestMethod()]
        public void testBEStyle_Block_leftBEOnNewline4()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "IF @cost <= @compareprice \n" + "BEGIN \n" + "PRINT 'These products can be purchased for less than ' \n" + "END \n" + "ELSE \n" + "PRINT 'The prices for all products in this category exceed '";

			 sqlparser.parse();
			 option.beStyleBlockLeftBEOnNewline = true;
			 option.beStyleBlockLeftBEIndentSize = 2;
			 option.beStyleBlockIndentSize = 2;
			 option.beStyleIfElseSingleStmtIndentSize = 2;

			 string result = FormatterFactory.pp(sqlparser, option);
			Assert.IsTrue(result.Trim().Equals("IF @cost <= @compareprice \n" + "  BEGIN \n" + "    PRINT 'These products can be purchased for less than ' \n" + "  END \n" + "ELSE \n" + "  PRINT 'The prices for all products in this category exceed '", StringComparison.OrdinalIgnoreCase));
			// System.out.println(result);
		}

        [TestMethod()]
        public void testMssqlCreateFunction_ReturnStmt()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			sqlparser.sqltext = "CREATE FUNCTION Sales.fn_SalesByStore (@storeid int)\n" + "RETURNS TABLE\n" + "AS\n" + "RETURN \n" + "(\n" + "    SELECT P.ProductID, P.Name, SUM(SD.LineTotal) AS 'YTD Total'\n" + "    FROM Production.Product AS P \n" + "      JOIN Sales.SalesOrderDetail AS SD ON SD.ProductID = P.ProductID\n" + "      JOIN Sales.SalesOrderHeader AS SH ON SH.SalesOrderID = SD.SalesOrderID\n" + "    WHERE SH.CustomerID = @storeid\n" + "    GROUP BY P.ProductID, P.Name\n" + "); ";

			 sqlparser.parse();
			 option.beStyleBlockIndentSize = 3;
			 string result = FormatterFactory.pp(sqlparser, option);

		   // System.out.println(result);
			Assert.IsTrue(result.Trim().Equals("CREATE FUNCTION sales.Fn_salesbystore (@storeid INT\n" + ") \n" + "RETURNS TABLE \n" + "AS \n" + "   RETURN  \n" + "   (\n" + "      SELECT   p.productid,\n" + "               p.name,\n" + "               Sum(sd.linetotal) AS 'YTD Total'\n" + "      FROM     production.product AS p\n" + "               JOIN sales.salesorderdetail AS sd\n" + "               ON sd.productid = p.productid\n" + "               JOIN sales.salesorderheader AS sh\n" + "               ON sh.salesorderid = sd.salesorderid\n" + "      WHERE    sh.customerid = @storeid\n" + "      GROUP BY p.productid,\n" + "               p.name\n" + "   );", StringComparison.OrdinalIgnoreCase));
		}

        [TestMethod()]
        public void testMssqlCreateFunction_BlockStmt()
		{
			 GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "create function infFKfrom(@tbID int, @colID smallint) returns varchar(2000) as \n" + "begin declare @r varchar(2000), @a varchar(200)\n" + "select @r='', @a=''\n" + "DECLARE cs CURSOR FOR\n" + "      select FKfrom=convert(varchar(200),object_name(rkeyid)+'.'+r.[name])\n" + " from sysforeignkeys c     join syscolumns f on c.fkeyid=f.[id] and c.fkey=f.colID\n" + "join syscolumns r on c.rkeyid=r.[id] and c.rkey=r.colID where fkeyID=@tbID and fkey=@colID\n" + "    order by keyNo\n" + "\n" + "OPEN cs\n" + " FETCH NEXT FROM cs INTO @a\n" + "WHILE @@FETCH_STATUS = 0 BEGIN\n" + "select @r=@r+case when len(@r)>0 then ', ' else '' end+@a\n" + "FETCH NEXT FROM cs INTO @a\n" + "END\n" + "   CLOSE cs\n" + " DEALLOCATE cs\n" + "   return(@r)\n" + "end\n" + "GO";

			 sqlparser.parse();
			 option.beStyleBlockLeftBEOnNewline = true;
			 option.beStyleBlockLeftBEIndentSize = 2;
			 option.beStyleBlockIndentSize = 3;
			 string result = FormatterFactory.pp(sqlparser, option);
            //Console.WriteLine(result);
            //Assert.IsTrue(result.Trim().Equals("CREATE FUNCTION Inffkfrom(@tbID  INT,\n" + "                          @colID SMALLINT\n" + ") \n" + "RETURNS VARCHAR(2000) \n" + "AS \n" + "  BEGIN \n" + "     DECLARE @r VARCHAR(2000),\n" + "             @a VARCHAR(200) \n" + "     SELECT @r = '',\n" + "            @a = '' \n" + "     DECLARE cs CURSOR FOR SELECT   fkfrom = Convert(VARCHAR(200),Object_name(rkeyid)+'.'+r.[name])\n" + "                           FROM     sysforeignkeys c\n" + "                                    JOIN syscolumns f\n" + "                                    ON c.fkeyid=f.[id] AND c.fkey=f.colid\n" + "                                    JOIN syscolumns r\n" + "                                    ON c.rkeyid=r.[id] AND c.rkey=r.colid\n" + "                           WHERE    fkeyid = @tbID\n" + "                                    AND fkey = @colID\n" + "                           ORDER BY keyno \n" + "\n" + "     OPEN cs \n" + "     FETCH NEXT FROM cs INTO @a \n" + "     WHILE @@FETCH_STATUS = 0 \n" + "       BEGIN \n" + "          SELECT @r = @r + CASE\n" + "                             WHEN Len(@r) > 0\n" + "                             THEN ', '\n" + "                             ELSE ''\n" + "                           END + @a \n" + "          FETCH NEXT FROM cs INTO @a \n" + "       END \n" + "     CLOSE cs \n" + "     DEALLOCATE cs \n" + "     RETURN(@r) \n" + "  END\n" + "GO", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(result.Trim().Equals("CREATE FUNCTION INFFKFROM(@tbID  INT,\n                          @colID SMALLINT\n) \nRETURNS VARCHAR(2000) \nAS \n  BEGIN \n     DECLARE @r VARCHAR(2000),\n             @a VARCHAR(200) \n     SELECT @r = '',\n            @a = '' \n     DECLARE cs CURSOR FOR SELECT   fkfrom = CONVERT(VARCHAR(200),object_name(rkeyid) + '.' + r.[name])\n                           FROM     sysforeignkeys c\n                                    JOIN syscolumns f\n                                    ON c.fkeyid = f.[id]\n                                       AND c.fkey = f.colid\n                                    JOIN syscolumns r\n                                    ON c.rkeyid = r.[id]\n                                       AND c.rkey = r.colid\n                           WHERE    fkeyid = @tbID\n                                    AND fkey = @colID\n                           ORDER BY keyno \n\n     OPEN cs \n     FETCH NEXT FROM cs INTO @a \n     WHILE @@FETCH_STATUS = 0 \n       BEGIN \n          SELECT @r = @r + CASE\n                             WHEN len(@r) > 0\n                             THEN ', '\n                             ELSE ''\n                           END + @a \n          FETCH NEXT FROM cs INTO @a \n       END \n     CLOSE cs \n     DEALLOCATE cs \n     RETURN(@r) \n  END\nGO", StringComparison.OrdinalIgnoreCase));

        }

	}

}