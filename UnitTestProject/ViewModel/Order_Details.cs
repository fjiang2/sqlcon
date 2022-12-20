using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind.ViewModel
{
	public partial class Order_Details
		: INotifyPropertyChanged
	{
		public Order_Details()
		{
		}
		private int _OrderID;
		
		partial void OnOrderIDChanging(int value);
		
		partial void OnOrderIDChanged();
		
		public int OrderID
		{
			get
			{
				return this._OrderID;
			}
			set
			{
				this.OnOrderIDChanging(value);
				this._OrderID = value;
				this.OnOrderIDChanged();
				this.OnPropertyChanged(nameof(OrderID));
			}
		}
		private int _ProductID;
		
		partial void OnProductIDChanging(int value);
		
		partial void OnProductIDChanged();
		
		public int ProductID
		{
			get
			{
				return this._ProductID;
			}
			set
			{
				this.OnProductIDChanging(value);
				this._ProductID = value;
				this.OnProductIDChanged();
				this.OnPropertyChanged(nameof(ProductID));
			}
		}
		private decimal _UnitPrice;
		
		partial void OnUnitPriceChanging(decimal value);
		
		partial void OnUnitPriceChanged();
		
		public decimal UnitPrice
		{
			get
			{
				return this._UnitPrice;
			}
			set
			{
				this.OnUnitPriceChanging(value);
				this._UnitPrice = value;
				this.OnUnitPriceChanged();
				this.OnPropertyChanged(nameof(UnitPrice));
			}
		}
		private short _Quantity;
		
		partial void OnQuantityChanging(short value);
		
		partial void OnQuantityChanged();
		
		public short Quantity
		{
			get
			{
				return this._Quantity;
			}
			set
			{
				this.OnQuantityChanging(value);
				this._Quantity = value;
				this.OnQuantityChanged();
				this.OnPropertyChanged(nameof(Quantity));
			}
		}
		private float _Discount;
		
		partial void OnDiscountChanging(float value);
		
		partial void OnDiscountChanged();
		
		public float Discount
		{
			get
			{
				return this._Discount;
			}
			set
			{
				this.OnDiscountChanging(value);
				this._Discount = value;
				this.OnDiscountChanged();
				this.OnPropertyChanged(nameof(Discount));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}