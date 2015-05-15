using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace NetTcpServer
{
  [DataContract]
  class SecurityAuthorisationFailure { }

  [ServiceContract]
  interface IService1
  {
    [OperationContract, FaultContract(typeof(SecurityAuthorisationFailure))]
    void Foo();
  }

  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, UseSynchronizationContext = false)]
  class Service1 : IService1
  {
    public void Foo()
    {
      var name = OperationContext.Current.ServiceSecurityContext.PrimaryIdentity.Name;

      Console.WriteLine("{0}\tCall from {1}", DateTime.Now, name);
      
      if (name.Equals("kestral\\gavinm", StringComparison.CurrentCultureIgnoreCase)) 
        throw new FaultException<SecurityAuthorisationFailure>(new SecurityAuthorisationFailure(), "lalala");
      
      //throw new NotImplementedException();
    }
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

      var serviceHost = new ServiceHost(new Service1());
      serviceHost.AddServiceEndpoint(typeof(IService1), binding, target);
      serviceHost.Credentials.WindowsAuthentication.AllowAnonymousLogons = false;

      /*var throttlingBehavior = serviceHost.Description.Behaviors.Find<ServiceThrottlingBehavior>();
      if (throttlingBehavior == null)
      {
        throttlingBehavior = new ServiceThrottlingBehavior();
        serviceHost.Description.Behaviors.Add(throttlingBehavior);
      }
      //ServiceThrottlingBehavior.MaxConcurrentSessions = 2;
      */


      serviceHost.Open();

      Console.WriteLine("Listening on " + target);
      Console.WriteLine("Press enter to close ...");
      Console.ReadLine();

      serviceHost.Close();
    }
  }
}

