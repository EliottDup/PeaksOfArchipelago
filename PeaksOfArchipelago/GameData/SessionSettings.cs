using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PeaksOfArchipelago.GameData
{


    internal class SessionSettings
    {
        public enum RopeUnlockMode
        {
            INSTANT = 0,
            EARLY = 1,
            NORMAL = 2
        }

        public enum Goal
        {
            ALL_PEAKS = 0,
            ALL_ARTEFACTS = 1,
            ALL_ARTEFACTS_AND_PEAKS = 2,
            ALL_TIME_ATTACK = 3,
            ALL = 4
        }

        public enum GameMode
        {
            BOOK_UNLOCK = 0,
            PEAK_UNLOCK = 1
        }

        public enum Book
        {
            FUNDAMENTALS = 1,
            INTERMEDIATE = 2,
            ADVANCED = 4,
            EXPERT = 8,
        }

        public bool deathLinkEnabled = false;
        public RopeUnlockMode ropeUnlockMode = RopeUnlockMode.NORMAL;
        public Goal goal = Goal.ALL_PEAKS;
        public GameMode gameMode = GameMode.BOOK_UNLOCK;
        public int booksEnabled = 0;
        public bool excludeST = false;
        public bool includeFreeSolo = false;
        public bool includeTimeAttack = false;

        public SessionSettings(bool deathLinkEnabled, RopeUnlockMode ropeUnlockMode, Goal goal, GameMode gameMode, int booksEnabled, bool excludeST)
        {
            this.deathLinkEnabled = deathLinkEnabled;
            this.ropeUnlockMode = ropeUnlockMode;
            this.goal = goal;
            this.gameMode = gameMode;
            this.booksEnabled = booksEnabled;
            this.excludeST = excludeST;
        }

        public SessionSettings(Dictionary<string, object> optionsDict)
        {
            PeaksOfArchipelago.Logger.LogInfo("Loading session settings from options dict");
            foreach (string s in optionsDict.Keys)
            {
                PeaksOfArchipelago.Logger.LogInfo($"Option key: {s}, value: {optionsDict[s].ToString()}");
            }

            deathLinkEnabled = LoadIntFromDict(optionsDict, "deathLink", false) == 1;
            ropeUnlockMode = (RopeUnlockMode)LoadIntFromDict(optionsDict, "ropeUnlockMode", RopeUnlockMode.NORMAL);
            goal = (Goal)LoadIntFromDict(optionsDict, "goal", Goal.ALL_PEAKS);
            gameMode = (GameMode)LoadIntFromDict(optionsDict, "gameMode", GameMode.BOOK_UNLOCK);
            excludeST = LoadIntFromDict(optionsDict, "disableSolemnTempest", true) == 1;
            includeFreeSolo = LoadIntFromDict(optionsDict, "includeFreeSolo", false) == 1;
            includeTimeAttack = LoadIntFromDict(optionsDict, "includeTimeAttack", false) == 1;
        }

        int LoadIntFromDict(Dictionary<string, object> dict, string v, object defaultValue)
        {
            if (dict.TryGetValue(v, out var value))
            {
                PeaksOfArchipelago.Logger.LogInfo($"{v}: {value}");
                return Convert.ToInt32(value);
            }
            PeaksOfArchipelago.Logger.LogWarning($"{v} not found, defaulting to {defaultValue}");
            return Convert.ToInt32(defaultValue);
        }

        public bool IsBookEnabled(Book book)
        {
            return true;
            return (booksEnabled & (int)book) != 0;
        }

        

    }
}
