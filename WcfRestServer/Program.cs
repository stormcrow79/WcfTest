using System;
using System.Collections.Generic;
using System.IdentityModel.Selectors;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
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

  [DataContract]
  class RequestInfo
  {
    [DataMember]
    public string Site;
    [DataMember]
    public string DocId;
    [DataMember]
    public string Filename;
  }
  [ServiceContract]
  interface ITestService
  {
    [WebInvoke(UriTemplate = "/get", BodyStyle = WebMessageBodyStyle.Bare, RequestFormat = WebMessageFormat.Json)]
    System.ServiceModel.Channels.Message Get(RequestInfo info);
    [WebGet(UriTemplate = "/query/{query}", ResponseFormat = WebMessageFormat.Json)]
    Metadata[] Query(string query);
    [WebInvoke(UriTemplate = "/process")]
    void Process(Stream request);
  }
  class TestService : ITestService
  {
    public System.ServiceModel.Channels.Message Get(RequestInfo info)
    {
      if (info.Filename == "test.txt")
      {
        return WebOperationContext.Current.CreateTextResponse("hello");
      }
      else if (info.Filename == "test.htm")
      {
        return WebOperationContext.Current.CreateTextResponse("<html><body>hello</body><html>", "text/html");
      }
      else
      {
        return WebOperationContext.Current.CreateTextResponse("nope");
        //WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.NotFound;
        //return new MemoryStream(new byte[0]);
      }
    }

    public void Process(Stream request)
    {
      var reader = new StreamReader(request);
      var text = reader.ReadToEnd();

      var r = new Random();
      if (r.Next(2) == 0)
      {
        //throw new NotImplementedException();
        WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.NotImplemented;
        return;
      }

      Console.WriteLine(text);
    }

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

      var address = new Uri("http://localhost:5000");

      var binding = new WebHttpBinding();
      //binding.Security.Mode = WebHttpSecurityMode.Transport;
      //binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

      var provider = new TestService();
      var host = new WebServiceHost(typeof(TestService), address);
      var endpoint = host.AddServiceEndpoint(typeof(ITestService), binding, address);

      //host.Credentials.ClientCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.Custom;
      //host.Credentials.ClientCertificate.Authentication.CustomCertificateValidator = new CustomCertValidator();

      host.Open();
      
      Console.WriteLine(" ready!");
      Console.ReadLine();
    }
  }
  class CustomCertValidator : X509CertificateValidator
  {
    public override void Validate(X509Certificate2 certificate)
    {
      // throw System.IdentityModel.Tokens.SecurityTokenvalidationException to reject
      return;
    }
  }
}
