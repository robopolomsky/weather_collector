using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WeatherCollector
{
    class Program
    {
        private static List<CityInfo> _cities;

        static void Main()
        {
            _cities = new List<CityInfo>
            {
                new CityInfo("820323"), // Kosice
                new CityInfo("821722"), // Poprad
                new CityInfo("821782"), // Presov
                new CityInfo("819877"), // Humenne
                new CityInfo("818511"), // Banska Bystrica
                new CityInfo("818717"), // Bratislava
                new CityInfo("823382") // Zilina
            };
            Console.Out.WriteLine($"Zistujem akutalne pocasie v {_cities.Count} mestach ...");

            int loadAttempts = 0;
            const int maxLoadAttempts = 200;
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
            }
            Console.Out.WriteLine("");
            foreach (CityInfo city in _cities)
                city.Output();

            //Console.ReadKey();
        }

    }
}
