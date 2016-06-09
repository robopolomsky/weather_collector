using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;

namespace WeatherCollector
{
    class Program
    {
        private static List<CityInfo> _cities;

        static void Main()
        {
            _cities = new List<CityInfo>();

            StringCollection collection = Properties.Settings.Default.Cities;
            foreach (string s in collection)
            {
                if (!s.Contains(","))
                {
                    string errMsg = string.Format($"CHYBA: Udaj o meste \"{s}\" nema ciarku");
                    Console.Out.WriteLine(errMsg);
                    NLog.LogManager.GetCurrentClassLogger().Error(errMsg);
                }
                string[] split = s.Split(',');
                _cities.Add(new CityInfo(split[1]));
            }

            Console.Out.WriteLine($"Zistujem akutalne pocasie v {_cities.Count} mestach ...");

            int loadAttempts = 0;
            int maxLoadAttempts = Properties.Settings.Default.MaxLoadAttempts;
            while (_cities.Any(x => !x.Loaded) && loadAttempts < maxLoadAttempts)
            {                
                foreach (CityInfo city in _cities.Where(x => !x.Loaded))
                {
                    loadAttempts++;
                    city.LoadWeather();
                }

                if(_cities.Any(x => !x.Loaded))
                    Thread.Sleep(5000); // 5 sec
            }
            if (loadAttempts >= maxLoadAttempts)
            {
                string errMsg = string.Format($"CHYBA: Dosiahnuty maximalny pocet {maxLoadAttempts} pokusov");
                Console.Out.WriteLine(errMsg);
                NLog.LogManager.GetCurrentClassLogger().Error(errMsg);
                Thread.Sleep(3000);
            }
            Console.Out.WriteLine("");
            for (int index = 0; index < _cities.Count; index++)
            {
                CityInfo city = _cities[index];
                city.Output(index == _cities.Count -1);
            }
            
            //Console.ReadKey();
        }

    }
}
