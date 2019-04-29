
package demos.dlineage.dataflow.model;

import gudusoft.gsqlparser.ESetOperatorType;
import gudusoft.gsqlparser.TCustomSqlStatement;
import gudusoft.gsqlparser.nodes.TAliasClause;
import gudusoft.gsqlparser.nodes.TCTE;
import gudusoft.gsqlparser.nodes.TMergeInsertClause;
import gudusoft.gsqlparser.nodes.TMergeUpdateClause;
import gudusoft.gsqlparser.nodes.TObjectName;
import gudusoft.gsqlparser.nodes.TObjectNameList;
import gudusoft.gsqlparser.nodes.TParseTreeNode;
import gudusoft.gsqlparser.nodes.TResultColumn;
import gudusoft.gsqlparser.nodes.TResultColumnList;
import gudusoft.gsqlparser.nodes.TTable;
import gudusoft.gsqlparser.nodes.TTableList;
import gudusoft.gsqlparser.stmt.TCreateViewSqlStatement;
import gudusoft.gsqlparser.stmt.TCursorDeclStmt;
import gudusoft.gsqlparser.stmt.TSelectSqlStatement;
import gudusoft.gsqlparser.stmt.TUpdateSqlStatement;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.Comparator;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.CopyOnWriteArrayList;

import demos.dlineage.util.Pair;

@SuppressWarnings({
		"unchecked", "rawtypes"
})
public class ModelBindingManager
{

	private static final Map modelBindingMap = new LinkedHashMap( );
	private static final Map viewModelBindingMap = new LinkedHashMap( );
	private static final Map insertModelBindingMap = new LinkedHashMap( );
	private static final Map createModelBindingMap = new LinkedHashMap( );
	private static final Map createModelQuickBindingMap = new HashMap( );
	private static final Map mergeModelBindingMap = new LinkedHashMap( );
	private static final Map updateModelBindingMap = new LinkedHashMap( );
	private static final Map cursorModelBindingMap = new LinkedHashMap( );
	private static final List<Relation> relationHolder = new CopyOnWriteArrayList<Relation>( );

	private static final Map<String, TTable> tableAliasMap = new HashMap( );
	private static final Set tableSet = new HashSet( );

	public static void bindModel( Object gspModel, Object relationModel )
	{
		modelBindingMap.put( gspModel, relationModel );

		TTable table = null;
		if ( gspModel instanceof TTable )
		{
			table = ( (TTable) gspModel );
		}
		else if ( gspModel instanceof Table )
		{
			table = ( (Table) gspModel ).getTableObject( );
		}
		else if ( gspModel instanceof QueryTable )
		{
			table = ( (QueryTable) gspModel ).getTableObject( );
		}
		else if ( gspModel instanceof TCTE )
		{
			TTableList tables = ( (TCTE) gspModel ).getPreparableStmt( ).tables;
			for ( int j = 0; j < tables.size( ); j++ )
			{
				TTable item = tables.getTable( j );
				if ( item != null && item.getAliasName( ) != null )
				{
					tableAliasMap.put( item.getAliasName( ).toLowerCase( ),
							item );
				}

				if ( item != null )
				{
					tableSet.add( item );
				}
			}
		}
		else if ( gspModel instanceof Pair
				&& ( (Pair) gspModel ).first instanceof Table )
		{
			table = ( (Table) ( (Pair) gspModel ).first ).getTableObject( );
		}
		
		if ( table == null && relationModel instanceof QueryTable )
		{
			table = ( (QueryTable) relationModel ).getTableObject( );
		}

		if ( table != null && table.getAliasName( ) != null )
		{
			tableAliasMap.put( table.getAliasName( ).toLowerCase( ), table );
		}

		if ( table != null )
		{
			tableSet.add( table );
		}
	}

	public static Object getModel( Object gspModel )
	{
		if ( gspModel == null )
		{
			return null;
		}

		if ( gspModel instanceof TTable )
		{
			TTable table = (TTable) gspModel;
			if ( table.getCTE( ) != null )
			{
				return modelBindingMap.get( table.getCTE( ) );
			}
			if ( table.getSubquery( ) != null
					&& table.getSubquery( ).getResultColumnList( ) != null )
			{
				return modelBindingMap
						.get( table.getSubquery( ).getResultColumnList( ) );
			}
		}
		if ( gspModel instanceof TSelectSqlStatement )
		{
			TSelectSqlStatement select = (TSelectSqlStatement) gspModel;
			if ( select.getResultColumnList( ) != null )
			{
				return modelBindingMap.get( select.getResultColumnList( ) );
			}
		}
		Object result = modelBindingMap.get( gspModel );
		if ( result == null )
		{
			result = createModelBindingMap.get( gspModel );
		}
		if ( result == null )
		{
			result = insertModelBindingMap.get( gspModel );
		}
		if ( result == null )
		{
			result = updateModelBindingMap.get( gspModel );
		}
		if ( result == null )
		{
			result = mergeModelBindingMap.get( gspModel );
		}
		if ( result == null )
		{
			result = viewModelBindingMap.get( gspModel );
		}
		if ( result == null )
		{
			result = cursorModelBindingMap.get( gspModel );
		}
		return result;
	}

