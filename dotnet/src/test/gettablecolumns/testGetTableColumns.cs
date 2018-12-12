using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gudusoft.gsqlparser.demos.lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;



namespace gudusoft.gsqlparser.test.gettablecolumns
{

    [TestClass()]
    public class testGetTableColumns
    {
        public const string BASE_SQL_DIR = @"../../../../../../../gsp_sqlfiles/TestCases/";

        private void doGetTableColumn(EDbVendor dbvendor, String sqlText, String requiredStr)
        {
            TGetTableColumn getTableColumn = new TGetTableColumn(dbvendor);
            getTableColumn.listStarColumn = false;
            getTableColumn.showTreeStructure = false;
            getTableColumn.showTableEffect = true;
            getTableColumn.showColumnLocation = true;
            getTableColumn.linkOrphanColumnToFirstTable = true;
            getTableColumn.isConsole = false;
            //getTableColumn.setMetaDatabase(new sampleMetaDB());

            getTableColumn.runText(sqlText);
            String[] actualLines = getTableColumn.outList.ToString().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            String[] requiredLines = requiredStr.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            compareTwoStringArray(requiredLines, actualLines, "Inline Query");
        }

        public static bool includeFile(string filename, List<string> includingFiles)
        {
            bool ret = false;

            for (int i = 0; i < includingFiles.Count; i++)
            {
                if (includingFiles[i].Contains(filename))
                {
                    ret = true;
                    break;
                }

            }

            return ret;
        }

        [TestMethod()]
        public void testDB2Trigger()
        {
            doGetTableColumn(EDbVendor.dbvdb2,
                @"create trigger t_mdap_issure_insert
	after insert on issuer
	REFERENCING NEW AS N
	for each row
	insert into cops_mdap_mail_arg (table_name, key, sended) values ('ISSUER',
	concat(concat(n.partner_id, ';'), n.partyp_id), 'n'
	)",
                @"Tables:
cops_mdap_mail_arg
issuer
Fields:
cops_mdap_mail_arg.key
cops_mdap_mail_arg.sended
cops_mdap_mail_arg.table_name
issuer.partner_id
issuer.partyp_id"
                );
        }


        [TestMethod()]
        public void testGetTableColumn()
        {
            TGetTableColumn getTableColumn = new TGetTableColumn(EDbVendor.dbvoracle);
            getTableColumn.listStarColumn = false;
            getTableColumn.showTreeStructure = false;
            getTableColumn.showTableEffect = true;
            getTableColumn.showColumnLocation = true;
            getTableColumn.linkOrphanColumnToFirstTable = true;
            getTableColumn.isConsole = false;
            //getTableColumn.setMetaDatabase(new sampleMetaDB());

            String sqlText = "MERGE INTO bonuses D\r\n" +
                        "   USING(SELECT employee_id, salary, department_id FROM employees\r\n" +
                        "   WHERE department_id = 80) S\r\n" +
                        "   ON(D.employee_id = S.employee_id)\r\n" +
                        "   WHEN MATCHED THEN UPDATE SET D.bonus = D.bonus + S.salary * .01\r\n" +
                        "   WHEN NOT MATCHED THEN INSERT(D.employee_id, D.bonus)\r\n" +
                        "   VALUES(S.employee_id, S.salary * 0.1);";
            getTableColumn.runText(sqlText);
            String[] actualLines = getTableColumn.outList.ToString().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            String requiredStr = "Tables:\n"
                                    + "bonuses\n"
                                    + "employees\n"
                                    + "Fields:\n"
                                    + "bonuses.bonus\n"
                                    + "bonuses.employee_id\n"
                                    + "employees.department_id\n"
                                    + "employees.employee_id\n"
                                    + "employees.salary";
            String[] requiredLines = requiredStr.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            compareTwoStringArray(requiredLines, actualLines,"Inline Query");

        }


