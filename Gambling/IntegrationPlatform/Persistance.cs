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

        public static void EmptyBetFiles()
        {
            File.WriteAllText("..\\..\\..\\BetServer\\bin\\Debug\\betUsers.txt", string.Empty);
            File.WriteAllText("..\\..\\..\\BetServer\\bin\\Debug\\results.txt", string.Empty);
        }

        public static void EmptyBankFiles()
        {
            File.WriteAllText("..\\..\\..\\BankServer\\bin\\Debug\\bankUsers.txt", string.Empty);
        }

        public static bool WriteToFile(Object s, String path)
        {
            byte[] encrypted = Helper.Encrypt(s);


            lock (syncLock1)
            {
                File.WriteAllText(path, string.Empty);
                File.WriteAllBytes(path, encrypted);
            }
            return true;
        }

        public static Object ReadFromFile(String path)
        {
            byte[] readBytes;
            Object obj = null;


            lock (syncLock1)
            {
                readBytes = File.ReadAllBytes(path);
                if (readBytes.Count() > 0)
                    obj = Helper.Decrypt(readBytes);
            }

            return obj;
        }
    }
}
