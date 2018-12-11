using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model
{


	public abstract class AbstractRelation : Relation
	{
		public abstract RelationType RelationType {get;}

		public static int RELATION_ID = 0;

		private int id;

		protected internal RelationElement target;

        protected internal List<RelationElement> sources = new List<RelationElement>();

		public AbstractRelation()
		{
			id = ++RELATION_ID;
		}

		public virtual int Id
		{
			get
			{
				return id;
			}
		}

		public virtual RelationElement Target
		{
			get
			{
				return target;
			}
			set
			{
				this.target = value;
			}
		}


		public virtual RelationElement[] Sources
		{
			get
			{
				return sources.ToArray();
			}
		}

		public virtual void addSource(RelationElement source)
		{
			if (source != null && !sources.Contains(source))
			{
				sources.Add(source);
			}
		}
	}

}