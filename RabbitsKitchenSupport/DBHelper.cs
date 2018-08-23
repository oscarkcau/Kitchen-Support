using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RabbitsKitchenSupport
{
	class DBHelper
	{
		public static SqliteConnection Connection { get; set; }

		public static ObservableCollection<T> Populate<T>(string dbTableName = null, string whereClause = "") where T : new()
		{
			// get the table name if no argument provided
			Type type = typeof(T);
			if (dbTableName == null) dbTableName = GetTableName(type);
			if (dbTableName == null) throw new InvalidOperationException();

			// get column names and property informations
			var (columnNames, propertyInfos) = CollectPropertyNames(type);

			// prepare and execute query command
			SqliteCommand command = new SqliteCommand();
			command.Connection = DBHelper.Connection;
			command.CommandText = $"SELECT {columnNames} FROM {dbTableName} {whereClause};";
			SqliteDataReader query = command.ExecuteReader();

			// for each row
			ObservableCollection<T> list = new ObservableCollection<T>();
			while (query.Read())
			{
				// create new instance and add to list
				T obj = new T();
				list.Add(obj);

				// assign property values
				for (int i = 0; i < propertyInfos.Count(); i++)
				{
					// get field value from query object
					// use default value if value from DB is null
					object value = query.GetValue(i);
					if (value == DBNull.Value)
					{
						Type propertyType = propertyInfos[i].PropertyType;
						value = propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null;
					}

					// set property value
					propertyInfos[i].SetValue(obj, value);
				}
			}

			return list;
		}
		public static double? Sclar<T>(string dbTableName = null, string selectClause = "*", string whereClause = "")
		{
			// get the table name if no argument provided
			Type type = typeof(T);
			if (dbTableName == null) dbTableName = GetTableName(type);
			if (dbTableName == null) throw new InvalidOperationException();

			// prepare and execute query command
			SqliteCommand command = new SqliteCommand();
			command.Connection = DBHelper.Connection;
			command.CommandText = $"SELECT {selectClause} FROM {dbTableName} {whereClause};";
			object result = command.ExecuteScalar();

			if (result == null) return null;
			if (result == DBNull.Value) return null;
			return (double) result;
		}
		public static bool Insert(IHasID item, string dbTableName = null)
		{
			// get the table name if no argument provided
			if (dbTableName == null) dbTableName = GetTableName(item.GetType());
			if (dbTableName == null) throw new InvalidOperationException();

			// get colume names and values to be inserted
			var (names, values) = CollectInsertInfo(item);

			// prepare and execute command
			int n = values.Count;
			SqliteCommand command = new SqliteCommand();
			command.Connection = DBHelper.Connection; ;
			command.CommandText = $"INSERT INTO {dbTableName} ({names}) VALUES ({MakePlaceholders(n)});";
			for (int i = 0; i < n; i++)
			{
				command.Parameters.AddWithValue($"@P{i}", values[i]);
			}
			int rows = command.ExecuteNonQuery();

			// get and assign the last insert row id
			if (rows > 0)
			{
				command = new SqliteCommand("SELECT last_insert_rowid()", DBHelper.Connection);
				item.ID = (long)command.ExecuteScalar();
			}

			// reture true if item inserted successfully
			return rows > 0;
		}
		public static bool Update(IHasID item, string dbTableName = null)
		{
			// get the table name if no argument provided
			if (dbTableName == null) dbTableName = GetTableName(item.GetType());
			if (dbTableName == null) throw new InvalidOperationException();

			// get clauses (NAME=PLACEHOLDER) and values to be updated
			var (clauses, values) = CollectUpdateInfo(item);

			// prepare and execute command
			SqliteCommand command = new SqliteCommand();
			command.Connection = DBHelper.Connection;
			command.CommandText = $"UPDATE {dbTableName} SET {clauses} WHERE ID = {item.ID};";
			for (int i = 0; i < values.Count; i++)
			{
				command.Parameters.AddWithValue($"@P{i}", values[i]);
			}
			int rows = command.ExecuteNonQuery();

			// reture true if item inserted successfully
			return rows > 0;
		}
		public static bool Delete(IHasID item, string dbTableName = null)
		{
			// get the table name if no argument provided
			if (dbTableName == null) dbTableName = GetTableName(item.GetType());
			if (dbTableName == null) throw new InvalidOperationException();

			// prepare and execute command
			int rows = new SqliteCommand(
				$"DELETE FROM {dbTableName} WHERE ID = {item.ID};",
				DBHelper.Connection
			).ExecuteNonQuery();

			// reture true if item inserted successfully
			return rows > 0;
		}

		private static string GetTableName(Type type)
		{
			var prop = type.GetProperty("DBTableName", BindingFlags.Public | BindingFlags.Static);
			return (string)prop?.GetValue(null);
		}
		private static (string, List<PropertyInfo>) CollectPropertyNames(Type type)
		{
			// collection property names for Populate method

			List<PropertyInfo> propertyInfos = new List<PropertyInfo>();
			StringBuilder sb = new StringBuilder();
	
			foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				// skip property if NonDBValueAttribute found
				if (prop.GetCustomAttributes(typeof(NonDBValueAttribute), true).Length > 0) continue;

				// collect property name
				string name = prop.Name;
				// replace with ColumnName property if found
				var attributes = prop.GetCustomAttributes(typeof(DBValueAttribute), true);
				if (attributes.Length > 0) name = ((DBValueAttribute)attributes[0]).ColumnName;
				sb.Append($"{name},");

				// store prop object
				propertyInfos.Add(prop);
			}
			sb.Remove(sb.Length - 1, 1); // remove the last comma

			return (sb.ToString(), propertyInfos);
		}
		private static (string, List<object>) CollectInsertInfo(IHasID item, bool skipID = false)
		{
			List<object> values = new List<object>();
			StringBuilder sb = new StringBuilder();
			foreach (var prop in item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				// skip property if NonDBValueAttribute found
				if (prop.GetCustomAttributes(typeof(NonDBValueAttribute), true).Length > 0) continue;

				// skip ID if it is not needed
				if (skipID && prop.Name == "ID") continue;

				// collect property name
				string name = prop.Name;
				// replace with ColumnName property if found
				var attributes = prop.GetCustomAttributes(typeof(DBValueAttribute), true);
				if (attributes.Length > 0) name = ((DBValueAttribute)attributes[0]).ColumnName;
				sb.Append(name + ",");

				// collect property value
				object value = prop.GetValue(item) ?? DBNull.Value;
				if (prop.Name == "ID") value = DBNull.Value; // replace ID value with NULL
				values.Add(value);
			}
			sb.Remove(sb.Length - 1, 1); // remove the last comma

			return (sb.ToString(), values);
		}
		private static string MakePlaceholders(int n)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < n; i++)
			{
				sb.Append($"@P{i},");
			}
			sb.Remove(sb.Length - 1, 1); // remove the last comma

			return sb.ToString();
		}
		private static (string, List<object>) CollectUpdateInfo(IHasID item)
		{
			List<object> values = new List<object>();
			StringBuilder sb = new StringBuilder();
			int count = 0;
			foreach (var prop in item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				// skip property if NonDBValueAttribute found
				if (prop.GetCustomAttributes(typeof(NonDBValueAttribute), true).Length > 0) continue;

				// skip ID as it is not needed
				if (prop.Name == "ID") continue;

				// collect property name
				string name = prop.Name;
				// replace with ColumnName property if found
				var attributes = prop.GetCustomAttributes(typeof(DBValueAttribute), true);
				if (attributes.Length > 0) name = ((DBValueAttribute)attributes[0]).ColumnName;
				sb.Append($"{name}=@P{count++},");

				// collect property value
				object value = prop.GetValue(item) ?? DBNull.Value;
				values.Add(value);
			}
			sb.Remove(sb.Length - 1, 1); // remove the last comma

			return (sb.ToString(), values);
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	class NonDBValueAttribute : Attribute
	{
		// Attribute that specifies property which should not be stored in database
	}

	[AttributeUsage(AttributeTargets.Property)]
	class DBValueAttribute : Attribute
	{
		// Attribute that specifies property which should be stored in database
		public string ColumnName { get; set; }
	}
}
