using System.Diagnostics.CodeAnalysis;
using NahidaImpact.Gameserver.Network;

namespace NahidaImpact.Gameserver.Controllers.Result;
internal interface IResult
{
    bool NextPacket([MaybeNullWhen(false)] out NetPacket packet);
}
