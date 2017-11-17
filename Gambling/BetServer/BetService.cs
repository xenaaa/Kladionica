﻿using IntegrationPlatform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace BetServer
{
    public class BetService : IBetService
    {
      //  private static Dictionary<string, User> BetUsers = new Dictionary<string, User>();

        private static Dictionary<string,User> betUsers=new Dictionary<string, User>();

        public static Dictionary<string,User> BetUsers
        {
            get { return betUsers; }
            set { betUsers = value; }
        }

        private static Dictionary<int, Dictionary<int, double>> rezultati=new Dictionary<int, Dictionary<int, double>>();

        public static Dictionary<int, Dictionary<int, double>> Rezultati
        {
            get { return rezultati; }
            set { rezultati = value; }
        }


     


        public BetService()
        {


        }

        public bool Login(string username, string password)//ne treba autentifikacija kod LogIn
        {
            if (BetUsers.Keys.Contains(username))
            {
                if (BetUsers[username].Password == password)
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
                if (AddUser(user))
                    return true;
                else
                    return false;
            }
        }


        public bool AddUser(User user)
        {
            WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
            Console.WriteLine("User {0} je pozvao AddUser\n", identity.Name);
            if (BetUsers.ContainsKey(identity.Name) )
            {
                if (!BetUsers.ContainsKey(user.Username))
                {
                    BetUsers.Add(user.Username, user);
                    Console.WriteLine("User {0} successfully added to BetUsers", user.Username);
                    return true;
                }
                else
                {

                    Console.WriteLine("User {0} already exists.", user.Username);
                    return false;
                }
            }
            else
            {
                Console.WriteLine("User {0} doesn't exist", identity.Name);
                return false;
            }
        }

        public bool DeleteUser(User user)
        {
            WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
            Console.WriteLine("User {0} je pozvao DeleteUser\n", identity.Name);
            if (BetUsers.ContainsKey(identity.Name))
            {
                if (!BetUsers.ContainsKey(user.Username))
                {
                    Console.WriteLine("Error! There is no user {0} in BetService", user.Username);
                    return false;
                }
                else
                {
                    BetUsers.Remove(user.Username);
                    Console.WriteLine("User {0} removed from BetService", user.Username);
                    return true;
                }
            }
            else
            {
                Console.WriteLine("User {0} doesn't exist", identity.Name);
                return false;
            }
        }

        public bool EditUser(User user)
        {
            WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
            Console.WriteLine("User {0} je pozvao EditUser\n", identity.Name);
            if (BetUsers.ContainsKey(identity.Name))
            {
                if (!BetUsers.ContainsKey(user.Username))
                {
                    Console.WriteLine("Error! There is no user {0} in BetService", user.Username);
                    return false;
                }
                else
                {
                    foreach (KeyValuePair<string, User> kvp in BetUsers)
                    {
                        if (kvp.Key == user.Username)
                        {
                            kvp.Value.BetAccount = user.BetAccount;
                            kvp.Value.Role = user.Role;
                            kvp.Value.Password = user.Password;
                        }
                    }
                    return true;
                }
            }
            else
            {
                Console.WriteLine("User {0} doesn't exist", identity.Name);
                return false;
            }
        }


        public bool SendTicket(Ticket ticket, string username)
        { WindowsIdentity identity = (WindowsIdentity)Thread.CurrentPrincipal.Identity;
            Console.WriteLine("User {0} je pozvao SendTicket\n", identity.Name);
            if (BetUsers.ContainsKey(identity.Name))
            {

                if (BetUsers.ContainsKey(username))
                {
                    BetUsers[username].Tickets.Add(ticket);
                    return true;
                }
                else
                    return false;

            }
            else
            {
                Console.WriteLine("User {0} doesn't exist", identity.Name);
                return false;
            }
        }



        //bool sent = false;
        //try
        //{
        //    sent = factory.SendOffers(offers);
        //    Console.WriteLine("SendOffers() >> {0}", sent);
        //}
        //catch (Exception e)
        //{
        //    Console.WriteLine("Error while trying to SendOffers(). {0}", e.Message);
        //}

        //return sent;

    }
}


