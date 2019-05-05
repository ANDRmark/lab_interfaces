using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace WeatherInformer.WeatherClient
{
    public class WeatherClient
    {
        private static readonly string DEFAUL_TUNITS = "metric";
        private static readonly string APPID = @"ee7630a47a3c6a249628c0d2d09bb700";
        private static readonly string baseurl = @"http://api.openweathermap.org/data/2.5";
        public async Task<Models.CurrentWeather.CurrentWeather> GetCurrentWeatherAsync(long location)
        {
            var url = baseurl +"/weather";
            var parameters = PrepareCurrentWeatherParamneters(location);
            var result = await Get(url, parameters);
            return JsonSerializer.CreateDefault().Deserialize<Models.CurrentWeather.CurrentWeather>(new JsonTextReader(new StringReader(result)));
        }

        public async Task<Models.Forecast5days3hours.ForecastBy3h> GetForecastAsync(long location, int numberOfDays)
        {
            var url = baseurl + "/forecast";
            var parameters = PrepareForecastParamneters(location, numberOfDays);
            var result = await Get(url, parameters);
            return JsonSerializer.CreateDefault().Deserialize<Models.Forecast5days3hours.ForecastBy3h>(new JsonTextReader(new StringReader(result)));
        }

        private Dictionary<string, string> PrepareForecastParamneters(long location, int numberOfDays, string unitSystem = null)
        {
            var parameters = new Dictionary<string, string>();
            parameters["units"] = unitSystem ?? DEFAUL_TUNITS;
            parameters["id"] = location.ToString();
            parameters["cnt"] = (numberOfDays * 8).ToString(); // forecast made by 3 hours, one day is 8 forecasts
            parameters["APPID"] = APPID;
            return parameters;
        }

        private Dictionary<string, string> PrepareCurrentWeatherParamneters(long location, string unitSystem = null)
        {
            var parameters = new Dictionary<string, string>();
            parameters["units"] = unitSystem ?? DEFAUL_TUNITS;
            parameters["id"] = location.ToString();
            parameters["APPID"] = APPID;
            return parameters;
        }

        private async Task<string> Get(string url, Dictionary<string, string> queryParameters)
        {
            using (HttpClient client = new HttpClient())
            {
                url = ConstructURLWithParams(url, queryParameters);
                return await (await client.GetAsync(url)).Content.ReadAsStringAsync();
            }
        }

        private string ConstructURLWithParams(string url, Dictionary<string, string> querystringParams)
        {
            var builder = new UriBuilder(url);
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            foreach(var key in querystringParams.Keys)
            {
                query[key] = querystringParams[key];
            }
            builder.Query = query.ToString();
            return builder.ToString();
        }
    }
}
        
