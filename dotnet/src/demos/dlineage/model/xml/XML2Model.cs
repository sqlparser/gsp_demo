using System;
using System.IO;
using System.Xml.Serialization;

namespace demos.dlineage.model.xml
{

	public class XML2Model
	{

		public static columnImpactResult loadXML(string xml)
		{
            XmlSerializer serializer = new XmlSerializer(typeof(columnImpactResult));
			try
			{
                StringReader sr = new StringReader(xml);
                columnImpactResult result = (columnImpactResult)serializer.Deserialize(sr);
                sr.Close();
                return result;
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return null;
			}
		}
	}
}