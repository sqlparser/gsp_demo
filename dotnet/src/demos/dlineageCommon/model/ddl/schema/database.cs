using System.Collections.Generic;
using System.Xml.Serialization;

namespace gudusoft.gsqlparser.demos.dlineage.model.ddl.schema
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