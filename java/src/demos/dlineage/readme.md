## DataFlowAnalyzer

### description 
### Usage


## Dlineage
### description 
Collecting the data lineage model which includes the relationships between all 
source and target table columns.
With this data lineage model, we can look into the impact of changing the content or meaning of some data column
inside a lineage (forward analysis) or find sources of some data field (backward analysis).

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

-  [/f <path_to_sql_file>]
	
	Process a single SQL file.

-  [/d <path_to_directory_includes_sql_files>]
	
	Process all files under the diretory recursively. Only SQL filename ended with .sql extentsion will be processed.

## DlineageRelation

### description 
### Usage
### Related
  - [first version, 2015-8](https://github.com/sqlparser/wings/issues/341)