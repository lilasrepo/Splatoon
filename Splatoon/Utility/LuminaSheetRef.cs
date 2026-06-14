// porting-note: HEAD ECommons exposes static `Status.GetRef(uint)`, `BNpcName.GetRef(uint)` etc.
// Walk-back ECommons lacks these. Provide a generic helper on Splatoon side and rewrite the 2 leftover
// HEAD-style call sites that we couldn't just inline.

using Dalamud.Game;
using ECommons.DalamudServices;
using Lumina.Excel;

namespace Splatoon.Utility;

public static class LuminaSheetRef
{
    public static RowRef<T> GetRef<T>(uint id) where T : struct, IExcelRow<T>
        => new(Svc.Data.Excel, id, ToLuminaLang(Svc.Data.Language));

    private static Lumina.Data.Language? ToLuminaLang(ClientLanguage cl) => cl switch
    {
        ClientLanguage.Japanese => Lumina.Data.Language.Japanese,
        ClientLanguage.English => Lumina.Data.Language.English,
        ClientLanguage.German => Lumina.Data.Language.German,
        ClientLanguage.French => Lumina.Data.Language.French,
        _ => null,
    };
}
