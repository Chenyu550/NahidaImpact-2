using System.Diagnostics.CodeAnalysis;
using NahidaImpact.Gameserver.Network;

namespace NahidaImpact.Gameserver.Controllers.Result;
internal class SinglePacketResult(NetPacket? packet) : IResult
{
    private NetPacket? _packet = packet;

    public bool NextPacket([MaybeNullWhen(false)] out NetPacket packet)
    {
        packet = _packet;
        _packet = null;

        return packet != null;
    }
}
