using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel;

namespace UnitTestProject.Northwind.ViewModel
{
	public partial class EmployeeTerritories
		: INotifyPropertyChanged
	{
		public EmployeeTerritories()
		{
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
		private string _TerritoryID;
		
		partial void OnTerritoryIDChanging(string value);
		
		partial void OnTerritoryIDChanged();
		
		public string TerritoryID
		{
			get
			{
				return this._TerritoryID;
			}
			set
			{
				this.OnTerritoryIDChanging(value);
				this._TerritoryID = value;
				this.OnTerritoryIDChanged();
				this.OnPropertyChanged(nameof(TerritoryID));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}