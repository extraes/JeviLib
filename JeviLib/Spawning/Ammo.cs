using Jevil;
using System.Collections.Generic;
using UnityEngine;
using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Bonelab;
using Il2CppSLZ.Marrow.Pool;
using System.Threading.Tasks;

namespace Jevil.Spawning;

/// <summary>
/// Spawning ammo boxes from the pools
/// </summary>
public static class Ammo
{
    static readonly Dictionary<Weight, JevilBarcode> spawnableWeights = new(3);

    static Ammo()
    {
        spawnableWeights[Weight.LIGHT] = JevilBarcode.AMMO_BOX_LIGHT;
        spawnableWeights[Weight.MEDIUM] = JevilBarcode.AMMO_BOX_MEDIUM;
        spawnableWeights[Weight.HEAVY] = JevilBarcode.AMMO_BOX_HEAVY;
    }

    /// <summary>
    /// Redirect to <see cref="Spawn(Weight, int, Vector3, Quaternion)"/>, using <see cref="Quaternion.identity"/>.
    /// </summary>
    /// <param name="ammoWgt">The weight of ammo to use. <see cref="Weight.HEAVY"/> might not have its pool initialized.</param>
    /// <param name="ammoCount">The amount of ammo that the box holds.</param>
    /// <param name="pos">The position to spawn the ammo box at.</param>
    /// <returns>An inactive spawned ammo box.</returns>
    public static Task<GameObject> Spawn(Weight ammoWgt, int ammoCount, Vector3 pos) => Spawn(ammoWgt, ammoCount, pos, Quaternion.identity);

    /// <summary>
    /// Spawns an ammo box of weight <paramref name="ammoWgt"/> that gives the player <paramref name="ammoCount"/> ammo of that weight.
    /// </summary>
    /// <param name="ammoWgt">The weight of ammo to use. <see cref="Weight.HEAVY"/> might not have its pool initialized.</param>
    /// <param name="ammoCount">The amount of ammo that the box holds.</param>
    /// <param name="pos">The position to spawn the ammo box at.</param>
    /// <param name="rot"> The rotation to assign the ammo box.</param>
    /// <returns>An inactive spawned ammo box.</returns>
    public static async Task<GameObject> Spawn(Weight ammoWgt, int ammoCount, Vector3 pos, Quaternion rot)
    {
        Poolee spawnedAmmo = await Barcodes.SpawnAsync(spawnableWeights[ammoWgt], pos, rot);
        AmmoPickup pickup = spawnedAmmo.GetComponentInChildren<AmmoPickup>();
        pickup.ammoCount = ammoCount;
        return spawnedAmmo.gameObject;
    }
}
