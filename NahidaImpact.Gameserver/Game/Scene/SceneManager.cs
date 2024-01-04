using System.Numerics;
using NahidaImpact.Common.Data.Binout;
using NahidaImpact.Gameserver.Controllers;
using NahidaImpact.Gameserver.Game.Avatar;
using NahidaImpact.Gameserver.Game.Entity;
using NahidaImpact.Gameserver.Game.Entity.Factory;
using NahidaImpact.Gameserver.Network.Session;
using NahidaImpact.Protocol;

namespace NahidaImpact.Gameserver.Game.Scene;
internal class SceneManager(NetSession session, Player player, EntityManager entityManager, EntityFactory entityFactory, BinDataCollection binData)
{
    public uint EnterToken { get; private set; }
    public uint CurrentSceneId => _sceneId;

    private readonly BinDataCollection _binData = binData;

    private readonly NetSession _session = session;
    private readonly Player _player = player;
    private readonly EntityManager _entityManager = entityManager;
    private readonly EntityFactory _entityFactory = entityFactory;

    private readonly List<AvatarEntity> _teamAvatars = [];

    private uint _enterTokenSeed;
    private uint _sceneId;
    private ulong _beginTime;

    private SceneEnterState _enterState;

    public async ValueTask OnEnterStateChanged(SceneEnterState changedToState)
    {
        if (_enterState is SceneEnterState.None or SceneEnterState.Complete)
            throw new InvalidOperationException($"SceneManager::OnEnterStateChanged called when enter state is {_enterState}!");

        if (_enterState > changedToState)
            throw new ArgumentException($"SceneManager::OnEnterStateChanged - requested state is less than current! (curr={_enterState}, req={changedToState})");

        if (_enterState + 1 != changedToState)
            throw new ArgumentException($"SceneManager::OnEnterStateChanged - trying to skip enter state! (curr={_enterState}, req={changedToState})");

        _enterState = changedToState;
        switch (_enterState)
        {
            case SceneEnterState.ReadyToEnter:
                await OnReadyToEnterScene();
                break;
            case SceneEnterState.InitFinished:
                await OnSceneInitFinished();
                break;
            case SceneEnterState.EnterDone:
                await OnEnterDone();
                break;
            case SceneEnterState.PostEnter:
                await OnPostEnter();
                break;
        }

        if (_enterState == SceneEnterState.PostEnter)
            _enterState = SceneEnterState.Complete;
    }

    public async ValueTask ReplaceCurrentAvatarAsync(ulong replaceToGuid)
    {
        // TODO: add logic checks.

        AvatarEntity avatar = _teamAvatars.Find(a => a.GameAvatar.Guid == replaceToGuid) 
            ?? throw new ArgumentException($"ReplaceCurrentAvatar: avatar with guid {replaceToGuid} not in team!");

        await _entityManager.SpawnEntityAsync(avatar, VisionType.Replace);
    }

    public async ValueTask ChangeTeamAvatarsAsync(ulong[] guidList)
    {
        _teamAvatars.Clear();

        foreach (ulong guid in guidList)
        {
            GameAvatar gameAvatar = _player.Avatars.Find(avatar => avatar.Guid == guid)!;

            AvatarEntity avatarEntity = _entityFactory.CreateAvatar(gameAvatar, _player.Uid);
            avatarEntity.SetPosition(2336.789f, 249.98896f, -751.3081f);

            _teamAvatars.Add(avatarEntity);
        }

        await SendSceneTeamUpdate();
        await _entityManager.SpawnEntityAsync(_teamAvatars[0], VisionType.Born);
    }

    private async ValueTask OnEnterDone()
    {
        await _entityManager.SpawnEntityAsync(_teamAvatars[0], VisionType.Born);
    }

    private async ValueTask OnSceneInitFinished()
    {
        GameAvatarTeam avatarTeam = _player.GetCurrentTeam();

        foreach (ulong guid in avatarTeam.AvatarGuidList)
        {
            GameAvatar gameAvatar = _player.Avatars.Find(avatar => avatar.Guid == guid)!;

            AvatarEntity avatarEntity = _entityFactory.CreateAvatar(gameAvatar, _player.Uid);
            avatarEntity.SetPosition(2336.789f, 249.98896f, -751.3081f);

            _teamAvatars.Add(avatarEntity);
        }

        await SendEnterSceneInfo();
        await SendSceneTeamUpdate();
    }

