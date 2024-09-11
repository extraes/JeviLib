using Il2CppCysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Jevil.Internal.Stats;

internal static class StatsCommon
{
    internal static UniTask SendAndWait(UnityWebRequest request)
    {
        return AsyncUtilities.ToUniTask(request.SendWebRequest());
    }

#if DEBUG
    internal static void WarnIfFailed(UnityWebRequest request, string whatTryingDo)
    {
        if (request.result != UnityWebRequest.Result.Success)
        {
            string content;
            try
            {
                content = request.downloadHandler.text;
            }
            catch
            {
                content = "<Failed to read content as string>";
            }

            JeviLib.Warn($"Error when attempting to {whatTryingDo}. See below for details. If status code is in 500's, this represents an internal server error.");
            JeviLib.Warn($"Status code: {request.responseCode}; Result (according to Unity): {request.result}; Error (also according to Unity): '{request.error}'; Content: {content}");
        }
    }
#endif
}
