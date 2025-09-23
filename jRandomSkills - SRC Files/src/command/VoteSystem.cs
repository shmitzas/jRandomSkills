using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using jRandomSkills.src.utils;
using System.Collections.Concurrent;

namespace jRandomSkills
{
    public static class VoteSystem
    {
        private static readonly ConcurrentDictionary<VoteData, byte> votes = [];

        private static VoteData? CreateVote(VoteType voteType, string? args = null)
        {
            var vote = new VoteData(10,
                () => {
                    Server.ExecuteCommand($"{VoteTypeCommands.GetCommand(voteType)}{(!string.IsNullOrEmpty(args) ? $" {args}" : "")}");
                }, 60, 2, 5, 2, voteType, args);

            if (vote.MinimumPlayersToStartVoting > Utilities.GetPlayers().Count(p => !p.IsBot))
                return null;

            votes.TryAdd(vote, 0);
            string commandName = $"!{VoteTypeCommands.GetCommand(vote.Type)?.Replace("css_", "")}{(!string.IsNullOrEmpty(vote?.Args) ? $" {vote?.Args}" : "")}";

            Localization.PrintTranslationToChatAll($" {ChatColors.Lime}{{0}}", ["vote_started"], [commandName]);
            foreach (var player in Utilities.GetPlayers())
                player.EmitSound("UIPanorama.tab_mainmenu_news");

            if (vote == null) return vote;
            jRandomSkills.Instance.AddTimer(vote.TimeToVote, () =>
            {
                if (!votes.ContainsKey(vote) || !vote.GetActive()) return;
                vote.SetActive(false);
                vote.TimeToNextSameVoting = vote.TimeToNextVoting;
                Localization.PrintTranslationToChatAll($" {ChatColors.Red}{{0}}", ["vote_timeout"], [commandName]);
            });

            float[] times = [vote.TimeToVote, vote.TimeToVote + vote.TimeToNextVoting, vote.TimeToVote + vote.TimeToNextSameVoting];
            jRandomSkills.Instance.AddTimer(times.Max(), () =>
            {
                if (!votes.ContainsKey(vote)) return;
                votes.TryRemove(vote, out _);
            });
            return vote;
        }

        public static void Vote(this CCSPlayerController player, VoteType voteType, string? args = null)
        {
            var vote = votes.Keys.FirstOrDefault(v => v.Type == voteType && v.Args == args && v.GetActive());
            if (vote == null)
            {
                if (votes.Keys.Any(v => v.NextVoting() > DateTime.Now))
                {
                    player.PrintToChat($" {ChatColors.Red}{player.GetTranslation("vote_wait")}");
                    return;
                }
                else if (votes.Keys.Any(v => v.Type == voteType && v.NextSameVoting() > DateTime.Now))
                {
                    player.PrintToChat($" {ChatColors.Red}{player.GetTranslation("vote_same_wait")}");
                    return;
                }

                vote = CreateVote(voteType, args);
            }

            if (vote == null)
            {
                player.PrintToChat($" {ChatColors.Red}{player.GetTranslation("vote_not_enough_players")}");
                return;
            }

            if (!vote.PlayersVoted.TryAdd(player.SteamID, 0))
                player.PrintToChat($" {ChatColors.Red}{player.GetTranslation("vote_alredy_voted")}");
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
                vote.SetActive(false);
            }
            else
                Localization.PrintTranslationToChatAll($" {ChatColors.Yellow}{{0}} '!{VoteTypeCommands.GetCommand(vote.Type)?.Replace("css_", "")}{(!string.IsNullOrEmpty(vote?.Args) ? $" {vote?.Args}" : "")}': {ChatColors.Green}{voted}/{playersNeeded}", ["vote_vote"]);
        }
    }

    public class VoteData(float timeToVote, Action successAction, float percentagesToSuccess, float timeToNextVoting, float timeToNextSameVoting, int minimumPlayersToStartVoting, VoteType type, string? args = null)
    {
        private bool Active { get; set; } = true;
        public float TimeToVote { get; set; } = timeToVote;
        public Action SuccessAction { get; set; } = successAction;
        public float PercentagesToSuccess { get; set; } = percentagesToSuccess;
        public float TimeToNextVoting { get; set; } = timeToNextVoting;
        public float TimeToNextSameVoting { get; set; } = timeToNextSameVoting;
        public int MinimumPlayersToStartVoting { get; set; } = minimumPlayersToStartVoting;
        public VoteType Type { get; set; } = type;
        public string? Args { get; set; } = args;
        public ConcurrentDictionary<ulong, byte> PlayersVoted { get; set; } = [];

        private DateTime CreatedTime { get; set; } = DateTime.Now;

        public void SetActive(bool active)
        {
            Active = active;
        }

        public bool GetActive()
        {
            return Active;
        }

        public DateTime NextVoting()
        {
            return CreatedTime.AddSeconds(TimeToVote + TimeToNextVoting);
        }

        public DateTime NextSameVoting()
        {
            return CreatedTime.AddSeconds(TimeToVote + TimeToNextSameVoting);
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
        private static readonly ConcurrentDictionary<VoteType, string> names = new(
        [
            new KeyValuePair<VoteType, string>(VoteType.StartGame, "css_start"),
            new KeyValuePair<VoteType, string>(VoteType.PauseGame, "css_pause"),
            new KeyValuePair<VoteType, string>(VoteType.ShuffleTeam, "css_shuffle"),
            new KeyValuePair<VoteType, string>(VoteType.SwapTeam, "css_swap"),
            new KeyValuePair<VoteType, string>(VoteType.ChangeMap, "css_map"),
            new KeyValuePair<VoteType, string>(VoteType.SetScore, "css_setscore"),
        ]);

        public static string GetCommand(VoteType type) => names[type];
    }
}
