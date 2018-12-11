namespace gudusoft.gsqlparser.demos.dlineage.model.ddl.schema
{
    using System.Xml.Serialization;

    public class reference
	{
        [XmlAttribute]
        public string foreign { get; set; }

        [XmlAttribute]
        public string local { get; set; }

	}

}