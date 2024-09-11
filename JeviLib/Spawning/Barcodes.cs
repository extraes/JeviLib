using Il2CppSLZ.Marrow.Data;
using Il2CppSLZ.Marrow.Pool;
using Il2CppSLZ.Marrow.Warehouse;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Il2CppCysharp.Threading.Tasks;

namespace Jevil.Spawning;

/// <summary>
/// Contains all barcodes for base-game spawnables.
/// </summary>
public static class Barcodes
{
    static Dictionary<JevilBarcode, string> enumToBarcode = new(Barcodes_Array.value.Length);
    static Dictionary<string, JevilBarcode> barcodeToEnum = new(Barcodes_Array.value.Length);

    static Dictionary<string, Pool> barcodeStrToPool = new();

    internal static void Init()
    {
#if DEBUG
        Stopwatch sw = Stopwatch.StartNew();
#endif
        JevilBarcode[] codes = (JevilBarcode[])Enum.GetValues(typeof(JevilBarcode));
        var codesExceptInvalid = codes.Take(codes.Length - 1);
        foreach ((JevilBarcode jcode, string code) in codesExceptInvalid.Zip(Barcodes_Array.value))
        {
            enumToBarcode[jcode] = code;
            barcodeToEnum[code] = jcode;
        }
#if DEBUG
        sw.Stop();
        JeviLib.Log("Populated Barcodes to JevilBarcodes in " + sw.ElapsedMilliseconds + "ms.");
#endif
    }

    /// <summary>
    /// Convert a <see cref="JevilBarcode"/> to the Barcode string that BONELAB recognizes.
    /// </summary>
    /// <param name="jBarcode">The JeviLib Barcode Enum.</param>
    /// <returns>The asset Barcode.</returns>
    public static string ToBarcodeString(JevilBarcode jBarcode) => enumToBarcode[jBarcode];

    /// <summary>
    /// Convert a <see cref="JevilBarcode"/> to a Barcode instance. I'm not sure if this even works, so if it doesn't you can use <see cref="ToBarcodeString(JevilBarcode)"/> as a fallback.
    /// </summary>
    /// <param name="jBarcode"></param>
    /// <returns>A barcode created using the <see cref="Barcode(string)"/> constructor.</returns>
    public static Barcode ToAssetBarcode(JevilBarcode jBarcode) => new(ToAssetBarcode(jBarcode));

    /// <summary>
    /// Converts an asset barcode string to a <see cref="JevilBarcode"/> enum.
    /// </summary>
    /// <param name="barcodeStr">A barcode string</param>
    /// <returns></returns>
    public static JevilBarcode ToJevilBarcode(string barcodeStr)
    {
        if (barcodeToEnum.TryGetValue(barcodeStr, out JevilBarcode jBarcode)) return jBarcode;
        return JevilBarcode.INVALID;
    }

    /// <summary>
    /// Converts a barcode for a spawnable into an actual <see cref="Spawnable"/> instance.
    /// <para>This code originates from ordo#2606. Thanks, ordo!</para>
    /// </summary>
    /// <param name="barcode">A spawnable crate's barcode.</param>
    /// <returns>An instance of a <see cref="Spawnable"/>, from a SpawnableCrateReference</returns>
    /// <remarks>Original code: https://discord.com/channels/563139253542846474/724595991675797554/1030596129424953384</remarks>
    public static Spawnable ToSpawnable(string barcode)
    {
        SpawnableCrateReference reference = new(barcode);
        Spawnable spawnable = new()
        {
            crateRef = reference
        };
        
        AssetSpawner.Register(spawnable);
        return spawnable;
    }

    /// <summary>
    /// Converts a <see cref="JevilBarcode"/> to a <see cref="Spawnable"/>.
    /// </summary>
    /// <param name="jevilBarcode">A <see cref="JevilBarcode"/>. It is assumed to be the <see cref="JevilBarcode"/> of a <see cref="Spawnable"/>.</param>
    /// <returns>A spawnable, as long as no exception was thrown.</returns>
    public static Spawnable ToSpawnable(JevilBarcode jevilBarcode)
        => ToSpawnable(ToBarcodeString(jevilBarcode));

