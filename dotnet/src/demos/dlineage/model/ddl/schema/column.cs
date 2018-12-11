namespace demos.dlineage.model.ddl.schema
{
    using System.Xml.Serialization;

    public class column
	{
        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string primaryKey { get; set; }

        [XmlAttribute]
        public string required { get; set; }

        [XmlAttribute]
        public string type { get; set; }

        [XmlAttribute]
        public string size { get; set; }

        [XmlAttribute(AttributeName = "default")]
        public string defaultValue { get; set; }

        [XmlAttribute]
        public string autoIncrement { get; set; }

        [XmlAttribute]
        public string description { get; set; }
	}

}