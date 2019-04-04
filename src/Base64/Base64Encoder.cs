using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Base64
{
    class Base64FileEncoder
    {
        public void Encode(string inFileName, string outFileName)
        {
            if (outFileName != null)
            {
                using (System.IO.FileStream outStream = System.IO.File.Create(outFileName))
                {
                    Encode(inFileName, outStream);
                }
            }
            else
            {
                using (Stream outStream = Console.OpenStandardOutput())
                {
                    Encode(inFileName, outStream);
                }

            }
        }
        public void Decode(string inFileName, string outFileName)
        {
            if (outFileName != null)
            {
                using (System.IO.FileStream outStream = System.IO.File.Create(outFileName))
                {
                    Decode(inFileName, outStream);
                }
            }
            else
            {
                using (Stream outStream = Console.OpenStandardOutput())
                {
                    Decode(inFileName, outStream);
                }

            }
        }
        private void Encode(string inFileName, Stream outFile)
        {
            System.Security.Cryptography.ICryptoTransform transform = new System.Security.Cryptography.ToBase64Transform();
            using (System.IO.FileStream inFile = System.IO.File.OpenRead(inFileName))
            {
                
                using (System.Security.Cryptography.CryptoStream cryptStream = new System.Security.Cryptography.CryptoStream(outFile, transform, System.Security.Cryptography.CryptoStreamMode.Write))
                {
                    // I'm going to use a 4k buffer, tune this as needed
                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    while ((bytesRead = inFile.Read(buffer, 0, buffer.Length)) > 0)
                        cryptStream.Write(buffer, 0, bytesRead);

                    cryptStream.FlushFinalBlock();
                }
            }
        }

        private void Decode(string inFileName, Stream outFile)
        {
            System.Security.Cryptography.ICryptoTransform transform = new System.Security.Cryptography.FromBase64Transform();
            using (System.IO.FileStream inFile = System.IO.File.OpenRead(inFileName))
            {

                using (System.Security.Cryptography.CryptoStream cryptStream = new System.Security.Cryptography.CryptoStream(inFile, transform, System.Security.Cryptography.CryptoStreamMode.Read))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    while ((bytesRead = cryptStream.Read(buffer, 0, buffer.Length)) > 0)
                        outFile.Write(buffer, 0, bytesRead);

                    outFile.Flush();
                }
            }
        }

        // this version of Encode pulls everything into memory at once
        // you can compare the output of my Encode method above to the output of this one
        // the output should be identical, but the crytostream version
        // will use way less memory on a large file than this version.
        public void MemoryEncode(string inFileName, string outFileName)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(inFileName);
            System.IO.File.WriteAllText(outFileName, System.Convert.ToBase64String(bytes));
        }
    }
}
