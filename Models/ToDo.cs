using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicBridge.Models
{
    public class ToDo 
    {
        public string EntityId { get; set; }
        public int TranNum { get; set; }
        public int EntityNum { get; set; }
        public DateTime StartDtAct { get; set; }
        public string StartTmAct { get; set; }
        public string ActionGroup { get; set; }
        public string TranText { get; set; }
        public string TranTextAlias { get; set; }
        public string ActCode { get; set; }
        public string LocCode { get; set; }
        public int Priority { get; set; }
        public string TheStatus { get; set; }
        public string DocLoc { get; set; }
        public int ReservedF2 { get; set; }
        public double AmountBilled { get; set; }
        public string AssignedTo { get; set; }
        public List<ScanItem> ScanItems { get; set; }
        public IEnumerable<IAutomation> Automations { get; set; }
        public List<UserInput> UserInputs { get; set; }

    }
}
