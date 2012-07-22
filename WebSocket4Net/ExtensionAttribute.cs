using System;
using System.Collections.Generic;
using System.Text;

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class ExtensionAttribute : Attribute
    {
        public ExtensionAttribute() { }
    }
}
