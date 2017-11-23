using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class Persistance
    {
        private static object syncLock1 = new object();
        private static object syncLock2 = new object();
        private static object syncLock3 = new object();
        private static object syncLock4 = new object();

        public static void EmptyFiles()
        {
            File.WriteAllText("..\\..\\..\\BetServer\\bin\\Debug\\betUsers.txt", string.Empty);
            File.WriteAllText("..\\..\\..\\BankServer\\bin\\Debug\\bankUsers.txt", string.Empty);
            File.WriteAllText("..\\..\\..\\BetServer\\bin\\Debug\\results.txt", string.Empty);
        }

        public static bool WriteToFile(Object s, String type)
        {
            byte[] encrypted = Helper.Encrypt(s);

            switch (type)
            {
                case "betUsers":
                    {
                        lock (syncLock1)
                        {
                            File.WriteAllText("betUsers.txt", string.Empty);
                            File.WriteAllBytes("betUsers.txt", encrypted);
                            break;
                        }
                    }
                case "bankUsers":
                    {
                        lock (syncLock1)
                        {
                            File.WriteAllText("bankUsers.txt", string.Empty);
                            File.WriteAllBytes("bankUsers.txt", encrypted);
                            break;
                        }
                    }            
                case "results":
                    {
                        lock (syncLock3)
                        {
                            File.WriteAllText("results.txt", string.Empty);
                            File.WriteAllBytes("results.txt", encrypted);
                            break;
                        }
                    }            
            }
            return true;
        }

        public static Object ReadFromFile(String type)
        {
            byte[] readBytes;
            Object obj = null;

            switch (type)
            {
                case "betUsers":
                    {
                        lock (syncLock1)
                        {
                            readBytes = File.ReadAllBytes("betUsers.txt");
                            if (readBytes.Count() > 0)
                                obj = Helper.Decrypt(readBytes);
                            break;
                        }
                    }
                case "bankUsers":
                    {
                        lock (syncLock1)
                        {
                            readBytes = File.ReadAllBytes("bankUsers.txt");
                            if (readBytes.Count() > 0)
                                obj = Helper.Decrypt(readBytes);
                            break;
                        }
                    }               
                case "results":
                    {
                        lock (syncLock3)
                        {
                            readBytes = File.ReadAllBytes("results.txt");
                            if (readBytes.Count() > 0)
                                obj = Helper.Decrypt(readBytes);
                            break;
                        }
                    }     
            }
            return obj;
        }
    }
}
