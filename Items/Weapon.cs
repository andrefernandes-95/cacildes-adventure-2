﻿
namespace AF
{
    using System;
    using AF.Inventory;
    using AF.Stats;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Localization.Settings;

    [CreateAssetMenu(menuName = "Items / Weapon / New Weapon")]
    public class Weapon : Item
    {
        public WeaponAnimations weaponAnimations;
        public WeaponDamage weaponDamage;

        public bool canBeUpgraded = true;
        public WeaponScaling strengthScaling = WeaponScaling.E;
        public WeaponScaling dexterityScaling = WeaponScaling.E;
        public WeaponScaling intelligenceScaling = WeaponScaling.E;

        public int strengthRequired = 0;
        public int dexterityRequired = 0;
        public int intelligenceRequired = 0;
        public int positiveReputationRequired = 0;
        public int negativeReputationRequired = 0;
        public int lightAttackStaminaCost = 20;
        public int heavyAttackStaminaCost = 35;
        public int manaCostToUseWeaponSpecialAttack = 0;
        public float speedPenalty = 0f;
        [Range(0.1f, 2f)] public float oneHandAttackSpeedPenalty = 1f;
        [Range(0.1f, 2f)] public float twoHandAttackSpeedPenalty = 1f;
        [Min(1f)] public float coinMultiplierPerFallenEnemy = 1f;
        public bool doubleDamageDuringNightTime = false;
        public bool doubleDamageDuringDayTime = false;
        public int healthRestoredWithEachHit = 0;

        [Header("Range Category")]
        public bool isCrossbow = false;
        public bool isHuntingRifle = false;
        [Range(0, 1f)] public float blockAbsorption = .8f;

        [Header("Staff Options")]
        public bool shouldRegenerateMana = false;
        public bool ignoreSpellsAnimationClips = false;

#if UNITY_EDITOR
        private void OnEnable()
        {
            // No need to populate the list; it's serialized directly
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
            }
        }
#endif

        public bool HasRequirements()
        {
            return strengthRequired != 0 || dexterityRequired != 0 || intelligenceRequired != 0 || positiveReputationRequired != 0 || negativeReputationRequired != 0;
        }

        public bool AreRequirementsMet(StatsBonusController statsBonusController)
        {
            if (statsBonusController.ignoreWeaponRequirements)
            {
                return true;
            }

            if (strengthRequired != 0 && statsBonusController.GetCurrentStrength() < strengthRequired)
            {
                return false;
            }
            else if (dexterityRequired != 0 && statsBonusController.GetCurrentDexterity() < dexterityRequired)
            {
                return false;
            }
            else if (intelligenceRequired != 0 && statsBonusController.GetCurrentIntelligence() < intelligenceRequired)
            {
                return false;
            }
            else if (positiveReputationRequired != 0 && statsBonusController.GetCurrentReputation() < positiveReputationRequired)
            {
                return false;
            }
            else if (negativeReputationRequired != 0 && statsBonusController.GetCurrentReputation() > -negativeReputationRequired)
            {
                return false;
            }

            return true;
        }

        public string DrawRequirements(StatsBonusController statsBonusController)
        {
            string text = AreRequirementsMet(statsBonusController)
                ? LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "Requirements met: ")
                : LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "Requirements not met: ");

            if (strengthRequired != 0)
            {
                text += $"  {LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "Strength Required:")} {strengthRequired}   {LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "Current:")} {statsBonusController.GetCurrentStrength()}\n";
            }
            if (dexterityRequired != 0)
            {
                text += $"  {LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "Dexterity Required:")} {dexterityRequired}   {LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "Current:")} {statsBonusController.GetCurrentDexterity()}\n";
            }
            if (intelligenceRequired != 0)
            {
                text += $"  {LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "Intelligence Required:")} {intelligenceRequired}   {LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "Current:")} {statsBonusController.GetCurrentIntelligence()}\n";
            }
            if (positiveReputationRequired != 0)
            {
                text += $"  {LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "Reputation Required:")} {intelligenceRequired}   {LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "Current:")} {statsBonusController.GetCurrentReputation()}\n";
            }

            if (negativeReputationRequired != 0)
            {
                text += $"  {LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "Reputation Required:")} -{negativeReputationRequired}   {LocalizationSettings.StringDatabase.GetLocalizedString("UIDocuments", "Current:")} {statsBonusController.GetCurrentReputation()}\n";
            }

            return text.TrimEnd();
        }

        public bool IsHolyWeapon(WeaponInstance weaponInstance, InventoryDatabase inventoryDatabase)
            => weaponDamage.HasHolyDamage(weaponInstance, inventoryDatabase);

        public bool IsRangeWeapon()
        {
            return weaponDamage.weaponUpgradeLevels.Length > 0
                && weaponDamage.weaponUpgradeLevels[0].damage.weaponAttackType == WeaponAttackType.Range;
        }
        public bool IsStaffWeapon()
        {
            return weaponDamage.weaponUpgradeLevels.Length > 0
                && weaponDamage.weaponUpgradeLevels[0].damage.weaponAttackType == WeaponAttackType.Staff;
        }

    }

}
