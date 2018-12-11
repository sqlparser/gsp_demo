
package demos.dlineage.dataflow.model;

import java.util.ArrayList;
import java.util.List;

public abstract class AbstractRelation implements Relation
{

	public static int RELATION_ID = 0;

	private int id;

	protected RelationElement<?> target;
	protected List<RelationElement<?>> sources = new ArrayList<RelationElement<?>>( );

	public AbstractRelation( )
	{
		id = ++RELATION_ID;
	}

	public int getId( )
	{
		return id;
	}

	public RelationElement<?> getTarget( )
	{
		return target;
	}

	public void setTarget( RelationElement<?> target )
	{
		this.target = target;
	}

	public RelationElement<?>[] getSources( )
	{
		return sources.toArray( new RelationElement<?>[0] );
	}

	public void addSource( RelationElement<?> source )
	{
		if ( source != null && !sources.contains( source ) )
		{
			sources.add( source );
		}
	}
}
