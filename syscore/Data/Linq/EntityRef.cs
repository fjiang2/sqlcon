using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data.Linq
{
    public struct EntityRef<TEntity>
        where TEntity : class
    {
        public TEntity Entity { get; set; }

        public EntityRef(TEntity entity)
        {
            this.Entity = entity;
        }

        public EntityRef(IEnumerable<TEntity> entities)
        {
            this.Entity = entities.FirstOrDefault();
        }

        public EntityRef(EntityRef<TEntity> entityRef)
        {
            this.Entity = entityRef.Entity;
        }

        public override string ToString()
        {
            if (Entity != null)
                return Entity.ToString();
            else
                return "NULL";
        }

        public static explicit operator TEntity(EntityRef<TEntity> entityRef)
        {
            return entityRef.Entity;
        }

        //public static implicit operator EntityRef<TEntity>(TEntity entity)
        //{
        //    return new EntityRef<TEntity>(entity);
        //}
    }
}
