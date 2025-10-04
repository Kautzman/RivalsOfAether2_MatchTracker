using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Slipstream.Windows
{
    class FirstStart_VM : BindableBase
    {

        public Action Close { get; set; }

        private List<string> TutorialTextCollection = new();
        private List<string> TutorialImageCollection = new();
        private List<string> TutorialTitleCollection = new();
        private List<string> TutorialImageCaptionCollection = new();

        private string _sectionTitle = String.Empty;
        public string SectionTitle
        {
            get { return _sectionTitle; }
            set { SetProperty(ref _sectionTitle, value); }
        }

        private string _sectionText = String.Empty;
        public string SectionText
        {
            get { return _sectionText; }
            set { SetProperty(ref _sectionText, value); }
        }

        private string _sectionImage = String.Empty;
        public string SectionImage
        {
            get { return _sectionImage; }
            set { SetProperty(ref _sectionImage, value); }
        }

        private string _sectionImageCaption = String.Empty;
        public string SectionImageCaption
        {
            get { return _sectionImageCaption; }
            set { SetProperty(ref _sectionImageCaption, value); }
        }

        public Visibility SettingsVisibility
        {
            get
            {
                if (pageIndex == 2)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
        }

        private int pageIndex = 0;
        private const int pageIndexMax = 4;

        public DelegateCommand PreviousSlideCommand { get; private set; }
        public DelegateCommand NextSlideCommand { get; private set; }
        public DelegateCommand OpenSettingsCommand { get; private set; }

        public FirstStart_VM()
        {
            PreviousSlideCommand = new DelegateCommand(PreviousSlide);
            NextSlideCommand = new DelegateCommand(NextSlide);
            OpenSettingsCommand = new DelegateCommand(OpenSettings);

            TutorialTextCollection.Clear();
            TutorialImageCollection.Clear();
            TutorialTitleCollection.Clear();
            TutorialImageCaptionCollection.Clear();

            BuildTutorialTextCollection();
            BuildTitleCollection();
            BuildImageCollection();
            BuildCaptionCollection();

            BuildPage(pageIndex);
        }

        private void PreviousSlide()
        {
            pageIndex = Math.Clamp(--pageIndex, 0, pageIndexMax);
            BuildPage(pageIndex);
        }

        private void NextSlide()
        {
            if (pageIndex == pageIndexMax)
            {
                Close?.Invoke();
            }

            pageIndex = Math.Clamp(++pageIndex, 0, pageIndexMax);
            BuildPage(pageIndex);
        }

        private void BuildPage(int index)
        {
            RaisePropertyChanged(nameof(SettingsVisibility));
            SectionTitle = TutorialTitleCollection[index];
            SectionText = TutorialTextCollection[index];
            SectionImage = TutorialImageCollection[index];
            SectionImageCaption = TutorialImageCaptionCollection[index];
        }

        private void OpenSettings()
        {
            Settings settings = new Settings();

            settings.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            settings.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            bool? result = settings.ShowDialog();
        }

        public void BuildTitleCollection()
        {
            TutorialTitleCollection.Add("Introduction to Slipstream (1/5)");
            TutorialTitleCollection.Add("Understanding the Workflow (2/5)");
            TutorialTitleCollection.Add("Setup Your Settings! (3/5)");
            TutorialTitleCollection.Add("Check Out the Features (4/5)");
            TutorialTitleCollection.Add("Complain Loudly about Bugs! (5/5)");
        }

        private void BuildImageCollection()
        {
            TutorialImageCollection.Add("/Slipstream;component/Resources/tutorial1.jpg");
            TutorialImageCollection.Add("/Slipstream;component/Resources/tutorial2.jpg");
            TutorialImageCollection.Add("/Slipstream;component/Resources/tutorial3.jpg");
            TutorialImageCollection.Add("/Slipstream;component/Resources/tutorial4.jpg");
            TutorialImageCollection.Add("/Slipstream;component/Resources/tutorial5.jpg");
        }

        private void BuildCaptionCollection()
        {
            TutorialImageCaptionCollection.Add("Slipstream Main Window");
            TutorialImageCaptionCollection.Add("This is the window you need to capture a match on with your hotkey or the 'Start Match' button!");
            TutorialImageCaptionCollection.Add("Modifiers are supported for the 'Capture' keybind, but I personally just like using Scroll Lock.  This is a global hook and will lock the key (or key combination) out for other programs until you close Slipstream!");
            TutorialImageCaptionCollection.Add("Left click stages to mark them as 'Selected'.  Right click stages to mark them as 'Banned'");
            TutorialImageCaptionCollection.Add("Report Bugs, Issues, UX problems, and feature requests aggressively!");
        }
        public void BuildTutorialTextCollection()
        {
            TutorialTextCollection.Add(
                """
                Don't skip this!  Read the words here carefully or you'll be lost!

                Slipstream aims to provide a match tracking solution for Rivals of Aether II by making it easy to enter in match data - most of which will be automatic - and by making record-keeping simple.

                This is done by reading the screen and implementing a hotkey to capture a match even when Rivals is the focused window!

                A lot of this software is designed to be useful as a non-active window on a second monitor.  While you can use this if you have one monitor, the ideal is to have Rivals on one and Slipstream on the other.

                Click 'Next' below to go over the keywork flow concepts!  (It'll be short, I promise)
                """);

            TutorialTextCollection.Add(
                """
                To start using Slipstream, you have to understand the two primary modes it can be in:  "Waiting for Match", and "Match In Progress".

                To start a match, you need to do the following:

                1.  Start Rivals and enter a match.

                2.  At the Stage Selection screen (see image on right), you want to press your 'Capture Match' hotkey (Don't worry, we'll set this shortly).  This will automatically consume the match data (characters, names, elos) and the match will now be "In Progress".  Pressing the 'Start Match' button will do the same thing.

                2a. Slipstream relies on Optical Character Recognition (OCR) to grab the match data.  Because OCR is not 100% reliable, it will sometimes fail to grab the match data.  You might have to enter parts of it yourself.  You can edit any data once the match is "In Progress".

                3.  From the 'Active Match', you can record individual characters per game, and stage picks and bans.  Right Click to mark a stage as 'banned'.  Left click to mark it as the selected stage.

                4.  You can also record Notes for yourself at the bottom of the view.  These will show up again when you face that same player.

                5.  Once your match is complete, you can record the match with the [Win] or [Lose] Buttons.  You can discard the match with the [Cancel] button if you wish.

                6.  After the match is marked with a result, the view will return to the Recent Matches / Ready view and Slipstream is ready to start another match.

                """);

            TutorialTextCollection.Add(
                """
                Right now, we should look at the Settings Window!

                Most importantly, you should set a keybind to capture a match that is comfortable for you - I like Scroll Lock, but you can set anything you want.  At this point, you should set your Match Capture Hotkey and Your Rivals Tag.

                [Very Important] -- The Keybind you set here is a global hook!  While Slipstream is active, that keybind will work exclusively for Slipstream!  You can release it by simply closing Slipstream.
                
                A note about your Rivals tag - you should be setting it to how it appears ON THE STAGE SELECTION SCREEN.  This is where Slipstream will read it, so if you have a long name with ... at the end of it, type it as it appears on that screen!

                Click the button below to open the settings Window and set those options.  You should also set the current Rivals 2 patch for book keeping purposes here.  You can also enable and disable a couple other debugging options.
                """);

            TutorialTextCollection.Add(
                """
                Setup is now complete!  You can change your settings (and update the Patch number) in the top right hand corner of Slipstream's main window!

                With all this match tracking, you can see your win rates against characters, against other players, and even keep notes about other players you run into.  Click around to check out some of the features!    
                """);

            TutorialTextCollection.Add(
                """
                Finally - Keep in mind this is a very early release and your feedback is important.  Things you should report include:

                • Bugs that cause Slipstream to not work
                • Features or workflows you felt were difficult to understand.
                • Any features you feel would be awesome to have!

                As a beta, this is incomplete!  Known issues include:

                • Poor support on 4k monitors at full screen.
                • There might be issues with games not running at 16:9 - if you play on 21:9, 16:10, or some other goofy aspect ratio, please send me your captures in the 'OCRCapture' folder so I can add support for them!

                Expect bugs, expect jank, and except bad UX in places - Report them all!

                Finally, thank you for checking this out and I hope to continue to make this awesome
                """);
        }
    }
}
