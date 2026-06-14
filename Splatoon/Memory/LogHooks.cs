using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Memory;
using ECommons.ExcelServices;
using ECommons.EzHookManager;
using ECommons.MathHelpers;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Splatoon.Memory;

public unsafe class LogHooks
{
    private LogHooks()
    {
        // porting-note: EzSignatureHelper.Initialize would scan for "40 53 57 48 81 EC ?? ?? ?? ?? 48 8B FA 8B D1"
        // (game 7.5 ActorCast hook); this sig has no game-7.1 equivalent that we've identified.
        // Skipping the init keeps Splatoon loading without the spammy "can't find signature" exception.
        // Side effect: ScriptingProcessor.OnStartingCast / Projection.LastCast won't update from ActorCast packets;
        // both can still be driven from the (working) IBattleChara cast-info polling path.
        // EzSignatureHelper.Initialize(this);
    }

    private delegate nint ActorCastDelegate(uint sourceId, nint packetPtr);

    // [EzHook("40 53 57 48 81 EC ?? ?? ?? ?? 48 8B FA 8B D1")]
    private EzHook<ActorCastDelegate> ActorCastHook;
    private nint ActorCastDetour(uint sourceId, nint packetPtr)
    {
        try
        {
            var packet = (PacketActorCast*)packetPtr;
            /*PluginLog.Debug($"""
                ActorCast:
                {ExcelActionHelper.GetActionName(packet->ActionID, true)}
                Rotation: {packet->RotationRadians} {packet->RotationRadians.RadToDeg()}
                {MemoryHelper.ReadRaw(packetPtr, sizeof(PacketActorCast)).ToHexString()}
                """);*/
            S.Projection.LastCast.GetOrCreate(sourceId)[packet->ActionDescriptor] = *packet;
            ScriptingProcessor.OnStartingCast(sourceId, packet);
        }
        catch(Exception e)
        {
            e.Log();
        }
        return ActorCastHook.Original(sourceId, packetPtr);
    }
}
