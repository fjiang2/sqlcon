using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel;

namespace UnitTestProject.Northwind.ViewModel
{
	public partial class Orders
		: INotifyPropertyChanged
	{
		public Orders()
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
		private string _CustomerID;
		
		partial void OnCustomerIDChanging(string value);
		
		partial void OnCustomerIDChanged();
		
		public string CustomerID
		{
			get
			{
				return this._CustomerID;
			}
			set
			{
				this.OnCustomerIDChanging(value);
				this._CustomerID = value;
				this.OnCustomerIDChanged();
				this.OnPropertyChanged(nameof(CustomerID));
			}
		}
		private int _EmployeeID;
		
		partial void OnEmployeeIDChanging(int value);
		
		partial void OnEmployeeIDChanged();
		
		public int EmployeeID
		{
			get
			{
				return this._EmployeeID;
			}
			set
			{
				this.OnEmployeeIDChanging(value);
				this._EmployeeID = value;
				this.OnEmployeeIDChanged();
				this.OnPropertyChanged(nameof(EmployeeID));
			}
		}
		private DateTime _OrderDate;
		
		partial void OnOrderDateChanging(DateTime value);
		
		partial void OnOrderDateChanged();
		
		public DateTime OrderDate
		{
			get
			{
				return this._OrderDate;
			}
			set
			{
				this.OnOrderDateChanging(value);
				this._OrderDate = value;
				this.OnOrderDateChanged();
				this.OnPropertyChanged(nameof(OrderDate));
			}
		}
		private DateTime _RequiredDate;
		
		partial void OnRequiredDateChanging(DateTime value);
		
		partial void OnRequiredDateChanged();
		
		public DateTime RequiredDate
		{
			get
			{
				return this._RequiredDate;
			}
			set
			{
				this.OnRequiredDateChanging(value);
				this._RequiredDate = value;
				this.OnRequiredDateChanged();
				this.OnPropertyChanged(nameof(RequiredDate));
			}
		}
		private DateTime _ShippedDate;
		
		partial void OnShippedDateChanging(DateTime value);
		
		partial void OnShippedDateChanged();
		
		public DateTime ShippedDate
		{
			get
			{
				return this._ShippedDate;
			}
			set
			{
				this.OnShippedDateChanging(value);
				this._ShippedDate = value;
				this.OnShippedDateChanged();
				this.OnPropertyChanged(nameof(ShippedDate));
			}
		}
		private int _ShipVia;
		
		partial void OnShipViaChanging(int value);
		
		partial void OnShipViaChanged();
		
		public int ShipVia
		{
			get
			{
				return this._ShipVia;
			}
			set
			{
				this.OnShipViaChanging(value);
				this._ShipVia = value;
				this.OnShipViaChanged();
				this.OnPropertyChanged(nameof(ShipVia));
			}
		}
		private decimal _Freight;
		
		partial void OnFreightChanging(decimal value);
		
		partial void OnFreightChanged();
		
		public decimal Freight
		{
			get
			{
				return this._Freight;
			}
			set
			{
				this.OnFreightChanging(value);
				this._Freight = value;
				this.OnFreightChanged();
				this.OnPropertyChanged(nameof(Freight));
			}
		}
		private string _ShipName;
		
		partial void OnShipNameChanging(string value);
		
		partial void OnShipNameChanged();
		
		public string ShipName
		{
			get
			{
				return this._ShipName;
			}
			set
			{
				this.OnShipNameChanging(value);
				this._ShipName = value;
				this.OnShipNameChanged();
				this.OnPropertyChanged(nameof(ShipName));
			}
		}
		private string _ShipAddress;
		
		partial void OnShipAddressChanging(string value);
		
		partial void OnShipAddressChanged();
		
		public string ShipAddress
		{
			get
			{
				return this._ShipAddress;
			}
			set
			{
				this.OnShipAddressChanging(value);
				this._ShipAddress = value;
				this.OnShipAddressChanged();
				this.OnPropertyChanged(nameof(ShipAddress));
			}
		}
		private string _ShipCity;
		
		partial void OnShipCityChanging(string value);
		
		partial void OnShipCityChanged();
		
		public string ShipCity
		{
			get
			{
				return this._ShipCity;
			}
			set
			{
				this.OnShipCityChanging(value);
				this._ShipCity = value;
				this.OnShipCityChanged();
				this.OnPropertyChanged(nameof(ShipCity));
			}
		}
		private string _ShipRegion;
		
		partial void OnShipRegionChanging(string value);
		
		partial void OnShipRegionChanged();
		
		public string ShipRegion
		{
			get
			{
				return this._ShipRegion;
			}
			set
			{
				this.OnShipRegionChanging(value);
				this._ShipRegion = value;
				this.OnShipRegionChanged();
				this.OnPropertyChanged(nameof(ShipRegion));
			}
		}
		private string _ShipPostalCode;
		
		partial void OnShipPostalCodeChanging(string value);
		
		partial void OnShipPostalCodeChanged();
		
		public string ShipPostalCode
		{
			get
			{
				return this._ShipPostalCode;
			}
			set
			{
				this.OnShipPostalCodeChanging(value);
				this._ShipPostalCode = value;
				this.OnShipPostalCodeChanged();
				this.OnPropertyChanged(nameof(ShipPostalCode));
			}
		}
		private string _ShipCountry;
		
		partial void OnShipCountryChanging(string value);
		
		partial void OnShipCountryChanged();
		
		public string ShipCountry
		{
			get
			{
				return this._ShipCountry;
			}
			set
			{
				this.OnShipCountryChanging(value);
				this._ShipCountry = value;
				this.OnShipCountryChanged();
				this.OnPropertyChanged(nameof(ShipCountry));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}