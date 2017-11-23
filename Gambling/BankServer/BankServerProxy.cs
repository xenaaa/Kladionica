﻿using CertificateManager;
using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace BankServer
{
    public class BankServerProxy : ChannelFactory<IBetService>, IBetService, IDisposable
    {
        IBetService factory;

        public BankServerProxy(NetTcpBinding binding, EndpointAddress address) : base(binding, address)
        {

            string cltCertCN = "bankserviceclient"; 

            this.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.ChainTrust;
            this.Credentials.ServiceCertificate.Authentication.RevocationMode = X509RevocationMode.NoCheck;

            /// Set appropriate client's certificate on the channel. Use CertManager class to obtain the certificate based on the "cltCertCN"
            this.Credentials.ClientCertificate.Certificate = CertManager.GetCertificateFromStorage(StoreName.My, StoreLocation.LocalMachine, cltCertCN);
            //  Console.WriteLine(this.Credentials.ClientCertificate.Certificate.ToString());
            factory = this.CreateChannel();
        }

        public bool Deposit(byte[] acc, byte[] username)
        {
            try
            {
                factory.Deposit(acc, username);
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
            throw new NotImplementedException();
        }

        public bool BetLogin(byte[] username, byte[] password, byte[] port)
        {
            throw new NotImplementedException();
        }

        public bool CheckIfAlive()
        {
            throw new NotImplementedException();
        }

        public bool DeleteUser(byte[] username)
        {
            throw new NotImplementedException();
        }

        public bool EditUser(byte[] user)
        {
            throw new NotImplementedException();
        }

        public bool SendPort(byte[] username, byte[] port, byte[] address)
        {
            throw new NotImplementedException();
        }

        public bool SendTicket(byte[] ticket, byte[] username)
        {
            throw new NotImplementedException();
        }


        public bool GetServiceIP(byte[] AddressStringBytes)
        {
            try
            {
                factory.GetServiceIP(AddressStringBytes);
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