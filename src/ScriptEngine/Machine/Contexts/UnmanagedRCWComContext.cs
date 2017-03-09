/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
//#if !__MonoCS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    class UnmanagedRCWComContext : COMWrapperContext
    {
        private object _instance;

        private const uint E_DISP_MEMBERNOTFOUND = 0x80020003;
        private bool? _isIndexed;
        private readonly Dictionary<string, int> _dispIdIndexes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
        private readonly List<int> _dispIds = new List<int>();
        
        public UnmanagedRCWComContext(object instance)
        {
            _instance = instance;
            InitByInstance();
        }

        private void InitByInstance()
        {
            if (!DispatchUtility.ImplementsIDispatch(_instance))
            {
                _instance = null;
                throw new RuntimeException("Объект не реализует IDispatch.");
            }
        }

        protected override void Dispose(bool manualDispose)
        {
            base.Dispose(manualDispose);

            _dispIdIndexes.Clear();
            _dispIds.Clear();

            if (_instance != null)
            {
                System.Runtime.InteropServices.Marshal.ReleaseComObject(_instance);
                _instance = null;
            }
        }

        public override IEnumerator<IValue> GetEnumerator()
        {
            var comType = _instance.GetType();
            System.Collections.IEnumerator comEnumerator;

            try
            {

                comEnumerator = (System.Collections.IEnumerator)comType.InvokeMember("[DispId=-4]",
                                        BindingFlags.InvokeMethod,
                                        null,
                                        _instance,
                                        new object[0]);
            }
            catch (TargetInvocationException e)
            {
                uint hr = (uint)System.Runtime.InteropServices.Marshal.GetHRForException(e.InnerException);
                if (hr == E_DISP_MEMBERNOTFOUND)
                {
                    throw RuntimeException.IteratorIsNotDefined();
                }
                else
                {
                    throw;
                }
            }

            while (comEnumerator.MoveNext())
            {
                yield return CreateIValue(comEnumerator.Current);
            }

        }

        public override bool IsIndexed
        {
            get
            {
                if (_isIndexed == null)
                {
                    try
                    {

                        var comValue = _instance.GetType().InvokeMember("[DispId=0]",
                                                BindingFlags.InvokeMethod | BindingFlags.GetProperty,
                                                null,
                                                _instance,
                                                new object[0]);
                        _isIndexed = true;
                    }
                    catch (TargetInvocationException e)
                    {
                        uint hr = (uint)System.Runtime.InteropServices.Marshal.GetHRForException(e.InnerException);
                        if (hr == E_DISP_MEMBERNOTFOUND)
                        {
                            _isIndexed = false;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                return (bool)_isIndexed;
            }
        }

        public override object UnderlyingObject
        {
            get
            {
                return _instance;
            }
        }

        public override int FindProperty(string name)
        {
            var idx = FindMemberIndex(name);
            if (idx < 0)
                throw RuntimeException.PropNotFoundException(name);

            return idx;
        }

        private int GetDispIdByIndex(int index)
        {
            return _dispIds[index];
        }

        public override bool IsPropReadable(int propNum)
        {
            return true;
        }

        public override bool IsPropWritable(int propNum)
        {
            return true;
        }

        public override IValue GetPropValue(int propNum)
        {
            try
            {
                try
                {
                    var result = DispatchUtility.Invoke(_instance, GetDispIdByIndex(propNum), null);
                    return CreateIValue(result);
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
            catch (System.MissingMemberException)
            {
                throw RuntimeException.PropNotFoundException("dispid[" + GetDispIdByIndex(propNum) + "]");
            }
            catch (System.MemberAccessException)
            {
                throw RuntimeException.PropIsNotReadableException("dispid[" + GetDispIdByIndex(propNum) + "]");
            }
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            try
            {
                try
                {
                    object argToPass;
                    if(newVal.DataType == Machine.DataType.Date)
                    {
                        var date = newVal.AsDate();
                        if(date == DateTime.MinValue)
                        {
                            argToPass = new DateTime(100, 1, 1); // Min OLEAuth Date
                        }
                        else
                        {
                            argToPass = MarshalIValue(newVal);
                        }
                    }
                    else
                    {
                        argToPass = MarshalIValue(newVal);
                    }
                    DispatchUtility.InvokeSetProperty(_instance, GetDispIdByIndex(propNum), argToPass);
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
            catch (System.MissingMemberException)
            {
                throw RuntimeException.PropNotFoundException("dispid[" + GetDispIdByIndex(propNum) + "]");
            }
            catch (System.MemberAccessException)
            {
                throw RuntimeException.PropIsNotWritableException("dispid[" + GetDispIdByIndex(propNum) + "]");
            }
        }

        public int FindMemberIndex(string name)
        {
            int knownDiIndex;
            if (!_dispIdIndexes.TryGetValue(name, out knownDiIndex))
            {
                int dispId;
                if (DispatchUtility.TryGetDispId(_instance, name, out dispId))
                {
                    knownDiIndex = _dispIds.Count;
                    _dispIds.Add(dispId);
                    _dispIdIndexes.Add(name, knownDiIndex);
                }
                else
                {
                    knownDiIndex = -1;
                }
            }

            return knownDiIndex;
        }

        public override int FindMethod(string name)
        {
            var idx = FindMemberIndex(name);
            if (idx < 0)
                throw RuntimeException.MethodNotFoundException(name);
            
            return idx;
        }

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            return GetMethodDescription(methodNumber);
        }

        private MethodInfo GetMethodDescription(int methodNumber)
        {
            return new MethodInfo();
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            try
            {
                try
                {
                    DispatchUtility.Invoke(_instance, methodNumber, MarshalArguments(arguments));
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
            catch (System.MissingMemberException)
            {
                throw RuntimeException.MethodNotFoundException("dispid[" + methodNumber.ToString() + "]");
            }
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            try
            {
                try
                {
                    var result = DispatchUtility.Invoke(_instance, methodNumber, MarshalArguments(arguments));
                    retValue = CreateIValue(result);
                }
                catch (System.Reflection.TargetInvocationException e)
                {
                    throw e.InnerException;
                }
            }
            catch (System.MissingMemberException)
            {
                throw RuntimeException.MethodNotFoundException("dispid[" + methodNumber.ToString() + "]");
            }
        }

    }
}
//#endif