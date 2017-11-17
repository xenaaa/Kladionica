using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankServer
{
    public class BankService : IBankService
    {
        private static Dictionary<string, User> BankUsers = new Dictionary<string, User>();
        private static int accNumb = 11;

        public BankService()
        {

        }

        public bool CheckIfAlive()
        {
            return true;
        }
        public bool Login(string username, string password)
        {
            if (BankUsers.Keys.Contains(username))
            {
                if (BankUsers[username].Password == password)
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
                if (CreateAccount(user))
                    return true;
                else
                    return false;
            }
        }


        private bool CreateAccount(User user)
        {

            if (BankUsers.Equals(user.Username))
                return false;
            else
            {
                Account bankAcc = new Account(15, accNumb);
                accNumb++;
                Account betAcc = new Account(0, accNumb);
                accNumb++;
                User user1 = new User(user.Username, user.Password, user.Role, bankAcc, betAcc);
                BankUsers.Add(user1.Username, user1);
                Console.Write("Korisnik {0}", user1.Username);
                return true;
            }

        }

        public bool Deposit(Account acc, string username)
        {
            KeyValuePair<string, User> user = BankUsers.Where(u => u.Value.BankAccount.Number == acc.Number).FirstOrDefault();

            if (user.Key == null)
            {
                Console.WriteLine("Account number doesn't exist\n");
                return false;
            }
            else
            {
                if (BankUsers[username].BankAccount.Amount - acc.Amount < 0)
                {
                    Console.WriteLine("There is not enough amount on your bank account");
                    return false;
                }
                else
                {
                    BankUsers[user.Value.Username].BankAccount.Amount += acc.Amount; // povecavamo drugi
                    BankUsers[username].BankAccount.Amount = BankUsers[username].BankAccount.Amount - acc.Amount; // smanjujemo onaj s kog prebacujemo
                    return true;
                }
            }
        }
    }
}
