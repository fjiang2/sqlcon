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

    public class EmployeesAssociation
    {
        public EntitySet<EmployeeTerritories> EmployeeTerritory { get; set; }
        public EntitySet<Orders> Order { get; set; }
        public EntityRef<Employees> Employee { get; set; }
    }


    public static class ProductsAssociationExtension
    {
        public static ProductsAssociation GetAssociation(this Products entity)
        {
            return entity.AsEnumerable().GetAssociation().FirstOrDefault();
        }

        public static IEnumerable<ProductsAssociation> GetAssociation(this IEnumerable<Products> entites)
        {
            var reader = entites.Expand();

            var associations = new List<ProductsAssociation>();

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

        public static EmployeesAssociation GetAssociation(this Employees entity)
        {
            return entity.AsEnumerable().GetAssociation().FirstOrDefault();
        }

        public static IEnumerable<EmployeesAssociation> GetAssociation(this IEnumerable<Employees> entites)
        {
            var reader = entites.Expand();

            var associations = new List<EmployeesAssociation>();

            var _EmployeeTerritories = reader.Read<EmployeeTerritories>();
            var _Orders = reader.Read<Orders>();
            var _Employees = reader.Read<Employees>();

            foreach (var entity in entites)
            {
                var association = new EmployeesAssociation
                {
                    EmployeeTerritory = new EntitySet<EmployeeTerritories>(_EmployeeTerritories.Where(row => row.EmployeeID == entity.EmployeeID)),
                    Order = new EntitySet<Orders>(_Orders.Where(row => row.EmployeeID == entity.EmployeeID)),
                    Employee = new EntityRef<Employees>(_Employees.FirstOrDefault(row => row.ReportsTo == entity.EmployeeID)),
                };

                associations.Add(association);
            }

            return associations;
        }
    }
}
