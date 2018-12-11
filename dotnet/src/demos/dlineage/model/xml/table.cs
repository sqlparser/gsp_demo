namespace demos.dlineage.model.xml
{
    using System;
    using System.Xml.Serialization;
    public class table
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