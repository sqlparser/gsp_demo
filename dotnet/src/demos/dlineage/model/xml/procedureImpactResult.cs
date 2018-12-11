using System.Collections.Generic;
using System.Xml.Serialization;

namespace demos.dlineage.model.xml
{
    [XmlRoot("procedureImpactResult")]
    public class procedureImpactResult
	{

        [XmlElement("targetProcedure")]
        public targetProcedure[] targetProcedures { get; set; }

        [XmlElement("procedure")]
        public procedure[] procedures { get; set; }

	}

}