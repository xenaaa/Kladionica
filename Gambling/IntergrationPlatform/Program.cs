using Contracts;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace IntergrationPlatform
{
    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding();

            string address = "net.tcp://localhost:9991/BetIntegrationPlatform";
            ServiceHost host = new ServiceHost(typeof(BetService));
            host.AddServiceEndpoint(typeof(IBetService), binding, address);
            host.Authorization.ServiceAuthorizationManager = new CustomAuthorizationManager();

            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
            policies.Add(new CustomAuthorizationPolicy());
            host.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
            host.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });

            host.Open();
            Console.WriteLine("Bet Integration Platform service is started.");
            Console.WriteLine("Press <enter> to stop service...");


            address = "net.tcp://localhost:9991/BankIntegrationPlatform";
            ServiceHost host2 = new ServiceHost(typeof(BankService));
            host2.AddServiceEndpoint(typeof(IBankService), binding, address);


            host2.Authorization.ServiceAuthorizationManager = new CustomAuthorizationManager();
            host2.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
            host2.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            host2.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            host2.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });


            host2.Open();
            Console.WriteLine("Bank Integration Platform is started.");
            Console.WriteLine("Press <enter> to stop service...");



            address = "net.tcp://localhost:9991/ClientIntegrationPlatform";
            ServiceHost host3 = new ServiceHost(typeof(ClientHelper));
            host3.AddServiceEndpoint(typeof(IClientHelper), binding, address);


            host3.Authorization.ServiceAuthorizationManager = new CustomAuthorizationManager();
            host3.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
            host3.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            host3.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            host3.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });


            host3.Open();
            Console.WriteLine("Client Integration Platform is started.");
            Console.WriteLine("Press <enter> to stop service...");


            Console.ReadLine();
            host.Close();
            host2.Close();
            host3.Close();
        }
    }
}
