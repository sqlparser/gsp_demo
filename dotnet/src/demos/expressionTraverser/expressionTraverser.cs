using System;

namespace gudusoft.gsqlparser.demos.expressionTraverser
{
    using System.IO;
    using System.Collections.Generic;
    using gudusoft.gsqlparser.demos.util;
    using gudusoft.gsqlparser;
    using gudusoft.gsqlparser.nodes;

    public class expressionTraverser
	{

		public static void Main(string[] args)
		{
            FileInfo file = null;
            List<string> argList = new List<string>(args);
            int index = argList.IndexOf("/f");
            if (index != -1 && args.Length > index + 1)
            {
                file = new FileInfo(args[index + 1]);
            }

            TGSqlParser sqlparser;
            if (file != null)
            {
                sqlparser = new TGSqlParser(Common.GetEDbVendor(args));
                sqlparser.sqlfilename = file.FullName;
            }
            else
            {
                sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
                sqlparser.sqltext = "select col1, col2,sum(col3) from table1, table2 where col4 > col5 and col6= 1000 or c1 = 1 and not sal";
            }

            int ret = sqlparser.parse();
			if (ret == 0)
			{
				TCustomSqlStatement select = sqlparser.sqlstatements.get(0);
                if (select.WhereClause != null)
                {
                    TExpression expr = select.WhereClause.Condition;

                    Console.WriteLine("pre order");
                    expr.preOrderTraverse(new exprVisitor());

                    Console.WriteLine("\nin order");
                    expr.inOrderTraverse(new exprVisitor());

                    Console.WriteLine("\npost order");
                    expr.postOrderTraverse(new exprVisitor());
                    expr.postOrderTraverse(new exprVisitor());
                }
			}
			else
			{
				Console.WriteLine(sqlparser.Errormessage);
			}
		}

	}

	internal class exprVisitor : IExpressionVisitor
	{

		public virtual bool exprVisit(TParseTreeNode pNode, bool isLeafNode)
		{
			string sign = "";
			if (isLeafNode)
			{
				sign = "*";
			}
			 Console.WriteLine(sign + pNode.GetType().ToString() + " " + pNode.ToString());
			return true;
		}
	}

}