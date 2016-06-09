using System;
using System.Net;
using System.Xml;
using NLog;

namespace WeatherCollector
{
    class CityInfo
    {
        public bool Loaded { get; private set; }

        private string Town
        {
            get
            {
                return _town ?? _cityID;
            }
            set { _town = value; }
        }

        private readonly string _cityID;

        public CityInfo(string cityID)
        {
            _cityID = cityID;
        }

        private string _temperature;
        private string _condition;
        private string _humidity;
        private string _windSpeed;
        private string _town;
        private Exception _lastErr;


        public void LoadWeather()
        {
            Console.Out.Write($"citam mesto c. { _cityID} ... ");
            const string unit = "&u=c"; // celsius
            const string cKey = "add yahoo key here";
            const string cSecret = "add yahoo secret here";

            var baseURL = string.Format($"http://weather.yahooapis.com/forecastrss?w={_cityID}{unit}");

            string urlDes, Params;
            var oAuth = new OAuthBase();
            var signature = oAuth.GenerateSignature(new Uri(baseURL), cKey, cSecret, "", "", "GET", oAuth.GenerateTimeStamp(), oAuth.GenerateNonce(), out urlDes, out Params);

            var wc = new WebClient();
            try
            {
                string downloadString = wc.DownloadString(new Uri($"{urlDes}?{Params}&oauth_signature={signature}"));

                //XDocument xdoc = XDocument.Parse(downloadString, LoadOptions.None);
                //xdoc.Save("c:\\temp\\weather.xml");
                //Console.Out.WriteLine(xdoc.FirstNode);

                XmlDocument wData = new XmlDocument();
                wData.LoadXml(downloadString);

                XmlNamespaceManager manager = new XmlNamespaceManager(wData.NameTable);
                manager.AddNamespace("yweather", "http://xml.weather.yahoo.com/ns/rss/1.0");

                XmlNode channel = wData.SelectSingleNode("rss").SelectSingleNode("channel");

                _temperature = channel.SelectSingleNode("item").SelectSingleNode("yweather:condition", manager).Attributes["temp"].Value;
                _condition = channel.SelectSingleNode("item").SelectSingleNode("yweather:condition", manager).Attributes["text"].Value;
                _humidity = channel.SelectSingleNode("yweather:atmosphere", manager).Attributes["humidity"].Value;
                _windSpeed = channel.SelectSingleNode("yweather:wind", manager).Attributes["speed"].Value;
                Town = channel.SelectSingleNode("yweather:location", manager).Attributes["city"].Value;

                Console.Out.WriteLine(Message());
                Loaded = true;
            }
            catch (Exception e)
            {
                Console.WriteLine($" ERROR: {e.Message}");
                _lastErr = e;
                Loaded = false;
            }
        }

        public void Output()
        {
            LogEventInfo logEventInfo;
            if (Loaded)
                logEventInfo = new LogEventInfo
                {
                    Level = LogLevel.Info,
                    Message = Message()
                };
            else
            {
                logEventInfo = new LogEventInfo
                {
                    Level = LogLevel.Error,
                    Message = string.Format($"ID Mesta: {_cityID} Chyba: {_lastErr.Message}")
                };
            }
            LogManager.GetCurrentClassLogger().Log(logEventInfo);
            Console.Out.WriteLine(logEventInfo.Message);
        }

        private string Message()
        {
            return $"Mesto: {Town}, Teplota: {_temperature}°C, Vlhkost: {_humidity}%, Vietor: {_windSpeed}km/h, {_condition}";
        }
    }
}
