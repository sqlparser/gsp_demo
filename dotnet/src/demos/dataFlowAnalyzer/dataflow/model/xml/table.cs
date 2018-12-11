using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model.xml
{
    using System;
    using System.Xml.Serialization;

    public class table
	{

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string id { get; set; }

        [XmlAttribute]
        public string type { get; set; }

        [XmlAttribute]
        public string coordinate { get; set; }

        [XmlAttribute]
        public string alias { get; set; }

        [XmlAttribute("column")]
        public column[] columns { get; set; }


		public virtual bool isView
		{
			get
			{
				return "view".Equals(type);
			}
		}

		public virtual bool isTable
		{
			get
			{
				return "table".Equals(type);
			}
		}

		public virtual bool isResultSet
		{
			get
			{
				return !string.ReferenceEquals(type, null) && !isView && !isTable;
			}
		}
        public virtual Tuple<int, int> StartPos
        {
            get
            {
                return PositionUtil.getStartPos(coordinate);
            }
        }

        public virtual Tuple<int, int> EndPos
        {
            get
            {
                return PositionUtil.getEndPos(coordinate);
            }
        }

    }

}