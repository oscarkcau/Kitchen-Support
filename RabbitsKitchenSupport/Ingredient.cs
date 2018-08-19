using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;

namespace RabbitsKitchenSupport
{
	public class Ingredient : INotifyPropertyChanged, IHasID, IHasCategory
	{
		// private static properties
		public static string DBTableName { get; } = "Ingredient";

		// private fields
		private long _id;
		private string _name;
		private string _defaultUnit;
		private string _category;
		private string _thumbnailFilename;
		private ImageSource _imageSource;

		// public properties
		public long ID { get => _id; set => SetField(ref _id, value); }
		public string Name { get => _name; set => SetField(ref _name, value); }
		public string Category { get => _category; set => SetField(ref _category, value); }
		public string DefaultUnit { get => _defaultUnit; set => SetField(ref _defaultUnit, value); }
		public string ThumbnailFilename { get => _thumbnailFilename; set => SetField(ref _thumbnailFilename, value); }
		[NonDBValue] public ImageSource ImageSource { get => _imageSource; set => SetField(ref _imageSource, value); }

		// constructor
		public Ingredient() { }

		// public methods
		public static void CreateDBTable(SqliteConnection db)
		{
			string tableCommand = "CREATE TABLE IF NOT EXISTS " +
				$"{DBTableName} (ID INTEGER PRIMARY KEY, Name TEXT NULL, Category TEXT NULL, DefaultUnit TEXT NULL, ThumbnailFilename NULL)";
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
