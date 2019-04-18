using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.Collections.Generic;
using System.IO;

namespace gudusoft.gsqlparser.test
{
    public class UnitTestCommon
    {
        public static string BASE_SQL_DIR() {
                string SQLFilesDir =        @"../../../../../../../gsp_sqlfiles/TestCases/";
                string netcoreSQLFilesDir = @"../../../../../../../../gsp_sqlfiles/TestCases/";
                if (Directory.Exists(SQLFilesDir))
                {
                    return SQLFilesDir;
                }
                else if (Directory.Exists(netcoreSQLFilesDir))
                {
                    return netcoreSQLFilesDir;
                }
                else
                {
                    return SQLFilesDir;
                }
        }
        // dir for netcore testcases
     // public const string BASE_SQL_DIR = @"../../../../../../../../gsp_sqlfiles/TestCases/";

        public static void checkFile(TGSqlParser parser, string filename)
        {
           // Console.WriteLine(filename);
            parser.sqlfilename = filename;
            int ret = parser.parse();
            Assert.IsTrue(ret == 0, parser.Errormessage + Environment.NewLine + filename);
        }

        public static bool excludeFile(string filename, List <string> excludingFiles)
        {
            bool ret = false;

            for (int i = 0; i < excludingFiles.Count; i++)
            {
                if (excludingFiles[i].Contains(filename))
                {
                    ret = true;
                    break;
                }
               
            }

            return ret;
        }



    }
}
