using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace gudusoft.gsqlparser.dataFlowAnalyzer.dataflow.model.xml
{
    public class XML2Model<T>
    {

        public static T loadXML(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            try
            {
                StringReader sr = new StringReader(xml);
                T result = (T)serializer.Deserialize(sr);
                sr.Close();
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
                return default(T);
            }
        }
    }
}
