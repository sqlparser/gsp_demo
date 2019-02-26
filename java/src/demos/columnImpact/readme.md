## Description
This tool provides the capability to match source and target columns to extract 
the column-level dataflow (lineage), even if the sources are in a subquery. 

This would be very useful to provide impact analysis feature to ETL mappings in data warehouses and marts.

Let's take this simple SQL for example:
```sql
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

Run this tool with /s option, the result is:
```
Department depends on: scott.emp.deptno
Employees depends on: scott.emp(total count of record influences the result value), scott.emp.deptno(because it is in group by clause)
Salary depends on: scott.emp.sal, scott.emp.deptno
```



## Usage
`java ColumnImpact scriptfile [/d]/[/s [/xml] [/c]]/[/v] [/o <output file path>] [/t <database type>]`

## Related demo

## Changes
-  [2012-01-11, first version](https://github.com/sqlparser/wings/issues/1)