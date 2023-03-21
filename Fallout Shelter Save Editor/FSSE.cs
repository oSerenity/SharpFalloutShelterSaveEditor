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
        private static readonly uint[] key = new uint[] { 2815074099, 1725469378, 4039046167, 874293617, 3063605751, 3133984764, 4097598161, 3620741625 };
        private static readonly byte[] iv = HexStringToByteArray("7475383967656A693334307438397532");
        private static byte[] ConvertuIntArrayToByteArray(uint[] intArray)
        {
            byte[] byteArray = new byte[intArray.Length * 4];
            for (int i = 0; i < intArray.Length; i++)
            {
                byte[] temp = BitConverter.GetBytes(intArray[i]);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(temp);
                }
                Array.Copy(temp, 0, byteArray, i * 4, 4);
            }
            return byteArray;
        }
        static List<string> FindMatchingLines(string filePath, string searchString)
        {
            List<string> matchingLines = new List<string>();
            string[] lines = File.ReadAllLines(filePath);

            for (int i = 0; i < lines.Length; i++)
            {
                if (ContainsString(lines[i], searchString))
                {
                    matchingLines.Add(lines[i]);
                }
            }

            return matchingLines;
        }
        static bool ContainsString(string text, string searchString)
        {
            return text.Contains(searchString);
        }
        private static byte[] DecryptAes(byte[] cipherBytes, uint[] key, byte[] iv)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = ConvertuIntArrayToByteArray(key);
                aesAlg.IV = iv;
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
                    aes.Key = ConvertuIntArrayToByteArray(key);
                    aes.IV = iv;
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

            byte[] cipherBits = Convert.FromBase64String(File.ReadAllText(fileName));
            byte[] plainBits = DecryptAes(cipherBits, key, iv);
            string jsonStr = Encoding.UTF8.GetString(plainBits);
            string prettyJsonStr;
            try
            {
                prettyJsonStr = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(jsonStr), Newtonsoft.Json.Formatting.Indented);
                return Encoding.UTF8.GetBytes(prettyJsonStr);
            }
            catch (Exception e)
            {
                throw new Exception("Decrypted file does not contain valid JSON: " + e.Message);
            }


        }

        private static byte[] HexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("Hex string must have an even number of characters.");
            }
            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            }
            return bytes;
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
