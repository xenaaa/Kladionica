using Contracts;
using SecurityManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IntergrationPlatform
{
    public class BetService : IBetService
    {
        BetServiceProxy proxy;

        public BetService()
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9998/BetService";
            proxy = new BetServiceProxy(binding, address);
        }

        public bool AddUser(User user)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BetAdmin"))
            {
                proxy.AddUser(user);
                allowed = true;
            }
            else
                Console.WriteLine("AddUser() failed for user {0}.", principal.Identity.Name);
            return allowed;
        }

        public bool BetLogin(string username, string password, int port)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User") || principal.IsInRole("Reader") || principal.IsInRole("BetAdmin"))
            {
                proxy.BetLogin(username, password, port);
                allowed = true;
            }

            else
                Console.WriteLine("BetLogin() failed for user {0}.", principal.Identity.Name);
            return allowed;
        }

        public bool CheckIfAlive()
        {
            return proxy.CheckIfAlive();
        }

        public bool DeleteUser(string username)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BetAdmin"))
            {
                proxy.DeleteUser(username);
                allowed = true;
            }
            else
                Console.WriteLine("DeleteUser() failed for user {0}.", principal.Identity.Name);
            return allowed;
        }

        public bool EditUser(User user)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("BetAdmin"))
            {
                proxy.EditUser(user);
                allowed = true;
            }
            else
                Console.WriteLine("EditUser() failed for user {0}.", principal.Identity.Name);
            return allowed;
        }

        public bool SendPort(string username, int port)
        {
            return proxy.SendPort(username, port);
        }

        public bool SendTicket(Ticket ticket, string username)
        {
            bool allowed = false;

            CustomPrincipal principal = Thread.CurrentPrincipal as CustomPrincipal;
            if (principal.IsInRole("User"))
            {
                proxy.SendTicket(ticket, username);
                allowed = true;
            }
            else
                Console.WriteLine("SendTicket() failed for user {0}.", principal.Identity.Name);
            return allowed;
        }
    }
}
