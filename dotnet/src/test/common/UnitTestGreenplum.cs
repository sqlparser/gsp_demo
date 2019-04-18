using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.IO;
using gudusoft.gsqlparser.stmt;
using gudusoft.gsqlparser.nodes;
using gudusoft.gsqlparser.stmt.postgresql;

namespace gudusoft.gsqlparser.test
{
    /// <summary>
    /// UnitTestOracle 的摘要说明
    /// </summary>
    [TestClass]
    public class UnitTestGreenplum
    {
        TGSqlParser parser;

        public UnitTestGreenplum()
        {
            //
            //TODO:  在此处添加构造函数逻辑
            //
            parser = new TGSqlParser(EDbVendor.dbvgreenplum);
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
        public void TestGreenplumFiles()
        {
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"greenplum\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public  void testRenameColumn()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvgreenplum);
            sqlparser.sqltext = "ALTER TABLE distributors RENAME COLUMN address TO city;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TAlterTableStatement alterTable = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(alterTable.TableName.ToString().Equals("distributors", StringComparison.CurrentCultureIgnoreCase));

            TAlterTableOption ato = alterTable.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(ato.OptionType == EAlterTableOptionType.RenameColumn);
            Assert.IsTrue(ato.ColumnName.ToString().Equals("address", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(ato.NewColumnName.ToString().Equals("city", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public  void testSetSchema()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvgreenplum);
            sqlparser.sqltext = "ALTER TABLE myschema.distributors SET SCHEMA yourschema;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TAlterTableStatement alterTable = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(alterTable.TableName.ToString().Equals("myschema.distributors", StringComparison.CurrentCultureIgnoreCase));

            TAlterTableOption ato = alterTable.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(ato.OptionType == EAlterTableOptionType.setSchema);
            Assert.IsTrue(ato.SchemaName.ToString().Equals("yourschema", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public  void testCreateFunction()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvgreenplum);
            sqlparser.sqltext = "CREATE FUNCTION complex_add(complex, complex)\n" + "RETURNS complex\n" + "AS 'filename', 'complex_add'\n" + "LANGUAGE C IMMUTABLE STRICT;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TPostgresqlCreateFunction createFunction = (TPostgresqlCreateFunction)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createFunction.FunctionName.ToString().Equals("complex_add", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createFunction.ParameterDeclarations.size() == 2);
            TParameterDeclaration parameterDeclaration = (TParameterDeclaration)createFunction.ParameterDeclarations.getParameterDeclarationItem(0);
            Assert.IsTrue(parameterDeclaration.DataType.DataType == EDataType.generic_t);
            Assert.IsTrue(createFunction.ReturnDataType.DataType == EDataType.generic_t);
            Assert.IsTrue(createFunction.ProcedureLanguage.ToString().Equals("C", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createFunction.Objfile.ToString().Equals("'filename'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createFunction.LinkSymbol.ToString().Equals("'complex_add'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public  void testCreateTable()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvgreenplum);
            sqlparser.sqltext = "CREATE TEMP TABLE films_recent WITH (OIDS) ON COMMIT DROP AS \n" + "EXECUTE recentfilms('2007-01-01');";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableSqlStatement = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TExecutePreparedStatement executePreparedStatement = createTableSqlStatement.ExecutePreparedStatement;
            Assert.IsTrue(executePreparedStatement.StatementName.ToString().Equals("recentfilms", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(executePreparedStatement.Parameters.getExpression(0).ToString().Equals("'2007-01-01'", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public  void testValues1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvgreenplum);
            sqlparser.sqltext = "VALUES (1, 'one'), (2, 'two'), (3, 'three');";
            Assert.IsTrue(sqlparser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TValueClause valueClause = select.ValueClause;
            Assert.IsTrue(valueClause.ValueRows.size() == 3);
            TValueRowItem rowItem = valueClause.ValueRows.getValueRowItem(0);
            Assert.IsTrue(rowItem.ExprList.getExpression(0).ToString().Equals("1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(rowItem.ExprList.getExpression(1).ToString().Equals("'one'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testValues2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvgreenplum);
            sqlparser.sqltext = "SELECT f.* FROM films f, (VALUES('MGM', 'Horror'), ('UA', \n" + "'Sci-Fi')) AS t (studio, kind) WHERE f.studio = t.studio AND \n" + "f.kind = t.kind;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.joins.size() == 2);
            TTable table2 = select.joins.getJoin(1).Table;
            Assert.IsTrue(table2.TableType == ETableSource.subquery);
            Assert.IsTrue(table2.AliasClause.AliasName.ToString().Equals("t", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(table2.AliasClause.Columns.getObjectName(0).ToString().Equals("studio", StringComparison.CurrentCultureIgnoreCase));
            select = table2.Subquery;

            TValueClause valueClause = select.ValueClause;
            Assert.IsTrue(valueClause.ValueRows.size() == 2);
            TValueRowItem rowItem = valueClause.ValueRows.getValueRowItem(0);
            Assert.IsTrue(rowItem.ExprList.getExpression(0).ToString().Equals("'MGM'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(rowItem.ExprList.getExpression(1).ToString().Equals("'Horror'", StringComparison.CurrentCultureIgnoreCase));
            rowItem = valueClause.ValueRows.getValueRowItem(1);
            Assert.IsTrue(rowItem.ExprList.getExpression(0).ToString().Equals("'UA'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(rowItem.ExprList.getExpression(1).ToString().Equals("'Sci-Fi'", StringComparison.CurrentCultureIgnoreCase));
        }
    }

}

