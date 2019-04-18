using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using gudusoft.gsqlparser.nodes;
using gudusoft.gsqlparser.stmt.mssql;
using gudusoft.gsqlparser.stmt;
using gudusoft.gsqlparser.stmt.oracle;
using gudusoft.gsqlparser.pp.para;
using gudusoft.gsqlparser.pp.para.styleenums;
using gudusoft.gsqlparser.nodes.oracle;
using gudusoft.gsqlparser.pp.stmtformatter;
using gudusoft.gsqlparser.scriptWriter;

using System.Diagnostics;

namespace gudusoft.gsqlparser.test.scriptWriter
{

    [TestClass]
    public class testCreateNode
    {
        private TGSqlParser OracleParser = new TGSqlParser(EDbVendor.dbvoracle);
        private TGSqlParser SQLServerParser = new TGSqlParser(EDbVendor.dbvmssql);

        [TestMethod]
        public virtual void testCreateSourceToken()
        {
            TSourceToken st = new TSourceToken("AToken");
            Assert.IsTrue(st.toScript().Equals("AToken", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public virtual void testCreateObjectname()
        {
            // use new constructor to create an object name
            TObjectName tableName = new TObjectName(new TSourceToken("ATable"), EDbObjectType.table);
            Assert.IsTrue(tableName.ToScript().Equals("ATable", StringComparison.CurrentCultureIgnoreCase));

            TObjectName columnName = new TObjectName(new TSourceToken("ATable"), new TSourceToken("AColumn"), EDbObjectType.column);
            Assert.IsTrue(columnName.ToScript().Equals("ATable.AColumn", StringComparison.CurrentCultureIgnoreCase));

            // use parseObjectName() method to create a three parts object name
            TGSqlParser sqlParser = new TGSqlParser(EDbVendor.dbvmssql);
            columnName = sqlParser.parseObjectName("scott.emp.salary");
            Assert.IsTrue(columnName.ToScript().Equals("scott.emp.salary", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public virtual void testCreateConstant()
        {
            // use new constructor to create a constant object
            TConstant numberConstant = new TConstant(ELiteralType.numeric_et, new TSourceToken("9.1"));
            Assert.IsTrue(numberConstant.ToScript().Equals("9.1", StringComparison.CurrentCultureIgnoreCase));


            // use parseConstant() method to create a consatnt object
            TGSqlParser sqlParser = new TGSqlParser(EDbVendor.dbvmssql);
            numberConstant = sqlParser.parseConstant("9.1");
            Assert.IsTrue(numberConstant.ToScript().Equals("9.1", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public virtual void testCreateFunction()
        {
            TGSqlParser sqlParser = new TGSqlParser(EDbVendor.dbvmssql);
            TFunctionCall functionCall = sqlParser.parseFunctionCall("fx(a1,a2)");
            Assert.IsTrue(functionCall.FunctionName.ToScript().Equals("fx", StringComparison.CurrentCultureIgnoreCase));
        }

        [TestMethod]
        public virtual void testCreateSubquery()
        {
            TGSqlParser sqlParser = new TGSqlParser(EDbVendor.dbvmssql);
            string subqueryStr = @"SELECT * FROM CompanyData.dbo.Customers_33";
            TSelectSqlStatement subquery = sqlParser.parseSubquery(subqueryStr);
        }

        [TestMethod]
        public virtual void testCreateTable()
        {

            TCreateTableSqlStatement createTable = new TCreateTableSqlStatement(EDbVendor.dbvoracle);

            TTable table = new TTable();
            table.TableName = OracleParser.parseObjectName("newTable");
            createTable.TargetTable = table;

            TColumnDefinitionList columns = new TColumnDefinitionList();
            createTable.ColumnList = columns;

            TColumnDefinition column1 = new TColumnDefinition();
            columns.addColumn(column1);
            column1.ColumnName = OracleParser.parseObjectName("column1");
            TTypeName datatype1 = new TTypeName();
            datatype1.DataType = EDataType.number_t;
            datatype1.Precision = OracleParser.parseConstant("10");
            datatype1.Scale = OracleParser.parseConstant("2");
            column1.Datatype = datatype1;

            TConstraintList constraintList1 = new TConstraintList();
            column1.Constraints = constraintList1;
            TConstraint constraint1 = new TConstraint();
            constraintList1.addConstraint(constraint1);
            constraint1.Constraint_type = EConstraintType.primary_key;

            TColumnDefinition column2 = new TColumnDefinition();
            columns.addColumn(column2);
            column2.ColumnName = OracleParser.parseObjectName("column2");
            TTypeName datatype2 = new TTypeName();
            datatype2.DataType = EDataType.char_t;
            datatype2.Length = OracleParser.parseConstant("10");
            column2.Datatype = datatype2;

            TConstraintList constraintList2 = new TConstraintList();
            column2.Constraints = constraintList2;
            TConstraint constraint2 = new TConstraint();
            constraintList2.addConstraint(constraint2);
            constraint2.Constraint_type = EConstraintType.notnull;

            TColumnDefinition column3 = new TColumnDefinition();
            columns.addColumn(column3);
            column3.ColumnName = OracleParser.parseObjectName("title");
            TTypeName datatype3 = new TTypeName();
            datatype3.DataType = EDataType.varchar_t;
            datatype3.Length = OracleParser.parseConstant("20");
            column3.Datatype = datatype3;

            TConstraintList constraintList3 = new TConstraintList();
            column3.Constraints = constraintList3;
            TConstraint constraint3 = new TConstraint();
            constraintList3.addConstraint(constraint3);
            constraint3.Constraint_type = EConstraintType.default_value;
            constraint3.DefaultExpression = OracleParser.parseExpression("'manager'");

            TColumnDefinition column4 = new TColumnDefinition();
            columns.addColumn(column4);
            column4.ColumnName = OracleParser.parseObjectName("column4");
            TTypeName datatype4 = new TTypeName();
            datatype4.DataType = EDataType.integer_t;
            column4.Datatype = datatype4;

            TConstraintList constraintList4 = new TConstraintList();
            column4.Constraints = constraintList4;
            TConstraint constraint4 = new TConstraint();
            constraintList4.addConstraint(constraint4);
            constraint4.Constraint_type = EConstraintType.reference;
            constraint4.ReferencedObject = OracleParser.parseObjectName("table2");

            TObjectNameList referencedColumns = new TObjectNameList();
            referencedColumns.addObjectName(OracleParser.parseObjectName("ref_column"));
            constraint4.ReferencedColumnList = referencedColumns;

            TConstraintList tableConstraints = new TConstraintList();
            createTable.TableConstraints = tableConstraints;
            TConstraint tableConstraint = new TConstraint();
            tableConstraints.addConstraint(tableConstraint);
            tableConstraint.Constraint_type = EConstraintType.foreign_key;
            tableConstraint.ReferencedObject = OracleParser.parseObjectName("table3");

            TObjectNameList Columns = new TObjectNameList();
            Columns.addObjectName(OracleParser.parseObjectName("column1"));
            Columns.addObjectName(OracleParser.parseObjectName("column2"));
            tableConstraint.ColumnList = Columns;

            TObjectNameList referencedColumns2 = new TObjectNameList();
            referencedColumns2.addObjectName(OracleParser.parseObjectName("ref_column1"));
            referencedColumns2.addObjectName(OracleParser.parseObjectName("ref_column2"));
            tableConstraint.ReferencedColumnList = referencedColumns2;

            // System.out.println(scriptGenerator.generateScript(createTable,
            // true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle, createTable.ToScript(), "CREATE TABLE newtable(column1 NUMBER (10,2) PRIMARY KEY,\n" + "                      column2 CHAR (10) NOT NULL,\n" + "                      title   VARCHAR (20) DEFAULT 'manager',\n" + "                      column4 INTEGER REFERENCES table2(ref_column),\n" + "                      FOREIGN KEY (column1,column2) REFERENCES table3(ref_column1,ref_column2) )"));

        }

        [TestMethod]
        public virtual void testMssqlCreateTrigger()
        {
            TMssqlCreateTrigger createTrigger = new TMssqlCreateTrigger(EDbVendor.dbvmssql);

            createTrigger.TriggerName = SQLServerParser.parseObjectName("reminder");

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("titles");
            createTrigger.OnTable = table;

            createTrigger.TimingPoint = ETriggerTimingPoint.ttpFor;
            createTrigger.DmlTypes = ETriggerDmlType.tdtInsert | ETriggerDmlType.tdtUpdate;

            TStatementList stmts = new TStatementList();

            TMssqlRaiserror error = new TMssqlRaiserror(EDbVendor.dbvmssql);
            error.MessageText = SQLServerParser.parseExpression("50009");
            error.Severity = SQLServerParser.parseExpression("16");
            error.State = SQLServerParser.parseExpression("10");

            stmts.add(error);

            createTrigger.BodyStatements = stmts;

            string createTriggerQuery = "CREATE TRIGGER reminder\r\n" + "ON titles\r\n" + "FOR INSERT , UPDATE\r\n" + "AS RAISERROR (50009,16,10)";

           // Debug.WriteLine(createTrigger.ToScript());
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createTrigger.ToScript(), createTriggerQuery));

        }

        [TestMethod]
        public virtual void testMssqlCreateTrigger1()
        {
            TMssqlCreateTrigger createTrigger = new TMssqlCreateTrigger(EDbVendor.dbvmssql);

            createTrigger.TriggerName = SQLServerParser.parseObjectName("reminder");

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("titles");
            createTrigger.OnTable = table;

            createTrigger.TimingPoint = ETriggerTimingPoint.ttpTinsteadOf;
            createTrigger.DmlTypes = ETriggerDmlType.tdtInsert | ETriggerDmlType.tdtUpdate;

            TStatementList stmts = new TStatementList();

            TMssqlRaiserror error = new TMssqlRaiserror(EDbVendor.dbvmssql);
            error.MessageText = SQLServerParser.parseExpression("50009");
            error.Severity = SQLServerParser.parseExpression("16");
            error.State = SQLServerParser.parseExpression("10");

            stmts.add(error);

            createTrigger.BodyStatements = stmts;

            string createTriggerQuery = "CREATE TRIGGER reminder\r\n" + "ON titles\r\n" + "instead of INSERT , UPDATE\r\n" + "AS RAISERROR (50009,16,10)";

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createTrigger.ToScript(), createTriggerQuery));


        }

        [TestMethod]
        public virtual void testMssqlAlterTrigger2()
        {
            TMssqlCreateTrigger createTrigger = new TMssqlCreateTrigger(EDbVendor.dbvmssql);

            createTrigger.TriggerName = SQLServerParser.parseObjectName("reminder");
            createTrigger.AlterTrigger = true;

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("titles");
            createTrigger.OnTable = table;

            createTrigger.TimingPoint = ETriggerTimingPoint.ttpTinsteadOf;
            createTrigger.DmlTypes = ETriggerDmlType.tdtInsert | ETriggerDmlType.tdtUpdate;

            TStatementList stmts = new TStatementList();

            TMssqlRaiserror error = new TMssqlRaiserror(EDbVendor.dbvmssql);
            error.MessageText = SQLServerParser.parseExpression("50009");
            error.Severity = SQLServerParser.parseExpression("16");
            error.State = SQLServerParser.parseExpression("10");

            stmts.add(error);

            createTrigger.BodyStatements = stmts;

            string createTriggerQuery = "ALTER TRIGGER reminder\r\n" + "ON titles\r\n" + "instead of INSERT , UPDATE\r\n" + "AS RAISERROR (50009,16,10)";

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createTrigger.ToScript(), createTriggerQuery));


        }

        [TestMethod]
        public virtual void testMssqlCreateTrigger3()
        {
            TMssqlCreateTrigger createTrigger = new TMssqlCreateTrigger(EDbVendor.dbvmssql);

            createTrigger.TriggerName = SQLServerParser.parseObjectName("reminder");

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("titles");
            createTrigger.OnTable = table;

            createTrigger.TimingPoint = ETriggerTimingPoint.ttpTinsteadOf;
            createTrigger.DmlTypes = ETriggerDmlType.tdtInsert | ETriggerDmlType.tdtUpdate;

            TMssqlIfElse ifstmt = new TMssqlIfElse(EDbVendor.dbvmssql);
            ifstmt.Condition = SQLServerParser.parseExpression("update(col1)");
            TMssqlRaiserror error = new TMssqlRaiserror(EDbVendor.dbvmssql);
            error.MessageText = SQLServerParser.parseExpression("50009");
            error.Severity = SQLServerParser.parseExpression("16");
            error.State = SQLServerParser.parseExpression("10");
            ifstmt.Stmt = error;

            createTrigger.BodyStatements.add(ifstmt);

            string createTriggerQuery = "CREATE TRIGGER reminder\r\n" + "ON titles\r\n" + "instead of INSERT , UPDATE\r\n" + "AS if update(col1) RAISERROR (50009,16,10)";

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createTrigger.ToScript(), createTriggerQuery));

        }

        [TestMethod]
        public virtual void testMssqlCreateTrigger4()
        {
            TMssqlCreateTrigger createTrigger = new TMssqlCreateTrigger(EDbVendor.dbvmssql);

            createTrigger.TriggerName = SQLServerParser.parseObjectName("reminder");

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("titles");
            createTrigger.OnTable = table;

            createTrigger.TimingPoint = ETriggerTimingPoint.ttpTinsteadOf;
            createTrigger.DmlTypes = ETriggerDmlType.tdtInsert | ETriggerDmlType.tdtUpdate;

            TMssqlIfElse ifstmt = new TMssqlIfElse(EDbVendor.dbvmssql);
            ifstmt.Condition = SQLServerParser.parseExpression("update(col1)");
            TMssqlRaiserror error = new TMssqlRaiserror(EDbVendor.dbvmssql);
            error.MessageText = SQLServerParser.parseExpression("50009");
            error.Severity = SQLServerParser.parseExpression("16");
            error.State = SQLServerParser.parseExpression("10");
            TMssqlBlock block = new TMssqlBlock(EDbVendor.dbvmssql);
            block.BodyStatements.add(error);
            ifstmt.Stmt = block;

            createTrigger.BodyStatements.add(ifstmt);

            string createTriggerQuery = "CREATE TRIGGER reminder\r\n" + "ON titles\r\n" + "instead of INSERT , UPDATE\r\n" + "AS if update(col1)\r\n" + "  begin \r\n" + "  RAISERROR (50009,16,10)\r\n" + " end";

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createTrigger.ToScript(), createTriggerQuery));

        }

