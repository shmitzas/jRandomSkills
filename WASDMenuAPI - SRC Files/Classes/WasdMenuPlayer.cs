using System.Text;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using WASDMenuAPI.Classes;
using WASDSharedAPI;

namespace WASDMenuAPI;

public class WasdMenuPlayer
{
    public CCSPlayerController player { get; set; }
    public WasdMenu? MainMenu = null;
    public LinkedListNode<IWasdMenuOption>? CurrentChoice = null;
    public LinkedListNode<IWasdMenuOption>? MenuStart = null;
    public string CenterHtml = "";
    public int VisibleOptions = 3;
    public PlayerButtons Buttons { get; set; }

    public int scrollIndex = 0;
    public int maxLength = 21;
    public int scrollJump = 3;

    public void OpenMainMenu(WasdMenu? menu)
    {
        if (menu == null)
        {
            MainMenu = null;
            CurrentChoice = null;
            CenterHtml = "";
            return;
        }
        MainMenu = menu;
        CurrentChoice = MainMenu.Options?.First;
        MenuStart = CurrentChoice;
        UpdateCenterHtml();
    }

    public void OpenSubMenu(IWasdMenu? menu)
    {
        if (menu == null)
        {
            CurrentChoice = MainMenu?.Options?.First;
            MenuStart = CurrentChoice;
            UpdateCenterHtml();
            return;
        }

        CurrentChoice = menu.Options?.First;
        MenuStart = CurrentChoice;
        UpdateCenterHtml();
    }
    public void GoBackToPrev(LinkedListNode<IWasdMenuOption>? menu)
    {
        if (menu == null)
        {
            CurrentChoice = MainMenu?.Options?.First;
            MenuStart = CurrentChoice;
            UpdateCenterHtml();
            return;
        }

        CurrentChoice = menu;
        if (CurrentChoice.Value.Index >= 5 )
        {
            MenuStart = CurrentChoice;
            for (int i = 0; i < VisibleOptions; i++)
            {
                MenuStart = MenuStart?.Previous;
            }
        }
        else
            MenuStart = CurrentChoice.List?.First;
        UpdateCenterHtml();
    }

    public void CloseSubMenu()
    {
        if (CurrentChoice?.Value.Parent?.Prev == null)
            return;
        GoBackToPrev(CurrentChoice?.Value.Parent.Prev);
    }

    public void CloseAllSubMenus()
    {
        OpenSubMenu(null);
    }
    
    public void Choose()
    {
        CurrentChoice?.Value.OnChoose?.Invoke(player, CurrentChoice.Value);
    }

    public void ScrollDown()
    {
        if(CurrentChoice == null || MainMenu == null)
            return;
        scrollIndex = 0;
        CurrentChoice = CurrentChoice.Next ?? CurrentChoice.List?.First;
        MenuStart = CurrentChoice!.Value.Index >= VisibleOptions ? MenuStart!.Next : CurrentChoice.List?.First;
        UpdateCenterHtml();
    }
    
    public void ScrollUp()
    {
        if(CurrentChoice == null || MainMenu == null)
            return;
        scrollIndex = 0;
        CurrentChoice = CurrentChoice.Previous ?? CurrentChoice.List?.Last;
        if (CurrentChoice == CurrentChoice?.List?.Last && CurrentChoice?.Value.Index >= VisibleOptions)
        {
            MenuStart = CurrentChoice;
            for (int i = 0; i < VisibleOptions - 1; i++)
                MenuStart = MenuStart?.Previous;
        }
        else
            MenuStart = CurrentChoice!.Value.Index >= VisibleOptions ? MenuStart!.Previous : CurrentChoice.List?.First;
        UpdateCenterHtml();
    }

    private void UpdateCenterHtml()
    {
        if (CurrentChoice == null || MainMenu == null || MenuStart == null)
            return;

        StringBuilder builder = new();
        string endControl = "";
        LinkedListNode<IWasdMenuOption>? option = MenuStart;

        if (option?.Value?.Parent?.Title != "")
            builder.AppendLine($"{option.Value.Parent?.Title}</u><font class='fontSize-m' color='white'><br>");

        if (option?.Value?.Parent?.ControlText != "")
             endControl = option?.Value?.Parent?.ControlText ?? "";

        int shown = 0;
        int maxOptions = VisibleOptions;
        
        while (shown < maxOptions && option != null && option.Value != null)
        {
            /*            if (shown == maxOptions - 1 && option.Next != null)
                        {
                            builder.AppendLine(
                                $"<img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/arrow.gif' class=''> <img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/arrow.gif' class=''> <img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/arrow.gif' class=''> <br>");
                            break;
                        }*/
            string text = option.Value.OptionDisplay!;
            if (option == CurrentChoice)
            {
                if (text.Length > maxLength)
                {
                    if (scrollIndex >= text.Length - maxLength)
                    {
                        text = text.Substring(scrollIndex);
                        if (Server.TickCount % 32 == 0)
                            scrollIndex = 0;
                    }
                    else
                    {
                        text = text.Substring(scrollIndex, maxLength);
                        if (Server.TickCount % 32 == 0)
                            scrollIndex += scrollJump;
                    }
                }

                builder.AppendLine($"</font><font color='purple'>[ </font><font color='orange'>{text}</font><font color='purple'> ]</font><font color='white'> <br>");
            }
            else
                builder.AppendLine($"{text.Substring(0, maxLength)} <br>");

            option = option.Next;
            shown++;
        }

        if (!string.IsNullOrEmpty(endControl))
            builder.AppendLine(endControl);

        builder.AppendLine("</div>");
        CenterHtml = builder.ToString();
    }

    public void UpdateActiveMenu(Dictionary<string, Action<CCSPlayerController, IWasdMenuOption>> list)
    {
        if (list.Count == 0)
        {
            OpenMainMenu(null);
            return;
        }

        if (MainMenu == null || MainMenu.Options == null)
            return;

        MainMenu.Options.Clear();
        bool choiceExists = false;

        foreach (var item in list)
        {
            WasdMenuOption newOption = new WasdMenuOption
            {
                OptionDisplay = item.Key,
                OnChoose = item.Value,
                Index = MainMenu.Options.Count,
                Parent = MainMenu
            };
            MainMenu.Options.AddLast(newOption);

            if (CurrentChoice?.Value?.OptionDisplay == item.Key)
            {
                CurrentChoice = MainMenu.Options?.Last;
                choiceExists = true;
            }
        }
        if (!choiceExists)
            CurrentChoice = MainMenu.Options?.First;

        if (CurrentChoice != null)
        {
            if (CurrentChoice.Value.Index >= VisibleOptions)
            {
                MenuStart = CurrentChoice;
                for (int i = 0; i < VisibleOptions - 1; i++)
                    MenuStart = MenuStart?.Previous;
            }
            else
                MenuStart = MainMenu.Options?.First;
        }

        UpdateCenterHtml();
    }
}