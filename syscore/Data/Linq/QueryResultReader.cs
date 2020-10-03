using System;
using System.Data;
using System.Collections.Generic;

namespace Sys.Data.Linq
{
    public class QueryResultReader
    {
        private DataContext db;
        private Type[] types;
        private DataSet ds;

        internal QueryResultReader(DataContext db, Type[] types, DataSet ds)
        {
            this.db = db;
            this.types = types;
            this.ds = ds;
        }

        public List<TEntity> ToList<TEntity>() where TEntity : class
        {
            for (int i = 0; i < types.Length; i++)
            {
                if (typeof(TEntity) == types[i])
                    return db.GetTable<TEntity>().ToList(ds.Tables[i]);
            }

            return null;
        }

        public override string ToString()
        {
            return ds.ToString();
        }
    }
}
