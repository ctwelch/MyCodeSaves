using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicBridge.Models
{
    public class ScanItem
    {
        public int ScanId { get; set; }
        public string DocLoc { get; set; }
        public string DocLocAlias { get; set; }
        public string ActionGroup { get; set; }
    }
}