	public static void bindViewModel( Object gspModel, Object relationModel )
	{
		viewModelBindingMap.put( gspModel, relationModel );
	}

	public static Object getViewModel( Object gspModel )
	{
		return viewModelBindingMap.get( gspModel );
	}

	public static void bindUpdateModel( Object gspModel, Object relationModel )
	{
		updateModelBindingMap.put( gspModel, relationModel );
	}

	public static Object getUpdateModel( Object gspModel )
	{
		return updateModelBindingMap.get( gspModel );
	}

	public static void bindMergeModel( Object gspModel, Object relationModel )
	{
		mergeModelBindingMap.put( gspModel, relationModel );
	}

	public static Object getMergeModel( Object gspModel )
	{
		return mergeModelBindingMap.get( gspModel );
	}

	public static void bindInsertModel( Object gspModel, Object relationModel )
	{
		insertModelBindingMap.put( gspModel, relationModel );
	}

	public static Object getInsertModel( Object gspModel )
	{
		return insertModelBindingMap.get( gspModel );
	}

	public static Table getCreateTable( TTable table )
	{
		if ( table != null && table.getTableName( ) != null )
		{
			// Iterator iter = createModelBindingMap.keySet( ).iterator( );
			// while ( iter.hasNext( ) )
			// {
			// TTable node = (TTable) iter.next( );
			// if ( node.getFullName( ).equals( table.getFullName( ) ) )
			// {
			// return (Table) createModelBindingMap.get( node );
			// }
			// }
			return (Table) createModelQuickBindingMap
					.get( table.getFullName( ) );
		}
		return null;
	}

	public static Table getCreateModel( TTable table )
	{
		return (Table) createModelBindingMap.get( table );
	}

	public static void bindCreateModel( TTable table, Table tableModel )
	{
		createModelBindingMap.put( table, tableModel );
		if ( !createModelQuickBindingMap.containsKey( table.getFullName( ) ) )
		{
			createModelQuickBindingMap.put( table.getFullName( ), tableModel );
		}
	}

	public static TObjectName[] getTableColumns( TTable table )
	{
		Table createTable = ModelBindingManager.getCreateTable( table );
		if ( createTable != null )
		{
			List<TableColumn> columnList = createTable.getColumns( );
			TObjectName[] columns = new TObjectName[columnList.size( )];
			for ( int i = 0; i < columns.length; i++ )
			{
				columns[i] = columnList.get( i ).getColumnObject( );
			}
			Arrays.sort( columns, new Comparator<TObjectName>( ) {

				@Override
				public int compare( TObjectName o1, TObjectName o2 )
				{
					return o1.getStartToken( ).posinlist
							- o2.getStartToken( ).posinlist;
				}
			} );
			return columns;
		}

		TObjectNameList list = table.getObjectNameReferences( );
		List<TObjectName> columns = new ArrayList<TObjectName>( );

		if ( table.getCTE( ) != null )
		{
			ResultSet resultSet = (ResultSet) ModelBindingManager
					.getModel( table.getCTE( ) );
			if ( resultSet != null )
			{
				List<ResultColumn> columnList = resultSet.getColumns( );
				for ( int i = 0; i < columnList.size( ); i++ )
				{
					ResultColumn resultColumn = columnList.get( i );
					if ( resultColumn
							.getColumnObject( ) instanceof TResultColumn )
					{
						TResultColumn columnObject = ( (TResultColumn) resultColumn
								.getColumnObject( ) );
						TAliasClause alias = columnObject.getAliasClause( );
						if ( alias != null && alias.getAliasName( ) != null )
						{
							columns.add( alias.getAliasName( ) );
						}
						else
						{
							if ( columnObject.getFieldAttr( ) != null )
							{
								columns.add( columnObject.getFieldAttr( ) );
							}
							else
							{
								continue;
							}
						}
					}
					else if ( resultColumn
							.getColumnObject( ) instanceof TObjectName )
					{
						columns.add(
								(TObjectName) resultColumn.getColumnObject( ) );
					}
				}
			}
		}
		else if ( list.size( ) == 0 && table.getSubquery( ) != null )
		{
			ResultSet resultSet = (ResultSet) ModelBindingManager
					.getModel( table.getSubquery( ) );
			if ( resultSet != null )
			{
				List<ResultColumn> columnList = resultSet.getColumns( );
				for ( int i = 0; i < columnList.size( ); i++ )
				{
					ResultColumn resultColumn = columnList.get( i );
					if ( resultColumn
							.getColumnObject( ) instanceof TResultColumn )
					{
						TResultColumn columnObject = ( (TResultColumn) resultColumn
								.getColumnObject( ) );
						TAliasClause alias = columnObject.getAliasClause( );
						if ( alias != null && alias.getAliasName( ) != null )
						{
							columns.add( alias.getAliasName( ) );
						}
						else
						{
							if ( columnObject.getFieldAttr( ) != null )
							{
								columns.add( columnObject.getFieldAttr( ) );
							}
							else
							{
								continue;
							}
						}
					}
					else if ( resultColumn
							.getColumnObject( ) instanceof TObjectName )
					{
						columns.add(
								(TObjectName) resultColumn.getColumnObject( ) );
					}
				}
			}
		}
		else
		{
			for ( int i = 0; i < list.size( ); i++ )
			{
				columns.add( list.getObjectName( i ) );
			}
		}
		Collections.sort( columns, new Comparator<TObjectName>( ) {

			@Override
			public int compare( TObjectName o1, TObjectName o2 )
			{
				return o1.getStartToken( ).posinlist
						- o2.getStartToken( ).posinlist;
			}
		} );
		return columns.toArray( new TObjectName[0] );
	}

