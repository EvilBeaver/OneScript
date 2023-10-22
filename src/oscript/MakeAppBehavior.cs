using System;

namespace oscript
{
    internal class MakeAppBehavior : AppBehavior
    {
        public override int Execute()
        {
            throw new System.NotImplementedException();
        }

        public static AppBehavior Create(CmdLineHelper arg)
        {
            var fileName = arg.Next();
            var outputName = arg.Next();
            if (fileName == default || outputName == default)
            {
                return null;
            }

            return new MakeAppBehavior();
        }
    }
}