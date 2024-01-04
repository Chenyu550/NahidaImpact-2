using Google.Protobuf;
using NahidaImpact.Gameserver.Network;
using NahidaImpact.Protocol;

namespace NahidaImpact.Gameserver.Helpers;
internal static class UnionCmdHelper
{
    public static IMessage? DecodeCombatInvocation(this CombatInvokeEntry invocation)
    {
        return invocation.ArgumentType switch
        {
            CombatTypeArgument.EntityMove => EntityMoveInfo.Parser.ParseFrom(invocation.CombatData),
            _ => null
        };
    }

    public static NetPacket ToNetPacket(this UnionCmd cmd)
    {
        return new NetPacket()
        {
            CmdType = (CmdType)cmd.MessageId,
            Body = cmd.Body.Memory
        };
    }
}
