using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JeviLib.Research;

internal class TaskType
{
    internal class CompletionNotifier : INotifyCompletion
    {
        public void OnCompletedB(Il2CppSystem.Action continuation)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted(Action continuation)
        {
            OnCompletedB(continuation);
        }
    }
}

internal class TaskType<T>
{
    internal class CompletionNotifier : INotifyCompletion
    {
        public void OnCompletedB(Il2CppSystem.Action continuation)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted(Action continuation)
        {
            OnCompletedB(continuation);
        }
    }
}