    private async ValueTask OnReadyToEnterScene()
    {
        await _session.NotifyAsync(CmdType.EnterScenePeerNotify, new EnterScenePeerNotify
        {
            DestSceneId = _sceneId,
            EnterSceneToken = EnterToken,
            HostPeerId = 1, // TODO: Scene peers
            PeerId = 1
        });
    }

    private ValueTask OnPostEnter()
    {
        return ValueTask.CompletedTask;
    }

    public async ValueTask EnterSceneAsync(uint sceneId)
    {
        if (_beginTime != 0) ResetState();

        _beginTime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds();
        _sceneId = sceneId;
        EnterToken = ++_enterTokenSeed;

        _enterState = SceneEnterState.EnterRequested;
        await _session.NotifyAsync(CmdType.PlayerEnterSceneNotify, new PlayerEnterSceneNotify
        {
            SceneBeginTime = _beginTime,
            SceneId = _sceneId,
            SceneTransaction = CreateTransaction(_sceneId, _player.Uid, _beginTime),
            Pos = new()
            {
                X = 2191.16357421875f,
                Y = 214.65115356445312f,
                Z = -1120.633056640625f
            },
            TargetUid = _player.Uid,
            EnterSceneToken = EnterToken,
            PrevPos = new(),
            Type = EnterType.Self
        });
    }

    private async ValueTask SendSceneTeamUpdate()
    {
        SceneTeamUpdateNotify sceneTeamUpdate = new();
        foreach (AvatarEntity avatar in _teamAvatars)
        {
            sceneTeamUpdate.SceneTeamAvatarList.Add(new SceneTeamAvatar
            {
                SceneEntityInfo = avatar.AsInfo(),
                WeaponEntityId = avatar.WeaponEntityId,
                PlayerUid = _player.Uid,
                WeaponGuid = avatar.GameAvatar.WeaponGuid,
                EntityId = avatar.EntityId,
                AvatarGuid = avatar.GameAvatar.Guid,
                AbilityControlBlock = avatar.BuildAbilityControlBlock(_binData),
                SceneId = _sceneId
            });
        }

        await _session.NotifyAsync(CmdType.SceneTeamUpdateNotify, sceneTeamUpdate);
    }

    private async ValueTask SendEnterSceneInfo()
    {
        PlayerEnterSceneInfoNotify enterSceneInfo = new()
        {
            CurAvatarEntityId = _teamAvatars[0].EntityId,
            EnterSceneToken = EnterToken,
            MpLevelEntityInfo = new MPLevelEntityInfo
            {
                EntityId = 184549377,
                AbilityInfo = new AbilitySyncStateInfo(),
                AuthorityPeerId = 1
            },
            TeamEnterInfo = new TeamEnterSceneInfo
            {
                TeamEntityId = 150994946,
                AbilityControlBlock = new AbilityControlBlock(),
                TeamAbilityInfo = new AbilitySyncStateInfo()
            }
        };

        foreach (AvatarEntity avatar in _teamAvatars)
        {
            enterSceneInfo.AvatarEnterInfo.Add(new AvatarEnterSceneInfo
            {
                AvatarGuid = avatar.GameAvatar.Guid,
                AvatarEntityId = avatar.EntityId,
                WeaponEntityId = avatar.WeaponEntityId,
                WeaponGuid = avatar.GameAvatar.WeaponGuid
            });
        }

        await _session.NotifyAsync(CmdType.PlayerEnterSceneInfoNotify, enterSceneInfo);
    }

    private void ResetState()
    {
        _teamAvatars.Clear();
        _entityManager.Reset();
    }

    private static string CreateTransaction(uint sceneId, uint playerUid, ulong beginTime)
        => string.Format("{0}-{1}-{2}-13830", sceneId, playerUid, beginTime);
}
