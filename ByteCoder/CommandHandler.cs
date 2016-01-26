using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace ByteCoder
{
    public class CommandHandler
    {
        private StringBuilder resultBuilder;

        public string Result
        {
            get { return resultBuilder.ToString(); }
        }

        public string Error { get; private set; }

        public bool Process(Command command)
        {
            resultBuilder = new StringBuilder();
            if (command.ShowHelp)
            {
                showHelp();
                return true;
            }

            if (!File.Exists(command.FilePath))
            {
                Error = string.Format("File {0} does not exist.", command.FilePath);
                return false;
            }
            
            try
            {
                using (FileByteReader reader = new FileByteReader(command.FilePath))
                {
                    switch (command.Method)
                    {
                        case MethodType.Checksum:
                            calculateChecksum(reader);
                            break;
                        case MethodType.Find:
                            findString(reader, command);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Error = "Unexpected exception." + e;
                return false;
            }

            return true;
        }

        private void showHelp()
        {
            resultBuilder.AppendLine("Usage: -f <file> -m find -s <search string>");
            resultBuilder.AppendLine("   or: -f <file> -m checksum");
            resultBuilder.AppendLine("   or: -h");
        }

        private void findString(FileByteReader reader, Command command)
        {
            List<long> occurrences = reader.FindAll(command.KeyString);
            
            if (occurrences.Count == 0)
            {
                resultBuilder.AppendFormat("Can't find string {0} in file {1}.\n", command.KeyString, command.FilePath);
            }
            else
            {
                resultBuilder.AppendLine("String occurrence positions: ");
                resultBuilder.AppendLine(string.Join(" ", occurrences));
            }
        }

        private void calculateChecksum(FileByteReader reader)
        {
            resultBuilder.AppendFormat("Checksum = {0}", reader.CalculateChecksum());
            resultBuilder.AppendLine();
        }
        
    }
}