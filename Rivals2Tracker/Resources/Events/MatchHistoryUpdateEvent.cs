using Prism.Events;
using Rivals2Tracker.Models;
using System;

namespace Rivals2Tracker.Resources.Events
{
    class MatchHistoryUpdateEvent
    {
        public static event Action<MatchResult> MatchSaved;

        public static void PublishMatchSaved(MatchResult match)
        {
            MatchSaved?.Invoke(match);
        }
    }
}
