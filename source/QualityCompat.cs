using System;
using HG;
using ItemQualities;
using ItemQualities.Utilities.Extensions;
using RoR2;
using RoR2.ContentManagement;

namespace RDAS;

internal static class QualityCompat
{

    public static void Init()
    {
        Log.Info("Initializing Quality compatibility...");
        ContentManager.onContentPacksAssigned += ChangeQualityDefs; // Run before QualityCatalog formats in its Init tokens
    }

    public static void RemoveHooks()
    {
        IL.RoR2.EquipmentSlot.UpdateTargets -= ItemQualities.Equipments.Recycle.EquipmentSlot_UpdateTargets;
        IL.RoR2.EquipmentSlot.FireRecycle -= ItemQualities.Equipments.Recycle.IL_EquipmentSlot_FireRecycle;
        On.RoR2.EquipmentSlot.FireRecycle -= ItemQualities.Equipments.Recycle.On_EquipmentSlot_FireRecycle;
    }

    private static void ChangeQualityDefs(ReadOnlyArray<ReadOnlyContentPack> readOnlyContentPacks)
    {
        var recyclerGroup = ItemQualitiesContent.EquipmentQualityGroups.Recycle;
        
        recyclerGroup.GetEquipmentDef(QualityTier.Uncommon).pickupToken = "EQUIPMENT_RDAS_RECYCLER_UNCOMMON_PICKUP";
        recyclerGroup.GetEquipmentDef(QualityTier.Uncommon).descriptionToken = "EQUIPMENT_RDAS_RECYCLER_UNCOMMON_DESC";
                
        recyclerGroup.GetEquipmentDef(QualityTier.Rare).pickupToken = "EQUIPMENT_RDAS_RECYCLER_RARE_PICKUP";
        recyclerGroup.GetEquipmentDef(QualityTier.Rare).descriptionToken = "EQUIPMENT_RDAS_RECYCLER_RARE_DESC";
                
        recyclerGroup.GetEquipmentDef(QualityTier.Epic).pickupToken = "EQUIPMENT_RDAS_RECYCLER_EPIC_PICKUP";
        recyclerGroup.GetEquipmentDef(QualityTier.Epic).descriptionToken = "EQUIPMENT_RDAS_RECYCLER_EPIC_DESC";
        
        recyclerGroup.GetEquipmentDef(QualityTier.Legendary).pickupToken = "EQUIPMENT_RDAS_RECYCLER_LEGENDARY_PICKUP";
        recyclerGroup.GetEquipmentDef(QualityTier.Legendary).descriptionToken = "EQUIPMENT_RDAS_RECYCLER_LEGENDARY_DESC";
    }

    internal static bool AttemptRecycleQuality(EquipmentSlot equipmentSlot, UniquePickup source, out UniquePickup result)
    {
        var recyclerQuality = equipmentSlot.GetActiveEquipmentQualityTier();
        var sourceQuality = QualityCatalog.GetQualityTier(source.pickupIndex);
        Log.Debug($"AttemptRecycleQuality: recyclerQuality = {nameof(recyclerQuality)}");
        Log.Debug($"AttemptRecycleQuality: targetQuality = {nameof(sourceQuality)}");
        if (recyclerQuality == QualityTier.None || sourceQuality == QualityTier.None) return ModPlugin.AttemptRecycle(source, out result);
        
        var resultQuality = (QualityTier)Math.Min((int)recyclerQuality, (int)sourceQuality);
        Log.Debug($"AttemptRecycleQuality: resultQuality = {resultQuality}, resultQualityDef = {QualityCatalog.GetQualityTierDef(resultQuality).name}");
        
        ModPlugin.AttemptRecycle(source, out result);
        result = result.WithPickupIndex(QualityCatalog.GetPickupIndexOfQuality(result.pickupIndex, resultQuality));
        Log.Debug(
            $"AttemptRecycleQuality: result = {ItemCatalog.GetItemDef(PickupCatalog.GetPickupDef(result.pickupIndex).itemIndex).name}" +
            (QualityCatalog.GetQualityTier(result.pickupIndex) == QualityTier.None ? "(Quality erased T-T)" : ""));
        
        return result != source;
    }
}