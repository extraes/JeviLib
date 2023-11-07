using Jevil.Internal.Stats;
using MelonLoader.TinyJSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Jevil.ModStats;

/// <summary>
/// C# bindings for the consumable Mod Stats API.
/// <para>The entire API should be on GitHub. <see href="https://github.com/extraes/ModStatsAPI"/></para>
/// </summary>
public static class StatsEntry
{
    const string ENTRY_TEMPLATE = Const.MOD_STATS_DOMAIN + "ModStats/Categories/{0}/Entries/{1}/{2}";
    
    [ThreadStatic] static HttpClient _client;
    static HttpClient HttpClient
    {
        get
        {
            _client ??= new HttpClient();
            return _client;
        }
    }

    /// <summary>
    /// Sends an "Increment" command to the remote ModStats API.
    /// <para>To maintain compatibility with Quest 2, this uses code that interfaces with Unity Coroutines. Therefore it should not get synchronized (for example, do not use GetAwaiter().GetRestult()).</para>
    /// </summary>
    /// <param name="categoryName">The name of the ModStats Category to check in.</param>
    /// <param name="entryName">The name of the ModStats entry to increment.</param>
    /// <returns>Whether the request was successful. Debug builds will give details when the request fails.</returns>
    public static async Task<bool> IncrementValueAsync(string categoryName, string entryName)
    {
        UnityWebRequest req;

        Utilities.AttachIl2CppToThread();
        string url = string.Format(ENTRY_TEMPLATE, categoryName, entryName, "Increment");
        var handler = new DownloadHandlerBuffer();
        req = new(url, "POST", handler, null);
        
        await StatsCommon.SendAndWait(req);

#if DEBUG
        StatsCommon.WarnIfFailed(req, "increment a stats entry value");
#endif

        return req.result == UnityWebRequest.Result.Success;
    }

    /// <summary>
    /// Sends a "Set" command to the remote ModStats API.
    /// <para>To maintain compatibility with Quest 2, this uses code that interfaces with Unity Coroutines. Therefore it should not get synchronized (for example, do not use GetAwaiter().GetRestult()).</para>
    /// </summary>
    /// <param name="categoryName">The name of the ModStats Category to check in.</param>
    /// <param name="entryName">The name of the ModStats entry to set.</param>
    /// <param name="value">The value of the </param>
    /// <returns>Whether the request was successful. Debug builds will give details when the request fails.</returns>
    public static async Task<bool> SetValueAsync(string categoryName, string entryName, long value)
    {
        UnityWebRequest req;
        
        Utilities.AttachIl2CppToThread();
        string url = string.Format(ENTRY_TEMPLATE, categoryName, entryName, "Set") + "?value=" + value;
        var handler = new DownloadHandlerBuffer();
        req = new(url, "POST", handler, null);
        
        await StatsCommon.SendAndWait(req);

#if DEBUG
        StatsCommon.WarnIfFailed(req, "set a stats entry value");
#endif

        return req.result == UnityWebRequest.Result.Success;
    }

    /// <summary>
    /// Sends a "Get" request to the remote ModStats API.
    /// <para>To maintain compatibility with Quest 2, this uses code that interfaces with Unity Coroutines. Therefore it should not get synchronized (for example, do not use GetAwaiter().GetRestult()).</para>
    /// </summary>
    /// <param name="categoryName">The name of the ModStats Category to check in.</param>
    /// <param name="entryName">The name of the ModStats entry to get the value from.</param>
    /// <returns>The retrieved value if the request was successful, null if there was a failure. Debug builds will give details when the request fails.</returns>
    public static async Task<long?> GetValueAsync(string categoryName, string entryName)
    {
        UnityWebRequest req;
        
        Utilities.AttachIl2CppToThread();
        string url = string.Format(ENTRY_TEMPLATE, categoryName, entryName, "Get");
        req = UnityWebRequest.Get(url);
        
        await StatsCommon.SendAndWait(req);

#if DEBUG
        StatsCommon.WarnIfFailed(req, "get a stats entry value");
#endif

        if (req.result != UnityWebRequest.Result.Success || !long.TryParse(req.downloadHandler.text, out long value))
            return null;

        
        return value;
    }
}
