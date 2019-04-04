## Description
Search sql files that include the specified parse tree node name in a directory recursively, 
The name of all sql files that include the specified parse tree node name will be printed.

Please check [toXML demo](./visitors) to find out how to use the visitor pattern introduced in this library.

## Usage
`java searchClause class_name directory`


Content in the test.sql:
```sql
SELECT
    CONVERT(VARCHAR(10), GETDATE(), 104) AS ActualDate 
FROM SomeTable
```

run this command:

`java searchClause TFunctionCall directory_include_sql_files`

will return:

`Find TFunctionCall in test.sql`


