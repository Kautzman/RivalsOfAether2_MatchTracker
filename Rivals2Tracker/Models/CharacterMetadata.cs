using Prism.Mvvm;

namespace Slipstream.Models
{
    class CharacterMetadata : BindableBase
    {
        private RivalsCharacter _character;
        public RivalsCharacter Character
        {
            get { return _character; }
            set { SetProperty(ref _character, value); }
        }

        public WeightedCharacterMetadata WeightedData { get; set; } = new();

        public int Wins { get; set; }
        public int Loses { get; set; }
        public int TotalGames
        {
            get
            {
                return Wins + Loses;
            }
        }

        public string RecordString
        {
            get
            {
                return $"{Wins} - {Loses}";
            }
        }

        public float WinRatio
        {
            get
            {
                return ((float)Wins / (float)TotalGames) * 100.0f;
            }
        }

        public string WinRatioString
        {
            get => $"{WinRatio.ToString("n2")}%";
        }

        public CharacterMetadata(RivalsCharacter character)
        {
            Character = character;
            Wins = 0;
            Loses = 0;
        }
    }
}
