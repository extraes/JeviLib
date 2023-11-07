using Jevil.Internal.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Jevil.ModStats;

/// <summary>
/// C# bindings for the category admin (entry creation/deletion) part of the Mod Stats API.
/// <para>The entire API should be on GitHub. <see href="https://github.com/extraes/ModStatsAPI"/></para>
/// </summary>
public static class StatsCategoryAdmin
{
    const string CATEGORY_ADMIN_TEMPLATE = Const.MOD_STATS_DOMAIN + "ModStats/Categories/{0}/EntryModification/{1}?entryName={2}&pass={3}"; // category, action, entry, pass
    
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
    /// Sends a "Create" request to the remote ModStats API.
    /// <para>To maintain compatibility with Quest 2, this uses code that interfaces with Unity Coroutines. Therefore it should not get synchronized (for example, do not use GetAwaiter().GetRestult()).</para>
    /// </summary>
    /// <param name="categoryName">The name of the ModStats Category to modify.</param>
    /// <param name="entryName">The name of the ModStats entry to create.</param>
    /// <param name="pass">The password/passphrase setup when creating the category.</param>
    /// <returns>Whether the request was successful. Debug builds will give details when the request fails.</returns>
    public static async Task<bool> CreateEntryAsync(string categoryName, string entryName, string pass)
    {
        UnityWebRequest req;
        Utilities.AttachIl2CppToThread();
    string url = string.Format(CATEGORY_ADMIN_TEMPLATE, categoryName, "Create", entryName, pass);
        req = UnityWebRequest.Post(url, "");
        
        await StatsCommon.SendAndWait(req);

#if DEBUG
        StatsCommon.WarnIfFailed(req, "create a stats entry");
#endif

        return req.result == UnityWebRequest.Result.Success;
    }

    /// <summary>
    /// Sends a "Delete" request to the remote ModStats API.
    /// <para>To maintain compatibility with Quest 2, this uses code that interfaces with Unity Coroutines. Therefore it should not get synchronized (for example, do not use GetAwaiter().GetRestult()).</para>
    /// </summary>
    /// <param name="categoryName">The name of the ModStats Category to modify.</param>
    /// <param name="entryName">The name of the ModStats entry to create.</param>
    /// <param name="pass">The password/passphrase setup when creating the category.</param>
    /// <returns>Whether the request was successful. Debug builds will give details when the request fails.</returns>
    public static async Task<bool> DeleteEntryAsync(string categoryName, string entryName, string pass)
    {
        UnityWebRequest req;
        Utilities.AttachIl2CppToThread();
        string url = string.Format(CATEGORY_ADMIN_TEMPLATE, categoryName, "Delete", entryName, pass);
        req = UnityWebRequest.Delete(url);
        
        await StatsCommon.SendAndWait(req);

#if DEBUG
        StatsCommon.WarnIfFailed(req, "delete a stats entry");
#endif

        return req.result == UnityWebRequest.Result.Success;
    }
}
