using CounterStrikeSharp.API.Core;

namespace dRandomSkills
{
    public partial class dRandomSkills : BasePlugin
    {
        public static dRandomSkills Instance { get; private set; }

        public List<dSkill_PlayerInfo> skillPlayer { get; } = new List<dSkill_PlayerInfo>();
        public Random Random { get; } = new Random();

        public override string ModuleName => "[CS2] D3X - [ Random Skills ]";
        public override string ModuleAuthor => "D3X";
        public override string ModuleDescription => "Plugin dodaje na serwer losowe moce co runde na serwery CS2 by D3X";
        public override string ModuleVersion => "1.0.0";

        public override void Load(bool hotReload)
        {
            Instance = this;

            Config.Initialize();
            Event.Load();
            PlayerOnTick.Load();
            Command.Load();

            PawelJumper.LoadPawelJumper();
            Duszek.LoadDuszek();
            BunnyHop.LoadBunnyHop();
            Impostor.LoadImpostor();
            Phoenix.LoadPhoenix();
            Muhammed.LoadMuhammed();
            Pilot.LoadPilot();
            Rambo.LoadRambo();
            Bogacz.LoadBogacz();
            Flash.LoadFlash();
            Astronauta.LoadAstronauta();
            Medyk.LoadMedyk();
            Kurczak.LoadKurczak();
            OneShot.LoadOneShot();
            Drakula.LoadDrakula();
            AntyFlash.LoadAntyFlash();
            Rozbrojenie.LoadRozbrojenie();
            ZelaznaGlowa.LoadZelaznaGlowa();
            NieskonczoneAmmo.LoadNieskonczoneAmmo();
            Katapulta.LoadKatapulta();
            ObrotWroga.LoadObrotWroga();
            Cien.LoadCien();
            Teleporter.LoadTeleporter();
            Eliminator.LoadEliminator();
        }
    }

    public class dSkill_PlayerInfo
    {
        public required ulong SteamID { get; set; }
        public required string PlayerName { get; set; }
        public string? Skill { get; set; }
        public bool IsDrawing { get; set; }
    }

     public class dSkill_SkillInfo
    {
        public string Name { get; }
        public string Description { get; }
        public string Color { get; }

        public dSkill_SkillInfo(string name, string description, string color)
        {
            Name = name;
            Description = description;
            Color = color;
        }
    }

    public static class SkillData
    {
        public static dSkill_SkillInfo[] Skills { get; } = new[]
        {
            new dSkill_SkillInfo("Paweł Jumper", "Otrzymujesz dodatkowy Skok", "#FFA500"),
            new dSkill_SkillInfo("Duszek", "Jesteś całkowicie niewidzialny", "#FFFFFF"),
            new dSkill_SkillInfo("BunnyHop", "Otrzymujesz auto BunnyHopa", "#EB4034"),
            new dSkill_SkillInfo("Impostor", "Otrzymujesz na start rundy model postaci wroga", "#99140B"),
            new dSkill_SkillInfo("Phoenix", "Masz 35% szans na odrodzenie się po śmierci", "#ff5C0A"),
            new dSkill_SkillInfo("Muhammed", "Po śmierci wybucha i zabija graczy w obrębie", "#F5CB42"),
            new dSkill_SkillInfo("Pilot", "Latanie na noclip przez dany czas", "#1466F5"),
            new dSkill_SkillInfo("Rambo", "Otrzymujesz losową ilość zdrowia na start rundy", "#009905"),
            new dSkill_SkillInfo("Bogacz", "Otrzymujesz losową ilość kasy na start rundy", "#D4AF37"),
            new dSkill_SkillInfo("Flash", "Otrzymujesz losową ilość speed na start rundy", "#A31912"),
            new dSkill_SkillInfo("Astronauta", "Otrzymujesz losową ilość grawitacji na start rundy", "#7E10AD"),
            new dSkill_SkillInfo("Medyk", "Otrzymujesz losową ilość apteczek na start rundy", "#42FF5F"),
            new dSkill_SkillInfo("Kurczak", "Otrzymujesz model kurczaka + jesteś o 10% szybszy", "#FF8B42"),
            new dSkill_SkillInfo("One Shot", "Po trafieniu od razu zabija przeciwnika", "#ff5CD9"),
            new dSkill_SkillInfo("Drakula", "Po trafieniu ofiary otrzymujesz zwrot zdrowia w postaci danego procentu zadanych obrażeń", "#FA050D"),
            new dSkill_SkillInfo("Anty Flash", "Posiadasz odporność na flashe", "#D6E6FF"),
            // new dSkill_SkillInfo("Mini Gun", "Strzelasz 50% szybciej z broni", "#2CF5BF"),
            new dSkill_SkillInfo("Teleportator", "Zamieniasz się miejscami z trafionym wrogiem", "#8A2BE2"),
            new dSkill_SkillInfo("Cień", "Teleportujesz się za plecy losowego wroga", "#18171A"),
            new dSkill_SkillInfo("Katapulta", "Masz 25% szans na podrzucenie wroga", "#FF4500"),
            new dSkill_SkillInfo("Nieskończone Ammo", "Otrzymujesz nieskończoną ilość ammo do wszystkich swoich broni", "#0000FF"),
            new dSkill_SkillInfo("Żelazna Głowa", "Nie otrzymujesz obrażeń w głowę", "#8B4513"),
            new dSkill_SkillInfo("Eliminator", "Możesz szybciej podłożyć bombe oraz ją zdefować", "#8A2BE2"),
            // new dSkill_SkillInfo("Redukcja Obrażeń", "Redukujesz daną ilość obrażeń, które otrzymujesz", "#696969"),
            // new dSkill_SkillInfo("Mnożnik Obrażeń", "Zadajesz pomnożoną ilość obrażeń", "#FF0000"),
            new dSkill_SkillInfo("Obrót Wroga", "Masz 25% szans na obrócenie wroga o 180 stopni po trafieniu", "#00FF00"),
            new dSkill_SkillInfo("Rozbrojenie", "Masz 25% szans na wyrzucenie broni wroga po trafieniu", "#FF4500")
        };
    }
}