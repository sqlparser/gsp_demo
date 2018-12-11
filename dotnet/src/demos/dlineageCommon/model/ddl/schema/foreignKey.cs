using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.model.ddl.schema
{
    using System.Xml.Serialization;

    public class foreignKey
	{
        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string foreignTable { get; set; }

        [XmlAttribute]
        public string onDelete { get; set; }

        [XmlAttribute]
        public string onUpdate { get; set; }

        [XmlElement("reference")]
        public reference[] references { get; set; }

	}

}