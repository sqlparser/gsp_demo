using System.Collections.Generic;

namespace demos.dlineage.model.xml
{

    using System.Xml.Serialization;

    public class targetProcedure
	{

        [XmlAttribute]
        public string owner { get; set; }

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string highlightInfo { get; set; }

        [XmlAttribute]
        public string coordinate { get; set; }

        [XmlElement("sourceProcedure")]
        public sourceProcedure[] sourceProcedures { get; set; }
	}

}