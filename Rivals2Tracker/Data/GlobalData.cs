using Slipstream.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Permissions;

namespace Slipstream.Data
{
    public static class GlobalData
    {
        public static IntPtr MainWindowHandle { get; set; }
        public static RivalsSeason CurrentSeason = RivalsSeason.Season3;
        public static string MyName = "Kadecgos";
        public static bool IsSaveCaptures = false;
        public static bool IsPlayAudio = true;
        public static uint HotKeyCode = 0;
        public static uint ModifierCode = 0;
        public static int BestOf;
        public static ObservableCollection<RivalsStage> AllStages = new()
        {
            new RivalsStage("Aetherian Forest", "pack://application:,,,/Resources/StageIcons/AetherianForestV.png", false),
            new RivalsStage("Godai Delta", "pack://application:,,,/Resources/StageIcons/GodaiDeltaV.png", false),
            new RivalsStage("Hodojo", "pack://application:,,,/Resources/StageIcons/HodojoV.png", false),
            new RivalsStage("Julesvale", "pack://application:,,,/Resources/StageIcons/JulesvaleV.png", false),
            new RivalsStage("Merchant Port", "pack://application:,,,/Resources/StageIcons/MerchantPortV.png", false),
            new RivalsStage("Air Armada", "pack://application:,,,/Resources/StageIcons/AirArmadaV.png", true),
            new RivalsStage("Fire Capital", "pack://application:,,,/Resources/StageIcons/FireCapitalV.png", true),
            new RivalsStage("Hyperborean Harbor", "pack://application:,,,/Resources/StageIcons/HyperboreanHarborV.png", true),
            new RivalsStage("Rock Wall", "pack://application:,,,/Resources/StageIcons/RockWallV.png", true),
            new RivalsStage("Tempest Peak", "pack://application:,,,/Resources/StageIcons/TempestPeakV.png", true),
        };

        public static List<string> AllCharacters = new List<string>
        {
            "Absa",
            "Clairen",
            "Etalus",
            "Fleet",
            "Kragg",
            "Loxodont",
            "Maypul",
            "Olympia",
            "Orcane",
            "Ranno",
            "Wrastor",
            "Forsburn",
            "Zetterburn",
            "Unknown"
        };

        public static Dictionary<string, string> CharacterImageDict = new Dictionary<string, string>
        {
            { "Absa", "/Resources/CharacterIcons/absa.png" },
            { "Clairen", "/Resources/CharacterIcons/clairen.png" },
            { "Etalus", "/Resources/CharacterIcons/etalus.png" },
            { "Fleet", "/Resources/CharacterIcons/fleet.png" },
            { "Kragg",  "/Resources/CharacterIcons/kragg.png" },
            { "Loxodont", "/Resources/CharacterIcons/loxodont.png" },
            { "Maypul", "/Resources/CharacterIcons/maypul.png" },
            { "Olympia", "/Resources/CharacterIcons/olympia.png" },
            { "Orcane", "/Resources/CharacterIcons/orcane.png" },
            { "Ranno", "/Resources/CharacterIcons/ranno.png" },
            { "Wrastor", "/Resources/CharacterIcons/wrastor.png" },
            { "Forsburn", "/Resources/CharacterIcons/yeen.png" },
            { "Zetterburn", "/Resources/CharacterIcons/zetterburn.png" }
        };

        // TODO:  Update resources to point to portraits, not icons
        public static Dictionary<string, string> CharacterPortraitDict = new Dictionary<string, string>
        {
            { "Absa", "/ImageResources/Portraits/absa_portrait.png" },
            { "Clairen", "/ImageResources/Portraits/clairen_portrait.png" },
            { "Etalus", "/ImageResources/Portraits/etalus_portrait.png" },
            { "Fleet", "/ImageResources/Portraits/fleet_portrait.png" },
            { "Kragg",  "/ImageResources/Portraits/kragg_portrait.png" },
            { "Loxodont", "/ImageResources/Portraits/loxodont_portrait.png" },
            { "Maypul", "/ImageResources/Portraits/maypul_portrait.png" },
            { "Olympia", "/ImageResources/Portraits/olympia_portrait.png" },
            { "Orcane", "/ImageResources/Portraits/orcane_portrait.png" },
            { "Ranno", "/ImageResources/Portraits/ranno_portrait.png" },
            { "Wrastor", "/ImageResources/Portraits/wrastor_portrait.png" },
            { "Forsburn", "/ImageResources/Portraits/forsburn_portrait.png" },
            { "Zetterburn", "/ImageResources/Portraits/zetterburn_portrait.png" }
        };

        public static Dictionary<int, string> StageIndex = new Dictionary<int, string>
        {
            { 1, "Aetherian Forest" },
            { 2, "Godei Delta" },
            { 3, "Hodojo" },
            { 4, "Julesvale" },
            { 5, "Merchant Port" },
            { 6, "Air Armada" },
            { 7, "Fire Capital" },
            { 8, "Hyperborean Harbor" },
            { 9, "Rock Wall" },
            { 10, "Tempest Peak"}
        };
    }

    public enum MatchHistoryView
    {
        Rivals,
        Players
    }

    public enum PlayerInformationView
    {
        Notes,
        Matches
    }

    public enum RivalsCharacter
    {
        Absa,
        Clairen,
        Etalus,
        Fleet,
        Kragg,
        Loxodont,
        Maypul,
        Olympia,
        Orcane,
        Ranno,
        Wrastor,
        Zetterburn,
        Galvan
    }

    public enum GameResult
    {
        Lose = 0,
        Win = 1,
        Draw = 2,
        InProgress = 3
    }
}
