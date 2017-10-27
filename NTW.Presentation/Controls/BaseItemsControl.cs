using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections;
using System.ComponentModel;
using NTW.Commands;

namespace NTW.Presentation
{
    internal class BaseItemsControl : ListBox
    {

        public static readonly DependencyProperty ContextProperty =
              DependencyProperty.Register("Context", typeof(object), typeof(BaseItemsControl));

        public virtual Command RemoveCommand { get; set; }
    }


}
