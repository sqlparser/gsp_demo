namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{

	public class RecordSetRelation : AbstractRelation
	{

		private string aggregateFunction;

		public virtual string AggregateFunction
		{
			get
			{
				return aggregateFunction;
			}
			set
			{
				this.aggregateFunction = value;
			}
		}


		public override RelationType RelationType
		{
			get
			{
				return RelationType.dataflow_recordset;
			}
		}

	}

}