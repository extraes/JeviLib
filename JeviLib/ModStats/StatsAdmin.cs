using MelonLoader.TinyJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Jevil.Internal.Stats;

namespace Jevil.ModStats;

/// <summary>
/// C# bindings for the admin part of the Mod Stats API.
/// <para>The entire API should be on GitHub. <see href="https://github.com/extraes/ModStatsAPI"/></para>
/// </summary>
public static class StatsAdmin
{
    const string STATS_ADMIN_TEMPLATE = Const.MOD_STATS_DOMAIN + "ModStats/Admin/{0}/{1}?pass={2}"; // name, action, pass
    
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
    /// Sends a "Get" request to the remote ModStats API.
    /// <para>This uses code that interfaces with Unity Coroutines. Therefore it should not get synchronized (for example, do not use GetAwaiter().GetRestult()).</para>
    /// </summary>
    /// <param name="categoryName">The name of the ModStats Category to retrieve.</param>
    /// <param name="pass">The password/passphrase setup when creating the category.</param>
    /// <returns>The data recieved, or null if the request fails. Debug builds will give details when the request fails.</returns>
    public static async Task<Dictionary<string, long>?> GetCategoryAsync(string categoryName, string pass)
    {
        UnityWebRequest req;
        Utilities.AttachIl2CppToThread();
        string url = string.Format(STATS_ADMIN_TEMPLATE, categoryName, "Get", pass);
        req = UnityWebRequest.Get(url);
    
        await StatsCommon.SendAndWait(req);

#if DEBUG
        StatsCommon.WarnIfFailed(req, "get a stats category");
#endif
        if (req.result != UnityWebRequest.Result.Success)
            return null;

        // ive never had ML's TinyJSON work properly. i dont know where the fuck it came from, but if it fails here i blame ML.
        Dictionary<string, long>? category = System.Text.Json.JsonSerializer.Deserialize< Dictionary<string, long>>(req.downloadHandler.text);
        return category;
    }

    /// <summary>
    /// Sends a "Create" request to the remote ModStats API.
    /// <para>This uses code that interfaces with Unity Coroutines. Therefore it should not get synchronized (for example, do not use GetAwaiter().GetRestult()).</para>
    /// </summary>
    /// <param name="categoryName">The name of the ModStats Category to retrieve.</param>
    /// <param name="pass">The password/passphrase to set on the category.</param>
    /// <param name="authorization">A number given to you by the ModStats host that will authorize the creation of categories.</param>
    /// <returns>Whether the request succeeds. Debug builds will give details when the request fails.</returns>
    public static async Task<bool> CreateCategoryAsync(string categoryName, string pass, long authorization)
    {
        UnityWebRequest req;
        Utilities.AttachIl2CppToThread();
        string url = string.Format(STATS_ADMIN_TEMPLATE, categoryName, "Create", pass) + "&bless=" + authorization;
        req = UnityWebRequest.Post(url, "");
    
        await StatsCommon.SendAndWait(req);

#if DEBUG
        StatsCommon.WarnIfFailed(req, "create a stats category");
#endif

        return req.result == UnityWebRequest.Result.Success;
    }
}
