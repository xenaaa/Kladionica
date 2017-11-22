using Contracts;
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

        private static Dictionary<string, User> betUsers = new Dictionary<string, User>();
        private static Dictionary<int, Game> rezultati = new Dictionary<int, Game>();
        private static List<int> ports = new List<int>();

        private static object portLock = new object();

        public static object PortLock
        {
            get { return portLock; }
            set { portLock = value; }
        }


        public static Dictionary<string, User> BetUsers
        {
            get { return betUsers; }
            set { betUsers = value; }
        }

        public static Dictionary<int, Game> Rezultati // sifra utakmica i rezultat
        {
            get { return rezultati; }
            set { rezultati = value; }
        }

        public List<int> Ports
        {
            get
            {
                return ports;
            }
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
                string username = (string)Helper.Decrypt(usernameBytes);
                int port = (int)Helper.Decrypt(portBytes);
                string address = (string)Helper.Decrypt(addressBytes);

                BetUsers[username].Port = port;
                BetUsers[username].Address = address;

                ports.Add(port);
            }
            return true;
        }



        public bool BetLogin(byte[] usernameBytes, byte[] passwordBytes, byte[] portBytes)//da li dopustiti istom User-u da se loguje na vise klijenata?
        {

            string username = (string)Helper.Decrypt(usernameBytes);
            string password = (string)Helper.Decrypt(passwordBytes);
            int port = (int)Helper.Decrypt(portBytes);

            WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
            if (identity.Name == username)
            {
                if (BetUsers.Keys.Contains(username))
                {
                    if (BetUsers[username].Password == password)
                    {
                        //ako je dozvoljeno logovanje sa vise klijenata treba umesto jednog porta implementirati listu portova i svaki put ovde dodati novi port
                        //takodje kad vec postoji i log in dodati i log-out?
                        Console.WriteLine("You successfully logged in!");
                        return true;
                    }
                    else
                    {
                        Console.WriteLine("Your password is incorrect!");
                        return false;
                    }
                }
                //else
                //{

                //    User user = new User(username, password, "User");
                //    user.Port = port;
                //    if (AddUser(user))
                //        return true;
                //    else
                //        return false;
                //}
            }
            return false;
        }


        public bool AddUser(byte[] userBytes)
        {
            //dekpricija
            User user = (User)Helper.Decrypt(userBytes);

            WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
            Console.WriteLine("User {0} je pozvao AddUser\n", identity.Name);
            if (!BetUsers.ContainsKey(identity.Name))
            {
                if (!BetUsers.ContainsKey(user.Username))
                {
                    lock (PortLock)
                    {
                        BetUsers.Add(user.Username, user);
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
            //WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
            //Console.WriteLine("User {0} je pozvao DeleteUser\n", identity.Name);
            //if (BetUsers.ContainsKey(identity.Name))
            //{
            if (!BetUsers.ContainsKey(username))
            {
                Console.WriteLine("Error! There is no user {0} in BetService", username);
                return false;
            }
            else
            {
                BetUsers.Remove(username);
                Console.WriteLine("User {0} removed from BetService", username);
                return true;
            }
            //  }
            //else
            //{
            //    Console.WriteLine("User {0} doesn't exist", identity.Name);
            //    return false;
            //}
        }

        public bool EditUser(byte[] userBytes)
        {
            User user = (User)Helper.ByteArrayToObject(userBytes);

            if (!BetUsers.ContainsKey(user.Username))
            {
                Console.WriteLine("Error! There is no user {0} in BetService", user.Username);
                return false;
            }
            else
            {
                foreach (KeyValuePair<string, User> kvp in BetUsers)
                {
                    if (kvp.Key == user.Username)
                    {
                        kvp.Value.BetAccount = user.BetAccount;                    
                    }
                }
                return true;
            }
        }


        public bool SendTicket(byte[] ticketBytes, byte[] usernameBytes)
        {
            Ticket ticket = (Ticket)Helper.Decrypt(ticketBytes);
            string username = (string)Helper.Decrypt(usernameBytes);

            if (BetUsers.ContainsKey(username))
            {
                BetUsers[username].Tickets.Add(ticket);
                return true;
            }
            else
                return false;
        }
    }
}


