namespace OneScriptDocumenter
{
    class CommandLineArgs
    {
        readonly string[] _args;
        int _index = 0;

        public CommandLineArgs(string[] argsArray)
        {
            _args = argsArray;
        }

        public string Next()
        {
            if (_index >= _args.Length)
                return null;

            return _args[_index++];
        }
    }
}
