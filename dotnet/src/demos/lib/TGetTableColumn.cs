using System;
using System.Collections.Generic;
using System.Text;


namespace gudusoft.gsqlparser.demos.lib
{


    using EDbObjectType = gudusoft.gsqlparser.EDbObjectType;
    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using ESqlClause = gudusoft.gsqlparser.ESqlClause;
    using ETableEffectType = gudusoft.gsqlparser.ETableEffectType;
    using ETableSource = gudusoft.gsqlparser.ETableSource;
    using IMetaDatabase = gudusoft.gsqlparser.IMetaDatabase;
    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using TTable = gudusoft.gsqlparser.nodes.TTable;
    using TTypeName = gudusoft.gsqlparser.nodes.TTypeName;
    using TStoredProcedureSqlStatement = gudusoft.gsqlparser.stmt.TStoredProcedureSqlStatement;
    //using gettablecolumns;
    using System.IO;
    using gudusoft.gsqlparser.demos.gettablecolumns;



    internal class myMetaDB : IMetaDatabase
    {

        internal string[][] columns = new string[][]
        {
            new string[] {"server","db","DW","AcctInfo_PT","ACCT_ID"},
            new string[] {"server","db","DW","ImSysInfo_BC","ACCT_ID"},
            new string[] {"server","db","DW","AcctInfo_PT","SystemOfRec"},
            new string[] {"server","db","DW","ImSysInfo_BC","SystemOfRec"},
            new string[] {"server","db","DW","AcctInfo_PT","OfficerCode"},
            new string[] {"server","db","DW","ImSysInfo_BC","OpeningDate"}
        };

        public virtual bool checkColumn(string server, string database, string schema, string table, string column)
        {
            bool bServer, bDatabase, bSchema, bTable, bColumn, bRet = false;
            for (int i = 0; i < columns.Length; i++)
            {
                if ((string.ReferenceEquals(server, null)) || (server.Length == 0))
                {
                    bServer = true;
                }
                else
                {
                    bServer = columns[i][0].Equals(server, StringComparison.CurrentCultureIgnoreCase);
                }
                if (!bServer)
                {
                    continue;
                }

                if ((string.ReferenceEquals(database, null)) || (database.Length == 0))
                {
                    bDatabase = true;
                }
                else
                {
                    bDatabase = columns[i][1].Equals(database, StringComparison.CurrentCultureIgnoreCase);
                }
                if (!bDatabase)
                {
                    continue;
                }

                if ((string.ReferenceEquals(schema, null)) || (schema.Length == 0))
                {
                    bSchema = true;
                }
                else
                {
                    bSchema = columns[i][2].Equals(schema, StringComparison.CurrentCultureIgnoreCase);
                }

                if (!bSchema)
                {
                    continue;
                }

                bTable = columns[i][3].Equals(table, StringComparison.CurrentCultureIgnoreCase);
                if (!bTable)
                {
                    continue;
                }

                bColumn = columns[i][4].Equals(column, StringComparison.CurrentCultureIgnoreCase);
                if (!bColumn)
                {
                    continue;
                }

                bRet = true;
                break;

            }

            return bRet;
        }

    }

    internal class TInfoRecord
    {

        public virtual string SPString
        {
            get
            {
                if (SPName == null)
                {
                    return "N/A";
                }
                else
                {
                    return SPName.ToString();
                }
            }
        }

        internal virtual string getTableStr(TTable table)
        {
            string tableName = "";
            if (table.TableType == ETableSource.subquery)
            {
                tableName = "(subquery, alias:" + table.AliasName + ")";
            }
            else
            {
                tableName = table.TableName.ToString();
                if (table.LinkTable != null)
                {
                    tableName = tableName + "(" + table.LinkTable.TableName.ToString() + ")";
                }
                else if (table.CTEName)
                {
                    tableName = tableName + "(CTE)";
                }
            }

            return tableName;
        }

        public virtual string FullColumnName
        {
            get
            {
                if (dbObjectType != EDbObjectType.column)
                {
                    return "";
                }
                string columnName = Column.ColumnNameOnly;

                if (Table != null && Table.ObjectNameReferences.searchColumnReference(Column) != -1)
                {
                    if (getTableStr(Table).Length > 0)
                    {
                        columnName = getTableStr(Table) + "." + columnName;
                    }
                    schemaName = Table.PrefixSchema;
                    if (schemaName.Length > 0)
                    {
                        columnName = schemaName + "." + columnName;
                    }
                    return columnName;
                }

                return Column.ToString();
            }
        }

