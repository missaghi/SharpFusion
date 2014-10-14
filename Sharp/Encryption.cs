using System;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Reflection;

namespace Sharp
{

    public class Encryption
    {
        public byte[] Key { get; set; }  //32 byte
        public byte[] Vector { get; set; } //16 byte

        private ICryptoTransform EncryptorTransform, DecryptorTransform;
        private System.Text.UTF8Encoding UTFEncoder;

        public void Init()
        {
            //This is our encryption method
            RijndaelManaged rm = new RijndaelManaged();

            //Create an encryptor and a decryptor using our encryption method, key, and vector.
            EncryptorTransform = rm.CreateEncryptor(this.Key, this.Vector);
            DecryptorTransform = rm.CreateDecryptor(this.Key, this.Vector);

            //Used to translate bytes to text and vice versa
            UTFEncoder = new System.Text.UTF8Encoding();
        }

        /// <summary>
        /// Initialize encryption engine with specific data
        /// </summary>
        /// <param name="key">32 bytes</param>
        /// <param name="vector">16 bytes</param>
        public Encryption(byte[] key, byte[] vector)
        {
            this.Key = key;
            this.Vector = vector;
            Init();
        }


        static private string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = LocalFile.UnescapeDataString(uri.Path);
                //return Path.GetDirectoryName(path);
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }
         

        /// <summary>
        /// Initialize encryption engine with a key file in the assembly folder, or generate a new one if it doesn't exist
        /// </summary>
        public Encryption()
        {
            string KeyPath = Path.GetDirectoryName(AssemblyDirectory) + "/key.config";
            if (File.Exists(KeyPath))
            {
                BinaryReader sr = new BinaryReader(File.OpenRead(KeyPath));
                Key = sr.ReadBytes(32);
                Vector = sr.ReadBytes(16);
                sr.Dispose();
            }
            else //create key
            {
                Key = GenerateEncryptionKey();
                Vector = GenerateEncryptionVector();

                BinaryWriter br = new BinaryWriter(File.Create(KeyPath));
                br.Write(Key);
                br.Write(Vector);
                br.Dispose();
            }

            Init();

        }

        /// -------------- Two Utility Methods (not used but may be useful) -----------
        /// Generates an encryption key.
        static public byte[] GenerateEncryptionKey()
        {
            //Generate a Key.
            RijndaelManaged rm = new RijndaelManaged();
            rm.GenerateKey();
            return rm.Key;
        }

        /// Generates a unique encryption vector
        static public byte[] GenerateEncryptionVector()
        {
            //Generate a Vector
            RijndaelManaged rm = new RijndaelManaged();
            //rm.Padding = PaddingMode.PKCS7; 
            rm.GenerateIV();
            return rm.IV;
        }


        /// ----------- The commonly used methods ------------------------------    
        /// Encrypt some text and return a string suitable for passing in a URL.
        public string EncryptToString(string TextValue)
        {
            return ByteArrToString(Encrypt(TextValue));
        }

        /// ----------- The commonly used methods ------------------------------    
        /// Encrypt a long and return a string suitable for passing in a URL.
        public string EncryptToString(long Long)
        {
            return ByteArrToString(Encrypt(System.BitConverter.GetBytes(Long)));
        }

        /// Encrypt some text and return an encrypted byte array.
        public byte[] Encrypt(string TextValue)
        {
            //Translates our text value into a byte array.
            Byte[] bytes = UTFEncoder.GetBytes(TextValue);

            return Encrypt(bytes);
        }

        private byte[] Encrypt(Byte[] bytes)
        {
            //Used to stream the data in and out of the CryptoStream.
            MemoryStream memoryStream = new MemoryStream();

            /*
             * We will have to write the unencrypted bytes to the stream,
             * then read the encrypted result back from the stream.
             */
            #region Write the decrypted value to the encryption stream
            CryptoStream cs = new CryptoStream(memoryStream, EncryptorTransform, CryptoStreamMode.Write);
            cs.Write(bytes, 0, bytes.Length);
            cs.FlushFinalBlock();
            #endregion

            #region Read encrypted value back out of the stream
            memoryStream.Position = 0;
            byte[] encrypted = new byte[memoryStream.Length];
            memoryStream.Read(encrypted, 0, encrypted.Length);
            #endregion

            //Clean up.
            cs.Close();
            memoryStream.Close();

            return encrypted;
        }

        /// The other side: Decryption methods
        public string DecryptString(string EncryptedString)
        {
            if (EncryptedString.NNOE())
                return Decrypt(StrToByteArray(EncryptedString));
            else
                return "";
        }

        /// The other side: Decryption methods
        public long DecryptLong(string EncryptedString)
        {
            return System.BitConverter.ToInt64(DecryptToBytes(StrToByteArray(EncryptedString)), 0);
        }

        /// Decryption when working with byte arrays.    
        public string Decrypt(byte[] EncryptedValue)
        {
            Byte[] decryptedBytes = DecryptToBytes(EncryptedValue);
            return UTFEncoder.GetString(decryptedBytes);
        }

        private Byte[] DecryptToBytes(byte[] EncryptedValue)
        {
            #region Write the encrypted value to the decryption stream
            MemoryStream encryptedStream = new MemoryStream();
            CryptoStream decryptStream = new CryptoStream(encryptedStream, DecryptorTransform, CryptoStreamMode.Write);
            decryptStream.Write(EncryptedValue, 0, EncryptedValue.Length);
            decryptStream.FlushFinalBlock();
            #endregion

            #region Read the decrypted value from the stream.
            encryptedStream.Position = 0;
            Byte[] decryptedBytes = new Byte[encryptedStream.Length];
            encryptedStream.Read(decryptedBytes, 0, decryptedBytes.Length);
            encryptedStream.Close();
            #endregion
            return decryptedBytes;
        }

        /// Convert a string to a byte array.  NOTE: Normally we'd create a Byte Array from a string using an ASCII encoding (like so).
        //      System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
        //      return encoding.GetBytes(str);
        // However, this results in character values that cannot be passed in a URL.  So, instead, I just
        // lay out all of the byte values in a long string of numbers (three per - must pad numbers less than 100).
        public static byte[] StrToByteArray(string str)
        {
            if (str.Length == 0)
                throw new Exception("Invalid string value in StrToByteArray");

            byte val;
            byte[] byteArr = new byte[str.Length / 3];
            int i = 0;
            int j = 0;
            do
            {
                val = byte.Parse(str.Substring(i, 3));
                byteArr[j++] = val;
                i += 3;
            }
            while (i < str.Length);
            return byteArr;
        }

        // Same comment as above.  Normally the conversion would use an ASCII encoding in the other direction:
        //      System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
        //      return enc.GetString(byteArr);    
        public static string ByteArrToString(byte[] byteArr)
        {
            byte val;
            string tempStr = "";
            for (int i = 0; i <= byteArr.GetUpperBound(0); i++)
            {
                val = byteArr[i];
                if (val < (byte)10)
                    tempStr += "00" + val.ToString();
                else if (val < (byte)100)
                    tempStr += "0" + val.ToString();
                else
                    tempStr += val.ToString();
            }
            return tempStr;
        }
    }
}