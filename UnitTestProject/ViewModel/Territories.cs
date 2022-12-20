using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.ComponentModel;
using Sys.Data;
using Sys.Data.Linq;

namespace UnitTestProject.Northwind.ViewModel
{
	public partial class Territories
		: INotifyPropertyChanged
	{
		public Territories()
		{
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
		private string _TerritoryDescription;
		
		partial void OnTerritoryDescriptionChanging(string value);
		
		partial void OnTerritoryDescriptionChanged();
		
		public string TerritoryDescription
		{
			get
			{
				return this._TerritoryDescription;
			}
			set
			{
				this.OnTerritoryDescriptionChanging(value);
				this._TerritoryDescription = value;
				this.OnTerritoryDescriptionChanged();
				this.OnPropertyChanged(nameof(TerritoryDescription));
			}
		}
		private int _RegionID;
		
		partial void OnRegionIDChanging(int value);
		
		partial void OnRegionIDChanged();
		
		public int RegionID
		{
			get
			{
				return this._RegionID;
			}
			set
			{
				this.OnRegionIDChanging(value);
				this._RegionID = value;
				this.OnRegionIDChanged();
				this.OnPropertyChanged(nameof(RegionID));
			}
		}
		public event PropertyChangedEventHandler PropertyChanged;
		
		protected virtual void OnPropertyChanged(string property)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
		}
	}
}