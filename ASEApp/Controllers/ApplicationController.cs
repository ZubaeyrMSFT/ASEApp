using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QC = Microsoft.Data.SqlClient;
using DT = System.Data; //
using RestSharp;

namespace ASEApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApplicationController : ControllerBase
    {

        private readonly ILogger<ApplicationController> _logger;

        public ApplicationController(ILogger<ApplicationController> logger)
        {
            _logger = logger;
        }


        [Route("/")]
        public String getWelcomeMessage()
        {
            _logger.LogDebug("ZBXXX : home path!");
            Console.WriteLine("ZBXXX : home path!");

            return "Welcome to WebApp v6.0 from git local";
        }


        [Route("/weatherdata/date/{eventDate}")]
        public ArrayList getWeatherInfoByDate(int eventDate)
        {
            _logger.LogDebug("ZBXXX : fetching weather data by date = " + eventDate);
            Console.WriteLine("ZBXXX : fetching weather data by date = " + eventDate);
            return getWeatherDataFromDB(eventDate);
        }

        [Route("/weatherdata/name/{locName}")]
        public ArrayList getWeatherInfoByLoc(String locName)
        {
            _logger.LogDebug("ZBXXX : fetching weather data by loc = " + locName);
            Console.WriteLine("ZBXXX : fetching weather data by loc = " + locName);
            return getWeatherDataFromDB(locName);
        }


        /*      [HttpPost]*/
        [Route("/weatherdata/insert/{locName}/{eventDate}/{rainfall}")]
        public WeatherData insertWeatherData(String locName, int eventDate, double rainfall)
        {
            try
            {
                _logger.LogDebug("ZBXXX : inserting weather data, locName= "
                    + locName + ", eventDate=" + eventDate + ",rainfall=" + rainfall);
                return insertWeatherDataIntoDB(locName, eventDate, rainfall);
            }
            catch (Exception e)
            {
                _logger.LogError(e.StackTrace);
                return new WeatherData();
            }
        }


        [Route("/error")]
        public String getErrorMessage()
        {
            _logger.LogWarning("ZBXXX : incorrect path found!");
            Console.WriteLine("ZBXXX : incorrect path found!");
            return "Resource Not Found!";
        }


        [Route("/hybridcon")]
        public String hybridConnectionTest(int eventDate)
        {
            _logger.LogDebug("ZBXXX : redirecting to hybrid con ");
            Console.WriteLine("ZBXXX : redirecting to hybrid con ");

            var client = new RestClient("http://mywebapp:80");
            var request = new RestRequest("/", Method.GET);
            IRestResponse response = client.Execute(request);

            _logger.LogDebug("ZBXXX : response is " + response);
            Console.WriteLine("ZBXXX : response is " + response);
            return response.Content;
        }


        private QC.SqlConnection getDBConnection()
        {
            lock (this)
            {
                _logger.LogDebug("ZBXXX : Connecting To Azure SQL Database");
                Console.WriteLine("ZBXXX : Connecting To Azure SQL Database");

                var connection = new QC.SqlConnection("Server=tcp:dotnetmigration.database.windows.net,1433;" +
                    "Initial Catalog=dotnetmigration;Persist Security Info=False;User ID=zubair;Password=microsoft@123;" +
                    "MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=10;");

                connection.Open();

                _logger.LogDebug("ZBXXX : DB Connection Initialized Successfully!");
                Console.WriteLine("ZBXXX : DB Connection Initialized Successfully!");
                return connection;
            }
        }



        private ArrayList getWeatherDataFromDB(int eventDate)
        {
            ArrayList finalData = new ArrayList();
            using (var command = new QC.SqlCommand())
            {
                command.Connection = getDBConnection();
                command.CommandType = DT.CommandType.Text;
                command.CommandText = @"SELECT * FROM weather WHERE event_date=" + eventDate;

                _logger.LogDebug("ZBXXX : Executing By-Date Query...");
                QC.SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    finalData.Add(new WeatherData
                    {
                        LocationName = reader.GetString(0),
                        EventDate = reader.GetInt32(1),
                        Rainfall = reader.GetDouble(2)
                    });
                }

                _logger.LogDebug("final data is -> " + finalData);
            }

            return finalData;
        }


        private ArrayList getWeatherDataFromDB(String locationName)
        {
            ArrayList finalData = new ArrayList();

            using (var command = new QC.SqlCommand())
            {
                command.Connection = getDBConnection();
                command.CommandType = DT.CommandType.Text;
                command.CommandText = @"SELECT * FROM weather WHERE location_name='" + locationName + "'";

                _logger.LogDebug("ZBXXX - Executing ByName Query...");
                QC.SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    finalData.Add(new WeatherData
                    {
                        LocationName = reader.GetString(0),
                        EventDate = reader.GetInt32(1),
                        Rainfall = reader.GetDouble(2)
                    });
                }

                _logger.LogDebug("final data is -> " + finalData);
            }

            return finalData;
        }



        private WeatherData insertWeatherDataIntoDB(String locationName, int eventDate, double rainfall)
        {
            using (var command = new QC.SqlCommand())
            {
                command.Connection = getDBConnection();
                command.CommandType = DT.CommandType.Text;
                command.CommandText = @"INSERT into weather 
                    values('" + locationName + "', " + eventDate + "," + rainfall + ")";

                _logger.LogDebug("ZBXXX - Running Insert Query...");
                command.ExecuteScalar();
                _logger.LogDebug("ZBXXX - Successfully Inserted Data...");
            }

            return new WeatherData
            {
                LocationName = locationName,
                EventDate = eventDate,
                Rainfall = rainfall
            };
        }

    }
}