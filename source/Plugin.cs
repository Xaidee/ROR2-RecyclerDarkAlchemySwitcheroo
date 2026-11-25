using RoR2;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using System.IO;
using System.Linq;
using EntityStates.CursedBook.Spells;
using RoR2.Items;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using Path = System.IO.Path;

namespace RDAS;

[BepInDependency(ItemQualities.ItemQualitiesPlugin.PluginGUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin("Xaidee.RecyclerDarkAlchemySwitcheroo", "RecyclerDarkAlchemySwitcheroo", "1.0.0")]
public class ModPlugin : BaseUnityPlugin
{
    private static ConfigEntry<bool> _enableRecyclingHexes;

    private static readonly GameObject RecyclerBadIndicatorPrefab = Addressables
        .LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Recycle.RecyclerBadIndicator_prefab)
        .WaitForCompletion();
    
    private static readonly GameObject RecyclerIndicatorPrefab = Addressables
        .LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Recycle.RecyclerIndicator_prefab)
        .WaitForCompletion();

    private static readonly GameObject OmniRecycleEffectPrefab = Addressables
        .LoadAssetAsync<GameObject>(RoR2BepInExPack.GameAssetPathsBetter.RoR2_Base_Recycle.OmniRecycleEffect_prefab)
        .WaitForCompletion();

    private static bool isQualityLoaded;
        
    public void Awake()
    {
        Log.Init(Logger);

        _enableRecyclingHexes = Config.Bind("Recycler Dark Alchemy Switcheroo", "EnableRecyclingHexes", false,
            "Allows Recyclers to recycle Hexes into white items like base game's Dark Alchemy does.");

        var rootPath = Path.GetDirectoryName(Info.Location)!;
        
        if (Directory.Exists(Path.Combine(rootPath, "languages")))
            Language.collectLanguageRootFolders += list => list.Add(Path.Combine(rootPath, "languages"));

        isQualityLoaded =
            BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ItemQualities.ItemQualitiesPlugin.PluginGUID);
        
        ItemCatalog.availability.CallWhenAvailable(ChangeDefs);

        if (isQualityLoaded) QualityCompat.Init();           
        
        On.RoR2.EquipmentSlot.UpdateTargets += EquipmentSlotUpdateTargets;
        On.RoR2.Items.CursedBookBodyBehavior.UpdateTargets += CursedBookBodyBehaviorUpdateTargets;

        On.RoR2.EquipmentSlot.FireRecycle += FireRecycle;
        On.EntityStates.CursedBook.Spells.UpgradeItem.OnEnter += UpgradeItemOnEnter;
    }

    private void Start()
    {
        // Call in start to ensure the hooks are registered in the first place so we can actually remove them.
        if (isQualityLoaded) QualityCompat.RemoveHooks();
    }

    private static void ChangeDefs()
    {
        var recycler = RoR2Content.Equipment.Recycle;
        var darkAlchemy = DLC4Content.Items.PageUpgradeItem;

        recycler.pickupToken = "EQUIPMENT_RDAS_RECYCLER_PICKUP";
        recycler.descriptionToken = "EQUIPMENT_RDAS_RECYCLER_DESC";

        darkAlchemy.pickupToken = "CURSEDBOOK_SPELL_RDAS_DARKALCHEMY_PICKUP";
        darkAlchemy.descriptionToken = isQualityLoaded ? "CURSEDBOOK_SPELL_RDAS_DARKALCHEMY_DESC_QUALITY" : "CURSEDBOOK_SPELL_RDAS_DARKALCHEMY_DESC";
    }

    /// <summary>
    /// Replace with Dark Alchemy functionality
    /// <br><see cref="RoR2.Items.CursedBookBodyBehavior.UpdateTargets"/></br>
    /// </summary>
    /// <param name="orig"><see cref="RoR2.EquipmentSlot.UpdateTargets"/></param>
    /// <param name="self"><see cref="RoR2.EquipmentSlot"/></param>
    /// <param name ="targetingEquipmentIndex"></param>
    /// <param name="userShouldAnticipateTarget"></param>
    /// <returns></returns>
    private static void EquipmentSlotUpdateTargets(On.RoR2.EquipmentSlot.orig_UpdateTargets orig, EquipmentSlot self, EquipmentIndex targetingEquipmentIndex, bool userShouldAnticipateTarget)
    {
        if (self.remoteClientTargetOverride.HasValue ||
            targetingEquipmentIndex != RoR2Content.Equipment.Recycle.equipmentIndex)
        {
            orig(self, targetingEquipmentIndex, userShouldAnticipateTarget);
            return;
        }
        self.currentTarget = new EquipmentSlot.UserTargetInfo(self.FindPickupController(self.GetAimRay(), 10F, 30F, true, false));
        
        var transformToIndicateAt = self.currentTarget.transformToIndicateAt;
        var pickupController = self.currentTarget.pickupController;
        if (transformToIndicateAt)
            self.targetIndicator.visualizerPrefab = !CanRecyclePickup(pickupController)
                ? RecyclerBadIndicatorPrefab
                : self.stock <= 0
                    ? CharacterBody.CommonAssets.recyclerCooldownIndicatorPrefab
                    : RecyclerIndicatorPrefab;
        self.targetIndicator.active = transformToIndicateAt;
        self.targetIndicator.targetTransform = transformToIndicateAt ? self.currentTarget.transformToIndicateAt : null;
    }

    /// <summary>
    /// Replace with Dark Alchemy functionality
    /// <br><see cref="EntityStates.CursedBook.Spells.UpgradeItem.OnEnter"/></br>
    /// </summary>
    /// <param name="orig"><see cref="RoR2.EquipmentSlot.FireRecycle"/></param>
    /// <param name="self"><see cref="RoR2.EquipmentSlot"/></param>
    private static bool FireRecycle(On.RoR2.EquipmentSlot.orig_FireRecycle orig, EquipmentSlot self)
    {
        _ = orig;
        self.UpdateTargets(RoR2Content.Equipment.Recycle.equipmentIndex, false);
        var pickupController = self.currentTarget.pickupController;
        if (!pickupController || !CanRecyclePickup(pickupController)) return false;
        UniquePickup result;
        var recycleSuccessful= isQualityLoaded 
            ? QualityCompat.AttemptRecycleQuality(self,  pickupController.pickup, out result) 
            : AttemptRecycle( pickupController.pickup, out result);
        if (!recycleSuccessful)
        {
            Log.Debug($"Failed to recycle {ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(pickupController.pickup.pickupIndex).itemIndex)}");
            return false;
        }
        self.subcooldownTimer = 0.2F;
        pickupController.pickup = result;
        EffectManager.SimpleEffect(OmniRecycleEffectPrefab, pickupController.pickupDisplay.transform.position, Quaternion.identity, true);
        pickupController.NetworkRecycled = true;
        pickupController.ForcePickupDisplayUpdate();
        var createPickupInfo = new GenericPickupController.CreatePickupInfo
            { pickup = result, position = pickupController.transform.position, recycled = true };
        GenericPickupController.CreatePickup(in createPickupInfo);
        self.InvalidateCurrentTarget();
        return true;
    }

    /// <summary>
    /// Replace with Recycler functionality
    /// <br><see cref="RoR2.EquipmentSlot.UpdateTargets"/></br>
    /// </summary>
    /// <param name="orig"><see cref="RoR2.Items.CursedBookBodyBehavior.UpdateTargets"/></param>
    /// <param name="self"><see cref="RoR2.Items.CursedBookBodyBehavior"/></param>
    private static void CursedBookBodyBehaviorUpdateTargets(On.RoR2.Items.CursedBookBodyBehavior.orig_UpdateTargets orig, CursedBookBodyBehavior self)
    {
        if (!self.IsCurrentSpell(DLC4Content.CursedBookSpells.UpgradeItem.spellIndex))
        {
            orig(self);
            return;
        }
        
        self.currentTarget = new CursedBookBodyBehavior.PickupTargetInfo(self.FindPickupController(self.body.inputBank.GetAimRay(), 10F, 30F, true, true));

        self.indicator.visualizerPrefab = CanTransmutePickup(self.currentTarget.pickupController) ? CursedBookBodyBehavior.upgradeItemIndicatorPrefab : CursedBookBodyBehavior.upgradeItemCannotUpgradeIndicatorPrefab;
        
        self.indicator.active = true;
        self.indicator.targetTransform = self.currentTarget.transformToIndicateAt;
    }

    /// <summary>
    /// Replace with Recycler functionality
    /// <br><see cref="RoR2.EquipmentSlot.FireRecycle"/></br>
    /// </summary>
    /// <param name="orig"><see cref="EntityStates.CursedBook.Spells.UpgradeItem.OnEnter"/></param>
    /// <param name="self"><see cref="EntityStates.CursedBook.Spells.UpgradeItem"/></param>
    private static void UpgradeItemOnEnter(On.EntityStates.CursedBook.Spells.UpgradeItem.orig_OnEnter orig, UpgradeItem self)
    {
        _ = orig;
        // we gotta do some silly things here cause we can't do base.OnEnter():
        self.bodyAttachment = self.GetComponent<NetworkedBodyAttachment>();
        self.ownerBody = self.bodyAttachment.attachedBody;
        self.bodyBehavior = self.ownerBody.GetComponent<CursedBookBodyBehavior>();
        
        self.overrideConsumeOnExit = self.consumeCursedBookOnExit;
        //
        if (self.isAuthority) self.targetObject = self.bodyBehavior.currentTarget.rootObject;
        var overrideConsumeOnExit = false;
        if (self.targetObject && self.targetObject.TryGetComponent(out GenericPickupController pickupController) &&
            CanTransmutePickup(pickupController) && NetworkServer.active)
        {
            var initialPickupState = pickupController.pickup;
            var array =
                (PickupTransmutationManager
                    .GetAvailableGroupFromPickupIndex(pickupController.pickup.pickupIndex) ?? [])
                .Where(pickupIndex => pickupIndex != initialPickupState.pickupIndex)
                    .ToArray();
                if (array.Length != 0)
                {
                    pickupController.pickup =
                        pickupController.pickup.WithPickupIndex(Run.instance.treasureRng.NextElementUniform(array));
                    EffectManager.SimpleEffect(UpgradeItem.transmutationEffectPrefab,
                        pickupController.pickupDisplay.transform.position, Quaternion.identity, true);
                    overrideConsumeOnExit = true;
                    pickupController.ForcePickupDisplayUpdate();
            }
        }
        if (overrideConsumeOnExit) self.bodyBehavior.InvalidateCurrentTarget();
        self.overrideConsumeOnExit = overrideConsumeOnExit;
    }
    
    /// <summary>
    /// static and cleaned version of <see cref="CursedBookBodyBehavior.CanUpgradePickup"/>
    /// <br>Also includes configuration of whether Hexes can be recycled.</br>
    /// </summary>
    /// <param name="genericPickup">The pickup controller of a target.</param>
    /// <returns>Whether <paramref name="genericPickup"/> can be (in this case) Recycled</returns>
    private static bool CanRecyclePickup(GenericPickupController genericPickup)
    {
        var pickupDef = PickupCatalog.GetPickupDef(genericPickup.pickup.pickupIndex);
        if (pickupDef == null || pickupDef.itemIndex == ItemIndex.None) return false;
        
        if (!_enableRecyclingHexes.Value && pickupDef.itemTier == ItemTier.Hex) return false;
        
        var itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
        if (!itemDef) return false;
        if (CursedBookBodyBehavior.bannedTagsForUpgrade.Any(itemDef.ContainsTag))
            return false;
        var itemTierDef = ItemTierCatalog.GetItemTierDef(itemDef.tier);
        return itemTierDef && itemTierDef.previousTier != null && Run.instance.allAvailableTierDroplists.TryGetValue(itemTierDef.previousTier.tier, out var pickupIndexList) && pickupIndexList.Count > 0;
    }

    private static bool CanTransmutePickup(GenericPickupController genericPickup)
    {
        if (!genericPickup) return false;
        var pickupDef = PickupCatalog.GetPickupDef(genericPickup.pickup.pickupIndex);
        if (pickupDef == null ||
            (pickupDef.itemIndex == ItemIndex.None && pickupDef.equipmentIndex == EquipmentIndex.None)) return false;
        var itemDef = ItemCatalog.GetItemDef(pickupDef.itemIndex);
        return !itemDef || !CursedBookBodyBehavior.bannedTagsForUpgrade.Any(itemDef.ContainsTag);
    }
    
    /// <summary>
    /// static and cleaned version of <see cref="CursedBookBodyBehavior.AttemptUpgrade"/>
    /// </summary>
    /// <param name="source">The initial pickup</param>
    /// <param name="result">The recycled pickup or initial</param>
    /// <returns>Whether the recycle was unsuccessful (<paramref name="result"/> == <paramref name="source"/>/></returns>
    internal static bool AttemptRecycle(UniquePickup source, out UniquePickup result)
    {
        result = source;
        var pickupDef = PickupCatalog.GetPickupDef(source.pickupIndex);
        if (pickupDef == null) return result != source;
        var itemIndex = pickupDef.itemIndex;
        var itemTier = pickupDef.itemTier;
        var initialItemDef = ItemCatalog.GetItemDef(itemIndex);
        var itemTierDef = ItemTierCatalog.GetItemTierDef(itemTier);
        if (!initialItemDef || !itemTierDef || itemTierDef.previousTier == null) return result != source;
        var availableTierDroplist = Run.instance.allAvailableTierDroplists[itemTierDef.previousTier.tier];
        var weightedSelection = new WeightedSelection<PickupIndex>();
        foreach (var pickupIndex in availableTierDroplist)
        {
            var weight = 1F;
            var candidatePickupDef = PickupCatalog.GetPickupDef(pickupIndex);
            if (candidatePickupDef == null) continue;
            var candidateItemDef = ItemCatalog.GetItemDef(candidatePickupDef.itemIndex);
            if (candidateItemDef)
            {
                foreach (var tag in candidateItemDef.tags)
                {
                    if (initialItemDef.ContainsTag(tag))
                        weight *= 10F;
                    else
                        weight *= 0.5F;
                }
            }
            weightedSelection.AddChoice(pickupIndex, weight);
        }
        if (weightedSelection.Count > 0)
            result = source.WithPickupIndex(weightedSelection.Evaluate(Run.instance.treasureRng.nextNormalizedFloat));
        return result != source;
    }
}