using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind.dbo
{
	public partial class Order_Details
	{
		public int OrderID { get; set; }
		public int ProductID { get; set; }
		public decimal UnitPrice { get; set; }
		public short Quantity { get; set; }
		public float Discount { get; set; }
	}
	
	public class Order_DetailsAssociation
	{
		public EntityRef<Orders> Order { get; set; }
		public EntityRef<Products> Product { get; set; }
	}
	
	public static class Order_DetailsExtension
	{
		public const string TableName = "Order Details";
		public static readonly string[] Keys = new string[] { _ORDERID, _PRODUCTID };
		
		public static readonly IConstraint[] Constraints = new IConstraint[]
		{
			new Constraint<Orders>
			{
				Name = "FK_Order_Details_Orders",
				ThisKey = _ORDERID,
				OtherKey = OrdersExtension._ORDERID,
				IsForeignKey = true
			},
			new Constraint<Products>
			{
				Name = "FK_Order_Details_Products",
				ThisKey = _PRODUCTID,
				OtherKey = ProductsExtension._PRODUCTID,
				IsForeignKey = true
			}
		};
		
		public static List<Order_Details> ToOrder_DetailsCollection(this DataTable dt)
		{
			return dt.AsEnumerable()
			.Select(row => NewObject(row))
			.ToList();
		}
		
		public static Order_Details NewObject(DataRow row)
		{
			return new Order_Details
			{
				OrderID = row.GetField<int>(_ORDERID),
				ProductID = row.GetField<int>(_PRODUCTID),
				UnitPrice = row.GetField<decimal>(_UNITPRICE),
				Quantity = row.GetField<short>(_QUANTITY),
				Discount = row.GetField<float>(_DISCOUNT)
			};
		}
		
		public static void FillObject(this Order_Details item, DataRow row)
		{
			item.OrderID = row.GetField<int>(_ORDERID);
			item.ProductID = row.GetField<int>(_PRODUCTID);
			item.UnitPrice = row.GetField<decimal>(_UNITPRICE);
			item.Quantity = row.GetField<short>(_QUANTITY);
			item.Discount = row.GetField<float>(_DISCOUNT);
		}
		
		public static void UpdateRow(this Order_Details item, DataRow row)
		{
			row.SetField(_ORDERID, item.OrderID);
			row.SetField(_PRODUCTID, item.ProductID);
			row.SetField(_UNITPRICE, item.UnitPrice);
			row.SetField(_QUANTITY, item.Quantity);
			row.SetField(_DISCOUNT, item.Discount);
		}
		
		public static DataTable CreateTable()
		{
			DataTable dt = new DataTable();
			dt.Columns.Add(new DataColumn(_ORDERID, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_PRODUCTID, typeof(System.Int32)));
			dt.Columns.Add(new DataColumn(_UNITPRICE, typeof(System.Decimal)));
			dt.Columns.Add(new DataColumn(_QUANTITY, typeof(System.Int16)));
			dt.Columns.Add(new DataColumn(_DISCOUNT, typeof(System.Single)));
			
			return dt;
		}
		
		public static void ToDataTable(this IEnumerable<Order_Details> items, DataTable dt)
		{
			foreach (var item in items)
			{
				var row = dt.NewRow();
				UpdateRow(item, row);
				dt.Rows.Add(row);
			}
			dt.AcceptChanges();
		}
		
		public static DataTable ToDataTable(this IEnumerable<Order_Details> items)
		{
			var dt = CreateTable();
			ToDataTable(items, dt);
			return dt;
		}
		
		public static IDictionary<string, object> ToDictionary(this Order_Details item)
		{
			return new Dictionary<string,object>() 
			{
				[_ORDERID] = item.OrderID,
				[_PRODUCTID] = item.ProductID,
				[_UNITPRICE] = item.UnitPrice,
				[_QUANTITY] = item.Quantity,
				[_DISCOUNT] = item.Discount
			};
		}
		
		public static Order_Details FromDictionary(this IDictionary<string, object> dict)
		{
			return new Order_Details
			{
				OrderID = (int)dict[_ORDERID],
				ProductID = (int)dict[_PRODUCTID],
				UnitPrice = (decimal)dict[_UNITPRICE],
				Quantity = (short)dict[_QUANTITY],
				Discount = (float)dict[_DISCOUNT]
			};
		}
		
		public static bool CompareTo(this Order_Details a, Order_Details b)
		{
			return a.OrderID == b.OrderID
			&& a.ProductID == b.ProductID
			&& a.UnitPrice == b.UnitPrice
			&& a.Quantity == b.Quantity
			&& a.Discount == b.Discount;
		}
		
		public static void CopyTo(this Order_Details from, Order_Details to)
		{
			to.OrderID = from.OrderID;
			to.ProductID = from.ProductID;
			to.UnitPrice = from.UnitPrice;
			to.Quantity = from.Quantity;
			to.Discount = from.Discount;
		}
		
		public static Order_DetailsAssociation GetAssociation(this Order_Details entity)
		{
			return entity.AsEnumerable().GetAssociation().FirstOrDefault();
		}
		
		public static IEnumerable<Order_DetailsAssociation> GetAssociation(this IEnumerable<Order_Details> entities)
		{
			var reader = entities.Expand();
			
			var associations = new List<Order_DetailsAssociation>();
			
			var _Order = reader.Read<Orders>();
			var _Product = reader.Read<Products>();
			
			foreach (var entity in entities)
			{
				var association = new Order_DetailsAssociation
				{
					Order = new EntityRef<Orders>(_Order.FirstOrDefault(row => row.OrderID == entity.OrderID)),
					Product = new EntityRef<Products>(_Product.FirstOrDefault(row => row.ProductID == entity.ProductID)),
				};
				associations.Add(association);
			}
			
			return associations;
		}
		
		public static string ToSimpleString(this Order_Details obj)
		{
			return string.Format("{{OrderID:{0}, ProductID:{1}, UnitPrice:{2}, Quantity:{3}, Discount:{4}}}", 
			obj.OrderID, 
			obj.ProductID, 
			obj.UnitPrice, 
			obj.Quantity, 
			obj.Discount);
		}
		
		public const string _ORDERID = "OrderID";
		public const string _PRODUCTID = "ProductID";
		public const string _UNITPRICE = "UnitPrice";
		public const string _QUANTITY = "Quantity";
		public const string _DISCOUNT = "Discount";
	}
}