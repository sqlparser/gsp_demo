using System.Collections.Generic;

namespace demos.dlineage.model.ddl.schema
{
    using System.Xml.Serialization;

    public class unique
	{
        [XmlAttribute]
        public string name { get; set; }

        [XmlElement("unique-column")]
        public uniqueColumn[] uniqueColumns { get; set; }

	}

}