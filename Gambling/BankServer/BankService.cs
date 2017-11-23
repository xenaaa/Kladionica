
using CertificateManager;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BankServer
{
    public class BankService : IBankService
    {
        private static int accNumb = 11;

        public BankService()
        {

        }

        public bool CheckIfAlive()
        {
            return true;
        }

        public bool BankLogin(byte[] usernameBytes, byte[] passwordBytes, byte[] portBytes, byte[] addressBytes)
        {
            string username = (string)Helper.Decrypt(usernameBytes);
            string password = (string)Helper.Decrypt(passwordBytes);
            int port = (int)Helper.Decrypt(portBytes);
            string addressIPv4 = (string)Helper.Decrypt(addressBytes);

            Dictionary<string, User> bankUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("bankUsers");
            if (obj != null)
                bankUsersFromFile = (Dictionary<string, User>)obj;


            if (bankUsersFromFile.Keys.Contains(username))
            {
                if (bankUsersFromFile[username].Password == password)//OKK?****
                {
                    foreach (KeyValuePair<string, User> kvp in bankUsersFromFile)
                    {
                        if (kvp.Key == username)
                        {
                            kvp.Value.Address = addressIPv4;//valjda je ovo ok?
                            break;
                        }
                    }
                    Persistance.WriteToFile(bankUsersFromFile, "bankUsers");//*****

                    Console.WriteLine("You successfully logged in!");
                    return true;
                }
                else
                {
                    Console.WriteLine("Your password is incorrect!");
                    return false;
                }
            }
            return false;
        }


        public bool CreateAccount(byte[] userBytes)
        {
            User user = (User)Helper.Decrypt(userBytes);

            Dictionary<string, User> bankUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("bankUsers");
            if (obj != null)
                bankUsersFromFile = (Dictionary<string, User>)obj;

            if (bankUsersFromFile.Keys.Contains(user.Username))
                return false;
            else
            {
                Account bankAcc = new Account(15, accNumb);
                accNumb++;
                Account betAcc = new Account(0, accNumb);
                accNumb++;
                User user1 = new User(user.Username, user.Password, user.Role, bankAcc, betAcc);
                bankUsersFromFile.Add(user1.Username, user1);
                Persistance.WriteToFile(bankUsersFromFile, "bankUsers");
                Console.Write("Korisnik {0}", user1.Username);
                return true;
            }

        }

        public bool Deposit(byte[] accBytes, byte[] usernameBytes)
        {
            Account acc = (Account)Helper.Decrypt(accBytes);
            string username = (string)Helper.Decrypt(usernameBytes);

            Dictionary<string, User> bankUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("bankUsers");
            if (obj != null)
                bankUsersFromFile = (Dictionary<string, User>)obj;


            if (acc.Number == 0)
            {
                if (bankUsersFromFile[username].BankAccount.Amount - acc.Amount < 0)
                {
                    Console.WriteLine("There is not enough amount on your bank account");
                    return false;
                }
                else
                {
                    bankUsersFromFile[username].BankAccount.Amount -= acc.Amount; // povecavamo drugi
                    bankUsersFromFile[username].BetAccount.Amount += acc.Amount; // smanjujemo onaj s kog prebacujemo
                    Persistance.WriteToFile(bankUsersFromFile, "bankUsers");

                    string srvCertCN = "bankserviceintegration";
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

                    X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, srvCertCN);
                    EndpointAddress address = new EndpointAddress(new Uri("net.tcp://localhost:" + Helper.integrationHostPort + "/BetIntegrationPlatform2"),
                                              new X509CertificateEndpointIdentity(srvCert));

                    BankServerProxy proxy;
                    proxy = new BankServerProxy(binding, address);

                    byte[] encryptedAccount = Helper.Encrypt(acc);
                    byte[] encryptedUername = Helper.Encrypt(username);

                    proxy.Deposit(encryptedAccount, encryptedUername);
                    return true;
                }
            }
            else
            {
                KeyValuePair<string, User> user = bankUsersFromFile.Where(u => u.Value.BankAccount.Number == acc.Number).FirstOrDefault();

                if (user.Key == null)
                {
                    Console.WriteLine("Account number doesn't exist\n");
                    return false;
                }
                else
                {
                    if (bankUsersFromFile[username].BankAccount.Amount - acc.Amount < 0)
                    {
                        Console.WriteLine("There is not enough amount on your bank account");
                        return false;
                    }
                    else
                    {
                        bankUsersFromFile[user.Value.Username].BankAccount.Amount += acc.Amount; // povecavamo drugi
                        bankUsersFromFile[username].BankAccount.Amount = bankUsersFromFile[username].BankAccount.Amount - acc.Amount; // smanjujemo onaj s kog prebacujemo
                        Persistance.WriteToFile(bankUsersFromFile, "bankUsers");
                        return true;
                    }
                }
            }
        }

        public bool IntrusionPrevention(byte[] user)
        {
            string username = (string)Helper.Decrypt(user);

            Dictionary<string, User> bankUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("bankUsers");
            if (obj != null)
                bankUsersFromFile = (Dictionary<string, User>)obj;

            if (!bankUsersFromFile.ContainsKey(username))
            {
                return false;
            }
            else
            {
                DeleteUser(user);
                return true;
            }
        }

        private bool DeleteUser(byte[] usernameBytes)
        {
            string username = (string)Helper.Decrypt(usernameBytes);

            Dictionary<string, User> bankUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("bankUsers");
            if (obj != null)
                bankUsersFromFile = (Dictionary<string, User>)obj;

            if (!bankUsersFromFile.ContainsKey(username))
            {
                Console.WriteLine("Error! There is no user {0} in BetService", username);
                return false;
            }
            else
            {
                bankUsersFromFile.Remove(username);
                Persistance.WriteToFile(bankUsersFromFile, "bankUsers");
                Console.WriteLine("User {0} removed from BetService", username);
                return true;
            }
        }

    }
}