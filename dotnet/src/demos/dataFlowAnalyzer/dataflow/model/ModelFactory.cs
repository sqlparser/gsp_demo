namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

    using TConstant = gudusoft.gsqlparser.nodes.TConstant;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using TParseTreeNode = gudusoft.gsqlparser.nodes.TParseTreeNode;
    using TResultColumn = gudusoft.gsqlparser.nodes.TResultColumn;
    using TTable = gudusoft.gsqlparser.nodes.TTable;
    using TCreateViewSqlStatement = gudusoft.gsqlparser.stmt.TCreateViewSqlStatement;
    using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;
    using System;

    public class ModelFactory
    {

        public static SelectResultSet createResultSet(TSelectSqlStatement select)
        {
            if (ModelBindingManager.getModel(select.ResultColumnList) is ResultSet)
            {
                return (SelectResultSet)ModelBindingManager.getModel(select.ResultColumnList);
            }
            SelectResultSet resultSet = new SelectResultSet(select);
            ModelBindingManager.bindModel(select.ResultColumnList, resultSet);
            return resultSet;
        }

        public static ResultSet createResultSet(TParseTreeNode gspObject)
        {
            if (ModelBindingManager.getModel(gspObject) is ResultSet)
            {
                return (ResultSet)ModelBindingManager.getModel(gspObject);
            }
            ResultSet resultSet = new ResultSet(gspObject);
            ModelBindingManager.bindModel(gspObject, resultSet);
            return resultSet;
        }

        public static ResultColumn createResultColumn(ResultSet resultSet, TResultColumn resultColumn)
        {
            if (ModelBindingManager.getModel(resultColumn) is ResultColumn)
            {
                return (ResultColumn)ModelBindingManager.getModel(resultColumn);
            }
            ResultColumn column = new ResultColumn(resultSet, resultColumn);
            ModelBindingManager.bindModel(resultColumn, column);
            return column;
        }

        public static ResultColumn createSelectSetResultColumn(ResultSet resultSet, ResultColumn resultColumn)
        {
            if (ModelBindingManager.getModel(resultColumn) is ResultColumn)
            {
                return (ResultColumn)ModelBindingManager.getModel(resultColumn);
            }
            ResultColumn column = new SelectSetResultColumn(resultSet, resultColumn);
            ModelBindingManager.bindModel(resultColumn, column);
            return column;
        }

        public static ResultColumn createSelectSetResultColumn(ResultSet resultSet, TResultColumn resultColumn)
        {
            SelectSetResultColumn column = new SelectSetResultColumn(resultSet, resultColumn);
            return column;
        }

        public static ResultColumn createResultColumn(ResultSet resultSet, TObjectName resultColumn)
        {
            if (ModelBindingManager.getModel(resultColumn) is ResultColumn)
            {
                return (ResultColumn)ModelBindingManager.getModel(resultColumn);
            }
            ResultColumn column = new ResultColumn(resultSet, resultColumn);
            ModelBindingManager.bindModel(resultColumn, column);
            return column;
        }

        public static ResultColumn createMergeResultColumn(ResultSet resultSet, TObjectName resultColumn)
        {
            if (ModelBindingManager.getMergeModel(resultColumn) is ResultColumn)
            {
                return (ResultColumn)ModelBindingManager.getMergeModel(resultColumn);
            }
            ResultColumn column = new ResultColumn(resultSet, resultColumn);
            ModelBindingManager.bindMergeModel(resultColumn, column);
            return column;
        }

        public static ResultColumn createUpdateResultColumn(ResultSet resultSet, TObjectName resultColumn)
        {
            if (ModelBindingManager.getUpdateModel(resultColumn) is ResultColumn)
            {
                return (ResultColumn)ModelBindingManager.getUpdateModel(resultColumn);
            }
            ResultColumn column = new ResultColumn(resultSet, resultColumn);
            ModelBindingManager.bindUpdateModel(resultColumn, column);
            return column;
        }

        public static ResultColumn createResultColumn(QueryTable queryTableModel, TResultColumn resultColumn)
        {
            if (ModelBindingManager.getModel(resultColumn) is ResultColumn)
            {
                return (ResultColumn)ModelBindingManager.getModel(resultColumn);
            }
            ResultColumn column = new ResultColumn(queryTableModel, resultColumn);
            ModelBindingManager.bindModel(resultColumn, column);
            return column;
        }

        public static Table createTable(TTable table)
        {
            if (ModelBindingManager.getModel(table) is Table)
            {
                return (Table)ModelBindingManager.getModel(table);
            }
            Table tableModel = new Table(table);
            ModelBindingManager.bindModel(table, tableModel);
            return tableModel;
        }

        public static Table createTableFromCreateDML(TTable table)
        {
            if (ModelBindingManager.getCreateModel(table) is Table)
            {
                return (Table)ModelBindingManager.getCreateModel(table);
            }
            Table tableModel = new Table(table);
            ModelBindingManager.bindCreateModel(table, tableModel);
            return tableModel;
        }

        public static QueryTable createQueryTable(TTable table)
        {
            QueryTable tableModel = null;

            if (table.CTE != null)
            {
                if (ModelBindingManager.getModel(table.CTE) is QueryTable)
                {
                    return (QueryTable)ModelBindingManager.getModel(table.CTE);
                }

                tableModel = new QueryTable(table);

                ModelBindingManager.bindModel(table.CTE, tableModel);
            }
            else if (table.Subquery != null && table.Subquery.ResultColumnList != null)
            {
                if (ModelBindingManager.getModel(table.Subquery.ResultColumnList) is QueryTable)
                {
                    return (QueryTable)ModelBindingManager.getModel(table.Subquery.ResultColumnList);
                }

                tableModel = new QueryTable(table);
                ModelBindingManager.bindModel(table.Subquery.ResultColumnList, tableModel);
            }
            else
            {
                if (ModelBindingManager.getModel(table) is QueryTable)
                {
                    return (QueryTable)ModelBindingManager.getModel(table);
                }
                tableModel = new QueryTable(table);
                ModelBindingManager.bindModel(table, tableModel);
            }
            return tableModel;
        }

        public static TableColumn createTableColumn(Table table, TObjectName column)
        {
            if (ModelBindingManager.getModel(new Tuple<Table, TObjectName>(table, column)) is TableColumn)
            {
                return (TableColumn)ModelBindingManager.getModel(new Tuple<Table, TObjectName>(table, column));
            }
            TableColumn columnModel = new TableColumn(table, column);
            ModelBindingManager.bindModel(new Tuple<Table, TObjectName>(table, column), columnModel);
            return columnModel;
        }

        public static DataFlowRelation createDataFlowRelation()
        {
            DataFlowRelation relation = new DataFlowRelation();
            ModelBindingManager.addRelation(relation);
            return relation;
        }

        public static TableColumn createTableColumn(Table table, TResultColumn column)
        {
            if (column.AliasClause != null && column.AliasClause.AliasName != null)
            {
                TableColumn columnModel = new TableColumn(table, column.AliasClause.AliasName);
                ModelBindingManager.bindModel(column, columnModel);
                return columnModel;
            }
            return null;
        }

        public static RecordSetRelation createRecordSetRelation()
        {
            RecordSetRelation relation = new RecordSetRelation();
            ModelBindingManager.addRelation(relation);
            return relation;
        }

        public static ImpactRelation createImpactRelation()
        {
            ImpactRelation relation = new ImpactRelation();
            ModelBindingManager.addRelation(relation);
            return relation;
        }

        public static View createView(TCreateViewSqlStatement viewStmt)
        {
            if (ModelBindingManager.getViewModel(viewStmt) is View)
            {
                return (View)ModelBindingManager.getViewModel(viewStmt);
            }
            View viewModel = new View(viewStmt);
            ModelBindingManager.bindViewModel(viewStmt, viewModel);
            return viewModel;
        }

        public static ViewColumn createViewColumn(View viewModel, TObjectName column, int index)
        {
            if (ModelBindingManager.getViewModel(column) is ViewColumn)
            {
                return (ViewColumn)ModelBindingManager.getViewModel(column);
            }
            ViewColumn columnModel = new ViewColumn(viewModel, column, index);
            ModelBindingManager.bindViewModel(column, columnModel);
            return columnModel;
        }

        public static TableColumn createInsertTableColumn(Table tableModel, TObjectName column)
        {
            if (ModelBindingManager.getInsertModel(column) is TableColumn)
            {
                return (TableColumn)ModelBindingManager.getInsertModel(column);
            }
            TableColumn columnModel = new TableColumn(tableModel, column);
            ModelBindingManager.bindInsertModel(column, columnModel);
            return columnModel;
        }

        public static TableColumn createInsertTableColumn(Table tableModel, TConstant column, int columnIndex)
        {
            if (ModelBindingManager.getInsertModel(column) is TableColumn)
            {
                return (TableColumn)ModelBindingManager.getInsertModel(column);
            }
            TableColumn columnModel = new TableColumn(tableModel, column, columnIndex);
            ModelBindingManager.bindInsertModel(column, columnModel);
            return columnModel;
        }

        public static SelectSetResultSet createSelectSetResultSet(TSelectSqlStatement stmt)
        {
            if (ModelBindingManager.getModel(stmt) is SelectSetResultSet)
            {
                return (SelectSetResultSet)ModelBindingManager.getModel(stmt);
            }
            SelectSetResultSet resultSet = new SelectSetResultSet(stmt);
            ModelBindingManager.bindModel(stmt, resultSet);
            return resultSet;
        }
        public static ResultColumn createStarResultColumn(SelectResultSet resultSet, Tuple<TResultColumn, TObjectName> starColumnPair)
        {
            if (ModelBindingManager.getModel(starColumnPair) is ResultColumn)
            {
                return (ResultColumn)ModelBindingManager.getModel(starColumnPair);
            }
            ResultColumn column = new ResultColumn(resultSet, starColumnPair);
            ModelBindingManager.bindModel(starColumnPair, column);
            return column;
        }

    }

}