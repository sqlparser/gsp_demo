
package demos.dlineage.dataflow.model;

import demos.dlineage.util.Pair;
import gudusoft.gsqlparser.nodes.TConstant;
import gudusoft.gsqlparser.nodes.TObjectName;
import gudusoft.gsqlparser.nodes.TParseTreeNode;
import gudusoft.gsqlparser.nodes.TResultColumn;
import gudusoft.gsqlparser.nodes.TTable;
import gudusoft.gsqlparser.stmt.TCreateViewSqlStatement;
import gudusoft.gsqlparser.stmt.TCursorDeclStmt;
import gudusoft.gsqlparser.stmt.TSelectSqlStatement;

public class ModelFactory
{

	public static SelectResultSet createResultSet( TSelectSqlStatement select,
			boolean isTarget )
	{
		if ( ModelBindingManager.getModel( select.getResultColumnList( ) ) instanceof ResultSet )
		{
			return (SelectResultSet) ModelBindingManager.getModel( select.getResultColumnList( ) );
		}
		SelectResultSet resultSet = new SelectResultSet( select, isTarget );
		ModelBindingManager.bindModel( select.getResultColumnList( ), resultSet );
		return resultSet;
	}

	public static ResultSet createResultSet( TParseTreeNode gspObject,
			boolean isTarget )
	{
		if ( ModelBindingManager.getModel( gspObject ) instanceof ResultSet )
		{
			return (ResultSet) ModelBindingManager.getModel( gspObject );
		}
		ResultSet resultSet = new ResultSet( gspObject, isTarget );
		ModelBindingManager.bindModel( gspObject, resultSet );
		return resultSet;
	}

	public static ResultColumn createResultColumn( ResultSet resultSet,
			TResultColumn resultColumn )
	{
		if ( ModelBindingManager.getModel( resultColumn ) instanceof ResultColumn )
		{
			return (ResultColumn) ModelBindingManager.getModel( resultColumn );
		}
		ResultColumn column = new ResultColumn( resultSet, resultColumn );
		ModelBindingManager.bindModel( resultColumn, column );
		return column;
	}

	public static ResultColumn createSelectSetResultColumn(
			ResultSet resultSet, ResultColumn resultColumn, int index )
	{
		if ( ModelBindingManager.getModel( resultColumn ) instanceof ResultColumn )
		{
			return (ResultColumn) ModelBindingManager.getModel( resultColumn );
		}
		ResultColumn column = new SelectSetResultColumn( resultSet,
				resultColumn, index );
		ModelBindingManager.bindModel( resultColumn, column );
		return column;
	}

	public static ResultColumn createSelectSetResultColumn(
			ResultSet resultSet, TResultColumn resultColumn, int index )
	{
		SelectSetResultColumn column = new SelectSetResultColumn( resultSet,
				resultColumn, index );
		return column;
	}

	public static ResultColumn createResultColumn( ResultSet resultSet,
			TObjectName resultColumn )
	{
		if ( ModelBindingManager.getModel( resultColumn ) instanceof ResultColumn )
		{
			return (ResultColumn) ModelBindingManager.getModel( resultColumn );
		}
		ResultColumn column = new ResultColumn( resultSet, resultColumn );
		ModelBindingManager.bindModel( resultColumn, column );
		return column;
	}

	public static ResultColumn createMergeResultColumn( ResultSet resultSet,
			TObjectName resultColumn )
	{
		if ( ModelBindingManager.getMergeModel( resultColumn ) instanceof ResultColumn )
		{
			return (ResultColumn) ModelBindingManager.getMergeModel( resultColumn );
		}
		ResultColumn column = new ResultColumn( resultSet, resultColumn );
		ModelBindingManager.bindMergeModel( resultColumn, column );
		return column;
	}

