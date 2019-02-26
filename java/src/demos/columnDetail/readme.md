## Description
This tool parses SQL query to get the desired result about column and 
parameters in JSON format. It also reads metadata from DDL file in the 
same directory.
 
The output in JSON format looks like this:
```json
{"meta-data": {"columns": [
    {"WORK_LOC": {
        "alias": "",
        "autoIncrement": "",
        "catalogName": "",
        "check": "",
        "columnDisplaySize": "",
        "comment": "",
        "defaultValue": "",
        "foreignKey": "",
        "isNotNull": "",
        "isNull": "",
        "precision": "",
        "primaryKey": "",
        "readOnly": "",
        "scale": "",
        "schemaName": "",
        "tableName": "SFFND_WORK_LOC_DEF",
        "typeCode": "",
        "typeName": "",
        "unique": "",
        "writeable": ""
    }},
    {"LOCATION_ID": {
        "alias": "",
        "autoIncrement": "",
        "catalogName": "",
        "check": "",
        "columnDisplaySize": "",
        "comment": "",
        "defaultValue": "",
        "foreignKey": "",
        "isNotNull": "",
        "isNull": "",
        "precision": "",
        "primaryKey": "",
        "readOnly": "",
        "scale": "",
        "schemaName": "",
        "tableName": "SFFND_WORK_LOC_DEF",
        "typeCode": "",
        "typeName": "",
        "unique": "",
        "writeable": ""
    }},
...
```

## Usage
`java ColumnDetail <path_to_directory_includes_sql_files> [/t <database type>] [/s]`

## Changes
-  [2016-08-07, first version](https://github.com/sqlparser/wings/issues/342)