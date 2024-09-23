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
        public override string ModuleVersion => "1.0.1";

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
        public static List<dSkill_SkillInfo> Skills { get; } = new List<dSkill_SkillInfo>();
    }
}