using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetTcpClient
{
  [DataContract]
  class SecurityAuthorisationFailure { }

  [ServiceContract]
  interface IService1
  {
    [OperationContract, FaultContract(typeof(SecurityAuthorisationFailure))]
    void Foo();
  }

  class Program
  {
    static void Main(string[] args)
    {
      var host = (args.Length > 0 ? args[0] : Environment.MachineName) + ":9170";
      var target = "net.tcp://" + host;

      var binding = new NetTcpBinding(SecurityMode.Transport);
      binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
      binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
      /*binding.ReliableSession.Enabled = true;
      binding.ReliableSession.InactivityTimeout = TimeSpan.FromSeconds(60);
      binding.ReliableSession.Ordered = false;*/
      //binding.TransferMode = TransferMode.Streamed;

      var epid = EndpointIdentity.CreateDnsIdentity(host);
      var ep = new EndpointAddress(new Uri(target), epid);

      while (true)
      {
        ChannelFactory<IService1> factory = null;

        try
        {
          factory = new ChannelFactory<IService1>(binding, ep);
          //factory.Credentials.Windows.ClientCredential = new NetworkCredential("karisma", "karisma", "kestral");

          var client = factory.CreateChannel();
          try
          {
            client.Foo();
          }
          catch (FaultException<SecurityAuthorisationFailure>)
          {
            factory.Abort();
            factory.Close();

            Console.WriteLine("Failed, retry as karisma");
            
            factory = new ChannelFactory<IService1>(binding, ep);
            factory.Credentials.Windows.ClientCredential = new NetworkCredential("karisma", "karisma", "kestral");

            client = factory.CreateChannel();
            try
            {
              client.Foo();
            }
            catch
            {
              Console.WriteLine("FAILED");
            }
          }
        }
        finally
        {
          factory.Close();
        }

        Thread.Sleep(200);
      }

      Console.WriteLine("Done. Press enter ...");
      Console.ReadLine();
    }
  }
}
