using System;
using System.Collections.Generic;
using System.Text;

namespace gView.Framework.system
{
    public class Tokenizer
    {
        private Token _parentToken;
        private List<string> _strings = new List<string>();
        private char _qouteChar = '\'';

        public Tokenizer(string text)
            : this(text, '(', ')')
        {
        }
        public Tokenizer(string text, char opener, char closer)
        {
            _parentToken = new Token(RemoveStrings(text), opener, closer);
        }

        public Token Token
        {
            get
            {
                return _parentToken;
            }
        }

        public delegate void TransformTokenEventHandler(ref string text);
        public event TransformTokenEventHandler OnTransformToken = null;
        public string Transform()
        {
            return AddStrings(TransformToken(_parentToken));
        }
        public string TransformXML()
        {
            return AddStringsXML(TransformToken(_parentToken));
        }
        private string TransformToken(Token token)
        {
            if (token == null || token.Text == String.Empty) return String.Empty;

            string text = token.Text;
            if (OnTransformToken != null) OnTransformToken(ref text);

            for (int i = 0; i < token.ChildTokens.Count; i++)
            {
                text = text.Replace("[" + i + "]", TransformToken(token.ChildTokens[i]));
            }

            return text;
        }

        private string RemoveStrings(string text)
        {
            #region Parse Quotes
            
            Token.SubStringIndices index = null;
            List<Token.SubStringIndices> indeces = new List<Token.SubStringIndices>();

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == _qouteChar)
                {
                    if (index == null)
                    {
                        index = new Token.SubStringIndices(i);
                    }
                    else
                    {
                        index.to = i;
                        indeces.Add(index);
                        index = null;
                    }
                }
            }
            if (index != null) throw new Exception("Syntax error...");
            #endregion

            #region Cut Strings
            if (indeces.Count > 0)
            {
                indeces.Add(new Token.SubStringIndices(text.Length, text.Length));
                string t = String.Empty;
                for (int i = 0; i < indeces.Count - 1; i++)
                {
                    if (i == 0)
                        t += text.Substring(0, indeces[i].from);
                    t += _qouteChar + "[STRING" + i + "]" + _qouteChar;
                    t += text.Substring(indeces[i].to + 1, indeces[i + 1].from - indeces[i].to - 1);


                    string str = text.Substring(indeces[i].from + 1, indeces[i].to - indeces[i].from - 1);
                    _strings.Add(str);
                }

                text = t;
            }
            #endregion

            return text;
        }
        private string AddStrings(string text)
        {
            for (int i = 0; i < _strings.Count; i++)
            {
                text = text.Replace("[STRING" + i + "]", _strings[i]);
            }
            return text;
        }
        private string AddStringsXML(string text)
        {
            for (int i = 0; i < _strings.Count; i++)
            {
                string str = _strings[i];
                str = str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
                text = text.Replace("[STRING" + i + "]", str);
            }
            return text;
        }
    }

    public class Token
    {
        private List<Token> _childTokens = new List<Token>();
        private string _text = "";
        private char _opener, _closer;

        public Token(string text)
            : this(text, '(', ')')
        {
            
        }

        public Token(string text, char opener, char closer)
        {
            _text = text;
            _opener = opener;
            _closer = closer;

            Tokenize();
        }

        public List<Token> ChildTokens
        {
            get { return _childTokens; }
        }
        public string Text { get { return _text; } }

        private void Tokenize()
        {
            string text = _text.Trim();

            #region Parse opener, closer Indices
            int pos = 0;
            SubStringIndices index = null;
            List<SubStringIndices> indeces = new List<SubStringIndices>();

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == _opener)
                {
                    pos++;
                    if (pos == 1)
                    {
                        if (index != null) throw new Exception("Syntax error...");
                        index = new SubStringIndices(i);
                    }
                }
                else if (text[i] == _closer)
                {
                    pos--;
                    if (pos == 0)
                    {
                        if (index == null) throw new Exception("Syntax error...");
                        index.to = i;
                        indeces.Add(index);
                        index = null;
                    }
                    else if (pos < 0) throw new Exception("Syntax error...");
                }
            }
            if (index != null) throw new Exception("Syntax error...");
            #endregion

            #region Cut Expressions
            if (indeces.Count > 0)
            {
                indeces.Add(new SubStringIndices(text.Length, text.Length));
                string t = String.Empty;
                for (int i = 0; i < indeces.Count - 1; i++)
                {
                    if (i == 0)
                        t += text.Substring(0, indeces[i].from);
                    t += "[" + i + "]";
                    t += text.Substring(indeces[i].to + 1, indeces[i + 1].from - indeces[i].to - 1);

                    string str = text.Substring(indeces[i].from + 1, indeces[i].to - indeces[i].from - 1);
                    _childTokens.Add(new Token(str));
                }

                text = t;
            }
            #endregion

            _text = text;
        }
        #region Helper Classes
        public class SubStringIndices
        {
            public int from, to;

            public SubStringIndices() { }
            public SubStringIndices(int f)
            {
                from = f;
            }
            public SubStringIndices(int f, int t) : this(f)
            {
                to = t;
            }
        }
        #endregion
    }
}