        [TestMethod]
        public virtual void testMssqlCreateTrigger5()
        {
            TMssqlCreateTrigger createTrigger = new TMssqlCreateTrigger(EDbVendor.dbvmssql);

            createTrigger.TriggerName = SQLServerParser.parseObjectName("reminder");

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("titles");
            createTrigger.OnTable = table;

            createTrigger.TimingPoint = ETriggerTimingPoint.ttpTinsteadOf;
            createTrigger.DmlTypes = ETriggerDmlType.tdtDelete | ETriggerDmlType.tdtInsert | ETriggerDmlType.tdtUpdate;

            TMssqlExecute exec = new TMssqlExecute(EDbVendor.dbvmssql);
            exec.ModuleName = SQLServerParser.parseObjectName("master..xp_sendmail");
            TExecParameterList @params = new TExecParameterList();
            TExecParameter param1 = new TExecParameter();
            param1.ParameterValue = SQLServerParser.parseExpression("'MaryM'");
            @params.addExecParameter(param1);
            TExecParameter param2 = new TExecParameter();
            param2.ParameterValue = SQLServerParser.parseExpression("'Don''t forget to print a report for the distributors.'");
            @params.addExecParameter(param2);
            exec.Parameters = @params;
            createTrigger.BodyStatements.add(exec);

            string createTriggerQuery = "CREATE TRIGGER reminder\r\n" + "ON titles\r\n" + "instead of DELETE , INSERT , UPDATE\r\n" + "AS EXEC master..xp_sendmail 'MaryM',\r\n" + "      'Don''t forget to print a report for the distributors.'";

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createTrigger.ToScript(), createTriggerQuery));

        }

        [TestMethod]
        public virtual void testMssqlCreateTrigger6()
        {
            TMssqlCreateTrigger createTrigger = new TMssqlCreateTrigger(EDbVendor.dbvmssql);

            createTrigger.TriggerName = SQLServerParser.parseObjectName("employee_insupd");

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("employee");
            createTrigger.OnTable = table;

            createTrigger.TimingPoint = ETriggerTimingPoint.ttpFor;
            createTrigger.DmlTypes = ETriggerDmlType.tdtInsert | ETriggerDmlType.tdtUpdate;

            TMssqlDeclare declare = new TMssqlDeclare(EDbVendor.dbvmssql);
            TDeclareVariableList vars = new TDeclareVariableList();
            TDeclareVariable @var = new TDeclareVariable();
            @var.VariableName = SQLServerParser.parseObjectName("@min_lvl");
            TTypeName datatype = new TTypeName();
            datatype.DataType = EDataType.tinyint_t;
            @var.Datatype = datatype;
            vars.addDeclareVariable(@var);

            TDeclareVariable var1 = new TDeclareVariable();
            var1.VariableName = SQLServerParser.parseObjectName("@max_lvl");
            TTypeName datatype1 = new TTypeName();
            datatype1.DataType = EDataType.tinyint_t;
            var1.Datatype = datatype1;
            vars.addDeclareVariable(var1);

            TDeclareVariable var2 = new TDeclareVariable();
            var2.VariableName = SQLServerParser.parseObjectName("@emp_lvl");
            TTypeName datatype2 = new TTypeName();
            datatype2.DataType = EDataType.tinyint_t;
            var2.Datatype = datatype2;
            vars.addDeclareVariable(var2);

            TDeclareVariable var3 = new TDeclareVariable();
            var3.VariableName = SQLServerParser.parseObjectName("@job_id");
            TTypeName datatype3 = new TTypeName();
            datatype3.DataType = EDataType.smallint_t;
            var3.Datatype = datatype3;
            vars.addDeclareVariable(var3);

            declare.Variables = vars;

            createTrigger.BodyStatements.add(declare);
            createTrigger.BodyStatements.add(SQLServerParser.parseSubquery("SELECT @min_lvl = min_lvl,\r\n" + "   @max_lvl = max_lvl,\r\n" + "   @emp_lvl = i.job_lvl,\r\n" + "   @job_id = i.job_id\r\n" + "FROM employee e INNER JOIN inserted i ON e.emp_id = i.emp_id\r\n" + "   JOIN jobs j ON j.job_id = i.job_id"));

            TMssqlIfElse ifElse = new TMssqlIfElse(EDbVendor.dbvmssql);
            ifElse.Condition = SQLServerParser.parseExpression("(@job_id = 1) and (@emp_lvl <> 10)");

            TMssqlBlock block = new TMssqlBlock(EDbVendor.dbvmssql);
            TMssqlRaiserror error = new TMssqlRaiserror(EDbVendor.dbvmssql);
            error.MessageText = SQLServerParser.parseExpression("'Job id 1 expects the default level of 10.'");
            error.Severity = SQLServerParser.parseExpression("16");
            error.State = SQLServerParser.parseExpression("1");
            block.BodyStatements.add(error);

            // TMssqlRollback rollback = new TMssqlRollback( EDbVendor.dbvmssql );
            // rollback.setTrans_or_work( new TSourceToken( "transaction" ) );
            // block.getBodyStatements( ).Add( rollback );

            ifElse.Stmt = block;

            TMssqlIfElse ifStmt = new TMssqlIfElse(EDbVendor.dbvmssql);
            ifStmt.Condition = SQLServerParser.parseExpression("NOT(@emp_lvl BETWEEN @min_lvl AND @max_lvl)");

            TMssqlBlock block1 = new TMssqlBlock(EDbVendor.dbvmssql);
            TMssqlRaiserror error1 = new TMssqlRaiserror(EDbVendor.dbvmssql);
            error1.MessageText = SQLServerParser.parseExpression("'The level for job_id:%d should be between %d and %d.'");
            error1.Severity = SQLServerParser.parseExpression("16");
            error1.State = SQLServerParser.parseExpression("1");
            TExpressionList expressions = new TExpressionList();
            expressions.addExpression(SQLServerParser.parseExpression("@job_id"));
            expressions.addExpression(SQLServerParser.parseExpression("@min_lvl"));
            expressions.addExpression(SQLServerParser.parseExpression("@max_lvl"));
            error1.Args = expressions;
            block1.BodyStatements.add(error1);

            // TMssqlRollback rollback1 = new TMssqlRollback( EDbVendor.dbvmssql );
            // rollback1.setTrans_or_work( new TSourceToken( "transaction" ) );
            // block1.getBodyStatements( ).Add( rollback );
            ifStmt.Stmt = block1;

            ifElse.ElseStmt = ifStmt;
            createTrigger.BodyStatements.add(ifElse);

            string createTriggerQuery = "CREATE TRIGGER employee_insupd\r\n" + "ON employee\r\n" + "FOR INSERT , UPDATE\r\n" + "AS\r\n" + "DECLARE @min_lvl tinyint,\r\n" + "   @max_lvl tinyint,\r\n" + "   @emp_lvl tinyint,\r\n" + "   @job_id smallint ;\r\n" + "SELECT @min_lvl = min_lvl,\r\n" + "   @max_lvl = max_lvl,\r\n" + "   @emp_lvl = i.job_lvl,\r\n" + "   @job_id = i.job_id\r\n" + "FROM employee e INNER JOIN inserted i ON e.emp_id = i.emp_id\r\n" + "   JOIN jobs j ON j.job_id = i.job_id;\r\n" + "IF (@job_id = 1) and (@emp_lvl <> 10)\r\n" + "BEGIN \r\n" + "   RAISERROR ('Job id 1 expects the default level of 10.',16,1)\r\n" + "END \r\n" + "ELSE \r\n" + "IF NOT(@emp_lvl BETWEEN @min_lvl AND @max_lvl)\r\n" + "BEGIN \r\n" + "   RAISERROR ('The level for job_id:%d should be between %d and %d.'," + "16,1,@job_id,@min_lvl,@max_lvl)\r\n" + "END ;";
            // + "   ROLLBACK TRANSACTION\r\n"
            // + "   ROLLBACK TRANSACTION\r\n"

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createTrigger.ToScript(), createTriggerQuery));
        }

        [TestMethod]
        public virtual void testMssqlCreateTrigger7()
        {
            TMssqlCreateTrigger createTrigger = new TMssqlCreateTrigger(EDbVendor.dbvmssql);

            createTrigger.TriggerName = SQLServerParser.parseObjectName("trig1");

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("authors");
            createTrigger.OnTable = table;

            createTrigger.TimingPoint = ETriggerTimingPoint.ttpFor;

            createTrigger.DmlTypes = ETriggerDmlType.tdtDelete | ETriggerDmlType.tdtInsert | ETriggerDmlType.tdtUpdate;

            createTrigger.BodyStatements.add(SQLServerParser.parseSubquery("SELECT a.au_lname, a.au_fname, x.info\r\n" + "FROM authors a INNER JOIN does_not_exist x\r\n" + "   ON a.au_id = x.au_id"));

            string createTriggerQuery = "CREATE TRIGGER trig1\r\n" + "on authors\r\n" + "FOR DELETE , INSERT , UPDATE\r\n" + "AS\r\n" + "   SELECT a.au_lname, a.au_fname, x.info\r\n" + "   FROM authors a INNER JOIN does_not_exist x\r\n" + "      ON a.au_id = x.au_id";

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createTrigger.ToScript(), createTriggerQuery));
        }

        [TestMethod]
        public virtual void testMssqlCreateTrigger8()
        {
            TMssqlCreateTrigger createTrigger = new TMssqlCreateTrigger(EDbVendor.dbvmssql);

            createTrigger.TriggerName = SQLServerParser.parseObjectName("trig2");

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("authors");
            createTrigger.OnTable = table;

            createTrigger.TimingPoint = ETriggerTimingPoint.ttpFor;

            createTrigger.DmlTypes = ETriggerDmlType.tdtInsert | ETriggerDmlType.tdtUpdate;

            TMssqlDeclare declare = new TMssqlDeclare(EDbVendor.dbvmssql);
            TDeclareVariableList vars = new TDeclareVariableList();
            TDeclareVariable @var = new TDeclareVariable();
            @var.VariableName = SQLServerParser.parseObjectName("@fax");
            TTypeName datatype = new TTypeName();
            datatype.DataType = EDataType.varchar_t;
            datatype.Length = SQLServerParser.parseConstant("12");
            @var.Datatype = datatype;
            vars.addDeclareVariable(@var);
            declare.Variables = vars;

            createTrigger.BodyStatements.add(declare);

            createTrigger.BodyStatements.add(SQLServerParser.parseSubquery("SELECT @fax = phone\r\n" + "FROM authors"));

            string createTriggerQuery = "CREATE TRIGGER trig2\r\n" + "ON authors\r\n" + "FOR INSERT , UPDATE\r\n" + "AS\r\n" + "   DECLARE @fax varchar (12);\r\n" + "   SELECT @fax = phone\r\n" + "   FROM authors;";

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createTrigger.ToScript(), createTriggerQuery));
        }

        [TestMethod]
        public virtual void testMssqlCreateTrigger9()
        {
            TMssqlCreateTrigger createTrigger = new TMssqlCreateTrigger(EDbVendor.dbvmssql);

            createTrigger.TriggerName = SQLServerParser.parseObjectName("updEmployeeData");

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("employeeData");
            createTrigger.OnTable = table;

            createTrigger.TimingPoint = ETriggerTimingPoint.ttpFor;
            createTrigger.DmlTypes = ETriggerDmlType.tdtUpdate;

            TMssqlIfElse ifElse = new TMssqlIfElse(EDbVendor.dbvmssql);
            ifElse.Condition = SQLServerParser.parseExpression("(COLUMNS_UPDATED() & 14) > 0");

            TMssqlBlock block = new TMssqlBlock(EDbVendor.dbvmssql);

            TInsertSqlStatement insert = new TInsertSqlStatement(EDbVendor.dbvmssql);
            TTable insertTable = new TTable();
            insertTable.TableName = SQLServerParser.parseObjectName("auditEmployeeData");
            insert.TargetTable = insertTable;

            TObjectNameList columnNameList = new TObjectNameList();
            insert.ColumnList = columnNameList;
            columnNameList.addObjectName(SQLServerParser.parseObjectName("audit_log_type"));
            columnNameList.addObjectName(SQLServerParser.parseObjectName("audit_emp_id"));
            columnNameList.addObjectName(SQLServerParser.parseObjectName("audit_emp_bankAccountNumber"));
            columnNameList.addObjectName(SQLServerParser.parseObjectName("audit_emp_salary"));
            columnNameList.addObjectName(SQLServerParser.parseObjectName("audit_emp_SSN"));

            insert.SubQuery = SQLServerParser.parseSubquery("SELECT 'OLD',\r\n" + "   del.emp_id,\r\n" + "   del.emp_bankAccountNumber,\r\n" + "   del.emp_salary,\r\n" + "   del.emp_SSN\r\n" + "FROM deleted del");

            block.BodyStatements.add(insert);

            TInsertSqlStatement insert1 = new TInsertSqlStatement(EDbVendor.dbvmssql);
            TTable insertTable1 = new TTable();
            insertTable1.TableName = SQLServerParser.parseObjectName("auditEmployeeData");
            insert1.TargetTable = insertTable1;

            TObjectNameList columnNameList1 = new TObjectNameList();
            insert1.ColumnList = columnNameList1;
            columnNameList1.addObjectName(SQLServerParser.parseObjectName("audit_log_type"));
            columnNameList1.addObjectName(SQLServerParser.parseObjectName("audit_emp_id"));
            columnNameList1.addObjectName(SQLServerParser.parseObjectName("audit_emp_bankAccountNumber"));
            columnNameList1.addObjectName(SQLServerParser.parseObjectName("audit_emp_salary"));
            columnNameList1.addObjectName(SQLServerParser.parseObjectName("audit_emp_SSN"));

            insert1.SubQuery = SQLServerParser.parseSubquery("SELECT 'NEW',\r\n" + "   ins.emp_id,\r\n" + "   ins.emp_bankAccountNumber,\r\n" + "   ins.emp_salary,\r\n" + "   ins.emp_SSN\r\n" + "FROM inserted ins");
            block.BodyStatements.add(insert1);

            ifElse.Stmt = block;
            createTrigger.BodyStatements.add(ifElse);

            string createTriggerQuery = "CREATE TRIGGER updEmployeeData\r\n" + "ON employeeData\r\n" + "FOR update\r\n" + "AS\r\n" + "   IF (COLUMNS_UPDATED() & 14) > 0\r\n" + "      BEGIN \r\n" + "      INSERT INTO auditEmployeeData\r\n" + "         (audit_log_type,\r\n" + "         audit_emp_id,\r\n" + "         audit_emp_bankAccountNumber,\r\n" + "         audit_emp_salary,\r\n" + "         audit_emp_SSN)\r\n" + "         SELECT 'OLD',\r\n" + "            del.emp_id,\r\n" + "            del.emp_bankAccountNumber,\r\n" + "            del.emp_salary,\r\n" + "            del.emp_SSN\r\n" + "         FROM deleted del;\r\n" + "      INSERT INTO auditEmployeeData\r\n" + "         (audit_log_type,\r\n" + "         audit_emp_id,\r\n" + "         audit_emp_bankAccountNumber,\r\n" + "         audit_emp_salary,\r\n" + "         audit_emp_SSN)\r\n" + "         SELECT 'NEW',\r\n" + "            ins.emp_id,\r\n" + "            ins.emp_bankAccountNumber,\r\n" + "            ins.emp_salary,\r\n" + "            ins.emp_SSN\r\n" + "         FROM inserted ins;\r\n" + "   END";

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createTrigger.ToScript(), createTriggerQuery));
        }

        [TestMethod]
        public virtual void testMssqlCreateTrigger10()
        {
            TMssqlCreateTrigger createTrigger = new TMssqlCreateTrigger(EDbVendor.dbvmssql);

            createTrigger.TriggerName = SQLServerParser.parseObjectName("tr1");

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("Customers");
            createTrigger.OnTable = table;

            createTrigger.TimingPoint = ETriggerTimingPoint.ttpFor;
            createTrigger.DmlTypes = ETriggerDmlType.tdtUpdate;

            TMssqlIfElse ifStmt = new TMssqlIfElse(EDbVendor.dbvmssql);
            ifStmt.Condition = SQLServerParser.parseExpression("( (SUBSTRING(COLUMNS_UPDATED(),1,1)=power(2,(3 - 1))\r\n" + "   + power(2,(5 - 1)))\r\n" + "   AND (SUBSTRING(COLUMNS_UPDATED(),2,1)=power(2,(1 - 1)))\r\n   )");

            TMssqlPrint print = new TMssqlPrint(EDbVendor.dbvmssql);
            TExpressionList expressionList = new TExpressionList();
            expressionList.addExpression(SQLServerParser.parseExpression("'Columns 3, 5 and 9 updated'"));
            print.Messages = expressionList;
            ifStmt.Stmt = print;

            createTrigger.BodyStatements.add(ifStmt);

            string createTriggerQuery = "CREATE TRIGGER tr1 ON Customers\r\n" + "FOR UPDATE\r\nAS\r\n" + "   IF ( (SUBSTRING(COLUMNS_UPDATED(),1,1)=power(2,(3 - 1))\r\n" + "      + power(2,(5 - 1)))\r\n" + "      AND (SUBSTRING(COLUMNS_UPDATED(),2,1)=power(2,(1 - 1)))\r\n" + "      )\r\n" + "   PRINT 'Columns 3, 5 and 9 updated'";

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createTrigger.ToScript(), createTriggerQuery));
        }

        [TestMethod]
        public virtual void testCreateView()
        {
            TCreateViewSqlStatement createView = new TCreateViewSqlStatement(EDbVendor.dbvoracle);

            createView.ViewName = SQLServerParser.parseObjectName("vNessusTargetHostExtract");

            string subQuery = "SELECT     LoadKey, vcHost, CASE WHEN iPluginid = 12053 THEN SUBSTRING(vcResult,CHARINDEX('resolves as',vcResult) + 12,(DATALENGTH(vcResult) - 1)\n" + "                      - (CHARINDEX('resolves as',vcResult) + 12)) ELSE 'No registered hostname' END AS vcHostName, vcport, LoadedOn, iRecordTypeID,\n" + "                      iAgentProcessID, iTableID\n" + "FROM         dbo.vNessusResultExtract";
            createView.Subquery = OracleParser.parseSubquery(subQuery);

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle, createView.ToScript(), "CREATE VIEW vNessusTargetHostExtract\nAS \n" + subQuery));

        }

        [TestMethod]
        public virtual void testQualifiedNameWithServer()
        {
            string query = "select * FROM Server2.CompanyData.dbo.Customers_66";
            SQLServerParser.sqltext = query;
            SQLServerParser.parse();
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle, SQLServerParser.sqlstatements.get(0).ToScript(), SQLServerParser.sqlstatements.get(0).ToScript().ToString()));
        }

        [TestMethod]
        public virtual void testCreateView2()
        {
            TCreateViewSqlStatement createView = new TCreateViewSqlStatement(EDbVendor.dbvmssql);

            createView.ViewName = SQLServerParser.parseObjectName("Customers");

            string subQuery = "	SELECT *\r\n" + "	FROM CompanyData.dbo.Customers_33\r\n" + "	UNION ALL\r\n" + "	SELECT *\r\n" + "	FROM Server2.CompanyData.dbo.Customers_66\r\n" + "	UNION ALL\r\n" + "	SELECT *\r\n" + "	FROM Server3.CompanyData.dbo.Customers_99";
            createView.Subquery = SQLServerParser.parseSubquery(subQuery);
            //System.out.print(createView.ToScript());

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createView.ToScript(), "CREATE VIEW Customers\nAS \n" + subQuery));

        }

        [TestMethod]
        public virtual void testCreateIndex()
        {
            TCreateIndexSqlStatement createIndex = new TCreateIndexSqlStatement(EDbVendor.dbvmssql);

            createIndex.IndexName = SQLServerParser.parseObjectName("IX_TransactionHistory_ReferenceOrderID");
            createIndex.NonClustered = true;
            createIndex.TableName = SQLServerParser.parseObjectName("Production.TransactionHistory");
            TOrderByItemList items = new TOrderByItemList();
            TOrderByItem item = new TOrderByItem();
            item.SortKey = SQLServerParser.parseExpression("ReferenceOrderID");
            items.addOrderByItem(item);
            createIndex.ColumnNameList = items;

            createIndex.FilegroupOrPartitionSchemeName = SQLServerParser.parseObjectName("TransactionsPS1");
            createIndex.PartitionSchemeColumns = new TObjectNameList();
            createIndex.PartitionSchemeColumns.addObjectName(SQLServerParser.parseObjectName("TransactionDate"));

            string createIndexQuery = "CREATE NONCLUSTERED INDEX IX_TransactionHistory_ReferenceOrderID\r\n" + "ON Production.TransactionHistory (ReferenceOrderID)\r\nON TransactionsPS1 (TransactionDate)";

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createIndex.ToScript(), createIndexQuery));
        }

        [TestMethod]
        public virtual void testCreateUniqueIndex()
        {
            TCreateIndexSqlStatement createIndex = new TCreateIndexSqlStatement(EDbVendor.dbvmssql);

            createIndex.IndexName = SQLServerParser.parseObjectName("AK_UnitMeasure_Name");
            createIndex.IndexType = EIndexType.itUnique;
            //		createIndex.getCreateIndexNode( )
            //				.setTableName( sqlParser.parseObjectName( "Production.UnitMeasure" ) );
            createIndex.TableName = SQLServerParser.parseObjectName("Production.UnitMeasure");
            TOrderByItemList items = new TOrderByItemList();
            TOrderByItem item = new TOrderByItem();
            item.SortKey = SQLServerParser.parseExpression("Name");
            items.addOrderByItem(item);
            //		createIndex.getCreateIndexNode( ).setColumnNameList( items );
            createIndex.ColumnNameList = items;

            string createIndexQuery = "CREATE UNIQUE INDEX AK_UnitMeasure_Name\n" + "    ON Production.UnitMeasure (Name)";
            //		System.out.print(scriptGenerator.generateScript( createIndex, true ));

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createIndex.ToScript(), createIndexQuery));

        }

        [TestMethod]
        public virtual void testCreateViewWithAlias()
        {
            TCreateViewSqlStatement createView = new TCreateViewSqlStatement(EDbVendor.dbvoracle);

            createView.ViewName = OracleParser.parseObjectName("test1");
            createView.StReplace = new TSourceToken("replace");

            TViewAliasItemList itemList = new TViewAliasItemList();
            TViewAliasItem item = new TViewAliasItem();
            item.Alias = OracleParser.parseObjectName("account_name_alias");
            itemList.addViewAliasItem(item);
            TViewAliasItem item1 = new TViewAliasItem();
            item1.Alias = OracleParser.parseObjectName("account_number_alias");
            itemList.addViewAliasItem(item1);

            TViewAliasClause aliasClause = new TViewAliasClause();
            aliasClause.ViewAliasItemList = itemList;

            createView.ViewAliasClause = aliasClause;

            string subQuery = "select account_name, account_number from \n" + "AP10_BANK_ACCOUNTS t";
            createView.Subquery = OracleParser.parseSubquery(subQuery);

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createView.ToScript(), "CREATE OR REPLACE VIEW test1(account_name_alias, account_number_alias)\nAS \n" + subQuery));

        }


        [TestMethod]
        public virtual void testOracleCreateProcedure()
        {
            TPlsqlCreateProcedure createProcedure = new TPlsqlCreateProcedure(EDbVendor.dbvoracle);

            createProcedure.ProcedureName = OracleParser.parseObjectName("evaluate");

            TParameterDeclarationList @params = new TParameterDeclarationList();
            TParameterDeclaration param = new TParameterDeclaration();
            param.ParameterName = OracleParser.parseObjectName("my_empno");
            TTypeName dataType = new TTypeName();
            dataType.DataType = EDataType.number_t;
            param.DataType = dataType;
            @params.addParameterDeclarationItem(param);
            createProcedure.ParameterDeclarations = @params;

            TInvokerRightsClause invoke = new TInvokerRightsClause();
            invoke.Definer = OracleParser.parseObjectName("current_user");
            createProcedure.InvokerRightsClause = invoke;

            TVarDeclStmt variable = new TVarDeclStmt(EDbVendor.dbvoracle);
            variable.ElementName = OracleParser.parseObjectName("my_ename");
            TTypeName datatype = new TTypeName();
            datatype.DataType = EDataType.varchar2_t;
            TConstant c = new TConstant(ELiteralType.integer_et);
            c.String = "15";
            datatype.Length = c;
            variable.DataType = datatype;
            createProcedure.DeclareStatements.add(variable);

            string selectQuery = "SELECT ename INTO my_ename FROM emp WHERE empno = my_empno;";
            createProcedure.BodyStatements.add(OracleParser.parseSubquery(selectQuery));

            string createProcedureQuery = "CREATE PROCEDURE evaluate(my_empno NUMBER) \r\n" + "AUTHID CURRENT_USER AS \r\n" + "my_ename VARCHAR2 (15); \r\n" + "BEGIN \r\n" + "SELECT ename INTO my_ename FROM emp WHERE empno = my_empno;\r\n" + "END ;";

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, createProcedure.ToScript(), createProcedureQuery));

        }

        [TestMethod]
        public virtual void testDropIndex()
        {
            TDropIndexSqlStatement dropIndex = new TDropIndexSqlStatement(EDbVendor.dbvmssql);


            TDropIndexItemList itemList = new TDropIndexItemList();
            TDropIndexItem item = new TDropIndexItem();
            item.IndexName = SQLServerParser.parseObjectName("IX_SalesPerson_SalesQuota_SalesYTD");
            item.ObjectName = SQLServerParser.parseObjectName("Sales.SalesPerson");
            itemList.addDropIndexItem(item);
            dropIndex.DropIndexItemList = itemList;

            string dropIndexQuery = "DROP INDEX IX_SalesPerson_SalesQuota_SalesYTD ON Sales.SalesPerson";
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, dropIndex.ToScript(), dropIndexQuery));


        }

        [TestMethod]
        public virtual void testUseDatabase()
        {
            TUseDatabase useDatabase = new TUseDatabase(EDbVendor.dbvmssql);

            useDatabase.DatabaseName = SQLServerParser.parseObjectName("AdventureWorks");

            string useDatabaseQuery = "USE AdventureWorks";
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, useDatabase.ToScript(), useDatabaseQuery));

        }


        //[TestMethod]
        //public virtual void testOracleIfStmt()
        //{
        //    TIfStmt ifStmt = new TIfStmt(EDbVendor.dbvoracle);
        //    TExpression left = OracleParser.parseExpression("ILevel");
        //    TExpression lowExpr = OracleParser.parseExpression("'Low Income'");
        //    TExpression avgExpr = OracleParser.parseExpression("'Avg Income'");
        //    TExpression highExpr = OracleParser.parseExpression("'High Income'");

        //    ifStmt.Condition = OracleParser.parseExpression("monthly_value <= 4000");
        //    ifStmt.ThenStatements.add(new TAssignStmt(left, lowExpr));

        //    TElsifStmt elsIf = new TElsifStmt();
        //    elsIf.Condition = OracleParser.parseExpression("monthly_value > 4000  and  monthly_value <= 7000");
        //    elsIf.ThenStatements.add(new TAssignStmt(left, avgExpr));
        //    ifStmt.ElseifStatements.add(elsIf);
        //    ifStmt.ElseStatements.add(new TAssignStmt(left, highExpr));

        //    string ifQuery = "IF monthly_value <= 4000 THEN \r\n" + "    ILevel = 'Low Income'\r\n" + " ELSIF monthly_value > 4000  and  monthly_value <= 7000 THEN \r\n" + "    ILevel = 'Avg Income'\r\n" + " ELSE ILevel = 'High Income'\r\n" + " END  IF";

        //    //System.out.println(ifStmt.ToScript());
        //    Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle, ifStmt.ToScript(), ifQuery));

        //}


       
        internal virtual string formatSql(string inputQuery, EDbVendor dbVendor)
        {
            string Result = inputQuery;
            TGSqlParser sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqltext = inputQuery;
            int ret = sqlparser.parse();
            if (ret == 0)
            {
                GFmtOpt option = GFmtOptFactory.newInstance();
                option.caseFuncname = TCaseOption.CoNoChange;
                Result = FormatterFactory.pp(sqlparser, option);
            }
            return Result;
        }

        [TestMethod]
        public virtual void testInsertSubquery()
        {
            TInsertSqlStatement insert = new TInsertSqlStatement(EDbVendor.dbvoracle);

            TTable table = new TTable();
            table.TableName = OracleParser.parseObjectName("table1");
            insert.TargetTable = table;

            TObjectNameList columnNameList = new TObjectNameList();
            insert.ColumnList = columnNameList;
            columnNameList.addObjectName(OracleParser.parseObjectName("column1"));
            columnNameList.addObjectName(OracleParser.parseObjectName("column2"));

            insert.SubQuery = OracleParser.parseSubquery("select c1,c1 from table2");

            // System.out.println(scriptGenerator.generateScript(insert, true));

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, insert.ToScript(), "INSERT INTO table1\n" + "            (column1,\n" + "             column2)\n" + "SELECT c1,\n" + "       c1\n" + "FROM   table2"));


        }

        [TestMethod]
        public virtual void testInsert()
        {
            TInsertSqlStatement insert = new TInsertSqlStatement(EDbVendor.dbvoracle);

            TTable table = new TTable();
            table.TableName = OracleParser.parseObjectName("table1");
            insert.TargetTable = table;

            TObjectNameList columnNameList = new TObjectNameList();
            insert.ColumnList = columnNameList;
            columnNameList.addObjectName(OracleParser.parseObjectName("column1"));
            columnNameList.addObjectName(OracleParser.parseObjectName("column2"));

            TMultiTargetList values = new TMultiTargetList();
            insert.Values = values;
            TMultiTarget multiTarget = new TMultiTarget();
            values.addMultiTarget(multiTarget);

            TResultColumnList resultColumnList = new TResultColumnList();
            multiTarget.ColumnList = resultColumnList;

            TResultColumn resultColumn1 = new TResultColumn();
            resultColumnList.addResultColumn(resultColumn1);
            resultColumn1.Expr = OracleParser.parseExpression("1");

            TResultColumn resultColumn2 = new TResultColumn();
            resultColumnList.addResultColumn(resultColumn2);
            resultColumn2.Expr = OracleParser.parseExpression("2");

            // System.out.println(scriptGenerator.generateScript(insert, true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle, insert.ToScript(), "INSERT INTO table1\n" + "            (column1,\n" + "             column2)\n" + "VALUES      (1,\n" + "             2)"));

        }

        [TestMethod]
        public virtual void testUpdateMultiTable()
        {
            TUpdateSqlStatement update = new TUpdateSqlStatement(EDbVendor.dbvmssql);

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("dbo.Table2");
            update.TargetTable = table;

            TResultColumnList resultColumnList = new TResultColumnList();
            update.ResultColumnList = resultColumnList;

            TResultColumn resultColumn1 = new TResultColumn();
            resultColumnList.addResultColumn(resultColumn1);
            TExpression left = SQLServerParser.parseExpression("dbo.Table2.ColB");
            TExpression right = SQLServerParser.parseExpression("dbo.Table2.ColB + dbo.Table1.ColB");
            resultColumn1.Expr = new TExpression(EExpressionType.assignment_t, left, right);

            TJoinList joinList = new TJoinList();
            update.joins = joinList;
            TJoin join = new TJoin();
            joinList.addJoin(join);
            TTable table1 = new TTable();
            join.Table = table1;
            table1.TableName = SQLServerParser.parseObjectName("dbo.Table2");

            TJoinItem joinItem = new TJoinItem();
            join.JoinItems.addJoinItem(joinItem);
            joinItem.JoinType = EJoinType.inner;
            TTable joinTable = new TTable();
            joinItem.Table = joinTable;

            joinTable.TableName = SQLServerParser.parseObjectName("dbo.Table1");
            joinItem.OnCondition = SQLServerParser.parseExpression("(dbo.Table2.ColA = dbo.Table1.ColA)");

            // System.out.println(scriptGenerator.generateScript(update, true));

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, update.ToScript(), "UPDATE dbo.table2\n" + "SET    dbo.table2.colb=dbo.table2.colb + dbo.table1.colb FROM dbo.table2 INNER JOIN dbo.table1 ON (dbo.table2.cola = dbo.table1.cola)"));

        }

        [TestMethod]
        public virtual void testUpdate()
        {
            TUpdateSqlStatement update = new TUpdateSqlStatement(EDbVendor.dbvoracle);

            TTable table = new TTable();
            table.TableName = OracleParser.parseObjectName("table1");
            update.TargetTable = table;

            TResultColumnList resultColumnList = new TResultColumnList();
            update.ResultColumnList = resultColumnList;

            TResultColumn resultColumn1 = new TResultColumn();
            resultColumnList.addResultColumn(resultColumn1);
            TExpression left = OracleParser.parseExpression("column1");
            TExpression right = OracleParser.parseExpression("1");

            resultColumn1.Expr = new TExpression(EExpressionType.assignment_t, left, right);

            TResultColumn resultColumn2 = new TResultColumn();
            resultColumnList.addResultColumn(resultColumn2);
            TExpression left2 = OracleParser.parseExpression("column2");
            TExpression right2 = OracleParser.parseExpression("1");

            resultColumn2.Expr = new TExpression(EExpressionType.assignment_t, left2, right2);

            TWhereClause whereClause = new TWhereClause();
            update.WhereClause = whereClause;
            whereClause.Condition = OracleParser.parseExpression("column3 > 250.00");

            // System.out.println(scriptGenerator.generateScript(update, true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle, update.ToScript(), "UPDATE table1\n" + "SET    column1=1,\n" + "       column2=1\n" + "WHERE  column3 > 250.00"));

        }

        [TestMethod]
        public virtual void testDeleteMultiTable()
        {
            TDeleteSqlStatement delete = new TDeleteSqlStatement(EDbVendor.dbvmssql);
            delete.FromKeyword = true;

            TTable table = new TTable();
            table.TableName = SQLServerParser.parseObjectName("Sales.SalesPersonQuotaHistory ");
            delete.TargetTable = table;

            TJoinList joinList = new TJoinList();
            delete.joins = joinList;
            TJoin join = new TJoin();
            joinList.addJoin(join);
            TTable table1 = new TTable();
            join.Table = table1;
            table1.TableName = SQLServerParser.parseObjectName("Sales.SalesPersonQuotaHistory");

            TAliasClause aliasClause = new TAliasClause();
            table1.AliasClause = aliasClause;
            aliasClause.HasAs = true;
            aliasClause.AliasName = SQLServerParser.parseObjectName("spqh");

            TJoinItem joinItem = new TJoinItem();
            join.JoinItems.addJoinItem(joinItem);
            joinItem.JoinType = EJoinType.inner;
            TTable joinTable = new TTable();
            joinItem.Table = joinTable;
            TAliasClause aliasClause2 = new TAliasClause();
            joinTable.AliasClause = aliasClause2;
            aliasClause2.HasAs = true;
            aliasClause2.AliasName = SQLServerParser.parseObjectName("sp");

            joinTable.TableName = SQLServerParser.parseObjectName("Sales.SalesPerson");
            joinItem.OnCondition = SQLServerParser.parseExpression("spqh.BusinessEntityID = sp.BusinessEntityID");

            TWhereClause whereClause = new TWhereClause();
            delete.WhereClause = whereClause;
            whereClause.Condition = SQLServerParser.parseExpression("sp.SalesYTD > 2500000.00");

            // System.out.println(scriptGenerator.generateScript(delete, true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvmssql, delete.ToScript(), "DELETE FROM sales.salespersonquotahistory FROM sales.salespersonquotahistory AS spqh INNER JOIN sales.salesperson AS sp ON spqh.businessentityid = sp.businessentityid\n" + "WHERE       sp.salesytd > 2500000.00"));

        }

        [TestMethod]
        public virtual void testDelete()
        {
            TDeleteSqlStatement delete = new TDeleteSqlStatement(EDbVendor.dbvoracle);
            delete.FromKeyword = true;

            TTable table = new TTable();
            table.TableName = OracleParser.parseObjectName("table1");
            delete.TargetTable = table;

            TWhereClause whereClause = new TWhereClause();
            delete.WhereClause = whereClause;
            whereClause.Condition = OracleParser.parseExpression("f1>0");

            // System.out.println(scriptGenerator.generateScript(delete, true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle, delete.ToScript(), "DELETE FROM table1\n" + "WHERE       f1 > 0"));

        }

        [TestMethod]
        public virtual void testSelect()
        {
            TSelectSqlStatement select = new TSelectSqlStatement(EDbVendor.dbvoracle);

            TResultColumnList resultColumnList = new TResultColumnList();
            select.ResultColumnList = resultColumnList;
            TResultColumn resultColumn1 = new TResultColumn();
            resultColumnList.addResultColumn(resultColumn1);
            resultColumn1.Expr = OracleParser.parseExpression("column1");

            TResultColumn resultColumn2 = new TResultColumn();
            resultColumnList.addResultColumn(resultColumn2);
            resultColumn2.Expr = OracleParser.parseExpression("column2");
            TAliasClause aliasClause = new TAliasClause();
            resultColumn2.AliasClause = aliasClause;
            aliasClause.HasAs = true;
            aliasClause.AliasName = OracleParser.parseObjectName("c_alias");
            // System.out.println( scriptGenerator.generateScript(select) );

            TJoinList joinList = new TJoinList();
            select.joins = joinList;
            TJoin join = new TJoin();
            joinList.addJoin(join);
            TTable table = new TTable();
            join.Table = table;
            // table.setTableType(ETableSource.objectname);
            table.TableName = OracleParser.parseObjectName("table1");

            TWhereClause whereClause = new TWhereClause();
            select.WhereClause = whereClause;
            whereClause.Condition = OracleParser.parseExpression("f1>0");

            TGroupBy groupBy = new TGroupBy();
            select.GroupByClause = groupBy;
            TGroupByItem groupByItem = new TGroupByItem();
            groupBy.Items.addGroupByItem(groupByItem);
            groupByItem.Expr = OracleParser.parseExpression("column1");
            groupBy.HavingClause = OracleParser.parseExpression("sum(column2) > 10");

            TOrderBy orderBy = new TOrderBy();
            select.OrderbyClause = orderBy;
            TOrderByItem orderByItem = new TOrderByItem();
            orderBy.Items.addElement(orderByItem);
            orderByItem.SortKey = OracleParser.parseExpression("column1");
            orderByItem.SortOrder = ESortType.desc;

            TOrderByItem orderByItem2 = new TOrderByItem();
            orderBy.Items.addElement(orderByItem2);
            orderByItem2.SortKey = OracleParser.parseExpression("column3");
            orderByItem2.SortOrder = ESortType.asc;

            // System.out.println(scriptGenerator.generateScript(select, true));
            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle, select.ToScript(), "SELECT   column1,\n" + "         column2 AS c_alias\n" + "FROM     table1\n" + "WHERE    f1 > 0\n" + "GROUP BY column1\n" + "HAVING  sum(column2) > 10\n" + "ORDER BY column1 DESC,\n" + "         column3 ASC"));

        }

        [TestMethod]
        public virtual void testSelectOracleJoin()
        {
            TSelectSqlStatement select = new TSelectSqlStatement(EDbVendor.dbvoracle);

            TResultColumnList resultColumnList = new TResultColumnList();
            select.ResultColumnList = resultColumnList;
            TResultColumn resultColumn1 = new TResultColumn();
            resultColumnList.addResultColumn(resultColumn1);
            resultColumn1.Expr = OracleParser.parseExpression("column1");

            TResultColumn resultColumn2 = new TResultColumn();
            resultColumnList.addResultColumn(resultColumn2);
            resultColumn2.Expr = OracleParser.parseExpression("column2");
            TAliasClause aliasClause = new TAliasClause();
            resultColumn2.AliasClause = aliasClause;
            aliasClause.HasAs = true;
            aliasClause.AliasName = OracleParser.parseObjectName("c_alias");

            TJoinList joinList = new TJoinList();
            select.joins = joinList;
            TJoin join = new TJoin();
            joinList.addJoin(join);
            TTable table = new TTable();
            join.Table = table;
            table.TableType = ETableSource.objectname;
            table.TableName = OracleParser.parseObjectName("table1");

            TJoin join2 = new TJoin();
            joinList.addJoin(join2);
            TTable table2 = new TTable();
            join2.Table = table2;
            // table2.setTableType(ETableSource.objectname);
            table2.TableName = OracleParser.parseObjectName("table2");

            TWhereClause whereClause = new TWhereClause();
            select.WhereClause = whereClause;
            whereClause.Condition = OracleParser.parseExpression("table1.f1 = table2.f1 and table1.f2 = 0");

            // System.out.println(scriptGenerator.generateScript(select, true));

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle, select.ToScript(), "SELECT column1,\n" + "       column2 AS c_alias\n" + "FROM   table1,\n" + "       table2\n" + "WHERE  table1.f1 = table2.f1\n" + "       AND table1.f2 = 0"));


        }

        [TestMethod]
        public virtual void testSelectAnsiJoin()
        {
            TSelectSqlStatement select = new TSelectSqlStatement(EDbVendor.dbvoracle);

            TResultColumnList resultColumnList = new TResultColumnList();
            select.ResultColumnList = resultColumnList;
            TResultColumn resultColumn1 = new TResultColumn();
            resultColumnList.addResultColumn(resultColumn1);
            resultColumn1.Expr = OracleParser.parseExpression("column1");

            TResultColumn resultColumn2 = new TResultColumn();
            resultColumnList.addResultColumn(resultColumn2);
            resultColumn2.Expr = OracleParser.parseExpression("column2");
            TAliasClause aliasClause = new TAliasClause();
            resultColumn2.AliasClause = aliasClause;
            aliasClause.HasAs = true;
            aliasClause.AliasName = OracleParser.parseObjectName("c_alias");

            TJoinList joinList = new TJoinList();
            select.joins = joinList;
            TJoin join = new TJoin();
            joinList.addJoin(join);
            TTable table = new TTable();
            join.Table = table;
            // table.setTableType(ETableSource.objectname);
            table.TableName = OracleParser.parseObjectName("table1");

            TJoinItem joinItem = new TJoinItem();
            join.JoinItems.addJoinItem(joinItem);
            joinItem.JoinType = EJoinType.inner;
            TTable joinTable = new TTable();
            joinItem.Table = joinTable;
            joinTable.TableName = OracleParser.parseObjectName("table2");
            joinItem.OnCondition = OracleParser.parseExpression("table1.f1 = table2.f1");

            TJoinItem joinItem3 = new TJoinItem();
            join.JoinItems.addJoinItem(joinItem3);
            joinItem3.JoinType = EJoinType.leftouter;
            TTable joinTable3 = new TTable();
            joinItem3.Table = joinTable3;
            joinTable3.TableName = OracleParser.parseObjectName("table3");
            joinItem3.OnCondition = OracleParser.parseExpression("table3.f1 = table2.f1");

            TJoinItem joinItem4 = new TJoinItem();
            join.JoinItems.addJoinItem(joinItem4);
            joinItem4.JoinType = EJoinType.rightouter;
            TTable joinTable4 = new TTable();
            joinItem4.Table = joinTable4;
            joinTable4.TableName = OracleParser.parseObjectName("table4");
            joinItem4.OnCondition = OracleParser.parseExpression("table4.f1 = table3.f1");

            TWhereClause whereClause = new TWhereClause();
            select.WhereClause = whereClause;
            whereClause.Condition = OracleParser.parseExpression("table1.f2 = 0");

            Assert.IsTrue(testScriptGenerator.verifyScript(EDbVendor.dbvoracle, select.ToScript(), "SELECT column1,\n" + "       column2 AS c_alias\n" + "FROM   table1\n" + "       INNER JOIN table2\n" + "       ON table1.f1 = table2.f1\n" + "       LEFT OUTER JOIN table3\n" + "       ON table3.f1 = table2.f1\n" + "       RIGHT OUTER JOIN table4\n" + "       ON table4.f1 = table3.f1\n" + "WHERE  table1.f2 = 0"));

            // System.out.println(scriptGenerator.generateScript(select, true));

        }
    }
}