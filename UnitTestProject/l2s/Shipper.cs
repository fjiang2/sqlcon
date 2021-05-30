using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "Shippers")]
	public partial class Shipper
	{
		[Column(Name = "ShipperID", IsPrimaryKey = true, IsDbGenerated = true)]
		public int ShipperID { get; set; }
		
		[Column(Name = "CompanyName", CanBeNull = false)]
		public string CompanyName { get; set; }
		
		[Column(Name = "Phone")]
		public string Phone { get; set; }
		
		private EntitySet<Order> _Orders;
		
		public Shipper()
		{
			this._Orders = new EntitySet<Order>();
		}
		
		[Association(Name = "Shipper_Order", Storage = "_Orders", ThisKey = "ShipperID", OtherKey = "ShipVia", IsForeignKey = false)]
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
	}
}