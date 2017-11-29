

using CertificateManager;
using Contracts;
using NLog;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.IdentityModel.Policy;
using System.IO;
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

        public static Dictionary<string, Dictionary<int, ClientProxy>> proxies = new Dictionary<string, Dictionary<int, ClientProxy>>();

        private static DateTime start;

        private static Dictionary<string, Dictionary<string, IntrusionTry>> attempts;
        public static Dictionary<string, Dictionary<string, IntrusionTry>> Attempts
        {
            get { return attempts; }
            set { attempts = value; }
        }
        static void Main(string[] args)
        {
            Attempts = new Dictionary<string, Dictionary<string, IntrusionTry>>(); //prvo mjesto log, drugo depozit, trece tiket
            start = DateTime.Now;

            NetTcpBinding binding = new NetTcpBinding();

            string address = "net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/BetIntegrationPlatform";
            ServiceHost hostBet = new ServiceHost(typeof(BetService));
            hostBet.AddServiceEndpoint(typeof(IBetServiceIntegration), binding, address);

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

            try
            {
                hostBet.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Bet Integration Platform service is started.");
            Console.WriteLine("Press <enter> to stop service...");


            //ZBOG DEPOZITA
            address = "net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/BetIntegrationPlatform2";
            ServiceHost hostBet2 = new ServiceHost(typeof(BetService));
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            hostBet2.AddServiceEndpoint(typeof(IBetServiceIntegration), binding, address);


            string srvCertCN = "bankserviceintegration";

            hostBet2.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            hostBet2.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            ///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
            hostBet2.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

            try
            {
                hostBet2.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Bet Integration Platform  2 service is started.");
            Console.WriteLine("Press <enter> to stop service...");




            binding = new NetTcpBinding();
            address = "net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/BankIntegrationPlatform";
            ServiceHost hostBank = new ServiceHost(typeof(BankService));
            hostBank.AddServiceEndpoint(typeof(IBankServiceIntegration), binding, address);


            hostBank.Authorization.ServiceAuthorizationManager = new CustomAuthorizationManager();
            hostBank.Authorization.ExternalAuthorizationPolicies = policies.AsReadOnly();
            hostBank.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.Custom;
            hostBank.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            hostBank.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });

            hostBank.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            hostBank.Description.Behaviors.Add(newAudit);

            try
            {
                hostBank.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Bank Integration Platform is started.");
            Console.WriteLine("Press <enter> to stop service...");



            address = "net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/ClientIntegrationPlatform";
            ServiceHost hostClient = new ServiceHost(typeof(ClientHelper));
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;
            hostClient.AddServiceEndpoint(typeof(IClientHelperIntegration), binding, address);


            hostClient.Description.Behaviors.Remove<ServiceSecurityAuditBehavior>();
            hostClient.Description.Behaviors.Add(newAudit);

            srvCertCN = "betserviceintegration";

            hostClient.Credentials.ClientCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.ChainTrust;
            hostClient.Credentials.ClientCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            ///Set appropriate service's certificate on the host. Use CertManager class to obtain the certificate based on the "srvCertCN"
            hostClient.Credentials.ServiceCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);

            try
            {
                hostClient.Open();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Client Integration Platform is started.");
            Console.WriteLine("Press <enter> to stop service...");


            Thread.Sleep(5000);
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                while (true)
                {
                    IntrusionDetection();
                    Thread.Sleep(3000);
                }
            }).Start();

            Console.ReadLine();
            hostBet.Close();
            hostBank.Close();
            hostClient.Close();
        }


        public static void IntrusionDetection()
        {
            if (File.Exists("ESB_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt"))//da ne iskace ako fajl ne postoji za novi dan
            {
                string line;
                int first;
                int last;
                string address;
                bool fresh = true;

                StreamReader file = new StreamReader("ESB_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

                string temp = file.ReadLine();

                while ((line = file.ReadLine()) != null)
                {
                    string time = line.Substring(0, 15);
                    string date = time.Substring(0, 6) + " 2017" + time.Substring(6, time.Length - 6);

                    if (Convert.ToDateTime(date) >= start)
                    {
                        if (fresh)
                        {
                            if (Convert.ToDateTime(date) == start)
                            /*moze se dogoditi da pri drugom ulazu u finkciju imamo vise starih unosa u istom vremenu. Ti unosi bi bili obradjeni kao novi.
                             To je greska
                             Zato ako je funkcija tek probudjena i imamo poklapanje vremena, preskacemo te unose, ako se posle dogodi vise unosa u logeru sa istim vremenom
                             fresh ce biti false i ono ce biti obradjeni*/
                            {
                                continue;
                            }
                            else
                                fresh = false;
                        }

                        start = Convert.ToDateTime(date);
                        if (line.Contains("Warn"))
                        {
                            first = line.IndexOf(" - ") + " - ".Length;
                            last = line.IndexOf(".", first);
                            string add = line.Substring(first, last - first);

                            first = line.IndexOf("address: ") + "address: ".Length;
                            last = line.IndexOf(" Port:", first);
                            address = line.Substring(first, last - first);

                            string type = "";
                            if (line.Contains("Bet login failed"))
                                type = "betlog";
                            else if (line.Contains("Bank login failed"))
                                type = "banklog";
                            else if (line.Contains("Deposit failed"))
                                type = "deposit";
                            else if (line.Contains("Failed to send ticket"))
                                type = "ticket";
                            else if (line.Contains("Failed to add user"))
                                type = "add";
                            else if (line.Contains("Failed to edit user"))
                                type = "edit";
                            else if (line.Contains("Failed to delete user"))
                                type = "delete";
                            else if (line.Contains("Failed to create user"))
                                type = "create";
                            else if (line.Contains("Failed to create report"))
                                type = "report";

                            if (type != "")
                                DetecetionToPrevention(type, date, address);

                        }
                    }
                }
                file.Close();
            }
        }

        private static void IntrusionPrevention(string address)
        {

            foreach (var item in proxies[address].Values)
            {
                item.CloseProxy();
                item.Close();
            }
            proxies.Remove(address);
        }

        private static void DetecetionToPrevention(string type, string date, string address)
        {
            if (!Attempts.ContainsKey(address))
            {
                Dictionary<string, IntrusionTry> intrusionTypes = new Dictionary<string, IntrusionTry>();
                intrusionTypes.Add("betlog", new IntrusionTry());
                intrusionTypes.Add("banklog", new IntrusionTry());
                intrusionTypes.Add("deposit", new IntrusionTry());
                intrusionTypes.Add("ticket", new IntrusionTry());
                intrusionTypes.Add("add", new IntrusionTry());
                intrusionTypes.Add("edit", new IntrusionTry());
                intrusionTypes.Add("delete", new IntrusionTry());
                intrusionTypes.Add("create", new IntrusionTry());
                intrusionTypes.Add("report", new IntrusionTry());

                intrusionTypes[type].LastTry = Convert.ToDateTime(date);
                intrusionTypes[type].Attempt = 1;

                Attempts.Add(address, intrusionTypes);
            }
            else
            {

                if (DateTime.Now - Attempts[address][type].LastTry < TimeSpan.FromMinutes(3))
                /*ako je unos pogresen u manje od 3 minuta, ako je razmak izmedju greske vise od 3 minuta pokusaji se vracaju na 1*/
                {
                    Attempts[address][type].Attempt += 1;
                    Attempts[address][type].LastTry = Convert.ToDateTime(date);
                }
                else
                {
                    Attempts[address][type].Attempt = 1;
                    Attempts[address][type].LastTry = Convert.ToDateTime(date);
                }
            }


            if (Attempts[address].Values.Any(x => x.Attempt == 3))
            {
                Attempts[address][type].Attempt = 0;
                IntrusionPrevention(address);
            }
        }
    }
}
