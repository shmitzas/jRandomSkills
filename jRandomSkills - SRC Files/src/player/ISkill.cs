using CounterStrikeSharp.API.Core;

namespace jRandomSkills.src.player;

public interface ISkill
{
    public static void LoadSkill() { }
    public static void EnableSkill(CCSPlayerController player) { }
    public static void DisableSkill(CCSPlayerController player) { }
    public static void UseSkill(CCSPlayerController player) { }
    public static void TypeSkill(CCSPlayerController player, string[] commands) { }
}

public enum Skills
{
    None,
    Dwarf,
    SwapPosition,
    FrozenDecoy,
    Soldier,
    Armored,
    Aimbot,
    Retreat,
    EnemySpawn,
    Zeus,
    RadarHack,
    QuickShot,
    Planter,
    Silent,
    KillerFlash,
    TimeManipulator,
    GodMode,
    RandomWeapon,
    WeaponsSwap,
    Wallhack,
    Mute,
    HolyHandGrenade,
    Replicator,
    ToxicSmoke,
    Deactivator,
    Thief,
    Duplicator,
    AreaReaper,
    Hermit,
    RobinHood,
    Jammer,
    Glitch,
    ReturnToSender,
    ReactiveArmor,
    OnlyHead,
    Prosthesis,
    SoundMaker,
    Ninja,
    C4Camouflage,
    SecondLife,
    NoRecoil,

    Flash,
    PawelJumper,
    BunnyHop,
    Impostor,
    OneShot,
    Muhammed,
    RichBoy,
    Rambo,
    Medic,
    Ghost,
    Chicken,
    Astronaut,
    Disarmament,
    AntyFlash,
    Behind,
    InfiniteAmmo,
    Catapult,
    Dracula,
    Teleporter,
    Saper,
    Phoenix,
    Pilot,
    Shade,
    AntyHead,
}
