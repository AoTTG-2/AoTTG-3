﻿using Assets.Scripts.Characters.Titan;
using Assets.Scripts.Settings.Titans;
using System.Collections.Generic;

namespace Assets.Scripts.Settings.Gamemodes
{
    public class TrostSettings : GamemodeSettings
    {
        public TrostSettings()
        {
            Titan = new SettingsTitan
            {
                Start = 2
            };
            GamemodeType = GamemodeType.Trost;
            PlayerShifters = false;
            Titan = new SettingsTitan()
            {
                Mindless = new MindlessTitanSettings
                {
                    Disabled = new List<MindlessTitanType> {MindlessTitanType.Punk}
                }
            };
        }
    }
}
