using System.Xml.Serialization;

namespace gudusoft.gsqlparser.demos.dlineage.model.xml
{
	public class sourceProcedure
	{
        [XmlAttribute]
        public string owner { get; set; }

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string highlightInfo { get; set; }

        [XmlAttribute]
        public string coordinate { get; set; }

	}

}