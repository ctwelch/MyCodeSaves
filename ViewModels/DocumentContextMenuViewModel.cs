using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BasicBridge.ViewModels
{
    public class DocumentContextMenuViewModel : BaseViewModel
    {
        public string Header { get; set; }
        public ICommand ContextMenuAction { get; set; }
    }
}
