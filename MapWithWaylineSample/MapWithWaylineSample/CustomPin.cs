using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Maps;

namespace MapWithWaylineSample
{
    public class CustomPin
    {
        public Pin Pin { get; set; }
        public string Id { get; set; }
        public uint XIndex { get; set; }
        public int Depth { get; set; }
    }
}
