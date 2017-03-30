namespace OneScript.DebugProtocol
{
    public interface IDebugController
    {
        void WaitForDebugEvent(DebugEventType theEvent);

        void NotifyProcessExit();

        int SetBreakpoint(string sourceLocation, int line);
    }
}
