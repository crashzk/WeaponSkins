using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Misc;
using SwiftlyS2.Shared.NetMessages;
using SwiftlyS2.Shared.Players;
using SwiftlyS2.Shared.ProtobufDefinitions;
using SwiftlyS2.Shared.SchemaDefinitions;

using WeaponSkins.Configuration;

namespace WeaponSkins.Services;

/// <summary>
/// Hooks <c>CCSUsrMsg_SendPlayerLoadout</c> to make players show the agent selected through
/// this plugin's menu to their teammates, instead of whatever agent Steam reports as their
/// real equipped item.
///
/// <para>
/// <b>Confirmed on a live server (2026-09-07):</b> this fixes the agent teammates see in the
/// world. It does <b>not</b> fix the buy menu's own character preview for the player who
/// picked the agent — that preview is rendered client-side straight from the local Steam
/// inventory cache and never goes through the server at all, so there is no server-side hook
/// (this one included) that can change it. That part is a CS2 client limitation, not a bug in
/// this plugin - the same limitation every other agent-changing CS2 plugin runs into. Don't
/// spend more time trying to "fix" the self buy-menu preview via netmessage hooks; it isn't
/// reachable from the server.
/// </para>
/// <para>
/// <see cref="HookInventoryUpdateService"/> applies an agent purely visually, by calling
/// <c>pawn.SetModel(...)</c> on the player's own pawn. That is enough to make the correct
/// model appear on the player *in the world*, but it never touches the data that CS2 uses to
/// render the agent to other clients on the same team - that's this message.
/// </para>
/// </summary>
public class AgentLoadoutFixService
{
    private static readonly ushort AgentLoadoutSlot = (ushort)loadout_slot_t.LOADOUT_SLOT_CLOTHING_CUSTOMPLAYER;

    private ISwiftlyCore Core { get; }
    private ILogger<AgentLoadoutFixService> Logger { get; }
    private DataService DataService { get; }
    private AgentFixConfig Config { get; set; }

    public AgentLoadoutFixService(ISwiftlyCore core,
        ILogger<AgentLoadoutFixService> logger,
        DataService dataService,
        IOptionsMonitor<MainConfigModel> options)
    {
        Core = core;
        Logger = logger;
        DataService = dataService;
        Config = options.CurrentValue.AgentFix;

        options.OnChange(model =>
        {
            Config = model.AgentFix;
        });

        Core.NetMessage.HookServerMessageInternal<CCSUsrMsg_SendPlayerLoadout>(OnSendPlayerLoadout);
    }

    private HookResult OnSendPlayerLoadout(CCSUsrMsg_SendPlayerLoadout msg,
        int recipientPlayerId)
    {
        try
        {
            var subject = Core.PlayerManager.GetPlayer(msg.Playerslot);
            if (subject is not { IsValid: true })
            {
                return HookResult.Continue;
            }

            var steamId = subject.SteamID;

            foreach (var item in msg.Loadout)
            {
                if (item.Slot != AgentLoadoutSlot) continue;

                var team = (Team)item.Team;

                if (DataService.AgentDataService.TryGetAgent(steamId, team, out var agentIndex))
                {
                    item.EconItem.Defindex = (uint)agentIndex;
                }
                else if (Config.EnforceManagedOnly)
                {
                    // No agent has been explicitly picked through the plugin for this
                    // player/team: blank the slot instead of letting the client fall back
                    // to whatever Steam reported (owned-but-not-equipped agents included).
                    item.EconItem.Defindex = 0;
                }
            }
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error in AgentLoadoutFixService.OnSendPlayerLoadout");
        }

        return HookResult.Continue;
    }
}
