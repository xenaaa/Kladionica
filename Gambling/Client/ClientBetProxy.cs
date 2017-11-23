using BetServer;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class ClientBetProxy : ChannelFactory<IBetService>, IBetService, IDisposable
    {
        IBetService factory;

        public ClientBetProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            factory = this.CreateChannel();
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

        public bool SendPort(byte[] username, byte[] port, byte[] address)
        {
            try
            {
                factory.SendPort(username, port, address);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool BetLogin(byte[] username, byte[] password, byte[] port)
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

        public bool AddUser(byte[] user)
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

        public bool DeleteUser(byte[] username)
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

        public bool EditUser(byte[] user)
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

        public bool SendTicket(byte[] ticket, byte[] username)
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

        public bool Deposit(byte[] acc, byte[] username)
        {
            throw new NotImplementedException();
        }
    }
}
