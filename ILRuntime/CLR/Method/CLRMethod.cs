﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;
using ILRuntime.Runtime.Intepreter.OpCodes;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Runtime.Stack;
namespace ILRuntime.CLR.Method
{
    class CLRMethod : IMethod
    {
        MethodInfo def;
        ConstructorInfo cDef;
        List<IType> parameters;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;
        CLRType declaringType;
        ParameterInfo[] param;
        bool isConstructor;

        public IType DeclearingType
        {
            get
            {
                return declaringType;
            }
        }

        public bool HasThis
        {
            get
            {
                return isConstructor ? !cDef.IsStatic : !def.IsStatic;
            }
        }

        public CLRMethod(MethodInfo def, CLRType type, ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            this.def = def;
            declaringType = type;
            this.appdomain = domain;
            param = def.GetParameters();
            if (!def.IsGenericMethod && !def.ContainsGenericParameters)
            {
                ReturnType = domain.GetType(def.ReturnType.FullName);
                InitParameters();
            }
            isConstructor = false;
        }
        public CLRMethod(ConstructorInfo def, CLRType type, ILRuntime.Runtime.Enviorment.AppDomain domain)
        {
            this.cDef = def;
            declaringType = type;
            this.appdomain = domain;
            param = def.GetParameters();
            if (!def.IsGenericMethod && !def.ContainsGenericParameters)
            {
                ReturnType = type;
                InitParameters();
            }
            isConstructor = true;
        }

        public int ParameterCount
        {
            get
            {
                return param != null ? param.Length : 0;
            }
        }


        public List<IType> Parameters
        {
            get
            {
                if (param == null)
                {
                    InitParameters();
                }
                return parameters;
            }
        }

        public IType ReturnType
        {
            get;
            private set;
        }

        public bool IsConstructor
        {
            get
            {
                return def.IsConstructor;
            }
        }

        void InitParameters()
        {
            parameters = new List<IType>();
            foreach (var i in param)
            {
                IType type = appdomain.GetType(i.ParameterType.FullName);
                parameters.Add(type);
            }
        }

        public unsafe object Invoke(StackObject* esp, List<object> mStack)
        {
            if (isConstructor)
            {
                throw new NotImplementedException();
            }
            else
            {
                int paramCount = ParameterCount;
                object[] param = new object[paramCount];
                for (int i = 1; i <= paramCount; i++)
                {
                    var p = esp - i;
                    param[paramCount - i] = p->ToObject(mStack);
                }
                object instance = null;
                if (!def.IsStatic)
                {
                    instance = (esp - paramCount - 1)->ToObject(mStack);
                }
                var res = def.Invoke(instance, param);
                return res;
            }
        }
    }
}