	public static ResultColumn createUpdateResultColumn( ResultSet resultSet,
			TObjectName resultColumn )
	{
		if ( ModelBindingManager.getUpdateModel( resultColumn ) instanceof ResultColumn )
		{
			return (ResultColumn) ModelBindingManager.getUpdateModel( resultColumn );
		}
		ResultColumn column = new ResultColumn( resultSet, resultColumn );
		ModelBindingManager.bindUpdateModel( resultColumn, column );
		return column;
	}

	public static ResultColumn createResultColumn( QueryTable queryTableModel,
			TResultColumn resultColumn )
	{
		if ( ModelBindingManager.getModel( resultColumn ) instanceof ResultColumn )
		{
			return (ResultColumn) ModelBindingManager.getModel( resultColumn );
		}
		ResultColumn column = new ResultColumn( queryTableModel, resultColumn );
		ModelBindingManager.bindModel( resultColumn, column );
		return column;
	}

	public static Table createTableFromCreateDML( TTable table )
	{
		if ( ModelBindingManager.getCreateModel( table ) instanceof Table )
		{
			return (Table) ModelBindingManager.getCreateModel( table );
		}
		Table tableModel = new Table( table );
		ModelBindingManager.bindCreateModel( table, tableModel );
		return tableModel;
	}

	public static Table createTable( TTable table )
	{
		if ( ModelBindingManager.getModel( table ) instanceof Table )
		{
			return (Table) ModelBindingManager.getModel( table );
		}
		Table tableModel = new Table( table );
		ModelBindingManager.bindModel( table, tableModel );
		return tableModel;
	}

	public static QueryTable createQueryTable( TTable table )
	{
		QueryTable tableModel = null;

		if ( table.getCTE( ) != null )
		{
			if ( ModelBindingManager.getModel( table.getCTE( ) ) instanceof QueryTable )
			{
				return (QueryTable) ModelBindingManager.getModel( table.getCTE( ) );
			}

			tableModel = new QueryTable( table );

			ModelBindingManager.bindModel( table.getCTE( ), tableModel );
		}
		else if ( table.getSubquery( ) != null
				&& table.getSubquery( ).getResultColumnList( ) != null )
		{
			if ( ModelBindingManager.getModel( table.getSubquery( )
					.getResultColumnList( ) ) instanceof QueryTable )
			{
				return (QueryTable) ModelBindingManager.getModel( table.getSubquery( )
						.getResultColumnList( ) );
			}

			tableModel = new QueryTable( table );
			ModelBindingManager.bindModel( table.getSubquery( )
					.getResultColumnList( ), tableModel );
		}
		else
		{
			if ( ModelBindingManager.getModel( table ) instanceof QueryTable )
			{
				return (QueryTable) ModelBindingManager.getModel( table );
			}
			tableModel = new QueryTable( table );
			ModelBindingManager.bindModel( table, tableModel );
		}
		return tableModel;
	}

	public static TableColumn createTableColumn( Table table, TObjectName column )
	{
		if ( ModelBindingManager.getModel( new Pair<Table, TObjectName>( table,
				column ) ) instanceof TableColumn )
		{
			return (TableColumn) ModelBindingManager.getModel( new Pair<Table, TObjectName>( table,
					column ) );
		}
		TableColumn columnModel = new TableColumn( table, column );
		ModelBindingManager.bindModel( new Pair<Table, TObjectName>( table,
				column ), columnModel );
		return columnModel;
	}

	public static DataFlowRelation createDataFlowRelation( )
	{
		DataFlowRelation relation = new DataFlowRelation( );
		ModelBindingManager.addRelation( relation );
		return relation;
	}

	public static TableColumn createTableColumn( Table table,
			TResultColumn column )
	{
		if ( column.getAliasClause( ) != null
				&& column.getAliasClause( ).getAliasName( ) != null )
		{
			TableColumn columnModel = new TableColumn( table,
					column.getAliasClause( ).getAliasName( ) );
			ModelBindingManager.bindModel( column, columnModel );
			return columnModel;
		}
		return null;
	}

	public static RecordSetRelation createRecordSetRelation( )
	{
		RecordSetRelation relation = new RecordSetRelation( );
		ModelBindingManager.addRelation( relation );
		return relation;
	}

