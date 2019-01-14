using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StripCr
{
    class Program
    {
        static void Main(string[] args)
        {
            // Verify that the file(s) passed in are of the dir /b/s format

            // Read in the file list
            List<string> files = new List<string>();
            var line = string.Empty;
            while (!string.IsNullOrEmpty(line = Console.ReadLine()))
            {
                if (File.Exists(line))
                    files.Add(line);
            }

            List<Task> tasks = new List<Task>();
            foreach(var file in files)
            { 
                var task = Task.Run(() => ProcessFile(file));
                tasks.Add(task);
            }

            var taskArray = tasks.ToArray();
            while((from t in taskArray where !t.IsCompleted select t).FirstOrDefault() != null)
            {
                System.Threading.Thread.Sleep(1000);
                Console.WriteLine("Waiting for files to be processed...");
            }
            Console.WriteLine("Completed.");
        }

        static void ProcessFile(string fileName)
        {
            try
            {
                int instances = 0;
                byte[] bytes = null;
                bool isBinary = false;

                // Open the file byte stream
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
                {
                    bytes = new byte[fs.Length];

                    // TODO: handle sizes larger than max.int
                    fs.Read(bytes, 0, (int)fs.Length);
                    for (int currentRead = 0, currentWrite = 0; currentRead < fs.Length; currentRead++, currentWrite++)
                    {
                        if (char.IsControl((char)bytes[currentRead]) && bytes[currentRead] != '\r' && bytes[currentRead] != '\n')
                        {
                            isBinary = true;
                            break;
                        }

                        if (bytes[currentRead] == '\r')
                        {
                            instances++;
                            currentRead++;
                        }
                        bytes[currentWrite] = bytes[currentRead];
                    }
                }

                if (!isBinary && instances > 0)
                {
                    File.Delete(fileName);
                    using (FileStream fs = new FileStream(fileName, FileMode.CreateNew, FileAccess.Write))
                    {
                        fs.Write(bytes, 0, bytes.Length - instances);
                        fs.Flush();
                        Console.WriteLine($"Rewrote file({fileName}) removing {instances} carriage returns.");
                    }
                }
                else if (isBinary)
                {
                    Console.WriteLine($"No change to binary file {fileName}.");
                }
                else
                {
                    Console.WriteLine($"No change to file {fileName}.");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to Read/Write file({fileName}).  Message({ex.Message})");
            }
        }
    }
}
