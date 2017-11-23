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
                return factory.CheckIfAlive();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendPort(byte[] username, byte[] port, byte[] address,byte[] printPort)
        {
            try
            {
                return factory.SendPort(username, port, address,printPort);
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
               return factory.BetLogin(username, password, port);

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
                return factory.AddUser(user);
             
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
                return factory.DeleteUser(username);
          
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
                return factory.EditUser(user);
           
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendTicket(byte[] ticket, byte[] username)
        {
            try
            {
                return factory.SendTicket(ticket, username);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to SendTicket(). {0}", e.Message);
                return false;
            }

        }

        public bool Deposit(byte[] acc, byte[] username)
        {
            throw new NotImplementedException();
        }

        public bool GetServiceIP(byte[] AddressStringBytes)
        {
            throw new NotImplementedException();
        }

        public bool IntrusionPrevention(byte[] user)
        {
            throw new NotImplementedException();
        }
    }
}
