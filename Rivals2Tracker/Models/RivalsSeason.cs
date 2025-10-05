using System.Collections.ObjectModel;

namespace Slipstream.Models
{
    public class RivalsSeason
    {
        public long ID { get; set; }
        public string Label { get; set; }
        public string Patch { get; set; }
        public bool IsCurrentSeason { get; set; }
        public ObservableCollection<RivalsMatch> MatchsPlayed { get; set; } = new();
    }
}
