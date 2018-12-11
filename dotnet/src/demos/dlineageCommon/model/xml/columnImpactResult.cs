using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.model.xml
{
    using System;
    using System.Xml.Serialization;

    [XmlRoot("columnImpactResult")]
    public class columnImpactResult
	{
        [XmlElement("targetColumn")]
        public targetColumn[] columns { get; set; }

        [XmlElement("table")]
        public table[] tables { get; set; }
	}

}