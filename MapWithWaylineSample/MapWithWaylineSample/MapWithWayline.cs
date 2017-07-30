using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace MapWithWaylineSample
{
    // Draws lines on map
    public class MapWithWayline : Map
    {
        public ObservableCollection<CustomPin> CustomPins { get; private set; } = new ObservableCollection<CustomPin>();

        private CustomPin selectedPin = null;

        public CustomPin SelectedPin
        {
            get { return selectedPin; }
            set
            {
                selectedPin = value;
                OnPropertyChanged();
            }
        }


        public MapWithWayline() : base()
        {
            CustomPins.CollectionChanged += CustomPins_CollectionChanged;
        }

        public void AddPin(Position position)
        {
            var label = $"{position.Latitude}, {position.Longitude}";
            var pin = new Pin { Position = position, Type = PinType.Generic, Label = label };
            var customPin = new CustomPin { Id = label, Pin = pin, Depth = 0, XIndex = 0 };
            CustomPins.Add(customPin);
        }

        private void CustomPins_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (CustomPin cp in e.OldItems)
                {
                    Pins.Remove(cp.Pin);
                }
            }

            if (e.NewItems != null)
            {
                foreach (CustomPin cp in e.NewItems)
                {
                    Pins.Add(cp.Pin);
                }
            }
        }
    }
}
