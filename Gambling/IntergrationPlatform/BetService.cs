using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
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
            return proxy.AddUser(user);
           
        }

        public bool BetLogin(string username, string password, int port)
        {
            return proxy.BetLogin(username, password, port);
        }

        public bool CheckIfAlive()
        {
           return proxy.CheckIfAlive();         
        }

        public bool DeleteUser(User user)
        {
            return proxy.DeleteUser(user);
        }

        public bool EditUser(User user)
        {
            return proxy.EditUser(user);
        }

        public bool SendPort(int port)
        {
            return proxy.SendPort(port);
        }

        public bool SendTicket(Ticket ticket, string username)
        {
            return proxy.SendTicket(ticket,username);
        }
    }
}
