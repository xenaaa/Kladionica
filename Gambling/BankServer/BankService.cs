
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

        public bool CheckIfAlive(int port)
        {
            return true;
        }

        public bool BankLogin(byte[] usernameBytes, byte[] passwordBytes, byte[] addressBytes)
        {
            string username = (string)Helper.Decrypt(usernameBytes);
            string password = (string)Helper.Decrypt(passwordBytes);
            string addressIPv4 = (string)Helper.Decrypt(addressBytes);

            Dictionary<string, User> bankUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("bankUsers.txt");
            if (obj != null)
                bankUsersFromFile = (Dictionary<string, User>)obj;


            if (bankUsersFromFile.Keys.Contains(username))
            {
                if (bankUsersFromFile[username].Password == password)
                {
                    foreach (KeyValuePair<string, User> kvp in bankUsersFromFile)
                    {
                        if (kvp.Key == username)
                        {
                            kvp.Value.Address = addressIPv4;
                            break;
                        }
                    }
                    Persistance.WriteToFile(bankUsersFromFile, "bankUsers.txt");

                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public bool CreateAccount(byte[] userBytes)
        {
            User user = (User)Helper.Decrypt(userBytes);

            Dictionary<string, User> bankUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("bankUsers.txt");
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
                Persistance.WriteToFile(bankUsersFromFile, "bankUsers.txt");
                return true;
            }

        }

        public bool CreateFirstAccounts(User user)
        {
            Dictionary<string, User> bankUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("bankUsers.txt");
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
                Persistance.WriteToFile(bankUsersFromFile, "bankUsers.txt");
                return true;
            }

        }

        public bool Deposit(byte[] accBytes, byte[] usernameBytes)
        {
            Account acc = (Account)Helper.Decrypt(accBytes);
            string username = (string)Helper.Decrypt(usernameBytes);


            Dictionary<string, User> bankUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("bankUsers.txt");
            if (obj != null)
                bankUsersFromFile = (Dictionary<string, User>)obj;


            if (acc.Number == 0)
            {
                if (bankUsersFromFile[username].BankAccount.Amount - acc.Amount < 0)
                {
                    return false;
                }
                else
                {
                    bankUsersFromFile[username].BankAccount.Amount -= acc.Amount; // povecavamo drugi
                    bankUsersFromFile[username].BetAccount.Amount += acc.Amount; // smanjujemo onaj s kog prebacujemo
                    Persistance.WriteToFile(bankUsersFromFile, "bankUsers.txt");

                    string srvCertCN = "bankserviceintegration";
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Certificate;

                    X509Certificate2 srvCert = CertManager.GetCertificateFromStorage(StoreName.TrustedPeople, StoreLocation.LocalMachine, srvCertCN);
                    EndpointAddress address = new EndpointAddress(new Uri("net.tcp://" + Helper.integrationHostAddress + ":" + Helper.integrationHostPort + "/BetIntegrationPlatform2"),
                                              new X509CertificateEndpointIdentity(srvCert));

                    BankServerProxy proxy;
                    byte[] encryptedAccount = Helper.Encrypt(acc);
                    byte[] encryptedUername = Helper.Encrypt(username);

                    proxy = new BankServerProxy(binding, address);
                    proxy.Deposit(encryptedAccount, encryptedUername);
                    return true;
                }
            }
            else
            {
                KeyValuePair<string, User> user = bankUsersFromFile.Where(u => u.Value.BankAccount.Number == acc.Number).FirstOrDefault();

                if (user.Key == null)
                {
                    return false;
                }
                else
                {
                    if (bankUsersFromFile[username].BankAccount.Amount - acc.Amount < 0)
                    {
                        return false;
                    }
                    else
                    {
                        bankUsersFromFile[user.Value.Username].BankAccount.Amount += acc.Amount; // povecavamo drugi
                        bankUsersFromFile[username].BankAccount.Amount = bankUsersFromFile[username].BankAccount.Amount - acc.Amount; // smanjujemo onaj s kog prebacujemo
                        Persistance.WriteToFile(bankUsersFromFile, "bankUsers.txt");
                        return true;
                    }
                }
            }
        }
    }
}