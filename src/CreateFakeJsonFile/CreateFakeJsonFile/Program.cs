using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace CreateFakeJsonFile
{
    class Program
    {
        static Random random = new Random();
        static double fileSize = 2 ^ 16;
        static string filePath = Path.Combine(Path.GetDirectoryName("."), Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".json");
        static void Main(string[] args)
        {
            filePath = Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".json";
            filePath = Path.Combine(Path.GetDirectoryName("."), filePath);
            if (!ParseArgs(args)) return;
            CreateJson();
            FileInfo file = new FileInfo(filePath);
            Print($"Created file of size {file.Length.ToString("N0")}");
            Print($"{filePath}");
        }

        static void CreateJson()
        {

            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            while(sb.Length < fileSize)
            {
                sb.AppendLine(CreateRandomEntry());
                if (sb.Length < fileSize)
                    sb.Append(",");
            }
            sb.Append("]");
            if (File.Exists(filePath)) File.Delete(filePath);
            using (StreamWriter sw = new StreamWriter(filePath))
                sw.Write(sb.ToString());
        }

        static string CreateRandomEntry()
        {
            var obj = new
            {
                id = Guid.NewGuid().ToString(),
                index = random.Next(),
                isActive = (random.Next() % 2 == 0).ToString(),
                balance = random.Next(100000).ToString("C2"),
                eyeColor = (new string[] { "blue", "red", "green", "purple" })[random.Next() % 4],
                phone = string.Format("+1 ({0,3}){1,3}-{2,3}", random.Next(100, 999), random.Next(100, 999), random.Next(1000, 9999)),
                about = "Nisi proident aliqua irure nisi ad commodo adipisicing labore laborum magna.Cupidatat fugiat est esse esse ipsum.Ut aliquip adipisicing ullamco nisi officia elit eiusmod voluptate magna.Amet labore quis laborum mollit incididunt mollit minim nulla cillum incididunt culpa.\r\n",
                registered = DateTime.Now.AddDays(random.Next(0, 4000) * -1)
            };
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        static bool ParseArgs(string[] args)
        {
            for(int i = 0; i < args.Length; i++)
            {
                if (args[i].ToLower() == "-size")
                    fileSize = double.Parse(args[++i]);
                else if (args[i].ToLower() == "-help")
                    return Usage(false);
                else if (args[i].ToLower() == "-out")
                    filePath = args[++i];
            }

            if (string.IsNullOrEmpty(filePath)) return false;
            if (fileSize < 1) return false;
            return true;
        }

        static bool Usage(bool cancelExit)
        {
            Print("CreateFakeJson [-size {double}] [-out filePath]");
            return cancelExit;
        }

        static void Print(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
