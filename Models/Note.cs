using System;

namespace BasicBridge.Models
{
    public class Note
    {
        public int TranNum { get; set; }
        public int EntityNum { get; set; }
        public string ActCode { get; set; }
        public DateTime StartDtAct { get; set; }
        public string TranText { get; set; }
        public int CustomUserNoteId { get; set; }
        public int AmountBilled { get; set; }
        public int ReservedF2 { get; set; }
    }
}
