using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace DutyTracker;

public static class Sheets
{
    public static readonly ExcelSheet<TerritoryType> TerritorySheet;

    static Sheets()
    {
        TerritorySheet = DutyTracker.Data.Excel.GetSheet<TerritoryType>()!;
    }
}
