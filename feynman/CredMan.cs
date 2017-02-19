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

        public void Load()
        {
            Path = "acc.json";
            LoadFile("blade");

            if( UserAccs.AccountList.Count > 0 )
            {
                CurrentAccount = UserAccs.AccountList[0];
            }
        }     

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

        public bool CreateAccount( string accountName, Dictionary<string,string> credentials )
        {
            if( DoesAccountNameExist(accountName) )
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

        public void Replace(string accountName, Dictionary<string,string> details )
        {
            if( CurrentAccount.Name != accountName)
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

        public string ConvertStringTo64Bit(string word )
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

        public void Save(string password)
        {
            if( File.Exists(Path))
            {
                File.Delete(Path);
            }

            // 64 bits is the only valid key size for the DES encryption algorithm.
            // ASCII encoding uses 8 bits per character, therefore 8 ASCII characters == 64 bits.

            password = "cheesecheese";

            if( password.Length < 8 )
            {
                int dif = 8 - password.Length;

                for (int i = 0; i < dif; i++)
                {
                    password += i.ToString();

                }
            }

            if( password.Length > 8 )
            {
                password = password.Remove(8, password.Length - 8);
            }        

            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(password);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(password);

            FileStream fsEncrypted = new FileStream(Path, FileMode.Create, FileAccess.Write);

            ICryptoTransform desencrypt = DES.CreateEncryptor();

            CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);

            string json = JsonConvert.SerializeObject(UserAccs, Formatting.Indented);

            byte[] bytearrayinput = Encoding.ASCII.GetBytes(json);

            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Close();

            return;



            // serialize JSON directly to a file
            using (StreamWriter file = File.CreateText(Path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, UserAccs);
            }

            EncryptFile(Path, Path, password);
        }


        public void LoadFile(string password)
        {
            if (File.Exists(Path) == true)
            {
                UserAccs = JsonConvert.DeserializeObject<UserAccounts>(File.ReadAllText(Path));
            }
            else
            {
                UserAccs = new UserAccounts();
                UserAccs.AccountList = new List<Account>();

                Account acc = new Account();
                acc.Name = "Blank";

                UserAccs.AccountList.Add(acc);
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

            foreach( Account acc in UserAccs.AccountList )
            {
                names.Add( acc.Name );
            }

            return names;
        }

        public Account GetAndSetCurrent( string accountName )
        {
            foreach( Account acc in UserAccs.AccountList )
            {
                if( acc.Name == accountName )
                { 
                    CurrentAccount = acc;
                    return CurrentAccount;
                }                
            }
            return null;
        }

        public string GetValueFromCurrent( string key)
        {
            string value = CurrentAccount.Credentials[key];
            return value;       
        }

        public bool DoesAccountNameExist( string accountName )
        {
            foreach( Account acc in UserAccs.AccountList )
            {
                if( accountName == acc.Name )
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveAccount(string accountName )
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
                
                if( CurrentAccount == accountToRemove )
                {
                    if (UserAccs.AccountList.Count > 0 )
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

        public void CleanUp()
        {
            Save("blade");
        }

        private void EncryptFile(string sInputFilename, string sOutputFilename, string sKey)
        {
            FileStream fsInput = new FileStream(sInputFilename, FileMode.Open, FileAccess.Read);
            FileStream fsEncrypted = new FileStream(sOutputFilename, FileMode.Create, FileAccess.Write);
            
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);

            ICryptoTransform desencrypt = DES.CreateEncryptor();
            CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[fsInput.Length];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Close();
            fsInput.Close();
            fsEncrypted.Close();
        }

        private void DecryptFile(string sInputFilename, 
            string sOutputFilename, string sKey)
        {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            //A 64 bit key and IV is required for this provider.
            //Set secret key For DES algorithm.
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            //Set initialization vector.
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);

            //Create a file stream to read the encrypted file back.
            FileStream fsread = new FileStream(sInputFilename,
               FileMode.Open,
               FileAccess.Read);
            //Create a DES decryptor from the DES instance.
            ICryptoTransform desdecrypt = DES.CreateDecryptor();
            //Create crypto stream set to read and do a 
            //DES decryption transform on incoming bytes.
            CryptoStream cryptostreamDecr = new CryptoStream(fsread,
               desdecrypt,
               CryptoStreamMode.Read);
            //Print the contents of the decrypted file.
            StreamWriter fsDecrypted = new StreamWriter(sOutputFilename);
            fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
            fsDecrypted.Flush();
            fsDecrypted.Close();
        }

        public void TestEncryption()
        {
            string testString = "testtesttest";
            string password = "passwordpassword";

            string filepath = "plain.txt";
            string encryptedFilepath = "encrypted.txt";
            string encryptedStreamFilepath = "streamEncrypted.txt";
            string decryptedFilepath = "decrypted.txt";
            string decryptedStreamFilepath = "streamDecrypted.txt";

            password = ConvertStringTo64Bit(password);

            // Write to file 

            if( File.Exists( filepath ) )
            {
                File.Delete(filepath);
            }

            if( File.Exists( encryptedFilepath ) )
            {
                File.Delete(encryptedFilepath);
            }

            if( File.Exists( decryptedFilepath ) )
            {
                File.Delete(decryptedFilepath);
            }

            StreamWriter fs = new StreamWriter(filepath);
            fs.WriteLine(testString);
            fs.Close();

            EncryptFile(filepath, encryptedFilepath, password);
            EncryptStringWriteToFile(testString, encryptedStreamFilepath, password);

            DecryptFile(encryptedFilepath, decryptedFilepath, password);
            DecryptFile(encryptedStreamFilepath, decryptedStreamFilepath, password);
            string result = DecryptFileToString(encryptedFilepath, password);
        }

        private void EncryptStringWriteToFile( string text, string filepath, string password )
        {
            FileStream fsEncrypted = new FileStream(filepath, FileMode.Create, FileAccess.Write);

            password = ConvertStringTo64Bit(password);

            byte[] byteArray = Encoding.ASCII.GetBytes(text.ToString());
            MemoryStream fsInput = new MemoryStream(byteArray);

            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            DES.Key = ASCIIEncoding.ASCII.GetBytes(password);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(password);

            ICryptoTransform desencrypt = DES.CreateEncryptor();
            CryptoStream cryptostream = new CryptoStream(fsEncrypted, desencrypt, CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[fsInput.Length];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);

            cryptostream.Close();
            fsInput.Close();
            fsEncrypted.Close();
        }

        private string DecryptFileToString( string filepath, string password )
        {
            StreamReader reader = new StreamReader(filepath);

            string text = reader.ReadToEnd();

            text = DecryptString(text, password);

            return text;           
        }

        public string DecryptString(string cipherText, string sKey)
        {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            //A 64 bit key and IV is required for this provider.
            //Set secret key For DES algorithm.
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            //Set initialization vector.
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);

            // Instantiate a new MemoryStream object to contain the encrypted bytes
            MemoryStream memoryStream = new MemoryStream();

            //Create a DES decryptor from the DES instance.
            ICryptoTransform desdecrypt = DES.CreateDecryptor();

            // Instantiate a new CryptoStream object to process the data and write it to the 
            // memory stream
            CryptoStream cryptoStream = new CryptoStream(memoryStream, desdecrypt, CryptoStreamMode.Write);

            // Will contain decrypted plaintext
            string plainText = String.Empty;

            try
            {
                // Convert the ciphertext string into a byte array
                byte[] cipherBytes = Encoding.ASCII.GetBytes(cipherText);  
                 
                // Decrypt the input ciphertext string
                cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

                // Complete the decryption process
                cryptoStream.FlushFinalBlock();

                // Convert the decrypted data from a MemoryStream to a byte array
                byte[] plainBytes = memoryStream.ToArray();

                // Convert the encrypted byte array to a base64 encoded string
                plainText = Encoding.ASCII.GetString(plainBytes, 0, plainBytes.Length);
            }
            finally
            {
                // Close both the MemoryStream and the CryptoStream
                memoryStream.Close();
                cryptoStream.Close();
            }

            // Return the encrypted data as a string
            return plainText;
        }
        

    }
}
