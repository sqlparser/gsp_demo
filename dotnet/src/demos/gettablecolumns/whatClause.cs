using System;

namespace gudusoft.gsqlparser.demos.gettablecolumns
{


	using gudusoft.gsqlparser;
	using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
	using TResultColumnList = gudusoft.gsqlparser.nodes.TResultColumnList;
	using TDeleteSqlStatement = gudusoft.gsqlparser.stmt.TDeleteSqlStatement;
	using TInsertSqlStatement = gudusoft.gsqlparser.stmt.TInsertSqlStatement;
	using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;
	using TUpdateSqlStatement = gudusoft.gsqlparser.stmt.TUpdateSqlStatement;

	public class whatClause
	{

		public static void doIt()
		{
			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
			 sqlparser.sqltext = "select employee_id,last_name,sal\n" + "from employees\n" + "where department_id = 90\n" + "group by employee_id having sal>10\n" + "order by last_name;";

			 sqlparser.parse();

			 TSelectSqlStatement select = (TSelectSqlStatement)sqlparser.sqlstatements.get(0);
                gudusoft.gsqlparser.nodes.TTable table = select.tables.getTable(0);
			 TObjectName o;
			 Console.WriteLine("Select statement, find out what clause a TObjectName belongs to:");
			 for (int i = 0;i < table.LinkedColumns.size();i++)
			 {
				 o = table.LinkedColumns.getObjectName(i);
				 Console.WriteLine(o.ToString() + "\t\t\tlocation:" + o.Location);
			 }

			 sqlparser.sqltext = "insert into emp e1 (e1.lastname,job) values('scott',10);";
			 sqlparser.parse();

			 TInsertSqlStatement insert = (TInsertSqlStatement)sqlparser.sqlstatements.get(0);
			 table = insert.tables.getTable(0);

			 Console.WriteLine("\n\nInsert statement, find out what clause a TObjectName belongs to:");
			 for (int i = 0;i < table.LinkedColumns.size();i++)
			 {
				 o = table.LinkedColumns.getObjectName(i);
				 Console.WriteLine(o.ToString() + "\t\t\tlocation:" + o.Location);
			 }

			 sqlparser.sqltext = "update employees\n" + "set department_ID = 70\n" + "where employee_id = 113;";
			 sqlparser.parse();


			 TUpdateSqlStatement update = (TUpdateSqlStatement)sqlparser.sqlstatements.get(0);
			 table = update.tables.getTable(0);
			 Console.WriteLine("\n\nUpdate statement, find out what clause a TObjectName belongs to:");
			 for (int i = 0;i < table.LinkedColumns.size();i++)
			 {
				 o = table.LinkedColumns.getObjectName(i);
				 Console.WriteLine(o.ToString() + "\t\t\tlocation:" + o.Location);
			 }

			 sqlparser.sqltext = "delete from employees E\n" + "where employee_id = \n" + "(select employee_sal\n" + "from emp_history\n" + "where employee_id = e.employee_id);";
			 sqlparser.parse();


			 TDeleteSqlStatement delete = (TDeleteSqlStatement)sqlparser.sqlstatements.get(0);
			 table = delete.tables.getTable(0);

			 Console.WriteLine("\n\nDelete statement, find out what clause a TObjectName belongs to:");
			 for (int i = 0;i < table.LinkedColumns.size();i++)
			 {
				 o = table.LinkedColumns.getObjectName(i);
				 Console.WriteLine(o.ToString() + "\t\t\tlocation:" + o.Location);
			 }


			 // subquery in where clause
			 select = (TSelectSqlStatement)delete.Statements.get(0);
             gudusoft.gsqlparser.nodes.TTable table1 = select.tables.getTable(0);
			 Console.WriteLine("\nSubquery in delete statement, find out what clause a TObjectName belongs to:");
			 for (int i = 0;i < table1.LinkedColumns.size();i++)
			 {
				 o = table1.LinkedColumns.getObjectName(i);
				 Console.WriteLine(o.ToString() + "\t\t\tlocation:" + o.Location);
			 }

		}

	}
}