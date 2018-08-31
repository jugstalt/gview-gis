using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace gView.Framework.Geometry.SpatialRefTranslation
{
    internal class WktStreamTokenizer : StreamTokenizer
    {
        public WktStreamTokenizer(TextReader reader)
            : base(reader, true)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
        }

        private bool _readNext=true;
        public void ReadToken(string expectedToken)
        {
            if(_readNext) this.NextToken();
            if (this.GetStringValue() != expectedToken)
                throw new ParseException(String.Format("Expecting comma ('{3}') but got a '{0}' at line {1} column {2}.",
                    this.GetStringValue(), this.LineNumber, this.Column, expectedToken));

            _readNext = true;
        }

        public bool TryReadToken(string expectedToken)
        {
            if(_readNext) this.NextToken();
            if (this.GetStringValue() != expectedToken)
            {
                return _readNext = false;
            }
            return _readNext = true;
        }

        public string ReadDoubleQuotedWord()
        {
            string word = String.Empty;
            ReadToken("\"");
            NextToken(false);
            while (GetStringValue() != String.Empty && GetStringValue() != "\"")
            {
                word = word + this.GetStringValue();
                NextToken(false);
            }
            return word;
        }

        public void ReadAuthority(ref string authority, ref string authorityCode)
        {
            //AUTHORITY["EPSG","9102"]]
            ReadToken("AUTHORITY");
            ReadToken("[");
            authority = this.ReadDoubleQuotedWord();
            ReadToken(",");
            authorityCode = this.ReadDoubleQuotedWord();
            ReadToken("]");
        }
    }
}
