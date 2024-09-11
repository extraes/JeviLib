using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JeviLib.Research;

internal class UniTaskIL : INotifyCompletion
{

    public void AnotherMethod(Il2CppSystem.Action continuation)
    {

    }

    public void OnCompleted(Action continuation)
    {
        AnotherMethod(continuation);
    }
}
