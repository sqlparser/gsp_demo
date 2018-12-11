namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{


	public class DataFlowRelation : AbstractRelation
	{

		public override RelationType RelationType
		{
			get
			{
				return RelationType.dataflow;
			}
		}
	}

}