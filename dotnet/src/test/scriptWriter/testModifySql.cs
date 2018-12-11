using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gudusoft.gsqlparser.test.scriptWriter
{
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.nodes;
    using gudusoft.gsqlparser.stmt;
    using System.Text.RegularExpressions;

    [TestClass]
    public class testModifySql
    {

        private TGSqlParser parser = new TGSqlParser(EDbVendor.dbvoracle);

        [TestMethod]
        public virtual void testModifySelectList()
        {
            parser.sqltext = "select t1.f1, t2.f2 as f2 from table1 t1 left join table2 t2 on t1.f1 = t2.f2 ";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);

            select.ResultColumnList.removeElementAt(1);
            select.ResultColumnList.removeElementAt(0);

            TResultColumn resultColumn1 = new TResultColumn();
            resultColumn1.Expr = parser.parseExpression("t1.f3");
            TAliasClause aliasClause1 = new TAliasClause();
            aliasClause1.AliasName = parser.parseObjectName("f1");
            aliasClause1.HasAs = true;
            resultColumn1.AliasClause = aliasClause1;
            select.ResultColumnList.addResultColumn(resultColumn1);

            TResultColumn resultColumn2 = new TResultColumn();
            resultColumn2.Expr = parser.parseExpression("t2.f3");
            select.ResultColumnList.addResultColumn(resultColumn2);
            // System.out.println(scriptGenerator.generateScript(select,true));

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT t1.f3 AS f1,\n" +
                        "       t2.f3\n" +
                        "FROM   table1 t1\n" +
                        "       LEFT JOIN table2 t2\n" +
                        "       ON t1.f1 = t2.f2"
            ));
        }

        [TestMethod]
        public virtual void testFromClaueJoinTable()
        {
            parser.sqltext = "select table1.col1, table2.col2\n" + "from table1, table2\n" + "where table1.foo > table2.foo";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);

            select.joins.removeElementAt(1);

            TJoin join = new TJoin();
            select.joins.addJoin(join);
            //join.setWithParen(true);
            join.NestedParen = 1;
            TTable table1 = new TTable();
            table1.TableName = parser.parseObjectName("tableX");
            join.Table = table1;

            TJoinItem joinItem = new TJoinItem();
            join.JoinItems.addJoinItem(joinItem);
            joinItem.JoinType = EJoinType.join;
            TTable table2 = new TTable();
            table2.TableName = parser.parseObjectName("tableY");
            joinItem.Table = table2;


            TObjectNameList usingColumns = new TObjectNameList();
            usingColumns.addObjectName(parser.parseObjectName("id"));
            joinItem.UsingColumns = usingColumns;

            TAliasClause aliasClause = new TAliasClause();
            aliasClause.AliasName = parser.parseObjectName("table2");
            aliasClause.HasAs = true;
            join.AliasClause = aliasClause;

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                               , select.ToScript()
        , "SELECT table1.col1,\n" +
                "       table2.col2\n" +
                "FROM   table1, (\n" +
                "       tablex JOIN \n" +
                "       tabley USING (ID)) AS table2\n" +
                "WHERE  table1.foo > table2.foo"
        ));
        }


        [TestMethod]
        public virtual void testRemoveResultColumnInSelectList()
        {
            parser.sqltext = "SELECT A as A_Alias, B AS B_Alias FROM TABLE_X";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            TResultColumnList columns = select.ResultColumnList;
            columns.removeElementAt(1);
            TResultColumn resultColumn = new TResultColumn();
            resultColumn.Expr = parser.parseExpression("x");
            columns.addResultColumn(resultColumn);
            // System.out.println(scriptGenerator.generateScript(select, true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT a AS a_alias,\n" +
                "       x\n" +
                "FROM   table_x"
                ));
        }

        [TestMethod]

        public virtual void testAddResultColumnInSelectList()
        {
            parser.sqltext = "SELECT A as A_Alias, B AS B_Alias FROM TABLE_X";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            TResultColumnList columns = select.ResultColumnList;

            TResultColumn resultColumn = new TResultColumn();
            resultColumn.Expr = parser.parseExpression("d");
            columns.addResultColumn(resultColumn);
            TAliasClause aliasClause = new TAliasClause();
            aliasClause.AliasName = parser.parseObjectName("d_alias");
            aliasClause.HasAs = true;
            resultColumn.AliasClause = aliasClause;

            //  System.out.println(scriptGenerator.generateScript(select, true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT a AS a_alias,\n" +
                        "       b AS b_alias,\n" +
                        "       d AS d_alias\n" +
                        "FROM   table_x"
        ));

        }


        [TestMethod]
        public virtual void testRemoveTableInFromClauseAndRemoveWhereClause()
        {
            parser.sqltext = "SELECT * FROM t1,t2 where t1.f1=t2.f2";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            TJoinList joinList = select.joins;
            joinList.removeElementAt(0);
            select.WhereClause = null;

            // System.out.println(scriptGenerator.generateScript(select, true));

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT *\n" +
                        "FROM   t2"
        ));
        }


        [TestMethod]
        public virtual void testRemoveTableAndAddJoinClause()
        {
            parser.sqltext = "SELECT * FROM t1,t2 where t1.f1=t2.f2";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            TJoinList joinList = select.joins;
            // let's remove t2 and where clause
            joinList.removeElementAt(1);

            TJoinItem joinItem = new TJoinItem();
            joinList.getJoin(0).JoinItems.addJoinItem(joinItem);
            joinItem.JoinType = EJoinType.left;
            TTable joinTable = new TTable();
            joinItem.Table = joinTable;
            joinTable.TableName = parser.parseObjectName("t2");
            joinItem.OnCondition = parser.parseExpression("t1.f1=t2.f2");

            // remove where clause
            select.WhereClause = null;

            // System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT *\n" +
                        "FROM   t1\n" +
                        "       LEFT JOIN t2\n" +
                        "       ON t1.f1 = t2.f2"
        ));

        }


        [TestMethod]
        public virtual void testRemoveCTE()
        {
            parser.sqltext = "with test as (select id from emp)\n" + "select * from test";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            select.CteList = null;

            //  System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT *\n" +
                        "FROM   test"
        ));
        }


        [TestMethod]
        public virtual void testAddNewConditionInWhereClause2()
        {
            parser.sqltext = "SELECT * FROM TABLE_X where f > 0";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);

            TExpression expression1 = parser.parseExpression("c1>1");
            TExpression expression2 = new TExpression();
            expression2.ExpressionType = EExpressionType.logical_and_t;
            expression2.LeftOperand = select.WhereClause.Condition;
            expression2.RightOperand = expression1;
            select.WhereClause.Condition = expression2;

            //System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT *\n" +
                        "FROM   table_x\n" +
                        "WHERE  f > 0\n" +
                        "       AND c1 > 1"
        ));
        }


        [TestMethod]
        public virtual void testAddORConditionInWhereClause()
        {
            parser.sqltext = "SELECT * FROM TABLE_X where f > 0";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);

            TExpression expression1 = parser.parseExpression("c1>1");

            TExpression expression2 = new TExpression();
            expression2.ExpressionType = EExpressionType.logical_or_t;
            TExpression parensExpr = new TExpression();
            parensExpr.ExpressionType = EExpressionType.parenthesis_t;
            parensExpr.LeftOperand = select.WhereClause.Condition;
            expression2.LeftOperand = parensExpr;
            expression2.RightOperand = expression1;

            select.WhereClause.Condition = expression2;

            //System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT *\n" +
                        "FROM   table_x\n" +
                        "WHERE  ( f > 0 )\n" +
                        "       OR c1 > 1"
        ));
        }


        [TestMethod]
        public virtual void testAddNewConditionInWhereClause()
        {
            parser.sqltext = "select count(*) from TableName where NOT a OR NOT b";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);

            TExpression expression1 = parser.parseExpression("c1=1");
            TExpression expression2 = new TExpression();
            expression2.ExpressionType = EExpressionType.logical_and_t;
            TExpression parensExpr = new TExpression();
            parensExpr.ExpressionType = EExpressionType.parenthesis_t;
            parensExpr.LeftOperand = select.WhereClause.Condition;
            expression2.LeftOperand = parensExpr;
            expression2.RightOperand = expression1;
            select.WhereClause.Condition = expression2;

            // System.out.println(scriptGenerator.generateScript(select,true));

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT count(*)\n" +
                        "FROM   tablename\n" +
                        "WHERE  ( NOT a\n" +
                        "         OR NOT b )\n" +
                        "       AND c1 = 1"
        ));
        }


        [TestMethod]
        public virtual void testAddWhereClause2()
        {
            parser.sqltext = "SELECT * FROM TABLE_X";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            TWhereClause whereClause = new TWhereClause();
            select.WhereClause = whereClause;
            whereClause.Condition = parser.parseExpression("c>1");

            // System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT *\n" +
                        "FROM   table_x\n" +
                        "WHERE  c > 1"
        ));
        }


        [TestMethod]
        public virtual void testAddWhereClauseBeforeGrouBy()
        {
            parser.sqltext = "SELECT * FROM TABLE_X group by a";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);

            TWhereClause whereClause = new TWhereClause();
            select.WhereClause = whereClause;
            whereClause.Condition = parser.parseExpression("c>1");

            //   System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT   *\n" +
                        "FROM     table_x\n" +
                        "WHERE    c > 1\n" +
                        "GROUP BY a"
        ));

        }


        [TestMethod]
        public virtual void testAddWhereClauseAfterJoin()
        {
            parser.sqltext = "SELECT tableA.itemA1, tableB.itemB1\n" + " FROM tableA\n" + " INNER JOIN tableB\n" + " ON tableB.itemB2 = tableA.itemA2\n" + " INNER JOIN (\n" + "   SELECT tableC.itemC1\n" + "   FROM tableC\n" + "   WHERE tableC.itemC3='ABC'\n" + "   GROUP BY tableC.itemC1\n" + ") unNamedJoin\n" + " ON unNamedJoin.itemC1 = tableB.itemB2\n";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);

            TJoinList joinList = select.joins;
            TJoinItem item = joinList.getJoin(0).JoinItems.getJoinItem(0);

            TExpression expression1 = parser.parseExpression("1=1");
            TExpression expression2 = new TExpression();
            expression2.ExpressionType = EExpressionType.logical_and_t;
            TExpression parensExpr = new TExpression();
            parensExpr.ExpressionType = EExpressionType.parenthesis_t;
            parensExpr.LeftOperand = item.OnCondition;
            expression2.LeftOperand = parensExpr;
            expression2.RightOperand = expression1;
            item.OnCondition = expression2;

            TWhereClause whereClause = new TWhereClause();
            whereClause.Condition = parser.parseExpression("c>1");
            select.WhereClause = whereClause;


            // System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT tablea.itema1,\n" +
                        "       tableb.itemb1\n" +
                        "FROM   tablea\n" +
                        "       INNER JOIN tableb\n" +
                        "       ON (tableb.itemb2 = tablea.itema2) AND 1 = 1\n" +
                        "       INNER JOIN ( SELECT tablec.itemc1 FROM tablec WHERE tablec.itemc3 = 'ABC' GROUP BY tablec.itemc1) unnamedjoin\n" +
                        "       ON unnamedjoin.itemc1 = tableb.itemb2\n" +
                        "WHERE  c > 1"
        ));
        }


        [TestMethod]
        public virtual void testRemoveWhereClause()
        {
            parser.sqltext = "SELECT * FROM TABLE_X where a>1 order by a";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            select.WhereClause = null;
            // System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT   *\n" +
                        "FROM     table_x\n" +
                        "ORDER BY a"
        ));

        }


        [TestMethod]
        public virtual void testAddOrderByClause()
        {
            parser.sqltext = "SELECT * FROM TABLE_X";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);

            TOrderBy orderBy = new TOrderBy();
            select.OrderbyClause = orderBy;
            TOrderByItem orderByItem = new TOrderByItem();
            orderBy.Items.addElement(orderByItem);
            orderByItem.SortKey = parser.parseExpression("a");
            orderByItem.SortOrder = ESortType.desc;

            //System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT   *\n" +
                        "FROM     table_x\n" +
                        "ORDER BY a DESC"
        ));

            parser.sqltext = "SELECT * FROM TABLE_X where a>1";
            Assert.IsTrue(parser.parse() == 0);
            select = (TSelectSqlStatement)parser.sqlstatements.get(0);

            orderBy = new TOrderBy();
            select.OrderbyClause = orderBy;
            orderByItem = new TOrderByItem();
            orderBy.Items.addElement(orderByItem);
            orderByItem.SortKey = parser.parseExpression("a");
            orderByItem.SortOrder = ESortType.desc;

            //  System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT   *\n" +
                        "FROM     table_x\n" +
                        "WHERE    a > 1\n" +
                        "ORDER BY a DESC"
        ));

            parser.sqltext = "SELECT * FROM TABLE_X where a>1 group by a having count(*) > 1";
            Assert.IsTrue(parser.parse() == 0);
            select = (TSelectSqlStatement)parser.sqlstatements.get(0);

            orderBy = new TOrderBy();
            select.OrderbyClause = orderBy;
            orderByItem = new TOrderByItem();
            orderBy.Items.addElement(orderByItem);
            orderByItem.SortKey = parser.parseExpression("a");
            orderByItem.SortOrder = ESortType.asc;

            // System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT   *\n" +
                        "FROM     table_x\n" +
                        "WHERE    a > 1\n" +
                        "GROUP BY a\n" +
                        "HAVING  count(*) > 1\n" +
                        "ORDER BY a ASC"
        ));

            parser.sqltext = "SELECT * FROM TABLE_X where a>1 group by a having count(*) > 1 order by c desc";
            Assert.IsTrue(parser.parse() == 0);
            select = (TSelectSqlStatement)parser.sqlstatements.get(0);

            orderByItem = new TOrderByItem();
            orderBy.Items.addElement(orderByItem);
            orderByItem.SortKey = parser.parseExpression("a");
            orderByItem.SortOrder = ESortType.asc;
            select.OrderbyClause.Items.addOrderByItem(orderByItem);
            //  System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT   *\n" +
                        "FROM     table_x\n" +
                        "WHERE    a > 1\n" +
                        "GROUP BY a\n" +
                        "HAVING  count(*) > 1\n" +
                        "ORDER BY c DESC,\n" +
                        "         a ASC"
        ));

            parser.sqltext = "SELECT * FROM TABLE_X";
            Assert.IsTrue(parser.parse() == 0);
            select = (TSelectSqlStatement)parser.sqlstatements.get(0);

            TWhereClause whereClause = new TWhereClause();
            whereClause.Condition = parser.parseExpression("a>1 and b>2");
            select.WhereClause = whereClause;
            //select.addWhereClause("a>1 and b>2") ;

            orderBy = new TOrderBy();
            select.OrderbyClause = orderBy;
            orderByItem = new TOrderByItem();
            orderBy.Items.addElement(orderByItem);
            orderByItem.SortKey = parser.parseExpression("a");
            orderByItem.SortOrder = ESortType.desc;

            //System.out.println(scriptGenerator.generateScript(select,true));

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT   *\n" +
                        "FROM     table_x\n" +
                        "WHERE    a > 1\n" +
                        "         AND b > 2\n" +
                        "ORDER BY a DESC"
        ));

        }


        [TestMethod]
        public virtual void testRemoveItemInOrderByClause()
        {
            parser.sqltext = "SELECT * FROM TABLE_X order by a,b";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            select.OrderbyClause.Items.removeElementAt(1);
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT   *\n" +
                        "FROM     table_x\n" +
                        "ORDER BY a"
        ));

            select.OrderbyClause = null;
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT *\n" +
                        "FROM   table_x"
        ));
        }


        [TestMethod]
        public virtual void testReplaceOrderByItemAndAddSortType()
        {
            parser.sqltext = "SELECT * FROM TABLE_X order by a";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            select.OrderbyClause.Items.removeElementAt(0);
            TOrderBy orderBy = select.OrderbyClause;

            TOrderByItem orderByItem = new TOrderByItem();
            orderBy.Items.addElement(orderByItem);
            orderByItem.SortKey = parser.parseExpression("b");
            orderByItem.SortOrder = ESortType.asc;

            orderByItem = new TOrderByItem();
            orderBy.Items.addElement(orderByItem);
            orderByItem.SortKey = parser.parseExpression("a1");
            orderByItem.SortOrder = ESortType.desc;



            //System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT   *\n" +
                        "FROM     table_x\n" +
                        "ORDER BY b ASC,\n" +
                        "         a1 DESC"
        ));
        }


        [TestMethod]
        public virtual void testRemoveSetClauseInUpdate()
        {
            parser.sqltext = "UPDATE BLA SET A=2, B=3 WHERE X=5";
            Assert.IsTrue(parser.parse() == 0);

            TUpdateSqlStatement updateSqlStatement = (TUpdateSqlStatement)parser.sqlstatements.get(0);
            TResultColumnList setClauses = updateSqlStatement.ResultColumnList;
            setClauses.removeElementAt(0);
            // System.out.println(scriptGenerator.generateScript(updateSqlStatement, true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , updateSqlStatement.ToScript()
                , "UPDATE bla\n" +
                        "SET    b=3\n" +
                        "WHERE  x = 5"
        ));
        }


        [TestMethod]
        public virtual void testModifyJoinCondition()
        {
            parser.sqltext = "select * from t1 inner join t2 on t1.col1 = t2.col2";
            Assert.IsTrue(parser.parse() == 0);

            TSelectSqlStatement selectSqlStatement = (TSelectSqlStatement)parser.sqlstatements.get(0);
            TJoin join = selectSqlStatement.joins.getJoin(0);
            TTable table = join.Table;
            table.TableName = parser.parseObjectName("t2");
            TJoinItem joinItem = join.JoinItems.getJoinItem(0);
            table = joinItem.Table;
            table.TableName = parser.parseObjectName("t1");
            joinItem.OnCondition = parser.parseExpression("t1.col3 = t2.col5");

            // System.out.println(scriptGenerator.generateScript(selectSqlStatement, true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , selectSqlStatement.ToScript()
                , "SELECT *\n" +
                        "FROM   t2\n" +
                        "       INNER JOIN t1\n" +
                        "       ON t1.col3 = t2.col5"
        ));
        }


        [TestMethod]
        public virtual void testModifyTableInFromClause()
        {
            parser.sqltext = "select * from t1";
            Assert.IsTrue(parser.parse() == 0);

            TTable table = parser.sqlstatements.get(0).tables.getTable(0);
            table.TableName = parser.parseObjectName("newt");
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , parser.sqlstatements.get(0).ToScript()
                , "SELECT *\n" +
                        "FROM   newt"
        ));
        }


        [TestMethod]
        public virtual void testAddTableAlias()
        {
            parser.sqltext = "select * from t1";
            Assert.IsTrue(parser.parse() == 0);

            TTable table = parser.sqlstatements.get(0).tables.getTable(0);
            TAliasClause aliasClause = new TAliasClause();
            aliasClause.HasAs = true;
            aliasClause.AliasName = parser.parseObjectName("foo");
            table.AliasClause = aliasClause;

            //Assert.IsTrue(parser.sqlstatements.get(0).toString().trim().Equals("select * from t1 AS foo", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , parser.sqlstatements.get(0).ToScript()
                , "SELECT *\n" +
                        "FROM   t1 AS foo"
        ));
        }


        [TestMethod]
        public virtual void testModifyTableInCreateTable()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvimpala);
            sqlparser.sqltext = "create table if not exists campaign_1 ( id int, name string )";
            int ret = sqlparser.parse();
            TCustomSqlStatement stmt = sqlparser.sqlstatements.get(0);
            TTable table = stmt.tables.getTable(0);
            table.TableName = parser.parseObjectName("prefix_." + table.TableName.ToString());
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , table.ToScript()
                , "prefix_.campaign_1"
        ));

        }


        [TestMethod]
        public virtual void testRemoveHavingClause()
        {
            parser.sqltext = "SELECT\n" + "c.ID AS \"SMS.ID\"\n" + "FROM\n" + "SUMMIT.cntrb_detail c\n" + "where\n" + "c.cntrb_date >='$GivingFromDate$'\n" + "and c.cntrb_date<='$GivingThruDate$'\n" + "group by c.id\n" + "having sum(c.amt) >= '$GivingFromAmount$' and sum(c.amt) <= '$GivingThruAmount$'";
            Assert.IsTrue(parser.parse() == 0);

            TSelectSqlStatement selectSqlStatement = (TSelectSqlStatement)parser.sqlstatements.get(0);
            TGroupBy groupBy = selectSqlStatement.GroupByClause;
            groupBy.HavingClause = null;
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , selectSqlStatement.ToScript()
                , "SELECT   c.ID AS \"SMS.ID\"\n" +
                        "FROM     summit.cntrb_detail c\n" +
                        "WHERE    c.cntrb_date >= '$GivingFromDate$'\n" +
                        "         AND c.cntrb_date <= '$GivingThruDate$'\n" +
                        "GROUP BY c.ID"
        ));
        }


        [TestMethod]
        public virtual void testAddRefernceColumnInAlterTable()
        {
            TGSqlParser lcparser = new TGSqlParser(EDbVendor.dbvoracle);
            lcparser.sqltext = "ALTER TABLE P_CAP \n" + "ADD CONSTRAINT FK_P_CAP_R_PH_111_P_CEL \n" + "FOREIGN KEY (CAP_CEL) REFERENCES P_CEL (CEL_COD);";
            Assert.IsTrue(lcparser.parse() == 0);
            TAlterTableStatement at = (TAlterTableStatement)lcparser.sqlstatements.get(0);
            TAlterTableOption alterTableOption = at.AlterTableOptionList.getAlterTableOption(0);
            TConstraint constraint = alterTableOption.ConstraintList.getConstraint(0);
            Assert.IsTrue(constraint.Constraint_type == EConstraintType.foreign_key);

            constraint.ReferencedColumnList.addObjectName(parser.parseObjectName("CEL_NEWID"));
            //System.out.println(scriptGenerator.generateScript(at, true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , at.ToScript()
                , "ALTER TABLE p_cap \n" +
                        "  ADD CONSTRAINT fk_p_cap_r_ph_111_p_cel FOREIGN KEY (cap_cel) REFERENCES p_cel(cel_cod,cel_newid)"
        ));
        }

        private string format(string value)
        {
            Regex rgx = new Regex("\\s+");
            return rgx.Replace(value, " ");
        }

        [TestMethod]
        public virtual void testAddRefernceColumnInAlterTable2()
        {
            TGSqlParser lcparser = new TGSqlParser(EDbVendor.dbvoracle);
            lcparser.sqltext = "ALTER TABLE P_CAP \n" + "ADD CONSTRAINT FK_P_CAP_R_PH_111_P_CEL \n" + "FOREIGN KEY (CAP_CEL) REFERENCES P_CEL (CEL_COD);";
            Assert.IsTrue(lcparser.parse() == 0);
            TAlterTableStatement at = (TAlterTableStatement)lcparser.sqlstatements.get(0);
            TAlterTableOption alterTableOption = at.AlterTableOptionList.getAlterTableOption(0);
            TConstraint constraint = alterTableOption.ConstraintList.getConstraint(0);
            Assert.IsTrue(constraint.Constraint_type == EConstraintType.foreign_key);

            constraint.ReferencedColumnList.insertElementAt(parser.parseObjectName("cel_newid"), 0);
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , at.ToScript()
                , "ALTER TABLE p_cap \n" +
                        "  ADD CONSTRAINT fk_p_cap_r_ph_111_p_cel FOREIGN KEY (cap_cel) REFERENCES p_cel(cel_newid,cel_cod)"
        ));
        }


        [TestMethod]
        public virtual void testAddWhereClause()
        {
            TGSqlParser lcparser = new TGSqlParser(EDbVendor.dbvoracle);
            lcparser.sqltext = "SELECT * FROM TABLE_X";
            Assert.IsTrue(lcparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)lcparser.sqlstatements.get(0);
            TWhereClause whereClause = new TWhereClause();
            select.WhereClause = whereClause;
            whereClause.Condition = parser.parseExpression("f > 0");
            //System.out.println(scriptGenerator.generateScript(select,true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT *\n" +
                        "FROM   table_x\n" +
                        "WHERE  f > 0"
        ));
        }


        [TestMethod]
        public virtual void testRemoveAdditionalParenthesisOfSubquery()
        {
            TSelectSqlStatement select = null, subquery = null;
            parser.sqltext = "select * from ((select * from some_table where some_column < ?)) some_view where a_column = something";
            Assert.IsTrue(parser.parse() == 0);
            select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            subquery = select.tables.getTable(0).Subquery;
            subquery.ParenthesisCount = 1;
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT *\n" +
                        "FROM   (SELECT *\n" +
                        "        FROM   some_table\n" +
                        "        WHERE  some_column < ?) some_view\n" +
                        "WHERE  a_column = something"
        ));

            parser.sqltext = "(((select a from b)) order by a)";
            Assert.IsTrue(parser.parse() == 0);
            select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            select.ParenthesisCount = 0;
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "(( SELECT   a\n" +
                        "   FROM     b))\n" +
                        "   ORDER BY a"
        ));

            parser.sqltext = "((((select a from b)) order by a))";
            Assert.IsTrue(parser.parse() == 0);
            select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            select.ParenthesisCount = 1;
            select.ParenthesisCountBeforeOrder = 1;
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "(( SELECT   a\n" +
                        "   FROM     b)\n" +
                        "   ORDER BY a)"
        ));

            parser.sqltext = "select * from user_table where ((username like '%admin%'));";
            Assert.IsTrue(parser.parse() == 0);
            select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            TExpression expression = select.WhereClause.Condition;
            select.WhereClause.Condition = expression.LeftOperand;
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT *\n" +
                        "FROM   user_table\n" +
                        "WHERE  ( username LIKE '%admin%' )"
        ));
        }

        [TestMethod]
        public virtual void testSetNewWhereCondition()
        {
            parser.sqltext = "select t1.f1 from table1 t1 where t1.f2 = 2 ";
            Assert.IsTrue(parser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)parser.sqlstatements.get(0);
            select.WhereClause.Condition = parser.parseExpression("t1.f2>2");
            
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , "SELECT t1.f1\n" +
                        "FROM   table1 t1\n" +
                        "WHERE  t1.f2 > 2"
             ));
        }


        [TestMethod]
        public virtual void testOracleBindVariable()
        {
            parser.sqltext = "UPDATE Art SET Desc=:Desc WHERE ID=:ID";
            Assert.IsTrue(parser.parse() == 0);
            TUpdateSqlStatement updateStmt = (TUpdateSqlStatement)parser.sqlstatements.get(0);

            TResultColumn setClause = updateStmt.ResultColumnList.getResultColumn(0);
            TExpression assignment = setClause.Expr;
            // create a vairable 
            TObjectName bindVar = new TObjectName(new TSourceToken(":bindVar"), EDbObjectType.variable);
            TExpression newVariable = new TExpression(EExpressionType.simple_object_name_t);
            newVariable.ObjectOperand = bindVar;
            //set new varaible
            assignment.RightOperand = newVariable;

            //Console.WriteLine(updateStmt.ToScript());

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , updateStmt.ToScript()
                , "UPDATE Art SET Desc=:bindVar WHERE ID=:ID"
             ));
        }

        [TestMethod]
        public virtual void testOracleBindVar()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"select emp_id, emp_dept into :b0, :b2 from T1 where rownum < 2;";

            sqlparser.parse();

            //Console.WriteLine(sqlparser.sqlstatements.get(0).ToScript());
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }


        [TestMethod]
        public virtual void testOracleAddHint()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"select distinct emp_id from T1";
            sqlparser.parse();
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            select.OracleHint = "/*+ FULL(products) */";

            //Console.WriteLine(sqlparser.sqlstatements.get(0).ToScript());
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , @"select /*+ FULL(products) */ distinct 
                    emp_id
                     from 
                    T1"
             ));
        }

        [TestMethod]
        public virtual void testOracleHintModify()
        {
            parser.sqltext = @"merge into products p
                                using newproducts np
                                on(p.product_id = np.product_id)
                                when matched then
                                update
                                set p.product_name = np.product_name, p.category = np.category";
            Assert.IsTrue(parser.parse() == 0);
            TMergeSqlStatement mergeStmt = (TMergeSqlStatement)parser.sqlstatements.get(0);
            mergeStmt.OracleHint = "/*+ FULL(products) */";

            //Console.WriteLine(mergeStmt.ToScript());

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , mergeStmt.ToScript()
                , @"merge /*+ FULL(products) */ into products p using newproducts np on (p.product_id = np.product_id)
 when matched then update set p.product_name=np.product_name,p.category=np.category"
             ));
        }

        [TestMethod]
        public virtual void testModifyAlias()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = @"Select X as [Y]";

            sqlparser.parse();
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TResultColumn resultColumn = select.ResultColumnList.getResultColumn(0);
            TAliasClause aliasClause = resultColumn.AliasClause;
            aliasClause.AliasName = parser.parseObjectName("\"Y\""); 

            //Console.WriteLine(sqlparser.sqlstatements.get(0).ToScript());
        }

        [TestMethod]
        public virtual void testRemoveIntoClause()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"SELECT * INTO newTbl FROM ( SELECT * FROM Instructor) AS tmp;";

            sqlparser.parse();
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            // remove into clause
            select.IntoClause = null;
            // remove * in the select list
            select.ResultColumnList.removeElementAt(0);

            // add a new select list item
            TResultColumn resultColumn = new TResultColumn();
            resultColumn.Expr = sqlparser.parseExpression("count(*)");
            TAliasClause aliasClause = new TAliasClause();
            aliasClause.AliasName = sqlparser.parseObjectName("count");
            aliasClause.HasAs = true;
            resultColumn.AliasClause = aliasClause;

            select.ResultColumnList.addResultColumn(resultColumn);

            // Console.WriteLine(sqlparser.sqlstatements.get(0).ToScript());
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle
                , select.ToScript()
                , @"select 
                    count(*) as count
                     from 
                    ( select 
                    *
                     from 
                    Instructor) as tmp"
                                 ));

        }

        //[TestMethod]
        //public virtual void testModifyColumnDefinition()
        //{
        //    TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
        //    sqlparser.sqltext = @"CREATE TABLE `DEPT_MANAGER_TBL` ( 
        //             `EMP_NO` INT(4) unsigned zerofill NOT NULL DEFAULT 1000, 
        //             `DEPT_NO` CHAR(4) CHARACTER SET latin1 COLLATE latin1_german1_ci NOT NULL, 
        //             `TO_DATE` GEOMETRY NOT NULL , `FROM_DATE` DATE NOT NULL, 
        //             PRIMARY KEY (`EMP_NO`, `DEPT_NO`)
        //            ) COLLATE=utf8_unicode_ci;";

        //    sqlparser.parse();

        //    Console.WriteLine(sqlparser.sqlstatements.get(0).ToScript());

        //}

    }

}