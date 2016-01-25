using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;

namespace ByteCoder
{
    public class CommandHandler
    {
        private const int BufferSize = 4096 * 4096;
        private const int SizeOfWord = sizeof(UInt32);

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

            if (!validateFile(command))
                return false;
            
            try
            {
                switch (command.Method)
                {
                    case MethodType.Checksum:
                        calculateChecksum(command);
                        break;
                    case MethodType.Find:
                        findString(command);
                        break;
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

        private bool validateFile(Command command)
        {
            if (!File.Exists(command.FilePath))
            {
                Error = string.Format("File {0} does not exist.", command.FilePath);
                return false;
            }

            return true;
        }

        private void findString(Command command)
        {
            int top = Console.CursorTop;
            Console.SetCursorPosition(0, top);
            List<TestOccurrence> testOccurrences = new List<TestOccurrence>();
            List<long> occurrences = new List<long>();
            using (FileStream stream = getFileStream(command))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    char[] buffer = new char[BufferSize];
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        int readedLength = reader.ReadBlock(buffer, 0, buffer.Length);

                        for (int i = 0; i < readedLength; i++)
                        {
                            checkTestOccurrences(command,
                                testOccurrences, occurrences, i, buffer);

                            if (buffer[i] == command.KeyString[0])
                            {
                                testOccurrences.Add(new TestOccurrence
                                {
                                    Position = reader.BaseStream.Position - readedLength + i
                                });
                            }
                        }

                        Console.SetCursorPosition(0, top);
                        Console.Write("{0}%..", getProgress(reader.BaseStream));
                    }
                }
            }

            if (occurrences.Count == 0)
            {
                resultBuilder.AppendFormat("Can't find string {0} in file {1}.\n", command.KeyString, command.FilePath);
            }
            else
            {
                resultBuilder.AppendLine("String occurrence positions: ");
                resultBuilder.AppendLine(string.Join(" ", occurrences));
            }
            Console.SetCursorPosition(0, top);
        }

        private void calculateChecksum(Command command)
        {
            int top = Console.CursorTop;
            BigInteger checksum = 0;
            
            using (FileStream stream = getFileStream(command))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    byte[] buffer = new byte[BufferSize];
                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        int length = reader.Read(buffer, 0, buffer.Length);

                        for (int i = 0; i < length; i += SizeOfWord)
                        {
                            if (length - i < SizeOfWord)
                            {
                                for (int j = length; j < buffer.Length && j < i + SizeOfWord; j++)
                                {
                                    buffer[j] = 0;
                                }
                            }
                            checksum += BitConverter.ToUInt32(buffer, i);
                        }

                        Console.SetCursorPosition(0, top);
                        Console.Write("{0}%..", getProgress(reader.BaseStream));
                    }

                }
            }

            Console.SetCursorPosition(0, top);
            resultBuilder.AppendFormat("Checksum = {0}", checksum);
            resultBuilder.AppendLine();
        }

        private static int getProgress(Stream stream)
        {
            return (int)(Math.Floor(((double)stream.Position / stream.Length) * 100));
        }

        private static FileStream getFileStream(Command command)
        {
            return new FileStream(command.FilePath, FileMode.Open, FileAccess.Read);
        }

        private static void checkTestOccurrences(Command command,
           List<TestOccurrence> testOccurrences,
           List<long> occurrences,
           int bufferIndex,
           char[] buffer)
        {
            for (int i = 0; i < testOccurrences.Count; )
            {
                testOccurrences[i].CurrentKeyIndex++;
                if (testOccurrences[i].CurrentKeyIndex >= command.KeyString.Length)
                {
                    occurrences.Add(testOccurrences[i].Position);
                    testOccurrences.RemoveAt(i);
                }
                else
                {
                    if (buffer[bufferIndex] == command.KeyString[testOccurrences[i].CurrentKeyIndex])
                    {
                        i++;
                    }
                    else
                    {
                        testOccurrences.RemoveAt(i);
                    }
                }
            }
        }

        private class TestOccurrence
        {
            public long Position;
            public int CurrentKeyIndex;
        }
    }
}