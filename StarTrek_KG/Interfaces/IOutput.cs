﻿
namespace StarTrek_KG.Interfaces
{
    public interface IOutput
    {
        void Clear();

        string ReadLine();

        void Write(string text);
        void WriteLine();
        void WriteLine(string text);
        void WriteLine(string text, object text2);
        void WriteLine(string text, object text2, object text3);
        void WriteLine(string text, object text2, object text3, object text4);

        void HighlightTextBW(bool on);
    }
}
