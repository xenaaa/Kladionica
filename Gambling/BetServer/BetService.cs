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
      //  private static Dictionary<string, User> BetUsers = new Dictionary<string, User>();

        private static Dictionary<string,User> betUsers=new Dictionary<string, User>();

        public static Dictionary<string,User> BetUsers
        {
            get { return betUsers; }
            set { betUsers = value; }
        }

        private static Dictionary<int, Dictionary<int, double>> rezultati=new Dictionary<int, Dictionary<int, double>>();

        public static Dictionary<int, Dictionary<int, double>> Rezultati
        {
            get { return rezultati; }
            set { rezultati = value; }
        }


     


        public BetService()
        {


        }


        public bool CheckIfAlive()
        {
            return true;
        }

        public bool SendPort(int port)
        {
            ports.Add(port);
            return true;
        }

        public bool Login(string username, string password)
        {
            if (BetUsers.Keys.Contains(username))
            {
                if (BetUsers[username].Password == password)
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
            else
            {

                User user = new User(username, password, "User");
                if (AddUser(user))
                    return true;
                else
                    return false;
            }
        }


        public bool AddUser(User user)
        {
            WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
            Console.WriteLine("User {0} je pozvao AddUser\n", identity.Name);
            if (BetUsers.ContainsKey(identity.Name) )
            {
                if (!BetUsers.ContainsKey(user.Username))
                {
                    BetUsers.Add(user.Username, user);
                    Console.WriteLine("User {0} successfully added to BetUsers", user.Username);
                    return true;
                }
                else
                {

                    Console.WriteLine("User {0} already exists.", user.Username);
                    return false;
                }
            }
            else
            {
                Console.WriteLine("User {0} doesn't exist", identity.Name);
                return false;
            }
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
        { WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
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


        public void SendOffers()
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "";

            List<BetOffer> offers = new List<BetOffer>();

            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object

            while (true)
            {
                xmlDoc.Load("lista.xml"); // Load the XML document from the specified file

                // Get elements
                XmlNodeList id = xmlDoc.GetElementsByTagName("ID");
                XmlNodeList home = xmlDoc.GetElementsByTagName("DOMACIN");
                XmlNodeList away = xmlDoc.GetElementsByTagName("GOST");
                XmlNodeList kec = xmlDoc.GetElementsByTagName("KEC");
                XmlNodeList iks = xmlDoc.GetElementsByTagName("IKS");
                XmlNodeList dvojka = xmlDoc.GetElementsByTagName("DVOJKA");

                Dictionary<int, double> odds = new Dictionary<int, double>();
                odds.Add(1, Convert.ToDouble(kec[0].InnerText));
                odds.Add(0, Convert.ToDouble(iks[0].InnerText));
                odds.Add(2, Convert.ToDouble(dvojka[0].InnerText));

                for (int i = 0; i < id.Count; i++)
                {
                    BetOffer bo = new BetOffer(home[i].InnerText, away[i].InnerText, Convert.ToInt32(id[i].InnerText), odds);
                    offers.Add(bo);
                }

                foreach (var port in ports)
                {
                    address = "net.tcp://localhost:" + port + "/ClientHelper";
                    BetServerProxy proxy = new BetServerProxy(binding, address);
                    {
                        if (proxy.CheckIfAlive())
                            proxy.SendOffers(offers);


                    }
                }
                Thread.Sleep(4000);
            }
        }
    }
}


