
package demo.view.dlineage;

import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Set;

import demo.view.dlineage.utils.StringEscapeUtils;
import demos.dlineage.dataflow.model.RelationType;
import demos.dlineage.dataflow.model.xml.column;
import demos.dlineage.dataflow.model.xml.dataflow;
import demos.dlineage.dataflow.model.xml.relation;
import demos.dlineage.dataflow.model.xml.sourceColumn;
import demos.dlineage.dataflow.model.xml.table;
import demos.dlineage.dataflow.model.xml.targetColumn;
import demos.dlineage.util.Pair;
import demos.dlineage.util.SQLUtil;

public class DataFlowGraph
{

	public Pair<String, Boolean> generateDataFlowGraph( dataflow dataflow,
			boolean showLinkOnly, RelationType showRelationType,
			Map<String, String> tooltipMap )
	{
		try
		{
			return convertMetaInfoToGraphXML( dataflow,
					showLinkOnly,
					showRelationType,
					tooltipMap );
		}
		catch ( Exception e2 )
		{
			e2.printStackTrace( );
		}

		return null;
	}

	private List<Object> nodes = new ArrayList<Object>( );

	public List<Object> getNodes( )
	{
		return nodes;
	}

	private Pair<String, Boolean> convertMetaInfoToGraphXML( dataflow dataflow,
			boolean showLinkOnly, RelationType showRelationType,
			Map<String, String> tooltipMap ) throws IOException
	{
		nodes.clear( );

		boolean containGroup = false;

		String columnContent = SQLUtil.getInputStreamContent( getClass( ).getResourceAsStream( "/demo/view/dlineage/resource/column.template" ),
				false );
		String tableContent = SQLUtil.getInputStreamContent( getClass( ).getResourceAsStream( "/demo/view/dlineage/resource/table.template" ),
				false );
		String linkContent = SQLUtil.getInputStreamContent( getClass( ).getResourceAsStream( "/demo/view/dlineage/resource/link.template" ),
				false );
		String graphContent = SQLUtil.getInputStreamContent( getClass( ).getResourceAsStream( "/demo/view/dlineage/resource/graph.template" ),
				false );

		StringBuffer tableFullBuffer = new StringBuffer( );

		List<table> tables = new ArrayList<table>( );
		if ( dataflow.getTables( ) != null )
			tables.addAll( dataflow.getTables( ) );
		if ( dataflow.getViews( ) != null )
			tables.addAll( dataflow.getViews( ) );
		if ( dataflow.getResultsets( ) != null )
			tables.addAll( dataflow.getResultsets( ) );

		Map<String, Map<String, List<column>>> tableColumns = new HashMap<String, Map<String, List<column>>>( );
		Set<String> columnSet = new HashSet<String>( );
		Set<String> linkColumnSet = new HashSet<String>( );

		for ( int j = 0; j < tables.size( ); j++ )
		{
			table currentTable = tables.get( j );
			if ( currentTable.getColumns( ) == null
					|| currentTable.getColumns( ).isEmpty( ) )
				continue;

			nodes.add( currentTable );

			StringBuffer columnBuffer = new StringBuffer( );

			if ( currentTable.isTable( ) )
			{
				Map<String, List<column>> columnMap = new LinkedHashMap<String, List<column>>( );
				tableColumns.put( currentTable.getId( ), columnMap );
				for ( int k = 0; k < currentTable.getColumns( ).size( ); k++ )
				{
					column column = currentTable.getColumns( ).get( k );
					String columnLabel = column.getName( );
					List<column> columns;
					if ( !columnMap.containsKey( columnLabel ) )
					{
						columns = new ArrayList<column>( );
						columnMap.put( columnLabel, columns );
					}
					else
					{
						columns = columnMap.get( columnLabel );
					}
					columns.add( column );
				}

				Iterator<Entry<String, List<column>>> iterator = columnMap.entrySet( )
						.iterator( );
				for ( int k = 0; k < columnMap.size( ); k++ )
				{
					Entry<String, List<column>> entry = iterator.next( );
					nodes.add( entry.getValue( ) );
					String content = new String( columnContent );

					String columnId = "column_"
							+ currentTable.getId( )
							+ "_index_"
							+ k;
					columnSet.add( "\"" + columnId + "\"" );

					content = content.replace( "{columnId}", columnId );
					content = content.replace( "{columnPosY}",
							String.valueOf( 35 * k ) );
					String columnLabel = entry.getKey( );
					if ( columnLabel.length( ) > 25 )
					{
						String shortLabel = getShortLabel( columnLabel, 25 );
						if ( tooltipMap != null )
						{
							tooltipMap.put( shortLabel.replace( "\r\n", "\n" )
									.replace( "\n", " " ), columnLabel );
						}
						columnLabel = shortLabel;

					}
					content = content.replace( "{columnLabel}",
							StringEscapeUtils.escapeXml( columnLabel ) );
					columnBuffer.append( content ).append( "\n" );
				}
			}
			else
			{
				for ( int k = 0; k < currentTable.getColumns( ).size( ); k++ )
				{
					column column = currentTable.getColumns( ).get( k );
					nodes.add( column );
					String content = new String( columnContent );

					String columnParentId = currentTable.getId( );
					String columnId = column.getId( );
					columnId = convertColumnId( tableColumns,
							columnId,
							columnParentId );

					columnId = "column_" + columnId;
					columnSet.add( "\"" + columnId + "\"" );

					content = content.replace( "{columnId}", columnId );
					content = content.replace( "{columnPosY}",
							String.valueOf( 35 * k ) );
					String columnLabel = column.getName( );
					if ( columnLabel.length( ) > 25 )
					{
						String shortLabel = getShortLabel( columnLabel, 25 );
						if ( tooltipMap != null )
						{
							tooltipMap.put( shortLabel.replace( "\r\n", "\n" )
									.replace( "\n", " " ), columnLabel );
						}
						columnLabel = shortLabel;

					}
					content = content.replace( "{columnLabel}",
							StringEscapeUtils.escapeXml( columnLabel ) );
					columnBuffer.append( content ).append( "\n" );
				}
			}
			String content = new String( tableContent );
			content = content.replace( "{tableId}",
					"table_" + currentTable.getId( ) );

			String tableLabel = currentTable.getName( );
			if ( tableLabel.length( ) > 25 )
			{
				String shortLabel = getShortLabel( tableLabel, 25 );
				if ( tooltipMap != null )
				{
					tooltipMap.put( shortLabel.replace( "\r\n", "\n" )
							.replace( "\n", " " ),
							tableLabel );
				}
				tableLabel = shortLabel;

			}

			content = content.replace( "{tableLabel}",
					StringEscapeUtils.escapeXml( tableLabel ) );
			content = content.replace( "{columns}", columnBuffer.toString( ) );
			if ( currentTable.isView( ) )
			{
				content = content.replace( "{contentColor}", "#ff99cc" );
				content = content.replace( "{labelColor}", "#ffccff" );
			}
			else if ( currentTable.isResultSet( ) )
			{
				content = content.replace( "{contentColor}", "#009944" );
				content = content.replace( "{labelColor}", "#aaffcc" );
			}
			else
			{
				content = content.replace( "{contentColor}", "#9999ff" );
				content = content.replace( "{labelColor}", "#DBDADA" );
			}
			tableFullBuffer.append( content ).append( "\n" );

			containGroup = true;
		}

		StringBuffer linkBuffer = new StringBuffer( );

		List<relation> relations = dataflow.getRelations( );
		if ( relations != null )
		{
			List<String> links = new ArrayList<String>( );
			for ( int i = 0; i < relations.size( ); i++ )
			{
				relation relation = relations.get( i );
				if ( !relation.getType( ).equals( showRelationType.name( ) ) )
				{
					if ( showRelationType.equals( RelationType.join )
							&& relation.getType( )
									.equals( RelationType.dataflow.name( ) ) )
					{
						if ( !traceJoin( relation, relations, tableColumns, 0 ) )
						{
							continue;
						}
					}
					else
					{
						continue;
					}
				}
				targetColumn target = relation.getTarget( );
				String targetColumnId = target.getId( );
				String targetParentId = target.getParent_id( );

				targetColumnId = convertColumnId( tableColumns,
						targetColumnId,
						targetParentId );

				if ( relation.getSources( ) != null )
				{
					List<sourceColumn> linkTables = relation.getSources( );
					for ( int k = 0; k < linkTables.size( ); k++ )
					{
						sourceColumn source = linkTables.get( k );

						String sourceColumnId = source.getId( );
						String sourceParentId = source.getParent_id( );
						sourceColumnId = convertColumnId( tableColumns,
								sourceColumnId,
								sourceParentId );

						String content = linkContent;

						if ( relation.getType( )
								.equals( RelationType.join.name( ) ) )
						{
							content = content.replace( "type=\"line\"",
									"type=\"dashed\"" );
						}

						String sourceId = "column_" + sourceColumnId;
						String targetId = "column_" + targetColumnId;
						content = content.replace( "{sourceId}", sourceId );
						content = content.replace( "{targetId}", targetId );

						if ( columnSet.contains( "\"" + sourceId + "\"" )
								&& columnSet.contains( "\"" + targetId + "\"" ) )
						{
							String temp = content;
							if ( !links.contains( temp ) )
							{
								links.add( temp );

								linkColumnSet.add( "\"" + sourceId + "\"" );
								linkColumnSet.add( "\"" + targetId + "\"" );

								content = content.replace( "{linkId}", "link_"
										+ relation.getId( )
										+ "_"
										+ sourceColumnId
										+ "_"
										+ targetColumnId );
								linkBuffer.append( content ).append( "\n" );
							}
						}
						else
						{
							System.err.println( "Can't get "
									+ "link_"
									+ relation.getId( )
									+ "_"
									+ sourceColumnId
									+ "_"
									+ targetColumnId );
						}
					}
				}
			}
		}

		String linkString = linkBuffer.toString( );

		if ( showLinkOnly && linkString.trim( ).length( ) > 0 )
		{
			nodes.clear( );
			StringBuffer tableLinkBuffer = new StringBuffer( );
			int showColumnCount = 0;
			for ( int j = 0; j < tables.size( ); j++ )
			{
				table currentTable = tables.get( j );
				if ( currentTable.getColumns( ) == null
						|| currentTable.getColumns( ).isEmpty( ) )
					continue;
				nodes.add( currentTable );
				StringBuffer columnBuffer = new StringBuffer( );
				if ( currentTable.isTable( ) )
				{
					Map<String, List<column>> columnMap = new LinkedHashMap<String, List<column>>( );
					for ( int k = 0; k < currentTable.getColumns( ).size( ); k++ )
					{
						column column = currentTable.getColumns( ).get( k );
						String columnLabel = column.getName( );
						List<column> columns;
						if ( !columnMap.containsKey( columnLabel ) )
						{
							columns = new ArrayList<column>( );
							columnMap.put( columnLabel, columns );
						}
						else
						{
							columns = columnMap.get( columnLabel );
						}
						columns.add( column );
					}

					Iterator<Entry<String, List<column>>> iterator = columnMap.entrySet( )
							.iterator( );
					for ( int k = 0; k < columnMap.size( ); k++ )
					{
						Entry<String, List<column>> entry = iterator.next( );
						String columnId = "column_"
								+ currentTable.getId( )
								+ "_index_"
								+ k;
						if ( currentTable.isTable( )
								&& !linkColumnSet.contains( "\""
										+ columnId
										+ "\"" ) )
							continue;

						nodes.add( entry.getValue( ) );

						String content = new String( columnContent );
						content = content.replace( "{columnId}", columnId );
						content = content.replace( "{columnPosY}",
								String.valueOf( 35 * showColumnCount ) );
						String columnLabel = entry.getKey( );
						if ( columnLabel.length( ) > 25 )
						{
							String shortLabel = getShortLabel( columnLabel, 25 );
							if ( tooltipMap != null )
							{
								tooltipMap.put( shortLabel.replace( "\r\n",
										"\n" ).replace( "\n", " " ),
										columnLabel );
							}
							columnLabel = shortLabel;

						}
						content = content.replace( "{columnLabel}",
								StringEscapeUtils.escapeXml( columnLabel ) );
						columnBuffer.append( content ).append( "\n" );
						showColumnCount++;
					}
				}
				else
				{
					for ( int k = 0; k < currentTable.getColumns( ).size( ); k++ )
					{
						column column = currentTable.getColumns( ).get( k );

						String columnParentId = currentTable.getId( );
						String columnId = column.getId( );
						columnId = convertColumnId( tableColumns,
								columnId,
								columnParentId );

						columnId = "column_" + columnId;
						if ( !currentTable.isTable( )
								&& !linkColumnSet.contains( "\""
										+ columnId
										+ "\"" ) )
							continue;

						nodes.add( column );

						String content = new String( columnContent );
						content = content.replace( "{columnId}", columnId );
						content = content.replace( "{columnPosY}",
								String.valueOf( 35 * showColumnCount ) );
						String columnLabel = column.getName( );
						if ( columnLabel.length( ) > 25 )
						{
							String shortLabel = getShortLabel( columnLabel, 25 );
							if ( tooltipMap != null )
							{
								tooltipMap.put( shortLabel.replace( "\r\n",
										"\n" ).replace( "\n", " " ),
										columnLabel );
							}
							columnLabel = shortLabel;

						}
						content = content.replace( "{columnLabel}",
								StringEscapeUtils.escapeXml( columnLabel ) );
						columnBuffer.append( content ).append( "\n" );
						showColumnCount++;
					}
				}

				if ( columnBuffer.toString( ).trim( ).length( ) == 0 )
				{
					nodes.remove( currentTable );
					continue;
				}

				String content = new String( tableContent );
				content = content.replace( "{tableId}",
						"table_" + currentTable.getId( ) );

				String tableLabel = currentTable.getName( );
				if ( tableLabel.length( ) > 25 )
				{
					String shortLabel = getShortLabel( tableLabel, 25 );
					if ( tooltipMap != null )
					{
						tooltipMap.put( shortLabel.replace( "\r\n", "\n" )
								.replace( "\n", " " ), tableLabel );
					}
					tableLabel = shortLabel;
				}

				content = content.replace( "{tableLabel}",
						StringEscapeUtils.escapeXml( tableLabel ) );
				content = content.replace( "{columns}", columnBuffer.toString( ) );
				if ( currentTable.isView( ) )
				{
					content = content.replace( "{contentColor}", "#ff99cc" );
					content = content.replace( "{labelColor}", "#ffccff" );
				}
				else if ( currentTable.isResultSet( ) )
				{
					content = content.replace( "{contentColor}", "#009944" );
					content = content.replace( "{labelColor}", "#aaffcc" );
				}
				else
				{
					content = content.replace( "{contentColor}", "#9999ff" );
					content = content.replace( "{labelColor}", "#DBDADA" );
				}
				tableLinkBuffer.append( content ).append( "\n" );
				containGroup = true;
			}

			graphContent = graphContent.replace( "{nodes}",
					tableLinkBuffer.toString( ) );
		}
		else
		{
			graphContent = graphContent.replace( "{nodes}",
					tableFullBuffer.toString( ) );
		}

		graphContent = graphContent.replace( "{links}", linkBuffer.toString( ) );
		return new Pair<String, Boolean>( graphContent, containGroup );
	}

