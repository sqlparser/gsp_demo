namespace demos.dlineage.model.ddl.schema
{
    using System.Xml.Serialization;

    public class indexColumn
	{
        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string size { get; set; }

	}

}