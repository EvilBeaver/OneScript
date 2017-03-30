namespace OneScript.DebugProtocol
{
    public interface IDebugController
    {
        void WaitForDebugEvent(DebugEventType theEvent);

        void NotifyProcessExit();
    }
}
