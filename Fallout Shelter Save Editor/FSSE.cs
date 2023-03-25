using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Fallout_Shelter_Save_Editor
{
    internal class FSSE
    {
        private static byte[] ProvideKey()
        {
            return new byte[] { 167, 202, 159, 51, 102, 216, 146, 194, 240, 190, 244, 23, 52, 28, 169, 113, 182, 154, 233, 247, 186, 204, 207, 252, 244, 60, 98, 209, 215, 208, 33, 249 };
        }
        private static byte[] ProvideIV()
        {
            return new byte[] { 116, 117, 56, 57, 103, 101, 106, 105, 51, 52, 48, 116, 56, 57, 117, 50 };
        }
        private static byte[] DecryptAes(byte[] cipherBytes)
        {
            using (Aes aesAlg = Aes.Create())
            {

                aesAlg.Key = ProvideKey();
                aesAlg.IV = ProvideIV();
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (MemoryStream msPlain = new MemoryStream())
                        {
                            byte[] buffer = new byte[1024];
                            int bytesRead;
                            while ((bytesRead = csDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                msPlain.Write(buffer, 0, bytesRead);
                            }
                            return msPlain.ToArray();
                        }
                    }
                }
            }
        }
        public static byte[] Encrypt(string fileName)
        {
            try
            {
                var compactJsonStr = Newtonsoft.Json.JsonConvert.SerializeObject(Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(fileName)));
                var plainBytes = Encoding.UTF8.GetBytes(compactJsonStr);

                using (var aes = new AesCryptoServiceProvider())
                {
                    aes.Key = ProvideKey();
                    aes.IV = ProvideIV();
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Mode = CipherMode.CBC;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                        return Encoding.ASCII.GetBytes(Convert.ToBase64String(cipherBytes));
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("File does not contain valid JSON: " + e.Message);
            }
        }
        public static byte[] Decrypt(string fileName)
        {

            byte[] plainBits = DecryptAes(Convert.FromBase64String(File.ReadAllText(fileName)));
            string prettyJsonStr;
            try
            {
                prettyJsonStr = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(Encoding.UTF8.GetString(plainBits)), Formatting.Indented);
                return Encoding.UTF8.GetBytes(prettyJsonStr);
            }
            catch (Exception e)
            {
                throw new Exception("Decrypted file does not contain valid JSON: " + e.Message);
            }


        }


        static List<string> SearchForBase64Files(string directoryPath)
        {
            List<string> base64Files = new List<string>();

            foreach (string filePath in Directory.GetFiles(directoryPath))
            {
                if (IsBase64Encoded(File.ReadAllText(filePath)))
                {
                    base64Files.Add(filePath);
                }
            }

            return base64Files;
        }
        static bool IsBase64Encoded(string text)
        {
            try
            {
                byte[] data = Convert.FromBase64String(text);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
