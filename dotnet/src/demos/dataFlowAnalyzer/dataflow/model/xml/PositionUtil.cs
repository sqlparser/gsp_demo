namespace gudusoft.gsqlparser.demos.dlineage.dataflow.model.xml
{
    using System;

    public class PositionUtil
	{

        public static Tuple<int, int> getStartPos(string coordinate)
        {
            if (!string.ReferenceEquals(coordinate, null))
            {
                string[] splits = coordinate.Replace("(", "").Replace(")", "").Split(',');
                if (splits.Length == 4)
                {
                    return new Tuple<int, int>(int.Parse(splits[0].Trim()), int.Parse(splits[1].Trim()));
                }
            }
            return null;
        }

        public static Tuple<int, int> getEndPos(string coordinate)
        {
            if (!string.ReferenceEquals(coordinate, null))
            {
                string[] splits = coordinate.Replace("(", "").Replace(")", "").Split(',');
                if (splits.Length == 4)
                {
                    return new Tuple<int, int>(int.Parse(splits[2].Trim()), int.Parse(splits[3].Trim()));
                }
            }
            return null;
        }
    }

}