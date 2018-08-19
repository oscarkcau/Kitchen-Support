using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI.Extensions;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RabbitsKitchenSupport
{
	public sealed partial class ContentDialogIngredientQuantity : ContentDialog
	{
		RecipeIngredientItem ingredientItem;

		public ContentDialogIngredientQuantity(RecipeIngredientItem item)
		{
			this.InitializeComponent();

			// initialize data context
			this.ingredientItem = item;
			this.DataContext = item;
		}

		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
		{
			// make sure quantity is in correct format
			if (TextBoxRegex.GetIsValid(TextBoxQuantity) == false)
			{
				FlyoutWarmingText.Text = "Wrong numerical value!";
				FlyoutWarming.ShowAt(TextBoxQuantity);
				args.Cancel = true;
				return;
			}

			// perform real update
			TextBoxQuantity.GetBindingExpression(TextBox.TextProperty).UpdateSource();
		}
	}
}
