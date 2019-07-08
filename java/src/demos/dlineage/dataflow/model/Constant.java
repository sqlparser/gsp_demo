
package demos.dlineage.dataflow.model;

import gudusoft.gsqlparser.TSourceToken;
import gudusoft.gsqlparser.nodes.TConstant;
import demos.dlineage.util.Pair;

public class Constant
{

	protected int id;

	protected String fullName;
	protected String name;

	protected Pair<Long, Long> startPosition;
	protected Pair<Long, Long> endPosition;

	protected TConstant constant;

	public Constant( TConstant constant )
	{
		if ( constant == null )
			throw new IllegalArgumentException( "Constant arguments can't be null." );

		id = ++ModelBindingManager.get( ).TABLE_COLUMN_ID;

		this.constant = constant;

		TSourceToken startToken = constant.getStartToken( );
		TSourceToken endToken = constant.getEndToken( );

		this.name = constant.toString( );

		this.fullName = constant.toString( );

		this.startPosition = new Pair<Long, Long>( startToken.lineNo,
				startToken.columnNo );
		this.endPosition = new Pair<Long, Long>( endToken.lineNo,
				endToken.columnNo + endToken.astext.length( ) );
	}

	public int getId( )
	{
		return id;
	}

	public String getFullName( )
	{
		return fullName;
	}

	public Pair<Long, Long> getStartPosition( )
	{
		return startPosition;
	}

	public Pair<Long, Long> getEndPosition( )
	{
		return endPosition;
	}

	public TConstant getConstantObject( )
	{
		return constant;
	}

	public String getName( )
	{
		return name;
	}

}
