package demos.search;

import gudusoft.gsqlparser.TGSqlParser;
import gudusoft.gsqlparser.EDbVendor;
import gudusoft.gsqlparser.nodes.TParseTreeVisitor;
import gudusoft.gsqlparser.nodes.THierarchical;

import java.io.File;
import java.io.FilenameFilter;
import java.util.ArrayList;

/**
 * search sql files that include specified class name in a directory recursively..
 * Usage:
 * searchClause class_name directory
 *
 * You need to modify searchVisitor to add support to search other clause, it support THierarchical only. 
*/
public class searchClause {

    public static void main(String args[])
    {
        long t;
        t = System.currentTimeMillis();

        if (args.length != 2){
            System.out.println("Usage: java searchClause class_name directory");
            return;
        }

        String class_name = args[0];
        String dir = args[1];
        int ret;

        TGSqlParser sqlparser = new TGSqlParser(EDbVendor.dbvoracle);
        SqlFileList sqlfiles = new SqlFileList(dir);
        for(int k=0;k < sqlfiles.sqlfiles.size()-1;k++){
            sqlparser.sqlfilename = sqlfiles.sqlfiles.get(k).toString();
            ret = sqlparser.parse();
            if (ret == 0){
                for(int i=0;i<sqlparser.sqlstatements.size();i++){
                searchVisitor sv = new searchVisitor(class_name);
                sqlparser.sqlstatements.get(i).accept(sv);
                if (sv.isFound()) {
                    System.out.println(sqlparser.sqlfilename);
                    break;
                }
                }
            }
        }
        System.out.println("Time Escaped: "+ (System.currentTimeMillis() - t) );
    }

}

class searchVisitor extends TParseTreeVisitor {
    private boolean found = false;
    private String class_name = null;

    public boolean isFound() {
        return found;
    }

    public searchVisitor(String c){
        this.class_name = c;

    }
    
    public void preVisit(THierarchical node){
        if (node.getClass().getSimpleName().compareToIgnoreCase(this.class_name) == 0){
            found = true;
        }
    }
}

class SqlFileList {
    String dir;
    FilenameFilter ffobj;
    public ArrayList sqlfiles;
    public  SqlFileList(String dir){
       this.dir = dir;
       this.ffobj = ffobj;
        sqlfiles = new ArrayList();
        getfiles(this.dir);
    }

    void getfiles(String pdir){
        File f1 = new File(pdir);
        if(f1.isDirectory()){
          File[]  fs = f1.listFiles();
            for (int i=0;i<fs.length;i++){
                if(fs[i].isDirectory()){
                    getfiles(pdir+"/"+fs[i].getName());
                }else{
                    if (fs[i].getName().endsWith("sql"))
                      sqlfiles.add(pdir+"/"+fs[i].getName());
                }
            }
        }
    }
}