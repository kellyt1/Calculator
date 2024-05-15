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
    public class ptranxOut
    {
        private readonly ILogger<ptranxOut> _logger;

        public ptranxOut(ILogger<ptranxOut> logger)
        {
            _logger = logger;
        }

        [Function("ptranxOut")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var payOutTransactionList = new List<PayInTransaction>();

                        var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.Headers.Add("Content-Security-Policy", "defautl-src 'self'; script-src 'self'");
            
            //var jsondate = JsonSerializer.Serialize(resData);
            initTransactions(payOutTransactionList);
            payOutTransactionAll(payOutTransactionList);
            PayInTransactionResponse payInTransactionResponse = new PayInTransactionResponse();
            payInTransactionResponse.PayInTransactions = payOutTransactionList;
            var jsondate = JsonSerializer.SerializeToUtf8Bytes(payInTransactionResponse); 

            return new FileContentResult(jsondate, "application/json");

        }

                public void initTransactions(List<PayInTransaction> payInTransactionList)
        {
            for(int i=0; i<2; i++)
            {
                PayInTransaction payInTransaction = new PayInTransaction();
                PawtnaItem pawtnaItem = new PawtnaItem();
                pawtnaItem.Bank  = 0 ;
                pawtnaItem.PersonList = createPeoplebaseonRequestInput(2);
                pawtnaItem.PayIn = 50;

                payInTransaction.PawtnaItem = pawtnaItem;
                payInTransactionList.Add(payInTransaction);
                payInTransaction.PayInDate = DateTime.Now;
            }
        }

        public List<Person> createPeoplebaseonRequestInput(int numOfPeople)
        {
            var personList =  new List<Person>();
            for (int i = 0; i < numOfPeople; i++) 
            {
                var person = new Person();
                person.Name = "person"+i;
                Wallet wallet = new Wallet();
                wallet.Stash = 5000;
                person.Wallet = wallet;
                personList.Add(person);
            }
            
            return personList;
        }

        public void payOutTransactionAll(List<PayInTransaction> payInTransactionList)
        {
            foreach (PayInTransaction i in payInTransactionList) 
            {
                
                foreach (Person p in i.PawtnaItem.PersonList)
                {
                    payOutTransactionsFunction(i.PayInDate,p, i.PawtnaItem );
                }
                
            } 
        }

        public void payOutTransactionsFunction(DateTime payInDate, Person person, PawtnaItem pawtna)
        {
            if(payInDate.Date == DateTime.Now.Date)
            {
                person.Wallet.Stash = person.Wallet.Stash - pawtna.PayIn;
                pawtna.Bank = pawtna.Bank  + pawtna.PayIn;
            }
        }
    }
}
