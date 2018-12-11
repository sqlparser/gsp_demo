
package demos.dlineage.dataflow.model;

public class ResultColumnRelationElement implements
		RelationElement<ResultColumn>
{

	private ResultColumn column;

	public ResultColumnRelationElement( ResultColumn column )
	{
		this.column = column;
	}

	@Override
	public ResultColumn getElement( )
	{
		return column;
	}

	@Override
	public int hashCode( )
	{
		final int prime = 31;
		int result = 1;
		result = prime
				* result
				+ ( ( column == null ) ? 0 : column.hashCode( ) );
		return result;
	}

	@Override
	public boolean equals( Object obj )
	{
		if ( this == obj )
			return true;
		if ( obj == null )
			return false;
		if ( getClass( ) != obj.getClass( ) )
			return false;
		ResultColumnRelationElement other = (ResultColumnRelationElement) obj;
		if ( column == null )
		{
			if ( other.column != null )
				return false;
		}
		else if ( !column.equals( other.column ) )
			return false;
		return true;
	}

}
