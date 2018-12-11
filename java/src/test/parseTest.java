package test;

import demos.visitors.xmlVisitor;
import junit.framework.TestCase;


import gudusoft.gsqlparser.EDbVendor;
import gudusoft.gsqlparser.TGSqlParser;


public class parseTest extends TestCase {
    String xsdfile = "file:/C:/prg/gsp_java/library/doc/xml/sqlquery.xsd";


void parsefiles(EDbVendor db,String dir)  {

    TGSqlParser sqlparser = new TGSqlParser(db);
    SqlFileList sqlfiles = new SqlFileList(dir,true);
    for(int k=0;k < sqlfiles.sqlfiles.size();k++){
        sqlparser.sqlfilename = sqlfiles.sqlfiles.get(k).toString();
        // System.out.printf("%s\n",sqlparser.sqlfilename);
        // boolean b = sqlparser.parse() == 0;

        try{
            boolean b = sqlparser.parse() == 0;
            assertTrue(sqlparser.sqlfilename+"\n"+sqlparser.getErrormessage(),b);

            if (b){
//                xmlVisitor xv2 = new xmlVisitor(xsdfile);
//                xv2.run(sqlparser);

                //xv2.validXml();
            }
        }catch (Exception e){
            System.out.println("parsefiles error:"+e.getMessage()+" "+ sqlparser.sqlfilename);
        }
    }

}


    public  void testOracle(){
        parsefiles(EDbVendor.dbvoracle,"c:/prg/gsqlparser/Test/TestCases/oracle");
        parsefiles(EDbVendor.dbvoracle,"c:/prg/gsqlparser/Test/TestCases/java/oracle/");
    }

    public  void testSQLServer(){
        parsefiles(EDbVendor.dbvmssql,"c:/prg/gsqlparser/Test/TestCases/mssql");
        parsefiles(EDbVendor.dbvmssql,"c:/prg/gsqlparser/Test/TestCases/java/mssql");
    }

    public  void testSybase(){
        parsefiles(EDbVendor.dbvsybase,"c:/prg/gsqlparser/Test/TestCases/sybase");
        parsefiles(EDbVendor.dbvsybase,"c:/prg/gsqlparser/Test/TestCases/java/sybase");
    }

    public  void testTeradata(){
        parsefiles(EDbVendor.dbvteradata,"c:/prg/gsqlparser/Test/TestCases/teradata/verified");
        parsefiles(EDbVendor.dbvteradata,"c:/prg/gsqlparser/Test/TestCases/java/teradata");
    }


    public  void testDB2(){
        parsefiles(EDbVendor.dbvdb2,"c:/prg/gsqlparser/Test/TestCases/db2");
        parsefiles(EDbVendor.dbvdb2,"c:/prg/gsqlparser/Test/TestCases/java/db2/");
    }

    public  void testMySQL(){
        parsefiles(EDbVendor.dbvmysql,"c:/prg/gsqlparser/Test/TestCases/mysql");
        parsefiles(EDbVendor.dbvmysql,"c:/prg/gsqlparser/Test/TestCases/java/mysql");

//        parsefiles(EDbVendor.dbvmysql,"C:\\prg\\sofia2.0\\sofia\\big-ds\\testing");
//        parsefiles(EDbVendor.dbvmysql,"C:\\prg\\sofia2.0\\sofia\\big-ds\\training");
//        parsefiles(EDbVendor.dbvmysql,"C:\\prg\\sofia2.0\\sofia\\hotelrs-x");
//        parsefiles(EDbVendor.dbvmysql,"C:\\prg\\sofia2.0\\sofia\\taskfreak-b");
//        parsefiles(EDbVendor.dbvmysql,"C:\\prg\\sofia2.0\\sofia\\wordpress-s");
//          parsefiles(EDbVendor.dbvmysql,"C:\\prg\\sofia2.0\\sofia\\theorganizer-s");
    }

    public  void testMdx(){
        parsefiles(EDbVendor.dbvmdx,"c:/prg/gsqlparser/Test/TestCases/mdx");
    }

    public  void testNetezza(){
        parsefiles(EDbVendor.dbvnetezza,"c:/prg/gsqlparser/Test/TestCases/netezza");
        parsefiles(EDbVendor.dbvnetezza,"c:/prg/gsqlparser/Test/TestCases/java/netezza");
    }

    public  void testInformix(){
        parsefiles(EDbVendor.dbvinformix,"c:/prg/gsqlparser/Test/TestCases/informix");
    }

    public  void testPostgresql(){
        parsefiles(EDbVendor.dbvpostgresql,"c:/prg/gsqlparser/Test/TestCases/postgresql/verified");
        parsefiles(EDbVendor.dbvpostgresql,"c:/prg/gsqlparser/Test/TestCases/java/postgresql");
    }

    public  void testGreenplum(){
        parsefiles(EDbVendor.dbvgreenplum,"c:/prg/gsqlparser/Test/TestCases/greenplum");
    }

    public  void testRedshift(){
        parsefiles(EDbVendor.dbvredshift,"c:/prg/gsqlparser/Test/TestCases/java/redshift");
    }

    public  void testHive(){
        parsefiles(EDbVendor.dbvhive,"c:/prg/gsqlparser/Test/TestCases/hive");
    }

    public  void testImpala(){
        parsefiles(EDbVendor.dbvimpala,"c:/prg/gsqlparser/Test/TestCases/impala");
        parsefiles(EDbVendor.dbvimpala,"c:/prg/gsqlparser/Test/TestCases/java/impala");
    }

    public  void testHana(){
        parsefiles(EDbVendor.dbvhana,"c:/prg/gsqlparser/Test/TestCases/hana");
    }
    public  void testDax(){
        parsefiles(EDbVendor.dbvdax,"c:/prg/gsqlparser/Test/TestCases/dax");
    }

    public  void testODBC(){
        parsefiles(EDbVendor.dbvodbc,"c:/prg/gsqlparser/Test/TestCases/odbc");
    }

    public  void testVertica(){
        parsefiles(EDbVendor.dbvvertica,"c:/prg/gsqlparser/Test/TestCases/vertica");
    }

    public  void testOpenedge(){
        parsefiles(EDbVendor.dbvopenedge,"c:/prg/gsqlparser/Test/TestCases/openedge");
    }

    public  void testCouchbase(){
        parsefiles(EDbVendor.dbvcouchbase,"c:/prg/gsqlparser/Test/TestCases/couchbase");
    }

    public  void testSnowflake(){
        parsefiles(EDbVendor.dbvsnowflake,"c:/prg/gsqlparser/Test/TestCases/snowflake");
    }
}
