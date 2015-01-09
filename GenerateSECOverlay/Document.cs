using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GenerateSECOverlay
{
    internal class Document
    {
        #region Constructor

        public Document(string prodBegBates, string prodBegAttach, string prodEndAttach, string fileExtension,
            string emailSubject, string docTitle, string origEFileSource, string sourceFilePath)
        {
            ProdBegBates = prodBegBates;
            ProdBegAttach = prodBegAttach;
            ProdEndAttach = prodEndAttach;
            FileExtension = fileExtension;
            EmailSubject = emailSubject;
            DocTitle = docTitle;
            OrigEFileSource = origEFileSource;
            SourceFilePath = sourceFilePath;
        }

        #endregion

        #region Imported Properties

        public string ProdBegBates { get; set; }
        public string ProdBegAttach { get; set; }
        public string ProdEndAttach { get; set; }
        public string FileExtension { get; set; }
        public string EmailSubject { get; set; }
        public string DocTitle { get; set; }
        public string OrigEFileSource { get; set; }
        public string SourceFilePath { get; set; }

        #endregion

        #region CalculatedProperties

        //calculated
        public string FamilyRange { get; private set; }

        //calculated
        public string ParentBates { get; set; }

        //calculated
        public string ChildBates { get; set; }
        //calculated
        public string SecSubject { get; set; }
        //calculated
        public string Path { get; set; }

        //calculated
        public string IntFilePath { get; set; }

        #endregion

        #region Methods

        public void SetFamilyRange()
        {
            if (string.IsNullOrEmpty(ProdBegAttach) || string.IsNullOrEmpty(ProdEndAttach))
            {
                throw new InvalidOperationException("Missing Beg Attach or End Attach information.");
            }
            FamilyRange = string.Concat(ProdBegAttach, "-", ProdEndAttach);
        }

        public void SetSecSubject()
        {
            if (!string.IsNullOrEmpty(EmailSubject) && !string.IsNullOrEmpty(DocTitle))
            {
                Console.WriteLine("Specify value to use: 1. Email Subject: {0} or 2. Doc Title: {1}", EmailSubject, DocTitle);
                var choice = Console.ReadLine();
                if (Regex.IsMatch(choice, "1"))
                {
                    SecSubject = EmailSubject;
                    return;
                }
                if (Regex.IsMatch(choice, "2"))
                {
                    SecSubject = DocTitle;
                    return;
                }
            }
            if (!string.IsNullOrEmpty(EmailSubject))
            {
                SecSubject = EmailSubject;
                return;
            }
            if (!string.IsNullOrEmpty((DocTitle)))
            {
                SecSubject = DocTitle;
            }
        }

        public void SetFinalPaths(string parentBatesFileExtension)
        {
            if (string.IsNullOrEmpty(OrigEFileSource) && string.IsNullOrEmpty(SourceFilePath))
            {
                return;
            }
            if (!string.IsNullOrEmpty(OrigEFileSource) && string.IsNullOrEmpty(SourceFilePath))
            {
                Path = OrigEFileSource;
                return;
            }
            if (string.IsNullOrEmpty(OrigEFileSource) && !string.IsNullOrEmpty(SourceFilePath))
            {
                IntFilePath = SourceFilePath;
                return;
            }
            if (ProdBegAttach.Equals(ProdBegBates) && Regex.IsMatch(FileExtension, ".msg.", RegexOptions.IgnoreCase))
            {
                IntFilePath = SourceFilePath;
                return;
            }
            if (Regex.IsMatch(parentBatesFileExtension, ".msg.", RegexOptions.IgnoreCase))
            {
                IntFilePath = SourceFilePath;
                return;
            }
            Path = OrigEFileSource;
        }

        #endregion
    }
}