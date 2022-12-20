using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel;

namespace UnitTestProject.Northwind.ViewModel
{
	public partial class Products
		: INotifyPropertyChanged
	{
		public Products()
		{
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
		private string _ProductName;
		
		partial void OnProductNameChanging(string value);
		
		partial void OnProductNameChanged();
		
		public string ProductName
		{
			get
			{
				return this._ProductName;
			}
			set
			{
				this.OnProductNameChanging(value);
				this._ProductName = value;
				this.OnProductNameChanged();
				this.OnPropertyChanged(nameof(ProductName));
			}
		}
		private int _SupplierID;
		
		partial void OnSupplierIDChanging(int value);
		
		partial void OnSupplierIDChanged();
		
		public int SupplierID
		{
			get
			{
				return this._SupplierID;
			}
			set
			{
				this.OnSupplierIDChanging(value);
				this._SupplierID = value;
				this.OnSupplierIDChanged();
				this.OnPropertyChanged(nameof(SupplierID));
			}
		}
		private int _CategoryID;
		
		partial void OnCategoryIDChanging(int value);
		
		partial void OnCategoryIDChanged();
		
		public int CategoryID
		{
			get
			{
				return this._CategoryID;
			}
			set
			{
				this.OnCategoryIDChanging(value);
				this._CategoryID = value;
				this.OnCategoryIDChanged();
				this.OnPropertyChanged(nameof(CategoryID));
			}
		}
		private string _QuantityPerUnit;
		
		partial void OnQuantityPerUnitChanging(string value);
		
		partial void OnQuantityPerUnitChanged();
		
		public string QuantityPerUnit
		{
			get
			{
				return this._QuantityPerUnit;
			}
			set
			{
				this.OnQuantityPerUnitChanging(value);
				this._QuantityPerUnit = value;
				this.OnQuantityPerUnitChanged();
				this.OnPropertyChanged(nameof(QuantityPerUnit));
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
		private short _UnitsInStock;
		
		partial void OnUnitsInStockChanging(short value);
		
		partial void OnUnitsInStockChanged();
		
		public short UnitsInStock
		{
			get
			{
				return this._UnitsInStock;
			}
			set
			{
				this.OnUnitsInStockChanging(value);
				this._UnitsInStock = value;
				this.OnUnitsInStockChanged();
				this.OnPropertyChanged(nameof(UnitsInStock));
			}
		}
		private short _UnitsOnOrder;
		
		partial void OnUnitsOnOrderChanging(short value);
		
		partial void OnUnitsOnOrderChanged();
		
		public short UnitsOnOrder
		{
			get
			{
				return this._UnitsOnOrder;
			}
			set
			{
				this.OnUnitsOnOrderChanging(value);
				this._UnitsOnOrder = value;
				this.OnUnitsOnOrderChanged();
				this.OnPropertyChanged(nameof(UnitsOnOrder));
			}
		}
		private short _ReorderLevel;
		
		partial void OnReorderLevelChanging(short value);
		
		partial void OnReorderLevelChanged();
		
		public short ReorderLevel
		{
			get
			{
				return this._ReorderLevel;
			}
			set
			{
				this.OnReorderLevelChanging(value);
				this._ReorderLevel = value;
				this.OnReorderLevelChanged();
				this.OnPropertyChanged(nameof(ReorderLevel));
			}
		}
		private bool _Discontinued;
		
		partial void OnDiscontinuedChanging(bool value);
		
		partial void OnDiscontinuedChanged();
		
		public bool Discontinued
		{
			get
			{
				return this._Discontinued;
			}
			set
			{
				this.OnDiscontinuedChanging(value);
				this._Discontinued = value;
				this.OnDiscontinuedChanged();
				this.OnPropertyChanged(nameof(Discontinued));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}