using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WeatherInformer.WeatherClient.CurrentWeather;

namespace WeatherInformer.WeatherClient
{
    public class WeatherClient
    {
        private static readonly string APPID = @"ee7630a47a3c6a249628c0d2d09bb700";
        private static readonly string baseurl = @"http://api.openweathermap.org/data/2.5";
        public CurrentWeather.CurrentWeather GetCurrentWeather()
        {
            var url = baseurl +"/weather";
            throw new NotImplementedException();
        }

        public Forecast5days3hours.ForecastBy3h GetForecast()
        {
            var url = baseurl + "/forecast";
            throw new NotImplementedException();
        }

        private object Get(string url, Dictionary<string, string> queryParameters)
        {
            HttpClient client = new HttpClient();
            throw new NotImplementedException();
        }
    }
}
        
