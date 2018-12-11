using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.model.xml
{
    using System.Xml.Serialization;

    public class targetColumn
	{

        [XmlAttribute]
        public string alias { get; set; }

        [XmlAttribute]
        public string coordinate { get; set; }

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string aliasCoordinate { get; set; }

        [XmlAttribute]
        public string columnHighlightInfo { get; set; }

        [XmlAttribute]
        public string aliasHighlightInfo { get; set; }

        [XmlElement("sourceColumn")]
        public sourceColumn[] columns { get; set; }

        [XmlElement("linkTable")]
        public linkTable[] linkTables { get; set; }
	}

}