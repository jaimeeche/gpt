using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Security.Cryptography;

namespace Azure
{
    class Formato
    {
        public string text{ get; set; }
        public bool disponible { get; set; }
    }
    class PostData
    {
        public int id { get; set; }
        public string description { get; set; }
        public string active { get; set; }
    }

    public static class Function1
    {
        [FunctionName("post")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("SE HA PROCESADO UN REQUEST");

            //---------POST--------------
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://inforavanapitest.azurewebsites.net/Test");

            //Recogemos el body
            StreamReader sr = new StreamReader(req.Body);
            string texto = sr.ReadToEnd();

            //Deserealizamos el body
            Formato format = JsonConvert.DeserializeObject<Formato>(texto);

            //Introducimos los datos del body en el formato correcto
            PostData postData = new PostData
            {
                id = RandomNumberGenerator.GetInt32(100),
                description = format.text,
                active = format.disponible.ToString().ToLower()
            };           
           
            var json = System.Text.Json.JsonSerializer.Serialize(postData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response1 = client.PostAsync("https://inforavanapitest.azurewebsites.net/Test", content).Result;

      
            // -----GET----
            //crear request para la api
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://inforavanapitest.azurewebsites.net/Test/getlist");
            request.Method = "GET";

            //recogemos el response
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //recogemos su su stream
            Stream stream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(stream);
            String texto1 = streamReader.ReadToEnd();
            log.LogInformation("SE HA PROCESADO UN GET Y UN POST"); 
            return new OkObjectResult(texto1);


        }

    }
}
