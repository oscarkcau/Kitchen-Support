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
        private DateTime _date;

        // public properties
        public long ID { get => _id; set => SetField(ref _id, value); }
        public long IngredientID { get => _ingredientID; set => SetField(ref _ingredientID, value); }
        public double Quantity { get => _quantity; set => SetField(ref _quantity, value); }
        public double Cost { get => _cost; set => SetField(ref _cost, value); }

        // constructor
        public IngredientPurchase() { }

        // public methods
        public static void CreateDBTable(SqliteConnection db)
        {
            throw new NotImplementedException();
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
