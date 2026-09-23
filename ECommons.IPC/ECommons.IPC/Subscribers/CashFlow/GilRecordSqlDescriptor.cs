using System;
using System.Collections.Generic;
using System.Text;
// using TerraFX.Interop.Windows; // porting-note(api13): unused import; this csproj does not reference TerraFX

namespace ECommons.IPC.Subscribers.CashFlow;

public unsafe class GilRecordSqlDescriptor
{
    public long GilPlayer { get; set; }
    public long GilRetainer { get; set; }
    public long UnixTime { get; set; }
    public ulong CidUlong { get; set; }
}
