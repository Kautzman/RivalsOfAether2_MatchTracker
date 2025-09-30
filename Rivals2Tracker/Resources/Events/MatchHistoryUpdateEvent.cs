using Prism.Events;
using Slipstream.Models;
using System;

namespace Slipstream.Resources.Events
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
