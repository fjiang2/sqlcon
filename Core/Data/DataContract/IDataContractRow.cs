using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sys.Data
{
    public interface IDataContractRow
    {
        /// <summary>
        /// update object properties by data row; copy row->obj
        /// </summary>
        /// <param name="row"></param>
        void Fill(DataRow row);

        /// <summary>
        /// collect object property values and update row; copy obj->row
        /// </summary>
        /// <param name="row"></param>
        void Collect(DataRow row);
    }
}
