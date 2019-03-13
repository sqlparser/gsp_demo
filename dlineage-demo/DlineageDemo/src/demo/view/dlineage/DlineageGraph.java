
package demo.view.dlineage;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.Map;

import demo.view.dlineage.utils.StringEscapeUtils;
import demos.dlineage.Dlineage;
import demos.dlineage.model.ddl.schema.column;
import demos.dlineage.model.ddl.schema.database;
import demos.dlineage.model.ddl.schema.table;
import demos.dlineage.model.metadata.ProcedureMetaData;
import demos.dlineage.model.xml.columnImpactResult;
import demos.dlineage.model.xml.linkTable;
import demos.dlineage.model.xml.sourceColumn;
import demos.dlineage.model.xml.sourceProcedure;
import demos.dlineage.model.xml.targetColumn;
import demos.dlineage.model.xml.targetProcedure;
import demos.dlineage.util.Pair;
import demos.dlineage.util.SQLUtil;

public class DlineageGraph
{

	public Pair<String, Boolean> generateDlineageGraph( Dlineage dlineage,
			columnImpactResult result, boolean showLinkOnly )
	{
		return generateDlineageGraph( dlineage, result, showLinkOnly, null );
	}

	public Pair<String, Boolean> generateDlineageGraph( Dlineage dlineage,
			columnImpactResult result, boolean showLinkOnly,
			Map<String, String> tooltipMap )
	{
		try
		{
			return convertMetaInfoToGraphXML( dlineage,
					result,
					showLinkOnly,
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

	private Pair<String, Boolean> convertMetaInfoToGraphXML( Dlineage dlineage,
			columnImpactResult impactResult, boolean showLinkOnly,
			Map<String, String> tooltipMap ) throws IOException
	{
		nodes.clear( );
		
		if ( dlineage == null
				|| impactResult == null
				|| dlineage.getDataMetaInfos( ) == null )
			return null;

		boolean containGroup = false;

		String columnContent = SQLUtil.getInputStreamContent( getClass( ).getResourceAsStream( "/demo/view/dlineage/resource/column.template" ),
				false );
		String tableContent = SQLUtil.getInputStreamContent( getClass( ).getResourceAsStream( "/demo/view/dlineage/resource/table.template" ),
				false );
		String procedureContent = SQLUtil.getInputStreamContent( getClass( ).getResourceAsStream( "/demo/view/dlineage/resource/procedure.template" ),
				false );
		String linkContent = SQLUtil.getInputStreamContent( getClass( ).getResourceAsStream( "/demo/view/dlineage/resource/link.template" ),
				false );
		String graphContent = SQLUtil.getInputStreamContent( getClass( ).getResourceAsStream( "/demo/view/dlineage/resource/graph.template" ),
				false );

		StringBuffer tableFullBuffer = new StringBuffer( );
		for ( int i = 0; i < dlineage.getDataMetaInfos( ).length; i++ )
		{
			database db = dlineage.getDataMetaInfos( )[i];
			for ( int j = 0; j < db.getTables( ).size( ); j++ )
			{
				table currentTable = db.getTables( ).get( j );
				if ( currentTable.getColumns( ) == null
						|| currentTable.getColumns( ).isEmpty( ) )
					continue;

				nodes.add( currentTable );

				StringBuffer columnBuffer = new StringBuffer( );
				for ( int k = 0; k < currentTable.getColumns( ).size( ); k++ )
				{
					column column = currentTable.getColumns( ).get( k );
					nodes.add( column );
					String content = new String( columnContent );
					content = content.replace( "{columnId}",
							StringEscapeUtils.escapeXml( SQLUtil.trimObjectName( SQLUtil.trimObjectName( db.getName( ) )
									+ "."
									+ currentTable.getName( )
									+ "."
									+ column.getName( ) ) ) );
					content = content.replace( "{columnPosY}",
							String.valueOf( 15 * k ) );
					String columnLabel = column.getName( )
							+ ( column.getType( ) != null ? ( " : " + column.getType( ) )
									: "" );
					if ( columnLabel.length( ) > 27 )
					{
						String shortLabel = getShortLabel( columnLabel, 27 );
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
				String content = new String( tableContent );
				content = content.replace( "{tableId}",
						StringEscapeUtils.escapeXml( db.getName( )
								+ "."
								+ currentTable.getName( ) ) );

				String tableLabel = currentTable.getName( );
				if ( tableLabel.length( ) > 27 )
				{
					String shortLabel = getShortLabel( tableLabel, 27 );
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
				if ( currentTable.getIsView( ) != null
						&& Boolean.valueOf( currentTable.getIsView( ) ) == Boolean.TRUE )
				{
					content = content.replace( "{contentColor}", "#ff99cc" );
					content = content.replace( "{labelColor}", "#ffccff" );
				}
				else
				{
					content = content.replace( "{contentColor}", "#9999ff" );
					content = content.replace( "{labelColor}", "#ccccff" );
				}
				tableFullBuffer.append( content ).append( "\n" );

				containGroup = true;
			}
		}

		for ( int i = 0; i < dlineage.getProcedures( ).second.size( ); i++ )
		{

			ProcedureMetaData procedure = dlineage.getProcedures( ).second.get( i );
			String content = getProcedureContent( tooltipMap,
					procedureContent,
					procedure );
			tableFullBuffer.append( content ).append( "\n" );
		}

		String tableFullString = tableFullBuffer.toString( );

		StringBuffer linkBuffer = new StringBuffer( );

		int linkCount = 0;

		List<targetColumn> targetColumns = impactResult.getColumns( );
		if ( impactResult != null && targetColumns != null )
		{
			List<String> links = new ArrayList<String>( );
			for ( int i = 0; i < targetColumns.size( ); i++ )
			{
				targetColumn target = targetColumns.get( i );
				if ( target.getLinkTable( ) != null
						&& target.getColumns( ) != null )
				{
					List<linkTable> linkTables = target.getLinkTable( );
					for ( int k = 0; k < linkTables.size( ); k++ )
					{
						linkTable link = linkTables.get( k );
						for ( int j = 0; j < target.getColumns( ).size( ); j++ )
						{
							sourceColumn source = target.getColumns( ).get( j );

							if ( "true".equals( source.getOrphan( ) ) )
								continue;

							if ( source.getName( ) == null
									|| link.getName( ) == null )
								continue;

							if ( source.getClause( ) != null
									&& ( "select".equalsIgnoreCase( source.getClause( ) ) || "assign".equalsIgnoreCase( source.getClause( ) ) ) )
							{
								String sourceColumnId = StringEscapeUtils.escapeXml( SQLUtil.trimObjectName( ( source.getTableOwner( ) != null ? source.getTableOwner( )
										: "unknown" )
										+ "."
										+ source.getTableName( )
										+ "."
										+ ( source.getName( ).indexOf( "." ) != -1 ? source.getName( )
												.substring( source.getName( )
														.lastIndexOf( "." ) + 1 )
												: source.getName( ) ) ) );
								String targetColumnId = StringEscapeUtils.escapeXml( SQLUtil.trimObjectName( ( link.getTableOwner( ) != null ? link.getTableOwner( )
										: "unknown" )
										+ "."
										+ link.getTableName( )
										+ "."
										+ ( link.getName( ) ) ) );

								if ( tableFullString.contains( sourceColumnId )
										&& tableFullString.contains( targetColumnId ) )
								{
									String content = linkContent;
									content = content.replace( "{sourceId}",
											sourceColumnId );
									content = content.replace( "{targetId}",
											targetColumnId );

									String temp = content;
									if ( !links.contains( temp ) )
									{
										links.add( temp );
										content = content.replace( "{linkId}",
												"link" + ( ++linkCount ) );
										linkBuffer.append( content )
												.append( "\n" );
									}
								}
							}
						}
					}
				}
			}
		}

		List<targetProcedure> targetProcedures = dlineage.getProcedures( ).first.getTargetProcedures( );
		if ( targetProcedures != null )
		{
			List<String> links = new ArrayList<String>( );
			for ( int i = 0; i < targetProcedures.size( ); i++ )
			{
				targetProcedure target = targetProcedures.get( i );
				if ( target.getSourceProcedures( ) != null )
				{
					for ( int j = 0; j < target.getSourceProcedures( ).size( ); j++ )
					{
						sourceProcedure source = target.getSourceProcedures( )
								.get( j );

						if ( source.getName( ) == null )
							continue;

						String sourceId = StringEscapeUtils.escapeXml( SQLUtil.trimObjectName( ( source.getOwner( ) != null ? source.getOwner( )
								+ "."
								+ source.getName( )
								: source.getName( ) ) ) );
						String targetId = StringEscapeUtils.escapeXml( SQLUtil.trimObjectName( ( target.getOwner( ) != null ? target.getOwner( )
								+ "."
								+ target.getName( )
								: target.getName( ) ) ) );

						if ( tableFullString.contains( sourceId )
								&& tableFullString.contains( targetId ) )
						{
							String content = new String( linkContent );
							content = content.replace( "{sourceId}", sourceId );
							content = content.replace( "{targetId}", targetId );

							String temp = content;
							if ( !links.contains( temp ) )
							{
								links.add( temp );
								content = content.replace( "{linkId}", "link"
										+ ( ++linkCount ) );
								linkBuffer.append( content ).append( "\n" );
							}
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
			for ( int i = 0; i < dlineage.getDataMetaInfos( ).length; i++ )
			{
				database db = dlineage.getDataMetaInfos( )[i];
				int showColumnCount = 0;
				for ( int j = 0; j < db.getTables( ).size( ); j++ )
				{
					table currentTable = db.getTables( ).get( j );
					if ( currentTable.getColumns( ) == null
							|| currentTable.getColumns( ).isEmpty( ) )
						continue;
					nodes.add( currentTable );
					StringBuffer columnBuffer = new StringBuffer( );
					for ( int k = 0; k < currentTable.getColumns( ).size( ); k++ )
					{
						column column = currentTable.getColumns( ).get( k );
						String columnId = StringEscapeUtils.escapeXml( SQLUtil.trimObjectName( SQLUtil.trimObjectName( db.getName( ) )
								+ "."
								+ currentTable.getName( )
								+ "."
								+ column.getName( ) ) );
						if ( !linkString.contains( columnId )
								&& !Boolean.valueOf( currentTable.getIsView( ) ) == Boolean.TRUE )
							continue;

						nodes.add( column );

						String content = new String( columnContent );
						content = content.replace( "{columnId}", columnId );
						content = content.replace( "{columnPosY}",
								String.valueOf( 15 * showColumnCount ) );
						String columnLabel = column.getName( )
								+ ( column.getType( ) != null ? ( " : " + column.getType( ) )
										: "" );
						if ( columnLabel.length( ) > 27 )
						{
							String shortLabel = getShortLabel( columnLabel, 27 );
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

					if ( columnBuffer.toString( ).trim( ).length( ) == 0 )
					{
						nodes.remove( currentTable );
						continue;
					}

					String content = new String( tableContent );
					content = content.replace( "{tableId}",
							StringEscapeUtils.escapeXml( db.getName( )
									+ "."
									+ currentTable.getName( ) ) );

					String tableLabel = currentTable.getName( );
					if ( tableLabel.length( ) > 27 )
					{
						String shortLabel = getShortLabel( tableLabel, 27 );
						if ( tooltipMap != null )
						{
							tooltipMap.put( shortLabel.replace( "\r\n", "\n" )
									.replace( "\n", " " ), tableLabel );
						}
						tableLabel = shortLabel;
					}

					content = content.replace( "{tableLabel}",
							StringEscapeUtils.escapeXml( tableLabel ) );
					content = content.replace( "{columns}",
							columnBuffer.toString( ) );
					if ( currentTable.getIsView( ) != null
							&& Boolean.valueOf( currentTable.getIsView( ) ) == Boolean.TRUE )
					{
						content = content.replace( "{contentColor}", "#ff99cc" );
						content = content.replace( "{labelColor}", "#ffccff" );
					}
					else
					{
						content = content.replace( "{contentColor}", "#9999ff" );
						content = content.replace( "{labelColor}", "#ccccff" );
					}
					tableLinkBuffer.append( content ).append( "\n" );
					containGroup = true;
				}
			}

			for ( int i = 0; i < dlineage.getProcedures( ).second.size( ); i++ )
			{

				ProcedureMetaData procedure = dlineage.getProcedures( ).second.get( i );
				String content = getProcedureContent( tooltipMap,
						procedureContent,
						procedure );
				tableLinkBuffer.append( content ).append( "\n" );
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

	private String getProcedureContent( Map<String, String> tooltipMap,
			String procedureContent, ProcedureMetaData procedure )
	{
		String content = new String( procedureContent );
		content = content.replace( "{procedureId}",
				StringEscapeUtils.escapeXml( SQLUtil.trimObjectName( procedure.getFullName( ) ) ) );

		String procedureLabel = procedure.getDisplayFullName( );
		if ( procedureLabel.length( ) > 30 )
		{
			String shortLabel = getShortLabel( procedureLabel, 30 );
			if ( tooltipMap != null )
			{
				tooltipMap.put( shortLabel.replace( "\r\n", "\n" )
						.replace( "\n", " " ),
						procedureLabel );
			}
			procedureLabel = shortLabel;

		}
		content = content.replace( "{procedureLabel}",
				StringEscapeUtils.escapeXml( procedureLabel ) );

		if ( procedure.isFunction( ) )
		{
			content = content.replace( "{contentColor}", "#48FF48" );
			content = content.replace( "{labelColor}", "#ffccff" );
		}
		else if ( procedure.isTrigger( ) )
		{
			content = content.replace( "{contentColor}", "#FF8080" );
			content = content.replace( "{labelColor}", "#ffccff" );
		}
		else
		{

			content = content.replace( "{contentColor}", "#ffac84" );
			content = content.replace( "{labelColor}", "#ccccff" );
		}
		return content;
	}

	private String getShortLabel( String label, int length )
	{
		int index = length / 2 - 1;
		return label.substring( 0, index - 1 )
				+ "..."
				+ label.substring( label.length( ) - ( index + 1 ) );
	}
}
