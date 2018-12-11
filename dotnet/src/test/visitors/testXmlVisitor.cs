using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using gudusoft.gsqlparser.demos.lib;
using gudusoft.gsqlparser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gudusoft.gsqlparser.test.visitors
{
    [TestClass()]
    public class testXmlVisitor
    {
        [TestMethod()]
        public void testToXML()
        {
            string sqltext = "MERGE INTO bonuses D\r\n" +
                        "   USING(SELECT employee_id, salary, department_id FROM employees\r\n" +
                        "   WHERE department_id = 80) S\r\n" +
                        "   ON(D.employee_id = S.employee_id)\r\n" +
                        "   WHEN MATCHED THEN UPDATE SET D.bonus = D.bonus + S.salary * .01\r\n" +
                        "   WHEN NOT MATCHED THEN INSERT(D.employee_id, D.bonus)\r\n" +
                        "   VALUES(S.employee_id, S.salary * 0.1);";

            TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
            sqlparser.sqltext = sqltext;
            sqlparser.parse();

            xmlVisitor xv2 = new xmlVisitor();
            xv2.run(sqlparser);

            //Type type = MethodBase.GetCurrentMethod().DeclaringType;
            //string _namespace = Assembly.GetExecutingAssembly().GetName().Name+"."+type.Namespace;
            //string resourceName = _namespace + ".result.xml";

            //Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            //byte[] arrFileData = new byte[stream.Length];
            //stream.Read(arrFileData, 0, arrFileData.Length);
            //stream.Close();

           // Assert.IsTrue(xv2.FormattedXml.Equals(Encoding.Default.GetString(arrFileData)));
        }
    }
}
