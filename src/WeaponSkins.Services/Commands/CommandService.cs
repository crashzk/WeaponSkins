using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Commands;

using WeaponSkins.Database;

namespace WeaponSkins;

public partial class CommandService
{
    private ISwiftlyCore Core { get; init; }
    private ILogger Logger { get; init; }
    private MenuService MenuService { get; init; }
    private DatabaseSynchronizeService DatabaseSynchronizeService { get; init; }
    private StorageService StorageService { get; init; }

    public CommandService(ISwiftlyCore core,
        ILogger<CommandService> logger,
        MenuService menuService,
        DatabaseSynchronizeService databaseSynchronizeService,
        StorageService storageService)
    {
        Core = core;
        Logger = logger;
        MenuService = menuService;
        DatabaseSynchronizeService = databaseSynchronizeService;
        StorageService = storageService;

        RegisterCommands();
    }

    public void RegisterCommands()
    {
        Core.Command.RegisterCommand("ws", CommandSkin);
        Core.Command.RegisterCommand("wp", CommandReload);
    }

    private void CommandSkin(ICommandContext context)
    {
        if (!context.IsSentByPlayer)
        {
            context.Reply("This command can only be used by players.");
            return;
        }

        MenuService.OpenMainMenu(context.Sender!);
    }

    private void CommandReload(ICommandContext context)
    {
        if (!context.IsSentByPlayer)
        {
            context.Reply("This command can only be used by players.");
            return;
        }

        var player = context.Sender;
        if (player == null)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            await DatabaseSynchronizeService.ReloadPlayerAsync(StorageService.Get(), player.SteamID);
            Logger.LogInformation("Reloaded skins from the database for {SteamId}.", player.SteamID);
        });
        context.Reply("Reloading your skins from the database.");
    }
}
