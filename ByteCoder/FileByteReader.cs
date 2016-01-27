using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace ByteCoder
{
    public class FileByteReader : IDisposable
    {
        private const int MaxBufferSize = 4096 * 4096;
        private const int SizeOfWord = sizeof(UInt32);

        private readonly int bufferSize;
        private readonly Stream stream;

        public FileByteReader(string filePath)
        {
            stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            bufferSize = (int) Math.Max(Math.Min((stream.Length/4) * 4, MaxBufferSize), 4);
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        public BigInteger CalculateChecksum()
        {
            if (!stream.CanSeek) return 0;

            int top = Console.CursorTop;
            stream.Seek(0, SeekOrigin.Begin);
            
            BigInteger checksum = 0;
            using (BinaryReader reader = new BinaryReader(stream))
            {
                byte[] buffer = new byte[bufferSize];
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
                    Console.Write("{0}%..", getPositionPercent(reader.BaseStream));
                }

            }

            return checksum;
        }


        public List<long> FindAll(string value)
        {
            if (!stream.CanSeek) return new List<long>();
            
            int top = Console.CursorTop;
            stream.Seek(0, SeekOrigin.Begin);

            List<TestOccurrence> testOccurrences = new List<TestOccurrence>();
            List<long> occurrences = new List<long>();
            using (StreamReader reader = new StreamReader(stream))
            {
                char[] buffer = new char[bufferSize];
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    int readedLength = reader.ReadBlock(buffer, 0, buffer.Length);

                    for (int i = 0; i < readedLength; i++)
                    {
                        checkTestOccurrences(value, testOccurrences, occurrences, i, buffer);

                        if (buffer[i] == value[0])
                        {
                            testOccurrences.Add(new TestOccurrence
                            {
                                Position = reader.BaseStream.Position - reader.CurrentEncoding.GetByteCount(buffer, i, readedLength - i)
                            });
                        }
                    }

                    Console.SetCursorPosition(0, top);
                    Console.Write("{0}%..", getPositionPercent(reader.BaseStream));
                }
                checkTestOccurrences(value, testOccurrences, occurrences, buffer.Length - 1, buffer);
            }
           
            Console.SetCursorPosition(0, top);
            return occurrences;
        }


        private static void checkTestOccurrences(string value,
           List<TestOccurrence> testOccurrences,
           List<long> occurrences,
           int bufferIndex,
           char[] buffer)
        {
            for (int i = 0; i < testOccurrences.Count; )
            {
                testOccurrences[i].CurrentKeyIndex++;
                if (testOccurrences[i].CurrentKeyIndex >= value.Length)
                {
                    occurrences.Add(testOccurrences[i].Position);
                    testOccurrences.RemoveAt(i);
                }
                else
                {
                    if (buffer[bufferIndex] == value[testOccurrences[i].CurrentKeyIndex])
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

        private static int getPositionPercent(Stream stream)
        {
            return (int)(Math.Floor(((double)stream.Position / stream.Length) * 100));
        }
        
        private class TestOccurrence
        {
            public long Position;
            public int CurrentKeyIndex;
        }
    }
}