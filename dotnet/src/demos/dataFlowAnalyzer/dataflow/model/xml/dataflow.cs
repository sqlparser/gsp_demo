using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model.xml
{
    using System.Xml.Serialization;

    public class dataflow
	{
        [XmlElement("relation")]
        public relation[] relations { get; set; }

        [XmlElement("table")]
        public table[] tables { get; set; }

        [XmlElement("view")]
        public table[] views { get; set; }

        [XmlElement("resultset")]
        public table[] resultsets { get; set; }

    }
}