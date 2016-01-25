using System;
using System.Collections.Generic;

namespace ByteCoder
{   
    public class CommandParser
    {
        private readonly Dictionary<string, Func<bool>> argumentReaders;
        private string[] arguments;
        private int currentIndex;
        private Command command;

        public string Error { get; private set; }
       

        public CommandParser()
        {
            argumentReaders = new Dictionary<string, Func<bool>>
            {
                {"-f", parseFileName},
                {"-m", parseMethodName},
                {"-s", parseKeyString},
                {"-h", setHelpState},
            };
        }


        public Command Parse(string[] args)
        {
            arguments = args;
            Error = string.Empty;
            command = new Command();

            if (arguments.Length == 0)
            {
                Error = "Command expected";
                return null;
            }

            for (currentIndex = 0; currentIndex < arguments.Length; currentIndex++)
            {
                Func<bool> readerFunc;
                if (!argumentReaders.TryGetValue(arguments[currentIndex].ToLowerInvariant(), out readerFunc))
                {
                    Error = string.Format("Unknown command: {0}.", arguments[currentIndex]);
                    return null;
                }

                currentIndex++;
                if (!readerFunc())
                    return null;
            }

            if (!command.ShowHelp && !command.Method.HasValue)
            {
                Error = "Method expected";
                return null;
            }

            if (!command.ShowHelp && string.IsNullOrEmpty(command.FilePath))
            {
                Error = "File path expected.";
                return null;
            }

            if (command.Method == MethodType.Find && string.IsNullOrEmpty(command.KeyString))
            {
                Error = "Searching string expected";
                return null;
            }

            return command;
        }

        private bool parseFileName()
        {
            if (arguments.Length <= currentIndex)
            {
                Error = "File path expected.";
                return false;
            }

            command.FilePath = arguments[currentIndex];
            return true;
        }

        private bool parseMethodName()
        {
            if (arguments.Length <= currentIndex)
            {
                Error = "Method name expected.";
                return false;
            }

            string methodName = arguments[currentIndex];
            MethodType method;
            if (!Enum.TryParse(methodName, true, out method))
            {
                Error = string.Format("Unknown method name {0}.", methodName);
                return false;
            }
            command.Method = method;
            return true;
        }

        private bool parseKeyString()
        {
            if (arguments.Length <= currentIndex)
            {
                Error = "String expected.";
                return false;
            }

            command.KeyString = arguments[currentIndex];
            return true;
        }

        private bool setHelpState()
        {
            command.ShowHelp = true;
            return true;
        }
    }
}