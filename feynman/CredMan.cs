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
        private const string Path = "acc.json";
        private Account CurrentAccount;
        private string Password = null;
        
        public void SetPassword(string password)
        {
            this.Password = password;
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

        public bool Save()
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

            string encryptedString = Encryption.Encrypt(jsonString, Password);

            StreamWriter fs = new StreamWriter(Path, false);

            fs.WriteLine(encryptedString);

            fs.Close();

            return true;      
        }


        public bool LoadFile(string password)
        {
            string decryptedString = ReadFileGetDecryptedString(Path, password);

            if (String.IsNullOrEmpty(decryptedString))
            {
                return false;
            }
            UserAccs = JsonConvert.DeserializeObject<UserAccounts>(decryptedString);
            Password = password;
            return true;
        }

        public Account GetCurrentAccount()
        {
            return CurrentAccount;
        }

        public List<Account> GetAccounts()
        {
            return this.UserAccs.AccountList;
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

        public bool DoesFileExist()
        {
            if( File.Exists(Path) )
            {
                return true;
            }
            return false;
        }
    }
}
