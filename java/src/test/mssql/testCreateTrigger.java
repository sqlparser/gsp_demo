package test.mssql;


import gudusoft.gsqlparser.EDbVendor;
import gudusoft.gsqlparser.ESqlStatementType;
import gudusoft.gsqlparser.TGSqlParser;
import gudusoft.gsqlparser.stmt.mssql.TMssqlCreateTrigger;
import junit.framework.TestCase;

public class testCreateTrigger extends TestCase {

    public void test1(){

        TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvmssql);
        sqlparser.sqltext = "CREATE TRIGGER reminder\n" +
                "ON titles\n" +
                "FOR INSERT, UPDATE \n" +
                "AS RAISERROR (50009, 16, 10)\n" +
                "GO";
        int result = sqlparser.parse();
        assertTrue(result==0);
        assertTrue(sqlparser.sqlstatements.get(0).sqlstatementtype == ESqlStatementType.sstmssqlcreatetrigger);
        TMssqlCreateTrigger createTrigger = (TMssqlCreateTrigger)sqlparser.sqlstatements.get(0);
        assertTrue(createTrigger.getDmlTypes().toString().equalsIgnoreCase("[tdtInsert, tdtUpdate]"));
        assertTrue(createTrigger.getTimingPoint().toString().equalsIgnoreCase("ttpFor"));
    }
}
