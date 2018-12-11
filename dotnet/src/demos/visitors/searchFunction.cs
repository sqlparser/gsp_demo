using System;

namespace gudusoft.gsqlparser.demos.visitors
{


    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using TBaseType = gudusoft.gsqlparser.TBaseType;
    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using TFunctionCall = gudusoft.gsqlparser.nodes.TFunctionCall;
    using TParseTreeVisitor = gudusoft.gsqlparser.nodes.TParseTreeVisitor;
    using TCallStatement = gudusoft.gsqlparser.stmt.TCallStatement;
    using TMssqlExecute = gudusoft.gsqlparser.stmt.mssql.TMssqlExecute;
    using System.IO;

    public class searchFunction
	{
		public static void doSearch(String filename)
		{
			//long t = DateTime.Now.Millisecond;

			EDbVendor dbVendor = EDbVendor.dbvmssql;
			Console.WriteLine("Selected SQL dialect: " + dbVendor.ToString());

			TGSqlParser sqlparser = new TGSqlParser(dbVendor);
			sqlparser.sqlfilename = filename;

			int ret = sqlparser.parse();
			if (ret == 0)
			{
				functionVisitor fv = new functionVisitor();
				for (int i = 0;i < sqlparser.sqlstatements.size();i++)
				{
					TCustomSqlStatement sqlStatement = sqlparser.sqlstatements.get(i);
					Console.WriteLine(sqlStatement.sqlstatementtype);
					sqlStatement.acceptChildren(fv);
				}

			}
			else
			{
				Console.WriteLine(sqlparser.Errormessage);
			}

			//Console.WriteLine("Time Escaped: " + (DateTime.Now.Millisecond - t));
		}

	}

	internal class functionVisitor : TParseTreeVisitor
	{
		public override void preVisit(TFunctionCall node)
		{
			Console.WriteLine("--> function: " + node.FunctionName.ToString());
		}

		public override void preVisit(TCallStatement statement)
		{
			Console.WriteLine("--> call: " + statement.RoutineName.ToString());
		}

		public override void preVisit(TMssqlExecute statement)
		{
			if (statement.ExecType == TBaseType.metExecSp)
			{
				Console.WriteLine("--> execute: " + statement.ModuleName.ToString());
			}

		}


	}

}