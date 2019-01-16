using System.Data;
using Sys.Data.IO;

namespace Sys.Data
{
    public interface IDbCreator
    { 
        /// <summary>
        /// write database schema
        /// </summary>
        /// <param name="dname"></param>
        /// <returns>schema file name</returns>
        string WriteSchema(DatabaseName dname);


        /// <summary>
        /// write db server schema
        /// </summary>
        /// <param name="sname"></param>
        /// <returns>schema file anme</returns>
        string WriteSchema(ServerName sname);


        /// <summary>
        /// write data into a file
        /// </summary>
        /// <param name="tname"></param>
        /// <param name="dt"></param>
        /// <returns>data file name</returns>
        string WriteData(TableName tname, DataTable dt);
    }

    
}