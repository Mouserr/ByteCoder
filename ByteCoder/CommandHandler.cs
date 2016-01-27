using System;
using System.Collections.Generic;
using System.IO;
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
            resultBuilder.Append(
@"
Usage: [-f <file> -m <method>] [-s <search string>] [-h]
where:
    <method>            Name of requesting method: find or checksum
    find                Find all occurences of the <search string> in <file> and display their positions
    checksum            Calculate checksum for <file>
    <file>              File path
    <search string>     Key string to find
    -h                  Show this help message");
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