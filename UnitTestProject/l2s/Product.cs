using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;

namespace UnitTestProject.Northwind.l2s
{
	[Table(Name = "Products")]
	public partial class Product
	{
		[Column(Name = "ProductID", IsPrimaryKey = true, IsDbGenerated = true)]
		public int ProductID { get; set; }
		
		[Column(Name = "ProductName", CanBeNull = false)]
		public string ProductName { get; set; }
		
		[Column(Name = "SupplierID")]
		public int? SupplierID { get; set; }
		
		[Column(Name = "CategoryID")]
		public int? CategoryID { get; set; }
		
		[Column(Name = "QuantityPerUnit")]
		public string QuantityPerUnit { get; set; }
		
		[Column(Name = "UnitPrice")]
		public decimal? UnitPrice { get; set; }
		
		[Column(Name = "UnitsInStock")]
		public short? UnitsInStock { get; set; }
		
		[Column(Name = "UnitsOnOrder")]
		public short? UnitsOnOrder { get; set; }
		
		[Column(Name = "ReorderLevel")]
		public short? ReorderLevel { get; set; }
		
		[Column(Name = "Discontinued", CanBeNull = false)]
		public bool Discontinued { get; set; }
		
		private EntitySet<Order_Detail> _Order_Detail;
		
		private EntityRef<Supplier> _Supplier;
		private EntityRef<Category> _Category;
		
		public Product()
		{
			this._Order_Detail = new EntitySet<Order_Detail>();
		}
		
		[Association(Name = "Product_Order_Detail", Storage = "_Order_Detail", ThisKey = "ProductID", OtherKey = "ProductID", IsForeignKey = false)]
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
		
		[Association(Name = "Supplier_Product", Storage = "_Supplier", ThisKey = "SupplierID", OtherKey = "SupplierID", IsForeignKey = true)]
		public Supplier Supplier
		{
			get
			{
				return this._Supplier.Entity;
			}
			set
			{
				this._Supplier.Entity = value;
			}
		}
		
		[Association(Name = "Category_Product", Storage = "_Category", ThisKey = "CategoryID", OtherKey = "CategoryID", IsForeignKey = true)]
		public Category Category
		{
			get
			{
				return this._Category.Entity;
			}
			set
			{
				this._Category.Entity = value;
			}
		}
	}
}