        public virtual string printMe(bool includingTitle)
        {
            string spTitle = "\nfilename|spname|object type\n";
            string tableTitle = "\nfilename|spname|object type|schema|table|table effect\n";
            string columnTitle = "\nfilename|spname|object type|schema|table|column|location|coordinate|datatype\n";
            string indexTitle = "\nfilename|spname|object type|schema|index|table|column|location|coordinate\n";

            string schemaName = "N/A";
            string tableName = "unknownTable";
            string indexName = "unknownIndex";

            StringBuilder sb = new StringBuilder(1024);
            switch (dbObjectType)
            {
                case EDbObjectType.procedure:
                    if (includingTitle)
                    {
                        sb.Append(spTitle);
                    }
                    sb.Append(FileName + "|" + SPName.ToString() + "|" + dbObjectType);
                    break;
                case EDbObjectType.table:
                    if (includingTitle)
                    {
                        sb.Append(tableTitle);
                    }

                    tableName = getTableStr(Table);
                    schemaName = Table.PrefixSchema;
                    if (schemaName.Length == 0)
                    {
                        schemaName = "N/A";
                    }

                    sb.Append(FileName + "|" + SPString + "|" + dbObjectType + "|" + schemaName + "|" + tableName + "|" + Table.EffectType);
                    break;
                case EDbObjectType.column:
                    if (includingTitle)
                    {
                        sb.Append(columnTitle);
                    }
                    if (Table != null)
                    {
                        //it's an orphan column
                        tableName = getTableStr(Table);
                        schemaName = Table.PrefixSchema;
                        if (schemaName.Length == 0)
                        {
                            schemaName = "N/A";
                        }
                    }
                    else
                    {
                        tableName = "missed";
                    }

                    String cn = "";
                    if  (Column.linkedColumnDefinition != null)
                    {
                        //column in create table, add datatype information as well
                        TTypeName datatype = Column.linkedColumnDefinition.Datatype;
                        cn = datatype.DataTypeName;
                        if (datatype.Length != null)
                        {
                            cn = cn + ":" + datatype.Length.ToString();
                        }
                        else if (datatype.Precision != null)
                        {
                            cn = cn + ":" + datatype.Precision.ToString();
                            if (datatype.Scale != null)
                            {
                                cn = cn + ":" + datatype.Scale.ToString();
                            }
                        }
                    }

                    sb.Append(FileName + "|" + SPString + "|" + dbObjectType + "|" + schemaName + "|" + tableName + "|" + Column.ToString() + "|" + Column.Location + "|(" + Column.coordinate()  + ")" + "|" + cn);
                    break;
                case EDbObjectType.index:
                    if (includingTitle)
                    {
                        sb.Append(indexTitle);
                    }
                    if (Table != null)
                    {
                        schemaName = Table.PrefixSchema;
                        if (schemaName.Length == 0)
                        {
                            schemaName = "N/A";
                        }
                        tableName = Table.TableName.ToString();
                    }
                    if (Index != null)
                    {
                        indexName = Index.ToString();
                    }
                    sb.Append(FileName + "|" + SPString + "|" + dbObjectType + "|" + schemaName + "|" + indexName + "|" + tableName + "|" + Column.ToString() + "|" + Column.Location + "|(" + Column.coordinate() + ")");
                    break;
            }

            return sb.ToString();
        }

        private TObjectName index;

        public virtual TObjectName Index
        {
            set
            {
                this.index = value;
            }
            get
            {

                return index;
            }
        }


        private EDbObjectType dbObjectType;

        public virtual EDbObjectType DbObjectType
        {
            set
            {
                this.dbObjectType = value;
            }
            get
            {

                return dbObjectType;
            }
        }


        public TInfoRecord()
        {

        }

        public TInfoRecord(EDbObjectType dbObjectType)
        {
            this.dbObjectType = dbObjectType;
        }

        public TInfoRecord(TTable table)
        {
            this.table = table;
            this.dbObjectType = EDbObjectType.table;
        }

