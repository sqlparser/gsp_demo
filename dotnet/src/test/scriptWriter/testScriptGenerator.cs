using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gudusoft.gsqlparser.test.scriptWriter
{
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.nodes;
    using gudusoft.gsqlparser.pp.para;
    using gudusoft.gsqlparser.pp.para.styleenums;
    using gudusoft.gsqlparser.pp.stmtformatter;
    using gudusoft.gsqlparser.stmt;
   

    [TestClass]
    public class testScriptGenerator
    {
        [TestMethod]
        public virtual void testMySQLUpdateJoins()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"update user u join user_info ui on u.id=ui.userid set u.summa = 999 where u.id=1";

            sqlparser.parse();
            // Console.Out.WriteLine(formatSql(EDbVendor.dbvmysql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmysql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testMySQLOnduplicate()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"insert into user (id, name, summa) values(104,'Melony',2999) on duplicate key update sum=values(suma)";

            sqlparser.parse();
           // Console.Out.WriteLine(formatSql(EDbVendor.dbvmysql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmysql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testMySQLDelteFrom()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"delete from user where id=1";

            sqlparser.parse();
            // Console.Out.WriteLine(formatSql(EDbVendor.dbvmysql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmysql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testSQLServerColumnAlias()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = @"SELECT REFSTR, NAME, VERSION,
       'Test' = CASE WHEN STARTDATE < CURRENT_TIMESTAMP THEN 'Start in past'
                ELSE 'Start in the future'
                END
        FROM   APPLICATION";

            sqlparser.parse();
            //  Console.Out.WriteLine(formatSql(EDbVendor.dbvmssql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testSQLServerDeclare()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = @"DECLARE foo CURSOR FOR SELECT * FROM foo";

            sqlparser.parse();
            //  Console.Out.WriteLine(formatSql(EDbVendor.dbvmssql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testModifyColumnDefinition()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmysql);
            sqlparser.sqltext = @"CREATE TABLE `DEPT_MANAGER_TBL` ( 
                     `EMP_NO` INT(4) unsigned zerofill NOT NULL DEFAULT 1000, 
                     `DEPT_NO` CHAR(4) CHARACTER SET latin1 COLLATE latin1_german1_ci NOT NULL, 
                     `TO_DATE` GEOMETRY NOT NULL , `FROM_DATE` DATE NOT NULL, 
                     PRIMARY KEY (`EMP_NO`, `DEPT_NO`)
                    ) COLLATE=utf8_unicode_ci;";

            sqlparser.parse();

            //Console.WriteLine(sqlparser.sqlstatements.get(0).ToScript());
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));

        }

        [TestMethod]
        public virtual void testSQLServerHintOption()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = @"SELECT ProductID, OrderQty, SUM(LineTotal) AS Total  
                                    FROM Sales.SalesOrderDetail  
                                    WHERE UnitPrice < $5.00  
                                    GROUP BY ProductID, OrderQty  
                                    ORDER BY ProductID, OrderQty  
                                    OPTION (HASH GROUP, FAST 10);";

            sqlparser.parse();
           //  Console.Out.WriteLine(formatSql(EDbVendor.dbvmssql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testSQLServerCallTarget()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = @"SELECT STUFF(( SELECT N',' + CONVERT( nvarchar(18), Id )
                                         FROM dbo.MyTable
                                       FOR XML PATH( '' ), TYPE ).value( N'(./text())[1]', N'nvarchar(MAX)' ),
                                     1,
                                     1,
                                     N'' )";

            sqlparser.parse();
            // Console.Out.WriteLine(formatSql(EDbVendor.dbvmssql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testFetchWithTies()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"SELECT emp_id, emp_name, emp_salary
                  FROM t1
                  ORDER BY emp_salary
                  FETCH FIRST 5 PERCENT ROWS WITH TIES;";

            sqlparser.parse();
            // Console.Out.WriteLine(formatSql(EDbVendor.dbvmssql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testOffset()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"SELECT emp_id, emp_name, emp_salary
                                  FROM t1
                                  ORDER BY emp_salary
                                  OFFSET 5 ROWS FETCH NEXT 5 ROWS ONLY;";

            sqlparser.parse();
            // Console.Out.WriteLine(formatSql(EDbVendor.dbvmssql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testFetchFirst()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"update t2
                            set emp_salary = emp_salary * 1.5
                            where emp_id in 
                            (
                            SELECT emp_id
                              FROM t1
                              ORDER BY emp_salary
                              FETCH FIRST 5 ROWS ONLY
                            )";

            sqlparser.parse();
           // Console.Out.WriteLine(formatSql(EDbVendor.dbvmssql,sqlparser.sqlstatements.get(0).ToScript()));
             Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testFetch()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"   SELECT emp_id, emp_name, emp_salary
                                      FROM t1
                                      ORDER BY emp_salary
                                      FETCH FIRST 5 ROWS ONLY;";

            sqlparser.parse();
            // Console.Out.WriteLine(formatSql(EDbVendor.dbvmssql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testLateral()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"SELECT *
                                   FROM employee e,
                                        lateral(SELECT * FROM department d WHERE e.emp_dept = d.dpt_id)
                                                                    WHERE e.emp_id=6965998;";

            sqlparser.parse();
            //Console.Out.WriteLine(formatSql(EDbVendor.dbvmssql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }


        [TestMethod]
        public virtual void testUnPivotAlias()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = @"SELECT CustomerID,
       Phone
  FROM (
         SELECT CustomerID,
                Phone1,
                Phone2,
                Phone3
           FROM dbo.CustomerPhones
       ) CpAlias UNPIVOT (Phone FOR Phones IN(Phone1, Phone2, Phone3)) UnpivotAlias
    LEFT JOIN dbo.DummyTable
      ON 1 = 1";

            sqlparser.parse();
            // Console.Out.WriteLine(formatSql(EDbVendor.dbvmssql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testPivotAlias()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = @"SELECT 'AverageCost' AS Cost_Sorted_By_Production_Days,  
