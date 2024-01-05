using NahidaImpact.Gameserver.Controllers.Result;
using NahidaImpact.Gameserver.Network;
using NahidaImpact.Protocol;
using Google.Protobuf;

namespace NahidaImpact.Gameserver.Controllers;
internal abstract class ControllerBase
{
    public NetPacket? Packet { get; set; }

    protected IResult Ok()
    {
        return new SinglePacketResult(null);
    }

    protected IResult Response<TMessage>(CmdType cmdType, TMessage message) where TMessage : IMessage
    {
        return new SinglePacketResult(new()
        {
            CmdType = cmdType,
            Head = Memory<byte>.Empty,
            Body = message.ToByteArray()
        });
    }

    protected IResult Response(CmdType cmdType)
    {
        return new SinglePacketResult(new()
        {
            CmdType = cmdType,
            Head = Memory<byte>.Empty,
            Body = Memory<byte>.Empty
        });
    }
}
