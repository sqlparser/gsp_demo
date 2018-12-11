using System;

namespace gudusoft.gsqlparser.demos.gettablecolumns
{
    using System.IO;
    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;



    public class columnTableStmt
	{

		public static void doIt(String sqlFilename)
		{

            EDbVendor dbVendor = EDbVendor.dbvmssql;
			Console.WriteLine("Selected SQL dialect: " + dbVendor.ToString());


			TGSqlParser sqlparser = new TGSqlParser(dbVendor);
			sqlparser.sqlfilename = sqlFilename;

			int ret = sqlparser.parse();
			if (ret == 0)
			{
				for (int i = 0;i < sqlparser.sqlstatements.size();i++)
				{
					iterateStmt(sqlparser.sqlstatements.get(i));
				}
			}
			else
			{
				Console.WriteLine(sqlparser.Errormessage);
			}
		}

		protected internal static void iterateStmt(TCustomSqlStatement stmt)
		{
		   // System.out.println(stmt.sqlstatementtype.toString());

			for (int i = 0;i < stmt.tables.size();i++)
			{
                gudusoft.gsqlparser.nodes.TTable table = stmt.tables.getTable(i);
				string table_name = table.Name;
				Console.WriteLine("Analyzing: " + table_name + " <- " + stmt.sqlstatementtype);
				for (int j = 0; j < table.LinkedColumns.size(); j++)
				{
				  TObjectName objectName = table.LinkedColumns.getObjectName(j);
				  string column_name = table_name + "." + objectName.ColumnNameOnly.ToLower();
				  if (!objectName.TableDetermined)
				  {
					 column_name = "?." + objectName.ColumnNameOnly.ToLower();
				  }
				  Console.WriteLine("Analyzing: " + column_name + " in " + stmt.sqlstatementtype + " " + objectName.Location);
				}
			}

			for (int i = 0;i < stmt.Statements.size();i++)
			{
			   iterateStmt(stmt.Statements.get(i));
			}

		}


	}

}