	public static TTable getTable( TCustomSqlStatement stmt,
			TObjectName column )
	{
		if ( column.getTableString( ) != null
				&& column.getTableString( ).trim( ).length( ) > 0 )
		{
			TTable table = tableAliasMap
					.get( column.getTableString( ).toLowerCase( ) );

			if ( table != null && table.getSubquery( ) != stmt )
				return table;
		}

		Iterator iter = tableSet.iterator( );
		while ( iter.hasNext( ) )
		{
			TTable table = (TTable) iter.next( );

			if ( table.getSubquery( ) == stmt )
				continue;

			TObjectName[] columns = getTableColumns( table );
			for ( int i = 0; i < columns.length; i++ )
			{
				TObjectName columnName = columns[i];
				if ( "*".equals( columnName.getColumnNameOnly( ) ) )
					continue;
				if ( columnName == column )
				{
					if ( columnName.getSourceTable( ) == null
							|| columnName.getSourceTable( ) == table )
					{
						return table;
					}
				}
			}
		}
		return null;
	}

	public static List<TTable> getBaseTables( )
	{
		List<TTable> tables = new ArrayList<TTable>( );

		Iterator iter = modelBindingMap.keySet( ).iterator( );
		while ( iter.hasNext( ) )
		{
			Object key = iter.next( );
			if ( !( key instanceof TTable ) )
			{
				continue;
			}
			TTable table = (TTable) key;
			if ( table.getSubquery( ) == null )
			{
				tables.add( table );
			}
		}

		iter = createModelBindingMap.keySet( ).iterator( );
		while ( iter.hasNext( ) )
		{
			Object key = iter.next( );
			if ( !( key instanceof TTable ) )
			{
				continue;
			}
			TTable table = (TTable) key;
			tables.add( table );
		}

		iter = insertModelBindingMap.keySet( ).iterator( );
		while ( iter.hasNext( ) )
		{
			Object key = iter.next( );
			if ( !( key instanceof TTable ) )
			{
				continue;
			}
			TTable table = (TTable) key;
			tables.add( table );
		}

		iter = mergeModelBindingMap.keySet( ).iterator( );
		while ( iter.hasNext( ) )
		{
			Object key = iter.next( );
			if ( !( key instanceof TTable ) )
			{
				continue;
			}
			TTable table = (TTable) key;
			tables.add( table );
		}

		iter = updateModelBindingMap.keySet( ).iterator( );
		while ( iter.hasNext( ) )
		{
			Object key = iter.next( );
			if ( !( key instanceof TTable ) )
			{
				continue;
			}
			TTable table = (TTable) key;
			tables.add( table );
		}

		return tables;
	}

	public static List<TCreateViewSqlStatement> getViews( )
	{
		List<TCreateViewSqlStatement> views = new ArrayList<TCreateViewSqlStatement>( );

		Iterator iter = viewModelBindingMap.keySet( ).iterator( );
		while ( iter.hasNext( ) )
		{
			Object key = iter.next( );
			if ( !( key instanceof TCreateViewSqlStatement ) )
			{
				continue;
			}
			TCreateViewSqlStatement view = (TCreateViewSqlStatement) key;
			views.add( view );
		}
		return views;
	}

