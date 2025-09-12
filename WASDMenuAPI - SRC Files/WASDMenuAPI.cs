using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using WASDMenuAPI.Classes;

namespace WASDMenuAPI;

public class WASDMenuAPI
{
    public string ModuleName => "WASDMenuAPI";
    public string ModuleVersion => "1.0.2";
    public string ModuleAuthor => "Interesting";

    public static readonly Dictionary<ulong, WasdMenuPlayer> Players = [];
    
    public static void LoadPlugin(BasePlugin basePlugin, bool hotReload)
    {
        var wasdMenuManager = new WasdManager();
        basePlugin.RegisterEventHandler<EventPlayerActivate>((@event, info) =>
        {
            if (@event.Userid != null)
                Players[@event.Userid.SteamID] = new WasdMenuPlayer
                {
                    player = @event.Userid,
                    Buttons = 0
                };
            return HookResult.Continue;
        });
        basePlugin.RegisterEventHandler<EventPlayerDisconnect>((@event, info) =>
        {
            if (@event.Userid != null) Players.Remove(@event.Userid.SteamID);
            return HookResult.Continue;
        });

        basePlugin.RegisterListener<Listeners.OnTick>(OnTick);
        
        if(hotReload)
            foreach (var pl in Utilities.GetPlayers())
            {
               Players[pl.SteamID] = new WasdMenuPlayer
               {
                   player = pl,
                   Buttons = pl.Buttons
               };
            }
    }

    public static void OnTick()
    {
        foreach (var player in Players.Values.Where(p => p.MainMenu != null))
        {
            if ((player.Buttons & PlayerButtons.Forward) == 0 && (player.player.Buttons & PlayerButtons.Forward) != 0)
            {
                player.ScrollUp();
            }
            else if((player.Buttons & PlayerButtons.Back) == 0 && (player.player.Buttons & PlayerButtons.Back) != 0)
            {
                player.ScrollDown();
            }
            else if((player.Buttons & PlayerButtons.Use) == 0 && (player.player.Buttons & PlayerButtons.Use) != 0)
            {
                player.Choose();
            }
            
            player.Buttons = player.player.Buttons;
            if(player.CenterHtml != "")
                Server.NextFrame(() =>
                player.player.PrintToCenterHtml(player.CenterHtml)
            );
        }
    }
    
    
}
