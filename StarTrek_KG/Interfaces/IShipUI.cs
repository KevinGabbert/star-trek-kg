using System.Collections.Generic;

namespace StarTrek_KG.Interfaces
{
    public interface IShipUI
    {
        void ResetPrompt();
        void OutputLine(string textToOutput);
        void ClearOutputQueue();

        string GetSubCommand();
        List<string> OutputQueue();
    }
}
