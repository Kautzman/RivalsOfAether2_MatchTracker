namespace Slipstream.Models
{
    public class RivalsCharacter
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Patch { get; set; }
        public string IconRef { get; set; } = "/Resources/CharacterIcons/empty.png";
        public string PortraitRef { get; set; }

        public RivalsCharacter(RivalsCharacterRecord record)
        {
            ID = record.ID;
            Name = record.Name;
            Patch = record.Patch;
            IconRef = record.IconRef;
            PortraitRef = record.PortraitRef;
        }

        public RivalsCharacter() { }
    }

    // This is what gets read from the DB literally.  We might transform some data before writing it back to the RivalsCharacter object
    public record RivalsCharacterRecord
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Patch { get; set; }
        public string IconRef { get; set; }
        public string PortraitRef { get; set; }
    }
}
