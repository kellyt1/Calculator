using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace Company.Function
{
    public class calculator
    {
        private readonly ILogger<calculator> _logger;

        public calculator(ILogger<calculator> logger)
        {
            _logger = logger;
        }

        [Function("calculator")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = string.IsNullOrEmpty(requestBody) ? null : JsonSerializer.Deserialize<PawtnaItem>(requestBody);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.Headers.Add("Content-Security-Policy", "defautl-src 'self'; script-src 'self'");
            
            //var jsondate = JsonSerializer.Serialize(resData);
            var jsondate = JsonSerializer.SerializeToUtf8Bytes(createPawntaResponse()); 


            //return new HttpResponseMessage(HttpStatusCode.OK) {
            //    Content = new StringContent(jsondate, Encoding.UTF8, "application/json")
           // };
            
            return new FileContentResult(jsondate, "application/json");
        }

        public PawtnaResponsItem createPawntaResponse(){
            PawtnaResponsItem resData = new PawtnaResponsItem();

            var person = new Person();
            person.Name = "TK";
            var person2 = new Person();
            person2.Name = "Quality";
            var personList =  new List<Person> {person, person2 };
            resData.Person = personList;

            return resData;
        }
    }

    public class PawtnaItem
    {
        public string StartDate { get; set; }
    }

    public class PawtnaResponsItem
    {
        public List<Person> Person { get; set; }
        public PawtnaItem Pawtna { get; set; }
    }

    public class Person
    {
        public string Name { get; set; }

    }
}