[0], [1], [2], [3], [4] 
FROM 
(SELECT DaysToManufacture, StandardCost  
    FROM Production.Product) AS SourceTable 
PIVOT 
( 
AVG(StandardCost) 
FOR DaysToManufacture IN ([0], [1], [2], [3], [4]) 
) AS PivotTable;";

            sqlparser.parse();
            //Console.Out.WriteLine(formatSql(EDbVendor.dbvmssql,sqlparser.sqlstatements.get(0).ToScript()));
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testTryCastFunction()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "SELECT TRY_CAST( 1 AS bigint )";

            sqlparser.parse();
            //Console.Out.WriteLine(sqlparser.sqlstatements.get(0).ToScript());
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testConvertFunction()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "SELECT CONVERT(DATETIME, '19000101', 112)";

            sqlparser.parse();
            // Console.Out.WriteLine(sqlparser.sqlstatements.get(0).ToScript());
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testBindVar()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = @"select emp_id, emp_dept
                                    into :b0 :b1,
                                    :b2 :b3
                                    from T1
                                    where rownum < 2;";

            sqlparser.parse();

           // Console.Out.WriteLine(sqlparser.sqlstatements.get(0).ToScript());
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));

            //		TScriptGenerator scriptGenerator = new TScriptGenerator( EDbVendor.dbvoracle );
            //		scriptGenerator.generateScript( sqlparser.sqlstatements.get( 0 ) );
            //		Assert.IsTrue( scriptGenerator.verifyScript( sqlparser.sqlstatements.get( 0 ) ) );
        }

        [TestMethod]
        public virtual void testForXML()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = @"select DISTINCT (', ' + dom.NAME)
		from APPLICATION app, BUSINESSSUPPORT bs, DOMAIN dom
		where app.ICTOBJECT = icto.REFSTR AND bs.OBJECT = app.REFSTR AND bs.XOBJECT = dom.REFSTR
		for xml path('')";

            sqlparser.parse();

            //Console.Out.WriteLine(sqlparser.sqlstatements.get(0).ToScript());
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));

            //		TScriptGenerator scriptGenerator = new TScriptGenerator( EDbVendor.dbvoracle );
            //		scriptGenerator.generateScript( sqlparser.sqlstatements.get( 0 ) );
            //		Assert.IsTrue( scriptGenerator.verifyScript( sqlparser.sqlstatements.get( 0 ) ) );
        }


        [TestMethod]
        public virtual void testCrossApply()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT d.department_name, v.employee_id, v.last_name\n" + "  FROM departments d CROSS APPLY (SELECT * FROM employees e\n" + "                                  WHERE e.department_id = d.department_id) v";

            sqlparser.parse();

            //System.out.println(sqlparser.sqlstatements.get(0).ToScript());
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));

            //		TScriptGenerator scriptGenerator = new TScriptGenerator( EDbVendor.dbvoracle );
            //		scriptGenerator.generateScript( sqlparser.sqlstatements.get( 0 ) );
            //		Assert.IsTrue( scriptGenerator.verifyScript( sqlparser.sqlstatements.get( 0 ) ) );
        }


        [TestMethod]
        public virtual void testIsOfType()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select a from b\n" + "where c is of type(only scott.tn)";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testForeignReferences()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE TABLE registered_students (\n" + "  student_id NUMBER(5) NOT NULL,\n" + "  department CHAR(3)   NOT NULL,\n" + "  course     NUMBER(3) NOT NULL,\n" + "  grade      CHAR(1),\n" + "  CONSTRAINT rs_grade\n" + "    CHECK (grade IN ('A', 'B', 'C', 'D', 'E')),\n" + "  CONSTRAINT rs_student_id\n" + "    FOREIGN KEY (student_id) REFERENCES students (id),\n" + "  CONSTRAINT rs_department_course\n" + "    FOREIGN KEY (department, course)\n" + "    REFERENCES classes (department, course)\n" + "  )";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testForUpdateOf()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select all department_id\n" + "from employees\n" + "for update of scott.employees.ename;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testDatabaseLink()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select emp.e@usa b from emp";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]

        public virtual void testDatabaseLink2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select user@!, sysdate@! from dual ;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testAnalyticFunction3()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT last_name, salary, department_id,\n" + "   PERCENTILE_CONT(0.5) WITHIN GROUP (ORDER BY salary1 DESC) \n" + "      OVER (PARTITION BY department_id) \"Percentile_Cont\",\n" + "   PERCENT_RANK() \n" + "      OVER (PARTITION BY department_id ORDER BY salary DESC) \n" + "\"Percent_Rank\"\n" + "FROM employees WHERE department_id IN (30, 60);";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testAnalyticFunction4()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT empno,\n" + "       deptno,\n" + "       sal,\n" + "       MIN(sal) KEEP (DENSE_RANK FIRST ORDER BY sal) OVER (PARTITION BY deptno) \"Lowest\",\n" + "       MAX(sal) KEEP (DENSE_RANK LAST ORDER BY sal) OVER (PARTITION BY deptno) \"Highest\"\n" + "FROM   emp\n" + "ORDER BY deptno, sal;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testAnalyticFunction5()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT empno,\n" + "       deptno,\n" + "       sal,\n" + "       DENSE_RANK() OVER (PARTITION BY deptno ORDER BY sal) \"rank\"\n" + "FROM   emp;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testAnalyticFunction6()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT empno,\n" + "              deptno,\n" + "              sal,\n" + "              RANK() OVER (PARTITION BY deptno ORDER BY sal) \"rank\"\n" + "       FROM   emp;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]

        public virtual void testAnalyticFunction7()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT empno, deptno, TO_CHAR(hiredate, 'YYYY') YEAR,\n" + "COUNT(*) OVER (PARTITION BY TO_CHAR(hiredate, 'YYYY')\n" + "ORDER BY hiredate ROWS BETWEEN 3 PRECEDING AND 1 FOLLOWING) FROM_P3_TO_F1,\n" + "COUNT(*) OVER (PARTITION BY TO_CHAR(hiredate, 'YYYY')\n" + "ORDER BY hiredate ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) FROM_PU_TO_C,\n" + "COUNT(*) OVER (PARTITION BY TO_CHAR(hiredate, 'YYYY')\n" + "ORDER BY hiredate ROWS BETWEEN 3 PRECEDING AND 1 PRECEDING) FROM_P2_TO_P1,\n" + "COUNT(*) OVER (PARTITION BY TO_CHAR(hiredate, 'YYYY')\n" + "ORDER BY hiredate ROWS BETWEEN 1 FOLLOWING AND 3 FOLLOWING) FROM_F1_TO_F3\n" + "FROM emp\n" + "ORDER BY hiredate;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]

        public virtual void testGroupBy1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT DECODE(GROUPING(department_name), 1, 'All Departments',\n" + "      department_name) AS department_name,\n" + "   DECODE(GROUPING(job_id), 1, 'All Jobs', job_id) AS job_id,\n" + "   COUNT(*) \"Total Empl\", AVG(salary) * 12 \"Average Sal\"\n" + "   FROM employees e, departments d\n" + "   WHERE d.department_id = e.department_id\n" + "   GROUP BY CUBE (department_name, job_id)";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]

        public virtual void testKeepDenseRank()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT department_id,\n" + "MIN(salary) KEEP (DENSE_RANK FIRST ORDER BY commission_pct) \"Worst\",\n" + "MAX(salary) KEEP (DENSE_RANK LAST ORDER BY commission_pct) \"Best\"\n" + "   FROM employees\n" + "   GROUP BY department_id;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]

        public virtual void testDeleteNestedTable()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "DELETE TABLE(SELECT h.people FROM hr_info h\n" + "   WHERE h.department_id = 280) p\n" + "   WHERE p.salary > 1700;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void test11()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT salary FROM employees\n" + "versions between scn minvalue and maxvalue\n" + "ORDER BY 1,2;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testOracleJoin2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select a from b \n" + "where waehrungscode_iso        = TO_NUMBER(e.code(+))";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testAnalyticFunction2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT manager_id, last_name, hire_date, salary,\n" + "   AVG(salary) OVER (PARTITION BY manager_id ORDER BY hire_date \n" + "   ROWS BETWEEN 1 PRECEDING AND 1 FOLLOWING) AS c_mavg\n" + "   FROM employees;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }


        [TestMethod]
        public virtual void testCreateTableDefault2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "create table myTable (\n" + "myColumn number  default null  null \n" + ");";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]

        public virtual void testCreateTableDefault()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "create table myTable (\n" + "myColumn number  default null not null\n" + ");";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testCreateViewDefault()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE VIEW vNessusTargetHostExtract \n" + "AS \n" + "SELECT     LoadKey, vcHost, CASE WHEN iPluginid = 12053 THEN SUBSTRING(vcResult, CHARINDEX('resolves as', vcResult) + 12, (DATALENGTH(vcResult) - 1) \n" + "                      - (CHARINDEX('resolves as', vcResult) + 12)) ELSE 'No registered hostname' END AS vcHostName, vcport, LoadedOn, iRecordTypeID, \n" + "                      iAgentProcessID, iTableID \n" + "FROM         dbo.vNessusResultExtract;";
            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testOracleCreateProcedure()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "CREATE PROCEDURE evaluate(my_empno NUMBER) \r\n" + "AUTHID CURRENT_USER AS \r\n" + "my_ename VARCHAR2 (15); \r\n" + "BEGIN \r\n" + "SELECT ename INTO my_ename FROM emp WHERE empno = my_empno;\r\n" + "END ;";
            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testDropIndex()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "DROP INDEX IX_SalesPerson_SalesQuota_SalesYTD ON Sales.SalesPerson;";
            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testUseDatabase()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
            sqlparser.sqltext = "USE AdventureWorks;";
            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvmssql, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]

        public virtual void testDelete()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "delete from department\n" + "where department_name = 'Finance';";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]

        public virtual void testJoinNested()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select a_join.f1\n" + "from ((a as a_alias left outer join a1 on a1.f1 = a_alias.f1) ) as a_join\n" + "join b on a_join.f1 = b.f1;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testJoinNested2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select * \n" + "FROM (a AS alias_a \n" + "   RIGHT JOIN ((b left outer join f on (b.f1=f.f2)) LEFT JOIN c \n" + "\t\tON (b.b1 = c.c1) AND (b.b2 = c.c2)) \n" + "\tON (a.a1 = b.b3) AND (a.a2 = b.b4)) b;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }


        [TestMethod]

        public virtual void testSelectAlias()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select last_name as name ,commission_pct comm,\n" + "salary*12 \"Annual Salary\"\n" + "from employees;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testComment()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select a\n" + "from b --s\n" + "--ss\n" + "where a in (1, 1>2 and c>d);";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testForUpdate()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select * from abc order by a for update nowait;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testConcatenate()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT /*+ use_hash(KUO) */\n" + "          C_BANK\n" + "       || '|'\n" + "from t  ";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testInlist()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select ANZ_MA\n" + "from t \n" + "WHERE   funktionscode IN ('U', 'H') ";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testAnalyticFunction()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select ROW_NUMBER() OVER \n" + "\t(PARTITION BY c_mandant, ma_parkey, me_parkey \n" + "\t\tORDER BY c_mandant, ma_parkey, me_parkey)  ANZ_MA\n" + "from t ";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testCase()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select CASE WHEN EXISTS (SELECT 1\n" + "                           FROM CDS_H_GRUPPE  GRP1\n" + "                          WHERE GRP1.c_mandant = c_mandant\n" + "                            AND GRP1.hist_datum    = ADD_MONTHS(LAST_DAY(TRUNC(SYSDATE)), -1)\n" + "                            AND GRP1.funktionscode = 'H'\n" + "                            AND GRP1.parkey1       = ma_parkey)\n" + "              THEN 1\n" + "          ELSE NULL\n" + "       END MA_ME\n" + "from t";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]

        public virtual void testSelectPivot()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT * FROM pivot_table\n" + "  UNPIVOT (yearly_total FOR order_mode IN (store AS 'direct', internet AS 'online'))\n" + "  ORDER BY year, order_mode;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testSelectWithParensOfUnion2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "(( \n" + "  select add_months(trunc(sysdate), -1) as dt\n" + "  from   dual\n" + "  union all\n" + "  select cte.dt+1 \n" + "  from   cte \n" + "  where  cte.dt+1 < sysdate\n" + ") order by 1)\n" + "\n";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testSelectWithParensOfUnion()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "( \n" + "  select add_months(trunc(sysdate), -1) as dt\n" + "  from   dual\n" + "  union all\n" + "  select cte.dt+1 \n" + "  from   cte \n" + "  where  cte.dt+1 < sysdate\n" + ") order by 1\n" + "\n";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]

        public virtual void testSelectWithParens2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT B.* FROM ((SELECT 2 FROM DUAL) B)";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testSelectWithParens()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "((select a from b\n" + "where a>c)\n" + "order by 1)";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]

        public virtual void testCTE()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "with cte (dt) as ( \n" + "  select add_months(trunc(sysdate), -1) as dt\n" + "  from   dual\n" + "  union all\n" + "  select cte.dt+1 \n" + "  from   cte \n" + "  where  cte.dt+1 < sysdate\n" + ")\n" + "  select * from cte;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }


        [TestMethod]
        public virtual void testSet2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select 'sing' as \"My dream\", 3 a_dummy\n" + "from dual\n" + "union\n" + "select 'I''d like to teach',1\n" + "from dual\n" + "union\n" + "select 'the world to',2\n" + "from dual\n" + "order by 2;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testSet1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select employee_id,job_id\n" + "from employees\n" + "union\n" + "select employee_id,job_id\n" + "from job_history;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testGroupBy()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select department_id,avg(salary)\n" + "from employees\n" + "group by department_id\n" + "having avg(salary) > 8000\n" + "order by sum(salary);";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testHierarchical()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "SELECT employee_id, last_name, manager_id\n" + "   FROM employees\n" + "   CONNECT BY PRIOR employee_id = manager_id;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testJoin3()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select t1.f1\n" + "from my.table1 t1\n" + " right outer join (\n" + " \t\t\t\t\t\t\t(my.table2 t2\n" + " \t\t\t\t\t\t\t\tleft outer join my.table3 t3\n" + " \t\t\t\t\t\t\t\t\ton (t2.f1 = t3.f2)\n" + " \t\t\t\t\t\t\t)\n" + " \t\t\t\t\t\tleft outer join (my.table4 t4\n" + " \t\t\t\t\t\t\t\t\t\t\t\t\tfull outer join my.table5 t5\n" + " \t\t\t\t\t\t\t\t\t\t\t\t\t\ton (t4.f1 = t5.f1)\n" + " \t\t\t\t\t\t\t\t\t\t\t ) t4alias\n" + " \t\t\t\t\t\t\ton (t4.b1 = t2.c1)\n" + " \t\t\t\t\t\t)\n" + " on (t1.a1 = t3.b3);";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testJoin2()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select t1.f1\n" + "from my.table1 t1\n" + " join (my.table2 t2\n" + " left outer join my.table3 t3\n" + " on t2.f1 = t3.f1) as joinalias1\n" + " on t1.f1 = t2.f1;";

            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testJoin()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select t1.f1\n" + "from my.table1 t1\n" + " join my.table2 t2 on t1.f1 = t2.f1\n" + " left outer join my.table3 t3 on t2.f1 = t3.f1";
            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testOracleJoin()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "Select t1.f1\n" + "from my.table1 t1,my.table2 t2\n" + "where t1.f1 = t2.f1\t";
            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void test1()
        {
            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = "select fx(1,2)+y from t";
            sqlparser.parse();
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, sqlparser.sqlstatements.get(0).ToString(), sqlparser.sqlstatements.get(0).ToScript()));
        }

        [TestMethod]
        public virtual void testCreateBinaryExpression()
        {
            TGSqlParser sqlParser = new TGSqlParser(EDbVendor.dbvoracle);

            TExpression left = sqlParser.parseExpression("1");
            TExpression right = sqlParser.parseExpression("2");
            TExpression plus = new TExpression(EExpressionType.arithmetic_plus_t,left,right);
            //plus.ExpressionType = EExpressionType.arithmetic_plus_t;
            //plus.LeftOperand = left;
            //plus.RightOperand = right;
            //System.out.println(plus.ToScript());
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, "1 + 2 ", plus.ToScript()));
        }

        [TestMethod]
        public virtual void testCreateComparisonPredicate()
        {
            TGSqlParser sqlParser = new TGSqlParser(EDbVendor.dbvoracle);

            TExpression left = sqlParser.parseExpression("salary");
            TExpression right = sqlParser.parseExpression("20");
            TExpression plus = new TExpression(EExpressionType.simple_comparison_t,left,right, EComparisonType.greaterThanOrEqualTo);
            //plus.ExpressionType = EExpressionType.simple_comparison_t;
            //plus.ComparisonType = EComparisonType.greaterThanOrEqualTo;
            //plus.LeftOperand = left;
            //plus.RightOperand = right;
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, "salary >= 20", plus.ToScript()));
        }

        [TestMethod]
        public virtual void testCreateAndPredicate()
        {
            TGSqlParser sqlParser = new TGSqlParser(EDbVendor.dbvoracle);

            TExpression left = sqlParser.parseExpression("salary");
            TExpression right = sqlParser.parseExpression("20");

            TExpression left2 = sqlParser.parseExpression("location");
            TExpression right2 = sqlParser.parseExpression("'NY'");

            TExpression c1 = new TExpression(EExpressionType.simple_comparison_t,left,right, EComparisonType.greaterThanOrEqualTo);
            //c1.ExpressionType = EExpressionType.simple_comparison_t;
            //c1.ComparisonType = EComparisonType.greaterThanOrEqualTo;
            //c1.LeftOperand = left;
            //c1.RightOperand = right;

            TExpression c2 = new TExpression(EExpressionType.simple_comparison_t,left2,right2, EComparisonType.equalsTo);
            //c2.ExpressionType = EExpressionType.simple_comparison_t;
            //c2.ComparisonType = EComparisonType.equalsTo;
            //c2.LeftOperand = left2;
            //c2.RightOperand = right2;

            TExpression c3 = new TExpression(EExpressionType.logical_and_t,c1,c2);
            //c3.ExpressionType = EExpressionType.logical_and_t;
            //c3.LeftOperand = c1;
            //c3.RightOperand = c2;
            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, "salary >= 20 and location = 'NY'", c3.ToScript()));

        }

        [TestMethod]
        public virtual void testCreateSubqueryPredicate()
        {
            TGSqlParser sqlParser = new TGSqlParser(EDbVendor.dbvoracle);

            TExpression left = sqlParser.parseExpression("salary");
            TExpression right = sqlParser.parseExpression("(select sal from emp where empno=1)");
            TExpression subqueryPredicate = new TExpression(EExpressionType.simple_comparison_t,left,right, EComparisonType.notLessThan);
            //subqueryPredicate.ExpressionType = EExpressionType.simple_comparison_t;
            //subqueryPredicate.ComparisonType = EComparisonType.notLessThan;
            //subqueryPredicate.LeftOperand = left;
            //subqueryPredicate.RightOperand = right;

            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, "salary !< (select sal from emp where empno=1)", subqueryPredicate.ToScript()));

            subqueryPredicate.ExpressionType = EExpressionType.group_comparison_t;
            subqueryPredicate.ComparisonType = EComparisonType.greaterThanOrEqualTo;
            subqueryPredicate.QuantifierType = EQuantifierType.all;
            //System.out.println(subqueryPredicate.ToScript());

            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, "salary >=  all (select sal from emp where empno=1)", subqueryPredicate.ToScript()));


        }

        [TestMethod]
        public virtual void testAssignStmt()
        {
            TGSqlParser sqlParser = new TGSqlParser(EDbVendor.dbvoracle);
            TAssignStmt assign = new TAssignStmt(sqlParser.parseExpression("ILevel"), sqlParser.parseExpression("'Low Income'"));

            //assign.Left = sqlParser.parseExpression("ILevel");
            //assign.Expression = sqlParser.parseExpression("'Low Income'");

            //System.out.println(assign.ToScript());

            Assert.IsTrue(verifyScript(EDbVendor.dbvoracle, "ILevel = 'Low Income'", assign.ToScript()));

        }


        public static bool verifyScript(EDbVendor dbVendor, string src, string target)
        {
            TGSqlParser sourceParser = new TGSqlParser(dbVendor);
            TGSqlParser targetParser = new TGSqlParser(dbVendor);
            sourceParser.sqltext = src;
            sourceParser.tokenizeSqltext();

            targetParser.sqltext = target;
            targetParser.tokenizeSqltext();

            return verifyTokens(sourceParser.sourcetokenlist, targetParser.sourcetokenlist, false);

        }

        private static bool verifyTokens(TSourceTokenList originalTokens, TSourceTokenList targetTokens, bool partialChecking)
        {
            bool result = true;
            int old = 0;
            bool startParenthesis = false;
            int nestedParenthesis = 0;

            for (int i = 0; i < targetTokens.size(); i++)
            {
                if (targetTokens.get(i).tokentype == ETokenType.ttkeyword)
                {
                    // must a space after keyword
                    if (i != targetTokens.size() - 1)
                    {
                        if ((targetTokens.get(i + 1).tokencode == TBaseType.lexnewline) || (targetTokens.get(i + 1).tokencode == TBaseType.lexspace) || (targetTokens.get(i + 1).tokencode < 127))
                        {
                            continue;
                        }
                        else
                        {
                            Console.Write("lack space after keyword:" + targetTokens.get(i).ToString());
                            result = false;
                            break;
                        }
                    }
                }

                if (targetTokens.get(i).tokentype == ETokenType.ttidentifier)
                {
                    // must a space between identifier and keyword/identifier
                    if (i != 0)
                    {
                        if ((targetTokens.get(i - 1).tokentype == ETokenType.ttkeyword) || (targetTokens.get(i - 1).tokentype == ETokenType.ttidentifier))
                        {
                            Console.Write("lack space between identifier and keyword:" + targetTokens.get(i).ToString());
                            result = false;
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

            }

            if (!result)
            {
                return result;
            }

            for (int i = 0; i < originalTokens.size(); i++)
            {
                if ((originalTokens.get(i).tokencode == TBaseType.lexnewline) || (originalTokens.get(i).tokencode == TBaseType.lexspace) 
                    || (originalTokens.get(i).tokentype == ETokenType.ttsimplecomment) || ((originalTokens.get(i).tokentype == ETokenType.ttbracketedcomment) &&(!originalTokens.get(i).ToString().StartsWith("/*+", StringComparison.Ordinal)))
                    || (originalTokens.get(i).tokentype == ETokenType.ttsemicolon))
                {
                    continue;
                }

                if (partialChecking)
                {
                    if (originalTokens.get(i).tokencode == '(')
                    {
                        startParenthesis = true;
                        nestedParenthesis++;
                    }
                    else if (originalTokens.get(i).tokencode == ')')
                    {
                        if (nestedParenthesis > 0)
                        {
                            nestedParenthesis--;
                        }
                        if ((nestedParenthesis == 0) && startParenthesis)
                        {
                            result = true;
                            break;
                        }
                    }
                }

                result = false;
                for (int j = old; j < targetTokens.size(); j++)
                {
                    if ((targetTokens.get(j).tokencode == TBaseType.lexnewline) || (targetTokens.get(j).tokencode == TBaseType.lexspace) 
                        || (targetTokens.get(j).tokentype == ETokenType.ttsimplecomment) || ((targetTokens.get(j).tokentype == ETokenType.ttbracketedcomment) && (!targetTokens.get(i).ToString().StartsWith("/*+", StringComparison.Ordinal)))
                        || (targetTokens.get(j).tokentype == ETokenType.ttsemicolon))
                    {
                        continue;
                    }

                    if ((originalTokens.get(i).tokencode == TBaseType.outer_join) && (targetTokens.get(j).tokencode == TBaseType.outer_join))
                    {
                        result = true;
                    }
                    else
                    {
                        result = originalTokens.get(i).ToString().Equals(targetTokens.get(j).ToString(), StringComparison.CurrentCultureIgnoreCase);
                    }

                    old = j + 1;
                    break;
                }

                if (!result)
                {
                    Console.Write("source token:" + originalTokens.get(i).ToString() + "(" + originalTokens.get(i).lineNo + "," + originalTokens.get(i).columnNo + ")");
                    Console.Write(", target token:" + targetTokens.get(old - 1).ToString() + "(" + targetTokens.get(old - 1).lineNo + "," + targetTokens.get(old - 1).columnNo + ")");
                    break;
                }
                //            if (! result) break;
            }


            return result;
        }

        public static string formatSql(EDbVendor dbVendor, string inputQuery)
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

    }
}