

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
        // public static Dictionary<int, ClientProxy> proxies2 = new Dictionary<int, ClientProxy>();

        private static DateTime start;
        private static Dictionary<string, IntrusionTry> attempts;

        public static Dictionary<string, IntrusionTry> Attempts
        {
            get { return attempts; }
            set { attempts = value; }
        }

        static void Main(string[] args)
        {
            Attempts = new Dictionary<string, IntrusionTry>();
            start = DateTime.Now;

            NetTcpBinding binding = new NetTcpBinding();

            string address = "net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/BetIntegrationPlatform";
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
            hostBet2.AddServiceEndpoint(typeof(IBetService), binding, address);


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
            hostBank.AddServiceEndpoint(typeof(IBankService), binding, address);


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
            hostClient.AddServiceEndpoint(typeof(IClientHelper), binding, address);


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


        public static void IntrusionDetection()//mozda dodati lock zbog preplitanja sa logom?
        {
            if (File.Exists("ESB_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt"))//da ne iskace ako fajl ne postoji za novi dan
            {
                //string prev_line;
                string line;
                int first;
                int last;
                string username, address;
                bool fresh = true;


                

                StreamReader file = new StreamReader("ESB_" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt");

                string temp = file.ReadLine();

                //   prev_line = temp.Substring(15, temp.Length - 15);

                //  if (prev_line != null)
                //    {
                
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

                            //if (line.Substring(15, line.Length - 15) == prev_line)
                            //    counter++;
                            //else
                            //    counter = 0;

                            first = line.IndexOf("address: ") + "address: ".Length;
                            last = line.IndexOf(" Port:", first);
                            address = line.Substring(first, last - first);

                            if (!Attempts.ContainsKey(address))
                            {
                                Attempts.Add(address, new IntrusionTry(1, Convert.ToDateTime(date)));
                            }
                            else
                            {
                                //if (DateTime.Now - Convert.ToDateTime(date) < TimeSpan.FromMinutes(3))
                                if (DateTime.Now - Attempts[address].LastTry < TimeSpan.FromMinutes(3))
                                /*ako je unos pogresen u manje od 3 minuta, ako je razmak izmedju greske vise od 3 minuta pokusaji se vracaju na 1*/
                                {
                                    Attempts[address].Attempt += 1;
                                    Attempts[address].LastTry = Convert.ToDateTime(date);
                                }
                                else
                                {
                                    Attempts[address].Attempt = 1;
                                    Attempts[address].LastTry = Convert.ToDateTime(date);
                                }
                            }

                            //prev_line = line.Substring(15, line.Length - 15);

                            List<string> matches;
                            if (Attempts.Values.Any(x => x.Attempt == 3))
                            {

                                matches = Attempts.Where(x => x.Value.Attempt == 3).Select(x => x.Key).ToList();
                                Attempts[address].Attempt = 0;
                                //start = DateTime.Now;
                                first = line.IndexOf("\\") + "\\".Length;
                                last = line.IndexOf(" ", first);
                                username = line.Substring(first, last - first);
                                //first = line.IndexOf("address: ") + "address: ".Length;
                                //last = line.IndexOf(" Port:", first);
                                //address = line.Substring(first, last - first);
                                //first = line.IndexOf("Port: ") + "Port: ".Length;
                                //last = line.IndexOf(" - ", first);
                                //string port = line.Substring(first, last - first);
                                IntrusionPrevention(username/*, Convert.ToInt32(port)*/, matches);
                            }
                        }

                    }
                    //  }
                }
                file.Close();
            }
        }

        private static void IntrusionPrevention(string username/*, int port*/, List<string> addresses)
        {

            foreach (string address in addresses)//bice uvek samo jedna, ali ajde...
            {
                foreach (var item in proxies[address].Values)
                {
                    item.CloseProxy();
                    item.Close();
                }
                proxies.Remove(address);
            }
        }
    }
}