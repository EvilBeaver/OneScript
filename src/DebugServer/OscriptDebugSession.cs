using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VSCodeDebug;

namespace DebugServer
{
    class OscriptDebugSession : DebugSession
    {
        public OscriptDebugSession() : base(true, false)
        {
        }

        public override void Initialize(Response response, dynamic args)
        {
            throw new NotImplementedException();
        }

        public override void Launch(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Attach(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Disconnect(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void SetBreakpoints(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Continue(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Next(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void StepIn(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void StepOut(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Pause(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void StackTrace(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Scopes(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Variables(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Threads(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }

        public override void Evaluate(Response response, dynamic arguments)
        {
            throw new NotImplementedException();
        }
    }
}
