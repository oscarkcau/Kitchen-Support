using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace RabbitsKitchenSupport
{
	public class Recipe : INotifyPropertyChanged, IHasID, IHasCategory
	{
		// private static fields
		public static string DBTableName { get; } = "Recipe";

		// private fields
		private long _id;
		private string _name;
		private string _category;
		private double _unitPrice;
		private string _thumbnailFilename;
		private ImageSource _imageSource;

		// public properties
		public long ID { get => _id; set => SetField(ref _id, value); }
		public string Name { get => _name; set => SetField(ref _name, value); }
		public string Category { get => _category; set => SetField(ref _category, value); }
		public double UnitPrice { get => _unitPrice; set => SetField(ref _unitPrice, value); }
		public string ThumbnailFilename { get => _thumbnailFilename; set => SetField(ref _thumbnailFilename, value); }
		[NonDBValue] public ImageSource ImageSource { get => _imageSource; set => SetField(ref _imageSource, value); }

		// constructor
		public Recipe() { }

		// public methods
		public static void CreateDBTable(SqliteConnection db)
		{
			string tableCommand = "CREATE TABLE IF NOT EXISTS " +
				$"{DBTableName} (ID INTEGER PRIMARY KEY, Name TEXT NULL, Category TEXT NULL, UnitPrice REAL NULL, ThumbnailFilename NULL)";
			SqliteCommand createTable = new SqliteCommand(tableCommand, db);
			createTable.ExecuteReader();
		}


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
