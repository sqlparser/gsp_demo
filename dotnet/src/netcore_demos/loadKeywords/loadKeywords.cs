using gudusoft.gsqlparser;
using System;
using System.Collections.Generic;
using System.IO;
using util = gudusoft.gsqlparser.util;


namespace gudusoft.gsqlparser.demos.loadKeywords
{
    class Program
    {
        static void Main(string[] args)
        {
			string searchedKeyword = "select";
			string searchedFunction = "substr";
			if (util.keywordChecker.isKeyword(searchedKeyword,EDbVendor.dbvsybase, "15.7", true)){
				Console.WriteLine(searchedKeyword+ " is a keyword");	
			}else{
				Console.WriteLine(searchedKeyword+ " is not a keyword");	
			};
			
			if (util.functionChecker.isBuiltInFunction(searchedFunction,EDbVendor.dbvsybase, "15.7")){
				Console.WriteLine(searchedFunction+ " is a function");	
			}else{
				Console.WriteLine(searchedFunction+ " is not a function");	
			};
            
        }
    }
}
