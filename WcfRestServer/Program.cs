using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace WcfRestServer
{
  [DataContract]
  class Metadata
  {
    [DataMember]
    public string Name;
    [DataMember]
    public DateTime Modified;
  }
  [ServiceContract]
  interface ITestService
  {
    [WebGet(UriTemplate = "/query/{query}", ResponseFormat = WebMessageFormat.Json)]
    Metadata[] Query(string query);
  }
  class TestService : ITestService
  {
    public Metadata[] Query(string query)
    {
      return new[] {
        new Metadata() { Name = "one.txt", Modified = new DateTime(2016,5,5,12,45,0) },
        new Metadata() { Name = "two.jpg", Modified = new DateTime(2016,5,5,13,56,0) }
      };
    }
  }
  class Program
  {
    static void Main(string[] args)
    {
      Console.Write("Starting ...");

      var provider = new TestService();
      var host = new WebServiceHost(typeof(TestService), new Uri("http://localhost:5000"));
      host.Open();
      
      Console.WriteLine(" ready!");
      Console.ReadLine();
    }
  }
}
