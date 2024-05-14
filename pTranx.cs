using Microsoft.AspNetCore.Http;
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
    public class pTranx
    {
        private readonly ILogger<pTranx> _logger;

        public pTranx(ILogger<pTranx> logger)
        {
            _logger = logger;
        }

        [Function("pTranx")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
           
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.Headers.Add("Content-Security-Policy", "defautl-src 'self'; script-src 'self'");
            
            //var jsondate = JsonSerializer.Serialize(resData);
            var jsondate = JsonSerializer.SerializeToUtf8Bytes(payInTransactionsFunction()); 


            //return new HttpResponseMessage(HttpStatusCode.OK) {
            //    Content = new StringContent(jsondate, Encoding.UTF8, "application/json")
           // };
            
            return new FileContentResult(jsondate, "application/json");
        }

        public PayInTransaction payInTransactionsFunction()
        {
            PayInTransaction payInTransaction = new PayInTransaction();
            if(payInTransaction.PayInDate == DateTime.Now)
            {
                payInTransaction.Wallet.Stash = payInTransaction.Wallet.Stash - payInTransaction.PawtnaResponsItem.Pawtna.PayIn;
                payInTransaction.PawtnaResponsItem.Pawtna.Bank = payInTransaction.PawtnaResponsItem.Pawtna.Bank  + payInTransaction.PawtnaResponsItem.Pawtna.PayIn;
            }
            return payInTransaction;
        }

    }



}
