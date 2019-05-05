using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherInformer.WeatherClient.Models.Location
{
    public class Coord
    {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    public class Location
    {
        public int id { get; set; }
        public string name { get; set; }
        public string country { get; set; }
        public Coord coord { get; set; }
    }
}
