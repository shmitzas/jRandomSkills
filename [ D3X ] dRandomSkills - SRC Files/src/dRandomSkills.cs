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
        public override string ModuleVersion => "1.0.2";

        public override void Load(bool hotReload)
        {
            Instance = this;

            Config.Initialize();
            Event.Load();
            PlayerOnTick.Load();
            Command.Load();

            // ==== NOWE ====
            MiniMajk.LoadMiniMajk();
            ZamianaMiejsc.LoadZamianaMiejsc();
            
            // ==== POPRAWIONE/NAPRAWIONE ====
            Flash.LoadFlash();
            PawelJumper.LoadPawelJumper();
            BunnyHop.LoadBunnyHop();
            Impostor.LoadImpostor();
            OneShot.LoadOneShot();
            Muhammed.LoadMuhammed();
            Bogacz.LoadBogacz();
            Rambo.LoadRambo();
            Medyk.LoadMedyk();
            Duszek.LoadDuszek();
            Kurczak.LoadKurczak();
            Astronauta.LoadAstronauta();
            Rozbrojenie.LoadRozbrojenie();
            AntyFlash.LoadAntyFlash();
            ObrotWroga.LoadObrotWroga();
            NieskonczoneAmmo.LoadNieskonczoneAmmo();
            Katapulta.LoadKatapulta();
            Drakula.LoadDrakula();
            Teleporter.LoadTeleporter();
            Saper.LoadEliminator();
            Phoenix.LoadPhoenix();
            Pilot.LoadPilot();
            Cien.LoadCien();
            ZelaznaGlowa.LoadZelaznaGlowa();
        }
    }

    public class dSkill_PlayerInfo
    {
        public required ulong SteamID { get; set; }
        public required string PlayerName { get; set; }
        public string? Skill { get; set; }
        public float? SkillChance { get; set; }
        public bool IsDrawing { get; set; }
    }

     public class dSkill_SkillInfo
    {
        public string Name { get; }
        public string Description { get; }
        public string Color { get; }

        public bool Display { get; }

        public dSkill_SkillInfo(string name, string description, string color, bool display)
        {
            Name = name;
            Description = description;
            Color = color;
            Display = display;
        }
    }

    public static class SkillData
    {
        public static List<dSkill_SkillInfo> Skills { get; } = new List<dSkill_SkillInfo>();
    }
}