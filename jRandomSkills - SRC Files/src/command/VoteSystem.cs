using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Modules.Utils;

namespace jRandomSkills
{
    public static class VoteSystem
    {
        private static HashSet<VoteData> votes = new HashSet<VoteData>();

        public static void Load()
        {
            jRandomSkills.Instance.DeregisterEventHandler<EventPlayerChat>((@event, info) =>
            {
                var player = @event.Userid;
                var text = @event.Text;
                return HookResult.Continue;
            });
        }

        private static VoteData CreateVote(VoteType voteType, string? args = null)
        {
            var vote = new VoteData(10, 
                () => { 
                    Server.ExecuteCommand($"{VoteTypeCommands.GetCommand(voteType)}{(args != null ? $" {args}" : "")}");
                }, 60, voteType, args);
            votes.Add(vote);

            jRandomSkills.Instance.AddTimer(vote?.TimeToVote ?? 0, () =>
            {
                if (!votes.Contains(vote)) return;
                votes.Remove(vote);
                Server.PrintToChatAll($" {ChatColors.Red}Vote '!{VoteTypeCommands.GetCommand(vote.Type)?.Replace("css_", "")}' timed out!");
            });
            return vote;
        }

        public static void Vote(this CCSPlayerController player, VoteType voteType, string? args = null)
        {
            var vote = votes.FirstOrDefault(v => v.Type == voteType);
            if (vote == null)
                vote = CreateVote(voteType, args);

            if (!vote.PlayersVoted.Add(player.SteamID))
                player.PrintToChat($" {ChatColors.Red}You have already voted!");
            else CheckVote(vote);
        }

        private static void CheckVote(VoteData vote)
        {
            int voted = vote.PlayersVoted.Count;
            int playerCount = Utilities.GetPlayers().Where(p => !p.IsBot).ToArray().Length;
            int playersNeeded = (int)Math.Ceiling(playerCount * (vote.PercentagesToSuccess / 100f));

            if (voted >= playersNeeded)
            {
                vote.SuccessAction.Invoke();
                votes.Remove(vote);
            }
            else
                Server.PrintToChatAll($" {ChatColors.Yellow}Vote '!{VoteTypeCommands.GetCommand(vote.Type)?.Replace("css_", "")}': {ChatColors.Green}{voted}/{playersNeeded}");
        }
    }

    public class VoteData
    {
        public float TimeToVote { get; set; }
        public Action SuccessAction { get; set; }
        public float PercentagesToSuccess { get; set; }
        public VoteType Type { get; set; }
        public string Args { get; set; }
        public HashSet<ulong> PlayersVoted { get; set; }

        public VoteData(float timeToVote, Action successAction, float percentagesToSuccess, VoteType type, string? args = null)
        {
            TimeToVote = timeToVote;
            SuccessAction = successAction;
            PercentagesToSuccess = percentagesToSuccess;
            Type = type;
            Args = args;
            PlayersVoted = new HashSet<ulong>();
        }
    }

    public enum VoteType
    {
        StartGame,
        PauseGame,
        ShuffleTeam,
        SwapTeam,
        ChangeMap,
        SetScore,
    }

    public static class VoteTypeCommands
    {
        private static readonly Dictionary<VoteType, string> names = new Dictionary<VoteType, string>()
        {
            { VoteType.StartGame, "css_start" },
            { VoteType.PauseGame, "css_pause" },
            { VoteType.ShuffleTeam, "css_shuffle" },
            { VoteType.SwapTeam, "css_swap" },
            { VoteType.ChangeMap, "css_map" },
            { VoteType.SetScore, "css_setscore" },
        };

        public static string GetCommand(VoteType type) => names[type];
    }
}
