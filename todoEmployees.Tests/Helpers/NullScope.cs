using System;

namespace todoJose.Tests.Helpers
{
    internal class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new NullScope();

        public void Dispose() { }

        private NullScope() { }


    }
}
