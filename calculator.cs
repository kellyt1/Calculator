using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Net;
using System.Net.NetworkInformation;
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
            PawtnaResponsItem pawtnaResponsItem = createPawntaResponse(data);

        var jsondate = JsonSerializer.SerializeToUtf8Bytes(createPawtnaTrnxRespons(pawtnaResponsItem)); 

            //return new HttpResponseMessage(HttpStatusCode.OK) {
            //    Content = new StringContent(jsondate, Encoding.UTF8, "application/json")
           // };

            
            
            return new FileContentResult(jsondate, "application/json");
        }

        public PawtnaTrnxResponsItem createPawtnaTrnxRespons(PawtnaResponsItem pawtnaResponsItem )
        {
            PawtnaTrnxResponsItem pawtnaTrnxResponsItem = new PawtnaTrnxResponsItem();
            PawtnaPayIn payin = pawtnaResponsItem.PawtnaPayinList.First();
            payin.PersonPayInList = pawtnaResponsItem.PersonList;
            var payInTrnxList = new List<PayInTransaction>();
            foreach(DateTime pin in payin.PayInDateList)
            {
                foreach( Person person in payin.PersonPayInList)
                {
                    PayInTransaction payInTransaction = new PayInTransaction();
                    PersonPayIn personPayIn = new PersonPayIn();
                    personPayIn.Pawtna = pawtnaResponsItem.Pawtna;
                    personPayIn.Person = person;
                    payInTransaction.PersonPayIn = personPayIn;
                    payInTransaction.PayInDate = pin;
                    payInTrnxList.Add(payInTransaction);
                }
            }
            pawtnaTrnxResponsItem.payInTransactionList = payInTrnxList;
            return pawtnaTrnxResponsItem;
        }
        public PawtnaResponsItem createPawntaResponse(PawtnaItem reqData){
            PawtnaResponsItem resData = new PawtnaResponsItem();

            var pawtna = new PawtnaItem();
            pawtna.Name = reqData.Name;
            pawtna.StartDate = reqData.StartDate;
            pawtna.NumOfPeople = reqData.NumOfPeople;
            // pawtna.PayIn = reqData.PayIn;
            pawtna.PayOut = reqData.PayOut;
            pawtna.PayInSchedule = reqData.PayInSchedule;
            pawtna.PayOutSchedule = reqData.PayOutSchedule;
            // pawtna.Duration = reqData.Duration;

           
            resData.PersonList = createPeoplebaseonRequestInput(reqData.NumOfPeople);
            resData.Pawtna = pawtna;
            calculate(resData);

            return resData;
        }

        public List<Person> createPeoplebaseonRequestInput(int numOfPeople)
        {
            var personList =  new List<Person>();
            for (int i = 0; i < numOfPeople; i++) 
            {
                var person = new Person();
                person.Name = "person"+i;
                person.Wallet = new Wallet(){Stash=200};
                personList.Add(person);
            }
            
            return personList;
        }

        public void calculate(PawtnaResponsItem pawtnaResponsItem)
        {
            calculatePawntaPayIn(pawtnaResponsItem.Pawtna);
            calculateDuration(pawtnaResponsItem.Pawtna);
            calculatePawntaDates(pawtnaResponsItem);
        }

        public void calculatePawntaDates(PawtnaResponsItem pawtnaResponsItem)
        { 
            calculatePawtnaPayOutDates(pawtnaResponsItem);
            calculatePawtnaPayInDates(pawtnaResponsItem);
        }

        public void calculatePawtnaPayOutDates(PawtnaResponsItem pawtnaResponsItem)
        {

        }

        public void calculatePawtnaPayInDates(PawtnaResponsItem pawtnaResponsItem)
        {
            var payInPeriod = pawtnaResponsItem.Pawtna.PayInSchedule * 7;
            var pawtnaPayInList = new List<PawtnaPayIn>();
            var numofPayIns = pawtnaResponsItem.Pawtna.Duration/pawtnaResponsItem.Pawtna.PayInSchedule;
            var startDate = dateConverter(pawtnaResponsItem.Pawtna.StartDate);
            DateTime nextDate =startDate;
            for (int i = 0; i < pawtnaResponsItem.Pawtna.NumOfPeople; i++) 
            {
                var pawtnaPayIn = new PawtnaPayIn();
                var payInDateList = new List<DateTime>();
                for(int j = 0; j < numofPayIns; j++)
                {
                     nextDate = j==0 ? startDate : nextDate.AddDays(payInPeriod);
                    payInDateList.Add(nextDate);
                }
                
                pawtnaPayIn.Pawtna = pawtnaResponsItem.Pawtna;
                pawtnaPayIn.PayInDateList=payInDateList;
                pawtnaPayInList.Add(pawtnaPayIn);

            }
            pawtnaResponsItem.PawtnaPayinList = pawtnaPayInList;
             
        }

        public DateTime dateConverter(string date)
        {
            var str = "5/12/2020";
            DateTime dt;
            var isValidDate = DateTime.TryParse(date, out dt);
            return dt;
            
        }

        public void calculateDuration(PawtnaItem pawtnaItem)
        {
            var a = (double) pawtnaItem.PayOut / pawtnaItem.PayOutSchedule; 
            var b = (double) a * pawtnaItem.PayInSchedule; 
            var c = (double) b / pawtnaItem.NumOfPeople; 
            var d = (double) pawtnaItem.NumOfPeople * pawtnaItem.PayOutSchedule; 

            if(validateCalcuator(c,d,pawtnaItem.PayInSchedule, pawtnaItem.PayOut))
            {
                pawtnaItem.Duration = d;
            }
        }

        public void calculatePawntaPayIn(PawtnaItem pawtnaItem)
        {

            var a = (double) pawtnaItem.PayOut / pawtnaItem.PayOutSchedule; 
            var b = (double) a * pawtnaItem.PayInSchedule; 
            var c = (double) b / pawtnaItem.NumOfPeople; 
            var d = (double) pawtnaItem.NumOfPeople * pawtnaItem.PayOutSchedule; 


            if(validateCalcuator(c,d,pawtnaItem.PayInSchedule, pawtnaItem.PayOut))
            {
                pawtnaItem.PayIn = c;
            }

        }

        public Boolean validateCalcuator(double a, double b, double c, double d)
        {
            var target = (b / c) * a;
            var valid = target == d ? true : false;
            return valid;
        }
    }

    public class PawtnaItem
    {
        public string StartDate { get; set; }
        public int NumOfPeople { get; set; }
        public double PayOut { get; set; }
        public int PayInSchedule { get; set; }
        public int PayOutSchedule { get; set; }
        public double PayIn { get; set; }
        public double Duration { get; set; }
        public string Name { get; set; }
        public Bank Bank { get; set; }
        //public List<PayInTransaction> payInTransactions { get; set; }
        //public List<PayOutTransaction> payOutTransactions { get; set; }
    }

    public class PawtnaPayIn
    {
        public PawtnaItem Pawtna { get; set; }
        public List<DateTime> PayInDateList { get; set; }
        public List<Person> PersonPayInList { get; set; }
    }

    public class PersonPayIn
    {
        public PawtnaItem Pawtna { get; set; }
        //public List<DateTime> PayInDateList { get; set; }
        public Person Person { get; set; }
    }

    public class PawtnaPayOut
    {
        public PawtnaItem Pawtna { get; set; }
        public List<DateTime> PayOutDateList { get; set; }
        public List<Person> PersonPayOutList { get; set; }
    }

    public class PersonPayOut
    {
        public PawtnaItem Pawtna { get; set; }
        public List<DateTime> PayOutDateList { get; set; }
        public Person Person { get; set; }
    }

    public class PawtnaResponsItem
    {
        public List<Person> PersonList { get; set; }
        public PawtnaItem Pawtna { get; set; }
        public List<PawtnaPayIn> PawtnaPayinList { get; set; }
        public List<PawtnaPayOut> PawtnaPayOutList { get; set; }
    }

    public class PawtnaTrnxResponsItem
    {
        public List<PayInTransaction> payInTransactionList { get; set; }
    }

    public class Person
    {
        public string Name { get; set; }
        public Wallet Wallet { get; set; }
    }

    public class Wallet
    {
        public double Stash { get; set; }
        public List<IndBank> IndBankList { get; set; }
        public List<IndCard> IndCards { get; set; }
    }


    public class PayInTransaction 
    {
        public DateTime PayInDate { get; set; }
       //public PawtnaPayIn PawtnaPayIn { get; set; }
       public PersonPayIn PersonPayIn { get; set; }
    }

        public class PayOutTransaction 
    {
        public DateTime PayOutDate { get; set; }
       public PawtnaPayOut PawtnaPayOut { get; set; }
    }

    public class PayInTransactionResponse
    {
        public List<PayInTransaction> PayInTransactions { get; set;}
    }

    public class PayOutTransactionResponse
    {
        public List<PayOutTransaction> PayOutTransactions { get; set;}
    }

    public class Bank
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public string BankAcct { get; set; }
    }

    public class IndBank
    {
        public string IndBankAccountNumber { get; set; }
    }

    public class IndCard
    {
        public string IndCardNumber { get; set; }
    }
}
