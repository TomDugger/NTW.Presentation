using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace NTW.Presentation.Attribute
{
    public class PresentationInfo : System.Attribute
    {
        public string CaptionName { get; set; }

        public TextWrapping PresentCaption { get; set; }
    }
}
