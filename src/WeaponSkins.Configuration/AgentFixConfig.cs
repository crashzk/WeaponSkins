namespace WeaponSkins.Configuration;

// Confirmed on a live server: this only affects what teammates see of a player's agent (via
// AgentLoadoutFixService). It has no effect on the buy menu preview a player sees of their
// own character - that one is rendered client-side from the local Steam inventory and cannot
// be overridden by the server.
public class AgentFixConfig
{
    // When true, the "custom player" (agent) loadout slot is blanked out for teams/players
    // that don't have an agent explicitly set through this plugin's menu, instead of
    // falling back to whatever agent Steam would otherwise report to teammates.
    //
    // Leave this false (default) if you only want the plugin's agent selection to be
    // reflected correctly when a player *does* pick one (the more conservative fix), and
    // don't want to touch the behavior for players who never open the Agents menu.
    public bool EnforceManagedOnly { get; set; } = false;
}
