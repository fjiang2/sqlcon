using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel;

namespace UnitTestProject.Northwind.ViewModel
{
	public partial class Shippers
		: INotifyPropertyChanged
	{
		public Shippers()
		{
		}
		private int _ShipperID;
		
		partial void OnShipperIDChanging(int value);
		
		partial void OnShipperIDChanged();
		
		public int ShipperID
		{
			get
			{
				return this._ShipperID;
			}
			set
			{
				this.OnShipperIDChanging(value);
				this._ShipperID = value;
				this.OnShipperIDChanged();
				this.OnPropertyChanged(nameof(ShipperID));
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
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}