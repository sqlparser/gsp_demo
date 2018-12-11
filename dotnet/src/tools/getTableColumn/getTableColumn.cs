using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using gudusoft.gsqlparser.demos.util;
using gudusoft.gsqlparser.demos.dlineage.model.xml;
using gudusoft.gsqlparser.demos.dlineage.model.ddl.schema;
using gudusoft.gsqlparser.demos.dlineage;

namespace gudusoft.gsqlparser.tools.getTableColumn
{


    public class getTableColumn
    {
        private const string OverWarning = "Waring: Only processes the first 30 SQL files.";

        internal class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder sb) : base(sb) { }

            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }



        private String getTableColumns(FileInfo file, DlineageCommon dlineage)
        {
            if (dlineage.DataMetaInfos == null)
            {
                return null;
            }
            StringBuilder buffer = new StringBuilder();
            for (int i = 0; i < dlineage.DataMetaInfos.Length; i++)
            {
                database db = dlineage.DataMetaInfos[i];
                for (int j = 0; j < db.tables.Length; j++)
                {
                    demos.dlineage.model.ddl.schema.table currentTable = db.tables[j];
                    if (currentTable.columns == null || currentTable.columns.Length == 0)
                    {
                        continue;
                    }

                    if (!string.ReferenceEquals(currentTable.isView, null) && true == Convert.ToBoolean(currentTable.isView))
                    {
                        continue;
                    }

                    if ("CONSTANT".Equals(currentTable.name))
                    {
                        continue;
                    }

                    String tableName = "";

                    if (currentTable.name.IndexOf(".") != -1)
                    {
                        tableName = currentTable.name.Replace(".", ",");
                    }
                    else
                    {
                        if (!string.ReferenceEquals(db.name, null) && !"unknown".Equals(db.name, StringComparison.CurrentCultureIgnoreCase))
                        {

                            tableName = (db.name + "," + currentTable.name);
                        }
                        else
                        {
                            tableName = ("," + currentTable.name);
                        }
                    }

                    for (int k = 0; k < currentTable.columns.Length; k++)
                    {
                        column column = currentTable.columns[k];
                        if ("*".Equals(column.name))
                            continue;
                        buffer.Append(tableName).Append(",").Append(column.name).Append(",").Append(file.Name).Append("\r\n");
                    }
                }
            }
            return buffer.ToString();
        }

        public static void Main(string[] args)
        {
            IList<string> argList = new List<string>(args);

            if (args.Length < 1 || argList.IndexOf("/d") == -1)
            {
                Console.WriteLine("Usage: getTableColumn [/d <path_to_directory_includes_sql_files>] [/o <path_to_directory_table_columns_result>] [/t <database type>]");
                Console.WriteLine("/d: specify the sql directory path to analyze table columns.");
                Console.WriteLine("/o: Option, write the output result to the specified directory.");
                Console.WriteLine("/t: Option, set the database type. Support oracle, mysql, mssql, db2, netezza, teradata, informix, sybase, postgresql, hive, greenplum and redshift, the default type is oracle");
                return;
            }

            FileInfo sqlFiles = null;
            FileInfo outputDir = null;

            if (argList.IndexOf("/d") != -1 && argList.Count > argList.IndexOf("/d") + 1)
            {
                sqlFiles = new FileInfo(args[argList.IndexOf("/d") + 1]);
                if (!sqlFiles.Attributes.HasFlag(FileAttributes.Directory))
                {
                    Console.WriteLine(sqlFiles + " is not a valid directory.");
                    return;
                }
            }

            if (argList.IndexOf("/o") != -1 && argList.Count > argList.IndexOf("/o") + 1)
            {
                outputDir = new FileInfo(args[argList.IndexOf("/o") + 1]);
                if (!outputDir.Attributes.HasFlag(FileAttributes.Directory))
                {
                    Console.WriteLine(outputDir + " is not a valid directory.");
                    return;
                }
            }

            string outputFile = null;
            string errorFile = null;

            if (outputDir != null)
            {
                outputFile = outputDir.FullName + "\\tableColumns.txt";
                errorFile = outputDir.FullName + "\\error.txt";
            }
            else
            {
                outputFile = ".\\tableColumns.txt";
                errorFile = ".\\error.txt";
            }

            System.IO.FileStream writer = null;
            StreamWriter sw = null;

            if (!string.ReferenceEquals(outputFile, null))
            {
                try
                {
                    writer = new System.IO.FileStream(outputFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    sw = new StreamWriter(writer);
                    Console.SetOut(sw);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }

            StringBuilder errorBuffer = new StringBuilder();

            List<FileInfo> files = new List<FileInfo>();
            getFiles(sqlFiles, files);

            bool isOver = false;

            StringBuilder buffer = new StringBuilder();
            buffer.Append("Schama,Table,Column,File\r\n");

            int count = 0;
            for (int i = 0; i < files.Count; i++)
            {
                if (count >= 30)
                {
                    isOver = true;
                    break;
                }
                try
                {
                    StringBuilder errorMessage = new StringBuilder();
                    DlineageCommon dlineage = new DlineageCommon(files[i], Common.GetEDbVendor(args), false, false);
                    columnImpactResult impactResult = dlineage.generateColumnImpact(errorMessage);
                    getTableColumn relation = new getTableColumn();
                    string result = relation.getTableColumns(files[i], dlineage);
                    if (result != null)
                    {
                        buffer.Append(result);
                    }
                    if (errorMessage.Length > 0)
                    {
                        errorBuffer.AppendLine(files[i].Name + ":\r\n" + errorMessage);
                        if (errorMessage.ToString().IndexOf("syntax error") == -1)
                        {
                            count += 1;
                        }
                    }
                    else
                    {
                        count += 1;
                    }
                }
                catch (Exception e)
                {
                    errorBuffer.AppendLine(files[i].Name + ":\r\n" + e.Message + "\r\n" + e.StackTrace);
                }
            }

            Console.Write(buffer.ToString());
            try
            {
                if (sw != null && writer != null)
                {
                    sw.Close();
                    writer.Close();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

            if (isOver)
            {
                errorBuffer.Insert(0, OverWarning + "\r\n");
            }

            if (errorBuffer.Length > 0)
            {
                Console.Error.WriteLine("Error log:\n" + errorBuffer.ToString());
                string log = errorBuffer.ToString().Replace(OverWarning, "").Trim();
                if (log.Length > 0 && !string.ReferenceEquals(errorFile, null))
                {
                    try
                    {
                        writer = new System.IO.FileStream(errorFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                        sw = new StreamWriter(writer);
                        sw.Write(log);
                        sw.Close();
                        writer.Close();
                    }
                    catch (FileNotFoundException e)
                    {
                        Console.WriteLine(e.ToString());
                        Console.Write(e.StackTrace);
                    }
                }
            }
        }

        private static void getFiles(FileInfo sqlFiles, List<FileInfo> files)
        {
            try
            {
                if (!sqlFiles.Attributes.HasFlag(FileAttributes.Directory))
                {
                    if (sqlFiles.FullName.ToLower().EndsWith(".sql"))
                    {
                        files.Add(sqlFiles);
                    }
                }

                if (sqlFiles.Attributes.HasFlag(FileAttributes.Directory))
                {
                    FileInfo[] children = new DirectoryInfo(sqlFiles.FullName).GetFiles();
                    for (int i = 0; i < children.Length; i++)
                    {
                        getFiles(children[i], files);
                    }

                    DirectoryInfo[] dirs = new DirectoryInfo(sqlFiles.FullName).GetDirectories();
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        getFiles(new FileInfo(dirs[i].FullName), files);
                    }
                }
            }
            catch (Exception) { }
        }
    }
}