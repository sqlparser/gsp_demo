## Description
Search sql files that include the specified parse tree node name in a directory recursively, 
The name of all sql files that include the specified parse tree node name will be printed.


## Usage
`java searchClause class_name directory`


If this test.sql includs this content:
```sql
SELECT
    CONVERT(VARCHAR(10), GETDATE(), 104) AS ActualDate 
FROM SomeTable
```

run this command:
`java searchClause TFunctionCall directory_include_sql_files`

will return:
`Find TFunctionCall in test.sql`


Only SQL filename ended with .sql extentsion will be processed.

