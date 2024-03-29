using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel;

namespace UnitTestProject.Northwind.ViewModel
{
	public partial class Suppliers
		: INotifyPropertyChanged
	{
		public Suppliers()
		{
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
		private string _CompanyName;
		
		partial void OnCompanyNameChanging(string value);
		
		partial void OnCompanyNameChanged();
		
		public string CompanyName
		{
			get
			{
				return this._CompanyName;
			}
			set
			{
				this.OnCompanyNameChanging(value);
				this._CompanyName = value;
				this.OnCompanyNameChanged();
				this.OnPropertyChanged(nameof(CompanyName));
			}
		}
		private string _ContactName;
		
		partial void OnContactNameChanging(string value);
		
		partial void OnContactNameChanged();
		
		public string ContactName
		{
			get
			{
				return this._ContactName;
			}
			set
			{
				this.OnContactNameChanging(value);
				this._ContactName = value;
				this.OnContactNameChanged();
				this.OnPropertyChanged(nameof(ContactName));
			}
		}
		private string _ContactTitle;
		
		partial void OnContactTitleChanging(string value);
		
		partial void OnContactTitleChanged();
		
		public string ContactTitle
		{
			get
			{
				return this._ContactTitle;
			}
			set
			{
				this.OnContactTitleChanging(value);
				this._ContactTitle = value;
				this.OnContactTitleChanged();
				this.OnPropertyChanged(nameof(ContactTitle));
			}
		}
		private string _Address;
		
		partial void OnAddressChanging(string value);
		
		partial void OnAddressChanged();
		
		public string Address
		{
			get
			{
				return this._Address;
			}
			set
			{
				this.OnAddressChanging(value);
				this._Address = value;
				this.OnAddressChanged();
				this.OnPropertyChanged(nameof(Address));
			}
		}
		private string _City;
		
		partial void OnCityChanging(string value);
		
		partial void OnCityChanged();
		
		public string City
		{
			get
			{
				return this._City;
			}
			set
			{
				this.OnCityChanging(value);
				this._City = value;
				this.OnCityChanged();
				this.OnPropertyChanged(nameof(City));
			}
		}
		private string _Region;
		
		partial void OnRegionChanging(string value);
		
		partial void OnRegionChanged();
		
		public string Region
		{
			get
			{
				return this._Region;
			}
			set
			{
				this.OnRegionChanging(value);
				this._Region = value;
				this.OnRegionChanged();
				this.OnPropertyChanged(nameof(Region));
			}
		}
		private string _PostalCode;
		
		partial void OnPostalCodeChanging(string value);
		
		partial void OnPostalCodeChanged();
		
		public string PostalCode
		{
			get
			{
				return this._PostalCode;
			}
			set
			{
				this.OnPostalCodeChanging(value);
				this._PostalCode = value;
				this.OnPostalCodeChanged();
				this.OnPropertyChanged(nameof(PostalCode));
			}
		}
		private string _Country;
		
		partial void OnCountryChanging(string value);
		
		partial void OnCountryChanged();
		
		public string Country
		{
			get
			{
				return this._Country;
			}
			set
			{
				this.OnCountryChanging(value);
				this._Country = value;
				this.OnCountryChanged();
				this.OnPropertyChanged(nameof(Country));
			}
		}
		private string _Phone;
		
		partial void OnPhoneChanging(string value);
		
		partial void OnPhoneChanged();
		
		public string Phone
		{
			get
			{
				return this._Phone;
			}
			set
			{
				this.OnPhoneChanging(value);
				this._Phone = value;
				this.OnPhoneChanged();
				this.OnPropertyChanged(nameof(Phone));
			}
		}
		private string _Fax;
		
		partial void OnFaxChanging(string value);
		
		partial void OnFaxChanged();
		
		public string Fax
		{
			get
			{
				return this._Fax;
			}
			set
			{
				this.OnFaxChanging(value);
				this._Fax = value;
				this.OnFaxChanged();
				this.OnPropertyChanged(nameof(Fax));
			}
		}
		private string _HomePage;
		
		partial void OnHomePageChanging(string value);
		
		partial void OnHomePageChanged();
		
		public string HomePage
		{
			get
			{
				return this._HomePage;
			}
			set
			{
				this.OnHomePageChanging(value);
				this._HomePage = value;
				this.OnHomePageChanged();
				this.OnPropertyChanged(nameof(HomePage));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}