﻿using Assets.Scripts.Gamemode;
using System;

namespace Assets.Scripts.Settings.Gamemodes
{
    public class KillTitansSettings : GamemodeSettings
    {
        public KillTitansSettings() { }

        public KillTitansSettings(Difficulty difficulty) : base(difficulty)
        {
            switch (difficulty)
            {
                case Difficulty.Easy:
                case Difficulty.Normal:
                case Difficulty.Hard:
                case Difficulty.Abnormal:
                case Difficulty.Realism:
                    GamemodeType = GamemodeType.Titans;
                    RespawnMode = RespawnMode.NEVER;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(difficulty), difficulty, null);
            }
        }
    }
}
