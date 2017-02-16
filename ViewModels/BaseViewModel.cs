using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicBridge.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        //protected virtual void FlagPropertyChanged([CallerMemberName] string PropertyName = null)
        protected virtual void FlagPropertyChanged(string PropertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
    }
}
