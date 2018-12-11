using System;

namespace gudusoft.gsqlparser.demos.gettablecolumns
{
    using global::gudusoft.gsqlparser.demos.util;
    using gudusoft.gsqlparser.demos.lib;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using IMetaDatabase = gudusoft.gsqlparser.IMetaDatabase;

    internal class sampleMetaDB : IMetaDatabase
	{

		internal string[][] columns = new string[][]
		{
			new string[] {"server","db","schema","promotion","promo_desc"},
			new string[] {"server","db","schema","sales","dollars"}
		};

		public virtual bool checkColumn(string server, string database, string schema, string table, string column)
		{
			bool bServer , bDatabase , bSchema , bTable , bColumn , bRet = false;
			for (int i = 0; i < columns.Length;i++)
			{
				if ((string.ReferenceEquals(server, null)) || (server.Length == 0))
				{
					bServer = true;
				}
				else
				{
					bServer = columns[i][0].Equals(server, StringComparison.CurrentCultureIgnoreCase);
				}
				if (!bServer)
				{
					continue;
				}

				if ((string.ReferenceEquals(database, null)) || (database.Length == 0))
				{
					bDatabase = true;
				}
				else
				{
					bDatabase = columns[i][1].Equals(database, StringComparison.CurrentCultureIgnoreCase);
				}
				if (!bDatabase)
				{
					continue;
				}

				if ((string.ReferenceEquals(schema, null)) || (schema.Length == 0))
				{
					bSchema = true;
				}
				else
				{
					bSchema = columns[i][2].Equals(schema, StringComparison.CurrentCultureIgnoreCase);
				}

				if (!bSchema)
				{
					continue;
				}

				bTable = columns[i][3].Equals(table, StringComparison.CurrentCultureIgnoreCase);
				if (!bTable)
				{
					continue;
				}

				bColumn = columns[i][4].Equals(column, StringComparison.CurrentCultureIgnoreCase);
				if (!bColumn)
				{
					continue;
				}

				bRet = true;
				break;

			}

			return bRet;
		}

	}

	public class runGetTableColumn
	{

        private static void displayInitInformation()
        {
            Console.WriteLine("Usage: gettablecolumns [/d <path_to_directory_includes_sql_files>] [/f <path_to_sql_file>] [/t <database type>] [/o <path_to_output_result>] [/<show option>]");
            Console.WriteLine("/d: Option, specify the sql directory path to analyze table columns.");
            Console.WriteLine("/f: Option, specify the sql file path to analyze table columns.");
            Console.WriteLine("/o: Option, write the output result to the specified path.");
            Console.WriteLine("/t: option, set the database type. Support oracle, mysql, mssql, db2, netezza, teradata, informix, sybase, postgresql, hive, greenplum and redshift, the default type is oracle");
            Console.WriteLine("/showSummary: default show option, display the summary information.");
            Console.WriteLine("/showDetail: show option, display the detail information.");
            Console.WriteLine("/showTreeStructure: show option, display the information as a tree structure.");
            Console.WriteLine("/showBySQLClause: show option, display the information by sql clause type.");
            Console.WriteLine("/showJoin: show option, display the table join information.");
        }

        public static void Main(string[] args)
        {
            EDbVendor dbVendor = Common.GetEDbVendor(args);

            if (args.Length < 2)
            {
                displayInitInformation();
                return;
            }

            IList<string> argList = new List<string>(args);

            FileInfo sqlFile = null;
            FileInfo outputFile = null;

            if (argList.IndexOf("/f") != -1 && argList.Count > argList.IndexOf("/f") + 1)
            {
                sqlFile = new FileInfo(args[argList.IndexOf("/f") + 1]);
                if (!sqlFile.Exists)
                {
                    Console.WriteLine(sqlFile + " is not a valid file.");
                    return;
                }
            }

            if (argList.IndexOf("/d") != -1 && argList.Count > argList.IndexOf("/d") + 1)
            {
                sqlFile = new FileInfo(args[argList.IndexOf("/d") + 1]);
                if (!sqlFile.Attributes.HasFlag(FileAttributes.Directory))
                {
                    Console.WriteLine(sqlFile + " is not a valid directory.");
                    return;
                }
            }


            if (sqlFile == null)
            {
                displayInitInformation();
                return;
            }

            if (argList.IndexOf("/o") != -1 && argList.Count > argList.IndexOf("/o") + 1)
            {
                outputFile = new FileInfo(args[argList.IndexOf("/o") + 1]);
                if (!outputFile.Exists)
                {
                    if (!outputFile.Directory.Exists) {
                        Directory.CreateDirectory(outputFile.Directory.FullName);
                    }
                }
            }


            System.IO.FileStream writer = null;
            StreamWriter sw = null;

            if (outputFile!=null)
            {
                try
                {
                    writer = new System.IO.FileStream(outputFile.FullName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                    sw = new StreamWriter(writer);
                    Console.SetOut(sw);
                }
                catch (FileNotFoundException e)
                {
                    Console.WriteLine(e.ToString());
                    Console.Write(e.StackTrace);
                }
            }

            TGetTableColumn getTableColumn = new TGetTableColumn(dbVendor);
            getTableColumn.showDetail = false;
            getTableColumn.showSummary = true;
            getTableColumn.showTreeStructure = false;
            getTableColumn.showBySQLClause = false;
            getTableColumn.showJoin = false;
            getTableColumn.showColumnLocation = true;
            getTableColumn.linkOrphanColumnToFirstTable = false;
            getTableColumn.showIndex = true;
            getTableColumn.showDatatype = true;
            getTableColumn.showTableEffect = false;

            if (argList.IndexOf("/showDetail") != -1)
            {
                getTableColumn.showSummary = false;
                getTableColumn.showDetail = true;
            }
            else if (argList.IndexOf("/showTreeStructure") != -1)
            {
                getTableColumn.showSummary = false;
                getTableColumn.showTreeStructure = true;
            }
            else if (argList.IndexOf("/showBySQLClause") != -1)
            {
                getTableColumn.showSummary = false;
                getTableColumn.showBySQLClause = true;
            }
            else if (argList.IndexOf("/showJoin") != -1)
            {
                getTableColumn.showSummary = false;
                getTableColumn.showJoin = true;
            }

            getTableColumn.runFile(sqlFile);


            try
            {
                if (sw != null && writer != null)
                {
                    sw.Close();
                    writer.Close();
                }
                else
                {
                    Console.ReadLine();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }

           
        }

    }

}