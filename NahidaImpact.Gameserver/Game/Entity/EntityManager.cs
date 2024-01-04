using NahidaImpact.Gameserver.Game.Entity.Listener;
using NahidaImpact.Protocol;

namespace NahidaImpact.Gameserver.Game.Entity;
internal class EntityManager(IEntityEventListener listener)
{
    private readonly List<SceneEntity> _entities = [];
    private readonly IEntityEventListener _listener = listener;

    public async ValueTask SpawnEntityAsync(SceneEntity entity, VisionType visionType)
    {
        _entities.Add(entity);
        await _listener.OnEntitySpawned(entity, visionType);
    }

    public void Reset()
    {
        _entities.Clear();
    }
}