    /// <summary>
    /// Spawns something using <see cref="ToSpawnable(JevilBarcode)"/>.
    /// </summary>
    /// <param name="barcodeToSpawn">A <see cref="JevilBarcode"/> corresponding to a spawnable.</param>
    /// <param name="pos">The worldspace position to spawn the object at.</param>
    /// <param name="rot">The worldspace rotation to spawn the object with.</param>
    public static UniTask<Poolee> SpawnAsync(JevilBarcode barcodeToSpawn, Vector3 pos, Quaternion rot)
    {
        Spawnable spawnable = ToSpawnable(barcodeToSpawn);
#if SELFCONTAINED
        return AssetSpawner.SpawnAsync(spawnable, pos, rot, Utilities.NulledNullable<Vector3>(), null, false, Utilities.NulledNullable<int>(), null, null);
#else
        return AssetSpawner.SpawnAsync(spawnable, pos, rot, Utilities.NulledNullable<Vector3>(), null, false, Utilities.NulledNullable<int>(), null, null);
#endif
    }

    private static void PopulateDictionary()
    {
        foreach (var kvp in AssetSpawner._instance._barcodeToPool)
        {
            barcodeStrToPool[kvp.key._id] = kvp.value;
        }
    }
}

// Behold, the shitfuck.
// Taken from Pallet_SLZ.BONELAB.Content.json

