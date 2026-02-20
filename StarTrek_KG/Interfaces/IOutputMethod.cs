
using System.Collections.Generic;

namespace StarTrek_KG.Interfaces
{
    /// <summary>
    /// Classes that implement this will be called upon to write text to the screen in some fashion
    /// </summary>
    public interface IOutputMethod: IQueue
    {
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
