using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace RtmpSharp.IO.AMF3
{
    [Serializable]
    [TypeConverter(typeof(ArrayCollectionConverter))]
    [SerializedName("flex.messaging.io.ArrayCollection")]
    public class ArrayCollection : IList<object>//, IExternalizable
    {
        private List<object> underlying=new List<object>();
        public object this[int index] { get => underlying[index]; set => underlying[index] = value; }

        [SerializedName("source")]
        public object[] Source
        {
            set
            {
                underlying = new List<object>(value);
            }
            get
            {
                return underlying.ToArray();
            }
        }

        public int Count => underlying.Count;

        public bool IsReadOnly => false;

        public void Add(object item)
        {
            underlying.Add(item);
        }

        public void Clear()
        {
            underlying.Clear();
        }

        public bool Contains(object item)
        {
            return underlying.Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            underlying.CopyTo(array, arrayIndex);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return underlying.GetEnumerator();
        }

        public int IndexOf(object item)
        {
            return underlying.IndexOf(item);
        }

        public void Insert(int index, object item)
        {
            underlying.Insert(index, item);
        }

        public void ReadExternal(IDataInput input)
        {
            var obj = input.ReadObject() as object[];
            if (obj != null)
                underlying.AddRange(obj);
        }

        public bool Remove(object item)
        {
            return underlying.Remove(item);
        }

        public void RemoveAt(int index)
        {
            underlying.RemoveAt(index);
        }

        public void WriteExternal(IDataOutput output)
        {
            output.WriteObject(this.ToArray());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return underlying.GetEnumerator();
        }
    }

    public class ArrayCollectionConverter : TypeConverter
    {
        static readonly Type[] ConvertibleTypes = new[]
        {
            typeof(ArrayCollection),
            typeof(System.Collections.IList)
        };

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            return MiniTypeConverter.ConvertTo(value, destinationType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType.IsArray || ConvertibleTypes.Any(x => x == destinationType);

        }
    }
}
