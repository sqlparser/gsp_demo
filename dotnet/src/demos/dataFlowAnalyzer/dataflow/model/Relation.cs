namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

	public interface Relation
	{
		RelationElement Target {get;}

		RelationElement[] Sources {get;}

		RelationType RelationType {get;}
	}

}