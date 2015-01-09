using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
            Console.WriteLine("Use default field names? (y/n)");
            var defaultFieldNames = Console.ReadLine();
            string begBatesFieldName;
            string begAttachFieldName;
            string endAttachFieldName;
            string fileExtensionFieldName;
            string emailSubjectFieldName;
            string docTitleFieldName;
            string origEFileSourceFieldname;
            string sourceFilePathFieldname;
            if (Regex.IsMatch(defaultFieldNames, "y", RegexOptions.IgnoreCase))
            {
                begBatesFieldName = "Production Bates Begin";
                begAttachFieldName = "Production Bates Attach Begin";
                endAttachFieldName = "Production Bates Attach End";
                fileExtensionFieldName = "File Extension";
                emailSubjectFieldName = "Email Subject";
                docTitleFieldName = "Doc Title";
                origEFileSourceFieldname = "Original E-File Source";
                sourceFilePathFieldname = "Source File Path";
            }
            else
            {
                Console.WriteLine("\nEnter beg bates field name.");
                begBatesFieldName = Console.ReadLine();
                Console.WriteLine("\nEnter beg attach field name.");
                begAttachFieldName = Console.ReadLine();
                Console.WriteLine("\nEnter end attach field name.");
                endAttachFieldName = Console.ReadLine();
                Console.WriteLine("\nEnter file extension field name.");
                fileExtensionFieldName = Console.ReadLine();
                Console.WriteLine("\nEnter email subject field name.");
                emailSubjectFieldName = Console.ReadLine();
                Console.WriteLine("\nEnter document title field name.");
                docTitleFieldName = Console.ReadLine();
                Console.WriteLine("\nEnter original e-file source field name.");
                origEFileSourceFieldname = Console.ReadLine();
                Console.WriteLine("\nEnter source file path field name.");
                sourceFilePathFieldname = Console.ReadLine();
            }
            var familyList = new List<Family>();
            df.GetNextRecord();
            while (!df.EndOfFile)
            {
                var begBates = df.GetFieldByName(begBatesFieldName);
                var begAttach = df.GetFieldByName(begAttachFieldName);
                var endAttach = df.GetFieldByName(endAttachFieldName);
                var fileExtension = df.GetFieldByName(fileExtensionFieldName);
                var emailSubject = df.GetFieldByName(emailSubjectFieldName);
                var docTitle = df.GetFieldByName(docTitleFieldName);
                var origEFileSource = df.GetFieldByName(origEFileSourceFieldname);
                var sourceFilePath = df.GetFieldByName(sourceFilePathFieldname);
                var doc = new Document(begBates, begAttach, endAttach, fileExtension, emailSubject, docTitle,
                    origEFileSource, sourceFilePath);
                if (begBates.Equals(begAttach))
                {
                    var family = new Family {FamilyID = begAttach};
                    family.Documents.Add(doc);
                    familyList.Add(family);
                }
                else
                {
                    var family = familyList.FirstOrDefault(f => f.FamilyID.Equals(begAttach));
                    if (family == null)
                    {
                        throw new InvalidDataException("No matching family found.");
                    }
                    family.Documents.Add(doc);
                }
                df.GetNextRecord();
            }
            using (var oStr = new StreamWriter(@"c:\temp\child_bates_test.csv", false, Encoding.ASCII))
            {
                oStr.AutoFlush = true;
                oStr.WriteLine("\"ProdBegBates\",\"ParentBates\",\"ChildBates\",\"FamilyRange\",\"SecSubject\",\"Path\",\"IntFilePath\"");
                var sortedFamilyList = familyList.OrderBy(f => f.FamilyID);
                foreach (var family in sortedFamilyList)
                {
                    family.SetChildBates();
                    family.SetParentBates();
                    var sortedDocuments = family.Documents.OrderBy(d => d.ProdBegBates);
                    foreach (var doc in sortedDocuments)
                    {
                        doc.SetFamilyRange();
                        doc.SetSecSubject();
                        var parent = family.Documents.FirstOrDefault(d => d.ProdBegBates.Equals(family.FamilyID));
                        if (parent == null)
                        {
                            throw new InvalidOperationException("No parent found.");
                        }
                        doc.SetFinalPaths(parent.FileExtension);
                        var strB = new StringBuilder();
                        strB.Append(string.Format("\"{0}\",", doc.ProdBegBates));
                        strB.Append(string.Format("\"{0}\",", doc.ParentBates));
                        strB.Append(string.Format("\"{0}\",", doc.ChildBates));
                        strB.Append(string.Format("\"{0}\",", doc.FamilyRange));
                        strB.Append(string.Format("\"{0}\",", doc.SecSubject));
                        strB.Append(string.Format("\"{0}\",", doc.Path));
                        strB.Append(string.Format("\"{0}\"", doc.IntFilePath));
                        oStr.WriteLine(strB.ToString());
                    }
                }
            }
        }
    }
}
