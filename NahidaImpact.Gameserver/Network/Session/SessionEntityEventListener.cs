using NahidaImpact.Gameserver.Game.Entity;
using NahidaImpact.Gameserver.Game.Entity.Listener;
using NahidaImpact.Protocol;

namespace NahidaImpact.Gameserver.Network.Session;
internal class SessionEntityEventListener(NetSession session) : IEntityEventListener
{
    private readonly NetSession _session = session;

    public async ValueTask OnEntitySpawned(SceneEntity entity, VisionType visionType)
    {
        await _session.NotifyAsync(CmdType.SceneEntityAppearNotify, new SceneEntityAppearNotify
        {
            AppearType = visionType,
            EntityList = { entity.AsInfo() }
        });
    }
}
