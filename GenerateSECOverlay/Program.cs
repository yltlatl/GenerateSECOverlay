using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace GenerateSECOverlay
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = args[0];
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("No path supplied.", "path");
            if (!File.Exists(path)) throw new ArgumentException("File not found", "path");
            var df = new DelimitedFile(path, "ASCII", "\n".ToCharArray().First(), (char)44, (char)59, (char)34);
            Console.WriteLine("Header fields:\n{0}", string.Join("\n", df.HeaderRecord));
            Console.WriteLine("\nEnter beg bates field name.");
            var begBates = Console.ReadLine();
            Console.WriteLine("\nEnter beg attach field name.");
            var begAttach = Console.ReadLine();
            var outDict = new Dictionary<string, string>(); 
            df.GetNextRecord();
            while (!df.EndOfFile)
            {
                var currentBegBates = df.GetFieldByName(begBates);
                var currentBegAttach = df.GetFieldByName(begAttach);
                if (outDict.Keys.Contains(currentBegAttach))
                {
                    if (string.IsNullOrEmpty(outDict[currentBegAttach]))
                    {
                        outDict[currentBegAttach] = currentBegBates;
                    }
                    else
                    {
                        outDict[currentBegAttach] += "; " + currentBegBates;
                    }
                }
                outDict[currentBegBates] = "";
                df.GetNextRecord();
            }
            using (var o_str = new StreamWriter(@"c:\temp\child_bates_test.csv", false, Encoding.ASCII))
            {
                o_str.AutoFlush = true;
                var innerDelimiter = "\",\"";
                o_str.WriteLine("\"{0}{1}{2}\"", begBates, innerDelimiter, begAttach);
                foreach (var kvp in outDict)
                {
                    var o_fields = new List<string>{ kvp.Key, kvp.Value };
                    var o_line = string.Format("\"{0}\"", string.Join(innerDelimiter, o_fields));
                    o_str.WriteLine(o_line);
                }
            }
        }
    }
}
