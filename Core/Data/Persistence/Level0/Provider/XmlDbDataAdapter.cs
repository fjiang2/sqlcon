﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data;

namespace Sys.Data
{
    public class XmlDbDataAdapter : DbDataAdapter
    {
        public XmlDbDataAdapter()
        {
        }

        public override int Fill(DataSet dataSet)
        {
            string sql = this.SelectCommand.CommandText;
            return -1;
        }
    }
}
