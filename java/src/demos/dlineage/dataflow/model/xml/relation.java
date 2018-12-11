
package demos.dlineage.dataflow.model.xml;

import java.util.List;

import org.simpleframework.xml.Attribute;
import org.simpleframework.xml.Element;
import org.simpleframework.xml.ElementList;

public class relation implements Cloneable
{

	@Attribute(required = false)
	private String id;

	@Attribute(required = false)
	private String type;

	@Element(name = "target", required = false)
	private targetColumn target;

	@ElementList(entry = "source", inline = true, required = false)
	private List<sourceColumn> sources;

	public String getId( )
	{
		return id;
	}

	public void setId( String id )
	{
		this.id = id;
	}

	public String getType( )
	{
		return type;
	}

	public void setType( String type )
	{
		this.type = type;
	}

	public targetColumn getTarget( )
	{
		return target;
	}

	public void setTarget( targetColumn target )
	{
		this.target = target;
	}

	public List<sourceColumn> getSources( )
	{
		return sources;
	}

	public void setSources( List<sourceColumn> sources )
	{
		this.sources = sources;
	}

	public boolean isDataFlow( )
	{
		return "dataflow".equals( type );
	}

	@Override
	public Object clone( ) throws CloneNotSupportedException
	{
		return super.clone( );
	}
}
