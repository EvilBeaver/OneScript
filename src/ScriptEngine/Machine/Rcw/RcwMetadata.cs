using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using FUNCDESC = System.Runtime.InteropServices.ComTypes.FUNCDESC;
using FUNCFLAGS = System.Runtime.InteropServices.ComTypes.FUNCFLAGS;
using TYPEATTR = System.Runtime.InteropServices.ComTypes.TYPEATTR;
using INVOKEKIND = System.Runtime.InteropServices.ComTypes.INVOKEKIND;
using VARDESC = System.Runtime.InteropServices.ComTypes.VARDESC;

namespace ScriptEngine.Machine.Rcw
{
    public class RcwMetadata
    {
        //https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-oaut/3fe7db9f-5803-4dc4-9d14-5425d3f5461f
        private const short VT_VOID = 0x0018;

        public RcwMembersMetadataCollection<RcwPropertyMetadata> Properties { get; }

        public RcwMembersMetadataCollection<RcwMethodMetadata> Methods { get; }

        public RcwMetadata(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (!Marshal.IsComObject(instance))
                throw new Exception("Instance is not a COM object");

            Properties = new RcwMembersMetadataCollection<RcwPropertyMetadata>();

            Methods = new RcwMembersMetadataCollection<RcwMethodMetadata>();

            LoadMetadata(instance);
        }

        private void LoadMetadata(object instance)
        {
            var typesCount = DispatchUtility.GetTypeInfoCount(instance);

            if (typesCount == 0)
                return;

            var typeInfo = DispatchUtility.GetITypeInfo(instance);

            typeInfo.GetTypeAttr(out var ptAttr);

            var typeAttr = Marshal.PtrToStructure<TYPEATTR>(ptAttr);

            LoadVars(typeInfo, typeAttr);
            LoadFuncs(typeInfo, typeAttr);

            Marshal.Release(ptAttr);
        }

        private void LoadVars(ITypeInfo typeInfo, TYPEATTR typeAttr)
        {
            //TODO: Узнать что сюда прилетает
            return;
            for (var i = 0; i < typeAttr.cVars; i++)
            {
                typeInfo.GetVarDesc(i, out var ptVarDesc);
                var varDesc = Marshal.PtrToStructure<VARDESC>(ptVarDesc);

                Marshal.Release(ptVarDesc);
            }

        }

        private void LoadFuncs(ITypeInfo typeInfo, TYPEATTR typeAttr)
        {
            var skippingFlags = FUNCFLAGS.FUNCFLAG_FRESTRICTED | FUNCFLAGS.FUNCFLAG_FHIDDEN;

            for (var i = 0; i < typeAttr.cFuncs; i++)
            {
                typeInfo.GetFuncDesc(i, out var ptFuncDesc);
                var funcDesc = Marshal.PtrToStructure<FUNCDESC>(ptFuncDesc);

                var currentFlags = (FUNCFLAGS)funcDesc.wFuncFlags;

                if ((currentFlags & skippingFlags) != 0)
                {
                    Marshal.Release(ptFuncDesc);
                    continue;
                }

                var arrOfNames = new string[1];
                var cNames = arrOfNames.Length;
                var dispId = funcDesc.memid;

                typeInfo.GetNames(dispId, arrOfNames, cNames, out var names);

                if (names == 0)
                {
                    Marshal.Release(ptFuncDesc);
                    continue;
                }

                var memberName = arrOfNames[0];

                if (funcDesc.invkind == INVOKEKIND.INVOKE_FUNC)
                {
                    var isFunc = funcDesc.elemdescFunc.tdesc.vt != VT_VOID;
                    AddMethod(memberName, dispId, isFunc);
                }
                else
                {
                    var prop = GetOrAddProperty(memberName, dispId);

                    if (funcDesc.invkind == INVOKEKIND.INVOKE_PROPERTYGET)
                        prop.IsReadable = true;
                    else
                        prop.IsWritable = true;
                }

                Marshal.Release(ptFuncDesc);
            }
        }

        private RcwPropertyMetadata GetOrAddProperty(string propName, int dispId)
        {
            if (Properties.Names.TryGetValue(propName, out var md)) return md;

            md = new RcwPropertyMetadata(propName, dispId);
            Properties.Add(md);

            return md;
        }

        private void AddMethod(string methodName, int dispId, bool isFunc)
        {
            var md = new RcwMethodMetadata(methodName, dispId, isFunc);
            Methods.Add(md);
        }
    }

    
}