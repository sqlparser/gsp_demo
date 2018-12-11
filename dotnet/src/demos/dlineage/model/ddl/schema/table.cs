using System;
using System.Collections.Generic;

namespace demos.dlineage.model.ddl.schema
{
    using System.Xml.Serialization;

    public class table : IComparable<table>
	{

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string isView { get; set; }

        [XmlAttribute]
        public string alias { get; set; }

        [XmlAttribute]
        public string description { get; set; }

        [XmlElement("column")]
        public column[] columns { get; set; }

        [XmlElement("foreign-key")]
        public foreignKey[] foreignKeys { get; set; }

        [XmlElement("index")]
        public index[] indices { get; set; }

        [XmlElement("unique")]
        public unique[] uniques { get; set; }

		public virtual int CompareTo(table o)
		{
			if ((string.ReferenceEquals(this.isView, null) || bool.Parse(this.isView) == false) && (!string.ReferenceEquals(o.isView, null) && bool.Parse(o.isView) == true))
			{
				return -1;
			}
			if ((!string.ReferenceEquals(this.isView, null) && bool.Parse(this.isView) == true) && (string.ReferenceEquals(o.isView, null) || bool.Parse(o.isView) == false))
			{
				return 1;
			}
			return this.name.CompareTo(o.name);
		}
	}

}