using System;

namespace gudusoft.gsqlparser.demos.lib
{
    /*
	 * Date: 11-5-5
	 */

    using TCustomSqlStatement = gudusoft.gsqlparser.TCustomSqlStatement;
    using IExpressionVisitor = gudusoft.gsqlparser.nodes.IExpressionVisitor;
    using TExpression = gudusoft.gsqlparser.nodes.TExpression;
    using TGroupByItemList = gudusoft.gsqlparser.nodes.TGroupByItemList;
    using TObjectName = gudusoft.gsqlparser.nodes.TObjectName;
    using TOrderBy = gudusoft.gsqlparser.nodes.TOrderBy;
    using TParseTreeNode = gudusoft.gsqlparser.nodes.TParseTreeNode;
    using TParseTreeVisitor = gudusoft.gsqlparser.nodes.TParseTreeVisitor;
    using TTable = gudusoft.gsqlparser.nodes.TTable;
    using gudusoft.gsqlparser;

    public class columnInClause
	{

		public columnInClause()
		{
		}

		public virtual void printColumns(TExpression expression, TCustomSqlStatement statement)
		{
			Console.WriteLine("Referenced columns:");
			columnVisitor cv = new columnVisitor(statement);
			expression.postOrderTraverse(cv);
		}

		public virtual void printColumns(TGroupByItemList list, TCustomSqlStatement statement)
		{
			Console.WriteLine("Referenced columns:");
			groupByVisitor gbv = new groupByVisitor(statement);
			list.accept(gbv);
		}

		public virtual void printColumns(TOrderBy orderBy, TCustomSqlStatement statement)
		{
			Console.WriteLine("Referenced columns:");
			orderByVisitor obv = new orderByVisitor(statement);
			orderBy.accept(obv);
		}


	}

	internal class columnVisitor : IExpressionVisitor
	{

		internal TCustomSqlStatement statement = null;

		public columnVisitor(TCustomSqlStatement statement)
		{
			this.statement = statement;
		}

		internal virtual string getColumnWithBaseTable(TObjectName objectName)
		{
			string ret = "";
			TTable table = null;
			bool find = false;
			TCustomSqlStatement lcStmt = statement;

			while ((lcStmt != null) && (!find))
			{
				for (int i = 0;i < lcStmt.tables.size();i++)
				{
					table = lcStmt.tables.getTable(i);
					for (int j = 0;j < table.LinkedColumns.size();j++)
					{
						if (objectName == table.LinkedColumns.getObjectName(j))
						{
							if (table.BaseTable)
							{
								ret = table.TableName + "." + objectName.ColumnNameOnly;
							}
							else
							{
								//derived table
								if (table.AliasClause != null)
								{
								   ret = table.AliasClause.ToString() + "." + objectName.ColumnNameOnly;
								}
								else
								{
									ret = objectName.ColumnNameOnly;
								}

								ret += "(column in derived table)";
							}
							find = true;
							break;
						}
					}
				}
				if (!find)
				{
					lcStmt = lcStmt.ParentStmt;
				}
			}

			return ret;
		}

		public virtual bool exprVisit(TParseTreeNode pNode, bool isLeafNode)
		{
			 TExpression expr = (TExpression)pNode;
			 switch ((expr.ExpressionType))
			 {
				 case EExpressionType.simple_object_name_t:
					 TObjectName obj = expr.ObjectOperand;
					 if (obj.ObjectType != TObjectName.ttobjNotAObject)
					 {
						Console.WriteLine(getColumnWithBaseTable(obj));
					 }
					 break;
				 case EExpressionType.function_t:
					 functionCallVisitor fcv = new functionCallVisitor(statement);
					 expr.FunctionCall.accept(fcv);
					 break;
			 }
			 return true;
		}

	}

	internal class functionCallVisitor : TParseTreeVisitor
	{

		internal TCustomSqlStatement statement = null;

		public functionCallVisitor(TCustomSqlStatement statement)
		{
			this.statement = statement;
		}

		public override void preVisit(TExpression expression)
		{
			columnVisitor cv = new columnVisitor(statement);
			expression.postOrderTraverse(cv);
		}
	}

	internal class groupByVisitor : TParseTreeVisitor
	{

		internal TCustomSqlStatement statement = null;

		public groupByVisitor(TCustomSqlStatement statement)
		{
			this.statement = statement;
		}

		public override void preVisit(TExpression expression)
		{
			columnVisitor cv = new columnVisitor(statement);
			expression.postOrderTraverse(cv);
		}
	}

	internal class orderByVisitor : TParseTreeVisitor
	{

		internal TCustomSqlStatement statement = null;

		public orderByVisitor(TCustomSqlStatement statement)
		{
			this.statement = statement;
		}

		public override void preVisit(TExpression expression)
		{
			columnVisitor cv = new columnVisitor(statement);
			expression.postOrderTraverse(cv);
		}
	}

}