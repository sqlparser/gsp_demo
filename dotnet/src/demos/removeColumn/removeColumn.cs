using System;
using System.Collections.Generic;
using System.Text;

namespace gudusoft.gsqlparser.demos.gettablecolumns
{

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using IMetaDatabase = gudusoft.gsqlparser.IMetaDatabase;
    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using TSourceToken = gudusoft.gsqlparser.TSourceToken;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using TTable = gudusoft.gsqlparser.nodes.TTable;
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.nodes;
    using gudusoft.gsqlparser.stmt;
    using gudusoft.gsqlparser.demos.util;
    using System.IO;

    public class removeColumn
    {
        internal TGSqlParser sqlparser;
        public string msg;
        public int renamedObjectsNum = 0;
        private string sourceTable, sourceColumn, sourceSchema;

        public virtual string ModifiedText
        {
            get
            {
                StringBuilder sb = new StringBuilder(1024);

                for (int i = 0; i < sqlparser.sqlstatements.size(); i++)
                {
                    TCustomSqlStatement sql = sqlparser.sqlstatements.get(i);
                    if (sql.ToScript() != null)
                    {
                        sb.Append(sql.ToScript());
                        sb.Append(";");
                        if (sqlparser.sqlstatements.Count > 1 && i < sqlparser.sqlstatements.Count - 1)
                        {
                            sb.AppendLine();
                            sb.AppendLine();
                        }
                    }
                }

                return sb.ToString();
            }
        }

        public removeColumn(EDbVendor vendor, string sqltext)
        {
            sqlparser = new TGSqlParser(vendor);
            sqlparser.sqltext = sqltext;
        }

        public virtual int deleteColumn(string sourceColumn)
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
                this.msg = "removed column name must in syntax like this: schema.tablename.column or tablename.column";
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
                deleteColumn(sql);
            }

