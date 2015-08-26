
using System.Collections.Generic;

namespace StarTrek_KG.Interfaces
{
    public interface IOutput
    {
        Queue<string> OutputQueue { get; set; }

        void Clear();

        string ReadLine();

        void Write(string text);
        List<string> Write(List<string> textLines);

        string WriteLine();
        List<string> WriteLine(string text);
        void WriteLine(string text, object text2);
        void WriteLine(string text, object text2, object text3);
        void WriteLine(string text, object text2, object text3, object text4);

        void HighlightTextBW(bool on);
    }
}
