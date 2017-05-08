using System;
using System.ComponentModel;

namespace RtmpSharp
{
    public static class TypeSerializer
    {
        public static void RegisterTypeConverters()
        {
            TypeDescriptor.AddAttributes(typeof(string), new TypeConverterAttribute(typeof(RtmpSharp.IO.TypeConverters.StringConverter)));
            TypeDescriptor.AddAttributes(typeof(DateTime), new TypeConverterAttribute(typeof(RtmpSharp.IO.TypeConverters.DateConverter)));
            TypeDescriptor.AddAttributes(typeof(double), new TypeConverterAttribute(typeof(RtmpSharp.IO.TypeConverters.DoubleConverter)));
        }
    }
}
