using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace MapWithWaylineSample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MappingPage : ContentPage
    {
        private Task addMapPings;

        public MappingPage()
        {
            InitializeComponent();
        }

        private async void BackButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
        }

        void MappingPage_Appearing(object sender, System.EventArgs e)
        {
            addMapPings = Task.Run(() =>
            {
                foreach(double[] value in LocationData.Data)
                {
                    var position = new Position(value[0], value[1]);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        BaseMap.AddPin(position);
                        BaseMap.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromMiles(1.0)));
                    });
                    Thread.Sleep(1000);
                }
            });
        }
    }
}