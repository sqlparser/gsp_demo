namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model.xml
{
    using System;
    using System.Xml.Serialization;

    public class column
	{

        [XmlAttribute]
        public string name { get; set; }

        [XmlAttribute]
        public string id { get; set; }

        [XmlAttribute]
        public string coordinate { get; set; }

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