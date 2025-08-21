using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.player;

namespace jRandomSkills
{
    public static class SkillUtils
    {
        public static void PrintToChat(CCSPlayerController player, string msg, bool isError)
        {
            string checkIcon = isError ? $"{ChatColors.DarkRed}✖{ChatColors.LightRed}" : $"{ChatColors.Green}✔{ChatColors.Lime}";
            player.PrintToChat($" {ChatColors.DarkRed}► {ChatColors.Green}[{ChatColors.DarkRed} jRadnomSkills {ChatColors.Green}] {checkIcon} {msg}");
        }

        public static void RegisterSkill(Skills skill, string color, bool display = true)
        {
            if (!SkillData.Skills.Any(s => s.Skill == skill))
                SkillData.Skills.Add(new dSkill_SkillInfo(skill, color, display));
        }

        public static void TryGiveWeapon(CCSPlayerController player, CsItem item, int count = 1)
        {
            string? itemString = EnumUtils.GetEnumMemberAttributeValue(item);
            if (string.IsNullOrWhiteSpace(itemString)) return;

            var exists = player.PlayerPawn.Value.WeaponServices.MyWeapons.FirstOrDefault(w => w.Value.DesignerName == itemString);
            if (exists == null)
                for (int i = 0; i < count; i++)
                    player.GiveNamedItem(item);
        }

        public static Vector GetForwardVector(QAngle angles)
        {
            float pitch = angles.X * (float)(Math.PI / 180);
            float yaw = angles.Y * (float)(Math.PI / 180);

            float x = (float)(Math.Cos(pitch) * Math.Cos(yaw));
            float y = (float)(Math.Cos(pitch) * Math.Sin(yaw));
            float z = (float)Math.Sin(pitch);

            return new Vector(x, y, z);
        }

        public static void TakeHealth(CCSPlayerPawn pawn, int damage)
        {
            if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
                return;

            int newHealth = (int)(pawn.Health - damage);
            pawn.Health = newHealth;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");

            if (pawn.Health <= 0)
                Server.NextFrame(() =>
                {
                    pawn?.CommitSuicide(false, true);
                });
        }

        public static void AddHealth(CCSPlayerPawn pawn, int extraHealth, int maxHealth = 100)
        {
            if (pawn.LifeState != (byte)LifeState_t.LIFE_ALIVE)
                return;

            int newHealth = (int)(pawn.Health + extraHealth);
            pawn.Health = Math.Min(newHealth, maxHealth);
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iHealth");

            pawn.MaxHealth = maxHealth;
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_iMaxHealth");
        }
    }
}