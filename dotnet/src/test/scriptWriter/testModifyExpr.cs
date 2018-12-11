using System;

namespace gudusoft.gsqlparser.test.scriptWriter
{

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using EExpressionType = gudusoft.gsqlparser.EExpressionType;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using TExpression = gudusoft.gsqlparser.nodes.TExpression;
    using TExpressionList = gudusoft.gsqlparser.nodes.TExpressionList;
    using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class testModifyExpr
	{

        private TGSqlParser parser = new TGSqlParser(EDbVendor.dbvoracle);

        [TestMethod]
        public virtual void testRemoveObjectExpr()
		{
			TExpression expression = parser.parseExpression("columnA");
			Assert.IsTrue(expression.ExpressionType == EExpressionType.simple_object_name_t);
            expression.remove();
            Assert.IsTrue(expression.ExpressionType == EExpressionType.removed_t);
		}

        [TestMethod]
        public virtual void testRemoveFunctionCallExpr()
		{
			TExpression expression = parser.parseExpression("fx(columnA)");
			Assert.IsTrue(expression.ExpressionType == EExpressionType.function_t);
            expression.remove();
            Assert.IsTrue(expression.ExpressionType == EExpressionType.removed_t);
		}

        [TestMethod]
        public virtual void testRemoveColumnInFunctionCall()
		{
			TExpression expression = parser.parseExpression("fx(columnA,columnB,fx2(1+columnC))");
			Assert.IsTrue(expression.ExpressionType == EExpressionType.function_t);

			TExpressionList resultList = expression.searchColumn("columnC");
            Assert.IsTrue(resultList.size() == 1);
			TExpression columnCExpr = resultList.getExpression(0);
            columnCExpr.remove();

            Assert.IsTrue(expression.ExpressionType == EExpressionType.removed_t);
		}

        [TestMethod]
        public virtual void testSearchColumn()
		{
			TExpression expression = parser.parseExpression("columnA+(columnB*2)+columnC");
			TExpressionList resultList = expression.searchColumn("columnB");
            Assert.IsTrue(resultList.size() == 1);
			TExpression columnBExpr = resultList.getExpression(0);
			Assert.IsTrue(columnBExpr.ExpressionType == EExpressionType.simple_object_name_t);
			Assert.IsTrue(columnBExpr.ToString().Equals("columnB", StringComparison.CurrentCultureIgnoreCase));

			Assert.IsTrue(expression.ToScript().Equals("columnA + (columnB * 2) + columnC", StringComparison.OrdinalIgnoreCase));
            columnBExpr.remove();
            Assert.IsTrue(expression.ToScript().Equals(""));
		}

        [TestMethod]
        public virtual void testColumnInComparision()
		{
			TExpression expression = parser.parseExpression("columnA+(columnB*2)>columnC");
			TExpressionList resultList = expression.searchColumn("columnA");
            Assert.IsTrue(resultList.size() == 1);
			TExpression columnAExpr = resultList.getExpression(0);
			Assert.IsTrue(columnAExpr.ExpressionType == EExpressionType.simple_object_name_t);
			Assert.IsTrue(columnAExpr.ToString().Equals("columnA", StringComparison.CurrentCultureIgnoreCase));

			Assert.IsTrue(expression.ToScript().Equals("columnA + (columnB * 2) > columnC", StringComparison.OrdinalIgnoreCase));
            columnAExpr.remove();
            Assert.IsTrue(expression.ToScript().Equals(""));
		}

        [TestMethod]
        public virtual void testColumnInAndOr()
		{
			TExpression expression = parser.parseExpression("columnA+(columnB*2)>columnC and columnD=columnE-9");
			TExpressionList resultList = expression.searchColumn("columnA");
            Assert.IsTrue(resultList.size() == 1);
			TExpression columnAExpr = resultList.getExpression(0);
			Assert.IsTrue(columnAExpr.ExpressionType == EExpressionType.simple_object_name_t);
			Assert.IsTrue(columnAExpr.ToString().Equals("columnA", StringComparison.CurrentCultureIgnoreCase));

			Assert.IsTrue(expression.ToScript().Equals("columnA + (columnB * 2) > columnC and  columnD = columnE - 9", StringComparison.OrdinalIgnoreCase));
            columnAExpr.remove();
            Assert.IsTrue(expression.ToScript().Equals("columnD = columnE - 9", StringComparison.OrdinalIgnoreCase));
		}

        [TestMethod]
        public virtual void testColumnInAndOr1()
		{
			parser.sqltext = "select *\n" + "from table1 pal, table2 pualr, table3 pu\n" + "WHERE  (pal.application_location_id = pualr.application_location_id \n" + "         AND pu.jbp_uid = pualr.jbp_uid \n" + "         AND pu.username = 'USERID')";
			int ret = parser.parse();
			Assert.IsTrue(ret == 0);
			TSelectSqlStatement selectSqlStatement = (TSelectSqlStatement)parser.sqlstatements.get(0);

			TExpression expression = selectSqlStatement.WhereClause.Condition;

			TExpressionList resultList = expression.searchColumn("application_location_id");
            Assert.IsTrue(resultList.size() == 2);
			TExpression expression1 = resultList.getExpression(0);
			Assert.IsTrue(expression1.ExpressionType == EExpressionType.simple_object_name_t);
			Assert.IsTrue(expression1.ToString().Equals("pal.application_location_id", StringComparison.CurrentCultureIgnoreCase));
            expression1.remove();
            Assert.IsTrue(expression.ToScript().Equals("(pu.jbp_uid = pualr.jbp_uid and  pu.username = 'USERID')", StringComparison.OrdinalIgnoreCase));
		}


        [TestMethod]
        public virtual void testColumnInAndOr2()
		{
			parser.sqltext = "SELECT m.*, \n" + "       altname.last_name  last_name_student, \n" + "       altname.first_name first_name_student, \n" + "       ccu.date_joined, \n" + "       ccu.last_login, \n" + "       ccu.photo_id, \n" + "       ccu.last_updated \n" + "FROM   summit.mstr m, \n" + "       summit.alt_name altname, \n" + "       smmtccon.ccn_user ccu \n" + "WHERE  m.id =?\n" + "       AND m.id = altname.id(+) \n" + "       AND m.id = ccu.id(+) \n" + "       AND altname.grad_name_ind(+) = '*'";
			int ret = parser.parse();

			Assert.IsTrue(ret == 0);
			TSelectSqlStatement selectSqlStatement = (TSelectSqlStatement)parser.sqlstatements.get(0);

			TExpression expression = selectSqlStatement.WhereClause.Condition;

            expression.RightOperand.remove();
            Assert.IsTrue(expression.ToScript().Equals("m.id = ? and  m.id = altname.id(+) and  m.id = ccu.id(+)", StringComparison.OrdinalIgnoreCase));
            expression.RightOperand.remove();
            Assert.IsTrue(expression.ToScript().Equals("m.id = ? and  m.id = altname.id(+)", StringComparison.OrdinalIgnoreCase));
            expression.RightOperand.remove();
            Assert.IsTrue(expression.ToScript().Equals("m.id = ?", StringComparison.OrdinalIgnoreCase));
		}


        [TestMethod]
        public virtual void testColumnInAndOr3()
		{
			parser.sqltext = "select *\n" + "from  ods_trf_pnb_stuf_lijst_adrsrt2 lst\n" + "\t\t, ods_stg_pnb_stuf_pers_adr pas\n" + "\t\t, ods_stg_pnb_stuf_pers_nat nat\n" + "\t\t, ods_stg_pnb_stuf_adr adr\n" + "\t\t, ods_stg_pnb_stuf_np prs\n" + "where \n" + "\tpas.soort_adres = lst.soort_adres\n" + "\tand prs.id(+) = nat.prs_id\n" + "\tand adr.id = pas.adr_id\n" + "\tand prs.id = pas.prs_id\n" + "  and lst.persoonssoort = 'PERSOON'\n" + "   and pas.einddatumrelatie is null";
			int ret = parser.parse();
			Assert.IsTrue(ret == 0);
			TSelectSqlStatement selectSqlStatement = (TSelectSqlStatement)parser.sqlstatements.get(0);

			TExpression expression = selectSqlStatement.WhereClause.Condition;

			TExpressionList resultList = expression.searchColumn("lst.soort_adres");
            Assert.IsTrue(resultList.size() == 1);
			TExpression expression1 = resultList.getExpression(0);
			Assert.IsTrue(expression1.ExpressionType == EExpressionType.simple_object_name_t);
            expression1.remove();

            resultList = expression.searchColumn("nat.prs_id");
            Assert.IsTrue(resultList.size() == 1);
			expression1 = resultList.getExpression(0);
			Assert.IsTrue(expression1.ExpressionType == EExpressionType.simple_object_name_t);
            expression1.remove();

            resultList = expression.searchColumn("adr.id");
            Assert.IsTrue(resultList.size() == 1);
			expression1 = resultList.getExpression(0);
			Assert.IsTrue(expression1.ExpressionType == EExpressionType.simple_object_name_t);
            expression1.remove();

            resultList = expression.searchColumn("prs.id");
            Assert.IsTrue(resultList.size() == 1);
			expression1 = resultList.getExpression(0);
			Assert.IsTrue(expression1.ExpressionType == EExpressionType.simple_object_name_t);
            expression1.remove();

            Assert.IsTrue(expression.ToScript().Trim().Equals("lst.persoonssoort = \'PERSOON\' and  pas.einddatumrelatie is null",StringComparison.OrdinalIgnoreCase));
		}

	}

}