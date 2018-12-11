using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.Collections.Generic;

namespace gudusoft.gsqlparser.test
{
    public class UnitTestCommon
    {
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
