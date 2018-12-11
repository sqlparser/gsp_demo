namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model.xml
{
    using System.Xml.Serialization;
    public class targetColumn
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
        public string function { get; set; }


	}

}