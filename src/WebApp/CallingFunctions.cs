using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WebApp
{
    public class CallingFunctions
    {
        public AIFunction GetWeatherTool => AIFunctionFactory.Create(GetWeather, nameof(GetWeather));
        public AIFunction GetTimeTool => AIFunctionFactory.Create(GetTimeWithZone, nameof(GetTimeWithZone));
        public AIFunction TestExceptionTool => AIFunctionFactory.Create(TestException, nameof(TestException));

        [Description("Ottiene il meteo attuale nella città")]
        public string GetWeather([Description("Il nome della città")] string city)
        {
            switch (city)
            {
                case "Venezia":
                    return GetTemperatureValue(20);
                case "Mestre":
                    return GetTemperatureValue(15);
                case "Padova":
                    return GetTemperatureValue(18);
                default:
                    return $"Le informazioni sul meteo a {city} non sono disponibili.";
            }
        }

        [Description("Ottiene l'ora corrente e il fuso orario")]
        public string GetTimeWithZone()
        {
            var now = DateTime.Now;
            var tz = TimeZoneInfo.Local;
            return $"{now:HH:mm} ({tz.DisplayName})";
        }
        
        [Description("Una funzione di test che genera sempre un'eccezione")]
        public string TestException()
        {
            throw new InvalidOperationException("Questa funzione è progettata per generare un errore a scopo dimostrativo.");
        }

        private string GetTemperatureValue(int value)
        {
            var valueInFahrenheits = value * 9 / 5 + 32;
            return $"{valueInFahrenheits}\u00b0F ({value}\u00b0C)";
        }
    }
}
