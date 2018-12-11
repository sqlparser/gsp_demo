using System.Collections;
using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

    using ESetOperatorType = gudusoft.gsqlparser.ESetOperatorType;
    using TMergeInsertClause = gudusoft.gsqlparser.nodes.TMergeInsertClause;
    using TMergeUpdateClause = gudusoft.gsqlparser.nodes.TMergeUpdateClause;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using TObjectNameList = gudusoft.gsqlparser.nodes.TObjectNameList;
    using TParseTreeNode = gudusoft.gsqlparser.nodes.TParseTreeNode;
    using TResultColumnList = gudusoft.gsqlparser.nodes.TResultColumnList;
    using TTable = gudusoft.gsqlparser.nodes.TTable;
    using TCreateViewSqlStatement = gudusoft.gsqlparser.stmt.TCreateViewSqlStatement;
    using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;
    using TUpdateSqlStatement = gudusoft.gsqlparser.stmt.TUpdateSqlStatement;
    using System;
    using gudusoft.gsqlparser.nodes;
    using gudusoft.gsqlparser;

    public class ModelBindingManager
    {

        private static readonly IDictionary modelBindingMap = new Hashtable();
        private static readonly IDictionary viewModelBindingMap = new Hashtable();
        private static readonly IDictionary insertModelBindingMap = new Hashtable();
        private static readonly IDictionary createModelBindingMap = new Hashtable();
        private static readonly IDictionary mergeModelBindingMap = new Hashtable();
        private static readonly IDictionary updateModelBindingMap = new Hashtable();
        private static readonly List<Relation> relationHolder = new List<Relation>();

        public static void bindModel(object gspModel, object relationModel)
        {
            modelBindingMap[gspModel] = relationModel;
        }

        public static object getModel(object gspModel)
        {
            if (gspModel == null)
                return null;

            if (gspModel is TTable)
            {
                TTable table = (TTable)gspModel;
                if (table.CTE != null)
                {
                    return modelBindingMap[table.CTE];
                }
                if (table.Subquery != null && table.Subquery.ResultColumnList != null)
                {
                    return modelBindingMap[table.Subquery.ResultColumnList];
                }
            }

            if (gspModel is TSelectSqlStatement)
            {
                TSelectSqlStatement select = (TSelectSqlStatement)gspModel;
                if (select.ResultColumnList != null)
                {
                    return modelBindingMap[select.ResultColumnList];
                }
            }

            object result = modelBindingMap[gspModel];
            if (result == null)
            {
                result = createModelBindingMap[gspModel];
            }
            if (result == null)
            {
                result = insertModelBindingMap[gspModel];
            }
            if (result == null)
            {
                result = updateModelBindingMap[gspModel];
            }
            if (result == null)
            {
                result = mergeModelBindingMap[gspModel];
            }
            if (result == null)
            {
                result = viewModelBindingMap[gspModel];
            }
            return result;
        }

        public static void bindViewModel(object gspModel, object relationModel)
        {
            viewModelBindingMap[gspModel] = relationModel;
        }

        public static object getViewModel(object gspModel)
        {
            return viewModelBindingMap[gspModel];
        }

        public static void bindUpdateModel(object gspModel, object relationModel)
        {
            updateModelBindingMap[gspModel] = relationModel;
        }

        public static object getUpdateModel(object gspModel)
        {
            return updateModelBindingMap[gspModel];
        }

        public static void bindMergeModel(object gspModel, object relationModel)
        {
            mergeModelBindingMap[gspModel] = relationModel;
        }

        public static object getMergeModel(object gspModel)
        {
            return mergeModelBindingMap[gspModel];
        }

        public static void bindInsertModel(object gspModel, object relationModel)
        {
            insertModelBindingMap[gspModel] = relationModel;
        }

        public static object getInsertModel(object gspModel)
        {
            return insertModelBindingMap[gspModel];
        }

        public static Table getCreateTable(TTable table)
        {
            if (table != null && table.TableName != null)
            {
                IEnumerator iter = createModelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    TTable node = (TTable)iter.Current;
                    if (node.FullName.Equals(table.FullName))
                    {
                        return (Table)createModelBindingMap[node];
                    }
                }
            }
            return null;
        }

        public static Table getCreateModel(TTable table)
        {
            return (Table)createModelBindingMap[table];
        }

        public static void bindCreateModel(TTable table, Table tableModel)
        {
            createModelBindingMap[table] = tableModel;
        }

        public static TTable getTable(TCustomSqlStatement stmt, TObjectName column)
        {
            IEnumerator iter = modelBindingMap.Values.GetEnumerator();
            while (iter.MoveNext())
            {
                object key = iter.Current;
                if (key is Table)
                {
                    key = ((Table)key).TableObject;
                }
                else if (key is QueryTable)
                {
                    key = ((QueryTable)key).TableObject;
                }
                else
                {
                    continue;
                }

                TTable table = (TTable)key;

                if (table.Subquery == stmt)
                    continue;

                if (column.TableString != null && column.TableString.Trim().Length > 0)
                {
                    if (table.AliasName != null)
                    {
                        if (!table.AliasName.Equals(column.TableString))
                        {
                            continue;
                        }
                        else
                        {
                            return table;
                        }
                    }
                }

                TObjectName[] columns = getTableColumns(table);
                for (int i = 0; i < columns.Length; i++)
                {
                    TObjectName columnName = columns[i];
                    if ("*".Equals(columnName.ColumnNameOnly))
                    {
                        continue;
                    }
                    if (columnName == column)
                    {
                        return table;
                    }
                }
            }
            return null;
        }

        public static TObjectName[] getTableColumns(TTable table)
        {
            Table createTable = ModelBindingManager.getCreateTable(table);
            if (createTable != null)
            {
                IList<TableColumn> columnList = createTable.Columns;
                TObjectName[] columns = new TObjectName[columnList.Count];
                for (int i = 0; i < columns.Length; i++)
                {
                    columns[i] = columnList[i].ColumnObject;
                }
                Array.Sort(columns, new ComparatorAnonymousInnerClass());
                return columns;
            }
            else
            {
                TObjectNameList list = table.ObjectNameReferences;
                List<TObjectName> columns = new List<TObjectName>();

                if (list.size() == 0 && table.Subquery != null)
                {
                    ResultSet resultSet = (ResultSet)ModelBindingManager.getModel(table.Subquery);
                    if (resultSet != null)
                    {
                        IList<ResultColumn> columnList = resultSet.Columns;
                        for (int i = 0; i < columnList.Count; i++)
                        {
                            ResultColumn resultColumn = columnList[i];
                            if (resultColumn.ColumnObject is TResultColumn)
                            {
                                TResultColumn columnObject = ((TResultColumn)resultColumn.ColumnObject);
                                TAliasClause alias = columnObject.AliasClause;
                                if (alias != null && alias.AliasName != null)
                                {
                                    columns.Add(alias.AliasName);
                                }
                                else
                                {
                                    if (columnObject.FieldAttr != null)
                                    {
                                        columns.Add(columnObject.FieldAttr);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
                            }
                            else if (resultColumn.ColumnObject is TObjectName)
                            {
                                columns.Add((TObjectName)resultColumn.ColumnObject);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < list.size(); i++)
                    {
                        columns.Add(list.getObjectName(i));
                    }
                }

                TObjectName[] columnArray = columns.ToArray();
                Array.Sort(columnArray, new ComparatorAnonymousInnerClass());
                return columnArray;
            }

        }

        private class ComparatorAnonymousInnerClass : IComparer<TObjectName>
        {
            public virtual int Compare(TObjectName o1, TObjectName o2)
            {
                return o1.startToken.posinlist - o2.startToken.posinlist;
            }
        }

        public static IList<TTable> BaseTables
        {
            get
            {
                IList<TTable> tables = new List<TTable>();

                IEnumerator iter = modelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    object key = iter.Current;
                    if (!(key is TTable))
                    {
                        continue;
                    }
                    TTable table = (TTable)key;
                    if (table.Subquery == null)
                    {
                        tables.Add(table);
                    }
                }

                iter = createModelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    object key = iter.Current;
                    if (!(key is TTable))
                    {
                        continue;
                    }
                    TTable table = (TTable)key;
                    tables.Add(table);
                }

                iter = insertModelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    object key = iter.Current;
                    if (!(key is TTable))
                    {
                        continue;
                    }
                    TTable table = (TTable)key;
                    tables.Add(table);
                }

                iter = mergeModelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    object key = iter.Current;
                    if (!(key is TTable))
                    {
                        continue;
                    }
                    TTable table = (TTable)key;
                    tables.Add(table);
                }

                iter = updateModelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    object key = iter.Current;
                    if (!(key is TTable))
                    {
                        continue;
                    }
                    TTable table = (TTable)key;
                    tables.Add(table);
                }

                return tables;
            }
        }

        public static IList<TCreateViewSqlStatement> Views
        {
            get
            {
                IList<TCreateViewSqlStatement> views = new List<TCreateViewSqlStatement>();

                IEnumerator iter = viewModelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    object key = iter.Current;
                    if (!(key is TCreateViewSqlStatement))
                    {
                        continue;
                    }
                    TCreateViewSqlStatement view = (TCreateViewSqlStatement)key;
                    views.Add(view);
                }
                return views;
            }
        }

        public static IList<TResultColumnList> SelectResultSets
        {
            get
            {
                IList<TResultColumnList> resultSets = new List<TResultColumnList>();

                IEnumerator iter = modelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    object key = iter.Current;
                    if (!(key is TResultColumnList))
                    {
                        continue;
                    }
                    TResultColumnList resultset = (TResultColumnList)key;
                    resultSets.Add(resultset);
                }
                return resultSets;
            }
        }

        public static IList<TSelectSqlStatement> SelectSetResultSets
        {
            get
            {
                IList<TSelectSqlStatement> resultSets = new List<TSelectSqlStatement>();

                IEnumerator iter = modelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    object key = iter.Current;
                    if (!(key is TSelectSqlStatement))
                    {
                        continue;
                    }

                    TSelectSqlStatement stmt = (TSelectSqlStatement)key;
                    if (stmt.SetOperatorType == ESetOperatorType.none)
                    {
                        continue;
                    }

                    resultSets.Add(stmt);
                }
                return resultSets;
            }
        }

        public static IList<TCTE> CTEs
        {
            get
            {
                IList<TCTE> ctes = new List<TCTE>();

                IEnumerator iter = modelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    object key = iter.Current;
                    if (!(key is TCTE))
                    {
                        continue;
                    }

                    TCTE cte = (TCTE)key;
                    ctes.Add(cte);
                }
                return ctes;
            }
        }

        public static IList<TTable> TableWithSelectSetResultSets
        {
            get
            {
                IList<TTable> resultSets = new List<TTable>();

                IEnumerator iter = modelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    object key = iter.Current;
                    if (!(key is TTable))
                    {
                        continue;
                    }

                    if (((TTable)key).Subquery == null)
                    {
                        continue;
                    }
                    TSelectSqlStatement stmt = ((TTable)key).Subquery;
                    if (stmt.SetOperatorType == ESetOperatorType.none)
                    {
                        continue;
                    }

                    resultSets.Add((TTable)key);
                }
                return resultSets;
            }
        }

        public static IList<TParseTreeNode> MergeResultSets
        {
            get
            {
                IList<TParseTreeNode> resultSets = new List<TParseTreeNode>();

                IEnumerator iter = modelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    object key = iter.Current;
                    if (!(key is TMergeUpdateClause) && !(key is TMergeInsertClause))
                    {
                        continue;
                    }
                    TParseTreeNode resultset = (TParseTreeNode)key;
                    resultSets.Add(resultset);
                }
                return resultSets;
            }
        }

        public static IList<TParseTreeNode> UpdateResultSets
        {
            get
            {
                IList<TParseTreeNode> resultSets = new List<TParseTreeNode>();

                IEnumerator iter = modelBindingMap.Keys.GetEnumerator();
                while (iter.MoveNext())
                {
                    object key = iter.Current;
                    if (!(key is TUpdateSqlStatement))
                    {
                        continue;
                    }
                    TParseTreeNode resultset = (TParseTreeNode)key;
                    resultSets.Add(resultset);
                }
                return resultSets;
            }
        }

        public static void addRelation(Relation relation)
        {
            if (relation != null && !relationHolder.Contains(relation))
            {
                relationHolder.Add(relation);
            }
        }

        public static Relation[] Relations
        {
            get
            {
                return relationHolder.ToArray();
            }
        }

        public static void reset()
        {
            relationHolder.Clear();
            modelBindingMap.Clear();
            viewModelBindingMap.Clear();
            insertModelBindingMap.Clear();
            mergeModelBindingMap.Clear();
            updateModelBindingMap.Clear();
            createModelBindingMap.Clear();
        }

    }

}