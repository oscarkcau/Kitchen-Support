using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RabbitsKitchenSupport
{
	public class Category : INotifyPropertyChanged, IHasID
	{
		// private fields
		private long _id;
		private string _name;
		private int _numOfItems;

		// public properties
		public long ID { get => _id; set => SetField(ref _id, value); }
		public string Name { get => _name; set => SetField(ref _name, value); }
		[NonDBValue] public int NumOfItems { get => _numOfItems; set => SetField(ref _numOfItems, value); }

		// constructor
		public Category() { }

		// INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		protected void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value)) return;
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
