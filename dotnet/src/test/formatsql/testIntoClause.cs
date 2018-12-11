using System;

namespace gudusoft.gsqlparser.test.formatsql
{
	/*
	 * Date: 11-3-22
	 */

	using EDbVendor = gudusoft.gsqlparser.EDbVendor;
	using TGSqlParser = gudusoft.gsqlparser.TGSqlParser;
	using GFmtOpt = gudusoft.gsqlparser.pp.para.GFmtOpt;
	using GFmtOptFactory = gudusoft.gsqlparser.pp.para.GFmtOptFactory;
	using FormatterFactory = gudusoft.gsqlparser.pp.stmtformatter.FormatterFactory;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

	public class testIntoClause
	{
        [TestMethod()]
        /// <summary>
        /// gFmtOpt.IntoClauseInNewline not implemented
        /// No need to implement in this version as it not in document before.
        /// </summary>
        public void testSelectIntoClause()
	   {
			GFmtOpt option = GFmtOptFactory.newInstance(this.GetType().Name+"."+ new System.Diagnostics.StackTrace().GetFrame(0).GetMethod().Name);

			TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
			sqlparser.sqltext = "select col1, col2,sum(col3) INTO  Persons_backup from table1";
			sqlparser.parse();

			string result = FormatterFactory.pp(sqlparser, option);
		   //Assert.IsTrue("gFmtOpt.IntoClauseInNewline not implemented",false);
			Console.WriteLine("gFmtOpt.IntoClauseInNewline not implemented");
	   }


	}

}