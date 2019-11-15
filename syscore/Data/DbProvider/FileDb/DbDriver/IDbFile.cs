using System.Data;
using Sys.Data.IO;

namespace Sys.Data
{
    public interface IDbFile
    {

        /// <summary>
        /// read db schema
        /// </summary>
        /// <param name="link"></param>
        /// <param name="dbSchema"></param>
        void ReadSchema(DataSet dbSchema);

        /// <summary>
        /// read table's data from data file with filtering
        /// </summary>
        /// <param name="link">data file</param>
        /// <param name="select">Sql select statement</param>
        /// <param name="result">read result</param>
        /// <returns>number of rows retrieved</returns>
        int SelectData(SelectClause select, DataSet result);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="insertt"></param>
        /// <returns></returns>
        int InsertData(InsertClause insertt);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="update"></param>
        /// <returns></returns>
        int UpdateData(UpdateClause update);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delete"></param>
        /// <returns></returns>
        int DeleteData(DeleteClause delete);

    }


}