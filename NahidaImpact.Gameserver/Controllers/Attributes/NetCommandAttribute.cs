using NahidaImpact.Protocol;

namespace NahidaImpact.Gameserver.Controllers.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal class NetCommandAttribute(CmdType cmdType) : Attribute
{
    public CmdType CmdType { get; } = cmdType;
}
