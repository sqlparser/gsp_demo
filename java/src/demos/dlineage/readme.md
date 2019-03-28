## DataFlowAnalyzer

### description 
### Usage

### Links
- [database schema](https://db.apache.org/torque/torque-4.0/documentation/orm-reference/defining-the-schema.html)

## Dlineage
### description 
Collecting the data lineage model which includes the relationships between all 
source and target table columns.
With this data lineage model, we can look into the impact of changing the content or meaning of some data column
inside a lineage (forward analysis) or find sources of some data field (backward analysis).

This demo generates the same result when [columnImpact](../columnImpact) using the /s /c option.
The difference is the result of this demo is in XML format while the result of [columnImpact](../columnImpact) is in the plain text.

```sql
create view v1 as
SELECT a.deptno "Department", 
       a.num_emp/b.total_count "Employees", 
       a.sal_sum/b.total_sal "Salary"
  FROM
(SELECT deptno, COUNT(*) num_emp, SUM(SAL) sal_sum
    FROM scott.emp
    GROUP BY deptno) a,
(SELECT COUNT(*) total_count, SUM(sal) total_sal
    FROM scott.emp) b
```

Find the impact of the change of column: scott.emp.deptno by using "/fo &lt;table column>" option
```
scott.emp.deptno
--->v1."Department"
```

Find the source of v1.Department by using "/b &lt;view column>" option
```
v1."Department"
--->scott.emp.deptno
```


### Usage
`java Dlineage [/f <path_to_sql_file>] [/d <path_to_directory_includes_sql_files>] [/t <database type>] [/fo <table column>] [/b <view column>] [/ddl] [/s] [/log]`

- [/f &lt;path_to_sql_file>]
	
	Process a single SQL file.

- [/d &lt;path_to_directory_includes_sql_files>]
	
	Process all files under the diretory recursively. Only SQL filename ended with .sql extentsion will be processed.
	
- [/fo &lt;table column>]	
	
	forward analysis, look into the impact of changing the column 
	
- [/b &lt;view column>]	

	backward analysis, find sources of the specified view column.
	
- [/ddl]
		
		Generates the schema DDL script including all the database objects involved in the input SQL.
		the generated schema file comply to [database-4-0.xsd](https://db.apache.org/torque/torque-4.0/documentation/orm-reference/database-4-0.xsd)
		
		```
		<?xml version="1.0"?>
		<!DOCTYPE database SYSTEM "http://db.apache.org/torque/dtd/database.dtd">
		<database name="unknown">
			<table name="scott.emp">
				<column name="SAL"/>
				<column name="deptno"/>
				<column name="*"/>
			</table>
			<table name="RESULT SET COLUMNS" isView="true">
				<column name="&quot;Salary&quot;"/>
				<column name="&quot;Department&quot;"/>
				<column name="&quot;Employees&quot;"/>
			</table>
		</database>
		```

## DlineageRelation

### description 
### Usage
### Related
  - [first version, 2015-8](https://github.com/sqlparser/wings/issues/341)