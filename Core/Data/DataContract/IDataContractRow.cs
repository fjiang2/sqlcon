using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sys.Data
{
    public interface IDataContractRow<T>
    {
        void UpdateObject(DataRow row);
        void UpdateRow(DataRow row);
    }
}
