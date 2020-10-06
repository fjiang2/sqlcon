using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sys.Data.Linq;

namespace UnitTestProject.Northwind.dbo
{
    public class ProductsAssociation
    {
        public EntitySet<Order_Details> Order_Details { get; set; }
        public EntityRef<Suppliers> Supplier { get; set; }
        public EntityRef<Categories> Category { get; set; }

    }


    public static class ProductsAssociationExtension
    {
        public static ProductsAssociation GetAssociation(this Products entity)
        {
            using (var db = new DataContext())
            {
                db.ExpandOnSubmit(entity);
                var reader = db.SumbitQueries();

                var _Order_Details = reader.Read<Order_Details>();
                var _Suppliers = reader.Read<Suppliers>();
                var _Categories = reader.Read<Categories>();

                return new ProductsAssociation
                {
                    Order_Details = new EntitySet<Order_Details>(_Order_Details),
                    Supplier = new EntityRef<Suppliers>(_Suppliers),
                    Category = new EntityRef<Categories>(_Categories),
                };
            }
        }

        public static IEnumerable<ProductsAssociation> GetAssociation(this IEnumerable<Products> entites)
        {
            using (var db = new DataContext())
            {
                db.ExpandOnSubmit(entites);
                var reader = db.SumbitQueries();

                List<ProductsAssociation> associations = new List<ProductsAssociation>();

                var _Order_Details = reader.Read<Order_Details>();
                var _Suppliers = reader.Read<Suppliers>();
                var _Categories = reader.Read<Categories>();

                foreach (var entity in entites)
                {
                    var association = new ProductsAssociation
                    {
                        Order_Details = new EntitySet<Order_Details>(_Order_Details.Where(row => row.ProductID == entity.ProductID)),
                        Supplier = new EntityRef<Suppliers>(_Suppliers.FirstOrDefault(row => row.SupplierID == entity.SupplierID)),
                        Category = new EntityRef<Categories>(_Categories.FirstOrDefault(row => row.CategoryID == entity.CategoryID)),
                    };

                    associations.Add(association);
                }

                return associations;
            }
        }
    }
}
