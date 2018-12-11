
package demos.dlineage.dataflow.model.xml;

import org.simpleframework.xml.Attribute;

import demos.dlineage.util.Pair;

public class column
{

	@Attribute(required = false)
	private String name;

	@Attribute(required = false)
	private String id;

	@Attribute(required = false)
	private String coordinate;

	public String getCoordinate( )
	{
		return coordinate;
	}

	public Pair<Integer, Integer> getStartPos( )
	{
		return PositionUtil.getStartPos( coordinate );
	}

	public Pair<Integer, Integer> getEndPos( )
	{
		return PositionUtil.getEndPos( coordinate );
	}

	public void setCoordinate( String coordinate )
	{
		this.coordinate = coordinate;
	}

	public String getName( )
	{
		return name;
	}

	public void setName( String name )
	{
		this.name = name;
	}

	public String getId( )
	{
		return id;
	}

	public void setId( String id )
	{
		this.id = id;
	}

}