        public TInfoRecord(TInfoRecord clone, EDbObjectType dbObjectType)
        {
            this.fileName = clone.fileName;
            this.SPName_Renamed = clone.SPName_Renamed;
            this.table = clone.table;
            this.column = clone.column;
            this.dbObjectType = dbObjectType;
        }

        private string fileName = "N/A";
        //JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
        private TObjectName SPName_Renamed; //stored procedure name

        public virtual string FileName
        {
            set
            {
                this.fileName = value;
            }
            get
            {

                return fileName;
            }
        }

        public virtual TObjectName SPName
        {
            set
            {
                this.SPName_Renamed = value;
            }
            get
            {
                return SPName_Renamed;
            }
        }

        public virtual string SchemaName
        {
            set
            {
                this.schemaName = value;
            }
            get
            {
                return schemaName;
            }
        }




        private string schemaName;

        //    public String tableName;
        //    public String columnName;

        private TTable table;

        public virtual TTable Table
        {
            set
            {
                this.table = value;
            }
            get
            {

                return table;
            }
        }

        public virtual TObjectName Column
        {
            set
            {
                this.column = value;
            }
            get
            {
                return column;
            }
        }



        private TObjectName column;
    }

    public class TGetTableColumn
    {
        private EDbVendor dbVendor;
        private string queryStr;
        private TGSqlParser sqlParser;
        private IMetaDatabase metaDatabase = null;

        public virtual IMetaDatabase MetaDatabase
        {
            set
            {
                this.metaDatabase = value;
                sqlParser.MetaDatabase = value;
            }
        }

        private StringBuilder functionlist, schemalist, triggerlist, sequencelist, databaselist;

        public StringBuilder infos;

        public StringBuilder outList;

        private List<TInfoRecord> infoList;

        private List<string> fieldlist, tablelist, indexList;

        private StringBuilder tableColumnList;

        private string newline = "\n";

        private string sqlFileName = "N/A";

        public bool isConsole;
        public bool listStarColumn;

        public bool showTableEffect;
        public bool showColumnLocation;
        public bool showDatatype;
        public bool showIndex;
        public bool linkOrphanColumnToFirstTable;
        public bool showDetail = false;
        public bool showSummary = true;
        public bool showTreeStructure = false;
        public bool showBySQLClause = false;
        public bool showJoin = false;

        private Stack<TStoredProcedureSqlStatement> spList;


        internal string dotChar = ".";
        public TGetTableColumn(EDbVendor pDBVendor)
        {
            dbVendor = pDBVendor;

            sqlParser = new TGSqlParser(dbVendor);
            //sqlParser.setMetaDatabase(new myMetaDB());

            tablelist = new List<string>();
            fieldlist = new List<string>();
            indexList = new List<string>();
            infoList = new List<TInfoRecord>();

            spList = new Stack<TStoredProcedureSqlStatement>();

            infos = new StringBuilder();
            functionlist = new StringBuilder();
            schemalist = new StringBuilder();
            triggerlist = new StringBuilder();
            sequencelist = new StringBuilder();
            databaselist = new StringBuilder();
            tableColumnList = new StringBuilder();
            outList = new StringBuilder();

            isConsole = true;
            listStarColumn = false;
            showTreeStructure = false;
            showTableEffect = false;
            showColumnLocation = false;
            linkOrphanColumnToFirstTable = true;
            showDatatype = false;
            showIndex = false;
        }

        public virtual void runText(string pQuery)
        {
            run(pQuery, false, true);
        }

        public virtual void runFile(FileInfo sqlFile)
        {
            List<FileInfo> files = new List<FileInfo>();
            getFiles(sqlFile, files);
            for (int i = 0; i < files.Count; i++)
            {
                this.sqlFileName = files[i].Name;
                bool showTitle = i == 0;
                run(files[i].FullName, true, showTitle);
            }
        }

        private static void getFiles(FileInfo sqlFiles, List<FileInfo> files)
        {
            try
            {
                if (!sqlFiles.Attributes.HasFlag(FileAttributes.Directory))
                {
                    if (sqlFiles.FullName.ToLower().EndsWith(".sql") || sqlFiles.FullName.ToLower().EndsWith(".txt"))
                    {
                        files.Add(sqlFiles);
                    }
                }

                if (sqlFiles.Attributes.HasFlag(FileAttributes.Directory))
                {
                    FileInfo[] children = new DirectoryInfo(sqlFiles.FullName).GetFiles();
                    for (int i = 0; i < children.Length; i++)
                    {
                        getFiles(children[i], files);
                    }

                    DirectoryInfo[] dirs = new DirectoryInfo(sqlFiles.FullName).GetDirectories();
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        getFiles(new FileInfo(dirs[i].FullName), files);
                    }
                }
            }
            catch (Exception) { }
        }

