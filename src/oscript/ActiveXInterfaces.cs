using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oscript.Interop
{
    #region Using directives.
    // ----------------------------------------------------------------------

    using System;
    using System.Runtime.InteropServices;
    using ComTypes = System.Runtime.InteropServices.ComTypes;

    // ----------------------------------------------------------------------
    #endregion

    /////////////////////////////////////////////////////////////////////////

    /* 
     * A good series of articles for working with Scripting Host under .NET
     * can be found at the DDJ:
     * http://google.com/search?q=site%3Addj.com+%22.NET+Scripting+Hosts%22
     * 
     * Also, the Weblog of Eric Lippert contains good references:
     * http://blogs.msdn.com/ericlippert
     * 
     * Last, the Google group microsoft.public.scripting.hosting contains 
     * good resources:
     * http://groups.google.com/group/microsoft.public.scripting.hosting
     * 
     * The definitions can also be found at:
     * http://msdn.microsoft.com/library/en-us/script56/html/f2afee5f-b930-4b32-b903-84ba41eb2d88.asp
     */

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Interface IActiveScript.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/d8acee11-7f0d-4999-b97a-66774af16f71.asp"/>
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid(@"BB1A2AE1-A4F9-11cf-8F20-00805F2CD064")]
    public interface IActiveScript
    {
        #region Interface members.
        // ------------------------------------------------------------------

        /// <summary>
        /// Sets the script site.
        /// </summary>
        /// <param name="site">The site.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/47d94c32-09f8-4539-ac56-0236026f627b.asp"/>
        void SetScriptSite(
            [In, MarshalAs(UnmanagedType.Interface)]
			IActiveScriptSite site);

        /// <summary>
        /// Gets the script site.
        /// </summary>
        /// <param name="riid">The riid.</param>
        /// <param name="ppvObject">The PPV object.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/83a2a89d-93d0-4cbd-9244-91a730cb406b.asp"/>
        void GetScriptSite(
            ref Guid riid,
            out IntPtr ppvObject);

        /// <summary>
        /// Sets the state of the script.
        /// </summary>
        /// <param name="ss">The ss.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/f2b2700c-0c8d-40db-ad84-dc751c5d9bc2.asp"/>
        void SetScriptState(
            SCRIPTSTATE ss);

        /// <summary>
        /// Gets the state of the script.
        /// </summary>
        /// <param name="ss">The ss.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/59837f7c-755d-45c4-8194-bd57638fe2e1.asp"/>
        void GetScriptState(
            out SCRIPTSTATE ss);

        /// <summary>
        /// Closes this instance.
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/cc7dd63b-1d7e-410a-857b-09ea3aade275.asp"/>
        void Close();

        /// <summary>
        /// Adds the named item.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="flags">The flags.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/a7c6317d-948f-4bb3-b169-1bbe5b7c7cc5.asp"/>
        void AddNamedItem(
            [In, MarshalAs(UnmanagedType.BStr)] 
			string name,
            [In, MarshalAs(UnmanagedType.U4)] 
			uint flags);

        /// <summary>
        /// Adds the type lib.
        /// </summary>
        /// <param name="rguidTypeLib">The rguid type lib.</param>
        /// <param name="major">The major.</param>
        /// <param name="minor">The minor.</param>
        /// <param name="flags">The flags.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/8e507ea8-c80a-471c-b482-ae753c6e8595.asp"/>
        void AddTypeLib(
            ref Guid rguidTypeLib,
            uint major,
            uint minor,
            uint flags);

        /// <summary>
        /// Gets the script dispatch.
        /// </summary>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="ppdisp">The ppdisp.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/2092ccd4-1f4c-493a-b5b7-077a70ce95ca.asp"/>
        void GetScriptDispatch(
            string itemName,
            out object ppdisp);

        /// <summary>
        /// Gets the current script threadi D.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/b09e8b48-4209-480e-8b71-e99ee9ae2e17.asp"/>
        void GetCurrentScriptThreadiD(
            out uint id);

        /// <summary>
        /// Gets the script thread ID.
        /// </summary>
        /// <param name="threadID">The thread ID.</param>
        /// <param name="id">The ID.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/2595d76e-30b5-429f-88b4-1d026645dd9b.asp"/>
        void GetScriptThreadID(
            uint threadID,
            out uint id);

        /// <summary>
        /// Gets the state of the script thread.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="state">The state.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/7cac94d0-436e-4c29-895b-0c4afa0b3ccc.asp"/>
        void GetScriptThreadState(
            uint id,
            out SCRIPTTHREADSTATE state);

        /// <summary>
        /// Interrupts the script thread.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="info">The info.</param>
        /// <param name="flags">The flags.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/2304d035-6d39-4811-acd3-8a9640fdbef6.asp"/>
        void InterruptScriptThread(
            uint id,
            ref ComTypes.EXCEPINFO info,
            uint flags);

        /// <summary>
        /// Clones the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/aa000b2a-7085-448d-a422-f7adac7851cb.asp"/>
        void Clone(
            out IActiveScript item);

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Interface IActiveScriptParse.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/8c967d70-f582-4f64-9e79-49f40c4dcb7c.asp"/>
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid(@"BB1A2AE2-A4F9-11cf-8F20-00805F2CD064")]
    public interface IActiveScriptParse
    {
        #region Interface members.
        // ------------------------------------------------------------------

        /// <summary>
        /// Inits the new.
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/3a582bd6-fc0d-43a5-812f-5ea55a8517a1.asp"/>
        void InitNew();

        /// <summary>
        /// Adds the scriptlet.
        /// </summary>
        /// <param name="defaultName">Name of the default.</param>
        /// <param name="code">The code.</param>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="subItemName">Name of the sub item.</param>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="delimiter">The delimiter.</param>
        /// <param name="sourceContextCookie">The source context cookie.</param>
        /// <param name="startingLineNumber">The starting line number.</param>
        /// <param name="flags">The flags.</param>
        /// <param name="name">The name.</param>
        /// <param name="info">The info.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/824929f4-4dd3-473a-92d9-0b96acea2f14.asp"/>
        void AddScriptlet(
            string defaultName,
            string code,
            string itemName,
            string subItemName,
            string eventName,
            string delimiter,
            uint sourceContextCookie,
            uint startingLineNumber,
            uint flags,
            out string name,
            out ComTypes.EXCEPINFO info);

        /// <summary>
        /// Parses the script text.
        /// </summary>
        /// <param name="code">LPCOLESTR pstrCode, address of scriptlet text.</param>
        /// <param name="itemName">LPCOLESTR pstrItemName, address of item name.</param>
        /// <param name="context">IUnknown *punkContext, address of debugging context.</param>
        /// <param name="delimiter">LPCOLESTR pstrDelimiter, address of end-of-scriptlet delimiter.</param>
        /// <param name="sourceContextCookie">DWORD_PTR dwSourceContextCookie, application-defined value for debugging.</param>
        /// <param name="startingLineNumber">ULONG ulStartingLineNumber, starting line number of the script.</param>
        /// <param name="flags">int dwFlags, scriptlet flags.</param>
        /// <param name="result">VARIANT *pvarResult, address of buffer for results.</param>
        /// <param name="info">EXCEPINFO *pexcepinfo, address of buffer for error data.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/2d237d6c-cc65-415b-8808-72791304a136.asp"/>
        void ParseScriptText(
            string code,
            string itemName,
            IntPtr context,
            string delimiter,
            uint sourceContextCookie,
            uint startingLineNumber,
            uint flags,
            IntPtr result,
            out ComTypes.EXCEPINFO info);

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Interface IActiveScriptSite.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/4d604a11-5365-46cf-ab71-39b3dbbe9f22.asp"/>
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid(@"DB01A1E3-A42B-11cf-8F20-00805F2CD064")]
    public interface IActiveScriptSite
    {
        #region Interface members.
        // ------------------------------------------------------------------

        /// <summary>
        /// Gets the LCID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/7b4a2dc1-bcf6-4bbf-884e-97b305a28eb7.asp"/>
        void GetLCID(
            out uint id);

        /// <summary>
        /// Gets the item info.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="returnMask">The return mask.</param>
        /// <param name="item">The item.</param>
        /// <param name="ppti">The ppti.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/f859ed3b-02c1-4924-99f8-5f5bf1bf8405.asp"/>
        void GetItemInfo(
            [In, MarshalAs(UnmanagedType.BStr)] string name,
            [In, MarshalAs(UnmanagedType.U4)] uint returnMask,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object item,
            IntPtr ppti);

        /// <summary>
        /// Gets the doc version string.
        /// </summary>
        /// <param name="v">The v.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/ab3f892d-06d3-4cb5-9ea5-20c4a1e518cd.asp"/>
        void GetDocVersionString(
            out string v);

        /// <summary>
        /// Called when [script terminate].
        /// </summary>
        /// <param name="result">The result.</param>
        /// <param name="info">The info.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/3301ddf4-5929-404c-81d3-1a720e589008.asp"/>
        void OnScriptTerminate(
            ref object result,
            ref ComTypes.EXCEPINFO info);

        /// <summary>
        /// Called when [state change].
        /// </summary>
        /// <param name="state">The state.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/3e9c5cbe-ca8e-426a-84fd-04e9c2daac7e.asp"/>
        void OnStateChange(
            SCRIPTSTATE state);

        /// <summary>
        /// Called when [script error].
        /// </summary>
        /// <param name="err">This should be casted to IActiveScriptError in the implementation
        /// of this interface. I had errors when changing the parameter here to the
        /// IActiveScriptError type.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/5c9c85cc-21ad-4232-be83-a24cc7570108.asp"/>
        void OnScriptError(
            [In, MarshalAs(UnmanagedType.IUnknown)] object err);

        /// <summary>
        /// Called when [enter script].
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/1ed9178c-fe80-41c4-b74d-23b85f9cddbf.asp"/>
        void OnEnterScript();

        /// <summary>
        /// Called when [leave script].
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/79af0e22-fbe3-4fae-8a5f-7af8b857678d.asp"/>
        void OnLeaveScript();

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Interface IActiveScriptError.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/4d604a11-5365-46cf-ab71-39b3dbbe9f22.asp"/>
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid(@"EAE1BA61-A4ED-11cf-8F20-00805F2CD064")]
    public interface IActiveScriptError
    {
        #region Interface members.
        // ------------------------------------------------------------------

        /// <summary>
        /// Gets the exception info.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/528416cc-8468-4ad7-a6c2-fa1daf6ecf33.asp"/>
        void GetExceptionInfo(
            [Out, MarshalAs(UnmanagedType.Struct)] out ComTypes.EXCEPINFO info);


        /// <summary>
        /// Gets the source position.
        /// </summary>
        /// <param name="sourceContext">The source context.</param>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="characterPosition">"-1" for unknown.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/64f7f37f-7288-4dbe-b626-a35d90897f36.asp"/>
        void GetSourcePosition(
            [Out, MarshalAs(UnmanagedType.U4)] out uint sourceContext,
            [Out, MarshalAs(UnmanagedType.U4)] out uint lineNumber,
            [Out, MarshalAs(UnmanagedType.U4)] out int characterPosition);

        /// <summary>
        /// Gets the source line text.
        /// </summary>
        /// <param name="sourceLine">The source line.</param>
        /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/ae9b26b1-82a7-4645-9686-3261d8248664.asp"/>
        /// <remarks>Throws E_FAIL if the line in the source file was not retrieved.</remarks>
        void GetSourceLineText(
            out string sourceLine);

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// The script state.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/5f5deb05-c4bb-4964-8077-e624c6b2a14e.asp"/>
    public enum SCRIPTSTATE : uint
    {
        #region Enum members.
        // ------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        SCRIPTSTATE_UNINITIALIZED = 0,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTSTATE_INITIALIZED = 5,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTSTATE_STARTED = 1,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTSTATE_CONNECTED = 2,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTSTATE_DISCONNECTED = 3,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTSTATE_CLOSED = 4,

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// The script thread state.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/975ec66b-c095-40ac-8ba9-631adb97b589.asp"/>
    public enum SCRIPTTHREADSTATE : uint
    {
        #region Enum members.
        // ------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        SCRIPTTHREADSTATE_NOTINSCRIPT = 0,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTTHREADSTATE_RUNNING = 1,

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// The script item flags when adding.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/a7c6317d-948f-4bb3-b169-1bbe5b7c7cc5.asp"/>
    [Flags]
    public enum SCRIPTITEMFLAGS : uint
    {
        #region Enum members.
        // ------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        SCRIPTITEM_ISVISIBLE = 0x00000002,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTITEM_ISSOURCE = 0x00000004,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTITEM_GLOBALMEMBERS = 0x00000008,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTITEM_ISPERSISTENT = 0x00000040,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTITEM_CODEONLY = 0x00000200,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTITEM_NOCODE = 0x00000400,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTITEM_ALL_FLAGS =
            SCRIPTITEM_ISSOURCE |
            SCRIPTITEM_ISVISIBLE |
            SCRIPTITEM_ISPERSISTENT |
            SCRIPTITEM_GLOBALMEMBERS |
            SCRIPTITEM_NOCODE |
            SCRIPTITEM_CODEONLY

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// The IActiveScriptSite.GetItemInfo() input flags.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/library/en-us/script56/html/f859ed3b-02c1-4924-99f8-5f5bf1bf8405.asp"/>
    [Flags]
    public enum SCRIPTINFOFLAGS : uint
    {
        #region Enum members.
        // ------------------------------------------------------------------

        /// <summary>
        /// 
        /// </summary>
        SCRIPTINFO_IUNKNOWN = 0x00000001,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTINFO_ITYPEINFO = 0x00000002,

        /// <summary>
        /// 
        /// </summary>
        SCRIPTINFO_ALL_FLAGS =
            SCRIPTINFO_IUNKNOWN |
            SCRIPTINFO_ITYPEINFO

        // ------------------------------------------------------------------
        #endregion
    }

    /////////////////////////////////////////////////////////////////////////
}
