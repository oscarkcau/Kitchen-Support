using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RabbitsKitchenSupport
{
	public sealed partial class ContentDialogIngredient : ContentDialog
	{
		// private fields
		Ingredient ingredient = null;
		string thumbnailFilename = null;

		// constructor
		public ContentDialogIngredient(string title, Ingredient ingredient)
		{
			this.InitializeComponent();

			// initialize customized title
			this.Title = title ?? throw new ArgumentException();

			// initialize data context
			this.ingredient = ingredient ?? throw new ArgumentException();
			this.DataContext = ingredient;
			this.thumbnailFilename = ingredient.ThumbnailFilename;

			// initialize combo box with predefined item sources
			ComboBoxUnit.ItemsSource = MainModelView.Current.Units;
			ComboBoxCategory.ItemsSource = MainModelView.Current.IngredientCategories;
		}

		// event handlers
		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			// make sure name and unit are not empty string
			if (string.IsNullOrWhiteSpace(TextBoxName.Text))
			{
				FlyoutWarmingText.Text = "Name cannot be empty!";
				FlyoutWarming.ShowAt(TextBoxName);
				args.Cancel = true;
				return;
			}
			if (ComboBoxCategory.SelectedIndex == -1)
			{
				FlyoutWarmingText.Text = "Category cannot be empty!";
				FlyoutWarming.ShowAt(ComboBoxCategory);
				args.Cancel = true;
				return;
			}
			if (ComboBoxUnit.SelectedIndex == -1)
			{
				FlyoutWarmingText.Text = "Unit cannot be empty!";
				FlyoutWarming.ShowAt(ComboBoxUnit);
				args.Cancel = true;
				return;
			}

			// perform real update
			TextBoxName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
			ComboBoxCategory.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
			ComboBoxUnit.GetBindingExpression(ComboBox.SelectedValueProperty).UpdateSource();
			ImageThumbnail.GetBindingExpression(Image.SourceProperty).UpdateSource();
			this.ingredient.ThumbnailFilename = this.thumbnailFilename;
		}
		private async void Image_Tapped(object sender, TappedRoutedEventArgs e)
		{
			// show picker for load image file
			var picker = new Windows.Storage.Pickers.FileOpenPicker();
			picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
			picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
			picker.FileTypeFilter.Add(".jpg");
			picker.FileTypeFilter.Add(".jpeg");
			picker.FileTypeFilter.Add(".png");

			StorageFile file = await picker.PickSingleFileAsync();
			if (file == null) return;

			var (thumbnailFilename, softwareBitmap) = await ThumbnailManager.AddThumbnailAsync(file);

			SoftwareBitmapSource source = new SoftwareBitmapSource();
			await source.SetBitmapAsync(softwareBitmap);
			this.ImageThumbnail.Source = source;
			this.thumbnailFilename = thumbnailFilename;
		}
		private void borderThumbnail_PointerEntered(object sender, PointerRoutedEventArgs e)
		{
			this.borderThumbnail.BorderThickness = new Thickness(4);
		}
		private void borderThumbnail_PointerExited(object sender, PointerRoutedEventArgs e)
		{
			this.borderThumbnail.BorderThickness = new Thickness(1);
		}
	}
}
