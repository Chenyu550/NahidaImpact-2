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

    public async ValueTask ChangeAvatarFightPropAsync(uint entityId, uint key, float value)
    {
        if (GetEntityById(entityId) is AvatarEntity entity)
        {
            entity.SetFightProp(key, value);
            await _listener.OnAvatarFightPropChanged(entity, key, value);
        }
    }

    public SceneEntity? GetEntityById(uint id) => 
        _entities.Find(entity => entity.EntityId == id);

    public void Reset()
    {
        _entities.Clear();
    }
}
