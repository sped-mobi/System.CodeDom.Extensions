using System.Text;

namespace System.CodeDom.CSharp
{
    internal class Indentation
    {
        private readonly CSharpTextWriter writer;

        private readonly int indent;

        private string s;

        internal Indentation(CSharpTextWriter writer, int indent)
        {
            this.writer = writer;
            this.indent = indent;
            s = null;
        }

        internal string IndentationString
        {
            get
            {
                if (s == null)
                {
                    string tabString = writer.TabString;
                    StringBuilder sb = new StringBuilder(indent * tabString.Length);
                    for (int i = 0; i < indent; i++)
                    {
                        sb.Append(tabString);
                    }
                    s = sb.ToString();
                }
                return s;
            }
        }
    }
}