using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerateSECOverlay
{
    class Family
    {
        #region Constructors
        public Family()
        {
           Documents = new List<Document>(); 
        }

        #endregion

        #region Properties
        public string FamilyID { get; set; }

        public List<Document> Documents { get; set; }
        #endregion

        #region Methods

        public void SetParentBates()
        {
            var children = Documents.Where(d => !d.ProdBegBates.Equals(FamilyID));
            foreach (var child in children)
            {
                child.ParentBates = FamilyID;
            }
        }

        public void SetChildBates()
        {
            var children = Documents.Where(d => !d.ProdBegBates.Equals(FamilyID));
            var strB = new StringBuilder();
            foreach (var child in children)
            {
                strB.Append(string.Format("{0};", child.ProdBegBates));
            }
            var parent = Documents.FirstOrDefault(d => d.ProdBegBates.Equals(FamilyID));
            if (parent == null)
            {
                throw new InvalidDataException("No parent found in family.");
            }
            if (strB.ToString().EndsWith(";")) strB = strB.Remove(strB.Length - 1, 1);
            parent.ChildBates = strB.ToString();
        }

        #endregion
    }
}
