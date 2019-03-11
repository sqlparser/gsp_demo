using System;
using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.analyzesp
{
    using gudusoft.gsqlparser.demos.util;
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.nodes;
    using gudusoft.gsqlparser.stmt;
    using gudusoft.gsqlparser.stmt.mssql;
    using System.IO;
    using System.Text;

    public class analyzesp
    {

        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: analyzesp scriptfile [/t <database type>] [/o <output file path>] [/d <csv delimiter character>]");
                Console.WriteLine("/o: Option, write the output stream to the specified file.");
                Console.WriteLine("/d: Option, set the csv delimiter character. The default delimiter character instanceof '|'.");
                Console.WriteLine("/a: Option, check all items.");
                Console.WriteLine("/r: Option, check the database object relations in the store procedure.");
                Console.WriteLine("/f: Option, check the built-in functions in the store procedure.");
                Console.WriteLine("/t: Option, check if the store procedure contains try catch clause.");
                return;
            }

            List<string> array = new List<string>(args);

            List<FileInfo> files = new List<FileInfo>();

            for (int i = 0; i < array.Count; i++)
            {
                FileInfo file = new FileInfo(array[i].ToString());
                if (file.Exists)
                {
                    files.Add(file);
                }
                else
                {
                    break;
                }
            }

            string outputFile = null;

            int index = array.IndexOf("/o");

            if (index != -1 && args.Length > index + 1)
            {
                outputFile = args[index + 1];
            }

            string delimiter = "|";

            index = array.IndexOf("/d");

            if (index != -1 && args.Length > index + 1)
            {
                delimiter = args[index + 1];
            }

            bool checkTryCatchClause = false;
            bool checkBuiltInFunction = false;
            bool checkDBObjectRelations = false;

            if (array.IndexOf("/a") != -1)
            {
                checkTryCatchClause = true;
                checkBuiltInFunction = true;
                checkDBObjectRelations = true;
            }
            else
            {
                if (array.IndexOf("/f") != -1)
                {
                    checkBuiltInFunction = true;
                }
                if (array.IndexOf("/t") != -1)
                {
                    checkTryCatchClause = true;
                }
                if (array.IndexOf("/r") != -1)
                {
                    checkDBObjectRelations = true;
                }
            }

            if (checkBuiltInFunction == false && checkTryCatchClause == false)
            {
                checkDBObjectRelations = true;
            }

            analyzesp impact = new analyzesp(Common.GetEDbVendor(args, EDbVendor.dbvmssql), files, delimiter);
            impact.CheckBuiltInFunction = checkBuiltInFunction;
            impact.CheckTryCatchClause = checkTryCatchClause;
            impact.CheckDBObjectRelation = checkDBObjectRelations;
            impact.analyzeSQL();

            System.IO.FileStream writer = null;
            StreamWriter sw = null;

            if (!string.ReferenceEquals(outputFile, null))
            {
                try
                {
                    writer = new System.IO.FileStream(outputFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    sw = new StreamWriter(writer);
                    Console.SetOut(sw);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }

            if (impact.checkObjectRelation)
            {
                Console.WriteLine("DB of Anayzed Object"
                    + delimiter
                    + "Name of Analyzed Object"
                    + delimiter
                    + "Object Type"
                    + delimiter
                    + "Object Used"
                    + delimiter
                    + "Object Type"
                    + delimiter
                    + "Usage Type"
                    + delimiter
                    + "Columns");
                Console.WriteLine(impact.DBObjectRelationsAnalysisResult);
            }
            if (impact.checkBuiltInFunction)
            {
                Console.WriteLine("File Name" + delimiter + "Built-in Function" + delimiter + "Line Number" + delimiter + "Column Number" + delimiter + "Usage Type" + delimiter);
                Console.WriteLine(impact.BuiltInFunctionAnalysisResult);
            }
            if (impact.checkTryCatchClause_Renamed)
            {
                Console.WriteLine("File Name"
                    + delimiter
                    + "DB of Anayzed Object"
                    + delimiter
                    + "Procedure"
                    + delimiter
                    + "With Try Catch");
                Console.WriteLine(impact.TryCatchClauseAnalysisResult);
            }

            try
            {
                if (sw != null && writer != null)
                {
                    sw.Close();
                    writer.Close();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

            

        }

        private StringBuilder relationBuffer = new StringBuilder();
        private StringBuilder functionBuffer = new StringBuilder();
        private StringBuilder tryCatchBuffer = new StringBuilder();
        private LinkedHashMap<string, spInfo> spInfoMap = new LinkedHashMap<string, spInfo>();
        private List<string> files = new List<string>();
        private string delimiter;
        private bool checkBuiltInFunction, checkTryCatchClause_Renamed, checkObjectRelation;
        private EDbVendor eDbVendor = EDbVendor.dbvmssql;

        public analyzesp(List<FileInfo> sqlFiles, string delimiter)
        {
            this.delimiter = delimiter;
            if (sqlFiles.Count > 0)
            {
                for (int i = 0; i < sqlFiles.Count; i++)
                {
                    files.Add(sqlFiles[i].FullName);
                    spInfo sp = new spInfo();
                    sp.file = sqlFiles[i].FullName;
                    spInfoMap[sqlFiles[i].FullName] = sp;

                }
            }
        }

        public analyzesp(EDbVendor eDbVendor, List<FileInfo> sqlFiles, string delimiter)
        {
            this.eDbVendor = eDbVendor;
            this.delimiter = delimiter;
            if (sqlFiles.Count > 0)
            {
                for (int i = 0; i < sqlFiles.Count; i++)
                {
                    files.Add(sqlFiles[i].FullName);
                    spInfo sp = new spInfo();
                    sp.file = sqlFiles[i].FullName;
                    spInfoMap[sqlFiles[i].FullName] = sp;

                }
            }
        }

        internal virtual void analyzeProcedure(procedureInfo procedureInfo, TMssqlCreateProcedure procedure)
        {
            for (int i = 0; i < procedure.Statements.size(); i++)
            {
                TCustomSqlStatement stmt = procedure.Statements.get(i);
                analyzeSqlStatement(procedureInfo, stmt);
            }
        }

        public virtual void analyzeSQL()
        {
            for (int i = 0; i < files.Count; i++)
            {
                TGSqlParser sqlparser = new TGSqlParser(eDbVendor);
                sqlparser.sqlfilename = files[i];
                int ret = sqlparser.parse();
                if (ret != 0)
                {
                    Console.WriteLine("Parse file " + sqlparser.sqlfilename + " failed.");
                    Console.WriteLine(sqlparser.Errormessage);
                    continue;
                }
                spInfo sp = (spInfo)spInfoMap[files[i]];
                analyzeSQL(sp, sqlparser);
            }
        }

        private void analyzeSQL(spInfo spInfo, TGSqlParser sqlparser)
        {
            procedureInfo procedureInfo = new procedureInfo();
            spInfo.procedures.Add(procedureInfo);
            for (int i = 0; i < sqlparser.sqlstatements.size(); i++)
            {
                TCustomSqlStatement sql = sqlparser.sqlstatements.get(i);
                if (sql is TUseDatabase)
                {
                    spInfo.db = ((TUseDatabase)sql).DatabaseName.ToString();
                }
                else if (sql is TMssqlCreateProcedure)
                {
                    procedureInfo.name = ((TMssqlCreateProcedure)sql).ProcedureName.ToString();
                    procedureInfo.objectType = objectType.SP;
                    if (checkObjectRelation)
                    {
                        analyzeProcedure(procedureInfo, (TMssqlCreateProcedure)sql);
                    }
                    if (checkTryCatchClause_Renamed)
                    {
                        checkTryCatchClause(procedureInfo, (TMssqlCreateProcedure)sql);
                    }
                }
                else if (procedureInfo != null)
                {
                    analyzeSqlStatement(procedureInfo, sql);
                }

                if (checkBuiltInFunction)
                {
                    checkFunction(spInfo, sql.sourcetokenlist);
                }
            }
        }

        private void analyzeSqlStatement(procedureInfo procedureInfo, TCustomSqlStatement stmt)
        {
            if (stmt is TMssqlBlock)
            {
                TMssqlBlock block = (TMssqlBlock)stmt;
                if (block.BodyStatements != null)
                {
                    for (int i = 0; i < block.BodyStatements.size(); i++)
                    {
                        analyzeSqlStatement(procedureInfo, block.BodyStatements.get(i));
                    }
                }
            }
            else if (stmt is TMssqlIfElse)
            {
                TMssqlIfElse ifElse = (TMssqlIfElse)stmt;
                if (ifElse.Stmt != null)
                {
                    analyzeSqlStatement(procedureInfo, ifElse.Stmt);
                }
                if (ifElse.Condition != null)
                {

                }
                if (ifElse.ElseStmt != null)
                {
                    analyzeSqlStatement(procedureInfo, ifElse.ElseStmt);
                }
            }
            else if (stmt is TMssqlDeclare)
            {
                TMssqlDeclare declareStmt = (TMssqlDeclare)stmt;
                if (declareStmt.Subquery != null && declareStmt.Subquery.ToString().Trim().Length > 0)
                {
                    analyzeSqlStatement(procedureInfo, declareStmt.Subquery);
                }
            }
            else if (stmt is TMssqlExecute && ((TMssqlExecute)stmt).ModuleName != null)
            {
                TMssqlExecute executeStmt = (TMssqlExecute)stmt;
                operateInfo operateInfo = new operateInfo();
                operateInfo.objectType = objectType.SP;
                operateInfo.objectUsed = executeStmt.ModuleName.ToString().Trim();
                operateInfo.usageType = usageType.Exec;
                procedureInfo.operates.Add(operateInfo);
            }
            else if (stmt is TCreateTableSqlStatement)
            {
                TCreateTableSqlStatement createStmt = (TCreateTableSqlStatement)stmt;
                TColumnDefinitionList columns = createStmt.ColumnList;
                operateInfo operateInfo = new operateInfo();
                operateInfo.objectType = objectType.Table;
                operateInfo.objectUsed = createStmt.TargetTable.ToString().Trim();
                operateInfo.usageType = usageType.Create;
                for (int i = 0; i < columns.size(); i++)
                {
                    TColumnDefinition column = columns.getColumn(i);
                    operateInfo.columns.Add(column.ColumnName.ToString());
                }
                procedureInfo.operates.Add(operateInfo);

            }
            else if (stmt is TInsertSqlStatement)
            {
                TInsertSqlStatement insertStmt = (TInsertSqlStatement)stmt;
                TObjectNameList columns = insertStmt.ColumnList;
                operateInfo operateInfo = new operateInfo();
                operateInfo.objectType = objectType.Table;
                operateInfo.objectUsed = insertStmt.TargetTable.ToString().Trim();
                operateInfo.usageType = usageType.Insert;
                if (columns != null)
                {
                    for (int i = 0; i < columns.size(); i++)
                    {
                        TObjectName column = columns.getObjectName(i);
                        operateInfo.columns.Add(column.ToString());
                    }
                }
                procedureInfo.operates.Add(operateInfo);

                // if (insertStmt.ExecStmt != null)
                // {
                // analyzeSqlStatement(procedureInfo, insertStmt.ExecStmt);
                // }
            }
            else if (stmt is TUpdateSqlStatement)
            {
                TUpdateSqlStatement updateStmt = (TUpdateSqlStatement)stmt;
                TResultColumnList columns = updateStmt.ResultColumnList;
                operateInfo operateInfo = new operateInfo();
                operateInfo.objectType = objectType.Table;
                operateInfo.objectUsed = updateStmt.TargetTable.ToString().Trim();
                operateInfo.usageType = usageType.Update;
                for (int i = 0; i < columns.size(); i++)
                {
                    TResultColumn column = columns.getResultColumn(i);
                    operateInfo.columns.Add(column.Expr.LeftOperand.ToString());
                }
                procedureInfo.operates.Add(operateInfo);
            }
            else if (stmt is TDeleteSqlStatement)
            {
                TDeleteSqlStatement deleteStmt = (TDeleteSqlStatement)stmt;
                operateInfo operateInfo = new operateInfo();
                operateInfo.objectType = objectType.Table;
                operateInfo.objectUsed = deleteStmt.TargetTable.ToString().Trim();
                operateInfo.usageType = usageType.Delete;
                procedureInfo.operates.Add(operateInfo);
            }
            else if (stmt is TMssqlDropTable)
            {
                TMssqlDropTable dropStmt = (TMssqlDropTable)stmt;
                operateInfo operateInfo = new operateInfo();
                operateInfo.objectType = objectType.Table;
                operateInfo.objectUsed = dropStmt.TargetTable.ToString().Trim();
                operateInfo.usageType = usageType.Drop;
                procedureInfo.operates.Add(operateInfo);
            }
            else if (stmt is TDropTableSqlStatement)
            {
                TDropTableSqlStatement dropStmt = (TDropTableSqlStatement)stmt;
                operateInfo operateInfo = new operateInfo();
                operateInfo.objectType = objectType.Table;
                operateInfo.objectUsed = dropStmt.TableName.ToString().Trim();
                operateInfo.usageType = usageType.Drop;
                procedureInfo.operates.Add(operateInfo);
            }
            else if (stmt is TSelectSqlStatement)
            {
                TSelectSqlStatement selectStmt = (TSelectSqlStatement)stmt;
                List<columnInfo> columnInfos = new List<columnInfo>();
                List<tableInfo> tableInfos = new List<tableInfo>();
                tableTokensInStmt(columnInfos, tableInfos, selectStmt);
                LinkedHashMap<tableInfo, List<columnInfo>> columnMap = new LinkedHashMap<tableInfo, List<columnInfo>>();
                for (int i = 0; i < columnInfos.Count; i++)
                {
                    columnInfo column = columnInfos[i];
                    tableInfo table = column.table;
                    if (columnMap.ContainsKey(table))
                    {
                        List<columnInfo> columns = (List<columnInfo>)columnMap[table];
                        bool flag = false;
                        foreach (columnInfo temp in columns)
                        {
                            if (temp.ToString().Equals(column.ToString(), StringComparison.CurrentCultureIgnoreCase))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (!flag)
                        {
                            columns.Add(column);
                        }
                    }
                    else
                    {
                        List<columnInfo> columns = new List<columnInfo>();
                        columnMap[table] = columns;
                        columns.Add(column);
                    }
                }
                for (int i = 0; i < tableInfos.Count; i++)
                {
                    operateInfo operateInfo = new operateInfo();
                    operateInfo.objectType = objectType.Table;
                    operateInfo.objectUsed = tableInfos[i].ToString();
                    if (tableInfos[i].stmt is TSelectSqlStatement && ((TSelectSqlStatement)tableInfos[i].stmt).IntoClause != null)
                    {
                        operateInfo.usageType = usageType.Insert;
                    }
                    else
                    {
                        operateInfo.usageType = usageType.Read;
                    }
                    if (columnMap.ContainsKey(tableInfos[i]))
                    {
                        foreach (columnInfo column in (List<columnInfo>)columnMap[tableInfos[i]])
                        {
                            operateInfo.columns.Add(column.ToString());
                            operateInfo.objectUsed = column.table.ToString();
                        }
                    }
                    procedureInfo.operates.Add(operateInfo);
                }
            }
        }

        private void checkFunction(spInfo spInfo, TSourceTokenList tokenList)
        {
            for (int i = 0; i < tokenList.size(); i++)
            {
                TSourceToken token = tokenList.get(i);
                if (token.DbObjType == EDbObjectType.function)
                {
                    List<TParseTreeNode> list = token.nodesStartFromThisToken;
                    for (int j = 0; j < list.Count; j++)
                    {
                        TParseTreeNode node = (TParseTreeNode)list[j];
                        if (node is TFunctionCall)
                        {
                            builtInFunctionInfo function = new builtInFunctionInfo();
                            function.function = token.astext;
                            function.lineNo = token.lineNo;
                            function.columnNo = token.columnNo;
                            TCustomSqlStatement stmt = token.stmt;
                            if (stmt == null)
                            {
                                bool flag = false;
                                for (int k = token.posinlist - 1; k >= 0; k--)
                                {
                                    TSourceToken before = node.Gsqlparser.sourcetokenlist.get(k);
                                    if (token.nodesStartFromThisToken != null)
                                    {
                                        for (int z = 0; z < before.nodesStartFromThisToken.Count; z++)
                                        {
                                            if (before.nodesStartFromThisToken[z] is TCustomSqlStatement)
                                            {
                                                TCustomSqlStatement tempStmt = (TCustomSqlStatement)before.nodesStartFromThisToken[z];
                                                if (tempStmt.startToken.posinlist <= token.posinlist && tempStmt.endToken.posinlist >= token.posinlist)
                                                {
                                                    stmt = tempStmt;
                                                    flag = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (flag)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (stmt is TInsertSqlStatement)
                            {
                                function.stmtType = usageType.Insert;
                            }
                            else if (stmt is TSelectSqlStatement)
                            {
                                function.stmtType = usageType.Read;
                            }
                            else if (stmt is TUpdateSqlStatement)
                            {
                                function.stmtType = usageType.Update;
                            }
                            else if (stmt is TDeleteSqlStatement)
                            {
                                function.stmtType = usageType.Delete;
                            }
                            else if (stmt is TMssqlDropTable)
                            {
                                function.stmtType = usageType.Drop;
                            }
                            else if (stmt is TDropTableSqlStatement)
                            {
                                function.stmtType = usageType.Drop;
                            }
                            else if (stmt is TMssqlExecute)
                            {
                                function.stmtType = usageType.Exec;
                            }
                            else if (stmt is TMssqlCreateProcedure)
                            {
                                function.stmtType = usageType.Create;
                            }
                            spInfo.functions.Add(function);
                        }
                    }
                }
            }
        }

        private void checkTryCatchClause(procedureInfo procedureInfo, TMssqlCreateProcedure procedure)
        {
            TSourceTokenList tokenList = procedure.sourcetokenlist;
            for (int i = 0; i < tokenList.size(); i++)
            {
                TSourceToken token = tokenList.get(i);
                if (token.tokentype == ETokenType.ttkeyword && token.astext.Trim().Equals("try",StringComparison.OrdinalIgnoreCase))
                {
                    procedureInfo.hasTryCatch = true;
                }
            }
        }

        public virtual string DBObjectRelationsAnalysisResult
        {
            get
            {
                if (relationBuffer.Length == 0 && files != null)
                {
                    foreach (string file in files)
                    {
                        spInfo spInfo = (spInfo)spInfoMap[file];
                        foreach (procedureInfo procedure in spInfo.procedures)
                        {
                            foreach (operateInfo info in procedure.operates)
                            {
                                StringBuilder builder = new StringBuilder();
                                for (int i = 0; i < info.columns.Count; i++)
                                {
                                    builder.Append(info.columns[i]);
                                    if (i < info.columns.Count - 1)
                                    {
                                        builder.Append(",");
                                    }
                                }
                                relationBuffer.Append(spInfo.db)
                                .Append(delimiter)
                                .Append(procedure.name)
                                .Append(delimiter)
                                .Append(procedure.objectType)
                                .Append(delimiter)
                                .Append(info.objectUsed)
                                .Append(delimiter)
                                .Append(info.objectType)
                                .Append(delimiter)
                                .Append(info.usageType)
                                .Append(delimiter)
                                .Append(builder)
                                .AppendLine();
                            }

                        }
                    }
                }
                return relationBuffer.ToString();
            }
        }

        public virtual string TryCatchClauseAnalysisResult
        {
            get
            {
                if (tryCatchBuffer.Length == 0 && files != null)
                {
                    foreach (string file in files)
                    {
                        spInfo spInfo = (spInfo)spInfoMap[file];
                        foreach (procedureInfo procedure in spInfo.procedures)
                        {
                            tryCatchBuffer.Append(System.IO.Path.GetFileName(file)).Append(spInfo.db)
                            .Append(delimiter).Append(procedure.name).Append(delimiter).Append(procedure.hasTryCatch ? "Yes" : "No").AppendLine();
                        }
                    }
                }
                return tryCatchBuffer.ToString();
            }
        }

        public virtual string BuiltInFunctionAnalysisResult
        {
            get
            {
                if (functionBuffer.Length == 0 && files != null)
                {
                    foreach (string file in files)
                    {
                        spInfo spInfo = (spInfo)spInfoMap[file];
                        foreach (builtInFunctionInfo function in spInfo.functions)
                        {
                            functionBuffer.Append(System.IO.Path.GetFileName(file)).Append(delimiter).Append(function.function).Append(delimiter).Append(function.lineNo).Append(delimiter).Append(function.columnNo).Append(delimiter).Append(function.stmtType).AppendLine();
                        }
                    }
                }
                return functionBuffer.ToString();
            }
        }

        public virtual bool CheckBuiltInFunction
        {
            set
            {
                this.checkBuiltInFunction = value;
            }
        }

        public virtual bool CheckDBObjectRelation
        {
            set
            {
                this.checkObjectRelation = value;
            }
        }

        public virtual bool CheckTryCatchClause
        {
            set
            {
                this.checkTryCatchClause_Renamed = value;
            }
        }

        private void tableTokensInStmt(List<columnInfo> columnInfos, List<tableInfo> tableInfos, TCustomSqlStatement stmt)
        {
            for (int i = 0; i < stmt.tables.size(); i++)
            {
                if (stmt.tables.getTable(i).BaseTable)
                {
                    if ((stmt.dbvendor == EDbVendor.dbvmssql) && ((stmt.tables.getTable(i).FullName.Equals("deleted",StringComparison.OrdinalIgnoreCase)) || (stmt.tables.getTable(i).FullName.Equals("inserted",StringComparison.OrdinalIgnoreCase))))
                    {
                        continue;
                    }

                    if (stmt.tables.getTable(i).EffectType == ETableEffectType.tetSelectInto)
                    {
                        continue;
                    }

                    tableInfo tableInfo = new tableInfo();
                    tableInfo.fullName = stmt.tables.getTable(i).FullName;
                    tableInfos.Add(tableInfo);

                    for (int j = 0; j < stmt.tables.getTable(i).LinkedColumns.size(); j++)
                    {

                        columnInfo columnInfo = new columnInfo();
                        columnInfo.table = tableInfo;
                        columnInfo.column = stmt.tables.getTable(i).LinkedColumns.getObjectName(j);
                        columnInfos.Add(columnInfo);
                    }
                }
            }

            if (stmt is TSelectSqlStatement && ((TSelectSqlStatement)stmt).IntoClause != null)
            {
                TExpressionList tables = ((TSelectSqlStatement)stmt).IntoClause.ExprList;
                for (int j = 0; j < tables.size(); j++)
                {
                    tableInfo tableInfo = new tableInfo();
                    tableInfo.fullName = tables.getExpression(j).ToString();
                    tableInfo.stmt = stmt;
                    tableInfos.Add(tableInfo);
                }
            }

            for (int i = 0; i < stmt.Statements.size(); i++)
            {
                tableTokensInStmt(columnInfos, tableInfos, stmt.Statements.get(i));
            }
        }

    }

    internal class builtInFunctionInfo
    {

        public string function;
        public long lineNo, columnNo;
        public usageType stmtType;
    }

    internal class columnInfo
    {

        public tableInfo table;
        public TObjectName column;

        public override string ToString()
        {
            return column == null ? "" : column.ColumnNameOnly.Trim();
        }
    }

    internal enum objectType
    {
        SP,
        Table
    }

    internal class operateInfo
    {

        public string objectUsed;
        public objectType objectType;
        public usageType usageType;
        public List<string> columns = new List<string>();
    }

    internal class procedureInfo
    {

        public string name;
        public objectType objectType;
        public List<operateInfo> operates = new List<operateInfo>();
        public bool hasTryCatch;

        public procedureInfo()
        {
            objectType = objectType.Table;
        }
    }

    internal class spInfo
    {

        public string file;
        public string db;
        public List<procedureInfo> procedures = new List<procedureInfo>();
        public List<builtInFunctionInfo> functions = new List<builtInFunctionInfo>();
    }

    internal class tableInfo
    {

        public string fullName;

        public TCustomSqlStatement stmt;

        public override string ToString()
        {
            return (string.ReferenceEquals(fullName, null) ? "" : fullName.Trim());
        }

    }

    internal enum usageType
    {
        Exec,
        Read,
        Insert,
        Update,
        Create,
        Delete,
        Drop
    }
}