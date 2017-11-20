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

        public bool SendPort(int port)
        {
            lock (PortLock)
            {
                ports.Add(port);
            }
            return true;
        }

        public bool BetLogin(string username, string password, int port)//da li dopustiti istom User-u da se loguje na vise klijenata?
        {
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
                else
                {

                    User user = new User(username, password, "User");
                    user.Port = port;
                    if (AddUser(user))
                        return true;
                    else
                        return false;
                }
            }
            return false;
        }


        public bool AddUser(User user)
        {
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

        public bool DeleteUser(User user)
        {
            WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
            Console.WriteLine("User {0} je pozvao DeleteUser\n", identity.Name);
            if (BetUsers.ContainsKey(identity.Name))
            {
                if (!BetUsers.ContainsKey(user.Username))
                {
                    Console.WriteLine("Error! There is no user {0} in BetService", user.Username);
                    return false;
                }
                else
                {
                    BetUsers.Remove(user.Username);
                    Console.WriteLine("User {0} removed from BetService", user.Username);
                    return true;
                }
            }
            else
            {
                Console.WriteLine("User {0} doesn't exist", identity.Name);
                return false;
            }
        }

        public bool EditUser(User user)
        {
            WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
            Console.WriteLine("User {0} je pozvao EditUser\n", identity.Name);
            if (BetUsers.ContainsKey(identity.Name))
            {
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
                            kvp.Value.Role = user.Role;
                            kvp.Value.Password = user.Password;
                        }
                    }
                    return true;
                }
            }
            else
            {
                Console.WriteLine("User {0} doesn't exist", identity.Name);
                return false;
            }
        }


        public bool SendTicket(Ticket ticket, string username)
        {
            WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
            Console.WriteLine("User {0} je pozvao SendTicket\n", identity.Name);
            if (BetUsers.ContainsKey(identity.Name))
            {
                if (BetUsers.ContainsKey(username))
                {
                    BetUsers[username].Tickets.Add(ticket);
                    return true;
                }
                else
                    return false;

            }
            else
            {
                Console.WriteLine("User {0} doesn't exist", identity.Name);
                return false;
            }
        }
    }
}


