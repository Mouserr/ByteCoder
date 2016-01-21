using System;

namespace ByteCoder
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandParser command = new CommandParser();

            if (!command.Parse(args))
            {
                Console.WriteLine("Error! Wrong input: {0}", command.Error);
                Console.WriteLine("Use with '-h' for syntax help.");
            }
            else
            {
                Console.WriteLine("command.FilePath: " + command.FilePath);
                Console.WriteLine("command.KeyString: " + command.KeyString);
                Console.WriteLine("command.MethodType: " + command.Method);
                Console.WriteLine("command.ShowHelp: " + command.ShowHelp);
            }
        }
    }
}
