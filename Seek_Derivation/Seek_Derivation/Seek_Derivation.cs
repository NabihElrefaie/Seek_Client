using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek_Derivation
{
    class Seek_Derivation
    {
        public static void Main(string[] args)
        {
            Console.Title = "Seek Application Configuration Manager";

            while (true)
            {
                Console.Clear();
                Console.WriteLine("╔══════════════════════════════════════╗");
                Console.WriteLine("║   Seek Application - Configuration   ║");
                Console.WriteLine("╚══════════════════════════════════════╝");
                Console.WriteLine();
                Console.WriteLine("1. Encrypt Email Settings (Initial Setup)");
                Console.WriteLine("2. Decrypt Email Settings (Verification)");
                Console.WriteLine("3. Exit");
                Console.WriteLine();

                Console.Write("Select option (1-3): ");
                var choice = Console.ReadLine();

                try
                {
                    switch (choice)
                    {
                        case "1":
                            EmailSettingsEncryptionTool.Run(args);
                            break;
                        case "2":
                            EmailSettingsDecryptionTool.Run(args);
                            break;
                        case "3":
                            return;
                        default:
                            Console.WriteLine("Invalid option. Please try again.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.ResetColor();
                }

                if (choice == "1" || choice == "2")
                {
                    Console.WriteLine("\nPress any key to continue...");
                    Console.ReadKey();
                }
            }
        }
    }
}
