using Rivals2Tracker.Data;
using System;
using System.Windows.Media;

namespace Rivals2Tracker.Services
{
    public static class AudioService
    {
        private static MediaPlayer mediaPlayer;

        public static void Initialize()
        {
            mediaPlayer = new MediaPlayer();
            mediaPlayer.Volume = 0.5;
            mediaPlayer.Open(new Uri("pack://siteoforigin:,,,/Resources/Audio/error.mp3"));
        }

        public static void PlayError()
        {
            if (GlobalData.IsPlayAudio)
            {
                mediaPlayer.Play();
            }
        }
    }
}
