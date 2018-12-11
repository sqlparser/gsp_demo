using System;
using System.Collections.Generic;
using System.Text;

namespace gudusoft.gsqlparser.demos.tableColumnRename
{

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using IMetaDatabase = gudusoft.gsqlparser.IMetaDatabase;
    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using TSourceToken = gudusoft.gsqlparser.TSourceToken;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using TTable = gudusoft.gsqlparser.nodes.TTable;


    public class tableColumnRename
    {
        internal TGSqlParser sqlparser;
        public string msg;
        public int renamedObjectsNum = 0;
        private string sourceTable, targetTable, sourceColumn, targetColumn, sourceSchema, targetSchema;
        private IDictionary<string, IList<string>> metaInfo;

        public virtual string ModifiedText
        {
            get
            {
                StringBuilder sb = new StringBuilder(1024);
                for (int i = 0; i < sqlparser.sourcetokenlist.size(); i++)
                {
                    sb.Append(sqlparser.sourcetokenlist.get(i).astext);
                }

                return sb.ToString();
            }
        }

        private class metaDB : IMetaDatabase
        {
            private readonly tableColumnRename outerInstance;

            public metaDB(tableColumnRename outerInstance)
            {
                this.outerInstance = outerInstance;
            }


            public bool checkColumn(string pServer, string pDatabase, string pSchema, string pTable, string pColumn)
            {
                string tablePrefix = pTable;
                if (!string.ReferenceEquals(pSchema, null) && pSchema.Length > 0)
                {
                    tablePrefix = pSchema + "." + tablePrefix;
                }
                if (!string.ReferenceEquals(pDatabase, null) && pDatabase.Length > 0)
                {
                    tablePrefix = pDatabase + "." + tablePrefix;
                }
                if (!string.ReferenceEquals(pServer, null) && pServer.Length > 0)
                {
                    tablePrefix = pServer + "." + tablePrefix;
                }
                return outerInstance.metaInfo.ContainsKey(tablePrefix.Trim().ToLower()) && outerInstance.metaInfo[tablePrefix.ToLower()].Contains(pColumn.Trim().ToLower());
            }
        }

        public tableColumnRename(EDbVendor vendor, string sqltext, IDictionary<string, IList<string>> metaInfo)
        {
            sqlparser = new TGSqlParser(vendor);
            sqlparser.sqltext = sqltext;
            this.metaInfo = metaInfo;
            if (metaInfo != null)
            {
                sqlparser.MetaDatabase = new metaDB(this);
            }
        }

        public virtual int renameColumn(string sourceColumn, string targetColumn)
        {
            string[] names = sourceColumn.Split(new char[] { '.' });
            if (names.Length == 2)
            {
                this.sourceSchema = null;
                this.sourceTable = names[0];
                this.sourceColumn = names[1];
            }
            else if (names.Length == 3)
            {
                this.sourceSchema = names[0];
                this.sourceTable = names[1];
                this.sourceColumn = names[2];
            }
            else
            {
                this.msg = "source column name must in syntax like this: schema.tablename.column or tablename.column";
                return -1;
            }

            this.targetColumn = targetColumn;
            names = targetColumn.Split(new char[] { '.' });
            if (names.Length != 1)
            {
                this.msg = "target column name must in syntax like this: column";
                return -1;
            }

            int ret = sqlparser.parse();
            if (ret != 0)
            {
                msg = "syntax error: " + sqlparser.Errormessage;
                return -1;
            }

            for (int i = 0; i < sqlparser.sqlstatements.size(); i++)
            {
                TCustomSqlStatement sql = sqlparser.sqlstatements.get(i);
                modifyColumnName(sql);
            }

            this.msg = "renamed column occurs:" + this.renamedObjectsNum;
            return renamedObjectsNum;
        }