        [TestMethod()]
        public void testGetTableColumnOracleFiles()
        {
            List<String> includeFiles = new List<string>{
                "berger_example_01.sql" ,"berger_sqltest_01.sql","berger_sqltest_02.sql",
                 "berger_sqltest_03.sql","berger_sqltest_04.sql","berger_sqltest_05.sql",
                 "createtrigger.sql","plsql_block_correlated_subquery.sql","createfunction.sql"  ,
                 "createpackagebody.sql","merge.sql","no_qualified_subquery.sql"
            };
            doTableColumnFiles(EDbVendor.dbvoracle,
                BASE_SQL_DIR + @"java\oracle\dbobject\",
                includeFiles
                );
        }

        [TestMethod()]
        public void testGetTableColumnSQLServerFiles()
        {
            List<String> includeFiles = new List<string>{
            "bigjoin1.sql","shurleyjoin.sql",
            "delete1.sql","delete2.sql","delete4.sql","delete5.sql",
            "update1.sql","update2.sql","update3.sql","update4.sql",
            "createfunction1.sql","createprocedure1.sql","createtrigger1.sql",
            "createview1.sql",
            "while1.sql","keyword_not_column_name.sql",
            "ogcmethod.sql",
           "funcitonOnXMLColumn.sql"
            };
            doTableColumnFiles(EDbVendor.dbvmssql,
                BASE_SQL_DIR+@"java\mssql\dbobject\",
                includeFiles
                );
        }

        private void doTableColumnFiles(EDbVendor dbVendor, String dir, List<String> includeFiles)
        {
            if (!Directory.Exists(dir))
            {
                Console.WriteLine("Skip this testcase, directory not exists:"+dir);
                return;
            }
            
            TGetTableColumn getTableColumn = new TGetTableColumn(dbVendor);
            getTableColumn.listStarColumn = false;
            getTableColumn.showTreeStructure = false;
            getTableColumn.showTableEffect = false;
            getTableColumn.showColumnLocation = false;
            getTableColumn.linkOrphanColumnToFirstTable = true;
            getTableColumn.isConsole = false;
            getTableColumn.showSummary = true;
            

            String[] allFiles = System.IO.Directory.GetFiles(dir, "*.sql", System.IO.SearchOption.AllDirectories);
            
            foreach (var file in allFiles)
            {
                FileInfo info = new FileInfo(file);
                if (!includeFile(info.Name, includeFiles))
                {
                    continue;
                }
                //sqlFile = new FileInfo(args[argList.IndexOf("/f") + 1]);
                getTableColumn.runText(File.ReadAllText(info.FullName));
                String outFile = Path.ChangeExtension(info.FullName, ".outn");
                if (!File.Exists(outFile))
                {
                    outFile = Path.ChangeExtension(info.FullName, ".out");
                }
                
                String[] requiredLines = File.ReadAllLines(outFile);
                String[] actualLines = getTableColumn.outList.ToString().Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                compareTwoStringArray(requiredLines,actualLines,info.Name);
            }


        }

        void compareTwoStringArray(String[] requiredLines, String[] actualLines, String sourceName)
        {
            int cnt = 0;
            //Console.WriteLine("required:");
            //foreach (var line in requiredLines)
            //    Console.WriteLine(line);
            //Console.WriteLine("actual:");
            //foreach (var line in actualLines)
            //    Console.WriteLine(line);


            foreach (var line in requiredLines)
            {
                if (requiredLines[cnt].Equals("Functions:", StringComparison.CurrentCultureIgnoreCase)) break;
                if (requiredLines[cnt].Equals("Schema:", StringComparison.CurrentCultureIgnoreCase)) break;
                Assert.IsTrue(cnt < actualLines.Length
                     , String.Format("\nFile: {0}\nLine: {1}\nrequired: {2}\nactual: no more content", sourceName, cnt + 1, requiredLines[cnt])
                    );
                Assert.IsTrue(
                    requiredLines[cnt].Equals(actualLines[cnt], StringComparison.CurrentCultureIgnoreCase)
                    , String.Format("\nFile: {0}\nLine: {1}\nrequired: {2}\nactual: {3}", sourceName, cnt + 1, requiredLines[cnt], actualLines[cnt])
                    );
                cnt++;
            }

            Assert.IsTrue(cnt == actualLines.Length
                 , String.Format("\nFile: {0}\nLine: {1}\nrequired: no more content\nactual: {2}", sourceName, cnt + 1, actualLines[cnt - 1])
                );


        }
    }
}
