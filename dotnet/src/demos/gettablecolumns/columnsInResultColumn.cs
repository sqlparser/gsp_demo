using System;

namespace gudusoft.gsqlparser.demos.gettablecolumns
{
    using gudusoft.gsqlparser.demos.lib;

    /*
	 * Date: 11-4-19
	 * ref : http://www.dpriver.com/blog/list-of-demos-illustrate-how-to-use-general-sql-parser/get-referenced-table-column-in-a-select-list-item/
	 */

    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
    using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
    using TResultColumn = gudusoft.gsqlparser.nodes.TResultColumn;
    using TResultColumnList = gudusoft.gsqlparser.nodes.TResultColumnList;
    using TSelectSqlStatement = gudusoft.gsqlparser.stmt.TSelectSqlStatement;

    public class columnsInResultColumn
	{

		public static void doSearch(String sqlText)
		{
			 TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);

            //         sqlparser.sqltext  = "select sal.income + sal.bonus * emp.age + 5 as real_sal,\n" +
            //                 "       emp.name as title \n" +
            //                 "from employee emp, salary sal \n" +
            //                 "where emp.id=sal.eid";

            //         sqlparser.sqltext = "SELECT CASE \n" +
            //                 "         WHEN \"employees\".\"firstname1\" = ''THEN \"employees\".\"lastname1\" \n" +
            //                 "         ELSE \"employees\".\"firstname2\" + ' ' \n" +
            //                 "              + \"employees\".\"lastname2\" \n" +
            //                 "       END AS \"Full Name\" \n" +
            //                 "FROM   \"NORTHWIND\".\"DBO\".\"employees\" \"Employees\" ";

            //         sqlparser.sqltext  = "SELECT e.ename as employeename,\n" +
            //                 "       m.ename,\n" +
            //                 "       d.name,\n" +
            //                 "       e.first_name||d.dname,\n" +
            //                 "       (select max(sal) from emp) as max_sal\n" +
            //                 "FROM   employees e\n" +
            //                 "       LEFT OUTER JOIN employees m ON ( e.mgr_id = m.id )\n" +
            //                 "       LEFT OUTER JOIN department d ON ( e.department_id = d.department_id )";

            //         sqlparser.sqltext  = "SELECT a.customerid, \n" +
            //                 "       (SELECT CASE \n" +
            //                 "                 WHEN b.city = 'Berlin' THEN b.country + ' ' + b.city \n" +
            //                 "                 ELSE b.postalcode \n" +
            //                 "               END \n" +
            //                 "        FROM   customers b \n" +
            //                 "        WHERE  a.customerid = b.customerid) AS Location \n" +
            //                 "FROM   customers a ";
            sqlparser.sqltext = sqlText;
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

		internal static void iterateStmt(TCustomSqlStatement pStmt)
		{

			if (pStmt is TSelectSqlStatement)
			{
				processSelect((TSelectSqlStatement)pStmt);
			}
			for (int i = 0;i < pStmt.Statements.size();i++)
			{
				iterateStmt(pStmt.Statements.get(i));
			}
		}

		internal static void processSelect(TSelectSqlStatement select)
		{

			TResultColumnList columns = select.ResultColumnList;

			for (int i = 0; i < columns.size();i++)
			{
				printColumns(columns.getResultColumn(i),select);
			}

		}

		 internal static void printColumns(TResultColumn cl, TCustomSqlStatement sqlStatement)
		 {

			 if (cl.AliasClause != null)
			 {
				 Console.WriteLine("\nResult column:" + cl.AliasClause.ToString());
			 }
			 else
			 {
				Console.WriteLine("\nResult column:" + cl.Expr.ToString());
			 }

			 (new columnInClause()).printColumns(cl.Expr,sqlStatement);
		 }
	}


}