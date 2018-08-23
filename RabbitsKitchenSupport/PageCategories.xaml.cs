using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RabbitsKitchenSupport
{
	using CategoryList = ObservableCollection<Category>;

	public sealed partial class PageCategories : Page
	{
		// private fields
		string CategoryName;
		CategoryList categories = null;

		// constructors
		public PageCategories()
		{
			this.InitializeComponent();

			// register keyboard events for back action
			KeyboardAccelerator GoBack = new KeyboardAccelerator();
			GoBack.Key = VirtualKey.GoBack;
			GoBack.Invoked += BackInvoked;
			KeyboardAccelerator AltLeft = new KeyboardAccelerator();
			AltLeft.Key = VirtualKey.Left;
			AltLeft.Modifiers = VirtualKeyModifiers.Menu;
			AltLeft.Invoked += BackInvoked;
			this.KeyboardAccelerators.Add(GoBack);
			this.KeyboardAccelerators.Add(AltLeft);
		}

		// overrided methods
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			var (title, name, source) = ((string, string, CategoryList))e.Parameter;

			this.TextBlockTitle.Text = title;
			this.CategoryName = name;
			this.ListViewMain.ItemsSource = this.categories = source;
		}

		// event handlers
		private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
		{
			var b = sender as AppBarButton;

			switch (b.Tag as string)
			{
				case "Add":
					AddCategory();
					break;
				case "Edit":
					EditCategory();
					break;
			}
		}
		private void DeleteConfirmation_Click(object sender, RoutedEventArgs e)
		{
			DeleteCategory();

			DeleteFlyout.Hide();
		}
		private void ButtonBack_Tapped(object sender, TappedRoutedEventArgs e)
		{
			OnBackRequested();
		}
		private void BackInvoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
		{
			OnBackRequested();
			args.Handled = true;
		}

		// private methods
		private async void AddCategory()
		{
			var category = new Category();
			var dialog = new ContentDialogCategories($"Add {CategoryName}", category);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				MainModelView.Current.AddCategory(category, categories);
			}
		}
		private async void EditCategory()
		{
			if (!(this.ListViewMain.SelectedItem is Category category)) return;

			string oldName = category.Name;
			var dialog = new ContentDialogCategories($"Edit {CategoryName}", category);
			var result = await dialog.ShowAsync();
			if (result == ContentDialogResult.Primary)
			{
				MainModelView.Current.UpdateCategory(category, categories, oldName);
			}
		}
		private void DeleteCategory()
		{
			if (!(this.ListViewMain.SelectedItem is Category category)) return;

			MainModelView.Current.DeleteCategory(category, categories);
		}
		private bool OnBackRequested()
		{
			if (this.Frame.CanGoBack)
			{
				this.Frame.GoBack();
				return true;
			}
			return false;
		}
	}
}
