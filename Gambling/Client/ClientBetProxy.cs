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
    public class ClientBetProxy : ChannelFactory<IClientBet>, IClientBet, IDisposable
    {
        IClientBet factory;

        public ClientBetProxy(NetTcpBinding binding, string address) : base(binding, address)
        {
            try
            {
                factory = this.CreateChannel();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        public bool CheckIfAlive(int port)
        {
            try
            {
                return factory.CheckIfAlive(port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendPort(byte[] username, byte[] port, byte[] address, byte[] printPort)
        {
            try
            {
                return factory.SendPort(username, port, address, printPort);
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

        public bool AddUser(byte[] user, byte[] port)
        {
            try
            {
                return factory.AddUser(user,port);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool DeleteUser(byte[] username, byte[] port)
        {
            try
            {
                return factory.DeleteUser(username,port);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool EditUser(byte[] user, byte[] port)
        {
            try
            {
                return factory.EditUser(user,port);

            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return false;
            }
        }

        public bool SendTicket(byte[] ticket, byte[] username, byte[] port)
        {
            try
            {
                return factory.SendTicket(ticket, username, port);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while trying to SendTicket(). {0}", e.Message);
                return false;
            }

        }

        public List<Dictionary<string, int>> Report()
        {
            try
            {
                return factory.Report();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error {0}", e.Message);
                return null;
            }
        }
    }
}