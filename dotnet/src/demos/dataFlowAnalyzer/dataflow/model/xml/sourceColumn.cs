using System.Xml.Serialization;

namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model.xml
{

	public class sourceColumn
	{

        [XmlAttribute]
        public string coordinate { get; set; }

        [XmlAttribute]
        public string column { get; set; }

        [XmlAttribute]
        public string id { get; set; }

        [XmlAttribute]
        public string parent_id { get; set; }

        [XmlAttribute]
        public string parent_name { get; set; }

        [XmlAttribute]
        public string value { get; set; }

        [XmlAttribute]
        public string source_name { get; set; }

        [XmlAttribute]
        public string source_id { get; set; }

    }

}