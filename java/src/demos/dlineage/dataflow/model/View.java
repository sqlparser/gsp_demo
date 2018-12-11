
package demos.dlineage.dataflow.model;

import gudusoft.gsqlparser.TSourceToken;
import gudusoft.gsqlparser.stmt.TCreateViewSqlStatement;

import java.util.ArrayList;
import java.util.List;

import demos.dlineage.util.Pair;

public class View
{

	private int id;
	private String name;
	private Pair<Long, Long> startPosition;
	private Pair<Long, Long> endPosition;
	private List<ViewColumn> columns = new ArrayList<ViewColumn>( );
	private TCreateViewSqlStatement viewObject;

	public View( TCreateViewSqlStatement view )
	{
		if ( view == null )
			throw new IllegalArgumentException( "Table arguments can't be null." );

		id = ++Table.TABLE_ID;

		this.viewObject = view;

		TSourceToken startToken = viewObject.getStartToken( );
		TSourceToken endToken = viewObject.getEndToken( );
		if ( viewObject.getViewName( ) != null )
		{
			startToken = viewObject.getViewName( ).getStartToken( );
			endToken = viewObject.getViewName( ).getEndToken( );
			this.name = viewObject.getViewName( ).toString( );
		}
		else
		{
			this.name = "";
			System.err.println( );
			System.err.println( "Can't get view name. View is " );
			System.err.println( view.toString( ) );
		}

		this.startPosition = new Pair<Long, Long>( startToken.lineNo,
				startToken.columnNo );
		this.endPosition = new Pair<Long, Long>( endToken.lineNo,
				endToken.columnNo + endToken.astext.length( ) );

	}

	public int getId( )
	{
		return id;
	}

	public String getName( )
	{
		return name;
	}

	public void setName( String name )
	{
		this.name = name;
	}

	public Pair<Long, Long> getStartPosition( )
	{
		return startPosition;
	}

	public Pair<Long, Long> getEndPosition( )
	{
		return endPosition;
	}

	public List<ViewColumn> getColumns( )
	{
		return columns;
	}

	public void addColumn( ViewColumn column )
	{
		if ( column != null && !this.columns.contains( column ) )
		{
			this.columns.add( column );
		}
	}

	public TCreateViewSqlStatement getViewObject( )
	{
		return viewObject;
	}

	public String getDisplayName( )
	{
		return getName( );
	}
}
