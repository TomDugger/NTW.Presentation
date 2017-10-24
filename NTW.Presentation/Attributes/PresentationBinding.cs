using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTW.Presentation.Attributes
{
    public class PresentationBinding : System.Attribute
    {
        private bool _IsAsync = true;

        public bool IsAsync {
            get { return _IsAsync; }
            set { _IsAsync = value; }
        }
    }
}
