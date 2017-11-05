using System;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace Wexflow.Tasks.ProcessLauncher
{
    public class ProcessWaitHandle : WaitHandle
    {
        public ProcessWaitHandle(IntPtr processHandle)
        {
            SafeWaitHandle = new SafeWaitHandle(processHandle, false);
        }
    }
}