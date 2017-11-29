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

        public bool CheckIfAlive(int port)
        {
            return true;
        }

        public bool SendPort(byte[] usernameBytes, byte[] portBytes, byte[] addressBytes, byte[] printPortBytes)
        {
            lock (PortLock)
            {
                Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
                Object obj = Persistance.ReadFromFile("betUsers.txt");
                if (obj != null)
                    betUsersFromFile = (Dictionary<string, User>)obj;

                string username = (string)Helper.Decrypt(usernameBytes);
                int port = (int)Helper.Decrypt(portBytes);
                string address = (string)Helper.Decrypt(addressBytes);
                int printPort = (int)Helper.Decrypt(printPortBytes);

                betUsersFromFile[username].Port = port;
                betUsersFromFile[username].Address = address;
                betUsersFromFile[username].PrintPort = printPort;

                Persistance.WriteToFile(betUsersFromFile, "betUsers.txt");
            }
            return true;
        }



        public bool BetLogin(byte[] usernameBytes, byte[] passwordBytes)
        {

            string username = (string)Helper.Decrypt(usernameBytes);
            string password = (string)Helper.Decrypt(passwordBytes);

            Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("betUsers.txt");
            if (obj != null)
                betUsersFromFile = (Dictionary<string, User>)obj;

            if (betUsersFromFile.Keys.Contains(username))
            {
                if (betUsersFromFile[username].Password == password)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }


        public bool AddUser(byte[] userBytes)
        {
            //dekpricija
            User user = (User)Helper.Decrypt(userBytes);


            Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("betUsers.txt");
            if (obj != null)
                betUsersFromFile = (Dictionary<string, User>)obj;


            if (!betUsersFromFile.ContainsKey(user.Username))
            {
                lock (PortLock)
                {
                    betUsersFromFile.Add(user.Username, user);
                    Persistance.WriteToFile(betUsersFromFile, "betUsers.txt");
                }
                return true;
            }
            else
            {
                return false;
            }


        }

        public bool DeleteUser(byte[] usernameBytes)
        {
            string username = (string)Helper.Decrypt(usernameBytes);

            Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("betUsers.txt");
            if (obj != null)
                betUsersFromFile = (Dictionary<string, User>)obj;

            if (!betUsersFromFile.ContainsKey(username))
            {
                return false;
            }
            else
            {
                betUsersFromFile.Remove(username);
                Persistance.WriteToFile(betUsersFromFile, "betUsers.txt");
                return true;
            }
        }

        public bool EditUser(byte[] userBytes)
        {
            User user = (User)Helper.Decrypt(userBytes);

            Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("betUsers.txt");
            if (obj != null)
                betUsersFromFile = (Dictionary<string, User>)obj;

            if (!betUsersFromFile.ContainsKey(user.Username))
            {
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
                Persistance.WriteToFile(betUsersFromFile, "betUsers.txt");
                return true;
            }
        }


        public bool SendTicket(byte[] ticketBytes, byte[] usernameBytes)
        {
            Ticket ticket = (Ticket)Helper.Decrypt(ticketBytes);
            string username = (string)Helper.Decrypt(usernameBytes);

            Dictionary<string, User> betUsersFromFile = new Dictionary<string, User>();
            Object obj = Persistance.ReadFromFile("betUsers.txt");
            if (obj != null)
                betUsersFromFile = (Dictionary<string, User>)obj;

            if (betUsersFromFile.ContainsKey(username))
            {
                double a = betUsersFromFile[username].BetAccount.Amount;
                if (betUsersFromFile[username].BetAccount.Amount >= ticket.Payment)
                {
                    betUsersFromFile[username].BetAccount.Amount -= ticket.Payment;
                    betUsersFromFile[username].Tickets.Add(ticket);
                    Persistance.WriteToFile(betUsersFromFile, "betUsers.txt");
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

            Object obj = Persistance.ReadFromFile("betUsers.txt");


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
    }
}