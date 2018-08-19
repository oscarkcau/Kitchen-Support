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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RabbitsKitchenSupport
{
	public sealed partial class ContentDialogCategories : ContentDialog
	{
		public ContentDialogCategories(string title, Category category = null)
		{
			this.InitializeComponent();

			// initialize customized title
			if (string.IsNullOrWhiteSpace(title) == false)
			{
				this.Title = title.Trim();
			}

			// initialize data context
			if (category != null)
			{
				this.DataContext = category;
			}
		}

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

			// perform real update
			TextBoxName.GetBindingExpression(TextBox.TextProperty).UpdateSource();
		}

	}
}
