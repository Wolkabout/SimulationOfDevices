using System.Security.Cryptography;
using System.Text;

namespace SimulationOfDevices.Services.Common
{
    public static class Helpers
    {
        public static DateTime DateThatNeverHappens()
        {
            return new DateTime(9999, 2, 20);
        }

        public static bool IsResponseArray(this string str)
        {
            return str.StartsWith("[");
        }

        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        public static byte[] GenerateRandomSalt()
        {
            RandomNumberGenerator rng = RandomNumberGenerator.Create();

            // Generate a random salt value
            byte[] salt = new byte[16];
            rng.GetBytes(salt);

            return salt;
        }

        //public static DeviceSettings? ReadConfigFile()
        //{
        //    try
        //    {
        //        DeviceSettings deviceSettings = JsonConvert.DeserializeObject<DeviceSettings>(File.ReadAllText(@"DeviceSettings.json"));
        //        if (deviceSettings is null)
        //        {
        //            DeviceSettings defaultSettings = JsonConvert.DeserializeObject<DeviceSettings>(File.ReadAllText(@"DefaultSettings.json"));
        //            return defaultSettings;
        //        }
        //        return deviceSettings;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        //




    }
}
