﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NTW.Presentation.Attribute
{
    public class PresentationInterfaceInfo : System.Attribute
    {

    }

    public enum PresentaryInterfaceType
    {
        OnlyInterfaceProperty,
        AllProperty,
        OnlyObjectPropertyNo
    }
}
