using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace IntergrationPlatform
{
    public class BankService : IBankService
    {
        BankServiceProxy proxy;

        public BankService()
        {
            NetTcpBinding binding = new NetTcpBinding();
            string address = "net.tcp://localhost:9999/BankService";
            proxy = new BankServiceProxy(binding, address);
        }

        public bool BankLogin(string username, string password, int port)
        {
            return proxy.BankLogin(username, password, port);
        }

        public bool CheckIfAlive()
        {
            return proxy.CheckIfAlive();
        }

        public bool Deposit(Account acc)
        {
            return proxy.Deposit(acc);
        }
    }
}
