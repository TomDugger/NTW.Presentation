using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTW.Presentation.Attributes
{
    public class PresentationInterface : System.Attribute
    {
        private bool _FindHeirsPropertyTypeInterface = false;

        public bool FindHeirsPropertyTypeInterface
        {
            get { return _FindHeirsPropertyTypeInterface; }
            set { _FindHeirsPropertyTypeInterface = value; }
        }
    }
}
