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
	public class RecipeIngredientItem : INotifyPropertyChanged, IHasID
	{
		// private static fields
		public static string DBTableName { get; } = "RecipeIngredientItem";

		// private fields
		private long _id;
		private long _recipeId;
		private long _ingredientId;
		private double _quantity;
		private Ingredient _ingredient;

		// public properties
		public long ID { get => _id; set => SetField(ref _id, value); }
		public long RecipeID { get => _recipeId; set => SetField(ref _recipeId, value); }
		public long IngredientID { get => _ingredientId; set => SetField(ref _ingredientId, value); }
		public double Quantity { get => _quantity; set => SetField(ref _quantity, value); }
		[NonDBValue] public Ingredient Ingredient { get => _ingredient; set => SetField(ref _ingredient, value); }

		// constructor
		public RecipeIngredientItem() { }

		// public methods
		public static void CreateDBTable(SqliteConnection db)
		{
			string tableCommand = "CREATE TABLE IF NOT EXISTS " +
				$"{DBTableName} (ID INTEGER PRIMARY KEY, RecipeID INTEGER NULL, IngredientID INTEGER NULL, Quantity REAL NULL)";
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
