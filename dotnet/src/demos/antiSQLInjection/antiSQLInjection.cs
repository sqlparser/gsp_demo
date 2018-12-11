using System;

namespace gudusoft.gsqlparser.demos.antiSQLInjection
{
    using util;
    using System.Collections.Generic;
    using System.IO;
    using EDbVendor = gudusoft.gsqlparser.EDbVendor;

    public class antiSQLInjection
    {

        public static void Main(string[] args)
        {
            string sqltext = @"SELECT e.last_name AS name,
                                e.commission_pct comm,
                                e.salary * 12 ""Annual Salary""
                                FROM scott.employees AS e
                                WHERE e.salary > 1000 or 1=1
                                ORDER BY
                                e.first_name,
                                e.last_name;";
            TAntiSQLInjection anti = new TAntiSQLInjection(Common.GetEDbVendor(args));

            List<string> argList = new List<string>(args);
            int index = argList.IndexOf("/f");

            FileInfo file = null;
            if (index != -1 && args.Length > index + 1)
            {
                file = new FileInfo(args[index + 1]);
            }

            if (file == null ? anti.isInjected(sqltext) : anti.isInjected(file))
            {
                Console.WriteLine("SQL injected found:");
                for (int i = 0; i < anti.SqlInjections.Count; i++)
                {
                    Console.WriteLine("type: " + anti.SqlInjections[i].Type + ", description: " + anti.SqlInjections[i].Description);
                }
            }
            else
            {
                Console.WriteLine("Not injected");
            }

        }

    }
}