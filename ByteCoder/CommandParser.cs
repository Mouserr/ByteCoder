using System;
using System.Collections.Generic;

namespace ByteCoder
{
    public enum MethodType
    {
        Find,
        Checksum
    }

    public class CommandParser
    {
        private readonly Dictionary<string, Func< bool>> argumentReaders;
        private string[] arguments;
        private int currentIndex;

        public string Error { get; private set; }
        public string FilePath { get; private set; }
        public MethodType? Method { get; private set; }
        public string KeyString { get; private set; }
        public bool ShowHelp { get; private set; }

        public CommandParser()
        {
            argumentReaders = new Dictionary<string, Func<bool>>
            {
                {"-f", parseFileName},
                {"-m", parseMethodName},
                {"-s", setKeyString},
                {"-h", setHelpState},
            };
        }


        public bool Parse(string[] args)
        {
            arguments = args;
            if (arguments.Length == 0)
            {
                Error = "Command expected";
                return false;
            }

            for (currentIndex = 0; currentIndex < arguments.Length; currentIndex++)
            {
                Func<bool> readerFunc;
                if (!argumentReaders.TryGetValue(arguments[currentIndex].ToLowerInvariant(), out readerFunc))
                {
                    Error = string.Format("Unknown command: {0}.", arguments[currentIndex]);
                    return false;
                }

                currentIndex++;
                if (!readerFunc())
                    return false;
            }

            if (!ShowHelp && !Method.HasValue)
            {
                Error = "Method expected";
                return false;
            }

            if (Method == MethodType.Find && string.IsNullOrEmpty(KeyString))
            {
                Error = "Searching string expected";
                return false;
            }

            return true;
        }

        private bool parseFileName()
        {
            if (arguments.Length <= currentIndex)
            {
                Error = "File path expected.";
                return false;
            }

            FilePath = arguments[currentIndex];
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
            Method = method;
            return true;
        }

        private bool setKeyString()
        {
            if (arguments.Length <= currentIndex)
            {
                Error = "String expected.";
                return false;
            }

            KeyString = arguments[currentIndex];
            return true;
        }

        private bool setHelpState()
        {
            ShowHelp = true;
            return true;
        }
    }
}