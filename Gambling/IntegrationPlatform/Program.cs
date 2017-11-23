

using CertificateManager;
using Contracts;
using NLog;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
    public class Program
    {
        private static readonly Logger loger = LogManager.GetLogger("Syslog");
        static void Main(string[] args)
        {
            Persistance.EmptyFiles();

            NetTcpBinding binding = new NetTcpBinding();

            string address = "net.tcp://localhost:" + Helper.integrationHostPort + "/BetIntegrationPlatform";
            ServiceHost hostBet = new ServiceHost(typeof(BetService));
            hostBet.AddServiceEndpoint(typeof(IBetService), binding, address);

            hostBet.Authorization.ServiceAuthorizationManager = new CustomAuthorizationManager();

            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
            policies.Add(new CustomAuthorizationPolicy());
            hostBet.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
            hostBet.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            hostBet.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            hostBet.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });

            ServiceSecurityAuditBehavior newAudit = new ServiceSecurityAuditBehavior();
            newAudit.AuditLogLocation = AuditLogLocation.Application;
            newAudit.ServiceAuthorizationAuditLevel = AuditLevel.SuccessOrFailure;
            newAudit.SuppressAuditFailure = true;

            hostBet.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            hostBet.Description.Behaviors.Add(newAudit);


            hostBet.Open();
            Console.WriteLine("Bet Integration Platform service is started.");
            Console.WriteLine("Press <enter> to stop service...");


            //ZBOG DEPOZITA
            address = "net.tcp://localhost:" + Helper.integrationHostPort + "/BetIntegrationPlatform2";
            ServiceHost hostBet2 = new ServiceHost(typeof(BetService));
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            hostBet2.AddServiceEndpoint(typeof(IBetService), binding, address);


            string srvCertCN = "bankserviceintegration";

            hostBet2.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            hostBet2.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            ///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
            hostBet2.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

            hostBet2.Open();
            Console.WriteLine("Bet Integration Platform  2 service is started.");
            Console.WriteLine("Press <enter> to stop service...");





            binding = new NetTcpBinding();
            address = "net.tcp://localhost:" + Helper.integrationHostPort + "/BankIntegrationPlatform";
            ServiceHost hostBank = new ServiceHost(typeof(BankService));
            hostBank.AddServiceEndpoint(typeof(IBankService), binding, address);


            hostBank.Authorization.ServiceAuthorizationManager = new CustomAuthorizationManager();
            hostBank.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
            hostBank.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            hostBank.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            hostBank.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });

            hostBank.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            hostBank.Description.Behaviors.Add(newAudit);

            hostBank.Open();
            Console.WriteLine("Bank Integration Platform is started.");
            Console.WriteLine("Press <enter> to stop service...");



            address = "net.tcp://localhost:" + Helper.integrationHostPort + "/ClientIntegrationPlatform";
            ServiceHost hostClient = new ServiceHost(typeof(ClientHelper));
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            hostClient.AddServiceEndpoint(typeof(IClientHelper), binding, address);


            hostClient.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            hostClient.Description.Behaviors.Add(newAudit);

            srvCertCN = "betserviceintegration";

            hostClient.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            hostClient.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            ///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
            hostClient.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

            hostClient.Open();
            Console.WriteLine("Client Integration Platform is started.");
            Console.WriteLine("Press <enter> to stop service...");


            Console.ReadLine();
            hostBet.Close();
            hostBank.Close();
            hostClient.Close();
        }
    }
}
