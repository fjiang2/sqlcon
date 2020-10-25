using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "EmployeeTerritories")]
	public partial class EmployeeTerritory
	{
		[Column(Name = "EmployeeID", IsPrimaryKey = true)]
		public int EmployeeID { get; set; }
		
		[Column(Name = "TerritoryID", IsPrimaryKey = true)]
		public string TerritoryID { get; set; }
		
		private EntityRef<Employee> _Employee;
		private EntityRef<Territory> _Territory;
		
		[Association(Name = "Employee_EmployeeTerritory", Storage = "_Employee", ThisKey = "EmployeeID", OtherKey = "EmployeeID", IsForeignKey = true)]
		public Employee Employee
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
		
		[Association(Name = "Territory_EmployeeTerritory", Storage = "_Territory", ThisKey = "TerritoryID", OtherKey = "TerritoryID", IsForeignKey = true)]
		public Territory Territory
		{
			get
			{
				return this._Territory.Entity;
			}
			set
			{
				this._Territory.Entity = value;
			}
		}
	}
}