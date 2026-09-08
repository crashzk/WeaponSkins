using SwiftlyS2.Core.Menus.OptionsBase;
using SwiftlyS2.Shared.Menus;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins;

public partial class MenuService
{
    public IMenuAPI BuildKnifePropertiesMenu(IPlayer player,
        KnifeSkinData weaponInHand)
    {
        var main = Core.MenusAPI.CreateBuilder();
        main.Design.SetMenuTitle(LocalizationService[player].MenuTitleKnifeProperties);

        var canUseWear = ItemPermissionService.CanUseWear(player.SteamID);
        var canUseSeed = ItemPermissionService.CanUseSeed(player.SteamID);
        var canUseNametag = ItemPermissionService.CanUseNametag(player.SteamID);
        var canUseStattrak = ItemPermissionService.CanUseStattrak(player.SteamID);

        var wearOption = new InputMenuOption(
            LocalizationService[player].MenuSkinPropertiesWear,
            validator: (value) =>
            {
                if (float.TryParse(value, out var result))
                {
                    return result is >= 0.0f and <= 1.0f;
                }

                return false;
            }
        );
        wearOption.SetValue(player, weaponInHand.PaintkitWear.ToString());
        wearOption.ValueChanged += (_,
            args) =>
        {
            weaponInHand.PaintkitWear = float.Parse(args.NewValue);
            Api.UpdateKnifeSkin(weaponInHand.SteamID, weaponInHand.Team, skin =>
            {
                skin.PaintkitWear = weaponInHand.PaintkitWear;
            }, true);
        };

        wearOption.Enabled = canUseWear;
        main.AddOption(wearOption);

        var seedOption = new InputMenuOption(
            LocalizationService[player].MenuSkinPropertiesSeed,
            validator: (value) =>
            {
                if (int.TryParse(value, out var result))
                {
                    return result >= 0;
                }

                return false;
            }
        );

        seedOption.SetValue(player, weaponInHand.PaintkitSeed.ToString());

        seedOption.ValueChanged += (_,
            args) =>
        {
            weaponInHand.PaintkitSeed = int.Parse(args.NewValue);

            Api.UpdateKnifeSkin(weaponInHand.SteamID, weaponInHand.Team, skin =>
            {
                skin.PaintkitSeed = weaponInHand.PaintkitSeed;
            }, true);
        };

        seedOption.Enabled = canUseSeed;
        main.AddOption(seedOption);


        var nametagOption = new InputMenuOption(
            LocalizationService[player].MenuSkinPropertiesNametag
        );

        nametagOption.SetValue(player,
            weaponInHand.Nametag ?? LocalizationService[player].MenuSkinPropertiesNametagNone);

        nametagOption.ValueChanged += (_,
            args) =>
        {
            Api.UpdateKnifeSkin(weaponInHand.SteamID, weaponInHand.Team, skin =>
            {
                skin.Nametag = args.NewValue;
            }, true);
        };

        nametagOption.Enabled = canUseNametag;
        main.AddOption(nametagOption);

        var unsetNametagOption = new ButtonMenuOption(LocalizationService[player].MenuSkinPropertiesNametagUnset);
        unsetNametagOption.Click += (_,
            args) =>
        {
            Api.UpdateKnifeSkin(weaponInHand.SteamID, weaponInHand.Team, skin =>
            {
                skin.Nametag = null;
            }, true);
            return ValueTask.CompletedTask;
        };

        unsetNametagOption.Enabled = canUseNametag;
        main.AddOption(unsetNametagOption);

        var setStattrakOption = new ButtonMenuOption(LocalizationService[player].MenuSkinPropertiesSetStattrak);
        setStattrakOption.Click += (_,
            args) =>
        {
            Api.UpdateKnifeSkin(weaponInHand.SteamID, weaponInHand.Team, skin =>
            {
                skin.Quality = EconItemQuality.StatTrak;
            }, true);
            return ValueTask.CompletedTask;
        };

        setStattrakOption.Enabled = canUseStattrak;
        main.AddOption(setStattrakOption);

        var unsetStattrakOption = new ButtonMenuOption(LocalizationService[player].MenuSkinPropertiesUnsetStattrak);
        unsetStattrakOption.Click += (_,
            args) =>
        {
            Api.UpdateKnifeSkin(weaponInHand.SteamID, weaponInHand.Team, skin =>
            {
                skin.Quality = EconItemQuality.Normal;
            }, true);
            return ValueTask.CompletedTask;
        };

        unsetStattrakOption.Enabled = canUseStattrak;
        main.AddOption(unsetStattrakOption);

        var setStattrakCountOption = new InputMenuOption(
            LocalizationService[player].MenuSkinPropertiesStattrakCount(weaponInHand.StattrakCount),
            validator: (value) =>
            {
                if (int.TryParse(value, out var result))
                {
                    return result >= 0;
                }

                return false;
            }
        );
        setStattrakCountOption.SetValue(player, weaponInHand.StattrakCount.ToString());
        setStattrakCountOption.ValueChanged += (_,
            args) =>
        {
            weaponInHand.StattrakCount = int.Parse(args.NewValue);
            Api.UpdateKnifeSkin(weaponInHand.SteamID, weaponInHand.Team, skin =>
            {
                skin.StattrakCount = weaponInHand.StattrakCount;
            }, true);
        };
        setStattrakCountOption.Enabled = canUseStattrak;
        main.AddOption(setStattrakCountOption);

        return main.Build();
    }

    public IMenuOption GetKnifePropertiesMenuSubmenuOption(IPlayer player)
    {
        if (!ItemPermissionService.CanUseKnifeSkins(player.SteamID))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleKnifeProperties);
        }

        if (!TryGetKnifeDataInHand(player, out var dataInHand))
        {
            return CreateDisabledOption(LocalizationService[player].MenuTitleKnifeProperties);
        }

        return new SubmenuMenuOption(LocalizationService[player].MenuTitleKnifeProperties,
            () => Task.FromResult(BuildKnifePropertiesMenu(player, dataInHand)));
    }
}
