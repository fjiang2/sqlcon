using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "Employees")]
	public partial class Employee
	{
		[Column(Name = "EmployeeID", IsPrimaryKey = true, IsDbGenerated = true)]
		public int EmployeeID { get; set; }
		
		[Column(Name = "LastName", CanBeNull = false)]
		public string LastName { get; set; }
		
		[Column(Name = "FirstName", CanBeNull = false)]
		public string FirstName { get; set; }
		
		[Column(Name = "Title")]
		public string Title { get; set; }
		
		[Column(Name = "TitleOfCourtesy")]
		public string TitleOfCourtesy { get; set; }
		
		[Column(Name = "BirthDate")]
		public DateTime? BirthDate { get; set; }
		
		[Column(Name = "HireDate")]
		public DateTime? HireDate { get; set; }
		
		[Column(Name = "Address")]
		public string Address { get; set; }
		
		[Column(Name = "City")]
		public string City { get; set; }
		
		[Column(Name = "Region")]
		public string Region { get; set; }
		
		[Column(Name = "PostalCode")]
		public string PostalCode { get; set; }
		
		[Column(Name = "Country")]
		public string Country { get; set; }
		
		[Column(Name = "HomePhone")]
		public string HomePhone { get; set; }
		
		[Column(Name = "Extension")]
		public string Extension { get; set; }
		
		[Column(Name = "Photo")]
		public byte[] Photo { get; set; }
		
		[Column(Name = "Notes", UpdateCheck = UpdateCheck.Never)]
		public string Notes { get; set; }
		
		[Column(Name = "ReportsTo")]
		public int? ReportsTo { get; set; }
		
		[Column(Name = "PhotoPath")]
		public string PhotoPath { get; set; }
		
		private EntitySet<EmployeeTerritory> _EmployeeTerritories;
		private EntitySet<Order> _Orders;
		
		private EntityRef<Employee> _Employee;
		
		public Employee()
		{
			this._EmployeeTerritories = new EntitySet<EmployeeTerritory>();
			this._Orders = new EntitySet<Order>();
		}
		
		[Association(Name = "Employee_EmployeeTerritory", Storage = "_EmployeeTerritories", ThisKey = "EmployeeID", OtherKey = "EmployeeID", IsForeignKey = false)]
		public EntitySet<EmployeeTerritory> EmployeeTerritories
		{
			get
			{
				return this._EmployeeTerritories;
			}
			set
			{
				this._EmployeeTerritories.Assign(value);
			}
		}
		
		[Association(Name = "Employee_Order", Storage = "_Orders", ThisKey = "EmployeeID", OtherKey = "EmployeeID", IsForeignKey = false)]
		public EntitySet<Order> Orders
		{
			get
			{
				return this._Orders;
			}
			set
			{
				this._Orders.Assign(value);
			}
		}
		
		[Association(Name = "Employee_Employee", Storage = "_Employee", ThisKey = "ReportsTo", OtherKey = "EmployeeID", IsForeignKey = true)]
		public Employee Employee1
		{
			get
			{
				return this._Employee.Entity;
			}
			set
			{
				this._Employee.Entity = value;
			}
		}
	}
}