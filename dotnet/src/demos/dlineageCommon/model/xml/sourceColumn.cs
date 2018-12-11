namespace gudusoft.gsqlparser.demos.dlineage.model.xml
{
    using System.Xml.Serialization;

    public class sourceColumn
	{

        [XmlAttribute]
        public string coordinate { get; set; }

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string clause { get; set; }

        [XmlAttribute]
        public string tableName { get; set; }

        [XmlAttribute]
        public string tableOwner { get; set; }

        [XmlAttribute]
        public string highlightInfos { get; set; }

        [XmlAttribute]
        public string orphan { get; set; }

	}

}