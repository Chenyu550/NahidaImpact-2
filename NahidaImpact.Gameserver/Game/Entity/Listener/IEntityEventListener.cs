using NahidaImpact.Protocol;

namespace NahidaImpact.Gameserver.Game.Entity.Listener;
internal interface IEntityEventListener
{
    ValueTask OnEntitySpawned(SceneEntity entity, VisionType visionType);
    ValueTask OnAvatarFightPropChanged(AvatarEntity entity, uint key, float value);
}
