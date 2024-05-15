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
            PayInTransactionResponse payInTransactionResponse = new PayInTransactionResponse();
            payInTransactionResponse.PayInTransactions = payInTransactionList;
            var jsondate = JsonSerializer.SerializeToUtf8Bytes(payInTransactionResponse); 


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
                PawtnaPayIn pawtnaPayIn = new PawtnaPayIn();
                
                PawtnaItem pawtnaItem = new PawtnaItem();
                pawtnaItem.Bank.Value  = 0 ;
                pawtnaItem.PayIn = 50;
                pawtnaItem.NumOfPeople = 2;

                pawtnaPayIn.Pawtna = pawtnaItem;
                //pawtnaPayIn.PersonPayInList = createPeoplebaseonRequestInput(pawtnaItem.NumOfPeople);

                //payInTransaction.PawtnaPayIn = pawtnaPayIn;
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

        public void payInTransactionAll(List<PayInTransaction> payInTransactionList)
        {
            foreach (PayInTransaction i in payInTransactionList) 
            {
                
                // foreach (Person p in i.PawtnaPayIn.PersonPayInList)
                // {
                //     payInTransactionsFunction(i.PayInDate,p, i.PawtnaPayIn.Pawtna );
                // }
                
            } 
        }

        public void payInTransactionsFunction(DateTime payInDate, Person person, PawtnaItem pawtna)
        {
            if(payInDate.Date == DateTime.Now.Date)
            {
                person.Wallet.Stash = person.Wallet.Stash - pawtna.PayIn;
                pawtna.Bank.Value = pawtna.Bank.Value  + pawtna.PayIn;
            }
        }

    }



}
