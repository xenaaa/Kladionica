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
        List<User> users = new List<User>();
        List<Account> accounts = new List<Account>();
        public bool CreateAccount(User user)
        {
            User user1 = new User();
            if (users.Equals(user.Username))
                return false;
            else
            {
                user1 = user;
                users.Add(user1);
                Console.Write("Korisnik {0}", user1.Username);
                return true;
            }
                
        }

        public bool Deposit(Account acc,User user)
        {
            
            Console.WriteLine("Enter the account number you want to deposit to\n");
            int account_no = Convert.ToInt32(Console.ReadLine());

            if (acc.Number==account_no)
            {
                Console.WriteLine("Account number you entered, doesn't exist\n");
                return false;
            }
            else
            {
                Console.WriteLine("Enter amount you want to deposit\n");
                int amount= Convert.ToInt32(Console.ReadLine());

                if((Convert.ToInt32(user.BankAccount)-amount)<0)
                    {
                      Console.WriteLine("There is not enough amount on your bank account");
                      return false;
                    }
                else
                {
                    acc.Amount += amount;
                    user.BankAccount.Amount = user.BankAccount.Amount - amount;
                    return true;
                }
            }
               
               
        }
    }
}
