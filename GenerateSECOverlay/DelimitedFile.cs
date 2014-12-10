using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace GenerateSECOverlay
{
    class DelimitedFile
    {
        #region Constructors
        //optionally specify the delimiters to use
        public DelimitedFile(string path, string encoding, char recordDelimiter = '\n', char fieldDelimiter = (char)20, char multiValueDelimiter = (char)59, char quote = (char)254)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Null or empty path.", path);
            if (!File.Exists(path)) throw new ArgumentException(string.Format("File {0} does not exist.", path));

            var encodingObj = ValidateEncodingString(encoding);

            _str = new StreamReader(path, encodingObj);

            RecordDelimiter = recordDelimiter;
            FieldDelimiter = fieldDelimiter;
            MultiValueDelimiter = multiValueDelimiter;
            Quote = quote;

            GetNextRecord();
            HeaderRecord = CurrentRecord;
        }


        #endregion


        #region Properties
        public char RecordDelimiter { get; private set; }

        public char FieldDelimiter { get; private set; }

        public char MultiValueDelimiter { get; private set; }

        public char Quote { get; private set; }

        public bool EndOfFile { get; private set; }

        private StreamReader _str { get; set; }

        public IEnumerable<string> CurrentRecord { get; private set; }

        public IEnumerable<string> HeaderRecord { get; private set; }

        #endregion

        //Get a particular field from the record by its zero-indexed position
        public string GetFieldByPosition(int position)
        {
            if (position < 0 || position >= HeaderRecord.Count())
                throw new ArgumentOutOfRangeException(position.ToString(CultureInfo.InvariantCulture), "Position is out of range.");

            return CurrentRecord.ElementAt(position);
        }

        //Get a particular field from the record by its name from the header row
        public string GetFieldByName(string name)
        {
            //do a case-insensitive matching
            var lName = name.ToLower(CultureInfo.InvariantCulture);
            
            var index = HeaderRecord.ToList().FindIndex(s => s.ToLower(CultureInfo.InvariantCulture).Equals(lName));
            if (index < 0) throw new ApplicationException(string.Format("\"{0}\" is not a valid column name.", name));
            return GetFieldByPosition(index);
        }
        
        //Get the next record in the file
        public void GetNextRecord()
        {
            var line = _str.ReadLine();
            if (_str.EndOfStream)
            {
                EndOfFile = true;
                return;
            }

            if (string.IsNullOrEmpty(line)) throw new ApplicationException("Empty line.");

            char[] delimiter = { FieldDelimiter };
            var record = line.Split(delimiter).AsEnumerable();

            if (record == null) throw new ApplicationException(string.Format("No fields found in {0}", line));

            List<string> nakedRecordList = new List<string>();

            var q = Quote.ToString(CultureInfo.InvariantCulture);

            var currentRecord = record as IList<string> ?? record.ToList();
            nakedRecordList.AddRange(currentRecord.Select(s => s.Replace(q, "")));

            CurrentRecord = nakedRecordList;
        }

        //Validate encoding string argument and return an Encoding object
        private Encoding ValidateEncodingString(string encoding)
        {
            //make the string case insensitive
            var lEncoding = encoding.ToLower(CultureInfo.InvariantCulture);
            
            EncodingInfo encodingInfo = null;
            try
            {
                encodingInfo = Encoding.GetEncodings().Single(e => e.Name.Contains(lEncoding));
            }
            catch (InvalidOperationException e)
            {
                throw new InvalidOperationException(string.Format("{0} is not a valid encoding", encoding));
            }

            return Encoding.GetEncoding(encodingInfo.Name);
        }
    }
}
