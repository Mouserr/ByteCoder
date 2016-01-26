using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using ByteCoder;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ByteCoderTests
{
    [TestClass]
    public class FileByteReaderTests
    {
        Random random = new Random();
        private const string TmpFileName = "justForTesting.test";
        

        [TestMethod]
        public void EmptyFileTest()
        {
            createFile(string.Empty);
            
            using (FileByteReader reader = new FileByteReader(TmpFileName))
            {
                Assert.AreEqual(0, reader.CalculateChecksum());
                List<long> occurences = reader.FindAll(string.Empty);
                Assert.IsNotNull(occurences);
                Assert.AreEqual(0, occurences.Count);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void NoFileExceptionTest()
        {
            using (FileByteReader reader = new FileByteReader(TmpFileName))
            {
            }
        }

        [TestMethod]
        public void ChecksumTest()
        {
            BigInteger expectedSum = createFileWithRandomNumbers(15);

            assertReaderSum(expectedSum);
        }

        [TestMethod]
        public void ChecksumTestWithCutEnding()
        {
            BigInteger expectedSum = createFileWithRandomNumbers(10, true);

            assertReaderSum(expectedSum);
        }


        [TestMethod]
        public void ChecksumTestBigFile()
        {
            BigInteger expectedSum = createFileWithRandomNumbers(1000000);

            assertReaderSum(expectedSum);
        }

        [TestMethod]
        public void FindAllCustomTest()
        {
            string searchString = "hello";
            List<long> expectedMatches = createFileWithStringInsertions(searchString);


            using (FileByteReader reader = new FileByteReader(TmpFileName))
            {
                List<long> matches = reader.FindAll(searchString);
                Assert.AreEqual(expectedMatches.Count, matches.Count);

                for (int i = 0; i < matches.Count; i++)
                {
                    Assert.AreEqual(expectedMatches[i], matches[i]);
                }
            }

            
        }

        [TestCleanup()]
        public void Cleanup()
        {

            removeFile();
        }

        private List<long> createFileWithStringInsertions(string searchString)
        {
            List<long> expectedMatches = new List<long>();
            int fileSize = 10000;
            int matchesCount = 12;
            int shift = fileSize/matchesCount;
            using (StreamWriter writer = new StreamWriter(TmpFileName))
            {
                for (int i = 0; i < fileSize; i++)
                {
                    writer.Write('a');
                }
            }
            using (FileStream file = new FileStream(TmpFileName, FileMode.OpenOrCreate))
            {
                for (int i = 0; i < matchesCount; i++)
                {
                    int offset = random.Next(shift*i, shift*(i + 1));
                    file.Seek(offset, SeekOrigin.Begin);
                    file.Write(Encoding.UTF8.GetBytes(searchString), 0, searchString.Length);
                    expectedMatches.Add(offset);
                }
            }
            return expectedMatches;
        }

        private void assertReaderSum(BigInteger realSum)
        {
            using (FileByteReader reader = new FileByteReader(TmpFileName))
            {
                Assert.AreEqual(realSum, reader.CalculateChecksum());
            }
        }

        private BigInteger createFileWithRandomNumbers(int count, bool withCutEnding = false)
        {
            BigInteger sum = 0;
            using(FileStream file = new FileStream(TmpFileName, FileMode.Create))
            {
                using (BinaryWriter writer = new BinaryWriter(file))
                {
                    byte[] buffer = new byte[4096*4096];
                    int cur = 0;
                    for (int i = 0; i < count; i++)
                    {
                        uint nextNumber = getRandomUInt();
                        sum += nextNumber;
                        byte[] bytes = BitConverter.GetBytes(nextNumber);
                        for (int j = 0; j < bytes.Length; j++)
                        {
                            buffer[cur + j] = bytes[j];
                        }
                        cur += bytes.Length;

                        if (cur >= buffer.Length)
                        {
                            writer.Write(buffer);
                            for (int j = 0; j < buffer.Length; j++)
                            {
                                buffer[j] = 0;
                            }
                            cur = 0;
                        }
                    }

                    if (cur < buffer.Length)
                    {
                        writer.Write(buffer, 0, cur);
                    }

                    if (withCutEnding)
                    {
                        byte[] bytes = BitConverter.GetBytes(getRandomUInt());
                        bytes[2] = 0;
                        bytes[3] = 0;
                        sum += BitConverter.ToUInt32(bytes, 0);
                        writer.Write(bytes, 0, 2);
                    }
                }
            }

            return sum;
        }

        private uint getRandomUInt()
        {
            return (uint)(random.NextDouble() * uint.MaxValue);
        }

        private void createFile(string text)
        {
            using (StreamWriter writer = new StreamWriter(TmpFileName))
            {
                writer.Write(text);
            }
        }

        private void removeFile()
        {
            if (File.Exists(TmpFileName))
                File.Delete(TmpFileName);
        }

    }
}
