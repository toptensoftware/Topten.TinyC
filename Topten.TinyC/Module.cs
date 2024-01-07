using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Topten.TinyC
{
    public class Module : IDisposable
    {
        internal Module(IntPtr code, IDictionary<string, IntPtr> symbols)
        {
            _code = code;
            Symbols = symbols;
        }

        IntPtr _code;

        public IDictionary<string, IntPtr> Symbols { get; }

        #region Dispose Pattern
        private bool isDisposed;
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                }

                Marshal.FreeHGlobal(_code);
                isDisposed = true;
            }
        }

        ~Module()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
