using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "Order Details")]
	public partial class Order_Detail
	{
		[Column(Name = "OrderID", IsPrimaryKey = true)]
		public int OrderID { get; set; }
		
		[Column(Name = "ProductID", IsPrimaryKey = true)]
		public int ProductID { get; set; }
		
		[Column(Name = "UnitPrice", CanBeNull = false)]
		public decimal UnitPrice { get; set; }
		
		[Column(Name = "Quantity", CanBeNull = false)]
		public short Quantity { get; set; }
		
		[Column(Name = "Discount", CanBeNull = false)]
		public Single Discount { get; set; }
		
		private EntityRef<Order> _Order;
		private EntityRef<Product> _Product;
		
		[Association(Name = "Order_Order_Detail", Storage = "_Order", ThisKey = "OrderID", OtherKey = "OrderID", IsForeignKey = true)]
		public Order Order
		{
			get
			{
				return this._Order.Entity;
			}
			set
			{
				this._Order.Entity = value;
			}
		}
		
		[Association(Name = "Product_Order_Detail", Storage = "_Product", ThisKey = "ProductID", OtherKey = "ProductID", IsForeignKey = true)]
		public Product Product
		{
			get
			{
				return this._Product.Entity;
			}
			set
			{
				this._Product.Entity = value;
			}
		}
	}
}