	public static List<TResultColumnList> getSelectResultSets( )
	{
		List<TResultColumnList> resultSets = new ArrayList<TResultColumnList>( );

		Iterator iter = modelBindingMap.keySet( ).iterator( );
		while ( iter.hasNext( ) )
		{
			Object key = iter.next( );
			if ( !( key instanceof TResultColumnList ) )
			{
				continue;
			}
			TResultColumnList resultset = (TResultColumnList) key;
			resultSets.add( resultset );
		}
		return resultSets;
	}

	public static List<TSelectSqlStatement> getSelectSetResultSets( )
	{
		List<TSelectSqlStatement> resultSets = new ArrayList<TSelectSqlStatement>( );

		Iterator iter = modelBindingMap.keySet( ).iterator( );
		while ( iter.hasNext( ) )
		{
			Object key = iter.next( );
			if ( !( key instanceof TSelectSqlStatement ) )
			{
				continue;
			}

			TSelectSqlStatement stmt = (TSelectSqlStatement) key;
			if ( stmt.getSetOperatorType( ) == ESetOperatorType.none )
				continue;

			resultSets.add( stmt );
		}
		return resultSets;
	}

	public static List<TCTE> getCTEs( )
	{
		List<TCTE> resultSets = new ArrayList<TCTE>( );

		Iterator iter = modelBindingMap.keySet( ).iterator( );
		while ( iter.hasNext( ) )
		{
			Object key = iter.next( );
			if ( !( key instanceof TCTE ) )
			{
				continue;
			}

			TCTE cte = (TCTE) key;
			resultSets.add( cte );
		}
		return resultSets;
	}

	public static List<TTable> getTableWithSelectSetResultSets( )
	{
		List<TTable> resultSets = new ArrayList<TTable>( );

		Iterator iter = modelBindingMap.keySet( ).iterator( );
		while ( iter.hasNext( ) )
		{
			Object key = iter.next( );
			if ( !( key instanceof TTable ) )
			{
				continue;
			}

			if ( ( (TTable) key ).getSubquery( ) == null )
				continue;
			TSelectSqlStatement stmt = ( (TTable) key ).getSubquery( );
			if ( stmt.getSetOperatorType( ) == ESetOperatorType.none )
				continue;

			resultSets.add( (TTable) key );
		}
		return resultSets;
	}

	public static List<TParseTreeNode> getMergeResultSets( )
	{
		List<TParseTreeNode> resultSets = new ArrayList<TParseTreeNode>( );

		Iterator iter = modelBindingMap.keySet( ).iterator( );
		while ( iter.hasNext( ) )
		{
			Object key = iter.next( );
			if ( !( key instanceof TMergeUpdateClause )
					&& !( key instanceof TMergeInsertClause ) )
			{
				continue;
			}
			TParseTreeNode resultset = (TParseTreeNode) key;
			resultSets.add( resultset );
		}
		return resultSets;
	}

	public static List<TParseTreeNode> getUpdateResultSets( )
	{
		List<TParseTreeNode> resultSets = new ArrayList<TParseTreeNode>( );

		Iterator iter = modelBindingMap.keySet( ).iterator( );
		while ( iter.hasNext( ) )
		{
			Object key = iter.next( );
			if ( !( key instanceof TUpdateSqlStatement ) )
			{
				continue;
			}
			TParseTreeNode resultset = (TParseTreeNode) key;
			resultSets.add( resultset );
		}
		return resultSets;
	}

	public static void addRelation( Relation relation )
	{
		if ( relation != null && !relationHolder.contains( relation ) )
		{
			relationHolder.add( relation );
		}
	}

	public static Relation[] getRelations( )
	{
		return relationHolder.toArray( new Relation[0] );
	}

	public static void reset( )
	{
		relationHolder.clear( );
		modelBindingMap.clear( );
		viewModelBindingMap.clear( );
		insertModelBindingMap.clear( );
		mergeModelBindingMap.clear( );
		updateModelBindingMap.clear( );
		createModelBindingMap.clear( );
		createModelBindingMap.clear( );
		createModelQuickBindingMap.clear( );
		tableSet.clear( );
		tableAliasMap.clear( );
	}

	public static void bindCursorModel( TCursorDeclStmt stmt,
			CursorResultSet resultSet )
	{
		createModelBindingMap.put( stmt.getCursorName( ).toScript( ),
				resultSet );
	}

	public static void bindCursorIndex( TObjectName indexName,
			TObjectName cursorName )
	{
		bindModel( indexName.toScript( ),
				modelBindingMap.get( ( (CursorResultSet) createModelBindingMap
						.get( cursorName.toScript( ) ) )
								.getResultColumnObject( ) ) );
	}

}
