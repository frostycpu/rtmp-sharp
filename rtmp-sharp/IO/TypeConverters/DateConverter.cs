using System;
using System.ComponentModel;
using System.Globalization;

namespace RtmpSharp.IO.TypeConverters
{
    // Adds support for converter converting a string to a char
    class DateConverter : System.ComponentModel.DateTimeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(int) || sourceType == typeof(long) || sourceType == typeof(double))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is int)
                return new DateTime((int)value);
            if (value is long)
                return new DateTime((long)value);
            if (value is double)
                return new DateTime((long)(double)value);

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {

            if (destinationType == typeof(int))
                return (int)((DateTime)value).Ticks;
            if (destinationType == typeof(long))
                return ((DateTime)value).Ticks;
            if (destinationType == typeof(double))
                return (double)((DateTime)value).Ticks;

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(int) || destinationType == typeof(long) || destinationType == typeof(double))
                return true;
            return base.CanConvertTo(context, destinationType);
        }
    }
    class DoubleConverter : System.ComponentModel.DoubleConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(DateTime))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is DateTime)
                return ((DateTime)value).Ticks;

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {

            if (destinationType == typeof(DateTime))
                return new DateTime((long)(double)value);

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DateTime))
                return true;
            return base.CanConvertTo(context, destinationType);
        }
    }
    class Int32Converter : System.ComponentModel.Int32Converter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(DateTime))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is DateTime)
                return ((DateTime)value).Ticks;

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {

            if (destinationType == typeof(DateTime))
                return new DateTime((int)value);

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DateTime))
                return true;
            return base.CanConvertTo(context, destinationType);
        }
    }
}
