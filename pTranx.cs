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
            var payInTransactionList = new List<PayInTransaction>();
           
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.Headers.Add("Content-Security-Policy", "defautl-src 'self'; script-src 'self'");
            
            //var jsondate = JsonSerializer.Serialize(resData);
            initTransactions(payInTransactionList);
            payInTransactionAll(payInTransactionList);
            var jsondate = JsonSerializer.SerializeToUtf8Bytes(payInTransactionList); 


            //return new HttpResponseMessage(HttpStatusCode.OK) {
            //    Content = new StringContent(jsondate, Encoding.UTF8, "application/json")
           // };
            
            return new FileContentResult(jsondate, "application/json");
        }

        public void initTransactions(List<PayInTransaction> payInTransactionList)
        {
            for(int i=0; i<2; i++)
            {
                PayInTransaction payInTransaction = new PayInTransaction();
                payInTransactionList.Add(payInTransaction);
            }
        }

        public void payInTransactionAll(List<PayInTransaction> payInTransactionList)
        {
            foreach (PayInTransaction i in payInTransactionList) 
            {
                
                foreach (Person p in i.PawtnaItem.PersonList)
                {
                    payInTransactionsFunction(i.PayInDate,p, i.PawtnaItem );
                }
                
            } 
        }

        public void payInTransactionsFunction(DateTime payInDate, Person person, PawtnaItem pawtna)
        {
            if(payInDate == DateTime.Now)
            {
                person.Wallet.Stash = person.Wallet.Stash - pawtna.PayIn;
                pawtna.Bank = pawtna.Bank  + pawtna.PayIn;
            }
        }

    }



}
