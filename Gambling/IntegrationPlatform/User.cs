using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IntegrationPlatform
{
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

        public User()
        { }

        public User(string un, string pass, string r)
        {
            username = un;
            password = pass;
            role = r;
            bankAccount = new Account();
            betAccount = new Account();
            tickets = new List<Ticket>();
        }

        public User(string un, string pass, string r, Account bankAcc, Account betAcc, List<Ticket> ticks)
        {
            username = un;
            password = pass;
            role = r;
            bankAccount = bankAcc;
            betAccount = betAcc;
            tickets = ticks;
        }
    }
}
