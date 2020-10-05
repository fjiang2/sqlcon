using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "Orders")]
	public partial class Order
	{
		[Column(Name = "OrderID", IsPrimaryKey = true, IsDbGenerated = true)]
		public int OrderID { get; set; }
		
		[Column(Name = "CustomerID")]
		public string CustomerID { get; set; }
		
		[Column(Name = "EmployeeID")]
		public int? EmployeeID { get; set; }
		
		[Column(Name = "OrderDate")]
		public DateTime? OrderDate { get; set; }
		
		[Column(Name = "RequiredDate")]
		public DateTime? RequiredDate { get; set; }
		
		[Column(Name = "ShippedDate")]
		public DateTime? ShippedDate { get; set; }
		
		[Column(Name = "ShipVia")]
		public int? ShipVia { get; set; }
		
		[Column(Name = "Freight")]
		public decimal? Freight { get; set; }
		
		[Column(Name = "ShipName")]
		public string ShipName { get; set; }
		
		[Column(Name = "ShipAddress")]
		public string ShipAddress { get; set; }
		
		[Column(Name = "ShipCity")]
		public string ShipCity { get; set; }
		
		[Column(Name = "ShipRegion")]
		public string ShipRegion { get; set; }
		
		[Column(Name = "ShipPostalCode")]
		public string ShipPostalCode { get; set; }
		
		[Column(Name = "ShipCountry")]
		public string ShipCountry { get; set; }
		
		private EntitySet<Order_Detail> _Order_Detail;
		
		private EntityRef<Customer> _Customer;
		private EntityRef<Employee> _Employee;
		private EntityRef<Shipper> _Shipper;
		
		public Order()
		{
			this._Order_Detail = new EntitySet<Order_Detail>();
		}
		
		[Association(Name = "Order_Order_Detail", Storage = "_Order_Detail", ThisKey = "OrderID", OtherKey = "OrderID", IsForeignKey = false)]
		public EntitySet<Order_Detail> Order_Detail
		{
			get
			{
				return this._Order_Detail;
			}
			set
			{
				this._Order_Detail.Assign(value);
			}
		}
		
		[Association(Name = "Customer_Order", Storage = "_Customer", ThisKey = "CustomerID", OtherKey = "CustomerID", IsForeignKey = true)]
		public Customer Customer
		{
			get
			{
				return this._Customer.Entity;
			}
			set
			{
				this._Customer.Entity = value;
			}
		}
		
		[Association(Name = "Employee_Order", Storage = "_Employee", ThisKey = "EmployeeID", OtherKey = "EmployeeID", IsForeignKey = true)]
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
		
		[Association(Name = "Shipper_Order", Storage = "_Shipper", ThisKey = "ShipVia", OtherKey = "ShipperID", IsForeignKey = true)]
		public Shipper Shipper
		{
			get
			{
				return this._Shipper.Entity;
			}
			set
			{
				this._Shipper.Entity = value;
			}
		}
	}
}