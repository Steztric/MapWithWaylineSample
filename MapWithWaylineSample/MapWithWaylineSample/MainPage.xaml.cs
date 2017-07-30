using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace MapWithWaylineSample
{
	public partial class MainPage : ContentPage
    {
        private MappingPage mappingPage = new MappingPage();

        public MainPage()
		{
			InitializeComponent();
		}
        private async void LaunchMapButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(mappingPage);
        }
    }
}