        internal virtual string numberOfSpace(int pNum)
        {
            string ret = "";
            for (int i = 0; i < pNum; i++)
            {
                ret = ret + " ";
            }
            return ret;
        }

        public virtual StringBuilder Infos
        {
            get
            {
                return infos;
            }
        }


        protected internal virtual void run(string pQuery, bool isFile, bool showTitle)
        {
            queryStr = pQuery;
            if (isFile)
            {
                sqlParser.sqlfilename = pQuery;
            }
            else
            {
                sqlParser.sqltext = pQuery;
            }
            int iRet = sqlParser.parse();
            if (iRet != 0)
            {
                if (isConsole)
                {
                    Console.WriteLine(sqlParser.Errormessage);
                }
                else
                {
                    throw new Exception(sqlParser.Errormessage);
                }
                return;
            }

            outList.Length = 0;
            tablelist.Clear();
            fieldlist.Clear();
            indexList.Clear();

            for (int i = 0; i < sqlParser.sqlstatements.size(); i++)
            {
                analyzeStmt(sqlParser.sqlstatements.get(i), 0);
            }


            // print detailed info
            if (showDetail)
            {
                bool includingTitle = true;
                for (int i = 0; i < infoList.Count; i++)
                {
                    if (i > 0)
                    {
                        includingTitle = !(infoList[i].DbObjectType == infoList[i - 1].DbObjectType);
                    }
                    outputResult(infoList[i].printMe(includingTitle));
                }
            }

            // print summary info
            if (showSummary)
            {
                removeDuplicateAndSort(tablelist);
                removeDuplicateAndSort(fieldlist);
                removeDuplicateAndSort(indexList);

                if (isFile)
                { 
                    outputResult("File:" + sqlFileName);
                }
                printArray("Tables:", tablelist);
                printArray("Fields:", fieldlist);
                if (showIndex && (indexList.Count > 0))
                {
                    printArray("Indexs:", indexList);
                }
                outputResult("");
            }

            // print tree structure
            if (showTreeStructure)
            {
                if (isFile)
                {
                    outputResult("File:" + sqlFileName);
                }
                outputResult(infos.ToString());
            }

            if (showBySQLClause)
            {
                if (isFile)
                {
                    outputResult("File:" + sqlFileName);
                }
                List<ETableEffectType> tableEffectTypes = new List<ETableEffectType>();
                List<ESqlClause> columnClauses = new List<ESqlClause>();

                for (int i = 0; i < infoList.Count; i++)
                {
                    if (infoList[i].DbObjectType == EDbObjectType.table)
                    {
                        if (!tableEffectTypes.Contains(infoList[i].Table.EffectType))
                        {
                            tableEffectTypes.Add(infoList[i].Table.EffectType);
                        }
                    }
                }
                outputResult("Tables:");
                for (int j = 0; j < tableEffectTypes.Count; j++)
                {
                    outputResult("\t" + tableEffectTypes[j].ToString());

                    for (int i = 0; i < infoList.Count; i++)
                    {
                        if (infoList[i].DbObjectType == EDbObjectType.table)
                        {
                            TTable lcTable = infoList[i].Table;
                            if (lcTable.EffectType == tableEffectTypes[j] && lcTable.TableName != null && lcTable.TableName.coordinate()!=null)
                            {
                                outputResult("\t\t" + lcTable.ToString() + "(" + lcTable.TableName.coordinate() + ")");
                            }
                        }
                    }
                }

                // column
                for (int i = 0; i < infoList.Count; i++)
                {
                    if (infoList[i].DbObjectType == EDbObjectType.column)
                    {
                        if (!columnClauses.Contains(infoList[i].Column.Location))
                        {
                            columnClauses.Add(infoList[i].Column.Location);
                        }
                    }
                }
                outputResult("Columns:");
                for (int j = 0; j < columnClauses.Count; j++)
                {
                    outputResult("\t" + columnClauses[j].ToString());

                    for (int i = 0; i < infoList.Count; i++)
                    {
                        if (infoList[i].DbObjectType == EDbObjectType.column)
                        {
                            TObjectName lcColumn = infoList[i].Column;
                            if (lcColumn.Location == columnClauses[j])
                            {
                                outputResult("\t\t" + infoList[i].FullColumnName + "(" + lcColumn.coordinate() + ")");
                            }
                        }
                    }
                }

                outputResult("");

            }

            if (showJoin)
            {
                joinRelationAnalyze analysis = new joinRelationAnalyze(sqlParser, showColumnLocation, showTitle);
                outputResult(analysis.AnalysisResult, false);
            }

            infoList.Clear();
            //  System.out.println("Fields:"+newline+fieldlist.toString());
        }

