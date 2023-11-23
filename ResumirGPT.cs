using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;

namespace AzureFunction
{
    public class RespuestaJson
    {
        public string resumen { get; set; }
        public string sentimiento { get; set; }
    }
    //CLASE REQUEST
    public class RootobjectRequest
    {
        public int frequency_penalty { get; set; }
        public int max_tokens { get; set; }
        public MessageRequest[] messages { get; set; }
        public int presence_penalty { get; set; }
        public object stop { get; set; }
        public float temperature { get; set; }
        public float top_p { get; set; }
    }

    public class MessageRequest
    {
        public string content { get; set; }
        public string role { get; set; }
        public MessageRequest(string contet,string role)
        {
            this.content = contet;
            this.role = role;
        }
    }
    public class TextoRecibido
    {
        public string texto { get; set; }
    }

    //CLASE RESPOND
    public class Rootobject
    {
        public string id { get; set; }
        public string _object { get; set; }
        public int created { get; set; }
        public string model { get; set; }
        public Prompt_Filter_Results[] prompt_filter_results { get; set; }
        public Choice[] choices { get; set; }
        public Usage usage { get; set; }
    }

    public class Usage
    {
        public int prompt_tokens { get; set; }
        public int completion_tokens { get; set; }
        public int total_tokens { get; set; }
    }

    public class Prompt_Filter_Results
    {
        public int prompt_index { get; set; }
        public Content_Filter_Results content_filter_results { get; set; }
    }

    public class Content_Filter_Results
    {
        public Hate hate { get; set; }
        public Self_Harm self_harm { get; set; }
        public Sexual sexual { get; set; }
        public Violence violence { get; set; }
    }

    public class Hate
    {
        public bool filtered { get; set; }
        public string severity { get; set; }
    }

    public class Self_Harm
    {
        public bool filtered { get; set; }
        public string severity { get; set; }
    }

    public class Sexual
    {
        public bool filtered { get; set; }
        public string severity { get; set; }
    }

    public class Violence
    {
        public bool filtered { get; set; }
        public string severity { get; set; }
    }

    public class Choice
    {
        public int index { get; set; }
        public string finish_reason { get; set; }
        public MessageRecibir message { get; set; }
        public Content_Filter_Results1 content_filter_results { get; set; }
    }

    public class MessageRecibir
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    public class Content_Filter_Results1
    {
        public Hate1 hate { get; set; }
        public Self_Harm1 self_harm { get; set; }
        public Sexual1 sexual { get; set; }
        public Violence1 violence { get; set; }
    }

    public class Hate1
    {
        public bool filtered { get; set; }
        public string severity { get; set; }
    }

    public class Self_Harm1
    {
        public bool filtered { get; set; }
        public string severity { get; set; }
    }

    public class Sexual1
    {
        public bool filtered { get; set; }
        public string severity { get; set; }
    }

    public class Violence1
    {
        public bool filtered { get; set; }
        public string severity { get; set; }
    }

    public static class ResumirGPT
    {
        [FunctionName("ResumirGPT")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string apiURL = "https://cele-ai.openai.azure.com/openai/deployments/oviedo/chat/completions?api-version=2023-07-01-preview";
            string apikey = req.Headers["api-key"];
            //RECOGEMOS EL BODY DEL REQUEST
            StreamReader sr = new StreamReader(req.Body);
            string texto = sr.ReadToEnd();
            TextoRecibido textorecibido = JsonConvert.DeserializeObject<TextoRecibido>(texto);
            
            //---POST---
            var client = new HttpClient();
            client.BaseAddress = new Uri(apiURL);
            client.DefaultRequestHeaders.Add("api-key", apikey);
            
            //primera request
            Task<string> resultado = requestIA(textorecibido.texto, "you are an ia that summarizes the texts you receive in four lines and in spanish",client,apiURL);           
            Rootobject root = JsonConvert.DeserializeObject<Rootobject>(await resultado);
            Choice[] choice = root.choices;
            string respuesta = choice[0].message.content;
            
            //segunda request
            Task<string> resultado1 = requestIA(textorecibido.texto, "Your role is to return only one word positive or negative feedback on ratings in spanish.", client, apiURL);
            root = JsonConvert.DeserializeObject<Rootobject>(await resultado1);
            choice = root.choices;
            string respuesta1 = choice[0].message.content;
            
            RespuestaJson respJson = new RespuestaJson();
            respJson.resumen = respuesta;
            respJson.sentimiento = respuesta1;
            var jsonRespond = System.Text.Json.JsonSerializer.Serialize(respJson);

            log.LogInformation("TODO CORECTO!!!");
            return new OkObjectResult(jsonRespond);
        }
        public async static Task<string> requestIA(string textoRecibido, string roleIA, HttpClient client,string apiURL)
        {
            RootobjectRequest request = new RootobjectRequest();
            request.frequency_penalty = 0;
            request.max_tokens = 800;
            MessageRequest[] messageRequests = new MessageRequest[2];
            messageRequests[0] = new MessageRequest(roleIA, "system");
            messageRequests[1] = new MessageRequest(textoRecibido, "user");
            request.messages = messageRequests;
            request.presence_penalty = 0;
            request.stop = null;
            request.temperature = 0.4F;
            request.top_p = 0.95F;
            var json = System.Text.Json.JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiURL, content);
            string resultado = await response.Content.ReadAsStringAsync();

            return resultado;
        }
    }
}
