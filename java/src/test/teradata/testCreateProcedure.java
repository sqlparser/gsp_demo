package test.teradata;
/*
 * Date: 14-8-14
 */

import gudusoft.gsqlparser.EDbVendor;
import gudusoft.gsqlparser.ESqlStatementType;
import gudusoft.gsqlparser.TGSqlParser;
import gudusoft.gsqlparser.stmt.TMergeSqlStatement;
import gudusoft.gsqlparser.stmt.teradata.TTeradataCreateProcedure;
import junit.framework.TestCase;

public class testCreateProcedure extends TestCase {

    public void test1(){

     TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvteradata);
     sqlparser.sqltext = "create proc merge_salesdetail\n" +
             "as\n" +
             "merge into salesdetail as s\n" +
             "using salesdetailupdates as u \n" +
             "on s.stor_id = u.stor_id and\n" +
             "  s.ord_num = u.ord_num and\n" +
             "  s.title_id = u.title_id\n" +
             "when not matched then   \n" +
             "    insert (stor_id, ord_num, title_id, qty, discount) values(u.stor_id, u.ord_num, u.title_id, u.qty, u.discount) \n" +
             "when matched then   \n" +
             "    update set qty=u.qty, discount=u.discount";
     assertTrue(sqlparser.parse() == 0);

        TTeradataCreateProcedure cp = (TTeradataCreateProcedure)sqlparser.sqlstatements.get(0);
        assertTrue(cp.getProcedureName().toString().equalsIgnoreCase("merge_salesdetail"));

        assertTrue(cp.getBodyStatements().size() == 1);

       //System.out.println(cp.getBodyStatements().get(0).sqlstatementtype.toString());

        assertTrue(cp.getBodyStatements().get(0).sqlstatementtype == ESqlStatementType.sstmerge);
        TMergeSqlStatement merge = (TMergeSqlStatement)cp.getBodyStatements().get(0);
        assertTrue(merge.getTargetTable().toString().equalsIgnoreCase("salesdetail"));
    }

}
