using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Windows.Storage;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace RabbitsKitchenSupport
{
	// type alias
	using IngredientList = ObservableCollection<Ingredient>;
	using RecipeList = ObservableCollection<Recipe>;
	using GroupedList = ObservableCollection<KeyedList>;
	using CategoryList = ObservableCollection<Category>;

	class MainModelView : IDisposable
	{
		// static public properties
		public static MainModelView Current { get; } = new MainModelView();

		// privte fields
		string connectionString;
		SqliteConnection db;
		List<string> loadedThumbnailFilenames = new List<string>();

		// public properties
		public string[] Units { get; } = new string[] { "g", "kg", "mL", "L", "oz" };
		public IngredientList Ingredients { get; private set; } = new IngredientList();
		public RecipeList Recipes { get; private set; } = new RecipeList();
		public GroupedList GroupedIngredients { get; private set; }
		public GroupedList GroupedRecipes { get; private set; }
		public CategoryList IngredientCategories { get; private set; } = new CategoryList();
		public CategoryList RecipeCategories { get; private set; } = new CategoryList();
		public CategoryList IngredientProviders { get; private set; } = new CategoryList();

		// constructor
		private MainModelView()
		{
			InitializeDatabase();
			ReadAllFromDB();
			IngredientCategories.CountItemsFrom(GroupedIngredients);

			// async post-process for loading thumbnails and delete unused thumbnails
			LoadThumbtails().ContinueWith(t => DeleteUnusedThumbnails());
		}

		// public methods
		public void AddCategory(Category category, CategoryList list)
		{
			// add new record to database
			string tableName = GetDBTableName(list);
			bool ok = DBHelper.Insert(category, tableName);
			if (!ok) throw new InvalidOperationException();

			// add to catagory list
			list.Add(category);

			// update item count of new category
			if (list == IngredientCategories)
			{
				IngredientCategories.CountItemsFrom(GroupedIngredients);
			}
		}
		public void UpdateCategory(Category category, CategoryList list, string oldname = null)
		{
			// update record in database
			string tableName = GetDBTableName(list);
			bool ok = DBHelper.Update(category, tableName);
			if (!ok) throw new InvalidOperationException();

			
			if (oldname != null && list == IngredientCategories)
			{
				// update category name of Ingredient objects
				var group = GroupedIngredients.GetOrAddGroup(oldname);
				string newName = category.Name;
				group.Key = newName;
				foreach (var item in group)
				{
					var ingredient = item as Ingredient;
					ingredient.Category = newName;
					UpdateIngredient(ingredient);
				}

				// recreate grouped records 
				// (otherwise there is display problem in grid view)
				this.GroupedIngredients = Ingredients.GetGroupedList();

				// update item count of all categories
				IngredientCategories.CountItemsFrom(GroupedIngredients);
			}
		}
		public void DeleteCategory(Category category, CategoryList list)
		{
			// delete record from database
			string tableName = GetDBTableName(list);
			bool ok = DBHelper.Delete(category, tableName);
			if (!ok) throw new InvalidOperationException();

			// remove category from list
			list.Remove(category);
		}

		public void AddIngredient(Ingredient item)
		{
			// add new record to database
			bool ok = DBHelper.Insert(item);
			if (!ok) throw new InvalidOperationException();

			// add item to collection
			Ingredients.Add(item);

			// add item to grouped collection
			GroupedIngredients.GetOrAddGroup(item.Category).Add(item);

			// recount items in categories
			IngredientCategories.CountItemsFrom(GroupedIngredients);
		}
		public void UpdateIngredient(Ingredient item)
		{
			// update record in db
			bool ok = DBHelper.Update(item);
			if (!ok) throw new InvalidOperationException();

			// remove item from old group if category is changed
			bool isRemoved = false;
			foreach (var group in GroupedIngredients)
			{
				if (group.Contains(item) && (string)group.Key != item.Category)
				{
					group.Remove(item);
					isRemoved = true;
				}
			}

			// add item to new group if needed
			if (isRemoved)
			{
				GroupedIngredients.GetOrAddGroup(item.Category).Add(item);

				// recount items in categories
				IngredientCategories.CountItemsFrom(GroupedIngredients);
			}
		}
		public void DeleteIngredient(Ingredient item)
		{
			// delete record from db
			bool ok = DBHelper.Delete(item);
			if (!ok) throw new InvalidOperationException();

			// remove item from collection
			Ingredients.Remove(item);

			// remove item from grouped collection
			GroupedIngredients.GetOrAddGroup(item.Category).Remove(item);

			// recount items in categories
			IngredientCategories.CountItemsFrom(GroupedIngredients);
		}

		public void AddIngredientPurchase(IngredientPurchase item)
		{
			DBHelper.Insert(item);
		}
		public void UpdateIngredientPurchase(IngredientPurchase item)
		{
			DBHelper.Update(item);
		}
		public void DeleteIngredientPurchase(IngredientPurchase item)
		{
			DBHelper.Delete(item);
		}
		public ObservableCollection<IngredientPurchase> GetIngredientPurchase(Ingredient ingredient, int months = -1)
		{
			string clause = $"WHERE IngredientID = {ingredient.ID}";
			if (months > 0)
			{
				DateTimeOffset firstDate = DateTimeOffset.Now.AddMonths(-months);
				clause += $" AND DateTicks >= {firstDate.UtcTicks}";
			}

			var list = DBHelper.Populate<IngredientPurchase>(null, clause);
			foreach (var item in list)
			{
				item.Ingredient = Ingredients.First(x => x.ID == item.IngredientID);
			}

			return list;
		}
		public double? GetAverageIngredientPurchaseCost(Ingredient ingredient, int months = -1)
		{
			string selectClause = "(sum(Cost) / sum(Quantity))";
			string whereClause = $"WHERE IngredientID = {ingredient.ID}";
			if (months > 0)
			{
				DateTimeOffset firstDate = DateTimeOffset.Now.AddMonths(-months);
				whereClause += $" AND DateTicks >= {firstDate.UtcTicks}";
			}

			return DBHelper.Sclar<IngredientPurchase>(null, selectClause, whereClause);
		}

		public void AddRecipe(Recipe item)
		{
			// add new record to database
			bool ok = DBHelper.Insert(item);
			if (!ok) throw new InvalidOperationException();

			// add item to collection
			Recipes.Add(item);

			// add item to grouped collection
			GroupedRecipes.GetOrAddGroup(item.Category).Add(item);

			// recount items in categories
			RecipeCategories.CountItemsFrom(GroupedRecipes);
		}
		public void UpdateRecipe(Recipe item)
		{
			// update record in db
			bool ok = DBHelper.Update(item);
			if (!ok) throw new InvalidOperationException();

			// remove item from old group if category is changed
			bool isRemoved = false;
			foreach (var group in GroupedRecipes)
			{
				if (group.Contains(item) && (string)group.Key != item.Category)
				{
					group.Remove(item);
					isRemoved = true;
				}
			}

			// add item to new group if needed
			if (isRemoved)
			{
				GroupedRecipes.GetOrAddGroup(item.Category).Add(item);

				// recount items in categories
				RecipeCategories.CountItemsFrom(GroupedRecipes);
			}
		}
		public void DeleteRecipe(Recipe item)
		{
			// delete record from db
			bool ok = DBHelper.Delete(item);
			if (!ok) throw new InvalidOperationException();

			// remove item from collection
			Recipes.Remove(item);

			// remove item from grouped collection
			GroupedRecipes.GetOrAddGroup(item.Category).Remove(item);

			// recount items in categories
			RecipeCategories.CountItemsFrom(GroupedRecipes);
		}

		public void AddIngredientItem(RecipeIngredientItem item)
		{
			DBHelper.Insert(item);
		}
		public void UpdateIngredientItem(RecipeIngredientItem item)
		{
			DBHelper.Update(item);
		}
		public void DeleteIngredientItem(RecipeIngredientItem item)
		{
			DBHelper.Delete(item);
		}
		public ObservableCollection<RecipeIngredientItem> GetIngredientItems(Recipe recipe)
		{
			var list = DBHelper.Populate<RecipeIngredientItem>(null, $"WHERE RecipeID = {recipe.ID}");

			foreach (var item in list)
			{
				item.Ingredient = Ingredients.First(x => x.ID == item.IngredientID);
			}

			return list;
		}

		// private methods
		private void InitializeDatabase()
		{
			// specify SQLite library provider 
			SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_winsqlite3());

			// connect to database
			connectionString = "Filename=" + ApplicationData.Current.LocalFolder.Path + "\\" + "main.db";
			db = new SqliteConnection(connectionString);
			db.Open();
			DBHelper.Connection = db;

			// create category tables if not exist
			CategoryList[] categoryLists = { IngredientCategories, RecipeCategories, IngredientProviders };
			foreach (var list in categoryLists)
			{
				string tableName = GetDBTableName(list);
				string tableCommand = "CREATE TABLE IF NOT EXISTS " +
					$"{tableName} (ID INTEGER PRIMARY KEY, Name TEXT NULL)";
				SqliteCommand createTable = new SqliteCommand(tableCommand, db);
				createTable.ExecuteReader();
			}

			// create main tables if not exist
			Ingredient.CreateDBTable(db);
			IngredientPurchase.CreateDBTable(db);
			Recipe.CreateDBTable(db);
			RecipeIngredientItem.CreateDBTable(db);
		}
		private void ReadAllFromDB()
		{
			this.IngredientCategories = DBHelper.Populate<Category>(GetDBTableName(IngredientCategories));
			this.RecipeCategories = DBHelper.Populate<Category>(GetDBTableName(RecipeCategories));
			this.IngredientProviders = DBHelper.Populate<Category>(GetDBTableName(IngredientProviders));
			this.Ingredients = DBHelper.Populate<Ingredient>();
			this.Recipes = DBHelper.Populate<Recipe>();
			this.GroupedIngredients = this.Ingredients.GetGroupedList();
			this.GroupedRecipes = this.Recipes.GetGroupedList();
		}
		private string GetDBTableName(CategoryList list)
		{
			if (list == IngredientCategories) return "Ingredient_Category";
			else if (list == RecipeCategories) return "Recipe_Category";
			else if (list == IngredientProviders) return "Ingredient_Provider";

			throw new ArgumentException();
		} 
		private async Task LoadThumbtails()
		{
			this.loadedThumbnailFilenames.Clear();

			StorageFolder thumbnailFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
				"thumbnails",
				CreationCollisionOption.OpenIfExists
				);

			foreach (var item in Ingredients)
			{
				string thumbnailFilename = item.ThumbnailFilename;
				if (string.IsNullOrEmpty(thumbnailFilename)) continue;
				if (thumbnailFolder.TryGetItemAsync(thumbnailFilename) == null) continue;

				try
				{
					StorageFile thumbnailFile = await thumbnailFolder.GetFileAsync(thumbnailFilename);
					var softwareBitmap = await ThumbnailManager.LoadImageAsync(thumbnailFile);
					var source = new SoftwareBitmapSource();
					await source.SetBitmapAsync(softwareBitmap);

					item.ImageSource = source;
					this.loadedThumbnailFilenames.Add(thumbnailFilename);
				}
				catch { Debug.WriteLine("file not founc " + item.ThumbnailFilename);  }
			}

			foreach (var item in Recipes)
			{
				string thumbnailFilename = item.ThumbnailFilename;
				if (string.IsNullOrEmpty(thumbnailFilename)) continue;
				if (thumbnailFolder.TryGetItemAsync(thumbnailFilename) == null) continue;

				try
				{
					StorageFile thumbnailFile = await thumbnailFolder.GetFileAsync(thumbnailFilename);
					var softwareBitmap = await ThumbnailManager.LoadImageAsync(thumbnailFile);
					var source = new SoftwareBitmapSource();
					await source.SetBitmapAsync(softwareBitmap);

					item.ImageSource = source;
					this.loadedThumbnailFilenames.Add(thumbnailFilename);
				}
				catch { Debug.WriteLine("file not founc " + item.ThumbnailFilename); }
			}
		}
		private async Task DeleteUnusedThumbnails()
		{
			StorageFolder thumbnailFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
				"thumbnails",
				CreationCollisionOption.OpenIfExists
				);

			var files = await thumbnailFolder.GetFilesAsync();
			var unusedFiles = files.Where(f => !loadedThumbnailFilenames.Contains(f.Name));
			foreach (var file in unusedFiles)
			{
				await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
			}
		}

		// IDisposable interface
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (db != null)
				{
					db.Close();
					db.Dispose();
				}
			}
		}
	}


	static class MainModelViewExtensions
	{
		public static GroupedList GetGroupedList(this IEnumerable<IHasCategory> list)
		{
			var query = from item in list
						group item by item.Category into g
						orderby g.Key
						select new { GroupName = g.Key, Items = g };

			var groups = new ObservableCollection<KeyedList>();
			foreach (var g in query)
			{
				KeyedList info = new KeyedList(g.GroupName);
				foreach (var item in g.Items)
				{
					info.Add(item);
				}
				groups.Add(info);
			}

			return groups;
		}

		public static KeyedList GetOrAddGroup(this GroupedList list, string key)
		{
			// get group (keyed list) in grouped list with specified key 
			// if no group is found, new group is created and returned

			if (list.Any(g => g.Key == key))
			{
				return list.First(g => g.Key == key);
			}
			else
			{
				var newGroup = new KeyedList(key);
				list.Add(newGroup);
				return newGroup;
			}
		}

		public static void CountItemsFrom(this CategoryList categories, GroupedList groupedList)
		{
			// count item in each category group

			foreach (var g in groupedList)
			{
				foreach (var c in categories.Where(c => c.Name == g.Key))
				{
					c.NumOfItems = g.Count;
				}
			}
		}
	}
}
