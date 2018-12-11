using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using gudusoft.gsqlparser;
using System.IO;

namespace gudusoft.gsqlparser.test.scriptWriter
{


    [TestClass]
    public class testGenerator
    {

        internal virtual bool supportedSqlType(ESqlStatementType sqlStatementType)
        {
            return ((sqlStatementType == ESqlStatementType.sstselect) || (sqlStatementType == ESqlStatementType.sstdelete) || (sqlStatementType == ESqlStatementType.sstupdate) || (sqlStatementType == ESqlStatementType.sstinsert) || (sqlStatementType == ESqlStatementType.sstmerge) || (sqlStatementType == ESqlStatementType.sstcreatetable) || (sqlStatementType == ESqlStatementType.sstcreateview) || (sqlStatementType == ESqlStatementType.sstdropindex) || (sqlStatementType == ESqlStatementType.sstUseDatabase) || (sqlStatementType == ESqlStatementType.sstmssqlcreatefunction) || (sqlStatementType == ESqlStatementType.sstmssqlif));
        }
        internal virtual void processfiles(EDbVendor db, string dir)
        {

            TGSqlParser sqlparser = new TGSqlParser(db);
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            FileInfo[] sqlfiles = dirInfo.GetFiles();

            for (int k = 0; k < sqlfiles.Length; k++)
            {
                sqlparser.sqlfilename = sqlfiles[k].FullName;

                try
                {
                    bool b = sqlparser.parse() == 0;
                    Assert.IsTrue(b);
                    if (b)
                    {
                        for (int i = 0; i < sqlparser.sqlstatements.size(); i++)
                        {
                            if (supportedSqlType(sqlparser.sqlstatements.get(i).sqlstatementtype))
                            {
                                //assertTrue(sqlparser.sqlfilename,rewriteQuery(sqlparser.sqlstatements.get(i),db));
                                if (!rewriteQuery(sqlparser.sqlstatements.get(i), db))
                                {
                                    Console.WriteLine("\n" + sqlparser.sqlfilename + "\n");
                                }
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    Console.WriteLine("testGenerator error:" + e.Message + " " + sqlparser.sqlfilename);
                }
            }

        }

        internal virtual bool rewriteQuery(TCustomSqlStatement sqlStatement, EDbVendor dbVendor)
        {
            String sourceSql = sqlStatement.ToString();
            String targetSql = sqlStatement.ToScript();
            return testScriptGenerator.verifyScript(dbVendor, sourceSql, targetSql);
        }

        [TestMethod]
        public virtual void testOracle()
        {
            // processfiles(EDbVendor.dbvoracle, "c:/prg/gsqlparser/Test/TestCases/oracle");
            //  processfiles(EDbVendor.dbvoracle, "c:/prg/gsqlparser/Test/TestCases/java/oracle/");
            //  processfiles(EDbVendor.dbvoracle, "c:/prg/gsqlparser/Test/TestCases/commonsql/");

        }

    }

}