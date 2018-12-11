using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model.xml
{
    using System.Xml.Serialization;

    public class relation
	{

        [XmlAttribute]
        public string id { get; set; }

        [XmlAttribute]
        public string type { get; set; }

        [XmlAttribute("target")]
        public targetColumn target { get; set; }

        [XmlAttribute("source")]
        public sourceColumn[] sources { get; set; }


		public virtual bool isDataFlow
		{
			get
			{
				return "dataflow".Equals(type);
			}
		}

	}

}