        private void outputResult(string result, bool appendNewLine)
        {
            if (isConsole)
            {
                if (appendNewLine)
                {
                    Console.WriteLine(result);
                }
                else {
                    Console.Write(result);
                }
            }
            else
            {
                //if(outList.length()>0)
                //	outList.append(newline);
                outList.Append(result);
                if (appendNewLine) outList.AppendLine();
            }
        }

        private void outputResult(string result)
        {
            if (isConsole)
            {
                Console.WriteLine(result);
            }
            else
            {
                //if(outList.length()>0)
                //	outList.append(newline);
                outList.Append(result).Append(newline);
            }
        }

        internal virtual void printArray(string pTitle, List<string> pList)
        {
            outputResult(pTitle);
            object[] str = pList.ToArray();
            for (int i = 0; i < str.Length; i++)
            {
                outputResult(str[i].ToString());
            }
        }


        internal virtual void removeDuplicateAndSort(List<string> pList)
        {
            pList.Sort(new SortIgnoreCase());

            for (int i = 0; i < pList.Count - 1; i++)
            {
                for (int j = pList.Count - 1; j > i; j--)
                {
                    if (pList[j].Equals((pList[i]), StringComparison.CurrentCultureIgnoreCase))
                    {
                        pList.RemoveAt(j);
                    }
                }
            }
        }

