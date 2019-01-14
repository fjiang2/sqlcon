using System.Data;
using Sys.Data.IO;

namespace Sys.Data
{
    public interface IDbFile
    {
        
        /// <summary>
        /// read table's data from data file with filtering
        /// </summary>
        /// <param name="link">data file</param>
        /// <param name="tname">table to be wanted to read</param>
        /// <param name="ds">read result</param>
        /// <param name="where">filter expression</param>
        /// <returns></returns>
        int ReadData(FileLink link, TableName tname, DataSet ds, string where);

        /// <summary>
        /// write data into a file
        /// </summary>
        /// <param name="tname"></param>
        /// <param name="dt"></param>
        /// <returns>data file name</returns>
        string WriteData(TableName tname, DataTable dt);

        /// <summary>
        /// read db schema
        /// </summary>
        /// <param name="link"></param>
        /// <param name="dbSchema"></param>
        void ReadSchema(FileLink link, DataSet dbSchema);

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
    }

    
}