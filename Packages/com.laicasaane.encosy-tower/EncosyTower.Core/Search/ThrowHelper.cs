using System.Runtime.CompilerServices;
using EncosyTower.Logging;

namespace EncosyTower.Search
{
    static class ThrowHelper
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void LogErrorIfFuzzySharpIsNotInstalled()
        {
            StaticDevLogger.LogError(
                "Please install Raffinert.FuzzySharp plugin via one of these methods:\n" +
                "1. Open window <a href=\"\\menu:Encosy Tower/Project Settings/Features\" " +
                    "router=\"encosy-tower\">Encosy Tower/Project Settings/Features</a>, then go to page " +
                    "\"<b>6. Encosy Tower: Search</b>\" " +
                    "and install all required packages.\n" +
                "2. Open window <a href=\"\\open:Project/Player\" router=\"encosy-tower\">" +
                    "Project Settings/Player</a>, then go to <b>Script Compilation</b> and add symbol " +
                    "<b>ENCOSY_RAFFINERT_FUZZYSHARP</b> to the list."
            );
        }
    }
}
