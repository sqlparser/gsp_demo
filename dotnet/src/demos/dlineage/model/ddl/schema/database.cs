using System.Collections.Generic;
using System.Xml.Serialization;

namespace demos.dlineage.model.ddl.schema
{

    [XmlRoot("database")]
    public class database
	{
        [XmlAttribute]
        public string name { get; set; }

        [XmlElement("table")]
        public table[] tables { get; set; }
	}

}