	public static ImpactRelation createImpactRelation( )
	{
		ImpactRelation relation = new ImpactRelation( );
		ModelBindingManager.addRelation( relation );
		return relation;
	}
	
	public static JoinRelation createJoinRelation( )
	{
		JoinRelation relation = new JoinRelation( );
		ModelBindingManager.addRelation( relation );
		return relation;
	}

	public static View createView( TCreateViewSqlStatement viewStmt )
	{
		if ( ModelBindingManager.getViewModel( viewStmt ) instanceof View )
		{
			return (View) ModelBindingManager.getViewModel( viewStmt );
		}
		View viewModel = new View( viewStmt );
		ModelBindingManager.bindViewModel( viewStmt, viewModel );
		return viewModel;
	}

	public static ViewColumn createViewColumn( View viewModel,
			TObjectName column, int index )
	{
		Pair<View, TObjectName> bindingModel = new Pair<View, TObjectName>(
				viewModel, column );
		if ( ModelBindingManager.getViewModel( bindingModel ) instanceof ViewColumn )
		{
			return (ViewColumn) ModelBindingManager.getViewModel( bindingModel );
		}
		ViewColumn columnModel = new ViewColumn( viewModel, column, index );
		ModelBindingManager.bindViewModel( bindingModel, columnModel );
		return columnModel;
	}

	public static TableColumn createInsertTableColumn( Table tableModel,
			TObjectName column )
	{
		Pair<Table, TObjectName> bindingModel = new Pair<Table, TObjectName>(
				tableModel, column );
		if ( ModelBindingManager
				.getInsertModel( bindingModel ) instanceof TableColumn )
		{
			return (TableColumn) ModelBindingManager
					.getInsertModel( bindingModel );
		}
		TableColumn columnModel = new TableColumn( tableModel, column );
		ModelBindingManager.bindInsertModel( bindingModel, columnModel );
		return columnModel;
	}

	public static TableColumn createInsertTableColumn( Table tableModel,
			TConstant column, int columnIndex )
	{
		Pair<Table, TConstant> bindingModel = new Pair<Table, TConstant>(
				tableModel, column );

		if ( ModelBindingManager
				.getInsertModel( bindingModel ) instanceof TableColumn )
		{
			return (TableColumn) ModelBindingManager
					.getInsertModel( bindingModel );
		}
		TableColumn columnModel = new TableColumn( tableModel,
				column,
				columnIndex );
		ModelBindingManager.bindInsertModel( bindingModel, columnModel );
		return columnModel;
	}

	public static SelectSetResultSet createSelectSetResultSet(
			TSelectSqlStatement stmt )
	{
		if ( ModelBindingManager.getModel( stmt ) instanceof SelectSetResultSet )
		{
			return (SelectSetResultSet) ModelBindingManager.getModel( stmt );
		}
		SelectSetResultSet resultSet = new SelectSetResultSet( stmt );
		ModelBindingManager.bindModel( stmt, resultSet );
		return resultSet;
	}

	public static ResultColumn createStarResultColumn(
			SelectResultSet resultSet,
			Pair<TResultColumn, TObjectName> starColumnPair )
	{
		if ( ModelBindingManager.getModel( starColumnPair ) instanceof ResultColumn )
		{
			return (ResultColumn) ModelBindingManager.getModel( starColumnPair );
		}
		ResultColumn column = new ResultColumn( resultSet, starColumnPair );
		ModelBindingManager.bindModel( starColumnPair, column );
		return column;
	}

	public static CursorResultSet createCursorResultSet( TCursorDeclStmt stmt )
	{
		if ( ModelBindingManager.getModel( stmt ) instanceof SelectSetResultSet )
		{
			return (CursorResultSet) ModelBindingManager.getModel( stmt );
		}
		CursorResultSet resultSet = new CursorResultSet( stmt );
		ModelBindingManager.bindModel( stmt, resultSet );
		return resultSet;
	}

}
