namespace OneScript.DebugProtocol
{
    public static class DebuggerSettings
    {
        public const int MAX_BUFFER_SIZE = 5000000;
        public const int MAX_PRESENTATION_LENGTH = (int)(MAX_BUFFER_SIZE / 2.5);
    }
}