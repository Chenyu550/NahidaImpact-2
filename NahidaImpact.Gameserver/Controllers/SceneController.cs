using Google.Protobuf;
using NahidaImpact.Gameserver.Controllers.Attributes;
using NahidaImpact.Gameserver.Controllers.Result;
using NahidaImpact.Gameserver.Game;
using NahidaImpact.Gameserver.Game.Scene;
using NahidaImpact.Gameserver.Helpers;
using NahidaImpact.Gameserver.Network.Session;
using NahidaImpact.Protocol;

namespace NahidaImpact.Gameserver.Controllers;

[NetController]
internal class SceneController : ControllerBase
{
    [NetCommand(CmdType.EvtDoSkillSuccNotify)]
    public async ValueTask<IResult> OnEvtDoSkillSuccNotify(SceneManager sceneManager)
    {
        EvtDoSkillSuccNotify notify = Packet!.DecodeBody<EvtDoSkillSuccNotify>();
        await sceneManager.ResetAllCoolDownsForAvatar(notify.CasterId);

        return Ok();
    }

    [NetCommand(CmdType.CombatInvocationsNotify)]
    public async ValueTask<IResult> OnCombatInvocationsNotify(ClientActionManager clientActionManager)
    {
        CombatInvocationsNotify notify = Packet!.DecodeBody<CombatInvocationsNotify>();
        foreach (CombatInvokeEntry invocation in notify.InvokeList)
        {
            IMessage? message = invocation.DecodeCombatInvocation();
            if (message != null)
                await clientActionManager.InvokeAsync(invocation.ArgumentType, message);
        }

        return Ok();
    }

    [NetCommand(CmdType.UnionCmdNotify)]
    public async ValueTask<IResult> OnUnionCmdNotify(NetSession session)
    {
        UnionCmdNotify notify = Packet!.DecodeBody<UnionCmdNotify>();

        foreach (UnionCmd cmd in notify.CmdList)
        {
            await session.HandlePacket(cmd.ToNetPacket());
        }

        return Ok();
    }

    [NetCommand(CmdType.GetScenePointReq)]
    public ValueTask<IResult> OnGetScenePointReq(SceneManager sceneManager, Player player)
    {
        GetScenePointRsp rsp = new()
        {
            SceneId = sceneManager.CurrentSceneId,
            BelongUid = player.Uid
        };

        for (uint i = 1; i <= 777; i++)
        {
            rsp.UnlockedPointList.Add(i);
        }

        return ValueTask.FromResult(Response(CmdType.GetScenePointRsp, rsp));
    }

    [NetCommand(CmdType.GetSceneAreaReq)]
    public ValueTask<IResult> OnGetSceneAreaReq(SceneManager sceneManager)
    {
        GetSceneAreaRsp rsp = new()
        {
            SceneId = sceneManager.CurrentSceneId
        };

        for (uint i = 1; i <= 100; i++)
        {
            rsp.AreaIdList.Add(i);
        }

        return ValueTask.FromResult(Response(CmdType.GetSceneAreaRsp, rsp));
    }

    [NetCommand(CmdType.PostEnterSceneReq)]
    public async ValueTask<IResult> OnPostEnterSceneReq(SceneManager sceneManager)
    {
        await sceneManager.OnEnterStateChanged(SceneEnterState.PostEnter);

        return Response(CmdType.PostEnterSceneRsp, new PostEnterSceneRsp
        {
            EnterSceneToken = sceneManager.EnterToken
        });
    }

    [NetCommand(CmdType.EnterSceneDoneReq)]
    public async ValueTask<IResult> OnEnterSceneDoneReq(SceneManager sceneManager)
    {
        await sceneManager.OnEnterStateChanged(SceneEnterState.EnterDone);

        return Response(CmdType.EnterSceneDoneRsp, new EnterSceneDoneRsp
        {
            EnterSceneToken = sceneManager.EnterToken
        });
    }

    [NetCommand(CmdType.SceneInitFinishReq)]
    public async ValueTask<IResult> OnSceneInitFinishReq(SceneManager sceneManager)
    {
        await sceneManager.OnEnterStateChanged(SceneEnterState.InitFinished);

        return Response(CmdType.SceneInitFinishRsp, new SceneInitFinishRsp
        {
            EnterSceneToken = sceneManager.EnterToken
        });
    }

    [NetCommand(CmdType.EnterSceneReadyReq)]
    public async ValueTask<IResult> OnEnterSceneReadyReq(SceneManager sceneManager)
    {
        await sceneManager.OnEnterStateChanged(SceneEnterState.ReadyToEnter);

        return Response(CmdType.EnterSceneReadyRsp, new EnterSceneReadyRsp
        {
            EnterSceneToken = sceneManager.EnterToken
        });
    }
}
