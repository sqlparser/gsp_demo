using System;

namespace gudusoft.gsqlparser.demos.visitors
{
    using System.IO;
    using gudusoft.gsqlparser;
    using lib;
    using global::gudusoft.gsqlparser.demos.util;
    using System.Collections.Generic;

    public class sample
    {
        public static void Main(string[] args)
        {
            EDbVendor dbVendor = Common.GetEDbVendor(args);

            IList<string> argList = new List<string>(args);
            if (argList.IndexOf("/f") != -1 && argList.Count > argList.IndexOf("/f") + 1)
            {
                FileInfo sqlFile = new FileInfo(args[argList.IndexOf("/f") + 1]);
                if (!sqlFile.Exists || sqlFile.Attributes == FileAttributes.Directory)
                {
                    Console.WriteLine("File {0} does not exit.", sqlFile);
                    return;
                }
                toXML(sqlFile.FullName, dbVendor);
            }
            else
            {
                toXML();
            }

            Console.ReadLine();
        }

        static void doSearchFunction(String sqlFilename)
        {
            searchFunction.doSearch(sqlFilename);
        }

        static void toXML()
        {
            EDbVendor dbVendor = EDbVendor.dbvoracle;
            Console.WriteLine("Selected SQL dialect: " + dbVendor.ToString());

            TGSqlParser sqlparser = new TGSqlParser(dbVendor);
            sqlparser.sqltext = @"SELECT e.last_name AS name,
                                    e.commission_pct comm,
                                    e.salary * 12 ""Annual Salary""
                                    FROM scott.employees AS e
                                    WHERE e.salary > 1000
                                    ORDER BY
                                    e.first_name,
                                    e.last_name; ";

            int ret = sqlparser.parse();
            if (ret == 0)
            {
                xmlVisitor xv2 = new xmlVisitor();
                xv2.run(sqlparser);
                Console.WriteLine(xv2.FormattedXml);

            }
            else
            {
                Console.WriteLine(sqlparser.Errormessage);
            }
        }

        static void toXML(String sqlFilename, EDbVendor dbVendor)
        {
            Console.WriteLine("Selected SQL dialect: " + dbVendor.ToString());

            TGSqlParser sqlparser = new TGSqlParser(dbVendor);
            String inputFile = sqlFilename;

            sqlparser.sqlfilename = inputFile;
            string xmlFile = inputFile + ".xml";

            int ret = sqlparser.parse();
            if (ret == 0)
            {
                xmlVisitor xv2 = new xmlVisitor();
                xv2.run(sqlparser);
                xv2.writeToFile(xmlFile);
                Console.WriteLine(xmlFile + " was generated!");

            }
            else
            {
                Console.WriteLine(sqlparser.Errormessage);
            }
        }
    }
}