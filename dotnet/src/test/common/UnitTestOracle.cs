using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.IO;
using gudusoft.gsqlparser.stmt;
using gudusoft.gsqlparser.nodes;
using gudusoft.gsqlparser.nodes.oracle;
using gudusoft.gsqlparser.stmt.oracle;

namespace gudusoft.gsqlparser.test
{
    /// <summary>
    /// UnitTestOracle 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTestOracle
    {
        TGSqlParser parser;

        public UnitTestOracle()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
            parser = new TGSqlParser(EDbVendor.dbvoracle);
        }

        private TestContext testContextInstance;

        /// <summary>
        ///获取或设置测试上下文，该上下文提供
        ///有关当前测试运行及其功能的信息。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性: 
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestQuery()
        {
            parser.sqltext = "select f from t where f>1";
            int ret = parser.parse();
            Assert.IsTrue(ret == 0, parser.Errormessage);
        }

        [TestMethod]
        public void TestOracleFiles()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"oracle\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public void TestOracleFiles2()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"new_dotnet\oracle\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            List<String> excludeFiles = new List<string>{
                "ZINV_MST_PRE_CHK.sql" ,"sqlplus_after_query.sql","truncate_table.sql",
                "new_keyword_in_plsql.sql","treat_in_plsql.sql","691.sql","694_compound_trigger.sql",
                "715_flashback_archive.sql","717_concatenation_op.sql","718_truncate_partition.sql",
                "726_suplemental.sql"
            };

            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                if (UnitTestCommon.excludeFile(info.Name,excludeFiles) ){
                    continue;
                }
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public void testXMLElement()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select\n" + "  column_a,\n" + "  xmlelement(\"ns1:item\",\n" + "   xmlelement(\"ns2:item1\",\n" + "     xmlattributes(\n" + "      attr1 \"attribute\",\n" + "      nvl((select code from mapping where code = 'Default'),'0') \"code\"\n" + "     ),\n" + "   xmlelement(\"ns2:item2\",\n" + "     xmlattributes('type' \"type\"),\n" + "     xmlagg(factor order by name)))) sample_item\n" + "from TABLE_ABC";
            Assert.IsTrue(sqlparser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TResultColumn column = select.ResultColumnList.getResultColumn(1);
            TExpression expr = column.Expr;
            TFunctionCall f = expr.FunctionCall;
            Assert.IsTrue(f.FunctionType == EFunctionType.xmlelement_t);
            Assert.IsTrue(f.XMLElementNameExpr.ToString().EndsWith("\"ns1:item\"", StringComparison.Ordinal));
            Assert.IsTrue(f.XMLElementValueExprList.size() == 1);
            TExpression expr1 = f.XMLElementValueExprList.getResultColumn(0).Expr;
            Assert.IsTrue(expr1.ExpressionType == EExpressionType.function_t);
            f = expr1.FunctionCall;
            Assert.IsTrue(f.FunctionType == EFunctionType.xmlelement_t);
            Assert.IsTrue(f.XMLElementNameExpr.ToString().EndsWith("\"ns2:item1\"", StringComparison.Ordinal));
            TXMLAttributesClause xmlac = f.XMLAttributesClause;
            Assert.IsTrue(xmlac.ValueExprList.size() == 2);
            Assert.IsTrue(xmlac.ValueExprList.getResultColumn(0).Expr.ToString().EndsWith("attr1", StringComparison.Ordinal));
            Assert.IsTrue(xmlac.ValueExprList.getResultColumn(0).AliasClause.ToString().EndsWith("\"attribute\"", StringComparison.Ordinal));
            Assert.IsTrue(xmlac.ValueExprList.getResultColumn(1).AliasClause.ToString().EndsWith("\"code\"", StringComparison.Ordinal));
            expr1 = xmlac.ValueExprList.getResultColumn(1).Expr;
            Assert.IsTrue(expr1.ExpressionType == EExpressionType.function_t);
            f = expr1.FunctionCall;
            Assert.IsTrue(f.FunctionName.ToString().EndsWith("nvl", StringComparison.Ordinal));
            Assert.IsTrue(f.Args.getExpression(0).ExpressionType == EExpressionType.subquery_t);
            TSelectSqlStatement subquery = f.Args.getExpression(0).SubQuery;
            Assert.IsTrue(subquery.WhereClause.ToString().EndsWith("code = 'Default'", StringComparison.Ordinal));

        }

        [TestMethod]
        public void testXMLSERIALIZE()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT columna,\n" + "    XMLSERIALIZE(CONTENT DECODE(position_moniker, NULL, NULL, (SELECT DECODE(COUNT(*), 0, NULL, XMLELEMENT(\"Item\", XMLAGG(xml)))\n" + "                                                   from generic_item gi\n" + "                                                   WHERE gi.item_id = 10)) AS CLOB) items,\n" + "      columnc\n" + "  FROM TABLE_A";
            Assert.IsTrue(sqlparser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TResultColumn column = select.ResultColumnList.getResultColumn(1);
            TExpression expr = column.Expr;
            TFunctionCall f = expr.FunctionCall;
            Assert.IsTrue(f.FunctionType == EFunctionType.xmlserialize_t);
            expr = f.Expr1;
            Assert.IsTrue(expr.ExpressionType == EExpressionType.function_t);
            f = expr.FunctionCall;
            Assert.IsTrue(f.FunctionName.ToString().Equals("decode", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(f.Args.size() == 4);
            Assert.IsTrue(f.Args.getExpression(3).ExpressionType == EExpressionType.subquery_t);
            TSelectSqlStatement subquery = f.Args.getExpression(3).SubQuery;
            Assert.IsTrue(subquery.tables.getTable(0).ToString().Equals("generic_item", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testSequence()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE SEQUENCE SOME_SEQ increment by 1 start with 1 MAXVALUE 1.0E28 MINVALUE 1 NOCYCLE NOCACHE NOORDER;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateSequenceStmt sequenceStmt = (TCreateSequenceStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(sequenceStmt.SequenceName.ToString().Equals("SOME_SEQ", StringComparison.CurrentCultureIgnoreCase));
            for (int i = 0; i < sequenceStmt.Options.Count; i++)
            {
                TSequenceOption sequenceOption = sequenceStmt.Options[i];
                // System.out.println(dummy.toString());
            }
            //System.out.println(sequenceStmt.getSequenceName().toString());
        }

        [TestMethod]
        public void testRestrictionClause()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE OR REPLACE VIEW v1(v_c1, v_c2, v_c3) AS SELECT C1, C2, C3 FROM T1\n" + "WITH CHECK OPTION CONSTRAINT SYS_Cn;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateViewSqlStatement viewSqlStatement = (TCreateViewSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(viewSqlStatement.ViewName.ToString().Equals("v1", StringComparison.CurrentCultureIgnoreCase));

            TRestrictionClause r = viewSqlStatement.RestrictionClause;
            Assert.IsTrue(r.Type == TRestrictionClause.with_check_option);
            Assert.IsTrue(r.ConstraintName.ToString().Equals("SYS_Cn", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testplsqldatatype1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE OR REPLACE PROCEDURE \"PROC4\"( \"A1\" IN NUMBER, \"A2\" NUMBER ) IS\n" + "\tf0 NATURAL;\n" + "\tg1 NATURALN;\n" + "\th2 POSITIVE;\n" + "\ti3 POSITIVEN;\n" + "\tj4 SIGNTYPE;\n" + "\tk5 SIMPLE_INTEGER := 2147483645;\n" + "\tab6 ROWID;\n" + "\tac7 UROWID;\n" + "\taf8 STRING(10);\n" + "\tag9 BOOLEAN;\n" + "\tah10 DATE;\n" + "\tah11 NVARCHAR(10);\n" + "\tam  INTERVAL DAY(3) TO SECOND(3);\n" + "BEGIN\n" + "  NULL;\n" + "END;\n" + "/";
            Assert.IsTrue(sqlparser.parse() == 0);

            TPlsqlCreateProcedure createProcedure = (TPlsqlCreateProcedure)sqlparser.sqlstatements.get(0);
            TStatementList declares = createProcedure.DeclareStatements;
            //System.out.println(declares.get(0).sqlstatementtype);
            TVarDeclStmt varDeclStmt0 = (TVarDeclStmt)declares.get(0);
            TVarDeclStmt varDeclStmt1 = (TVarDeclStmt)declares.get(1);
            TVarDeclStmt varDeclStmt2 = (TVarDeclStmt)declares.get(2);
            TVarDeclStmt varDeclStmt3 = (TVarDeclStmt)declares.get(3);
            TVarDeclStmt varDeclStmt4 = (TVarDeclStmt)declares.get(4);
            TVarDeclStmt varDeclStmt5 = (TVarDeclStmt)declares.get(5);
            TVarDeclStmt varDeclStmt6 = (TVarDeclStmt)declares.get(6);
            TVarDeclStmt varDeclStmt7 = (TVarDeclStmt)declares.get(7);
            TVarDeclStmt varDeclStmt8 = (TVarDeclStmt)declares.get(8);
            TVarDeclStmt varDeclStmt9 = (TVarDeclStmt)declares.get(9);
            TVarDeclStmt varDeclStmt10 = (TVarDeclStmt)declares.get(10);
            TVarDeclStmt varDeclStmt11 = (TVarDeclStmt)declares.get(11);
            Assert.IsTrue(varDeclStmt0.DataType.DataType == EDataType.natural_t);
            Assert.IsTrue(varDeclStmt1.DataType.DataType == EDataType.naturaln_t);
            Assert.IsTrue(varDeclStmt2.DataType.DataType == EDataType.positive_t);
            Assert.IsTrue(varDeclStmt3.DataType.DataType == EDataType.positiven_t);
            Assert.IsTrue(varDeclStmt4.DataType.DataType == EDataType.signtype_t);
            Assert.IsTrue(varDeclStmt5.DataType.DataType == EDataType.simple_integer_t);
            Assert.IsTrue(varDeclStmt6.DataType.DataType == EDataType.rowid_t);
            Assert.IsTrue(varDeclStmt7.DataType.DataType == EDataType.urowid_t);
            Assert.IsTrue(varDeclStmt8.DataType.DataType == EDataType.string_t);
            Assert.IsTrue(varDeclStmt9.DataType.DataType == EDataType.boolean_t);
            Assert.IsTrue(varDeclStmt10.DataType.DataType == EDataType.date_t);
            Assert.IsTrue(varDeclStmt11.DataType.DataType == EDataType.nvarchar_t);
        }

        [TestMethod]
        public void testplsqldatatype2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE OR REPLACE PROCEDURE TESTPROC1( A1 NUMBER )\n" + "IS\n" + "TYPE TYP1 IS TABLE OF PLS_INTEGER INDEX BY VARCHAR2(64);\n" + "BEGIN\n" + "NULL;\n" + "END;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TPlsqlCreateProcedure createProcedure = (TPlsqlCreateProcedure)sqlparser.sqlstatements.get(0);
            TStatementList declares = createProcedure.DeclareStatements;
            TPlsqlTableTypeDefStmt varDeclStmt0 = (TPlsqlTableTypeDefStmt)declares.get(0);
            Assert.IsTrue(varDeclStmt0.IndexByDataType.DataType == EDataType.varchar2_t);
            //System.out.println(varDeclStmt0.getIndexByDataType().getDataType());
        }

        [TestMethod]
        public void testPivot1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT * FROM pivot_table\n" + "  UNPIVOT (yearly_total FOR order_mode IN (store AS 'direct', internet AS 'online'))\n" + "  ORDER BY year, order_mode;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.TargetTable.TableType == ETableSource.pivoted_table);
            TTable table = select.tables.getTable(0);
            Assert.IsTrue(table.ToString().Equals("pivot_table", StringComparison.CurrentCultureIgnoreCase));

            TPivotedTable pivotedTable = select.TargetTable.PivotedTable;

            TPivotClause pivotClause = pivotedTable.PivotClauseList[0];
            Assert.IsTrue(pivotClause.Type == TPivotClause.unpivot);
            Assert.IsTrue(pivotClause.ValueColumnList.getObjectName(0).ToString().Equals("yearly_total", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(pivotClause.PivotColumnList.getObjectName(0).ToString().Equals("order_mode", StringComparison.CurrentCultureIgnoreCase));

            TUnpivotInClause inClause = pivotClause.UnpivotInClause;
            TUnpivotInClauseItem item0 = inClause.Items[0];
            Assert.IsTrue(item0.Column.ToString().Equals("store", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(item0.Constant.ToString().Equals("'direct'", StringComparison.CurrentCultureIgnoreCase));

            TUnpivotInClauseItem item1 = inClause.Items[1];
            Assert.IsTrue(item1.Column.ToString().Equals("internet", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(item1.Constant.ToString().Equals("'online'", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testPivot2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT * FROM orders\n" + "PIVOT(SUM(order_total) \n" + "\t\tFOR order_mode IN (SELECT 'direct' AS Store, 'online' AS Internet FROM orders));";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.TargetTable.TableType == ETableSource.pivoted_table);
            TTable table = select.tables.getTable(0);
            Assert.IsTrue(table.ToString().Equals("orders", StringComparison.CurrentCultureIgnoreCase));
            TPivotedTable pivotedTable = select.TargetTable.PivotedTable;
            TPivotClause pivotClause = pivotedTable.PivotClauseList[0];
            Assert.IsTrue(pivotClause.Type == TPivotClause.pivot);

            TPivotInClause inClause = pivotClause.PivotInClause;
            select = inClause.SubQuery;
            Assert.IsTrue(select.ResultColumnList.size() == 2);
            Assert.IsTrue(select.tables.getTable(0).ToString().Equals("orders", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testOracleHint()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "create or replace view test22 as select /*+  RULE  */ t.account_name, t.account_number from  AP13_BANK_ACCOUNTS t;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateViewSqlStatement viewSqlStatement = (TCreateViewSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(viewSqlStatement.ViewName.ToString().Equals("test22", StringComparison.CurrentCultureIgnoreCase));

            TSelectSqlStatement select = viewSqlStatement.Subquery;
            Assert.IsTrue(string.Equals(select.OracleHint,"/*+  RULE  */",StringComparison.CurrentCultureIgnoreCase));

            sqlparser.sqltext = "delete /*+ FULL(t1) */ from t1;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TDeleteSqlStatement deleteStmt = (TDeleteSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(string.Equals(deleteStmt.OracleHint, "/*+ FULL(t1) */", StringComparison.CurrentCultureIgnoreCase));

            sqlparser.sqltext = "update /*+ FULL(t2) */ t1 set emp_id=1;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TUpdateSqlStatement udpateStmt = (TUpdateSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(string.Equals(udpateStmt.OracleHint, "/*+ FULL(t2) */", StringComparison.CurrentCultureIgnoreCase));

            sqlparser.sqltext = "insert /*+ FULL(t3) */ into t1 values(1);";
            Assert.IsTrue(sqlparser.parse() == 0);
            TInsertSqlStatement insertStmt = (TInsertSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(string.Equals(insertStmt.OracleHint, "/*+ FULL(t3) */", StringComparison.CurrentCultureIgnoreCase));

            sqlparser.sqltext = @"merge /*+ FULL(t4) */ into products p
  using newproducts np
  on(p.product_id = np.product_id)
  when matched then
  update
  set p.product_name = np.product_name";
            Assert.IsTrue(sqlparser.parse() == 0);
            TMergeSqlStatement mergeStmt = (TMergeSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(string.Equals(mergeStmt.OracleHint, "/*+ FULL(t4) */", StringComparison.CurrentCultureIgnoreCase));
        }



        [TestMethod]
        public void testOffsetClause1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT name FROM Temp_Test\n" + "ORDER BY name\n" + "OFFSET 2 ROWS FETCH NEXT 4 ROWS ONLY;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TOffsetClause offsetClause = select.OffsetClause;
            Assert.IsTrue(offsetClause.SelectOffsetValue.ToString().Equals("2", StringComparison.CurrentCultureIgnoreCase));
            TFetchFirstClause fetchFirstClause = select.FetchFirstClause;
            Assert.IsTrue(fetchFirstClause.FetchValue.ToString().Equals("4", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testObjectAccess1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT\n" + "   O.OBJECT_ID,\n" + "   XMLAGG (XMLELEMENT (K, O.KEY_1 || '|')).EXTRACT ('//text()') AS TEXT_KEY\n" + "FROM DAG_OBJECT_FACT O";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TResultColumn resultColumn = select.ResultColumnList.getResultColumn(1);
            TExpression expression = resultColumn.Expr;
            TObjectAccess objectAccess = expression.ObjectAccess;
            TExpression objectExpr = objectAccess.ObjectExpr;
            Assert.IsTrue(objectExpr.ExpressionType == EExpressionType.function_t);
            TFunctionCall functionCall1 = objectExpr.FunctionCall;
            TExpression arg1 = functionCall1.Args.getExpression(0);
            Assert.IsTrue(arg1.ExpressionType == EExpressionType.function_t);
            //        TFunctionCall functionCall2 = arg1.getFunctionCall();
            //        Assert.IsTrue(functionCall2.getArgs().getExpression(0).toString().equalsIgnoreCase("K"));
            //        Assert.IsTrue(functionCall2.getArgs().getExpression(1).getExpressionType() == EExpressionType.concatenate_t);
            TFunctionCall functionCall = objectAccess.Method;
            Assert.IsTrue(functionCall.FunctionName.ToString().Equals("EXTRACT", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(functionCall.Args.getExpression(0).ToString().Equals("'//text()'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testNewConstructor()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "  function LogPerformance(\n" + "    pCodStatoElaborazione in varchar2,\n" + "    pDataInizio in timestamp,\n" + "    pDataFine in timestamp default null) return GOL_AGGREGATORI.AGGO_LOG_Performance\n" + "  is\n" + "  begin\n" + "    return new\n" + "      GOL_AGGREGATORI.AGGO_LOG_Performance(\n" + "        pCodStatoElaborazione,\n" + "        pDataInizio,\n" + "        pDataFine);\n" + "  end LogPerformance;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TPlsqlCreateFunction function = (TPlsqlCreateFunction)sqlparser.sqlstatements.get(0);
            TReturnStmt returnStmt = (TReturnStmt)function.BodyStatements.get(0);
            Assert.IsTrue(returnStmt.Expression.ExpressionType == EExpressionType.type_constructor_t);

        }

        [TestMethod]
        public void testNaturalJoin1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT times.time_id, product, quantity FROM inventory NATURAL LEFT OUTER JOIN t1;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TJoin lcJoin = select.joins.getJoin(0);

            TJoinItem lcitem = lcJoin.JoinItems.getJoinItem(0);
            Assert.IsTrue(lcitem.JoinType == EJoinType.natural_leftouter);


        }

        [TestMethod]
        public void testNamedParameter()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "begin\t\n" + "\ttest_function_call(a => b, c => d);\n" + "end;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TCommonBlock block = (TCommonBlock)sqlparser.sqlstatements.get(0);
            TBasicStmt basicStmt = (TBasicStmt)block.BodyStatements.get(0);
            TFunctionCall f = basicStmt.Expr.FunctionCall;
            Assert.IsTrue(f.Args.getExpression(0).ToString().Equals("a => b", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testMergeInPlsql()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "PROCEDURE load_bank_file ()\n" + "IS\n" + "BEGIN\n" + "\n" + "MERGE INTO ap_bank_branches apb\n" + "USING (SELECT xbl.bank_num\n" + "FROM xxuom_bank_load xbl\n" + "WHERE\t  xbl.status = cn_status_bank_num_match) bnk_load\n" + "ON (apb.bank_num = bnk_load.bank_num)\n" + "WHEN MATCHED\n" + "THEN\n" + "UPDATE SET\n" + "apb.bank_name = SUBSTR (bnk_load.bank_name, 1, 60)\n" + "WHEN NOT MATCHED\n" + "THEN\n" + "INSERT\t  (address_line1)\n" + "VALUES (NULL);\n" + "COMMIT;\n" + "\n" + "END load_bank_file";
            Assert.IsTrue(sqlparser.parse() == 0);

            TPlsqlCreateProcedure createProcedure = (TPlsqlCreateProcedure)sqlparser.sqlstatements.get(0);
            TMergeSqlStatement merge = (TMergeSqlStatement)createProcedure.BodyStatements.get(0);
            Assert.IsTrue(merge.TargetTable.ToString().Equals("ap_bank_branches", StringComparison.CurrentCultureIgnoreCase));

            TSelectSqlStatement select = merge.UsingTable.Subquery;
            Assert.IsTrue(select.tables.getTable(0).ToString().Equals("xxuom_bank_load", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testInvokerRights()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE OR REPLACE FUNCTION RULE13014TESTFUNC1( A1 IN NUMBER )\n" + "RETURN NUMBER AUTHID CURRENT_USER\n" + "AS\n" + "BEGIN\n" + "NULL;\n" + "END;\n" + "/";
            Assert.IsTrue(sqlparser.parse() == 0);

            TPlsqlCreateFunction function = (TPlsqlCreateFunction)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(function.InvokerRightsClause.Definer.ToString().Equals("CURRENT_USER", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testInsertMultiTable1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "INSERT ALL\n" + "INTO sales (prod_id, cust_id, time_id, amount)\n" + "VALUES (product_id, customer_id, weekly_start_date, sales_sun)\n" + "INTO sales (prod_id, cust_id, time_id, amount)\n" + "VALUES (product_id, customer_id, weekly_start_date+1, sales_mon)\n" + "INTO sales (prod_id, cust_id, time_id, amount)\n" + "VALUES (product_id, customer_id, weekly_start_date+2, sales_tue)\n" + "INTO sales (prod_id, cust_id, time_id, amount)\n" + "VALUES (product_id, customer_id, weekly_start_date+3, sales_wed)\n" + "INTO sales (prod_id, cust_id, time_id, amount)\n" + "VALUES (product_id, customer_id, weekly_start_date+4, sales_thu)\n" + "INTO sales (prod_id, cust_id, time_id, amount)\n" + "VALUES (product_id, customer_id, weekly_start_date+5, sales_fri)\n" + "INTO sales (prod_id, cust_id, time_id, amount)\n" + "VALUES (product_id, customer_id, weekly_start_date+6, sales_sat)\n" + "SELECT product_id, customer_id, weekly_start_date, sales_sun,\n" + "sales_mon, sales_tue, sales_wed, sales_thu, sales_fri, sales_sat\n" + "FROM sales_input_table;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TInsertSqlStatement insert = (TInsertSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(insert.InsertIntoValues.Count == 7);
            TInsertIntoValue insertIntoValue = insert.InsertIntoValues[6];
            Assert.IsTrue(insertIntoValue.Table.ToString().Equals("sales", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(insertIntoValue.ColumnList.getObjectName(0).ToString().Equals("prod_id", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(insertIntoValue.ColumnList.getObjectName(3).ToString().Equals("amount", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(insertIntoValue.TargetList.getMultiTarget(0).ColumnList.getResultColumn(0).ToString().Equals("product_id", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(insertIntoValue.TargetList.getMultiTarget(0).ColumnList.getResultColumn(3).ToString().Equals("sales_sat", StringComparison.CurrentCultureIgnoreCase));
            // System.out.println(insertIntoValue.getTargetList().size());

            TSelectSqlStatement select = insert.SubQuery;
            Assert.IsTrue(select.ResultColumnList.getResultColumn(0).ToString().Equals("product_id", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(select.tables.getTable(0).ToString().Equals("sales_input_table", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testInsertMultiTable2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "INSERT ALL\n" + "WHEN order_total < 1000000 THEN\n" + "INTO small_orders\n" + "WHEN order_total > 1000000 AND order_total < 2000000 THEN\n" + "INTO medium_orders\n" + "WHEN order_total > 2000000 THEN\n" + "INTO large_orders\n" + "SELECT order_id, order_total, sales_rep_id, customer_id\n" + "FROM orders;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TInsertSqlStatement insert = (TInsertSqlStatement)sqlparser.sqlstatements.get(0);

            Assert.IsTrue(insert.InsertConditions.Count == 3);
            TInsertCondition condition = insert.InsertConditions[0];

            Assert.IsTrue(condition.Condition.ExpressionType == EExpressionType.simple_comparison_t);
            Assert.IsTrue(condition.Condition.ToString().Equals("order_total < 1000000", StringComparison.CurrentCultureIgnoreCase));

            TInsertIntoValue intoValue = condition.InsertIntoValues[0];
            Assert.IsTrue(intoValue.Table.ToString().Equals("small_orders", StringComparison.CurrentCultureIgnoreCase));

            TSelectSqlStatement select = insert.SubQuery;
            Assert.IsTrue(select.ResultColumnList.getResultColumn(0).ToString().Equals("order_id", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(select.tables.getTable(0).ToString().Equals("orders", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testInsertMultiTable3()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "INSERT ALL\n" + "WHEN order_total < 100000 THEN\n" + "INTO small_orders\n" + "WHEN order_total > 100000 AND order_total < 200000 THEN\n" + "INTO medium_orders\n" + "ELSE\n" + "INTO large_orders\n" + "SELECT order_id, order_total, sales_rep_id, customer_id\n" + "FROM orders;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TInsertSqlStatement insert = (TInsertSqlStatement)sqlparser.sqlstatements.get(0);

            Assert.IsTrue(insert.InsertConditions.Count == 2);
            TInsertCondition condition = insert.InsertConditions[0];

            Assert.IsTrue(condition.Condition.ExpressionType == EExpressionType.simple_comparison_t);
            Assert.IsTrue(condition.Condition.ToString().Equals("order_total < 100000", StringComparison.CurrentCultureIgnoreCase));

            TInsertIntoValue intoValue = condition.InsertIntoValues[0];
            Assert.IsTrue(intoValue.Table.ToString().Equals("small_orders", StringComparison.CurrentCultureIgnoreCase));


            TInsertIntoValue elseIntoValue = insert.InsertIntoValues[0];
            Assert.IsTrue(elseIntoValue.Table.ToString().Equals("large_orders", StringComparison.CurrentCultureIgnoreCase));

            TSelectSqlStatement select = insert.SubQuery;
            Assert.IsTrue(select.ResultColumnList.getResultColumn(0).ToString().Equals("order_id", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(select.tables.getTable(0).ToString().Equals("orders", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testInExpr1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select * from dual\n" + "where AS_OF_DATE IN Last_Day(Add_Months(('1'),-1))";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TExpression expression = select.WhereClause.Condition;
            Assert.IsTrue(expression.ExpressionType == EExpressionType.in_t);
            TExpression func_expr = expression.RightOperand;
            Assert.IsTrue(func_expr.FunctionCall.ToString().Equals("Last_Day(Add_Months(('1'),-1))", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testInExpr2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select * from dual where (dummy,dummy) in (:b3,:b2);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TExpression expression = select.WhereClause.Condition;
            Assert.IsTrue(expression.ExpressionType == EExpressionType.in_t);
            TExpression rexpr = expression.RightOperand;
            Assert.IsTrue(rexpr.ExpressionType == EExpressionType.list_t);
            TExpression e0 = rexpr.ExprList.getExpression(0);
            Assert.IsTrue(e0.ExpressionType == EExpressionType.simple_object_name_t);

            TExpression e1 = rexpr.ExprList.getExpression(1);
            Assert.IsTrue(e1.ExpressionType == EExpressionType.simple_object_name_t);
            Assert.IsTrue(e1.ObjectOperand.ToString().Equals(":b2", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testExplainPlan1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "explain plan for select * from dual;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TExplainPlan explainPlan = (TExplainPlan)sqlparser.sqlstatements.get(0);
            TSelectSqlStatement select = (TSelectSqlStatement)explainPlan.Statement;
            Assert.IsTrue(select.ToString().Equals("select * from dual", StringComparison.CurrentCultureIgnoreCase));
            TResultColumn resultColumn = select.ResultColumnList.getResultColumn(0);
            Assert.IsTrue(resultColumn.ToString().Equals("*", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(select.tables.getTable(0).ToString().Equals("dual", StringComparison.CurrentCultureIgnoreCase));
        }
        [TestMethod]
        public void testExplainPlan2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "EXPLAIN PLAN\n" + "SET STATEMENT_ID = 'Raise in Tokyo'\n" + "INTO plan_table\n" + "FOR UPDATE employees\n" + "SET salary = salary * 1.10\n" + "WHERE department_id =\n" + "(SELECT department_id FROM departments\n" + "WHERE location_id = 1200);";
            Assert.IsTrue(sqlparser.parse() == 0);
            TExplainPlan explainPlan = (TExplainPlan)sqlparser.sqlstatements.get(0);
            TUpdateSqlStatement update = (TUpdateSqlStatement)explainPlan.Statement;
            Assert.IsTrue(update.TargetTable.ToString().Equals("employees", StringComparison.CurrentCultureIgnoreCase));

            TResultColumn resultColumn = update.ResultColumnList.getResultColumn(0);
            Assert.IsTrue(resultColumn.ToString().Equals("salary = salary * 1.10", StringComparison.CurrentCultureIgnoreCase));

            TExpression expression = update.WhereClause.Condition;
            Assert.IsTrue(expression.RightOperand.ExpressionType == EExpressionType.subquery_t);

            TSelectSqlStatement select = expression.RightOperand.SubQuery;
            Assert.IsTrue(select.ResultColumnList.getResultColumn(0).ToString().Equals("department_id", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testExecuteProcedure()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE TABLE a (a VARCHAR2(10));\n" + "EXECUTE some_package.some_proc('ARG')";
            Assert.IsTrue(sqlparser.parse() == 0);

            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstcreatetable);
            Assert.IsTrue(sqlparser.sqlstatements.get(1).sqlstatementtype == ESqlStatementType.sstoracleexecuteprocedure);

            TOracleExecuteProcedure executeProcedure = (TOracleExecuteProcedure)sqlparser.sqlstatements.get(1);
            Assert.IsTrue(executeProcedure.ProcedureName.ToString().Equals("some_package.some_proc", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(executeProcedure.ProcedureParameters.getExpression(0).ToString().Equals("'ARG'", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testExecImmediate1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "EXECUTE IMMEDIATE \n" + "\t'SELECT /*+ PARALLEL 4 */ count(1) FROM SCHEMA.TABLE_SAMPLE (P'||TO_CHAR(v_processDt,'YYYYMMDD')||')'\n" + "  INTO var;";
            Assert.IsTrue(sqlparser.parse() == 0);

            //System.out.println(sqlparser.sqlstatements.get(0).sqlstatementtype);
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstplsql_execimmestmt);
            TPlsqlExecImmeStmt execImmeStmt = (TPlsqlExecImmeStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(execImmeStmt.IntoVariables.getExpression(0).ToString().Equals("var", StringComparison.CurrentCultureIgnoreCase));
            //  Assert.IsTrue(execImmeStmt.getDynamicStatements().get(0).sqlstatementtype == ESqlStatementType.sstselect);
            //TSelectSqlStatement select = (TSelectSqlStatement)execImmeStmt.getDynamicStatements().get(0);
            //System.out.println(select.toString());
        }

        [TestMethod]
        public void testExecImmediate2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            // sqlparser.sqltext = "EXECUTE IMMEDIATE q'[alter view SCHEMATEMP.vls_master_d compile  ]';--' ";
            sqlparser.sqltext = "EXECUTE IMMEDIATE 'alter view SCHEMATEMP.vls_master_d compile  ';--' ";
            Assert.IsTrue(sqlparser.parse() == 0);

            //System.out.println(sqlparser.sqlstatements.get(0).sqlstatementtype);
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstplsql_execimmestmt);
            TPlsqlExecImmeStmt execImmeStmt = (TPlsqlExecImmeStmt)sqlparser.sqlstatements.get(0);
            //System.out.println(execImmeStmt.getDynamicStatements().get(0).toString());
            //Assert.IsTrue(execImmeStmt.getIntoVariables().getExpression(0).toString().equalsIgnoreCase("var"));
            Assert.IsTrue(execImmeStmt.DynamicStatements.get(0).sqlstatementtype == ESqlStatementType.sstalterview);
            TAlterViewStatement cv = (TAlterViewStatement)execImmeStmt.DynamicStatements.get(0);
            //System.out.println(cv.getViewName().toString());
            Assert.IsTrue(cv.ViewName.ToString().Equals("SCHEMATEMP.vls_master_d", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testMerge()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "merge into table1\n" + "using (select a, b from table2)\n" + "on (table1.a = table2.a)\n" + "when matched then update set table1.b = table2.p\n" + "LOG ERRORS INTO table3 (a) REJECT LIMIT 50 ;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMergeSqlStatement mergeSqlStatement = (TMergeSqlStatement)sqlparser.sqlstatements.get(0);
            TErrorLoggingClause errorLoggingClause = mergeSqlStatement.ErrorLoggingClause;
            Assert.IsTrue(errorLoggingClause.TableName.ToString().Equals("table3", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(errorLoggingClause.SimpleExpression.LeftOperand.ToString().Equals("a", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(errorLoggingClause.RejectLimitToken.ToString().Equals("50", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testDatatype()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "create or replace function FUNC0\n" + "return simple_integer\n" + "is\n" + "M_SIMPLE_INTEGER simple_integer := 2147483645;\n" + "type TYP11 is record ( M1 simple_integer );\n" + "begin\n" + "return M_SIMPLE_INTEGER;\n" + "end;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TPlsqlCreateFunction function = (TPlsqlCreateFunction)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(function.DeclareStatements.size() == 2);
            TPlsqlRecordTypeDefStmt recordTypeDefStmt = (TPlsqlRecordTypeDefStmt)function.DeclareStatements.get(1);
            Assert.IsTrue(recordTypeDefStmt.TypeName.ToString().Equals("TYP11", StringComparison.CurrentCultureIgnoreCase));
            TParameterDeclaration pd = recordTypeDefStmt.FieldDeclarations.getParameterDeclarationItem(0);
            Assert.IsTrue(pd.ParameterName.ToString().Equals("M1", StringComparison.CurrentCultureIgnoreCase));
            //  System.out.println(pd.getDataType().getDataType());
            Assert.IsTrue(pd.DataType.DataType == EDataType.simple_integer_t);
        }

        [TestMethod]
        public  void testCreateLibrary1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE LIBRARY app_lib as '${ORACLE_HOME}/lib/app_lib.so' AGENT 'sales.hq.acme.example.com';";
            Assert.IsTrue(sqlparser.parse() == 0);

            TOracleCreateLibraryStmt libraryStmt = (TOracleCreateLibraryStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(libraryStmt.LibraryName.ToString().Equals("app_lib", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(libraryStmt.FileName.ToString().Equals("'${ORACLE_HOME}/lib/app_lib.so'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(libraryStmt.DbLink.ToString().Equals("'sales.hq.acme.example.com'", StringComparison.CurrentCultureIgnoreCase));
        }
        [TestMethod]
        public  void testGetSchemaName()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "create library USER1.\"app_lib\" as '${ORACLE_HOME}/lib/app_lib.so' agent 'sales.hq.acme.example.com';";
            Assert.IsTrue(sqlparser.parse() == 0);

            TOracleCreateLibraryStmt libraryStmt = (TOracleCreateLibraryStmt)sqlparser.sqlstatements.get(0);
            TObjectName libraryName = libraryStmt.LibraryName;
            Assert.IsTrue(libraryName.ToString().Equals("USER1.\"app_lib\"", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(libraryName.SchemaToken.ToString().Equals("USER1", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testAggregate()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE FUNCTION SecondMax (input NUMBER) RETURN NUMBER\n" + "    PARALLEL_ENABLE AGGREGATE USING SecondMaxImpl;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TPlsqlCreateFunction f = (TPlsqlCreateFunction)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(f.FunctionName.ToString().Equals("SecondMax", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(f.ImplementionType.ToString().Equals("SecondMaxImpl", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testImplementionType()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "create or replace function \"USER1\".\"FUNC1\" ( \"I1\" in \"T1\".\"C1\"%TYPE )\n" + "return INTEGER\n" + "aggregate using \"T1\";";
            Assert.IsTrue(sqlparser.parse() == 0);

            TPlsqlCreateFunction f = (TPlsqlCreateFunction)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(f.FunctionName.ToString().Equals("\"USER1\".\"FUNC1\"", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(f.ImplementionType.ToString().Equals("\"T1\"", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testCompoundTrigger()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE OR REPLACE TRIGGER aud_emp\n" + "FOR INSERT OR UPDATE\n" + "ON employees\n" + "COMPOUND TRIGGER\n" + "   \n" + "  TYPE t_emp_changes       IS TABLE OF aud_emp%ROWTYPE INDEX BY SIMPLE_INTEGER;\n" + "  v_emp_changes            t_emp_changes;\n" + "   \n" + "  v_index                  SIMPLE_INTEGER       := 0;\n" + "  v_threshhold    CONSTANT SIMPLE_INTEGER       := 1000; --maximum number of rows to write in one go.\n" + "  v_user          VARCHAR2(50); --logged in user\n" + "   \n" + "  PROCEDURE flush_logs\n" + "  IS\n" + "    v_updates       CONSTANT SIMPLE_INTEGER := v_emp_changes.count();\n" + "  BEGIN\n" + " \n" + "    FORALL v_count IN 1..v_updates\n" + "        INSERT INTO aud_emp\n" + "             VALUES v_emp_changes(v_count);\n" + " \n" + "    v_emp_changes.delete();\n" + "    v_index := 0; --resetting threshold for next bulk-insert.\n" + " \n" + "  END flush_logs;\n" + " \n" + "  AFTER EACH ROW\n" + "  IS\n" + "  BEGIN\n" + "         \n" + "    IF INSERTING THEN\n" + "        v_index := v_index + 1;\n" + "        v_emp_changes(v_index).upd_dt       := SYSDATE;\n" + "        v_emp_changes(v_index).upd_by       := SYS_CONTEXT ('USERENV', 'SESSION_USER');\n" + "        v_emp_changes(v_index).emp_id       := :NEW.emp_id;\n" + "        v_emp_changes(v_index).action       := 'Create';\n" + "        v_emp_changes(v_index).field        := '*';\n" + "        v_emp_changes(v_index).from_value   := 'NULL';\n" + "        v_emp_changes(v_index).to_value     := '*';\n" + " \n" + "    ELSIF UPDATING THEN\n" + "        IF (   (:OLD.EMP_ID <> :NEW.EMP_ID)\n" + "                OR (:OLD.EMP_ID IS     NULL AND :NEW.EMP_ID IS NOT NULL)\n" + "                OR (:OLD.EMP_ID IS NOT NULL AND :NEW.EMP_ID IS     NULL)\n" + "                  )\n" + "             THEN\n" + "                v_index := v_index + 1;\n" + "                v_emp_changes(v_index).upd_dt       := SYSDATE;\n" + "                v_emp_changes(v_index).upd_by       := SYS_CONTEXT ('USERENV', 'SESSION_USER');\n" + "                v_emp_changes(v_index).emp_id       := :NEW.emp_id;\n" + "                v_emp_changes(v_index).field        := 'EMP_ID';\n" + "                v_emp_changes(v_index).from_value   := to_char(:OLD.EMP_ID);\n" + "                v_emp_changes(v_index).to_value     := to_char(:NEW.EMP_ID);\n" + "                v_emp_changes(v_index).action       := 'Update';\n" + "          END IF;\n" + "         \n" + "        IF (   (:OLD.NAME <> :NEW.NAME)\n" + "                OR (:OLD.NAME IS     NULL AND :NEW.NAME IS NOT NULL)\n" + "                OR (:OLD.NAME IS NOT NULL AND :NEW.NAME IS     NULL)\n" + "                  )\n" + "             THEN\n" + "                v_index := v_index + 1;\n" + "                v_emp_changes(v_index).upd_dt       := SYSDATE;\n" + "                v_emp_changes(v_index).upd_by       := SYS_CONTEXT ('USERENV', 'SESSION_USER');\n" + "                v_emp_changes(v_index).emp_id       := :NEW.emp_id;\n" + "                v_emp_changes(v_index).field        := 'NAME';\n" + "                v_emp_changes(v_index).from_value   := to_char(:OLD.NAME);\n" + "                v_emp_changes(v_index).to_value     := to_char(:NEW.NAME);\n" + "                v_emp_changes(v_index).action       := 'Update';\n" + "          END IF;\n" + "                        \n" + "        IF (   (:OLD.SALARY <> :NEW.SALARY)\n" + "                OR (:OLD.SALARY IS     NULL AND :NEW.SALARY IS NOT NULL)\n" + "                OR (:OLD.SALARY IS NOT NULL AND :NEW.SALARY IS     NULL)\n" + "                  )\n" + "             THEN\n" + "                v_index := v_index + 1;\n" + "                v_emp_changes(v_index).upd_dt      := SYSDATE;\n" + "                v_emp_changes(v_index).upd_by      := SYS_CONTEXT ('USERENV', 'SESSION_USER');\n" + "                v_emp_changes(v_index).emp_id      := :NEW.emp_id;\n" + "                v_emp_changes(v_index).field       := 'SALARY';\n" + "                v_emp_changes(v_index).from_value  := to_char(:OLD.SALARY);\n" + "                v_emp_changes(v_index).to_value    := to_char(:NEW.SALARY);\n" + "                v_emp_changes(v_index).action      := 'Update';\n" + "          END IF;\n" + "                        \n" + "    END IF;\n" + " \n" + "    IF v_index >= v_threshhold THEN\n" + "      flush_logs();\n" + "    END IF;\n" + " \n" + "   END AFTER EACH ROW;\n" + " \n" + "  -- AFTER STATEMENT Section:\n" + "  AFTER STATEMENT IS\n" + "  BEGIN\n" + "     flush_logs();\n" + "  END AFTER STATEMENT;\n" + " \n" + "END aud_emp;\n" + "/";
            Assert.IsTrue(sqlparser.parse() == 0);

            TPlsqlCreateTrigger createTrigger = (TPlsqlCreateTrigger)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createTrigger.TriggerName.ToString().Equals("aud_emp", StringComparison.CurrentCultureIgnoreCase));
            TCompoundTriggerBody compoundTriggerBody = (TCompoundTriggerBody)createTrigger.TriggerBody;
            //System.out.println(compoundTriggerBody.getDeclareStatements().size());
            //System.out.println(compoundTriggerBody.getTimingPointList().size());
            Assert.IsTrue(compoundTriggerBody.DeclareStatements.Count == 6);
            Assert.IsTrue(compoundTriggerBody.TimingPointList.Count == 2);
            //Assert.IsTrue();
        }

        [TestMethod]
        public void testCommentOn()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "COMMENT ON TABLE \"SFMFG\".\"SFFND_OPER_TYPE_DEF\" IS 'Test'";
            Assert.IsTrue(sqlparser.parse() == 0);
            TOracleCommentOnSqlStmt commentOnSqlStmt = (TOracleCommentOnSqlStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(commentOnSqlStmt.ObjectName.ToString().Equals("\"SFMFG\".\"SFFND_OPER_TYPE_DEF\"", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(string.Equals(commentOnSqlStmt.ObjectName.TableString,"\"SFFND_OPER_TYPE_DEF\"",StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(string.Equals(commentOnSqlStmt.ObjectName.SchemaString,"\"SFMFG\"",StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testCallSpec1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE PROCEDURE plsToC_insertIntoEmpTab_proc (\n" + "   empno PLS_INTEGER)\n" + "AS LANGUAGE C\n" + "   NAME \"C_insertEmpTab\"\n" + "   LIBRARY insert_lib\n" + "   PARAMETERS (\n" + "      CONTEXT, \n" + "      empno);";
            // System.out.print(sqlparser.sqltext);
            Assert.IsTrue(sqlparser.parse() == 0);

            TPlsqlCreateProcedure f = (TPlsqlCreateProcedure)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(f.ProcedureName.ToString().Equals("plsToC_insertIntoEmpTab_proc", StringComparison.CurrentCultureIgnoreCase));
            TCallSpec spec = f.CallSpec;
            Assert.IsTrue(string.Equals(spec.Lang,"C",StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCallSpec2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE OR REPLACE FUNCTION func1 RETURN VARCHAR2\n" + "AS LANGUAGE C NAME \"func1\" LIBRARY lib1\n" + "WITH CONTEXT PARAMETERS(CONTEXT, x INT, y STRING, z OCIDATE);";
            // System.out.print(sqlparser.sqltext);
            Assert.IsTrue(sqlparser.parse() == 0);

            TPlsqlCreateFunction f = (TPlsqlCreateFunction)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(f.FunctionName.ToString().Equals("func1", StringComparison.CurrentCultureIgnoreCase));
            TCallSpec spec = f.CallSpec;
            Assert.IsTrue(string.Equals(spec.Lang,"C",StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(string.Equals(spec.Declaration,"\"func1\"",StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(string.Equals(spec.LibName,"lib1",StringComparison.CurrentCultureIgnoreCase));
        }
        [TestMethod]
        public void testCallSpec3()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE OR REPLACE FUNCTION func1 RETURN VARCHAR2\n" + "AS LANGUAGE C NAME \"func1\" LIBRARY lib1\n" + "WITH CONTEXT PARAMETERS(CONTEXT, x INT, y STRING, z OCIDATE);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TPlsqlCreateFunction f = (TPlsqlCreateFunction)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(f.FunctionName.ToString().Equals("func1", StringComparison.CurrentCultureIgnoreCase));
            TCallSpec spec = f.CallSpec;

            //        for (int i=spec.getStartToken().posinlist;i<spec.getEndToken().posinlist+1;i++){
            //            System.out.println(i+spec.getStartToken().container.get(i).toString());
            //        }

            Assert.IsTrue(spec.startToken.container.get(16).ToString().Equals("LANGUAGE", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(spec.startToken.container.get(50).ToString().Equals(")", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCall()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CALL dbms_java.set_output(2000)";
            Assert.IsTrue(sqlparser.parse() == 0);
            TCallStatement callStatement = (TCallStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(callStatement.RoutineName.ToString().Equals("dbms_java.set_output", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(callStatement.Args.getExpression(0).ToString().Equals("2000", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testAlterTrigger()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "ALTER TRIGGER \"ADMORA\".\"GA_ACT_EST_AUT\" ENABLE";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTriggerStmt alterTriggerStmt = (TAlterTriggerStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(alterTriggerStmt.TriggerName.ToString().Equals("\"ADMORA\".\"GA_ACT_EST_AUT\"", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(alterTriggerStmt.AlterTriggerOption == EAlterTriggerOption.enable);
        }

        [TestMethod]
        public void testRename()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "ALTER TABLE FOO RENAME TO BAR";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTableStatement = (TAlterTableStatement)sqlparser.sqlstatements.get(0);

            Assert.IsTrue(alterTableStatement.TableName.ToString().Equals("foo", StringComparison.CurrentCultureIgnoreCase));

            TAlterTableOptionList l = alterTableStatement.AlterTableOptionList;
            TAlterTableOption o = l.getAlterTableOption(0);
            Assert.IsTrue(o.OptionType == EAlterTableOptionType.RenameTable);
            Assert.IsTrue(o.NewTableName.ToString().Equals("bar", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testAddColumn()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "ALTER TABLE TS_TRS_SVS ADD TA_PRT_TRS_COR_TRS number(10)";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTableStatement = (TAlterTableStatement)sqlparser.sqlstatements.get(0);

            Assert.IsTrue(alterTableStatement.TableName.ToString().Equals("TS_TRS_SVS", StringComparison.CurrentCultureIgnoreCase));

            TAlterTableOptionList l = alterTableStatement.AlterTableOptionList;
            TAlterTableOption o = l.getAlterTableOption(0);
            Assert.IsTrue(o.OptionType == EAlterTableOptionType.AddColumn);
            TColumnDefinition columnDefinition = o.ColumnDefinitionList.getColumn(0);
            Assert.IsTrue(columnDefinition.ColumnName.ToString().Equals("TA_PRT_TRS_COR_TRS", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(columnDefinition.Datatype.DataType == EDataType.number_t);

        }


        [TestMethod]
        public void testTableOnlyKeyword()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "DELETE TABLE(SELECT h.people FROM hr_info h\n" + "   WHERE h.department_id = 280) p\n" + "   WHERE p.salary > 1700;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TDeleteSqlStatement deleteStatement = (TDeleteSqlStatement)sqlparser.sqlstatements.get(0);
            TTable table = deleteStatement.tables.getTable(0);

            Assert.IsTrue(table.TableKeyword);

        }

        [TestMethod]
        public void testSelectUnion()
        {

            String varname1 = "";
            varname1 = varname1 + "CREATE VIEW SAMPLE_JOIN AS SELECT " + "\n";
            varname1 = varname1 + "         P_DATE, " + "\n";
            varname1 = varname1 + "          SAMPLE_DT NAME , " + "\n";
            varname1 = varname1 + "          SAMPLE_ADV_AM ADVANCE_AMOUNT, " + "\n";
            varname1 = varname1 + "          'ACTIVE' AS SMPLE_STATUS " + "\n";
            varname1 = varname1 + "     FROM SAMPLE_TABLE SAMP, " + "\n";
            varname1 = varname1 + "          (SELECT 'A' DELTA_BYTE FROM DUAL " + "\n";
            varname1 = varname1 + "           UNION ALL " + "\n";
            varname1 = varname1 + "           SELECT 'C' DELTA_BYTE FROM DUAL " + "\n";
            varname1 = varname1 + "           UNION ALL " + "\n";
            varname1 = varname1 + "           SELECT 'D' DELTA_BYTE FROM DUAL " + "\n";
            varname1 = varname1 + "		   ) BYTE_TAB " + "\n";
            varname1 = varname1 + "    WHERE     P_DATE = " + "\n";
            varname1 = varname1 + "                 (SELECT MAX (P_DATE) " + "\n";
            varname1 = varname1 + "                    FROM SAMPLE_LKUP " + "\n";
            varname1 = varname1 + "                   WHERE  STATUS = 'COMPLETED') " + "\n";
            varname1 = varname1 + "          AND SAMP.DELTA_FILE_BYTE = BYTE_TAB.DELTA_BYTE";

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = varname1;
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateViewSqlStatement createview = (TCreateViewSqlStatement)sqlparser.sqlstatements.get(0);
            TSelectSqlStatement select = createview.Subquery;
            TSelectSqlStatement subquery = select.joins.getJoin(1).Table.Subquery;
            TSelectSqlStatement right = subquery.LeftStmt.RightStmt;
            //Console.WriteLine(right.ToString());
            //Console.WriteLine(right.ParentStmt.ToString());
            Assert.IsTrue(right.ToString().Equals("SELECT 'C' DELTA_BYTE FROM DUAL", StringComparison.CurrentCultureIgnoreCase));
            String varname2 = "";
            varname2 = varname2 + "(SELECT 'A' DELTA_BYTE FROM DUAL " + "\n";
            varname2 = varname2 + "           UNION ALL " + "\n";
            varname2 = varname2 + "           SELECT 'C' DELTA_BYTE FROM DUAL " + "\n";
            varname2 = varname2 + "           UNION ALL " + "\n";
            varname2 = varname2 + "           SELECT 'D' DELTA_BYTE FROM DUAL " + "\n";
            varname2 = varname2 + "		   )";

            Assert.IsTrue(right.ParentStmt.ToString().Equals(varname2, StringComparison.CurrentCultureIgnoreCase));
        }


        [TestMethod]
        public void testDatabaseObjectsSequence()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE SEQUENCE customers_seq START WITH 1000 INCREMENT BY 1 NOCACHE NOCYCLE;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCustomSqlStatement sql = sqlparser.sqlstatements.get(0);
            Assert.IsTrue(sql.DatabaseObjects[0].ToString().Equals("customers_seq", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sql.DatabaseObjects[0].DbObjectType == EDbObjectType.sequence);

        }


        [TestMethod]
        public void testDatabaseObjectsSequence2()
        {
            String varname1 = "";
            varname1 = varname1 + "INSERT INTO COPS_CORIMA_EXECUTE_QUEUE ( " + "\n";
            varname1 = varname1 + "       REC_ID " + "\n";
            varname1 = varname1 + "       ,ACTION " + "\n";
            varname1 = varname1 + "       ,TABLENAME " + "\n";
            varname1 = varname1 + "       ,ROW_ID " + "\n";
            varname1 = varname1 + "       ,RS_ID " + "\n";
            varname1 = varname1 + ") VALUES ( " + "\n";
            varname1 = varname1 + "       SEQ_COPS_CORIMA_EXECUTE_QUEUE.nextval " + "\n";
            varname1 = varname1 + "       ,LOWER2(c_ACTION) " + "\n";
            varname1 = varname1 + "       ,c_TABLENAME " + "\n";
            varname1 = varname1 + "       ,c_ROW_ID " + "\n";
            varname1 = varname1 + "       ,c_RS_ID " + "\n";
            varname1 = varname1 + ")";

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = varname1;
            Assert.IsTrue(sqlparser.parse() == 0);

            TCustomSqlStatement sql = sqlparser.sqlstatements.get(0);

            Assert.IsTrue(sql.DatabaseObjects[0].ToString().Equals("SEQ_COPS_CORIMA_EXECUTE_QUEUE.nextval", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sql.DatabaseObjects[0].DbObjectType == EDbObjectType.sequence);
            Assert.IsTrue(sql.DatabaseObjects[1].ToString().Equals("LOWER2", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sql.DatabaseObjects[1].DbObjectType == EDbObjectType.function);
        }

        [TestMethod]
        public void testCreateTableDefaultOnNull()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"CREATE TABLE null_test (column_defaulted VARCHAR2(10) DEFAULT ON NULL 'Default')";

            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableStmt = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition column = createTableStmt.ColumnList.getColumn(0);
            Assert.IsTrue(column.ColumnName.ToString().Equals("column_defaulted", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(column.DefaultExpression.ToString().Equals("'Default'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(column.onNull);
        }

        [TestMethod]
        public void testLabelAfterLoop()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"begin
                                   <<basic_loop0>>
                                   LOOP 
                                      EXIT basic_loop1;
                                   END LOOP basic_loop2;
                                end;";

            Assert.IsTrue(sqlparser.parse() == 0);
            
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sst_block_with_label);
            TBlockSqlStatement blockSqlStatement = (TBlockSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(blockSqlStatement.BodyStatements.size() == 1);
            TLoopStmt loopStmt = (TLoopStmt)blockSqlStatement.BodyStatements.get(0);
            //Console.WriteLine(loopStmt.LabelName.ToString());
            Assert.IsTrue(loopStmt.LabelName.ToString().Equals("basic_loop0", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(loopStmt.EndLabelName.ToString().Equals("basic_loop2", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testLabelAfterWhile()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"begin
                                  <<while_loop0>>
                                   WHILE (i <= co_max_value) 
                                   LOOP 
                                      i := i + co_increment; 
                                   END LOOP while_loop1;
                                end;";

            Assert.IsTrue(sqlparser.parse() == 0);

            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sst_block_with_label);
            TBlockSqlStatement blockSqlStatement = (TBlockSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(blockSqlStatement.BodyStatements.size() == 1);
            TLoopStmt loopStmt = (TLoopStmt)blockSqlStatement.BodyStatements.get(0);
            //Console.WriteLine(loopStmt.LabelName.ToString());
            Assert.IsTrue(loopStmt.LabelName.ToString().Equals("while_loop0", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(loopStmt.EndLabelName.ToString().Equals("while_loop1", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreatePackageDatatype()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"CREATE OR REPLACE PACKAGE types_up IS
                                   SUBTYPE description_type2 IS VARCHAR2(254 CHAR);
                                END types_up;";

            Assert.IsTrue(sqlparser.parse() == 0);

            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstplsql_createpackage);
            TPlsqlCreatePackage createPkg = (TPlsqlCreatePackage)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createPkg.DeclareStatements.size() == 1);
            TVarDeclStmt varDeclStmt = (TVarDeclStmt)createPkg.DeclareStatements.get(0);
            Assert.IsTrue(varDeclStmt.WhatDeclared == TVarDeclStmt.whatDeclared_subtype);
            TTypeName datatype = varDeclStmt.DataType;
            Assert.IsTrue(datatype.DataType == EDataType.varchar2_t);
            Assert.IsTrue(datatype.Length.ToString().Equals("254", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(datatype.charUnitToken.ToString().Equals("CHAR", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testLabelNameOfBlock()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"<<process_data>>
                                   BEGIN
                                      NULL;
                                   END process_data;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TCommonBlock block = (TCommonBlock)sqlparser.sqlstatements.get(0);
           // Console.WriteLine(block.LabelName.ToString());
            Assert.IsTrue(block.LabelName.ToString().Equals("process_data", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(block.EndLabelName.ToString().Equals("process_data", StringComparison.CurrentCultureIgnoreCase));
        }

    }
}
