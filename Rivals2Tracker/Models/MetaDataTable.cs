using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rivals2Tracker.Models
{
    class MetaDataTable
    {
        public string TableTitle { get; set; }
        public string Patch { get; set; }
        public ObservableCollection<CharacterMetadata> CharacterData { get; set; }
    }
}
