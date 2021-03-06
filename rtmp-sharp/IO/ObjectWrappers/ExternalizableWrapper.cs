﻿using System;

namespace RtmpSharp.IO.ObjectWrappers
{
    class ExternalizableWrapper : IObjectWrapper
    {
        readonly SerializationContext serializationContext;

        public ExternalizableWrapper(SerializationContext serializationContext)
        {
            this.serializationContext = serializationContext;
        }

        public bool GetIsDynamic(object instance)
        {
            return false;
        }
        
        public bool GetIsExternalizable(object instance)
        {
            return false;
        }

        public ClassDescription GetClassDescription(object obj)
        {
            var type = obj.GetType();
            var typeName = serializationContext.GetAlias(type.FullName);
            return new ExternalizableClassDescription(typeName, new IMemberWrapper[] { }, true, false);
        }

        class ExternalizableClassDescription : BasicObjectWrapper.BasicObjectClassDescription
        {
            internal ExternalizableClassDescription(string name, IMemberWrapper[] members, bool externalizable, bool dynamic)
                : base(name, members, externalizable, dynamic)
            {
            }
        }
    }
}
