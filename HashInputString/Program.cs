using SimulationOfDevices.Services.Common;

namespace HashInputStringWithAes;

public static class Program
{
    public static void Main(string[] args)
    {
        string? yesNo;
        do
        {
            byte[] salt = Helpers.GenerateRandomSalt();

            var saltTobase64String = Convert.ToBase64String(salt);
            Console.WriteLine($"Key: {saltTobase64String}");
            Console.WriteLine("Please enter a secret key for the symmetric algorithm.");
            var key = Console.ReadLine();

            Console.WriteLine("Please enter a string for encryption");
            var str = Console.ReadLine();
            string encryptedString = Helpers.EncryptString(key: key, plainText: str);
            Console.WriteLine($"Encrypted string = {encryptedString}");

            var decryptedString = Helpers.DecryptString(key, encryptedString);
            Console.WriteLine($"Decrypted string = {decryptedString}");

            Console.ReadKey();
            Console.WriteLine("Press 'e' to exit or 'y' to perform another encryption");

            yesNo = Console.ReadLine().ToLower();
            while (!yesNo.Equals("y", StringComparison.Ordinal) && !yesNo.Equals("e", StringComparison.Ordinal))
            {
                Console.WriteLine("Invalid!");
                Console.WriteLine("Press 'e' to exit or 'y' to perform another conversion");
                yesNo = Console.ReadLine().ToLower();
            }
        } while (yesNo.Equals("y", StringComparison.Ordinal));
        Environment.Exit(0);
    }
}