using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using System.Collections;
using System.IO;
using Newtonsoft.Json;
using System.Security;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace feynman
{
    public class UserAccounts
    {
        public List<Account> AccountList = new List<Account>();
    }

    public class Account
    {
        public String Name;
        public Dictionary<string, string> Credentials = new Dictionary<string, string>();
        public int Index;
    }

    public class CredMan
    {
        private UserAccounts UserAccs = new UserAccounts();
        private string Path;
        private Account CurrentAccount;       

        public void CreateTestCredentials()
        {
            Account acc = new Account();
            acc.Name = "Barclasy";
            acc.Credentials.Add("username", "mt82uk");
            acc.Credentials.Add("password", "pips");

            UserAccs.AccountList.Add(acc);

            string json = JsonConvert.SerializeObject(UserAccs, Formatting.Indented);

            // serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(Path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, UserAccs);
            }
        }

        public bool CreateAccount(string accountName, Dictionary<string, string> credentials)
        {
            if (DoesAccountNameExist(accountName))
            {
                return false;
            }

            Account acc = new Account();
            acc.Name = accountName;
            acc.Index = GetEpoch();

            foreach (KeyValuePair<string, string> entry in credentials)
            {
                acc.Credentials.Add(entry.Key, entry.Value);
            }
            UserAccs.AccountList.Add(acc);

            CurrentAccount = acc;

            return true;
        }

        public void Replace(string accountName, Dictionary<string, string> details)
        {
            if (CurrentAccount.Name != accountName)
            {
                GetAndSetCurrent(accountName);
            }

            UserAccs.AccountList.Remove(CurrentAccount);

            CreateAccount(accountName, details);
        }

        public int GetEpoch()
        {
            TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
            int secondsSinceEpoch = (int)t.TotalSeconds;
            Console.WriteLine(secondsSinceEpoch);
            System.Threading.Thread.Sleep(1);

            return secondsSinceEpoch;
        }

        public string ConvertStringTo64Bit(string word)
        {
            string newWord = "";

            if (word.Length == 8)
            {
                return word;
            }

            if (word.Length < 8)
            {
                int dif = 8 - word.Length;
                newWord = word;

                for (int i = 0; i < dif; i++)
                {
                    newWord += i.ToString();
                }
            }

            if (word.Length > 8)
            {
                newWord = word.Remove(8, word.Length - 8);
            }

            return newWord;
        }

        public bool Save(string password)
        {
            if( Path == null )
            {
                return false;
            }

            if (File.Exists(Path))
            {
                File.Delete(Path);
            }            

            string jsonString = JsonConvert.SerializeObject(UserAccs, Formatting.Indented);

            string encryptedString = Encryption.Encrypt(jsonString, password);

            StreamWriter fs = new StreamWriter(Path, false);

            fs.WriteLine(encryptedString);

            fs.Close();

            return true;      
        }


        public bool LoadFile(string password)
        {
            Path = "acc.json";

            if (File.Exists(Path) == true)
            {
                string decryptedString = ReadFileGetDecryptedString(Path, password);
                if( String.IsNullOrEmpty( decryptedString ) ) {
                    return false;
                }
                UserAccs = JsonConvert.DeserializeObject<UserAccounts>(decryptedString);
                return true;
            }
            else
            {
                UserAccs = new UserAccounts();
                UserAccs.AccountList = new List<Account>();

                Account acc = new Account();
                acc.Name = "Blank";

                UserAccs.AccountList.Add(acc);
                return false;
            }
        }

        public Account GetCurrentAccount()
        {
            return CurrentAccount;
        }

        public List<Account> GetAccounts()
        {
            return this.UserAccs.AccountList;
        }

        public List<string> GetAccountNames()
        {
            List<string> names = new List<string>();

            foreach (Account acc in UserAccs.AccountList)
            {
                names.Add(acc.Name);
            }

            return names;
        }

        public Account GetAndSetCurrent(string accountName)
        {
            foreach (Account acc in UserAccs.AccountList)
            {
                if (acc.Name == accountName)
                {
                    CurrentAccount = acc;
                    return CurrentAccount;
                }
            }
            return null;
        }

        public string GetValueFromCurrent(string key)
        {
            string value = CurrentAccount.Credentials[key];
            return value;
        }

        public bool DoesAccountNameExist(string accountName)
        {
            foreach (Account acc in UserAccs.AccountList)
            {
                if (accountName == acc.Name)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveAccount(string accountName)
        {
            Account accountToRemove = null;

            foreach (Account acc in UserAccs.AccountList)
            {
                if (accountName == acc.Name)
                {
                    accountToRemove = acc;
                    break;
                }
            }

            if (accountToRemove != null)
            {
                UserAccs.AccountList.Remove(accountToRemove);

                if (CurrentAccount == accountToRemove)
                {
                    if (UserAccs.AccountList.Count > 0)
                    {
                        CurrentAccount = UserAccs.AccountList[0];
                    }
                    else
                    {
                        CurrentAccount = null;
                    }
                }
            }
        }

        private string ReadFileGetDecryptedString(string filepath, string password)
        {
            StreamReader reader = new StreamReader(filepath);

            string text = reader.ReadToEnd();

            try
            {
                text = Encryption.Decrypt(text, password);
            }
            catch( Exception ex )
            {
                return null;
            }

            return text;
        }
    }
}
