using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gudusoft.gsqlparser.demos.util
{
    using gudusoft.gsqlparser;
    using EDbVendor = gudusoft.gsqlparser.EDbVendor;
    public class Common
    {
        public static EDbVendor GetEDbVendor(string[] args)
        {
            EDbVendor dbVendor = EDbVendor.dbvoracle;
            return GetEDbVendor(args, dbVendor);
        }

        internal static EDbVendor GetEDbVendor(string[] args, EDbVendor defaultVendor)
        {
            List<string> argList = new List<string>(args);
            int index = argList.IndexOf("/t");
            EDbVendor dbVendor = defaultVendor;
            if (index != -1 && args.Length > index + 1)
            {
                if (args[index + 1].Equals("mssql", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvmssql;
                }
                else if (args[index + 1].Equals("db2", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvdb2;
                }
                else if (args[index + 1].Equals("mysql", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvmysql;
                }
                else if (args[index + 1].Equals("netezza", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvnetezza;
                }
                else if (args[index + 1].Equals("teradata", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvteradata;
                }
                else if (args[index + 1].Equals("oracle", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvoracle;
                }
                else if (args[index + 1].Equals("informix", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvinformix;
                }
                else if (args[index + 1].Equals("sybase", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvsybase;
                }
                else if (args[index + 1].Equals("postgresql", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvpostgresql;
                }
                else if (args[index + 1].Equals("hive", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvhive;
                }
                else if (args[index + 1].Equals("greenplum", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvgreenplum;
                }
                else if (args[index + 1].Equals("redshift", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvredshift;
                }
                else if (args[index + 1].Equals("mdx", StringComparison.CurrentCultureIgnoreCase))
                {
                    dbVendor = EDbVendor.dbvmdx;
                }
            }

            return dbVendor;
        }
    }
}
