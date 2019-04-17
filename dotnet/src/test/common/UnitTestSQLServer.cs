using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.IO;
using gudusoft.gsqlparser.stmt.mssql;
using gudusoft.gsqlparser.stmt;
using gudusoft.gsqlparser.nodes;
using System.Collections.Generic;
using gudusoft.gsqlparser.nodes.mssql;
using System.Text;

namespace gudusoft.gsqlparser.test
{
    [TestClass]
    public class UnitTestSQLServer
    {
        [TestMethod]
        public void TestQuery()
        {
            TGSqlParser parser = new TGSqlParser(EDbVendor.dbvmssql);
            parser.sqltext = "select f1,f2 from t where f3>1 and f4=1;select 1 from t;delete from t where f>0";
            int ret = parser.parse();
            Assert.IsTrue(ret == 0,parser.Errormessage);
        }

        [TestMethod]
        public void TestSQLServerFiles()
        {
            TGSqlParser parser = new TGSqlParser(EDbVendor.dbvmssql);
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"MSSQL\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public void TestSQLServerFiles2()
        {
            TGSqlParser parser = new TGSqlParser(EDbVendor.dbvmssql);
            String[] allfiles = System.IO.Directory.GetFiles(UnitTestCommon.BASE_SQL_DIR() + @"new_dotnet\MSSQL\", "*.sql", System.IO.SearchOption.AllDirectories);
            int cnt = 0;
            List<string> excludeFiles = new List<string> {
                "676_try_parse.sql","value_in_qualified_name.sql","719.sql"
            };
            foreach (var file in allfiles)
            {
                FileInfo info = new FileInfo(file);
                if (UnitTestCommon.excludeFile(info.Name, excludeFiles))
                {
                    continue;
                }

                UnitTestCommon.checkFile(parser, info.FullName);
                cnt++;
            }
        }

        [TestMethod]
        public void testAlterTable1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "IF NOT EXISTS (SELECT * FROM [dbo].[TableA] WHERE id = OBJECT_ID(N'[dbo].[TableB]') AND name = N'Foo')\n" + "BEGIN\n" + "                ALTER TABLE [dbo].[TableB]\n" + "                                ADD [Foo] [int] NULL,\n" + "                                CONSTRAINT [ForeignKeyA] FOREIGN KEY([Foo])\n" + "                                                REFERENCES [dbo].[TableC] ([AutoID])\n" + "                                                ON UPDATE CASCADE\n" + "                                                ON DELETE CASCADE\n" + "END";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMssqlIfElse ifElse = (TMssqlIfElse)sqlparser.sqlstatements.get(0);

            TMssqlBlock block = (TMssqlBlock)ifElse.Stmt;
            Assert.IsTrue(block.BodyStatements.get(0).sqlstatementtype == ESqlStatementType.sstaltertable);
            TAlterTableStatement alterTableStatement = (TAlterTableStatement)block.BodyStatements.get(0);
            Assert.IsTrue(alterTableStatement.AlterTableOptionList.size() == 1);
            TAlterTableOption ao = alterTableStatement.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(ao.OptionType == EAlterTableOptionType.AddColumn);
            Assert.IsTrue(ao.ColumnDefinitionList.size() == 1);
            TColumnDefinition cd = ao.ColumnDefinitionList.getColumn(0);
            Assert.IsTrue(cd.ColumnName.ToString().Equals("[Foo]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(cd.Constraints.size() == 1);
            TConstraint constraint = cd.Constraints.getConstraint(0);
            Assert.IsTrue(constraint.Constraint_type == EConstraintType.foreign_key);
            Assert.IsTrue(constraint.ConstraintName.ToString().Equals("[ForeignKeyA]", StringComparison.CurrentCultureIgnoreCase));

            //        Assert.IsTrue(alterTableStatement.getTableElementList().size() == 2);
            //        TTableElement element0 =      alterTableStatement.getTableElementList().getTableElement(0);
            //        TTableElement element1 =      alterTableStatement.getTableElementList().getTableElement(1);
            //        Assert.IsTrue(element0.getType() == TTableElement.type_column_def);
            //        TColumnDefinition columnDefinition = element0.getColumnDefinition();
            //        Assert.IsTrue(columnDefinition.getColumnName().toString().equalsIgnoreCase("[Foo]"));
            //        Assert.IsTrue(element1.getType() == TTableElement.type_table_constraint);
            //        TConstraint constraint = element1.getConstraint();
            //        Assert.IsTrue(constraint.getConstraintName().toString().equalsIgnoreCase("[ForeignKeyA]"));

        }

        [TestMethod]
        public void testAlterTable2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "ALTER TABLE [dbo].[TableA] ADD\n" + "                CONSTRAINT [ConstraintA] FOREIGN KEY\n" + "                (\n" + "                                [ColumnA]\n" + "                ) REFERENCES [dbo].[TableB] (\n" + "                                [ColumnA]\n" + "                ) NOT FOR REPLICATION ,\n" + "                CONSTRAINT [ConstraintB] FOREIGN KEY\n" + "                (\n" + "                                [ColumnB]\n" + "                ) REFERENCES [dbo].[TableC] (\n" + "                                [ColumnA]\n" + "                ) ON DELETE CASCADE  ON UPDATE CASCADE";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTableStatement = (TAlterTableStatement)sqlparser.sqlstatements.get(0);

            Assert.IsTrue(alterTableStatement.AlterTableOptionList.size() == 1);
            TAlterTableOption ao = alterTableStatement.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(ao.OptionType == EAlterTableOptionType.AddConstraint);
            Assert.IsTrue(ao.ConstraintList.size() == 2);
            TConstraint constraint1 = ao.ConstraintList.getConstraint(0);
            TConstraint constraint2 = ao.ConstraintList.getConstraint(1);
            Assert.IsTrue(constraint1.ConstraintName.ToString().Equals("[ConstraintA]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(constraint2.ConstraintName.ToString().Equals("[ConstraintB]", StringComparison.CurrentCultureIgnoreCase));

            //        TColumnDefinition cd = ao.getColumnDefinitionList().getColumn(0);
            //        Assert.IsTrue(cd.getColumnName().toString().equalsIgnoreCase("[Foo]"));
            //        Assert.IsTrue(cd.getConstraints().size() == 1);
            //        TConstraint constraint = cd.getConstraints().getConstraint(0);
            //        Assert.IsTrue(constraint.getConstraint_type() == EConstraintType.foreign_key);
            //        Assert.IsTrue(constraint.getConstraintName().toString().equalsIgnoreCase("[ForeignKeyA]"));

            //        Assert.IsTrue(alterTableStatement.getTableElementList().size() == 2);
            //        TTableElement element0 =      alterTableStatement.getTableElementList().getTableElement(0);
            //        TTableElement element1 =      alterTableStatement.getTableElementList().getTableElement(1);
            //        Assert.IsTrue(element0.getType() == TTableElement.type_table_constraint);
            //        Assert.IsTrue(element1.getType() == TTableElement.type_table_constraint);
            //        TConstraint constraint = element1.getConstraint();
            //        Assert.IsTrue(constraint.getConstraintName().toString().equalsIgnoreCase("[ConstraintB]"));

        }

        [TestMethod]
        public void testAlterTable3()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "ALTER TABLE FolderComment ADD CommentRSN INT NOT NULL DEFAULT NEXT VALUE FOR FolderCommentSeq";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTableStatement = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(alterTableStatement.AlterTableOptionList.size() == 1);
            TAlterTableOption ao = alterTableStatement.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(ao.OptionType == EAlterTableOptionType.AddColumn);

            //        Assert.IsTrue(alterTableStatement.getTableElementList().size() == 1);
            //        TTableElement element0 =      alterTableStatement.getTableElementList().getTableElement(0);
            //        Assert.IsTrue(element0.getType() == TTableElement.type_column_def);

            TColumnDefinition columnDefinition = ao.ColumnDefinitionList.getColumn(0);
            Assert.IsTrue(columnDefinition.ColumnName.ToString().Equals("CommentRSN", StringComparison.CurrentCultureIgnoreCase));
            TConstraint columnConstraint = columnDefinition.Constraints.getConstraint(0);
            Assert.IsTrue(columnConstraint.Constraint_type == EConstraintType.notnull);
            columnConstraint = columnDefinition.Constraints.getConstraint(1);
            Assert.IsTrue(columnConstraint.Constraint_type == EConstraintType.default_value);
            TExpression defaultValue = columnConstraint.DefaultExpression;
            Assert.IsTrue(defaultValue.ExpressionType == EExpressionType.next_value_for_t);
            Assert.IsTrue(defaultValue.SequenceName.ToString().Equals("FolderCommentSeq", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testAssignment()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "SELECT\n" + "   @var1 = varA,\n" + "   @var2= varB,\n" + "   @var3= varC\n" + "FROM @TestTable \n" + "WHERE Id = @id;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            //Assert.IsTrue(select.getSelectDistinct().getDistinctType() == TBaseType.dtDistinct);
            TExpression expr = select.ResultColumnList.getResultColumn(0).Expr;
            Assert.IsTrue(expr.ExpressionType == EExpressionType.assignment_t);
        }

        [TestMethod]
        public void testAlterDatabase()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 110";
            int result = sqlparser.parse();
            Assert.IsTrue(result == 0);
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstalterdatabase);
            TAlterDatabaseStmt alterDatabaseStmt = (TAlterDatabaseStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(alterDatabaseStmt.DatabaseName.ToString().Equals("CURRENT", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sqlparser.sqlstatements.get(0).ToString().Equals("ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 110", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCallTarget1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "\tSELECT DISTINCT p.COl1,\n" + "STUFF((SELECT distinct ',' + p1.[Col2]\n" + "        FROM sampleSchema.table1 p1\n" + "        WHERE p.COl1 = p1.COl1\n" + "        FOR XML PATH(''), TYPE\n" + "        ).value('.', 'NVARCHAR(MAX)'),1,1,'') Col2\n" + "FROM sampleSchema.table2 p WHERE SourceSchema +'.'+ SourceObject = @table";
            Assert.IsTrue(sqlparser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TExpression expr = select.ResultColumnList.getResultColumn(1).Expr;
            Assert.IsTrue(expr.ExpressionType == EExpressionType.function_t);
            TFunctionCall f1 = expr.FunctionCall;
            Assert.IsTrue(f1.FunctionName.ToString().Equals("STUFF", StringComparison.CurrentCultureIgnoreCase));
            TExpression p1 = f1.Args.getExpression(0);
            Assert.IsTrue(p1.ExpressionType == EExpressionType.function_t);
            TFunctionCall f2 = p1.FunctionCall;
            Assert.IsTrue(f2.FunctionName.ToString().Equals("value", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(f2.Args.getExpression(0).ToString().Equals("'.'", StringComparison.CurrentCultureIgnoreCase));
            TExpressionCallTarget ct = f2.CallTarget;
            TExpression expr2 = ct.Expr;
            Assert.IsTrue(expr2.ExpressionType == EExpressionType.subquery_t);
            TSelectSqlStatement select1 = expr2.SubQuery;
            Assert.IsTrue(select1.ResultColumnList.getResultColumn(0).Expr.ToString().Equals("',' + p1.[Col2]", StringComparison.CurrentCultureIgnoreCase));
        }
        [TestMethod]
        public void testCallTarget2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "SELECT CatalogDescription.value('             \n" + "    declare namespace PD=\"http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/ProductModelDescription\";             \n" + "       (/PD:ProductDescription/@ProductModelID)[1]', 'int') AS Result             \n" + "FROM Production.ProductModel             \n" + "WHERE CatalogDescription IS NOT NULL             \n" + "ORDER BY Result desc";
            Assert.IsTrue(sqlparser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TExpression expr = select.ResultColumnList.getResultColumn(0).Expr;
            Assert.IsTrue(expr.ExpressionType == EExpressionType.function_t);
            TFunctionCall f2 = expr.FunctionCall;
            Assert.IsTrue(f2.FunctionName.ToString().Equals("value", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(f2.Args.getExpression(1).ToString().Equals("'int'", StringComparison.CurrentCultureIgnoreCase));
            TExpressionCallTarget ct = f2.CallTarget;
            TExpression expr2 = ct.Expr;

            Assert.IsTrue(expr2.ExpressionType == EExpressionType.simple_object_name_t);
            Assert.IsTrue(expr2.ObjectOperand.ToString().Equals("CatalogDescription", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateIndex1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "CREATE NONCLUSTERED INDEX IX_TransactionHistory_ReferenceOrderID\n" + "ON Production.TransactionHistory (ReferenceOrderID)\n" + "ON TransactionsPS1 (TransactionDate);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateIndexSqlStatement createIndexSqlStatement = (TCreateIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createIndexSqlStatement.NonClustered);
            Assert.IsTrue(createIndexSqlStatement.IndexName.ToString().Equals("IX_TransactionHistory_ReferenceOrderID", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndexSqlStatement.TableName.ToString().Equals("Production.TransactionHistory", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndexSqlStatement.ColumnNameList.getOrderByItem(0).ToString().Equals("ReferenceOrderID", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndexSqlStatement.FilegroupOrPartitionSchemeName.ToString().Equals("TransactionsPS1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndexSqlStatement.PartitionSchemeColumns.getObjectName(0).ToString().Equals("TransactionDate", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testCreateIndexIncludeClause()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "CREATE INDEX [Idx6] ON [dbo].[Master2] ([MasterId], [Name]) INCLUDE ([MasterId2], [Name2]);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateIndexSqlStatement createIndex = (TCreateIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createIndex.IndexName.ToString().Equals("[Idx6]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndex.includeColumns.size() == 2);
            Assert.IsTrue(createIndex.includeColumns.getObjectName(0).ToString().Equals("[MasterId2]", StringComparison.CurrentCultureIgnoreCase));

        }


        [TestMethod]
        public void testCreateProcedureOption1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "CREATE PROCEDURE dbo.usp_myproc \n" + "  WITH EXECUTE AS CALLER\n" + "AS \n" + "    SELECT SUSER_NAME(), USER_NAME();\n" + "    EXECUTE AS USER = 'guest';\n" + "    SELECT SUSER_NAME(), USER_NAME();\n" + "    REVERT;\n" + "    SELECT SUSER_NAME(), USER_NAME();\n" + "    DBCC CHECKIDENT (\"HumanResources.Employee\", RESEED, 30);";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMssqlCreateProcedure createProcedure = (TMssqlCreateProcedure)sqlparser.sqlstatements.get(0);
            TProcedureOption option = createProcedure.ProcedureOptions[0];
            Assert.IsTrue(option.OptionType == EProcedureOptionType.potExecuteAs);
            TExecuteAsClause asClause = option.ExecuteAsClause;
            Assert.IsTrue(asClause.ExecuteAsOption == EExecuteAsOption.eaoCaller);

            //System.out.println(createProcedure.getBodyStatements().size());
            Assert.IsTrue(createProcedure.BodyStatements.get(1).sqlstatementtype == ESqlStatementType.sstmssqlexecuteas);
            TMssqlExecuteAs executeAs = (TMssqlExecuteAs)createProcedure.BodyStatements.get(1);
            Assert.IsTrue(executeAs.ExecuteAsOption == EExecuteAsOption.eaoUser);
            Assert.IsTrue(string.Equals(executeAs.loginName.ToString(),"'guest'",StringComparison.CurrentCultureIgnoreCase ));
        }

        [TestMethod]
        public void testCreateTrigger1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "CREATE TRIGGER reminder\n" + "ON titles\n" + "FOR INSERT, UPDATE \n" + "AS RAISERROR (50009, 16, 10)\n" + "GO";
            int result = sqlparser.parse();
            Assert.IsTrue(result == 0);
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstmssqlcreatetrigger);
            TMssqlCreateTrigger createTrigger = (TMssqlCreateTrigger)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createTrigger.DmlTypes == (ETriggerDmlType.tdtInsert | ETriggerDmlType.tdtUpdate));
            Assert.IsTrue(createTrigger.TimingPoint == ETriggerTimingPoint.ttpFor);
            //Console.WriteLine(createTrigger.DmlTypes.ToString());
            // Assert.IsTrue(createTrigger.DmlTypes.ToString().Equals("tdtInsert,tdtUpdate", StringComparison.CurrentCultureIgnoreCase));
            //Assert.IsTrue(createTrigger.TimingPoint.ToString().Equals("ttpFor", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testDatatype1()
        {
            //System.out.println(TBaseType.versionid);
            //System.out.println(TBaseType.releaseDate);
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            string sp_sql = "CREATE PROCEDURE [dbo].[test]" + "        @OD1 AS datetime = '2000-01-01'," + "        @OD2 AS datetime = '2012-12-31'" + "      AS" + "      BEGIN" + "         select * from [sales_fact]" + "         where" + "         [sales_fact].[OrderDate] between @OD1 and @OD2" + "      END";
            sqlparser.sqltext = sp_sql;

            int ret = sqlparser.parse();
            if (ret == 0)
            {
                TCustomSqlStatement sql = sqlparser.sqlstatements.get(0);
                //System.out.println("SQL Statement: " + sql.sqlstatementtype);
                //System.out.println("Parameters:");

                TStoredProcedureSqlStatement procedure = (TStoredProcedureSqlStatement)sql;
                TParameterDeclaration param = null;
                param = procedure.ParameterDeclarations.getParameterDeclarationItem(0);
                Assert.IsTrue(param.ParameterName.ToString().Equals("@OD1", StringComparison.CurrentCultureIgnoreCase));
                Assert.IsTrue(param.DataType.ToString().Equals("datetime", StringComparison.CurrentCultureIgnoreCase));
                Assert.IsTrue(param.DataType.DataType == EDataType.datetime_t);
                Assert.IsTrue(param.Mode == 0);
            }
            else
            {
                Console.WriteLine(sqlparser.Errormessage);
            }
        }

        [TestMethod]
        public void testDeclareBlock1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "DECLARE \n" + "\t@n_Count integer\n" + "BEGIN\t\n" + "\tSELECT @n_Count = count(JobTypeCode) FROM Validbatchjobtype WHERE JobTypeCode = 7\n" + "\tIF @n_Count = 1 \n" + "\t\tBEGIN\n" + "\t\t\tUPDATE  ValidBatchJobType SET JobTypeDesc = 'Merge Document Report',  JobTypeClass = 'WordMergeJob' WHERE JobTypeCode = 7\n" + "\t\tEND\n" + "\tELSE\n" + "\t\tBEGIN\n" + "\t\t\tINSERT INTO ValidBatchJobType(JobTypeCode, JobTypeDesc, JobTypeClass, StampUser, StampDate) \n" + "\t             VALUES(7, 'Merge Document Report', 'WordMergeJob', SYSTEM_USER, GETDATE())\n" + "\t\tEND\n" + "END";
            Assert.IsTrue(sqlparser.parse() == 0);

            Assert.IsTrue(sqlparser.sqlstatements.size() == 2);

            TMssqlDeclare declare = (TMssqlDeclare)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(declare.Variables.getDeclareVariable(0).Datatype.ToString().Equals("integer", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(declare.Variables.getDeclareVariable(0).VariableName.ToString().Equals("@n_Count", StringComparison.CurrentCultureIgnoreCase));

            TMssqlBlock block = (TMssqlBlock)sqlparser.sqlstatements.get(1);
            Assert.IsTrue(block.BodyStatements.size() == 2);
        }

        [TestMethod]
        public void testDeclareCursor()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "   declare sds_post_asset_identifier cursor for\n" + "   select distinct\n" + "          asset_id          \n" + "        , asset_name_1     \n" + "  \tfrom asd_wk_asset_identifier\n" + "     where last_mod_tmstmp < @source_start_time\n" + "\torder by asset_id, last_mod_tmstmp";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMssqlDeclare declare = (TMssqlDeclare)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(declare.CursorName.ToString().Equals("sds_post_asset_identifier", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public virtual void testDistinct1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "SELECT DISTINCT tf1, tf2 from tbl";
            Assert.IsTrue(sqlparser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(select.SelectDistinct.DistinctType == TBaseType.dtDistinct);
        }
        [TestMethod]
        public void testDropDBObject1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "DROP PROCEDURE ProcedureA";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMssqlDropDbObject dropDbObject = (TMssqlDropDbObject)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(dropDbObject.DbObjectType.ToString().Equals("procedure", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(dropDbObject.ObjectNameList.getObjectName(0).ToString().Equals("ProcedureA", StringComparison.CurrentCultureIgnoreCase));
        }
        [TestMethod]
        public void testDropDBObject2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "DROP FUNCTION FunctionA";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMssqlDropDbObject dropDbObject = (TMssqlDropDbObject)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(dropDbObject.DbObjectType.ToString().Equals("function", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(dropDbObject.ObjectNameList.getObjectName(0).ToString().Equals("FunctionA", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public virtual void testOutputClause1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "MERGE INTO [data].[TABLEDEST] dest\n" + "USING\n" + "   (\n" + "   SELECT src.[Column1],src.[Column2],src.[Column3]\n" + "   FROM [sys].[TABLESRC] src\n" + "   ) src1\n" + "   ON dest.[Column1] = src1.[Column1]\n" + "WHEN NOT MATCHED BY TARGET THEN\n" + "   INSERT ([Column1],[Column2],[Column3])\n" + "      VALUES([Column1],[Column2],[Column3])\n" + "WHEN MATCHED THEN\n" + "   UPDATE SET dest.[Column1] = dest.[Column1]\n" + "OUTPUT src1.[Column1],src1.[Column2],src1.[Column3]\n" + "   INTO @TABLEOUTPUT\n" + "   (\n" + "      [Column1]\n" + "      ,[Column2]\n" + "      ,[Column3]\n" + "   );";
            Assert.IsTrue(sqlparser.parse() == 0);

            TMergeSqlStatement mergeSqlStatement = (TMergeSqlStatement)sqlparser.sqlstatements.get(0);
            TOutputClause outputClause = mergeSqlStatement.OutputClause;
            Assert.IsTrue(outputClause.SelectItemList.size() == 3);
            Assert.IsTrue(outputClause.SelectItemList.getResultColumn(0).ToString().Equals("src1.[Column1]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(outputClause.IntoTable.ToString().Equals("@TABLEOUTPUT", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(outputClause.IntoColumnList.size() == 3);
            Assert.IsTrue(outputClause.IntoColumnList.getObjectName(2).ToString().Equals("[Column3]", StringComparison.CurrentCultureIgnoreCase));

        }
        [TestMethod]
        public void testSqlServer1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "SELECT 'AverageCost' AS Cost_Sorted_By_Production_Days, \n" + "[0], [1], [2], [3], [4]\n" + "FROM\n" + "(SELECT DaysToManufacture, StandardCost \n" + "    FROM Production.Product) AS SourceTable\n" + "PIVOT\n" + "(\n" + "AVG(StandardCost)\n" + "FOR DaysToManufacture IN ([0], [1], [2], [3], [4])\n" + ") AS PivotTable;";
            Assert.IsTrue(sqlparser.parse() == 0);

            //  System.out.print(sqlparser.sqltext);

            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);

            TTable table = select.tables.getTable(0);
            //Assert.IsTrue(table.getTableType() == ETableSource.pivoted_table);
            Assert.IsTrue(table.TableType == ETableSource.subquery);
            TSelectSqlStatement subquery = table.Subquery;
            Assert.IsTrue(subquery.tables.getTable(0).ToString().Equals("Production.Product", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(table.AliasClause.AliasName.ToString().Equals("SourceTable", StringComparison.CurrentCultureIgnoreCase));

            Assert.IsTrue(select.TargetTable.TableType == ETableSource.pivoted_table);
            TPivotedTable pivotedTable = select.TargetTable.PivotedTable;

            TPivotClause pivotClause = pivotedTable.PivotClause;
            Assert.IsTrue(pivotClause.Type == TPivotClause.pivot);

            Assert.IsTrue(pivotClause.Aggregation_function.ToString().Equals("AVG(StandardCost)", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(pivotClause.PivotColumnList.getObjectName(0).ToString().Equals("DaysToManufacture", StringComparison.CurrentCultureIgnoreCase));
            //Assert.IsTrue(pivotClause.getPivotColumnList().size() == 5);
            Assert.IsTrue(pivotClause.PivotInClause.Items.size() == 5);
            //Assert.IsTrue(pivotClause.getPivotColumnList().getObjectName(0).toString().equalsIgnoreCase("[0]"));
            Assert.IsTrue(pivotClause.PivotInClause.Items.getResultColumn(0).ToString().Equals("[0]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(pivotClause.AliasClause.ToString().Equals("PivotTable", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testSqlServer2()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "SELECT ShipName, ShipAddress, [1],[2],[3], France, Germany, Brazil\n" + "from [northwind].[dbo].[Orders]\n" + "pivot ( sum(freight) for shipvia in ([1],[2],[3])) sum_freight\n" + "pivot ( count(shipcountry) for Shipcountry in ([France], [Germany], [Brazil])) cntry";
            Assert.IsTrue(sqlparser.parse() == 0);
            //System.out.print(sqlparser.sqltext);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);

            TTable table = select.tables.getTable(0);
            //Assert.IsTrue(table.getTableType() == ETableSource.pivoted_table);
            Assert.IsTrue(table.TableType == ETableSource.objectname);
            Assert.IsTrue(table.ToString().Equals("[northwind].[dbo].[Orders]", StringComparison.CurrentCultureIgnoreCase));

            TPivotedTable pivotedTable = select.TargetTable.PivotedTable;
            Assert.IsTrue(pivotedTable.PivotClauseList.Count == 2);

            TPivotClause pivotClause = pivotedTable.PivotClauseList[0];
            Assert.IsTrue(pivotClause.Type == TPivotClause.pivot);
            Assert.IsTrue(pivotClause.Aggregation_function.ToString().Equals("sum(freight)", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(pivotClause.PivotColumnList.getObjectName(0).ToString().Equals("shipvia", StringComparison.CurrentCultureIgnoreCase));

            //        Assert.IsTrue(pivotClause.getPivotColumnList().size() == 3);
            Assert.IsTrue(pivotClause.PivotInClause.Items.size() == 3);
            //        Assert.IsTrue(pivotClause.getPivotColumnList().getObjectName(0).toString().equalsIgnoreCase("[1]"));
            Assert.IsTrue(pivotClause.PivotInClause.Items.getResultColumn(0).ToString().Equals("[1]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(pivotClause.AliasClause.ToString().Equals("sum_freight", StringComparison.CurrentCultureIgnoreCase));

            //        table = pivotedTable.getTableSource();
            //        Assert.IsTrue(table.getTableType() == ETableSource.pivoted_table);
            //        pivotedTable = table.getPivotedTable();

            pivotClause = pivotedTable.PivotClauseList[1];
            Assert.IsTrue(pivotClause.Type == TPivotClause.pivot);
            Assert.IsTrue(pivotClause.Aggregation_function.ToString().Equals("count(shipcountry)", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(pivotClause.PivotColumnList.getObjectName(0).ToString().Equals("Shipcountry", StringComparison.CurrentCultureIgnoreCase));
            //Assert.IsTrue(pivotClause.getPivotColumnList().size() == 3);
            Assert.IsTrue(pivotClause.PivotInClause.Items.size() == 3);
            //Assert.IsTrue(pivotClause.getPivotColumnList().getObjectName(0).toString().equalsIgnoreCase("[France]"));
            Assert.IsTrue(pivotClause.PivotInClause.Items.getResultColumn(0).ToString().Equals("[France]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(pivotClause.AliasClause.ToString().Equals("cntry", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testQualifiedName()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "CREATE VIEW Customers\n" + "AS\n" + "--Select from local member table.\n" + "SELECT *\n" + "FROM CompanyData.dbo.Customers_33\n" + "UNION ALL\n" + "--Select from member table on Server2.\n" + "SELECT *\n" + "FROM Server2.CompanyData.dbo.Customers_66\n" + "UNION ALL\n" + "--Select from mmeber table on Server3.\n" + "SELECT *\n" + "FROM Server3.CompanyData.dbo.Customers_99";
            int result = sqlparser.parse();
            Assert.IsTrue(result == 0);
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstcreateview);
            TCreateViewSqlStatement viewSqlStatement = (TCreateViewSqlStatement)sqlparser.sqlstatements.get(0);
            TSelectSqlStatement select = viewSqlStatement.Subquery;
            TSelectSqlStatement select1 = select.LeftStmt.RightStmt;
            TObjectName tableName = select1.tables.getTable(0).TableName;
            Assert.IsTrue(tableName.ServerToken.ToString().Equals("Server2", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateSequence()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "CREATE SEQUENCE Test.CountBy1\n" + "    START WITH 1\n" + "    INCREMENT BY 1 ;";
            Assert.IsTrue(sqlparser.parse() == 0);
            TCreateSequenceStmt stmt = (TCreateSequenceStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.Options[0].SequenceOptionType == ESequenceOptionType.startWith);
            Assert.IsTrue(stmt.Options[0].OptionValue.ToString().Equals("1", StringComparison.CurrentCultureIgnoreCase));
        }
        [TestMethod]
        public void testDrop()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "DROP SEQUENCE CountBy1 ; ;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TDropSequenceStmt stmt = (TDropSequenceStmt)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(stmt.SequenceName.ToString().Equals("CountBy1", StringComparison.CurrentCultureIgnoreCase));
        }


        StringBuilder sb = new StringBuilder(1024);

        protected internal void analyzeStmt(TCustomSqlStatement stmt)
        {
            for (int i = 0; i < stmt.tables.size(); i++)
            {
                TTable table = stmt.tables.getTable(i);
                if (table.BaseTable)
                {

                    if ((stmt.dbvendor == EDbVendor.dbvmssql) 
                        && ((string.Equals(table.FullName,"deleted",StringComparison.CurrentCultureIgnoreCase)) 
                        || (string.Equals(table.FullName,"inserted",StringComparison.CurrentCultureIgnoreCase))))
                    {
                        continue;
                    }

                    if (table.TableHintList == null)
                    {
                        //System.out.printf("No hint,table: %s\n",table.getFullName());
                        sb.Append(string.Format("No hint,table: {0}\n", table.FullName));
                    }
                    else
                    {
                        for (int j = 0; j < table.TableHintList.Count; j++)
                        {
                            TTableHint tableHint = table.TableHintList[j];
                            // System.out.printf("Hint: %s: table: %s\n",tableHint.toString(), table.getFullName());
                            sb.Append(string.Format("Hint: {0}: table: {1}\n", tableHint.ToString(), table.FullName));
                        }
                    }

                }

            }

            for (int i = 0; i < stmt.Statements.size(); i++)
            {
                analyzeStmt(stmt.Statements.get(i));
            }
        }

        [TestMethod]
        public void testTableHint1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "SELECT x.job_seeker_id, \n" + "       x.job_posting_id, \n" + "       x.job_seeker_ref, \n" + "       x.supplier_code, \n" + "       x.status, \n" + "       x.submit_time, \n" + "       (SELECT z.supplier_name \n" + "        FROM   dbo.Get_supplier_name ('user_type', 'suppress_supplier_flag', \n" + "               'role_flag', \n" + "                       x.jp_status, 'msp_coordinator_flag', x.supplier_name) AS \n" + "               z) AS \n" + "       supplier_name, \n" + "       x.work_order_id, \n" + "       x.work_order_ref, \n" + "       x.score, \n" + "       x.job_seeker_name, \n" + "       ( CASE \n" + "           WHEN x.duplicate != 0 THEN 'label.possibleDuplicateSubmittalCode ' \n" + "           ELSE '' \n" + "         END ) + ( CASE \n" + "                     WHEN x.potential_match != 0 THEN \n" + "                     'label.possibleMatchToWorkerCode ' \n" + "                     ELSE '' \n" + "                   END ) + ( CASE \n" + "                               WHEN x.pendingapproval != 0 THEN \n" + "                               'label.pendingApprovalCode ' \n" + "                               ELSE '' \n" + "                             END ) + ( CASE \n" + "                                         WHEN x.pendingprequalification != 0 \n" + "                                       THEN \n" + "                                         'label.pendingPreQualificationCode ' \n" + "                                         ELSE '' \n" + "                                       END ) + ( CASE \n" + "       WHEN x.prequalified != 0 THEN 'label.preQualifiedCode ' \n" + "       ELSE '' \n" + "                                                 END ) + ( CASE \n" + "       WHEN x.rejected != 0 THEN 'label.rejectedCode ' \n" + "       ELSE '' \n" + "                                                           END ) + \n" + "       ( CASE \n" + "           WHEN x.donothireflag != 0 THEN 'label.possibleDoNotHireCode ' \n" + "           ELSE '' \n" + "         END ) \n" + "       AS flag, \n" + "       x.buyer_supplier_contract_id, \n" + "       x.currency, \n" + "       Rtrim(CONVERT(DECIMAL(15, 2), x.strate)) \n" + "       AS strate \n" + "FROM   (SELECT js.job_seeker_id, \n" + "               js.job_posting_id, \n" + "               js.job_seeker_ref, \n" + "               js.supplier_code, \n" + "               CASE \n" + "                 WHEN js.status = 6 THEN 88 \n" + "                 ELSE js.status \n" + "               END \n" + "                      AS status, \n" + "               js.submit_time, \n" + "               cn.name \n" + "                      AS supplier_name, \n" + "               wo.work_order_id \n" + "                      AS work_order_id, \n" + "               wo.work_order_ref \n" + "                      AS work_order_ref, \n" + "               CONVERT(DECIMAL(15, 2), (js.score_from_cost + \n" + "               js.score_from_availability \n" + "                      + \n" + "               js.score_from_qualification )) \n" + "                      AS score, \n" + "               Isnull((SELECT COUNT(1) \n" + "                       FROM   dbo.security_id_mapping wk (nolock) \n" + "                              LEFT JOIN dbo.job_seeker jsd (nolock) \n" + "                                ON wk.job_seeker_id = jsd.job_seeker_id \n" + "                       WHERE  wk.job_seeker_id != js.job_seeker_id \n" + "                              AND ( EXISTS(SELECT 1 \n" + "                                           FROM   dbo.security_grou1p (nolock) \n" + "                                                  sg1 \n" + "                                                  INNER JOIN dbo.security_grou2p \n" + "                                                             ( \n" + "                                                             nolock) sg2 \n" + "                                                    ON sg1.security_group_id = \n" + "                                                       sg2.security_group_id \n" + "                                                       AND sg1.buyer_code = \n" + "                                                           wk.buyer_code \n" + "                                                       AND sg2.buyer_code = \n" + "                                                           js.buyer_code) \n" + "                                     OR wk.buyer_code = js.buyer_code ) \n" + "                              AND wk.job_posting_id != js.job_posting_id \n" + "                              AND ( ( wk.security_id != '' \n" + "                                      AND wk2.security_id != '' \n" + "                                      AND wk.security_id = wk2.security_id ) \n" + "                                     OR ( wk2.last_name = wk.last_name \n" + "                                          AND wk2.first_name = wk.first_name ) ) \n" + "                              AND wk.draft_flag = 0 \n" + "                              AND ( 'job_seeker_visibility_flag' = 0 \n" + "                                     OR ( wk.draft_flag = 0 \n" + "                                          AND ( ( 'job_seeker_visibility_flag' = \n" + "                                                  1 \n" + "                                                  AND jsd.status NOT IN ( \n" + "                                                      'job_seeker_status' ) \n" + "                                                ) \n" + "                                                 OR 'person_id' = \n" + "                                                    Isnull(wk.coordinator_id, '' \n" + "                                                    ) \n" + "                                                 OR 'person_id' = \n" + "                                                    Isnull(wk.distributor_id, '' \n" + "                                                    ) \n" + "                                                 OR 1 = 'msp_coordinator_flag' ) \n" + "                                        ) )), \n" + "               0) AS \n" + "               potential_match, \n" + "               Isnull((SELECT TOP (1) 1 \n" + "                       FROM   dbo.security_id_mapping wk (nolock) \n" + "                              LEFT JOIN dbo.job_seeker jsd (nolock) \n" + "                                ON wk.job_seeker_id = jsd.job_seeker_id \n" + "                       WHERE  wk.job_posting_id = js.job_posting_id \n" + "                              AND ( ( wk.security_id != '' \n" + "                                      AND wk2.security_id != '' \n" + "                                      AND wk.security_id = wk2.security_id ) \n" + "                                     OR ( wk2.last_name = wk.last_name \n" + "                                          AND wk2.first_name = wk.first_name ) ) \n" + "                              AND wk.draft_flag = 0 \n" + "                              AND wk.job_seeker_id != js.job_seeker_id \n" + "                              AND ( 'job_seeker_visibility_flag' = 0 \n" + "                                     OR ( wk.draft_flag = 0 \n" + "                                          AND ( ( 'job_seeker_visibility_flag' = \n" + "                                                  1 \n" + "                                                  AND jsd.status NOT IN ( \n" + "                                                      'job_seeker_status' ) \n" + "                                                ) \n" + "                                                 OR 'person_id' = \n" + "                                                    Isnull(wk.coordinator_id, '' \n" + "                                                    ) \n" + "                                                 OR 'person_id' = \n" + "                                                    Isnull(wk.distributor_id, '' \n" + "                                                    ) \n" + "                                                 OR 1 = 'msp_coordinator_flag' ) \n" + "                                        ) )), \n" + "               0) AS \n" + "               duplicate, \n" + "               CASE \n" + "                 WHEN \n" + "       dbo.Show_candidate_name('config_enabled_allow_candidate_anonymity', \n" + "       js.status) = \n" + "       1 THEN js.display_name \n" + "                 ELSE '-' \n" + "               END \n" + "                      AS job_seeker_name, \n" + "               Isnull((SELECT TOP (1) 1 \n" + "                       FROM   dbo.workforce_template wft (nolock), \n" + "                              dbo.job_posting jp (nolock) \n" + "                       WHERE  js.workforce_id = wft.workforce_id \n" + "                              AND jp.job_posting_id = js.job_posting_id \n" + "                              AND jp.job_template_id = wft.job_template_id \n" + "                              AND wft.status = 31), 0) \n" + "                      AS pendingapproval, \n" + "               Isnull((SELECT TOP (1) 1 \n" + "                       FROM   dbo.workforce_template wft (nolock), \n" + "                              dbo.job_posting jp (nolock) \n" + "                       WHERE  js.workforce_id = wft.workforce_id \n" + "                              AND jp.job_posting_id = js.job_posting_id \n" + "                              AND jp.job_template_id = wft.job_template_id \n" + "                              AND wft.status = 82), 0) \n" + "                      AS pendingprequalification, \n" + "               Isnull((SELECT TOP (1) 1 \n" + "                       FROM   dbo.workforce_template wft (nolock), \n" + "                              dbo.job_posting jp (nolock) \n" + "                       WHERE  js.workforce_id = wft.workforce_id \n" + "                              AND jp.job_posting_id = js.job_posting_id \n" + "                              AND jp.job_template_id = wft.job_template_id \n" + "                              AND wft.status = 83), 0) \n" + "                      AS prequalified, \n" + "               Isnull((SELECT TOP (1) 1 \n" + "                       FROM   dbo.workforce_template wft (nolock), \n" + "                              dbo.job_posting jp (nolock) \n" + "                       WHERE  js.workforce_id = wft.workforce_id \n" + "                              AND jp.job_posting_id = js.job_posting_id \n" + "                              AND jp.job_template_id = wft.job_template_id \n" + "                              AND wft.status = 5), 0) \n" + "                      AS rejected, \n" + "               Isnull((SELECT TOP (1) 1 \n" + "                       FROM   dbo.security_id_mapping wk (nolock), \n" + "                              dbo.buyer_supplier_contract bsc WITH(nolock) \n" + "                       WHERE  bsc.buyer_code = wk.buyer_code \n" + "                              AND bsc.supplier_code = wk.supplier_code \n" + "                              AND bsc.active_flag = 1 \n" + "                              AND ( EXISTS(SELECT 1 \n" + "                                           FROM   dbo.security_group (nolock) \n" + "                                                  sg1 \n" + "                                                  INNER JOIN dbo.security_group \n" + "                                                             (nolock \n" + "                                                             ) sg2 \n" + "                                                    ON sg1.security_group_id = \n" + "                                                       sg2.security_group_id \n" + "                                                       AND sg1.buyer_code = \n" + "                                                           wk.buyer_code \n" + "                                                       AND sg2.buyer_code = \n" + "                                                           js.buyer_code) \n" + "                                     OR wk.buyer_code = js.buyer_code ) \n" + "                              AND js.job_seeker_id != wk.job_seeker_id \n" + "                              AND wk.rehire_flag = 0 \n" + "                              AND ( ( wk.security_id != '' \n" + "                                      AND wk2.security_id != '' \n" + "                                      AND wk.security_id = wk2.security_id ) \n" + "                                     OR ( wk2.last_name = wk.last_name \n" + "                                          AND wk2.first_name = wk.first_name ) ) \n" + "                              AND 1 = 'doNotHireFlag'), 0) \n" + "                      AS donothireflag, \n" + "               js.status \n" + "                      AS jp_status, \n" + "               bsco.buyer_supplier_contract_id, \n" + "               js.currency, \n" + "               jsrate.strate \n" + "        FROM   (((((dbo.job_seeker js (nolock) \n" + "                    INNER JOIN dbo.security_id_mapping wk2 (nolock) \n" + "                      ON wk2.job_seeker_id = js.job_seeker_id \n" + "                    INNER JOIN dbo.job_posting_view jp (nolock) \n" + "                      ON jp.job_posting_id = js.job_posting_id) \n" + "                   LEFT JOIN dbo.work_order wo (nolock) \n" + "                     ON wo.job_seeker_id = js.job_seeker_id \n" + "                        AND wo.SEQUENCE = 1) \n" + "                  LEFT JOIN dbo.buyer_supplier_contract bsco (nolock) \n" + "                    ON bsco.buyer_code = jp.buyer_code \n" + "                       AND bsco.supplier_code = js.supplier_code \n" + "                       AND bsco.active_flag = 1) \n" + "                 INNER JOIN dbo.company_name cn (nolock) \n" + "                   ON js.supplier_code = cn.company_code \n" + "                      AND CONVERT(DATETIME, \n" + "                          CONVERT(VARCHAR(12), Isnull(js.submit_time, \n" + "                          Getdate \n" + "                          ( \n" + "                          ) \n" + "                          ))) \n" + "                          BETWEEN cn.start_date AND cn.end_date) \n" + "                LEFT JOIN (SELECT ( rate )          AS strate, \n" + "                                  js1.job_seeker_id AS job_seeker_id \n" + "                           FROM   dbo.job_seeker_rate jsr(nolock), \n" + "                                  dbo.job_seeker js1(nolock), \n" + "                                  dbo.rate_code(nolock) \n" + "                           WHERE  js1.job_seeker_id = jsr.job_seeker_id \n" + "                                  AND rate_code.rate_code_id = jsr.rate_code_id \n" + "                                  AND rate_code.rate_unit = 'Hr' \n" + "                                  AND jsr.rate_category_id = \n" + "                                      js1.buyer_code + 'ST') AS \n" + "                          jsrate \n" + "                  ON js.job_seeker_id = jsrate.job_seeker_id) \n" + "        WHERE  js.job_posting_id = 'job_posting_id' \n" + "               AND js.status NOT IN ( 0, 17, 20 ) \n" + "               AND ( 'job_seeker_visibility_flag' = 0 \n" + "                      OR ( 'job_seeker_visibility_flag' = 1 \n" + "                           AND js.status NOT IN ( 'job_seeker_status' ) ) \n" + "                      OR 'person_id' = jp.coordinator_id \n" + "                      OR 'person_id' = jp.distributor_id \n" + "                      OR EXISTS (SELECT 'x' \n" + "                                 FROM   dbo.person (nolock) \n" + "                                 WHERE  person_id = 'person_id' \n" + "                                        AND msp_coordinator_flag = 1) ) \n" + "               AND EXISTS (SELECT 1 \n" + "                           FROM   dbo.job_posting_distribution jpd (nolock) \n" + "                           WHERE  jpd.job_posting_id = jp.job_posting_id \n" + "                                  AND jpd.supplier_code = js.supplier_code))AS x \n" + "ORDER  BY x.submit_time ";
            Assert.IsTrue(sqlparser.parse() == 0);

            // StringBuffer sb = new StringBuffer(1024);


            for (int i = 0; i < sqlparser.sqlstatements.size(); i++)
            {
                analyzeStmt(sqlparser.sqlstatements.get(i));
            }

            Console.WriteLine(sb.ToString().Trim());
            Console.WriteLine("===================");
            Console.WriteLine("Hint: nolock: table: dbo.job_seeker\n" + "Hint: nolock: table: dbo.security_id_mapping\n" + "Hint: nolock: table: dbo.job_posting_view\n" + "Hint: nolock: table: dbo.work_order\n" + "Hint: nolock: table: dbo.buyer_supplier_contract\n" + "Hint: nolock: table: dbo.company_name\n" + "Hint: nolock: table: dbo.job_seeker_rate\n" + "Hint: nolock: table: dbo.job_seeker\n" + "Hint: nolock: table: dbo.rate_code\n" + "Hint: nolock: table: dbo.security_id_mapping\n" + "Hint: nolock: table: dbo.job_seeker\n" + "Hint: nolock: table: dbo.security_grou1p\n" + "Hint: nolock: table: dbo.security_grou2p\n" + "Hint: nolock: table: dbo.security_id_mapping\n" + "Hint: nolock: table: dbo.job_seeker\n" + "Hint: nolock: table: dbo.workforce_template\n" + "Hint: nolock: table: dbo.job_posting\n" + "Hint: nolock: table: dbo.workforce_template\n" + "Hint: nolock: table: dbo.job_posting\n" + "Hint: nolock: table: dbo.workforce_template\n" + "Hint: nolock: table: dbo.job_posting\n" + "Hint: nolock: table: dbo.workforce_template\n" + "Hint: nolock: table: dbo.job_posting\n" + "Hint: nolock: table: dbo.security_id_mapping\n" + "Hint: nolock: table: dbo.buyer_supplier_contract\n" + "Hint: nolock: table: dbo.security_group\n" + "Hint: nolock: table: dbo.security_group\n" + "Hint: nolock: table: dbo.person\n" + "Hint: nolock: table: dbo.job_posting_distribution");



            Assert.IsTrue(sb.ToString().Trim().Equals("Hint: nolock: table: dbo.job_seeker\n" + "Hint: nolock: table: dbo.security_id_mapping\n" + "Hint: nolock: table: dbo.job_posting_view\n" + "Hint: nolock: table: dbo.work_order\n" + "Hint: nolock: table: dbo.buyer_supplier_contract\n" + "Hint: nolock: table: dbo.company_name\n" + "Hint: nolock: table: dbo.job_seeker_rate\n" + "Hint: nolock: table: dbo.job_seeker\n" + "Hint: nolock: table: dbo.rate_code\n" + "Hint: nolock: table: dbo.security_id_mapping\n" + "Hint: nolock: table: dbo.job_seeker\n" + "Hint: nolock: table: dbo.security_grou1p\n" + "Hint: nolock: table: dbo.security_grou2p\n" + "Hint: nolock: table: dbo.security_id_mapping\n" + "Hint: nolock: table: dbo.job_seeker\n" + "Hint: nolock: table: dbo.workforce_template\n" + "Hint: nolock: table: dbo.job_posting\n" + "Hint: nolock: table: dbo.workforce_template\n" + "Hint: nolock: table: dbo.job_posting\n" + "Hint: nolock: table: dbo.workforce_template\n" + "Hint: nolock: table: dbo.job_posting\n" + "Hint: nolock: table: dbo.workforce_template\n" + "Hint: nolock: table: dbo.job_posting\n" + "Hint: nolock: table: dbo.security_id_mapping\n" + "Hint: nolock: table: dbo.buyer_supplier_contract\n" + "Hint: nolock: table: dbo.security_group\n" + "Hint: nolock: table: dbo.security_group\n" + "Hint: nolock: table: dbo.person\n" + "Hint: nolock: table: dbo.job_posting_distribution", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testThrow1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "THROW";
            Assert.IsTrue(sqlparser.parse() == 0);
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstmssqlthrow);
        }

        [TestMethod]
        public void testThrow2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "THROW 1,'sb',4000;";
            Assert.IsTrue(sqlparser.parse() == 0);
            Assert.IsTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstmssqlthrow);
            TMssqlThrow mssqlThrow = (TMssqlThrow)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(mssqlThrow.ErrorCode.ToString().Equals("1", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(mssqlThrow.ErrorMessage.ToString().Equals("'sb'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(mssqlThrow.ErrorState.ToString().Equals("4000", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testUse1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "use dbname";
            Assert.IsTrue(sqlparser.parse() == 0);

            TUseDatabase use = (TUseDatabase)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(use.DatabaseName.ToString().Equals("dbname", StringComparison.CurrentCultureIgnoreCase));

        }

        [TestMethod]
        public void testXMLfunction1()
        {

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "SELECT    p.Demographics.value('declare namespace awns=\"http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey\"; (awns:IndividualSurvey/awns:NumberCarsOwned) [1]',\n" + "                       'int') AS NumberCarsOwned,db1.schema1.func1(arg1)\n" + "FROM         Sales.Customer AS c INNER JOIN\n" + "                      Person.Person AS p ON p.BusinessEntityID = c.PersonID INNER JOIN\n" + "                      Person.BusinessEntityAddress AS a ON a.BusinessEntityID = p.BusinessEntityID INNER JOIN\n" + "                      Person.AddressType AS t ON a.AddressTypeID = t.AddressTypeID INNER JOIN\n" + "                      Person.Address AS ad ON ad.AddressID = a.AddressID INNER JOIN\n" + "                      Person.EmailAddress AS ea ON ea.BusinessEntityID = p.BusinessEntityID INNER JOIN\n" + "                      Person.StateProvince AS sp ON sp.StateProvinceID = ad.StateProvinceID\n" + "WHERE     (c.StoreID IS NULL) AND (t.Name = N'Home') AND (sp.CountryRegionCode = N'US')";
            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement selectSqlStatement = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TResultColumn column = selectSqlStatement.ResultColumnList.getResultColumn(0);
            TExpression expression = column.Expr;
            TFunctionCall functionCall = expression.FunctionCall;
            //System.out.println(functionCall.getFunctionName().getObjectToken().toString());
            Assert.IsTrue(functionCall.FunctionName.ObjectToken.DbObjType ==  EDbObjectType.function); // TObjectName.ttobjFunctionName

            TExpressionCallTarget callTarget = functionCall.CallTarget;
            Assert.IsTrue(callTarget.Expr.ToString().Equals("p.Demographics", StringComparison.CurrentCultureIgnoreCase));

            //        //System.out.println(functionCall.getFunctionName().getSchemaToken().toString());
            //        Assert.IsTrue(functionCall.getFunctionName().getSchemaToken().getDbObjType() == TObjectName.ttobjColumn);
            //        //System.out.println(functionCall.getFunctionName().getDatabaseToken().toString());
            //        Assert.IsTrue((functionCall.getFunctionName().getDatabaseToken().getDbObjType() == TObjectName.ttobjTable)
            //        ||(functionCall.getFunctionName().getDatabaseToken().getDbObjType() == TObjectName.ttObjTableAlias));


            TResultColumn column1 = selectSqlStatement.ResultColumnList.getResultColumn(1);
            TExpression expression1 = column1.Expr;
            TFunctionCall functionCall1 = expression1.FunctionCall;
            //System.out.println(functionCall1.getFunctionName().getObjectToken().toString());
            Assert.IsTrue(functionCall1.FunctionName.ObjectToken.DbObjType == EDbObjectType.function); // TObjectName.ttobjFunctionName
            //System.out.println(functionCall1.getFunctionName().getSchemaString().toString());
            Assert.IsTrue(functionCall1.FunctionName.SchemaToken.DbObjType ==  EDbObjectType.schema); // TObjectName.ttobjSchemaName
            // System.out.println(functionCall1.getFunctionName().getDatabaseToken().toString());
            Assert.IsTrue((functionCall1.FunctionName.DatabaseToken.DbObjType == EDbObjectType.database));// TObjectName.ttobjDatabaseName

        }

        [TestMethod]
        public void testWithUnion()
        {

            String varname1 = "";
            varname1 = varname1 + "WITH vnode(vref, vnam, vst) AS (SELECT vn.REFSTR, vn.NAME, vn.STEREOTYPE FROM VALUENODE vn WHERE vn.REFSTR = @BASE " + "\n";
            varname1 = varname1 + "UNION ALL " + "\n";
            varname1 = varname1 + "SELECT vn.REFSTR, vn.NAME, vn.STEREOTYPE FROM vnode v, VALUEARC va, VALUENODE vn " + "\n";
            varname1 = varname1 + "WHERE " + "\n";
            varname1 = varname1 + "va.HEAD = v.vref " + "\n";
            varname1 = varname1 + "AND va.TAIL = vn.REFSTR), vgol(vgref, vgnam) As ( " + "\n";
            varname1 = varname1 + "SELECT vref, vnam FROM vnode WHERE vst = 'Goal'), " + "\n";
            varname1 = varname1 + "TOTSC(tref,tot) " + "\n";
            varname1 = varname1 + "AS " + "\n";
            varname1 = varname1 + "( " + "\n";
            varname1 = varname1 + "SELECT CLASSCAPTION, SUM(SCORE) AS SCORE " + "\n";
            varname1 = varname1 + "FROM ( " + "\n";
            varname1 = varname1 + "SELECT class.vgnam AS CLASSCAPTION, " + "\n";
            varname1 = varname1 + "( SELECT AVG(ind.VALUE) " + "\n";
            varname1 = varname1 + "                    FROM   INDICATOR ind, EVALUATIONTYPE et " + "\n";
            varname1 = varname1 + "                               WHERE  ind.OBJECT = itpa.REFSTR " + "\n";
            varname1 = varname1 + "          AND  ind.EVALUATIONTYPE = et.REFSTR " + "\n";
            varname1 = varname1 + "          AND  et.NAME = 'BED Impact Indicators') AS SCORE FROM " + "\n";
            varname1 = varname1 + "ITPOLICY itp, ITPOLICYARCH itpa, vgol class " + "\n";
            varname1 = varname1 + "WHERE " + "\n";
            varname1 = varname1 + " itpa.OBJECT = class.vgref " + "\n";
            varname1 = varname1 + "  AND  itpa.POLICY = itp.REFSTR " + "\n";
            varname1 = varname1 + "  GROUP BY class.vgnam, itpa.REFSTR " + "\n";
            varname1 = varname1 + "  )x " + "\n";
            varname1 = varname1 + "  GROUP BY CLASSCAPTION " + "\n";
            varname1 = varname1 + ") " + "\n";
            varname1 = varname1 + "  " + "\n";
            varname1 = varname1 + "SELECT REFSTR, NAME, CLASSCAPTION, SUM(SCORE) AS SCORE, Tot AS tot " + "\n";
            varname1 = varname1 + "FROM ( " + "\n";
            varname1 = varname1 + "SELECT itp.REFSTR, itp.NAME, class.vgnam AS CLASSCAPTION, itpa.REFSTR AS ITPA, " + "\n";
            varname1 = varname1 + "       (SELECT AVG(ind.VALUE) " + "\n";
            varname1 = varname1 + "                    FROM   INDICATOR ind, EVALUATIONTYPE et " + "\n";
            varname1 = varname1 + "                               WHERE  ind.OBJECT = itpa.REFSTR " + "\n";
            varname1 = varname1 + "          --AND  ind.INDICATORTYPE = @INDTYPE " + "\n";
            varname1 = varname1 + "          AND  ind.EVALUATIONTYPE = et.REFSTR " + "\n";
            varname1 = varname1 + "          AND  et.NAME = 'BED Impact Indicators') AS SCORE, ts.tot AS Tot " + "\n";
            varname1 = varname1 + "FROM   ITPOLICY itp, ITPOLICYARCH itpa, vgol class, TOTSC ts " + "\n";
            varname1 = varname1 + "WHERE " + "\n";
            varname1 = varname1 + " itpa.OBJECT = class.vgref " + "\n";
            varname1 = varname1 + "  AND  itpa.POLICY = itp.REFSTR " + "\n";
            varname1 = varname1 + "  AND ts.tref = class.vgnam " + "\n";
            varname1 = varname1 + "GROUP BY itp.REFSTR, itp.NAME, class.vgnam, itpa.REFSTR, ts.tot " + "\n";
            varname1 = varname1 + ") x " + "\n";
            varname1 = varname1 + "GROUP BY REFSTR, NAME, CLASSCAPTION, Tot " + "\n";
            varname1 = varname1 + "ORDER BY NAME DESC, Tot DESC";

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = varname1;
            Assert.IsTrue(sqlparser.parse() == 0);

        }

        [TestMethod]
        public void testValueConstructor()
        {
            String varname1 = "";
            varname1 = varname1 + "INSERT INTO [db].[schema].[tbl] (id, LEA, memo, sort_major_id, lt_id, flag) " + "\n";
            varname1 = varname1 + "SELECT * FROM ( VALUES " + "\n";
            varname1 = varname1 + "	(1762,'x','y',16,1,0), " + "\n";
            varname1 = varname1 + "	(1857,'x','y',0,1,0), " + "\n";
            varname1 = varname1 + "	(1763,'x','y',17,1,0), " + "\n";
            varname1 = varname1 + "	(1858,'x','y',0,1,0), " + "\n";
            varname1 = varname1 + "	(1859,'x','y',0,1,0), " + "\n";
            varname1 = varname1 + "	(1901,'x','y',0,1,0) " + "\n";
            varname1 = varname1 + ") [kennzahlen_stat_schnittstelle_LEA_help] (LEA_id, LEA, LEA_memo, sort_major_id, liefertermin_id, ist_CA_intern)";

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = varname1;
            Assert.IsTrue(sqlparser.parse() == 0);


        }

        [TestMethod]
        public void testOpenrowSet()
        {
            String varname1 = "";
            varname1 = varname1 + "UPDATE jemas_dw.dbo.master_pag " + "\n";
            varname1 = varname1 + "SET logo = (SELECT * FROM OPENROWSET(BULK '\\\\121.png',  SINGLE_BLOB) AS x) " + "\n";
            varname1 = varname1 + ",	logo_35 = (SELECT * FROM OPENROWSET(BULK '\\\\121.png',  SINGLE_BLOB) AS x)";
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = varname1;
            Assert.IsTrue(sqlparser.parse() == 0);
        }

        [TestMethod]
        public void testTableHint()
        {
            String varname1 = "SELECT * FROM dbo.Employees (NOLOCK) WHERE EmployeeID = 16000";
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = varname1;
            Assert.IsTrue(sqlparser.parse() == 0);
            TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TTable table = select.tables.getTable(0);
            //Console.WriteLine(table.TableName.DbObjectType);
            //Console.WriteLine(table.TableType);
            Assert.IsTrue(table.TableType == ETableSource.objectname);

            varname1 = "SELECT * FROM dbo.Employees WITH (NOLOCK) WHERE EmployeeID = 16000";
            sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = varname1;
            Assert.IsTrue(sqlparser.parse() == 0);
            select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            table = select.tables.getTable(0);
            Assert.IsTrue(table.TableType == ETableSource.objectname);
        }

        [TestMethod]
        public void testDatabaseObjectsFunction()
        {
            String varname1 = "SELECT  round(convert(float,([JOB_ENDTIME]- [JOB_STARTTIME]))* 86400/60,2)";
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = varname1;
            Assert.IsTrue(sqlparser.parse() == 0);
            TCustomSqlStatement sql = sqlparser.sqlstatements.get(0);
            Assert.IsTrue(sql.DatabaseObjects.Count == 2);
            Assert.IsTrue(sql.DatabaseObjects[0].ToString().Equals("round", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sql.DatabaseObjects[0].DbObjectType == EDbObjectType.function);
            Assert.IsTrue(sql.DatabaseObjects[1].ToString().Equals("convert", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(sql.DatabaseObjects[1].DbObjectType == EDbObjectType.function);
        }

        [TestMethod]
        public void testExecParams()
        {
            String varname1 = "EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'test schema comment' , @level0type=N'SCHEMA',@level0name=N'test_schema';";
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = varname1;
            Assert.IsTrue(sqlparser.parse() == 0);
            TCustomSqlStatement sql = sqlparser.sqlstatements.get(0);
            Assert.IsTrue(sql.sqlstatementtype == ESqlStatementType.sstmssqlexec);
            TMssqlExecute execute = (TMssqlExecute)sql;
            Assert.IsTrue(execute.Parameters.size() == 4);
            TExecParameter execParameter = execute.Parameters.getExecParameter(0);
            Assert.IsTrue(execParameter.ParameterName.ToString().Equals("@name", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(execParameter.ParameterValue.ToString().Equals("N'MS_Description'", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateTableColumnRowguidcol()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "CREATE TABLE [dbo].[t3]([c1] [uniqueidentifier] ROWGUIDCOL NULL ) ON [PRIMARY]";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableStmt = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition column = createTableStmt.ColumnList.getColumn(0);
            Assert.IsTrue(column.ColumnName.ToString().Equals("[c1]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(column.RowGuidCol);
        }

        [TestMethod]
        public void testCreateTableColumnSparse()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "CREATE TABLE [dbo].[t3]([c1] INT SPARSE NULL) ON [PRIMARY]";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableStmt = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition column = createTableStmt.ColumnList.getColumn(0);
            Assert.IsTrue(column.ColumnName.ToString().Equals("[c1]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(column.sparseColumn);
        }

        [TestMethod]
        public void testCreateTableColumnFilestream()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = @"CREATE TABLE Archive.dbo.Records
                                    (
                                        [Id] [uniqueidentifier] ROWGUIDCOL NOT NULL UNIQUE, 
                                        [SerialNumber] INTEGER UNIQUE,
                                        [Chart] VARBINARY(MAX) FILESTREAM NULL
                                    )";

            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableStmt = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition column = createTableStmt.ColumnList.getColumn(2);
            Assert.IsTrue(column.ColumnName.ToString().Equals("[Chart]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(column.filestream);
        }

        [TestMethod]
        public void testCreateTableColumnPersisited()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = @"CREATE TABLE [dbo].[t1]
                                    (
                                     [c1] INT IDENTITY (5, 10) NOT NULL,
                                     [c2] NVARCHAR(40) NULL,
                                     [c3] NVARCHAR(20),
                                     [c4] NVARCHAR(20) COLLATE SQL_Latin1_General_CP1253_CS_AS,
                                     [c5] AS [c1] * [c1],
                                     [c6] AS [c1] * [c1] PERSISTED
                                    );";

            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateTableSqlStatement createTableStmt = (TCreateTableSqlStatement)sqlparser.sqlstatements.get(0);
            TColumnDefinition column = createTableStmt.ColumnList.getColumn(5);
            Assert.IsTrue(column.ColumnName.ToString().Equals("[c6]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(column.persistedColumn);
        }

        [TestMethod]
        public void testSourceTableOfColumn()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = @"SELECT [DATE] FROM (
	                                SELECT
	                                   LOGGEDIN AS [DATE]
	                                FROM
	                                SDC_SISUSERPAGELOADDATA SEL
                                )";


            Assert.IsTrue(sqlparser.parse() == 0);

            TSelectSqlStatement selectStmt = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
            TResultColumn resultColumn = selectStmt.ResultColumnList.getResultColumn(0);
            TObjectName columnName = resultColumn.Expr.ObjectOperand;
            Assert.IsTrue(columnName.SourceTable.TableType  == ETableSource.subquery);
        }

        [TestMethod]
        public void testCreateIndexFulltext()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "CREATE FULLTEXT INDEX ON [dbo].[Master] KEY INDEX [AK1_Master_Name] ON catalog_name;";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateIndexSqlStatement createIndexSqlStatement = (TCreateIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createIndexSqlStatement.IndexType == EIndexType.itFulltext);
            Assert.IsTrue(createIndexSqlStatement.IndexName.ToString().Equals("[AK1_Master_Name]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndexSqlStatement.TableName.ToString().Equals("[dbo].[Master]", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testCreateIndexSpatial()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "CREATE SPATIAL INDEX [IDX_Spacial] ON [dbo].[Master] ([Geom]) WITH (BOUNDING_BOX = (0,0,1,1));";
            Assert.IsTrue(sqlparser.parse() == 0);

            TCreateIndexSqlStatement createIndexSqlStatement = (TCreateIndexSqlStatement)sqlparser.sqlstatements.get(0);
            Assert.IsTrue(createIndexSqlStatement.IndexType == EIndexType.itSpatial);
            Assert.IsTrue(createIndexSqlStatement.IndexName.ToString().Equals("[IDX_Spacial]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(createIndexSqlStatement.TableName.ToString().Equals("[dbo].[Master]", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public void testAlterTableAddConstriant()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "ALTER TABLE [dbo].[t1] ADD CONSTRAINT [df_c1] DEFAULT 'abc' FOR [c1]";
            Assert.IsTrue(sqlparser.parse() == 0);

            TAlterTableStatement alterTableStatement = (TAlterTableStatement)sqlparser.sqlstatements.get(0);
            TAlterTableOption alterTableOption = alterTableStatement.AlterTableOptionList.getAlterTableOption(0);
            Assert.IsTrue(alterTableOption.OptionType == EAlterTableOptionType.AddConstraint);
            Assert.IsTrue(alterTableOption.ConstraintList.size() == 1);
            TConstraint constraint = alterTableOption.ConstraintList[0];
            Assert.IsTrue(constraint.ConstraintName.ToString().Equals("[df_c1]", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(constraint.DefaultExpression.ToString().Equals("'abc'", StringComparison.CurrentCultureIgnoreCase));
            Assert.IsTrue(constraint.DefaultForColumnName.ToString().Equals("[c1]", StringComparison.CurrentCultureIgnoreCase));
        }

    }
}
