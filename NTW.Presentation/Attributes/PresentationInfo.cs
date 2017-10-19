using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace NTW.Presentation.Attributes
{
    public class PresentationInfo : System.Attribute
    {
        public string CaptionName { get; set; }

        public TextWrapping PresentCaption { get; set; }

        public double MaxHeight { get; set; }

        public double MinHeight { get; set; }

        public double MaxWidth { get; set; }

        public double MinWidth { get; set; }

        public VerticalAlignment VAlignment { get; set; }

        public HorizontalAlignment HAlignment { get; set; }
    }
}
