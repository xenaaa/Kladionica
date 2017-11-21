using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IntergrationPlatform
{
    public class BetServiceProxy : ChannelFactory<IBetService>, IBetService, IDisposable
    {
        IBetService factory;

        public BetServiceProxy() { }
        public BetServiceProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            factory = this.CreateChannel();
        }

        public bool AddUser(User user)
        {
            try
            {
                factory.AddUser(user);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }


        public bool DeleteUser(string username)
        {
            try
            {
                factory.DeleteUser(username);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool EditUser(User user)
        {
            try
            {
                factory.EditUser(user);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendTicket(Ticket ticket, string username)
        {
            bool sent = false;
            try
            {
                sent = factory.SendTicket(ticket, username);
                Console.WriteLine("SendTicket() >> {0}", sent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to SendTicket(). {0}", e.Message);
            }

            return sent;
        }

        public bool BetLogin(string username, string password, int port)
        {
            try
            {
                factory.BetLogin(username, password, port);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool CheckIfAlive()
        {

            try
            {
                factory.CheckIfAlive();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }



        public bool SendPort(string username, int port)
        {
            try
            {
                factory.SendPort(username, port);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }
    }
}