using System;

namespace ByteCoder
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandParser commandParser = new CommandParser();

            Command command = commandParser.Parse(args);
            if (command == null)
            {
                Console.WriteLine("Error! Wrong input: {0}", commandParser.Error);
                Console.WriteLine("Use with '-h' for syntax help.");
                return;
            }
            
            CommandHandler handler = new CommandHandler();
            
            if (!handler.Process(command))
            {
                Console.WriteLine("Error! {0}", handler.Error);
            }
            else
            {
                Console.WriteLine(handler.Result);
            }

            Console.Write("Press any key to continue...");
            Console.ReadKey(true);
        }
    }
}
