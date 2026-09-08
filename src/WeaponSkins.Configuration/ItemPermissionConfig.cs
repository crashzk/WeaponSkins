using Microsoft.Extensions.Configuration;

namespace WeaponSkins.Configuration;

public class ItemPermissionConfig
{
    public string WeaponSkins { get; set; } = "";

    public string KnifeSkins { get; set; } = "";

    public string GloveSkins { get; set; } = "";

    public string Stickers { get; set; } = "";

    public string Keychains { get; set; } = "";

    public string Agents { get; set; } = "";

    public string MusicKits { get; set; } = "";

    // Gates the Wear input inside the Weapon/Knife/Glove "Properties" menus.
    public string Wear { get; set; } = "";

    // Gates the Seed input inside the Weapon/Knife/Glove "Properties" menus.
    public string Seed { get; set; } = "";

    // Gates the NameTag input (set/unset) inside the Weapon/Knife "Properties" menus.
    public string Nametag { get; set; } = "";

    // Gates the StatTrak options (set/unset/count) inside the Weapon/Knife "Properties" menus.
    public string Stattrak { get; set; } = "";
}
