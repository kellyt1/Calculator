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
            var payOutTransactionList = new List<PayOutTransaction>();

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.Headers.Add("Content-Security-Policy", "defautl-src 'self'; script-src 'self'");
            
            //var jsondate = JsonSerializer.Serialize(resData);
            initTransactions(payOutTransactionList);
            payOutTransactionAll(payOutTransactionList);
            PayOutTransactionResponse payOutTransactionResponse = new PayOutTransactionResponse();
            payOutTransactionResponse.PayOutTransactions = payOutTransactionList;
            var jsondate = JsonSerializer.SerializeToUtf8Bytes(payOutTransactionResponse); 

            return new FileContentResult(jsondate, "application/json");

        }

        public void initTransactions(List<PayOutTransaction> payOutTransactionList)
        {
            for(int i=0; i<2; i++)
            {
                PayOutTransaction payOutTransaction = new PayOutTransaction();
                //PawtnaPayIn pawtnaPayIn = new PawtnaPayIn();
                Wallet wallet = new Wallet(){  Stash=10};
                
                PawtnaItem pawtnaItem = new PawtnaItem();
                pawtnaItem.Bank = new Bank(){BankAcct = "test"+i, Value=500};
                pawtnaItem.PayOut = 500;
                pawtnaItem.NumOfPeople = 2;
                Person person = new Person(){ Name="test"+i, Wallet=wallet};


                PersonPayOut personPayOut = new PersonPayOut(){Pawtna = pawtnaItem, Person = person};

                payOutTransaction.PersonPayOut = personPayOut;
                payOutTransaction.PayOutDate = DateTime.Now;
                payOutTransactionList.Add(payOutTransaction);
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

        public void payOutTransactionAll(List<PayOutTransaction> payOutTransactionList)
        {
            foreach (PayOutTransaction i in payOutTransactionList) 
            {
                
                payOutTransactionsFunction(i.PayOutDate,i.PersonPayOut.Person, i.PersonPayOut.Pawtna );
                
            } 
        }

        public void payOutTransactionsFunction(DateTime payOutDate, Person person, PawtnaItem pawtna)
        {
            if(payOutDate.Date == DateTime.Now.Date)
            {
                person.Wallet.Stash = person.Wallet.Stash + pawtna.PayOut;
                pawtna.Bank.Value = pawtna.Bank.Value  - pawtna.PayOut;
            }
        }
    }
}
