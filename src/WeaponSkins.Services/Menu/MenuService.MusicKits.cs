using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

namespace WeaponSkins;

public partial class MenuService
{
    private ValueTask OnMusicKitMenuOptionClick(object? sender,
        MenuOptionClickEventArgs args)
    {
        Core.Scheduler.NextTick(() =>
        {
            if (DataService.MusicKitDataService.TryGetMusicKit(args.Player.SteamID, out var musicKitIndex))
            {
                var menu = Core.MenusAPI.GetCurrentMenu(args.Player);
                if (menu != null)
                {
                    var option = menu.Options.FirstOrDefault(o =>
                            o.Tag is int tag &&
                            tag == musicKitIndex);

                    if (option != null)
                        menu.MoveToOption(args.Player, option);
                }
            }
        });
        return ValueTask.CompletedTask;
    }

    private IMenuOption GetMusicKitMenuSubmenuOption(IPlayer player)
    {
        var option = new SubmenuMenuOption(LocalizationService[player].MenuTitleMusicKits, () =>
        {
            var menu = Core.MenusAPI.CreateBuilder();
            menu.Design.SetMenuTitle(LocalizationService[player].MenuTitleMusicKits);

            var resetOption = new ButtonMenuOption(LocalizationService[player].MenuReset);
            resetOption.Click += (_, args) =>
            {
                Api.ResetMusicKit(player.SteamID);
                return ValueTask.CompletedTask;
            };
            menu.AddOption(resetOption);

            foreach (var musicKit in EconService.MusicKits.Values.OrderBy(mk => mk.Index))
            {
                // Some schema entries (placeholder/"no kit" definitions) have no localized
                // name at all - skip them instead of showing a blank menu entry.
                if (musicKit.LocalizedNames.Count == 0)
                {
                    continue;
                }

                var musicKitName = EconService.GetLocalizedName(musicKit.LocalizedNames, player.PlayerLanguage.Value);

                var truncatedName = musicKitName.Length > 30 ? musicKitName.Substring(0, 27) + "..." : musicKitName;
                var index = musicKit.Index;

                var selectOption = new ButtonMenuOption(truncatedName);
                selectOption.Tag = index;
                selectOption.Click += (_, args) =>
                {
                    Api.SetMusicKit(args.Player.SteamID, index);
                    return ValueTask.CompletedTask;
                };

                menu.AddOption(selectOption);
            }

            return menu.Build();
        });

        option.Click += OnMusicKitMenuOptionClick;

        return option;
    }
}