	private boolean traceJoin( relation relation, List<relation> relations,
			Map<String, Map<String, List<column>>> tableColumns, int level )
	{
		targetColumn target = relation.getTarget( );
		String targetColumnId = target.getId( );
		String targetParentId = target.getParent_id( );

		targetColumnId = convertColumnId( tableColumns,
				targetColumnId,
				targetParentId );

		for ( int i = 0; i < relations.size( ); i++ )
		{
			relation tempRelation = relations.get( i );
			if ( relation == tempRelation )
				continue;

			String tempColumnId = tempRelation.getTarget( ).getId( );
			String tempParentId = tempRelation.getTarget( ).getParent_id( );
			tempColumnId = convertColumnId( tableColumns,
					tempColumnId,
					tempParentId );
			if ( tempColumnId.equals( targetColumnId ) )
			{
				if ( tempRelation.getType( ).equals( RelationType.join.name( ) ) )
				{
					return true;
				}
				else if ( tempRelation.getType( )
						.equals( RelationType.dataflow.name( ) ) )
				{
					if ( level < 10
							&& traceJoin( tempRelation,
									relations,
									tableColumns,
									level + 1 ) )
					{
						return true;
					}
				}
			}

			List<sourceColumn> sources = tempRelation.getSources( );
			for ( int j = 0; j < sources.size( ); j++ )
			{
				tempColumnId = sources.get( j ).getId( );
				tempParentId = sources.get( j ).getParent_id( );
				tempColumnId = convertColumnId( tableColumns,
						tempColumnId,
						tempParentId );
				if ( tempColumnId.equals( targetColumnId ) )
				{
					if ( tempRelation.getType( )
							.equals( RelationType.join.name( ) ) )
					{
						return true;
					}
					else if ( tempRelation.getType( )
							.equals( RelationType.dataflow.name( ) ) )
					{
						if ( traceJoin( tempRelation,
								relations,
								tableColumns,
								0 ) )
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	private String convertColumnId(
			Map<String, Map<String, List<column>>> tableColumns,
			String columnId, String parentId )
	{
		if ( tableColumns.containsKey( parentId ) )
		{
			Map<String, List<column>> columnMap = tableColumns.get( parentId );
			Iterator<List<column>> iter = columnMap.values( ).iterator( );
			for ( int i = 0; i < columnMap.size( ); i++ )
			{
				List<column> columns = iter.next( );
				for ( int j = 0; j < columns.size( ); j++ )
				{
					column column = columns.get( j );
					if ( column.getId( ).equals( columnId ) )
					{
						return parentId + "_index_" + i;
					}
				}
			}
		}
		return parentId + "_id_" + columnId;
	}

	private String getShortLabel( String label, int length )
	{
		int index = length / 2 - 1;
		return ( label.substring( 0, index - 1 ) + "..." + label.substring( label.length( )
				- ( index + 1 ) ) );
	}
}
