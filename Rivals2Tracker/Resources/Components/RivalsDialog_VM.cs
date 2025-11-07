using Prism.Commands;
using System;
using System.Threading.Tasks;

namespace Slipstream.Resources.Components
{
    public class RivalsDialog_VM
    {
        private readonly TaskCompletionSource<RivalsDialogResult> _tcs = new TaskCompletionSource<RivalsDialogResult>();

        public Action Close { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Option1 { get; set; } = string.Empty;
        public string Option2 { get; set; } = string.Empty;
        public string Option3 { get; set; } = string.Empty;
        public string MessageContent { get; set; } = string.Empty;

        public DelegateCommand Option1_Command { get; set; }
        public DelegateCommand Option2_Command { get; set; }
        public DelegateCommand Option3_Command { get; set; }

        public RivalsDialog_VM(string _title, string _messageConent, string _option1, string _option2, string _option3)
        {
            Title = _title;
            Option1 = _option1;
            Option2 = _option2;
            Option3 = _option3;
            MessageContent = _messageConent;

            Option1_Command = new DelegateCommand(() => SelectOption(RivalsDialogResult.OK));
            Option2_Command = new DelegateCommand(() => SelectOption(RivalsDialogResult.Cancel));
            Option3_Command = new DelegateCommand(() => SelectOption(RivalsDialogResult.Other));
        }

        private void SelectOption(RivalsDialogResult result)
        {
            _tcs.TrySetResult(result);
            Close?.Invoke();
        }

        public Task<RivalsDialogResult> GetDialogResultAsync()
        {
            return _tcs.Task;
        }
    }

    public enum RivalsDialogResult
    {
        OK,
        Cancel,
        Other
    }
}
