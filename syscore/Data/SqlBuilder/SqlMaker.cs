using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Sys.Data.Linq;

namespace Sys.Data
{
    public class SqlMaker : SqlColumnValuePairCollection
    {
        public string TableName { get; }
        public string[] PrimaryKeys { get; set; }
        public string[] IdentityKeys { get; set; }

        /// <summary>
        /// Search condition, 
        /// Use primary-keys as search condition if this property is not empty.
        /// </summary>
        public string Where { get; set; } = string.Empty;

        private SqlTemplate template;

        public SqlMaker(string formalName)
        {
            this.TableName = formalName;
            this.template = new SqlTemplate(formalName);
        }

        public override SqlColumnValuePair Add(string name, object value)
        {
            var pair = base.Add(name, value);

            pair.Field.Primary = PrimaryKeys != null && PrimaryKeys.Contains(name);
            pair.Field.Identity = IdentityKeys != null && IdentityKeys.Contains(name);

            return pair;
        }

        private string[] notUpdateColumns => columns.Where(p => !p.Field.Saved).Select(p => p.Field.Name).ToArray();


        public string Select()
        {
            string where = Condition();
            if (string.IsNullOrEmpty(where))
                return template.Select("*");

            return template.Select("*", where);
        }

        public string SelectRows() => SelectRows("*");

        public string SelectRows(IEnumerable<string> columns)
        {
            var L1 = string.Join(",", columns.Select(c => SqlColumnValuePair.FormalName(c)));
            if (L1 == string.Empty)
                L1 = "*";

            return SelectRows(L1);
        }

        private string SelectRows(string columns) => template.Select(columns);

        public string InsertOrUpdate(bool? exists)
        {
            if (exists == false)
                return Insert();

            if (exists == true)
                return Update();

            return InsertOrUpdate();
        }

        public string InsertOrUpdate()
        {
            string where = Condition();
            if(string.IsNullOrEmpty(where))
            {
                throw new InvalidOperationException("WHERE is blank");
            }

            if (PrimaryKeys.Length + notUpdateColumns.Length == columns.Count)
            {
                return template.IfNotExistsInsert(where, Insert());
            }
            else
            {
                return template.IfExistsUpdateElseInsert(where, Update(), Insert());
            }
        }

        public string Insert()
        {
            var C = columns.Where(c => !c.Field.Identity && !c.Value.IsNull);
            var L1 = string.Join(",", C.Select(c => c.ColumnFormalName));
            var L2 = string.Join(",", C.Select(c => c.Value.ToString()));

            return template.Insert(L1, L2);
        }

        public string Update()
        {
            var C2 = columns.Where(c => !PrimaryKeys.Contains(c.ColumnName) && !notUpdateColumns.Contains(c.ColumnName));
            var L2 = string.Join(",", C2.Select(c => c.ToString()));

            if (C2.Count() == 0)
                return string.Empty;

            string where = Condition();

            if (string.IsNullOrEmpty(where))
                return template.Update(L2);

            return template.Update(L2, where);
        }

        public string Delete()
        {
            string where = Condition();

            if(string.IsNullOrEmpty(where))
                return template.Delete();

            return template.Delete(where);
        }

        public string DeleteAll()
        {
            return template.Delete();
        }

        private string Condition()
        {
            if (!string.IsNullOrEmpty(Where))
                return Where;

            if (PrimaryKeys.Length > 0)
            {
                var C1 = columns.Where(c => PrimaryKeys.Contains(c.ColumnName));
                var L1 = string.Join(" AND ", C1.Select(c => c.ToString()));
                return L1;
            }

            return string.Empty;
        }
    }
}
