using Rewired;

public static class RewiredExtension
{
    public static Player SelectTheMap(this Player player, string mapName)
    {
        Player p = player;
        p.controllers.maps.SetAllMapsEnabled(false);
        p.controllers.maps.SetMapsEnabled(true, mapName);
        return p;
    }
}