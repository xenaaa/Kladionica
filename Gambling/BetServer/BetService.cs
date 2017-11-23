using Contracts;
using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;


namespace BetServer
{


    public class BetService : IBetService
    {
        private static object portLock = new object();

        public static object PortLock
        {
            get { return portLock; }
            set { portLock = value; }
        }

        public BetService()
        { }

        public bool CheckIfAlive()
        {
            return true;
        }

        public bool SendPort(byte[] usernameBytes, byte[] portBytes, byte[] addressBytes)
        {
            lock (PortLock)
            {
                Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
                Object obj = Persistance.ReadFromFile("betUsers");
                if (obj != null)
                    betUsersFromFile = (Dictionary<string, User>)obj;

                string username = (string)Helper.Decrypt(usernameBytes);
                int port = (int)Helper.Decrypt(portBytes);
                string address = (string)Helper.Decrypt(addressBytes);

                betUsersFromFile[username].Port = port;
                betUsersFromFile[username].Address = address;

                Persistance.WriteToFile(betUsersFromFile, "betUsers");

                List<int> portsFromFile = new List<int>();
                obj = Persistance.ReadFromFile("ports");
                if (obj != null)
                    portsFromFile = (List<int>)obj;

                portsFromFile.Add(port);

                Persistance.WriteToFile(portsFromFile, "ports");
            }
            return true;
        }



        public bool BetLogin(byte[] usernameBytes, byte[] passwordBytes, byte[] portBytes)//da li dopustiti istom User-u da se loguje na vise klijenata?
        {

            string username = (string)Helper.Decrypt(usernameBytes);
            string password = (string)Helper.Decrypt(passwordBytes);
            int port = (int)Helper.Decrypt(portBytes);

            WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;

            Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("betUsers");
            if (obj != null)
                betUsersFromFile = (Dictionary<string, User>)obj;

            if (identity.Name == username)
            {
                if (betUsersFromFile.Keys.Contains(username))
                {
                    if (betUsersFromFile[username].Password == password)
                    {
                        Console.WriteLine("You successfully logged in!");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Your password is incorrect!");
                        return false;
                    }
                }
            }
            return false;
        }


        public bool AddUser(byte[] userBytes)
        {
            //dekpricija
            User user = (User)Helper.Decrypt(userBytes);

            WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
            Console.WriteLine("User {0} je pozvao AddUser\n", identity.Name);

            Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("betUsers");
            if (obj != null)
                betUsersFromFile = (Dictionary<string, User>)obj;

            if (!betUsersFromFile.ContainsKey(identity.Name))
            {
                if (!betUsersFromFile.ContainsKey(user.Username))
                {

                    lock (PortLock)
                    {
                        betUsersFromFile.Add(user.Username, user);
                        Persistance.WriteToFile(betUsersFromFile, "betUsers");
                    }

                    Console.WriteLine("User {0} successfully added to BetUsers", user.Username);
                    return true;
                }
                else
                {

                    Console.WriteLine("User {0} already exists.", user.Username);
                    return false;
                }
            }
            Console.WriteLine("User {0} already exists", identity.Name);
            return false;
        }

        public bool DeleteUser(byte[] usernameBytes)
        {
            string username = (string)Helper.Decrypt(usernameBytes);

            Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("betUsers");
            if (obj != null)
                betUsersFromFile = (Dictionary<string, User>)obj;

            if (!betUsersFromFile.ContainsKey(username))
            {
                Console.WriteLine("Error! There is no user {0} in BetService", username);
                return false;
            }
            else
            {
                betUsersFromFile.Remove(username);
                Persistance.WriteToFile(betUsersFromFile, "users");
                Console.WriteLine("User {0} removed from BetService", username);
                return true;
            }
        }

        public bool EditUser(byte[] userBytes)
        {
            User user = (User)Helper.Decrypt(userBytes);

            Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("betUsers");
            if (obj != null)
                betUsersFromFile = (Dictionary<string, User>)obj;

            if (!betUsersFromFile.ContainsKey(user.Username))
            {
                Console.WriteLine("Error! There is no user {0} in BetService", user.Username);
                return false;
            }
            else
            {
                foreach (KeyValuePair<string, User> kvp in betUsersFromFile)
                {
                    if (kvp.Key == user.Username)
                    {
                        kvp.Value.BetAccount = user.BetAccount;
                        kvp.Value.Tickets = user.Tickets;
                    }
                }
                Persistance.WriteToFile(betUsersFromFile, "betUsers");
                return true;
            }
        }


        public bool SendTicket(byte[] ticketBytes, byte[] usernameBytes)
        {
            Ticket ticket = (Ticket)Helper.Decrypt(ticketBytes);
            string username = (string)Helper.Decrypt(usernameBytes);

            Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("betUsers");
            if (obj != null)
                betUsersFromFile = (Dictionary<string, User>)obj;

            if (betUsersFromFile.ContainsKey(username))
            {
                double a = betUsersFromFile[username].BetAccount.Amount;
                if (betUsersFromFile[username].BetAccount.Amount >= ticket.Payment)
                {
                    betUsersFromFile[username].BetAccount.Amount -= ticket.Payment;
                    betUsersFromFile[username].Tickets.Add(ticket);
                    Persistance.WriteToFile(betUsersFromFile, "betUsers");
                    return true;
                }
                else
                    return false;
                   
            
            }
            else
                return false;
        }

        public bool Deposit(byte[] accBytes, byte[] usernameBytes)
        {
            string username = (string)Helper.Decrypt(usernameBytes);
            Account acc = (Account)Helper.Decrypt(accBytes);

            Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("betUsers");
            if (obj != null)
                betUsersFromFile = (Dictionary<string, User>)obj;

            if (betUsersFromFile.ContainsKey(username))
            {
                User user = betUsersFromFile[username];
                user.BetAccount.Amount += acc.Amount;

                EditUser(Helper.Encrypt(user));
            }
            return true;
        }

        public bool GetServiceIP(byte[] AddressStringBytes)
        {
            throw new NotImplementedException();
        }
    }
}