internal static class Barcodes_Array
{
    internal static readonly string[] value =
    {
        "c1534c5a-97a9-43f7-be30-6095416d6d6f",
        "c1534c5a-683b-4c01-b378-6795416d6d6f",
        "c1534c5a-57d4-4468-b5f0-c795416d6d6f",
        "SLZ.BONELAB.Content.Spawnable.DestAmmoBoxHeavyVariant",
        "SLZ.BONELAB.Content.Spawnable.DestAmmoBoxLightVariant",
        "SLZ.BONELAB.Content.Spawnable.DestAmmoBoxMediumVariant",
        "c1534c5a-adaf-4ae7-bd46-f19541766174",
        "fa534c5a83ee4ec6bd641fec424c4142.Spawnable.BatteryA",
        "c1534c5a-e777-4d15-b0c1-3195426f6172",
        "c1534c5a-3813-49d6-a98c-f595436f6e73",
        "c1534c5a-c6a8-45d0-aaa2-2c954465764d",
        "c1534c5a-87ce-436d-b00c-ef9547726176",
        "c1534c5a-a1c3-437b-85ac-e09547726176",
        "c1534c5a-6b38-438a-a324-d7e147616467",
        "fa534c5a868247138f50c62e424c4144.Spawnable.OmniWay",
        "c1534c5a-52b6-490b-8c20-1cfe50616c6c",
        "c1534c5a-cebf-42cc-be3a-4595506f7765",
        "c1534c5a-5747-42a2-bd08-ab3b47616467",
        "c1534c5a-e963-4a7c-8c7e-1195546f7942",
        "c1534c5a-68d7-4708-b490-5c955a697053",
        "c1534c5a-fcfc-4f43-8fb0-d29531393131",
        "c1534c5a-7f05-402f-9320-609647756e35",
        "c1534c5a-a6b5-4177-beb8-04d947756e41",
        "SLZ.BONELAB.Content.Spawnable.HandgunEder22training",
        "c1534c5a-2a4f-481f-8542-cc9545646572",
        "c1534c5a-2774-48db-84fd-778447756e46",
        "c1534c5a-9f55-4c56-ae23-d33b47727562",
        "c1534c5a-ea97-495d-b0bf-ac955269666c",
        "c1534c5a-cc53-4aac-b842-46955269666c",
        "c1534c5a-9112-49e5-b022-9c955269666c",
        "c1534c5a-4e5b-4fb7-be33-08955269666c",
        "c1534c5a-e0b5-4d4b-9df3-567147756e4d",
        "c1534c5a-aade-4fa1-8f4b-d4c547756e4d",
        "c1534c5a-c061-4c5c-a5e2-3d955269666c",
        "c1534c5a-f3b6-4161-a525-a8955269666c",
        "c1534c5a-ec8e-418a-a545-cf955269666c",
        "c1534c5a-4b3e-4288-849c-ce955269666c",
        "c1534c5a-5c2b-4cb4-ae31-e7955269666c",
        "c1534c5a-d00c-4aa8-adfd-3495534d474d",
        "c1534c5a-3e35-4aeb-b1ec-4a95534d474d",
        "c1534c5a-9f54-4f32-b8b9-f295534d474d",
        "c1534c5a-ccfa-4d99-af97-5e95534d474d",
        "fa534c5a83ee4ec6bd641fec424c4142.Spawnable.MP5KRedDotSight",
        "c1534c5a-6670-4ac2-a82a-a595534d474d",
        "c1534c5a-bcb7-4f02-a4f5-da9550333530",
        "c1534c5a-04d7-41a0-b7b8-5a95534d4750",
        "c1534c5a-50cf-4500-83d5-c0b447756e50",
        "c1534c5a-571f-43dc-8bc6-8e9553686f74",
        "fa534c5a868247138f50c62e424c4144.Spawnable.Stapler",
        "c1534c5a-40e5-40e0-8139-194347756e55",
        "c1534c5a-8d03-42de-93c7-f595534d4755",
        "c1534c5a-4c47-428d-b5a5-b05747756e56",
        "c1534c5a-d7ea-4c98-a79d-244e4d616761",
        "c1534c5a-53ea-4354-950c-166c4d616761",
        "c1534c5a-de30-4591-8dd2-53954d616761",
        "c1534c5a-6125-45f0-ac59-a6954d616761",
        "c1534c5a-53c6-4aa3-8c88-93504d616761",
        "c1534c5a-e45e-4f53-a9ae-3c954d616761",
        "c1534c5a-55c5-4e30-8ad4-a7074d61675f",
        "c1534c5a-8bb2-47cc-977a-46954d616761",
        "c1534c5a-eae9-4837-9bf2-2fd94d616761",
        "c1534c5a-233c-413a-b218-56954d616761",
        "c1534c5a-dfeb-4562-9b6e-76d04d61675f",
        "c1534c5a-ce15-4235-b0d6-7efd4d616761",
        "c1534c5a-9828-4ba4-8292-25734d616761",
        "fa534c5a868247138f50c62e424c4144.Spawnable.MagazineStaplegun",
        "c1534c5a-18ce-44aa-a416-63174d616761",
        "c1534c5a-6e5b-4980-a3f2-95954d616761",
        "c1534c5a-3030-4338-bf76-b67c4d616761",
        "c1534c5a-6d6b-4414-a9f2-af034d656c65",
        "c1534c5a-4774-460f-a814-149541786546",
        "c1534c5a-0ba6-4876-be9c-216741786548",
        "c1534c5a-e962-46dd-b1ef-f39542617262",
        "c1534c5a-6441-40aa-a070-909542617365",
        "c1534c5a-d086-4e27-918d-ee9542617374",
        "fa534c5a868247138f50c62e424c4144.Spawnable.Baton",
        "c1534c5a-8036-440a-8830-b99543686566",
        "c1534c5a-3481-4025-9d28-2e95436c6561",
        "c1534c5a-1fb8-477c-afbe-2a95436f6d62",
        "c1534c5a-0c8a-4b82-9f8b-7a9543726f77",
        "c1534c5a-f0d1-40b6-9f9b-c19544616767",
        "c1534c5a-d3fc-4987-a93d-d79544616767",
        "SLZ.BONELAB.Content.Spawnable.ElectricGuitar",
        "c1534c5a-d0e9-4d53-9218-e76446727969",
        "c1534c5a-8597-4ffe-892e-b995476f6c66",
        "c1534c5a-53ae-487e-956f-707148616c66",
        "c1534c5a-11d0-4632-b36e-fa9548616d6d",
        "c1534c5a-dfa6-466d-9ab7-bf9548616e64",
        "c1534c5a-d605-4f85-870d-f68848617463",
        "SLZ.BONELAB.Content.Spawnable.MeleeIceAxe",
        "c1534c5a-282b-4430-b009-58954b617461",
        "c1534c5a-e606-4a82-878c-652f4b617461",
        "c1534c5a-f6f9-4c96-b88e-91d74c656164",
        "c1534c5a-a767-4a58-b3ef-26064d616368",
        "c1534c5a-3d5c-4f9f-92fa-c24c4d656c65",
        "c1534c5a-e75f-4ded-aa5a-a27b4178655f",
        "c1534c5a-f943-42a8-a994-6e955069636b",
        "c1534c5a-5d31-488d-b5b3-aa1c53686f76",
        "c1534c5a-1f5a-4993-bbc1-03be4d656c65",
        "c1534c5a-a97f-4bff-b512-e44d53706561",
        "c1534c5a-f5a3-4204-a199-a1e14d656c65",
        "c1534c5a-b59c-4790-9b09-499553776f72",
        "c1534c5a-d30c-4c18-9f5f-7cfe54726173",
        "c1534c5a-6d15-47c7-9ad4-b04156696b69",
        "c1534c5a-f6f3-46e2-aa51-67214d656c65",
        "c1534c5a-02e7-43cf-bc8d-26955772656e",
        "c1534c5a-af28-46cb-84c1-012343726162",
        "c1534c5a-4583-48b5-ac3f-eb9543726162",
        "SLZ.BONELAB.Content.Spawnable.NPCCultist",
        "c1534c5a-2ab7-46fe-b0d6-7495466f7264",
        "c1534c5a-481a-45d8-8bc1-d810466f7264",
        "c1534c5a-3fd8-4d50-9eaf-0695466f7264",
        "c1534c5a-2775-4009-9447-22d94e756c6c",
        "c1534c5a-0e54-4d5b-bdb8-31754e756c6c",
        "c1534c5a-d82d-4f65-89fd-a4954e756c6c",
        "c1534c5a-ef15-44c0-88ae-aebc4e756c6c",
        "c1534c5a-9c8a-47b8-8ceb-70b3456e656d",
        "c1534c5a-7c6d-4f53-b61c-e4024f6d6e69",
        "c1534c5a-0df5-495d-8421-75834f6d6e69",
        "SLZ.BONELAB.Content.Spawnable.NPCPeasantFemL",
        "SLZ.BONELAB.Content.Spawnable.NPCPeasantFemM",
        "SLZ.BONELAB.Content.Spawnable.NPCPeasantFemS",
        "SLZ.BONELAB.Content.Spawnable.NPCPeasantMaleL",
        "SLZ.BONELAB.Content.Spawnable.NPCPeasantMaleM",
        "SLZ.BONELAB.Content.Spawnable.NPCPeasantMaleS",
        "SLZ.BONELAB.Content.Spawnable.NPCPeasantNull",
        "SLZ.BONELAB.Content.Spawnable.NPCSecurityGuard",
        "c1534c5a-a750-44ca-9730-b487536b656c",
        "c1534c5a-bd53-469d-97f1-165e4e504353",
        "c1534c5a-de57-4aa0-9021-5832536b656c",
        "c1534c5a-290e-4d56-9b8e-ad95566f6964",
        "c1534c5a-dcdc-4614-b067-dd95566f6964",
        "SLZ.BONELAB.Content.Spawnable.ProjectileGachaSpawn",
        "SLZ.BONELAB.Content.Spawnable.ProjectileVoidEnergy",
        "SLZ.BONELAB.Content.Spawnable.ProjectileVoidBall",
        "SLZ.BONELAB.Content.Spawnable.AluminumTableB",
        "SLZ.BONELAB.Content.Spawnable.AluminumTable",
        "SLZ.BONELAB.Content.Spawnable.AngryVoidRadiation",
        "c1534c5a-f938-40cb-8be5-23db41706f6c",
        "SLZ.BONELAB.Content.Spawnable.Barrel2",
        "c1534c5a-837c-43ca-b4b5-33d842617365",
        "c1534c5a-0c71-4ce9-a9bb-af8a50726f70",
        "fa534c5a83ee4ec6bd641fec424c4142.Spawnable.PropBowlingBallBig",
        "c1534c5a-6f93-4d58-b9a9-ca1c50726f70",
        "SLZ.BONELAB.Content.Spawnable.Bucket",
        "SLZ.BONELAB.Content.Spawnable.CafeTray",
        "c1534c5a-f974-4581-b812-9c9543617264",
        "fa534c5a83ee4ec6bd641fec424c4142.Spawnable.DestcardboardBox",
        "SLZ.BONELAB.Content.Spawnable.CaveSmallRock",
        "SLZ.BONELAB.Content.Spawnable.CinderBlock",
        "SLZ.BONELAB.Content.Spawnable.ClipboardLore",
        "c1534c5a-9629-4660-8439-186b50726f70",
        "c1534c5a-c29c-4343-8809-5f07436f6e63",
        "c1534c5a-7a0d-4559-bbf2-f68d436f7265",
        "SLZ.BONELAB.Content.Spawnable.Couch2Seat",
        "SLZ.BONELAB.Content.Spawnable.Couch3Seat",
        "c1534c5a-935a-44e8-8036-d86043726174",
        "c1534c5a-450f-4fcd-95cf-887043726174",
        "c1534c5a-5be2-49d6-884e-d35c576f6f64",
        "c1534c5a-c4c8-41de-8644-0a9543726f77",
        "c1534c5a-7b2a-41d7-bf2e-af9544657374",
        "SLZ.BONELAB.Content.Spawnable.DestructibleBarrel",
        "SLZ.BONELAB.Content.Spawnable.Died20",
        "SLZ.BONELAB.Content.Spawnable.DungeonLargeBrick",
        "SLZ.BONELAB.Content.Spawnable.DungeonSmallBrick",
        "c1534c5a-38df-474e-abb3-7e81466c6173",
        "c1534c5a-48ab-4117-94d0-20e0476c6f77",
        "c1534c5a-df6a-4c22-8317-44fd47796d41",
        "c1534c5a-1da1-47fa-accc-95ed47796d42",
        "c1534c5a-d80e-4a91-8081-ba1950726f70",
        "c1534c5a-a419-426e-96d9-e12e50726f70",
        "c1534c5a-2995-44a8-8e86-155150726f70",
        "c1534c5a-a460-47db-b01e-505c50726f70",
        "c1534c5a-ef31-491b-bb58-4d6950726f70",
        "c1534c5a-067e-4466-9122-19c247796d43",
        "c1534c5a-e510-4c7d-92fb-73d647796d43",
        "c1534c5a-5c6f-473b-b20e-c8f447796d43",
        "c1534c5a-5b13-4a6c-8d86-cf0547796d43",
        "c1534c5a-bf39-4abf-832a-461347796d43",
        "c1534c5a-86b1-4192-9785-2e0750726f70",
        "c1534c5a-a7be-49a9-b6f2-bcbf47796d43",
        "c1534c5a-fd7b-48c5-8fd7-e1d347796d43",
        "c1534c5a-ac3b-42ea-a60c-5d5a47796d43",
        "c1534c5a-4b47-4f36-9cdb-6ea147796d43",
        "c1534c5a-375f-4410-8bc6-1a2750726f70",
        "c1534c5a-ea32-4493-898e-1aab50726f70",
        "c1534c5a-e1e6-488b-a49e-e0b850726f70",
        "c1534c5a-3199-4102-91e2-8ac650726f70",
        "c1534c5a-2a2d-488a-824b-df5b50726f70",
        "c1534c5a-33a1-4b6d-9312-bd8650726f70",
        "c1534c5a-33a6-4807-b4ee-ff9350726f70",
        "c1534c5a-783c-45a9-97ae-87cf47796d44",
        "c1534c5a-4295-4778-8761-89dd47796d44",
        "c1534c5a-5d9f-4744-bc75-70f247796d4d",
        "c1534c5a-1b8b-4e7c-a476-074547796d4f",
        "c1534c5a-1092-4da0-8736-3e2f47796d50",
        "c1534c5a-2b71-4b6f-8089-a40247796d53",
        "c1534c5a-33f4-4b5b-bb3b-d91547796d53",
        "c1534c5a-25db-4ece-a5d5-c81747796d54",
        "c1534c5a-15dd-4084-bf64-9ab747796d54",
        "c1534c5a-bc25-4967-9101-1b4b50726f70",
        "c1534c5a-c72d-4d56-95b7-a26250726f70",
        "c1534c5a-dea1-42b4-b95f-6b6f50726f70",
        "c1534c5a-66b3-4a1c-ad8d-9f7a50726f70",
        "c1534c5a-060f-44b5-ac72-d88450726f70",
        "c1534c5a-3ac8-4f19-913b-6bd348657861",
        "SLZ.BONELAB.Content.Spawnable.PropKeyBallAmber",
        "c1534c5a-b8ba-4b3a-914c-aad04b657942",
        "SLZ.BONELAB.Content.Spawnable.PropKeyBallGreen",
        "fa534c5a868247138f50c62e424c4144.Spawnable.PropKeyBallPurple",
        "SLZ.BONELAB.Content.Spawnable.RollBallwhite",
        "c1534c5a-ea4d-4e41-ae9c-a66f4b657942",
        "SLZ.BONELAB.Content.Spawnable.Keyboard",
        "SLZ.BONELAB.Content.Spawnable.MeetingRoomTable",
        "c1534c5a-8fc2-4596-b868-a7644d697272",
        "c1534c5a-4ca8-448c-80e1-882e4d6f6e69",
        "c1534c5a-202f-43f8-9a6c-1e9450726f70",
        "SLZ.BONELAB.Content.Spawnable.Mouse",
        "SLZ.BONELAB.Content.Spawnable.SMPaperFolder",
        "SLZ.BONELAB.Content.Spawnable.PC",
        "SLZ.BONELAB.Content.Spawnable.Pizzabox",
        "c1534c5a-aeee-4983-8239-fe6144657374",
        "fa534c5a83ee4ec6bd641fec424c4142.Spawnable.Plunger",
        "SLZ.BONELAB.Content.Spawnable.Printer",
        "SLZ.BONELAB.Content.Spawnable.Radio",
        "fa534c5a83ee4ec6bd641fec424c4142.Spawnable.PropRollBall100kg",
        "SLZ.BONELAB.Content.Spawnable.PropScaffolding",
        "c1534c5a-6a57-477d-8bcd-e2dc53686f70",
        "SLZ.BONELAB.Content.Spawnable.SmallChair2",
        "c1534c5a-9346-4ab8-a37d-698b536f7570",
        "SLZ.BONELAB.Content.Spawnable.PropStationaryTurret",
        "c1534c5a-a1c4-4c90-ad5d-ea1a53776f72",
        "SLZ.BONELAB.Content.Spawnable.Table01",
        "SLZ.BONELAB.Content.Spawnable.Trashbag01",
        "SLZ.BONELAB.Content.Spawnable.Trashbag02",
        "SLZ.BONELAB.Content.Spawnable.Trashcan01",
        "SLZ.BONELAB.Content.Spawnable.TrashcanA",
        "SLZ.BONELAB.Content.Spawnable.TrashcanMetal",
        "SLZ.BONELAB.Content.Spawnable.Trashcan",
        "SLZ.BONELAB.Content.Spawnable.VendingMachineUM",
        "c1534c5a-1e43-4d94-a504-31d457617465",
        "fa534c5a83ee4ec6bd641fec424c4142.Spawnable.VehicleGokart"
    };
}