using CounterStrikeSharp.API.Core;
using WASDSharedAPI;

namespace WASDMenuAPI.Classes;

public class WasdManager : IWasdMenuManager
{
    public void OpenMainMenu(CCSPlayerController? player, IWasdMenu? menu)
    {
        if(player == null)
            return;
        WASDMenuAPI.Players[player.SteamID].OpenMainMenu((WasdMenu?)menu);
    }

    public void CloseMenu(CCSPlayerController? player)
    {
        if(player == null)
            return;
        WASDMenuAPI.Players[player.SteamID].OpenMainMenu(null);
    }

    public void CloseSubMenu(CCSPlayerController? player)
    {
        if(player == null)
            return;
        WASDMenuAPI.Players[player.SteamID].CloseSubMenu();
    }

    public void CloseAllSubMenus(CCSPlayerController? player)
    {
        if(player == null)
            return;
        WASDMenuAPI.Players[player.SteamID].CloseAllSubMenus();
    }

    public void OpenSubMenu(CCSPlayerController? player, IWasdMenu? menu)
    {
        if (player == null)
            return;
        WASDMenuAPI.Players[player.SteamID].OpenSubMenu(menu);
    }

    public IWasdMenu CreateMenu(string title = "", string controlText = "")
    {
        WasdMenu menu = new WasdMenu
        {
            Title = title,
            ControlText = controlText
        };
        return menu;
    }

    public bool HasMenu(CCSPlayerController? player)
    {
        if (player == null)
            return false;
        return WASDMenuAPI.Players.ContainsKey(player.SteamID);
    }

    public void UpdateActiveMenu(CCSPlayerController? player, Dictionary<string, Action<CCSPlayerController, IWasdMenuOption>> list)
    {
        if (player == null)
            return;
        WASDMenuAPI.Players[player.SteamID].UpdateActiveMenu(list);
    }
}