        private void modifyTableName(TCustomSqlStatement stmt)
        {

            for (int k = 0; k < stmt.tables.size(); k++)
            {
                TTable table = stmt.tables.getTable(k);

                bool isfound = false;
                for (int m = 0; m < table.LinkedColumns.size(); m++)
                {
                    TObjectName column = table.LinkedColumns.getObjectName(m);
                    isfound = false;
                    if (column.TableString != null && column.TableString.Equals(this.sourceTable, StringComparison.OrdinalIgnoreCase))
                    {
                        isfound = true;
                        if (!string.ReferenceEquals(this.sourceSchema, null))
                        { // check schema of this
                          // table
                            if (column.SchemaString != null)
                            {
                                isfound = this.sourceSchema.Equals(column.SchemaString, StringComparison.OrdinalIgnoreCase);
                            }
                            else
                            {
                                isfound = false;
                            }
                        }
                    }

                    if (isfound)
                    {
                        // rename this table
                        TSourceToken st = column.ColumnToken;
                        if (column.TableString != null)
                        {
                            column.TableToken.String = this.targetTable;
                        }
                        else
                        {
                            st.String = this.targetTable + "." + st.astext;
                        }

                        if (!string.ReferenceEquals(this.targetSchema, null))
                        {
                            if (column.SchemaString != null)
                            {
                                column.SchemaToken.String = this.targetSchema;
                            }
                            else if (column.TableString != null)
                            {
                                column.TableToken.String = this.targetSchema + "." + column.TableString;
                            }
                            else
                            {
                                st.String = this.targetSchema + "." + st.astext;
                            }
                        }

                        this.renamedObjectsNum++;
                    }
                }

                if (table.TableName != null && table.TableName.TableString != null && table.TableName.TableString.Equals(this.sourceTable, StringComparison.OrdinalIgnoreCase))
                {
                    isfound = true;
                    if (!string.ReferenceEquals(this.sourceSchema, null))
                    { // check schema of this table
                        if (table.TableName.SchemaString != null)
                        {
                            isfound = this.sourceSchema.Equals(table.TableName.SchemaString, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            isfound = false;
                        }
                    }
                }

                if (isfound)
                {
                    if (table.TableName != null)
                    {
                        // rename this table
                        TSourceToken st = table.TableName.TableToken;
                        st.String = this.targetTable;
                        if (!string.ReferenceEquals(this.targetSchema, null))
                        {
                            if (table.PrefixSchema != null)
                            {
                                table.TableName.SchemaToken.String = this.targetSchema;
                            }
                            else
                            {
                                st.String = this.targetSchema + "." + st.astext;
                            }
                        }
                        this.renamedObjectsNum++;
                    }
                }
            }

            for (int j = 0; j < stmt.Statements.size(); j++)
            {
                modifyTableName(stmt.Statements.get(j));
            }
        }

        private void modifyColumnName(TCustomSqlStatement stmt)
        {

            for (int k = 0; k < stmt.tables.size(); k++)
            {
                TTable table = stmt.tables.getTable(k);

                if (table.TableName == null)
                {
                    continue;
                }

                bool isThisTable = true;

                isThisTable = table.TableName.TableString.Equals(this.sourceTable, StringComparison.OrdinalIgnoreCase);
                if (!isThisTable)
                {
                    continue;
                }

                if (!string.ReferenceEquals(this.sourceSchema, null) && table.TableName != null)
                {
                    if (table.TableName.SchemaString != null)
                    {
                        isThisTable = table.TableName.SchemaString.Equals(this.sourceSchema, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        isThisTable = false;
                    }
                }

                if (!isThisTable)
                {
                    continue;
                }

                for (int m = 0; m < table.LinkedColumns.size(); m++)
                {
                    TObjectName column = table.LinkedColumns.getObjectName(m);
                    if (column.ColumnToken != null)
                    {
                        if (column.ColumnNameOnly.Equals(this.sourceColumn, StringComparison.OrdinalIgnoreCase))
                        {
                            column.ColumnToken.String = this.targetColumn;
                            this.renamedObjectsNum++;
                        }
                    }
                }
            }

            for (int j = 0; j < stmt.Statements.size(); j++)
            {
                modifyColumnName(stmt.Statements.get(j));
            }
        }

        public virtual int renameTable(string sourceTable, string targetTable)
        {
            this.renamedObjectsNum = 0;
            string[] names = sourceTable.Split(new char[] { '.' });
            if (names.Length == 1)
            {
                this.sourceTable = sourceTable;
                this.sourceSchema = null;
            }
            else if (names.Length == 2)
            {
                this.sourceSchema = names[0];
                this.sourceTable = names[1];
            }
            else
            {
                this.msg = "source table name must in syntax like this: schema.tablename, or tablename";
                return -1;
            }

            names = targetTable.Split(new char[] { '.' });

            if (names.Length == 1)
            {
                this.targetTable = targetTable;
                this.targetSchema = null;
            }
            else if (names.Length == 2)
            {
                this.targetSchema = names[0];
                this.targetTable = names[1];
            }
            else
            {
                this.msg = "target table name must in syntax like this: schema.tablename, or tablename";
                return -1;
            }

            int ret = sqlparser.parse();
            if (ret != 0)
            {
                msg = "syntax error: " + sqlparser.Errormessage;
                return -1;
            }

            for (int i = 0; i < sqlparser.sqlstatements.size(); i++)
            {
                TCustomSqlStatement sql = sqlparser.sqlstatements.get(i);
                modifyTableName(sql);
            }

            this.msg = "renamed table occurs:" + this.renamedObjectsNum;

            return renamedObjectsNum;

        }

        public static void Main(string[] args)
        {
            string text = "CREATE PROCEDURE [dbo].[Testprocedure_2]\n"
                        + "        @BusinessID NVARCHAR(100)\n"
                        + " AS\n" + " BEGIN\n"
                        + "   SET NOCOUNT  ON;\n"
                        + "   SELECT dbo.tb_Rentals.*,\n"
                        + "          MinimalRentalID,\n"
                        + "          SEA.Name,\n"
                        + "          SEA.BeginDay,\n"
                        + "          SEA.EndDay,\n"
                        + "          dbo.tb_RentalTypes.Name AS TypeName\n"
                        + "   FROM   dbo.tb_Rentals,\n"
                        + "          dbo.tb_Seasons SEA,\n"
                        + "          dbo.tb_RentalTypes,\n"
                        + "          dbo.tb_RentalToSeason\n"
                        + "   WHERE  dbo.tb_Rentals.BusinessID_XXX = SEA.BusinessID \n"
                        + "          AND dbo.tb_Rentals.RentalTypeID = dbo.tb_RentalTypes.RentalTypeID\n"
                        + "          AND dbo.tb_RentalToSeason.RentalID = dbo.tb_Rentals.RentalID\n"
                        + "          AND dbo.tb_RentalToSeason.SeasonID = SEA.SeasonID\n"
                        + "          AND dbo.tb_Rentals.BusinessID = @BusinessID \n"
                        + "          AND @BusinessID IN (SELECT DISTINCT dbo.tb_Rentals.BusinessID_XXX\n"
                        + "                              FROM   dbo.tb_Rentals\n"
                        + "                              WHERE  dbo.tb_Rentals.BusinessID = @BusinessID)\n"
                        + " END";
            IDictionary<string, IList<string>> metaInfo = new Dictionary<string, IList<string>>();
            metaInfo["dbo.tb_Seasons".ToLower()] = new List<string>(new string[] { "MinimalRentalID".ToLower() });
            tableColumnRename ro = new tableColumnRename(EDbVendor.dbvmssql, text, metaInfo);
            int ret = ro.renameColumn("dbo.tb_Seasons.MinimalRentalID", "MinimalRentalID_xx");
            Console.WriteLine("Message: " + ro.msg);
            if (ret > 0)
            {
                Console.WriteLine("Result: ");
                Console.WriteLine(ro.ModifiedText);
            }
        }
    }
}