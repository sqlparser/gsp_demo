namespace demos.dlineage.model.xml
{
    using System.Xml.Serialization;

    public class procedure
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