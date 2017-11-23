using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    [Serializable]
    [DataContract]
    public class User
    {
        [DataMember]
        string username;
        string password;
        string role;
        Account bankAccount;
        Account betAccount;
        List<Ticket> tickets;
        int port;
        int printPort;
        string address;


        [DataMember]
        public string Username
        {
            get
            {
                return username;
            }

            set
            {
                username = value;
            }
        }
        [DataMember]
        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                password = value;
            }
        }
        [DataMember]
        public string Role
        {
            get
            {
                return role;
            }

            set
            {
                role = value;
            }
        }
        [DataMember]
        public Account BankAccount
        {
            get
            {
                return bankAccount;
            }

            set
            {
                bankAccount = value;
            }
        }
        [DataMember]
        public Account BetAccount
        {
            get
            {
                return betAccount;
            }

            set
            {
                betAccount = value;
            }
        }

        [DataMember]
        public List<Ticket> Tickets
        {
            get
            {
                return tickets;
            }
            set
            {
                tickets = value;
            }
        }

        [DataMember]
        public int Port
        {
            get
            {
                return port;
            }

            set
            {
                port = value;
            }
        }

        [DataMember]
        public string Address
        {
            get
            {
                return address;
            }

            set
            {
                address = value;
            }
        }
        [DataMember]
        public int PrintPort
        {
            get
            {
                return printPort;
            }

            set
            {
                printPort = value;
            }
        }

        public User()
        { }

        public User(string un, string pass, string r)
        {
            username = un;
            password = pass;
            role = r;
            bankAccount = new Account();
            Random rnd = new Random();
            betAccount = new Account();
            tickets = new List<Ticket>();
        }

        public User(string un, string pass, string r, Account bankAcc, Account betAcc)
        {
            username = un;
            password = pass;
            role = r;
            bankAccount = bankAcc;
            betAccount = betAcc;
            tickets = new List<Ticket>();
        }



    }
}
