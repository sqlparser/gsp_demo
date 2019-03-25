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
  - selectFromclauseComma = TLinefeedsCommaOption.LfAfterComma;
  - fromClauseInNewLine = false;
  - selectFromclauseJoinOnInNewline = true;
  - alignJoinWithFromKeyword = false;
  - andOrUnderWhere = false;
  - insertColumnlistStyle = TAlignStyle.AsStacked;
  - insertValuelistStyle = TAlignStyle.AsStacked;
  - beStyleCreatetableLeftBEOnNewline = false;
  - beStyleCreatetableRightBEOnNewline = false
  - createtableListitemInNewLine = false;
  - createtableFieldlistAlignOption = TAlignOption.AloLeft;
  - defaultCommaOption = TLinefeedsCommaOption.LfAfterComma;
  - defaultAligntype = TAlignStyle.AsStacked;
  - indentLen = 2;
  - useTab = false;
  - tabSize = 2;
  - beStyleFunctionBodyIndent = 2;
  - beStyleBlockLeftBEOnNewline = true;
  - beStyleBlockLeftBEIndentSize = 2;
  - beStyleBlockRightBEIndentSize = 2;
  - beStyleBlockIndentSize = 2;
  - beStyleIfElseSingleStmtIndentSize = 2;
  
  - caseWhenThenInSameLine = false;
  - indentCaseFromSwitch = 2;
  - indentCaseThen = 0;
  
	// keyword align option
  - selectKeywordsAlignOption = TAlignOption.AloLeft;
  - caseKeywords = TCaseOption.CoUppercase;
  - caseIdentifier = TCaseOption.CoNoChange;
  - caseQuotedIdentifier = TCaseOption.CoNoChange;
  - caseFuncname = TCaseOption.CoInitCap;
  - caseDatatype = TCaseOption.CoUppercase;

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
	