        protected internal virtual void analyzeStmt(TCustomSqlStatement stmt, int pNest)
        {
            TTable lcTable = null;
            TObjectName lcColumn = null;
            string tn = "", cn = "";

            if (stmt is TStoredProcedureSqlStatement)
            {
                spList.Push((TStoredProcedureSqlStatement)stmt);
                TInfoRecord spRecord = new TInfoRecord(EDbObjectType.procedure);
                spRecord.SPName = spList.Peek().StoredProcedureName;
            }
            //System.out.println( numberOfSpace(pNest)+ stmt.sqlstatementtype);
            infos.Append(numberOfSpace(pNest) + stmt.sqlstatementtype + newline);

            for (int i = 0; i < stmt.tables.size(); i++)
            {
                //if  (stmt.tables.getTable(i).isBaseTable())
                //{
                lcTable = stmt.tables.getTable(i);
                TInfoRecord tableRecord = new TInfoRecord(lcTable);
                tableRecord.FileName = this.sqlFileName;
                if (spList.Count > 0)
                {
                    tableRecord.SPName = spList.Peek().StoredProcedureName;
                }
                infoList.Add(tableRecord);

                if (lcTable.TableType == ETableSource.subquery)
                {
                    tn = "(subquery, alias:" + lcTable.AliasName + ")";
                }
                else
                {
                    tn = lcTable.TableName.ToString();
                    if (lcTable.LinkTable != null)
                    {
                        tn = tn + "(" + lcTable.LinkTable.TableName.ToString() + ")";
                    }
                    else if (lcTable.CTEName)
                    {
                        tn = tn + "(CTE)";
                    }
                }
                //System.out.println(numberOfSpace(pNest+1)+tn.getName());
                if ((showTableEffect) && (lcTable.BaseTable))
                {
                    infos.Append(numberOfSpace(pNest + 1) + tn + "(" + lcTable.EffectType + ")" + newline);
                }
                else
                {
                    infos.Append(numberOfSpace(pNest + 1) + tn + newline);
                }

                tableColumnList.Append("," + tn);

                if (!((lcTable.TableType == ETableSource.subquery) || (lcTable.CTEName)))
                {
                    if (lcTable.LinkTable != null)
                    {
                        // tablelist.append(lcTable.getLinkTable().toString()+newline);
                        tablelist.Add(lcTable.LinkTable.ToString());
                    }
                    else
                    {
                        // tablelist.append(lcTable.toString()+newline);
                        tablelist.Add(lcTable.TableName.ToString());
                    }
                }

                for (int j = 0; j < stmt.tables.getTable(i).LinkedColumns.size(); j++)
                {
                    lcColumn = stmt.tables.getTable(i).LinkedColumns.getObjectName(j);
                    TInfoRecord columnRecord = new TInfoRecord(tableRecord, EDbObjectType.column);
                    columnRecord.Column = lcColumn;
                    infoList.Add(columnRecord);
                    cn = lcColumn.ColumnNameOnly;

                    //System.out.println(numberOfSpace(pNest+2)+cn.getColumnNameOnly());
                    if (showColumnLocation)
                    {
                        string posStr = "";
                        //                        if ( lcColumn.getColumnToken() != null) {
                        //                            TSourceToken lcStartToken = lcColumn.getColumnToken();
                        //                            posStr ="("+ lcStartToken.lineNo+","+lcStartToken.columnNo+ ")";
                        //                        }
                        infos.Append(numberOfSpace(pNest + 3) + lcColumn.ColumnNameOnly + posStr + "(" + lcColumn.Location + ")" + newline);
                    }
                    else
                    {
                        infos.Append(numberOfSpace(pNest + 3) + lcColumn.ColumnNameOnly + newline);
                    }

                    if (!((lcTable.TableType == ETableSource.subquery) || (lcTable.CTEName)))
                    {
                        if ((listStarColumn) || (!(lcColumn.ColumnNameOnly.Equals("*"))))
                        {
                            if (lcTable.LinkTable != null)
                            {
                                fieldlist.Add(lcTable.LinkTable.TableName + dotChar + cn);
                            }
                            else
                            {
                                fieldlist.Add(tn + dotChar + cn);
                            }
                        }
                    }
                    tableColumnList.Append("," + tn + dotChar + cn);
                }

                //}
            }

            if (stmt.OrphanColumns.size() > 0)
            {
                infos.Append(numberOfSpace(pNest + 1) + " orphan columns:" + newline);
                string oc = "";
                for (int k = 0; k < stmt.OrphanColumns.size(); k++)
                {
                    TInfoRecord columnRecord = new TInfoRecord(EDbObjectType.column);
                    columnRecord.Column = stmt.OrphanColumns.getObjectName(k);
                    columnRecord.FileName = this.sqlFileName;
                    infoList.Add(columnRecord);

                    oc = stmt.OrphanColumns.getObjectName(k).ColumnNameOnly; // stmt.getOrphanColumns().getObjectName(k).toString();
                    if (showColumnLocation)
                    {
                        infos.Append(numberOfSpace(pNest + 3) + oc + "(" + stmt.OrphanColumns.getObjectName(k).Location + ")" + newline);
                    }
                    else
                    {
                        infos.Append(numberOfSpace(pNest + 3) + oc + newline);
                    }

                    if ((linkOrphanColumnToFirstTable) && (stmt.FirstPhysicalTable != null))
                    {
                        if ((listStarColumn) || (!(oc.Equals("*", StringComparison.CurrentCultureIgnoreCase))))
                        {
                            fieldlist.Add(stmt.FirstPhysicalTable.ToString() + dotChar + oc);
                        }
                        columnRecord.Table = stmt.FirstPhysicalTable;
                    }
                    else
                    {
                        fieldlist.Add("missed" + dotChar + oc);
                    }
                    tableColumnList.Append(",missed" + dotChar + oc + newline);
                }
            }

            for (int i = 0; i < stmt.Statements.size(); i++)
            {
                analyzeStmt(stmt.Statements.get(i), pNest + 1);
            }

            if (stmt is TStoredProcedureSqlStatement)
            {
                spList.Pop();
            }

        }
    }

    internal class SortIgnoreCase : IComparer<object>
    {
        public virtual int Compare(object o1, object o2)
        {
            string s1 = (string)o1;
            string s2 = (string)o2;
            return s1.ToLower().CompareTo(s2.ToLower());
        }
    }




}