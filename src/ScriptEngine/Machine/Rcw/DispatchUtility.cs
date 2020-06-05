/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

// This code is based on
// https://www.codeproject.com/Articles/523417/Reflection-with-IDispatch-based-COM-objects

#region Using Directives

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Permissions;

#endregion

namespace ScriptEngine.Machine.Rcw
{
    /// <summary>
    /// Provides helper methods for working with COM IDispatch objects that have a registered type library.
    /// </summary>
    static class DispatchUtility
    {
        #region Private Constants
        
        private const int S_OK = 0; //From WinError.h
        private const int LOCALE_SYSTEM_DEFAULT = 2 << 10; //From WinNT.h == 2048 == 0x800
        
        #endregion

        #region Public Methods

        /// <summary>
        /// Gets whether the specified object implements IDispatch.
        /// </summary>
        /// <param name="obj">An object to check.</param>
        /// <returns>True if the object implements IDispatch.  False otherwise.</returns>
        public static bool ImplementsIDispatch(object obj)
        {
            bool result = obj is IDispatchInfo;
            return result;
        }

        public static int GetTypeInfoCount(object obj)
        {
            var dispatch = (IDispatchInfo) obj;

            var hr = dispatch.GetTypeInfoCount(out var typeInfoCount);

            return hr == S_OK ? typeInfoCount : 0;
        }

        /// <summary>
        /// Gets a Type that can be used with reflection.
        /// </summary>
        /// <param name="obj">An object that implements IDispatch.</param>
        /// <returns>A .NET Type that can be used with reflection.</returns>
        /// <exception cref="InvalidCastException">If <paramref name="obj"/> doesn't implement IDispatch.</exception>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static ITypeInfo GetITypeInfo(object obj)
        {
#if NETSTANDARD
            throw new PlatformNotSupportedException();
#else

            RequireReference(obj, "obj");
            var dispatch = (IDispatchInfo) obj;
            dispatch.GetTypeInfo(0, LOCALE_SYSTEM_DEFAULT, out var typeInfo);
            return typeInfo;
#endif
        }

        /// <summary>
        /// Tries to get the DISPID for the requested member name.
        /// </summary>
        /// <param name="obj">An object that implements IDispatch.</param>
        /// <param name="name">The name of a member to lookup.</param>
        /// <param name="dispId">If the method returns true, this holds the DISPID on output.
        /// If the method returns false, this value should be ignored.</param>
        /// <returns>True if the member was found and resolved to a DISPID.  False otherwise.</returns>
        /// <exception cref="InvalidCastException">If <paramref name="obj"/> doesn't implement IDispatch.</exception>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static bool TryGetDispId(object obj, string name, out int dispId)
        {
            RequireReference(obj, "obj");
            bool result = TryGetDispId((IDispatchInfo)obj, name, out dispId);
            return result;
        }

        /// <summary>
        /// Invokes a member by DISPID.
        /// </summary>
        /// <param name="obj">An object that implements IDispatch.</param>
        /// <param name="dispId">The DISPID of a member.  This can be obtained using
        /// <see cref="TryGetDispId(object, string, out int)"/>.</param>
        /// <param name="args">The arguments to pass to the member.</param>
        /// <returns>The member's return value.</returns>
        /// <remarks>
        /// This can invoke a method or a property get accessor.
        /// </remarks>
        public static object Invoke(object obj, int dispId, object[] args)
        {
            string memberName = "[DispId=" + dispId + "]";
            object result = Invoke(obj, memberName, args);
            return result;
        }

        /// <summary>
        /// Invokes a member by name.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <param name="memberName">The name of the member to invoke.</param>
        /// <param name="args">The arguments to pass to the member.</param>
        /// <returns>The member's return value.</returns>
        /// <remarks>
        /// This can invoke a method or a property get accessor.
        /// </remarks>
        public static object Invoke(object obj, string memberName, object[] args)
        {
            RequireReference(obj, "obj");
            Type type = obj.GetType();
            object result = type.InvokeMember(memberName, BindingFlags.InvokeMethod | BindingFlags.GetProperty,
                null, obj, args, null);
            return result;
        }

        /// <summary>
        /// Invokes property setter by DispID
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <param name="dispId">The DISPID of a member.  This can be obtained using
        /// <see cref="TryGetDispId(object, string, out int)"/>.</param>
        /// <param name="propValue">new value of the property</param>
        public static void InvokeSetProperty(object obj, int dispId, object propValue)
        {
            string memberName = "[DispId=" + dispId + "]";
            InvokeSetProperty(obj, memberName, propValue);
        }

        /// <summary>
        /// Invokes property setter
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <param name="memberName">The name of the member to invoke.</param>
        /// <param name="propValue">new value of the property</param>
        public static void InvokeSetProperty(object obj, string memberName, object propValue)
        {
            RequireReference(obj, "obj");
            Type type = obj.GetType();
            object result = type.InvokeMember(memberName, BindingFlags.SetProperty,
                null, obj, new object[]{propValue}, null);
        }

#endregion

#region Private Methods

        /// <summary>
        /// Requires that the value is non-null.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="name">The name of the value.</param>
        private static void RequireReference<T>(T value, string name) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        
        /// <summary>
        /// Tries to get the DISPID for the requested member name.
        /// </summary>
        /// <param name="dispatch">An object that implements IDispatch.</param>
        /// <param name="name">The name of a member to lookup.</param>
        /// <param name="dispId">If the method returns true, this holds the DISPID on output.
        /// If the method returns false, this value should be ignored.</param>
        /// <returns>True if the member was found and resolved to a DISPID.  False otherwise.</returns>
        private static bool TryGetDispId(IDispatchInfo dispatch, string name, out int dispId)
        {
            RequireReference(dispatch, "dispatch");
            RequireReference(name, "name");

            bool result = false;

            // Members names aren't usually culture-aware for IDispatch, so we might as well
            // pass the default locale instead of looking up the current thread's LCID each time
            // (via CultureInfo.CurrentCulture.LCID).
            Guid iidNull = Guid.Empty;
            int hr = dispatch.GetDispId(ref iidNull, ref name, 1, LOCALE_SYSTEM_DEFAULT, out dispId);

            const int DISP_E_UNKNOWNNAME = unchecked((int)0x80020006); //From WinError.h
            const int DISPID_UNKNOWN = -1; //From OAIdl.idl
            if (hr == S_OK)
            {
                result = true;
            }
            else if (hr == DISP_E_UNKNOWNNAME && dispId == DISPID_UNKNOWN)
            {
                // This is the only supported "error" case because it means IDispatch
                // is saying it doesn't know the member we asked about.
                result = false;
            }
            else
            {
                // The other documented result codes are all errors.
                Marshal.ThrowExceptionForHR(hr);
            }

            return result;
        }

#endregion
    }
}