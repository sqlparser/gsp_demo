using gudusoft.gsqlparser.demos.util;
using gudusoft.gsqlparser;
using System;
using System.Collections.Generic;
using System.IO;


namespace gudusoft.gsqlparser.demos.checksyntax
{
    class Program
    {
        static void Main(string[] args)
        {
            string sqlText = @"SELECT e.last_name AS name,
                                e.commission_pct comm,
                                e.salary * 12 ""Annual Salary""
                                FROM scott.employees AS e
                                WHERE e.salary > 1000 or 1=1
                                ORDER BY
                                e.first_name,
                                e.last_name;";

            IList<string> argList = new List<string>(args);

            EDbVendor dbVendor = Common.GetEDbVendor(args);

            Console.WriteLine("versionId:{0}, releaseDate:{1}, datebase:{2}", TBaseType.versionId, TBaseType.releaseDate, dbVendor);

            TGSqlParser sqlparser = new TGSqlParser(dbVendor);

            if (argList.IndexOf("/f") != -1 && argList.Count > argList.IndexOf("/f") + 1)
            {
                FileInfo sqlFile = new FileInfo(args[argList.IndexOf("/f") + 1]);
                if (!sqlFile.Exists || sqlFile.Attributes == FileAttributes.Directory)
                {
                    Console.WriteLine("File {0} does not exit.",sqlFile);
                    return;
                }
                sqlparser.sqlfilename = sqlFile.FullName;
            }
            else
            {
                sqlparser.sqltext = sqlText;
            }

            int ret = sqlparser.parse();
            if (ret == 0)
            {
                Console.WriteLine("Success!");
            }
            else
            {
                Console.WriteLine("Syntax error detected: {0}",sqlparser.Errormessage);
            }
        }
    }
}
