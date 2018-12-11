using System.Xml.Serialization;

namespace gudusoft.gsqlparser.demos.dlineage.model.xml
{

	public class linkTable
	{

        [XmlAttribute]
        public string coordinate { get; set; }

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string tableName { get; set; }

        [XmlAttribute]
        public string tableOwner { get; set; }

        [XmlAttribute]
        public string highlightInfos { get; set; }

        [XmlAttribute]
        public string type { get; set; }

	}

}