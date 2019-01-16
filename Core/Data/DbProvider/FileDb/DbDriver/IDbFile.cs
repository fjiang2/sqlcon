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
        /// <param name="select">Sql select statement</param>
        /// <param name="ds">read result</param>
        /// <returns>number of rows retrieved</returns>
        int ReadData(FileLink link, SelectClause select, DataSet ds);

     

        /// <summary>
        /// read db schema
        /// </summary>
        /// <param name="link"></param>
        /// <param name="dbSchema"></param>
        void ReadSchema(FileLink link, DataSet dbSchema);
    }

    
}