            this.msg = "removed column occurs:" + this.renamedObjectsNum;
            return renamedObjectsNum;
        }

        private void deleteColumn(TCustomSqlStatement stmt)
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
                            removeObjectFromStatement(stmt, column);
                            this.renamedObjectsNum++;
                        }
                    }
                }
            }

            for (int j = 0; j < stmt.Statements.size(); j++)
            {
                deleteColumn(stmt.Statements.get(j));
            }
        }

        private void removeObjectFromStatement(TCustomSqlStatement stmt, TObjectName column)
        {
            if (column.Location == ESqlClause.resultColumn)
            {
                removeObjectFromResultColumn(stmt, column);
            }
            else if (column.Location == ESqlClause.where)
            {
                removeObjectFromWhereCondition(stmt, column);
            }
            else if (column.Location == ESqlClause.orderby)
            {
                removeObjectFromOrderBy(stmt, column);
            }
            else if (column.Location == ESqlClause.groupby)
            {
                removeObjectFromGroupBy(stmt, column);
            }
            else if (column.Location == ESqlClause.having)
            {
                removeObjectFromHavingClause(stmt, column);
            }
            else if (column.Location == ESqlClause.set)
            {
                removeObjectFromSetClause(stmt, column);
            }
            else
            {
                Console.WriteLine("Not yet implements removing column from " + column.Location);
            }
        }

        private void removeObjectFromSetClause(TCustomSqlStatement stmt, TObjectName column)
        {
            if (stmt is TUpdateSqlStatement)
            {
                TUpdateSqlStatement update = (TUpdateSqlStatement)stmt;
                if (update.ResultColumnList != null)
                {
                    for (int i = 0; i < update.ResultColumnList.Count; i++)
                    {
                        TResultColumn resultcolumn = update.ResultColumnList.getResultColumn(i);
                        if (resultcolumn.startToken.posinlist <= column.startToken.posinlist && resultcolumn.endToken.posinlist >= column.endToken.posinlist)
                        {
                            update.ResultColumnList.removeElementAt(i);
                            break;
                        }
                    }
                }
            }
        }

        private void removeObjectFromHavingClause(TCustomSqlStatement stmt, TObjectName column)
        {
            if (!(stmt is TSelectSqlStatement))
            {
                return;
            }

            TSelectSqlStatement select = (TSelectSqlStatement)stmt;
            TGroupBy groupBy = select.GroupByClause;
            if (groupBy == null)
            {
                return;
            }

            TExpression expression = groupBy.HavingClause;
            removeObjectFromExpression(expression, column);
            if (expression.ToScript().Trim().Length == 0)
            {
                groupBy.HavingClause = null;
            }
        }

        internal class exprVisitor : IExpressionVisitor
        {
            private TObjectName column;

            public exprVisitor(TObjectName column)
            {
                this.column = column;
            }

            public virtual bool exprVisit(TParseTreeNode pNode, bool isLeafNode)
            {
                if (pNode.startToken == column.startToken && pNode.endToken == column.endToken)
                {
                    if (pNode is TExpression)
                    {
                        ((TExpression)pNode).remove();
                    }
                }
                return true;
            }
        }
        private void removeObjectFromExpression(TExpression expression, TObjectName column)
        {
            expression.postOrderTraverse(new exprVisitor(column));
        }

        private void removeObjectFromResultColumn(TCustomSqlStatement stmt, TObjectName column)
        {
            if (stmt.ResultColumnList != null)
            {
                for (int i = 0; i < stmt.ResultColumnList.Count; i++)
                {
                    TResultColumn resultSetColumn = stmt.ResultColumnList.getResultColumn(i);
                    TExpression expression = resultSetColumn.Expr;
                    switch (expression.ExpressionType)
                    {
                        case EExpressionType.simple_object_name_t:
                            if (column.startToken == expression.startToken && column.endToken == expression.endToken)
                            {
                                stmt.ResultColumnList.removeElementAt(i);
                                return;
                            }
                            break;
                    }
                }
            }

            if (stmt is TInsertSqlStatement)
            {
                TInsertSqlStatement insert = (TInsertSqlStatement)stmt;
                if (insert.ColumnList == null)
                    return;
                for (int i = 0; i < insert.ColumnList.Count; i++)
                {
                    TObjectName insertColumn = insert.ColumnList.getObjectName(i);

                    if (column.startToken == insertColumn.startToken && column.endToken == insertColumn.endToken)
                    {
                        if (insert.Values != null)
                        {
                            for (int j = 0; j < insert.Values.Count; j++)
                            {
                                TMultiTarget target = insert.Values[j];
                                if (target.ColumnList != null && target.ColumnList.Count == insert.ColumnList.Count)
                                {
                                    target.ColumnList.removeElementAt(i);
                                }
                            }
                        }

                        insert.ColumnList.removeElementAt(i);
                        return;
                    }
                }
            }
        }

        private void removeObjectFromWhereCondition(TCustomSqlStatement stmt, TObjectName column)
        {
            TWhereClause where = stmt.WhereClause;
            if (where == null)
                return;
            TExpression condition = where.Condition;
            removeObjectFromExpression(condition, column);
            if (where.Condition.ToScript().Trim().Length == 0)
            {
                stmt.WhereClause = null;
            }
        }

        private void removeObjectFromOrderBy(TCustomSqlStatement stmt, TObjectName column)
        {
            if (!(stmt is TSelectSqlStatement))
            {
                return;
            }

            TSelectSqlStatement select = (TSelectSqlStatement)stmt;
            TOrderBy orderBy = select.OrderbyClause;
            if (orderBy == null)
            {
                return;
            }

            for (int i = 0; i < orderBy.Items.Count; i++)
            {
                TOrderByItem item = orderBy.Items.getOrderByItem(i);
                if (item.startToken.posinlist <= column.startToken.posinlist && item.endToken.posinlist >= column.endToken.posinlist)
                {
                    orderBy.Items.removeElementAt(i);
                    break;
                }
            }

            if (orderBy.Items.Count == 0)
            {
                select.OrderbyClause = null;
            }
        }

        private void removeObjectFromGroupBy(TCustomSqlStatement stmt, TObjectName column)
        {
            if (!(stmt is TSelectSqlStatement))
            {
                return;
            }

            TSelectSqlStatement select = (TSelectSqlStatement)stmt;
            TGroupBy groupBy = select.GroupByClause;
            if (groupBy == null)
            {
                return;
            }

            for (int i = 0; i < groupBy.Items.Count; i++)
            {
                TGroupByItem item = groupBy.Items.getGroupByItem(i);
                if (item.startToken.posinlist <= column.startToken.posinlist && item.endToken.posinlist >= column.endToken.posinlist)
                {
                    groupBy.Items.removeElementAt(i);
                    break;
                }
            }

            if (groupBy.Items.Count == 0)
            {
                select.GroupByClause = null;
            }
        }

        public static void Main(string[] args)
        {
            IList<string> argList = new List<string>(args);

            int scriptIndex = argList.IndexOf("/f");
            string sqltext = null;
            if (scriptIndex > 0 && args.Length > scriptIndex + 1)
            {
                sqltext = getFileContent(args[scriptIndex + 1]);
            }

            if (args.Length < 3 || sqltext == null)
            {
                Console.WriteLine("Usage: removeColumn [table1.colum1,table2.column2,...] [/f <scriptfile>] [/t <database type>]");
                Console.WriteLine("/f: specify the sql script file path.");
                Console.WriteLine("/t: Option, set the database type. Support oracle, mssql, the default type is oracle");
                return;
            }

            EDbVendor vendor = Common.GetEDbVendor(args);
            String columnArg = args[0];

            removeColumn ro = null;
            string[] columns = columnArg.Split(',');
            for (int i = 0; i < columns.Length; i++)
            {
                ro = new removeColumn(vendor, sqltext);
                int ret = ro.deleteColumn(columns[i]);
                if (ret == 0)
                {
                    Console.WriteLine("Error Message: " + ro.msg);
                    return;
                }
                sqltext = ro.ModifiedText;
            }

            if (ro != null)
            {
                Console.WriteLine(ro.ModifiedText);
            }

        }

        public static string getFileContent(string file)
        {
            try
            {
                string lcsqltext = "";

                StreamReader re = File.OpenText(file);

                string input = null;
                while ((input = re.ReadLine()) != null)
                {
                    if (lcsqltext.Length > 0)
                    {
                        lcsqltext = lcsqltext + Environment.NewLine;
                    }
                    lcsqltext = lcsqltext + input;
                }
                return lcsqltext.Trim();
            }
            catch (Exception e)
            {
                // Let the user know what went wrong.
                Console.WriteLine("File could not be read:");
                Console.WriteLine(e.Message);
            }
            return null;
        }
    }
}