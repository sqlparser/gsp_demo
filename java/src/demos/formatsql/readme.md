## Description
Tidy and improve SQL readability with different format options. 
The output can be in html format as well.

```sql
WITH upd AS (
  UPDATE employees SET sales_count = sales_count + 1 WHERE id =
    (SELECT sales_person FROM accounts WHERE name = 'Acme Corporation')
    RETURNING * 
)
INSERT INTO employees_log SELECT *, current_timestamp FROM upd;";
```

formatted SQL
```sql
WITH upd
     AS ( UPDATE employees
SET    sales_count = sales_count + 1
WHERE  ID = (SELECT sales_person
             FROM   accounts
             WHERE  NAME = 'Acme Corporation') RETURNING * ) 
  INSERT INTO employees_log
  SELECT *,
         Current_timestamp
  FROM   upd;
```

## Usage
`java formatsql sqlfile.sql`

## Format options
  - [selectColumnlistStyle](https://github.com/sqlparser/sql-pretty-printer/wiki/Select-list#stacked-select-list), default value: TAlignStyle.AsStacked; 
  - [selectColumnlistComma](https://github.com/sqlparser/sql-pretty-printer/wiki/Select-list#stacked-select-list), defualt value: TLinefeedsCommaOption.LfAfterComma;
  - [selectItemInNewLine](https://github.com/sqlparser/sql-pretty-printer/wiki/Select-list#stacked-select-list), default value:  false
  - [alignAliasInSelectList](https://github.com/sqlparser/sql-pretty-printer/wiki/Alignments), default value:  true
  - [treatDistinctAsVirtualColumn], default value: false
  - [selectFromclauseStyle](https://github.com/sqlparser/sql-pretty-printer/wiki/From-clause) default value: TAlignStyle.AsStacked;
  - [selectFromclauseComma](https://github.com/sqlparser/sql-pretty-printer/wiki/From-clause), default value: TLinefeedsCommaOption.LfAfterComma;
  - [fromClauseInNewLine](https://github.com/sqlparser/sql-pretty-printer/wiki/From-clause), default value: false;
  - [selectFromclauseJoinOnInNewline](https://github.com/sqlparser/sql-pretty-printer/wiki/From-clause-(join)), default value: true;
  - alignJoinWithFromKeyword, default value: false;
  - [andOrUnderWhere](https://github.com/sqlparser/sql-pretty-printer/wiki/Where-clause), default value: false;
  
  // Insert statement
  - [insertColumnlistStyle](https://github.com/sqlparser/sql-pretty-printer/wiki/Insert-statement), default value: TAlignStyle.AsStacked;
  - [insertValuelistStyle](https://github.com/sqlparser/sql-pretty-printer/wiki/Insert-statement), default value: TAlignStyle.AsStacked;
  
  // create table
  - [beStyleCreatetableLeftBEOnNewline](https://github.com/sqlparser/sql-pretty-printer/wiki/Create-table-statement), default value: false;
  - [beStyleCreatetableRightBEOnNewline](https://github.com/sqlparser/sql-pretty-printer/wiki/Create-table-statement), default value: false
  - [createtableListitemInNewLine](https://github.com/sqlparser/sql-pretty-printer/wiki/Create-table-statement), default value: false;
  - [createtableFieldlistAlignOption](https://github.com/sqlparser/sql-pretty-printer/wiki/Create-table-statement), default value: TAlignOption.AloLeft;
  
  // default options
  - [defaultCommaOption](https://github.com/sqlparser/sql-pretty-printer/wiki/SQL-list-style), default value: TLinefeedsCommaOption.LfAfterComma;
  - [defaultAligntype](https://github.com/sqlparser/sql-pretty-printer/wiki/SQL-list-style), default value: TAlignStyle.AsStacked;
  
  // Indent
  - [indentLen](https://github.com/sqlparser/sql-pretty-printer/wiki/Indentation-general-indentation-size), default value: 2;
  - [useTab](https://github.com/sqlparser/sql-pretty-printer/wiki/Indentation-general-indentation-size), default value: false;
  - [tabSize](https://github.com/sqlparser/sql-pretty-printer/wiki/Indentation-general-indentation-size), default value: 2;
  - [beStyleFunctionBodyIndent](https://github.com/sqlparser/sql-pretty-printer/wiki/SQL-block-indentation), default value: 2;
  - [beStyleBlockLeftBEOnNewline](https://github.com/sqlparser/sql-pretty-printer/wiki/SQL-block-indentation), default value: true;
  - [beStyleBlockLeftBEIndentSize](https://github.com/sqlparser/sql-pretty-printer/wiki/SQL-block-indentation), default value: 2;
  - [beStyleBlockRightBEIndentSize](https://github.com/sqlparser/sql-pretty-printer/wiki/SQL-block-indentation), default value: 2;
  - [beStyleBlockIndentSize](https://github.com/sqlparser/sql-pretty-printer/wiki/SQL-block-indentation), default value: 2;
  - [beStyleIfElseSingleStmtIndentSize](https://github.com/sqlparser/sql-pretty-printer/wiki/Indentation-if-statement), default value: 2;
  
  // case when
  - [caseWhenThenInSameLine](https://github.com/sqlparser/sql-pretty-printer/wiki/Case-expression), default value: false;
  - [indentCaseFromSwitch](https://github.com/sqlparser/sql-pretty-printer/wiki/Case-expression), default value: 2;
  - [indentCaseThen](https://github.com/sqlparser/sql-pretty-printer/wiki/Case-expression), default value: 0;
  
	// keyword align option
  - selectKeywordsAlignOption = TAlignOption.AloLeft;
  
  - [caseKeywords](https://github.com/sqlparser/sql-pretty-printer/wiki/SQL-capitalization-keywords), default value: TCaseOption.CoUppercase;
  - [caseIdentifier](https://github.com/sqlparser/sql-pretty-printer/wiki/SQL-capitalization-identifier), default value: TCaseOption.CoNoChange;
  - [caseQuotedIdentifier](https://github.com/sqlparser/sql-pretty-printer/wiki/SQL-capitalization-identifier), default value: TCaseOption.CoNoChange;
  - [caseFuncname](https://github.com/sqlparser/sql-pretty-printer/wiki/SQL-capitalization-function), default value: TCaseOption.CoInitCap;
  - [caseDatatype](https://github.com/sqlparser/sql-pretty-printer/wiki/SQL-capitalization-datatype), default value: TCaseOption.CoUppercase;

	// WSPadding
  - wsPaddingOperatorArithmetic = true;
  - wsPaddingParenthesesInFunction = false;
  - wsPaddingParenthesesInExpression = true;
  - wsPaddingParenthesesOfSubQuery = false;
  - wsPaddingParenthesesInFunctionCall = false;
  - wsPaddingParenthesesOfTypename = false;

	// CTE
  - cteNewlineBeforeAs = true;
  - linebreakAfterDeclare = false;

  // create function
  - parametersStyle = TAlignStyle.AsStacked;

  - parametersComma = TLinefeedsCommaOption.LfAfterComma;
  - beStyleFunctionLeftBEOnNewline = false;
  - beStyleFunctionLeftBEIndentSize = 0;
  - beStyleFunctionRightBEOnNewline = true;
  - beStyleFunctionRightBEIndentSize = 0;
  - beStyleFunctionFirstParamInNewline = false;
  - linebreakBeforeParamInExec = true;

	// the empty lines
  - emptyLines = TEmptyLinesOption.EloMergeIntoOne;
  - insertBlankLineInBatchSqls = false;
  - noEmptyLinesBetweenMultiSetStmts = false;

	// line number
  - linenumberEnabled = false;
  - linenumberZeroBased = false;
  - linenumberLeftMargin = 0;
  - linenumberRightMargin = 2;

  - functionCallParametersStyle = TAlignStyle.AsWrapped;
  - functionCallParametersComma = TLinefeedsCommaOption.LfAfterComma;
  - removeComment = false;

	// used for compact mode
  - compactMode = TCompactMode.CpmNone;
  - lineWidth = 99;
	

