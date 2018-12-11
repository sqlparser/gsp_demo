using System;
using System.Collections.Generic;

namespace gudusoft.gsqlparser.demos.dlineage
{
    using System.IO;
    using System.Text;
    using gudusoft.gsqlparser.demos.dlineage.model.xml;
    using gudusoft.gsqlparser.demos.dlineage.model.metadata;
    using gudusoft.gsqlparser.demos.util;
    //using util;

    public class Dlineage
    {
        
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: Dlineage [/f <path_to_sql_file>] [/d <path_to_directory_includes_sql_files>] [/t <database type>] [/fo <table column>] [/b <view column>] [/ddl] [/s] [/log]");
                Console.WriteLine("/f: Option, specify the sql file path to analyze dlineage.");
                Console.WriteLine("/d: Option, specify the sql directory path to analyze dlineage.");
                Console.WriteLine("/d: Option, forward analyze the specified table column.");
                Console.WriteLine("/t: Option, set the database type. Support oracle, mysql, mssql, db2, netezza, teradata, informix, sybase, postgresql, hive, greenplum and redshift, the default type is oracle");
                Console.WriteLine("/fo: Option, forward analyze the specified table column.");
                Console.WriteLine("/b: Option, backward analyze the specified view column.");
                Console.WriteLine("/ddl: Option, output the database DDL schema.");
                Console.WriteLine("/s: Option, set the strict match mode. It will match the catalog name and schema name.");
                Console.WriteLine("/log: Option, generate a dlineage.log file to log information.");
                return;
            }

            FileInfo sqlFiles = null;

            IList<string> argList = new List<string>(args);
            if (argList.IndexOf("/f") != -1 && argList.Count > argList.IndexOf("/f") + 1)
            {
                sqlFiles = new FileInfo(args[argList.IndexOf("/f") + 1]);
                if (!sqlFiles.Exists || sqlFiles.Attributes == FileAttributes.Directory)
                {
                    Console.WriteLine(sqlFiles + " is not a valid file.");
                    return;
                }
            }
            else if (argList.IndexOf("/d") != -1 && argList.Count > argList.IndexOf("/d") + 1)
            {
                sqlFiles = new FileInfo(args[argList.IndexOf("/d") + 1]);
                if (sqlFiles.Attributes != FileAttributes.Directory)
                {
                    Console.WriteLine(sqlFiles + " is not a valid directory.");
                    return;
                }
            }

            bool strict = argList.IndexOf("/s") != -1;
            bool log = argList.IndexOf("/log") != -1;

            TextWriter pw = null;
            StringBuilder sw = null;
            TextWriter systemSteam = Console.Error;

            try
            {
                sw = new StringBuilder();
                pw = new StringWriter(sw);
                Console.SetError(pw);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

            DlineageCommon dlineage;

            if (sqlFiles != null)
            {
                dlineage = new DlineageCommon(sqlFiles, Common.GetEDbVendor(args), strict, false);
            }
            else
            {
                string sqltext = @"SELECT e.last_name AS name,
                                e.commission_pct comm,
                                e.salary * 12 ""Annual Salary""
                                FROM scott.employees AS e
                                WHERE e.salary > 1000 or 1=1
                                ORDER BY
                                e.first_name,
                                e.last_name;";

                dlineage = new DlineageCommon(sqltext, Common.GetEDbVendor(args), strict, false);
            }

            bool forwardAnalyze = argList.IndexOf("/fo") != -1;
            bool backwardAnalyze = argList.IndexOf("/b") != -1;
            bool outputDDL = argList.IndexOf("/ddl") != -1;
           

            if (!forwardAnalyze && !backwardAnalyze && !outputDDL)
            {
                dlineage.columnImpact();
            }
            else if (outputDDL)
            {
                dlineage.outputDDLSchema();
            }
            else
            {
                columnImpactResult impactResult = dlineage.generateColumnImpact(null);
                IList<ColumnMetaData[]> relations = dlineage.collectDlineageRelations(impactResult);

                if (forwardAnalyze && argList.Count > argList.IndexOf("/fo") + 1)
                {
                    string tableColumn = argList[argList.IndexOf("/fo") + 1];
                    dlineage.forwardAnalyze(tableColumn, relations);
                }

                if (backwardAnalyze && argList.Count > argList.IndexOf("/b") + 1)
                {
                    string viewColumn = argList[argList.IndexOf("/b") + 1];
                    dlineage.backwardAnalyze(viewColumn, relations);
                }
            }

            if (pw != null)
            {
                pw.Close();
            }

            if (sw != null)
            {
                string errorMessage = sw.ToString().Trim();
                if (errorMessage.Length > 0)
                {
                    if (log)
                    {
                        try
                        {
                            pw = new StreamWriter(new FileInfo("./dlineage.log").FullName);
                            pw.WriteLine(errorMessage);
                            pw.Close();

                        }
                        catch (FileNotFoundException e)
                        {
                            Console.WriteLine(e.ToString());
                            Console.Write(e.StackTrace);
                        }
                    }

                    Console.SetError(systemSteam);
                    Console.Error.WriteLine(errorMessage);
                }
            }
        }
    }
}