
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using StackExchange.Redis;
using State.Model;
using NReJSON;
using Newtonsoft.Json;
using System.Dynamic;
using System.Text.RegularExpressions;

namespace State.Controllers
{
    [Route("api/v1")]
    [ApiController]
    //[Route("[controller]")]
    public class MyGarden : ControllerBase
    {

        static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(
              new ConfigurationOptions
              {
                  User = "nehme",
                  Password = "Nehme.2022",
                  AbortOnConnectFail = false,
                  EndPoints = { "redis-13453.c92.us-east-1-3.ec2.cloud.redislabs.com:13453" }

              }
           );

        private readonly ILogger<MyGarden> _logger;

        public MyGarden(ILogger<MyGarden> logger)
        {
            _logger = logger;
        }


        [HttpGet("GetDetails")]
        public async Task<IEnumerable<Garden>> GetGardenDetails()
        {
            List<Garden> gardens = new List<Garden>().ToList();

            //Garden garden = new Garden();

            var db = redis.GetDatabase();

            string key = "";
            string json = "";
            for (int i = 1; i <= 5; i++)
            {

                Garden plant = new Garden();

                plant.Key = "Plant" + i.ToString();
                plant.Value = "Not Watered";
                plant.WaterTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                key = "Plant" + i.ToString();

                gardens.Add(plant);
                json = JsonConvert.SerializeObject(plant);
                OperationResult result = await db.JsonSetAsync(key, json);
            }
            return gardens;
        }

        [HttpGet("water-plant")]
        public async Task<IActionResult> UpdatePlants(string key, string condition, string updateTime)//, List<Garden> myparam)
        {
            IDatabase db = redis.GetDatabase();
            //string _key =  "Plant1";

            



            OperationResult result = await db.JsonSetAsync(key, JsonConvert.SerializeObject(condition), ".Value");
            await db.JsonSetAsync(key, JsonConvert.SerializeObject(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")), ".WaterTime");
            //string resultValue = result.RawResult.ToString();

            return Ok();

        }

        [HttpPut("multi-plant-water")]
        public async Task<ActionResult<Garden>> WaterMultiPlant(IEnumerable<Garden> myGarden)
        {
            IDatabase db = redis.GetDatabase();
            var keys = redis.GetServer("redis-13453.c92.us-east-1-3.ec2.cloud.redislabs.com:13453").Keys().ToList();
            string[] parms = { "." };
            List<string> listKeys = new List<string>();
            Garden garden = new Garden();
            List<Garden> gardens = new List<Garden>().ToList();
            string plant = "";
            listKeys.AddRange(keys.Select(key => (string)key).ToList());

            foreach(var item in myGarden)
            {
                foreach(var key in listKeys)
                {
                    await db.JsonSetAsync(item.Key, JsonConvert.SerializeObject(item.Value), ".Value");
                    await db.JsonSetAsync(item.Key, JsonConvert.SerializeObject(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")), ".WaterTime");
                }
            }

            return Ok();
        }

        [HttpGet("read-data")]
        public async Task<IActionResult> GetUserProfile(string key)
        {
            IDatabase db = redis.GetDatabase();
            //string key = "Plant2";
            string[] parms = { "." };
            RedisResult result = await db.JsonGetAsync(key, parms);
            if (result.IsNull) { return BadRequest("GetUserProfile Failed"); }
            string profile = (string)result;
            return Ok(profile);
        }

        [HttpGet("read-data-all")]
        public async Task<IEnumerable<Garden>> GetAllPlants()
        {

            IDatabase db = redis.GetDatabase();
            var keys = redis.GetServer("redis-13453.c92.us-east-1-3.ec2.cloud.redislabs.com:13453").Keys().ToList();
            string[] parms = { "." };
            List<string> listKeys = new List<string>();
            Garden garden = new Garden();
            List<Garden> gardens = new List<Garden>().ToList();
            string plant = "";

            listKeys.AddRange(keys.Select(key => (string)key).ToList());

            foreach (var item in listKeys)
            {
                RedisResult result = await db.JsonGetAsync(item, parms);
                gardens.Add(JsonConvert.DeserializeObject<Garden>(plant = (string)result));
            }

            return gardens;

        }

    }
}
