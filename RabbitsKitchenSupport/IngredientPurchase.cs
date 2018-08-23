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
	public class IngredientPurchase : INotifyPropertyChanged, IHasID
	{
		// private static properties
		public static string DBTableName { get; } = "IngredientPurchase";

		// private fields
		private long _id;
		private long _ingredientID;
		private double _quantity;
		private double _cost;
		private DateTimeOffset _date;
		private Ingredient _ingredient;
		private string _provider;

		// public properties
		public long ID { get => _id; set => SetField(ref _id, value); }
		public long IngredientID { get => _ingredientID; set => SetField(ref _ingredientID, value); }
		public double Quantity { get => _quantity; set => SetField(ref _quantity, value); }
		public double Cost { get => _cost; set => SetField(ref _cost, value); }
		[NonDBValue] public DateTimeOffset Date {
			get => _date;
			set
			{
				SetField(ref _date, value);
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PurchaseYear)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PurchaseMonth)));
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PurchaseDate)));
			}
		}
		public long DateTicks { get => Date.UtcTicks; set => Date = new DateTimeOffset(value, TimeSpan.Zero); }
		[NonDBValue] public string PurchaseYear { get => Date.ToString("yyyy"); }
		[NonDBValue] public string PurchaseMonth { get => Date.ToString("MMM"); }
		[NonDBValue] public string PurchaseDate { get => Date.ToString("dd"); }
		[NonDBValue] public Ingredient Ingredient { get => _ingredient; set => SetField(ref _ingredient, value); }
		public string Provider { get => _provider; set => SetField(ref _provider, value); }

		// constructor
		public IngredientPurchase() { }

		// public methods
		public static void CreateDBTable(SqliteConnection db)
		{
			string tableCommand = "CREATE TABLE IF NOT EXISTS " +
				$"{DBTableName} (ID INTEGER PRIMARY KEY, IngredientID INTEGER NULL, Quantity REAL NULL, Cost REAL NULL, DateTicks INTEGER NULL, Provider TEXT NULL)";
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
		protected void RaiseChangedEvent(string propertyName)
		{

		}
	}
}
