using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins.Database;

public class DatabaseSynchronizeService
{
    private DatabaseService DatabaseService { get; init; }
    private DataService DataService { get; init; }
    private IInventoryUpdateService InventoryUpdateService { get; init; }
    private ISwiftlyCore Core { get; init; }

    public DatabaseSynchronizeService(DatabaseService databaseService,
        DataService dataService,
        IInventoryUpdateService inventoryUpdateService,
        ISwiftlyCore core)
    {
        DatabaseService = databaseService;
        DataService = dataService;
        InventoryUpdateService = inventoryUpdateService;
        Core = core;
    }

    public void Synchronize()
    {
        Task.Run(async () =>
        {
            var skins = await DatabaseService.GetAllSkinsAsync();
            skins.ToList().ForEach(skin => DataService.WeaponDataService.StoreSkin(skin));
            var knives = await DatabaseService.GetAllKnifesAsync();
            knives.ToList().ForEach(knife => DataService.KnifeDataService.StoreKnife(knife));
            var gloves = await DatabaseService.GetAllGlovesAsync();
            gloves.ToList().ForEach(glove => DataService.GloveDataService.StoreGlove(glove));
            var agents = await DatabaseService.GetAllAgentsAsync();
            agents.ToList().ForEach(agent => DataService.AgentDataService.SetAgent(agent.SteamID, agent.Team, agent.AgentIndex));
            var musicKits = await DatabaseService.GetAllMusicKitsAsync();
            musicKits.ToList().ForEach(mk => DataService.MusicKitDataService.SetMusicKit(mk.SteamID, mk.MusicKitIndex));
        });
    }

    public Task ReloadPlayerAsync(ulong steamId) => ReloadPlayerAsync(DatabaseService, steamId);

    public async Task ReloadPlayerAsync(IStorageProvider storage,
        ulong steamId)
    {
        var skins = (await storage.GetSkinsAsync(steamId)).ToList();
        var knives = (await storage.GetKnifesAsync(steamId)).ToList();
        var gloves = (await storage.GetGlovesAsync(steamId)).ToList();
        var agents = (await storage.GetAgentsAsync(steamId)).ToList();
        var musicKits = (await storage.GetMusicKitsAsync(steamId)).ToList();

        DataService.WeaponDataService.RemovePlayer(steamId);
        foreach (var skin in skins)
        {
            DataService.WeaponDataService.StoreSkin(skin);
        }

        DataService.KnifeDataService.RemovePlayer(steamId);
        foreach (var knife in knives)
        {
            DataService.KnifeDataService.StoreKnife(knife);
        }

        DataService.GloveDataService.RemovePlayer(steamId);
        foreach (var glove in gloves)
        {
            DataService.GloveDataService.StoreGlove(glove);
        }

        DataService.AgentDataService.TryRemoveAgent(steamId, Team.T);
        DataService.AgentDataService.TryRemoveAgent(steamId, Team.CT);
        foreach (var agent in agents)
        {
            DataService.AgentDataService.SetAgent(agent.SteamID, agent.Team, agent.AgentIndex);
        }

        DataService.MusicKitDataService.RemoveMusicKit(steamId);
        var musicKit = musicKits.LastOrDefault();
        if (musicKit.SteamID != 0)
        {
            DataService.MusicKitDataService.SetMusicKit(musicKit.SteamID, musicKit.MusicKitIndex);
        }

        Core.Scheduler.NextTick(() => InventoryUpdateService.RefreshPlayer(steamId));
    }

    public async Task ReloadOnlinePlayersAsync(IStorageProvider? storage = null)
    {
        storage ??= DatabaseService;
        foreach (var player in Core.PlayerManager.GetAllPlayers())
        {
            if (player.SteamID == 0)
            {
                continue;
            }

            await ReloadPlayerAsync(storage, player.SteamID);
        }
    }
}
