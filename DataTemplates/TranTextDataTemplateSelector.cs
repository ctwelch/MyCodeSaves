using BasicBridge.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace BasicBridge.DataTemplates
{
    public class TranTextDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ConstantTranTextTemplate { get; set; }
        public DataTemplate UserInputTranTextTemplate { get; set; }
        public DataTemplate ToDoPropertyTranTextTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var dpType = item.GetType();

            if (dpType == typeof(ConstantTranTextSegment))
            {
                return ConstantTranTextTemplate;
            }
            if (dpType == typeof(UserInputTranTextSegment))
            {
                return UserInputTranTextTemplate;
            }
            if (dpType == typeof(ToDoPropertyTranTextSegment))
            {
                return ToDoPropertyTranTextTemplate;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
