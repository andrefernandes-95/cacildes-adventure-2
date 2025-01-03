namespace AF
{
    using System.Collections.Generic;
    using System.Linq;
    using AF.Events;
    using AF.Health;
    using AF.Stats;
    using TigerForge;
    using UnityEngine;
    using UnityEngine.Localization;

    public class PlayerWeaponsManager : MonoBehaviour
    {
        [Header("Unarmed Weapon References In-World")]
        public CharacterWeaponHitbox leftHandHitbox;
        public CharacterWeaponHitbox rightHandHitbox;
        public CharacterWeaponHitbox leftFootHitbox;
        public CharacterWeaponHitbox rightFootHitbox;

        [Header("Weapon References In-World")]
        public List<CharacterWeaponHitbox> weaponInstances;
        public List<CharacterWeaponHitbox> secondaryWeaponInstances;
        public List<ShieldWorldInstance> shieldInstances;
        public List<HolsteredWeapon> holsteredWeapons;

        [Header("Current Weapon")]
        public CharacterWeaponHitbox currentWeaponInstance;
        public CharacterWeaponHitbox currentSecondaryWeaponInstance;
        public ShieldWorldInstance currentShieldInstance;

        [Header("Dual Wielding")]
        public CharacterWeaponHitbox secondaryWeaponInstance;

        [Header("Database")]
        public EquipmentDatabase equipmentDatabase;

        [Header("Components")]
        public PlayerManager playerManager;
        StatsBonusController statsBonusController;
        public NotificationManager notificationManager;

        [Header("Localization")]

        // "Can not apply buff to this weapon"
        public LocalizedString CanNotApplyBuffToThisWeapon;
        // "Weapon is already buffed"
        public LocalizedString WeaponIsAlreadyBuffed;

        private void Awake()
        {
            playerManager.damageReceiver.onDamageEvent += OnDamageEvent;

            statsBonusController = playerManager.statsBonusController;

            EventManager.StartListening(
                EventMessages.ON_EQUIPMENT_CHANGED,
                UpdateEquipment);

            EventManager.StartListening(EventMessages.ON_TWO_HANDING_CHANGED, () =>
            {
                UpdateCurrentWeapon();
                UpdateCurrentShield();
            });
        }

        private void Start()
        {
            UpdateEquipment();
        }

        void UpdateEquipment()
        {
            UpdateCurrentWeapon();
            UpdateCurrentShield();
            UpdateCurrentArrows();
            UpdateCurrentSpells();
        }

        public void ResetStates()
        {
            CloseAllWeaponHitboxes();
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void CloseAllWeaponHitboxes()
        {
            currentWeaponInstance?.DisableHitbox();
            currentSecondaryWeaponInstance?.DisableHitbox();
            leftFootHitbox?.DisableHitbox();
            rightFootHitbox?.DisableHitbox();
            leftHandHitbox?.DisableHitbox();
            rightHandHitbox?.DisableHitbox();
        }

        void UpdateCurrentWeapon()
        {
            var CurrentWeaponInstance = equipmentDatabase.GetCurrentWeapon();
            var SecondaryWeaponInstance = equipmentDatabase.GetCurrentSecondaryWeapon();

            if (currentWeaponInstance != null) currentWeaponInstance = null;
            if (currentSecondaryWeaponInstance != null) currentSecondaryWeaponInstance = null;

            List<CharacterWeaponHitbox> weaponsList = new List<CharacterWeaponHitbox>();
            weaponsList.AddRange(weaponInstances);
            weaponsList.AddRange(secondaryWeaponInstances);

            foreach (CharacterWeaponHitbox weaponHitbox in weaponsList)
            {
                weaponHitbox?.DisableHitbox();
                weaponHitbox?.gameObject.SetActive(false);
            }
            foreach (HolsteredWeapon holsteredWeapon in holsteredWeapons)
            {
                holsteredWeapon?.gameObject.SetActive(false);
            }

            if (CurrentWeaponInstance.Exists())
            {
                var gameObjectWeapon = weaponInstances.FirstOrDefault(weapon => CurrentWeaponInstance.HasItem(weapon.weapon));
                currentWeaponInstance = gameObjectWeapon;

                if (currentWeaponInstance != null)
                {
                    currentWeaponInstance.gameObject.SetActive(true);
                }
            }

            if (SecondaryWeaponInstance.Exists())
            {
                if (equipmentDatabase.isTwoHanding)
                {
                    var holsteredWeapon = holsteredWeapons.FirstOrDefault(holsteredWeapon => SecondaryWeaponInstance.HasItem(holsteredWeapon.weapon));

                    if (holsteredWeapon != null)
                    {
                        holsteredWeapon.gameObject.SetActive(true);
                    }
                    else
                    {
                        Debug.LogError("Missing Holstered Weapon for secondary weapon: " + SecondaryWeaponInstance?.GetItem()?.name ?? "");
                    }
                }
                else
                {
                    var gameObjectWeapon = secondaryWeaponInstances.FirstOrDefault(secondaryWeapon => SecondaryWeaponInstance.HasItem(secondaryWeapon.weapon));

                    if (gameObjectWeapon != null)
                    {
                        secondaryWeaponInstance = gameObjectWeapon;
                        secondaryWeaponInstance.gameObject.SetActive(true);
                    }
                    else
                    {
                        Debug.LogError("Missing Secondary Weapon instance for secondary weapon: " + SecondaryWeaponInstance?.GetItem()?.name ?? "");
                    }
                }
            }

            playerManager.UpdateAnimatorOverrideControllerClips();

            // If we equipped a bow, we must hide any active shield
            if (equipmentDatabase.IsBowEquipped() || equipmentDatabase.IsStaffEquipped())
            {
                UnassignShield();
            }
            // Otherwise, we need to check if we should activate a bow if we just switched from a bow to other weapon that might allow a shield
            else
            {
                UpdateCurrentShield();
            }
        }

        void UpdateCurrentArrows()
        {
            if (equipmentDatabase.IsBowEquipped() == false)
            {
                return;
            }

            UnassignShield();
        }

        void UpdateCurrentSpells()
        {
            if (equipmentDatabase.IsStaffEquipped() == false)
            {
                return;
            }

            UnassignShield();
        }

        void UpdateCurrentShield()
        {
            ShieldInstance CurrentShieldInstance = equipmentDatabase.GetCurrentShield();

            statsBonusController.RecalculateEquipmentBonus();

            if (currentShieldInstance != null)
            {
                currentShieldInstance = null;
            }

            foreach (var shieldInstance in shieldInstances)
            {
                shieldInstance.gameObject.SetActive(false);
                shieldInstance.shieldInTheBack.SetActive(false);
            }

            if (CurrentShieldInstance.Exists())
            {
                var gameObjectShield = shieldInstances.FirstOrDefault(shield => CurrentShieldInstance.HasItem(shield.shield));
                currentShieldInstance = gameObjectShield;
                currentShieldInstance.gameObject.SetActive(true);
            }
        }

        void UnassignShield()
        {
            if (currentShieldInstance != null)
            {
                currentShieldInstance.gameObject.SetActive(false);
                currentShieldInstance.shieldInTheBack.gameObject.SetActive(false);
                currentShieldInstance = null;
            }
        }

        public void EquipWeapon(WeaponInstance weaponToEquip, int slot)
        {
            equipmentDatabase.EquipWeapon(weaponToEquip, slot);

            UpdateCurrentWeapon();
        }

        public void UnequipWeapon(int slot)
        {
            equipmentDatabase.UnequipWeapon(slot);

            UpdateCurrentWeapon();
        }

        public void EquipSecondaryWeapon(WeaponInstance weaponToEquip, int slot)
        {
            equipmentDatabase.EquipSecondaryWeapon(weaponToEquip, slot);

            UpdateCurrentWeapon();
        }

        public void UnequipSecondaryWeapon(int slot)
        {
            equipmentDatabase.UnequipSecondaryWeapon(slot);

            UpdateCurrentWeapon();
        }

        public void EquipShield(ShieldInstance shieldToEquip, int slot)
        {
            equipmentDatabase.EquipShield(shieldToEquip, slot);

            UpdateCurrentShield();

            playerManager.statsBonusController.RecalculateEquipmentBonus();
        }

        public void UnequipShield(int slot)
        {
            equipmentDatabase.UnequipShield(slot);

            UpdateCurrentShield();

            playerManager.statsBonusController.RecalculateEquipmentBonus();
        }

        public void ShowEquipment()
        {
            currentWeaponInstance?.ShowWeapon();
            currentSecondaryWeaponInstance?.ShowWeapon();
            currentShieldInstance?.ResetStates();
        }

        public void HideEquipment()
        {
            currentWeaponInstance?.HideWeapon();
            currentSecondaryWeaponInstance?.HideWeapon();
            currentShieldInstance?.HideShield();
        }

        public void HideShield() => currentShieldInstance?.HideShield();
        public void ShowShield() => currentShieldInstance?.ShowShield();

        bool CanApplyBuff()
        {
            if (currentWeaponInstance == null || currentWeaponInstance.characterWeaponBuffs == null)
            {
                notificationManager.ShowNotification(
                    CanNotApplyBuffToThisWeapon.GetLocalizedString(), notificationManager.systemError);
                return false;
            }
            else if (currentWeaponInstance.characterWeaponBuffs.HasOnGoingBuff())
            {
                notificationManager.ShowNotification(
                    WeaponIsAlreadyBuffed.GetLocalizedString(), notificationManager.systemError);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void ApplyFireToWeapon(float customDuration)
        {
            ApplyWeaponBuffToWeapon(CharacterWeaponBuffs.WeaponBuffName.FIRE, customDuration);
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void ApplyFrostToWeapon(float customDuration)
        {
            ApplyWeaponBuffToWeapon(CharacterWeaponBuffs.WeaponBuffName.FROST, customDuration);
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void ApplyLightningToWeapon(float customDuration)
        {
            ApplyWeaponBuffToWeapon(CharacterWeaponBuffs.WeaponBuffName.LIGHTNING, customDuration);
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void ApplyMagicToWeapon(float customDuration)
        {
            ApplyWeaponBuffToWeapon(CharacterWeaponBuffs.WeaponBuffName.MAGIC, customDuration);
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void ApplyDarknessToWeapon(float customDuration)
        {
            ApplyWeaponBuffToWeapon(CharacterWeaponBuffs.WeaponBuffName.DARKNESS, customDuration);
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void ApplyPoisonToWeapon(float customDuration)
        {
            ApplyWeaponBuffToWeapon(CharacterWeaponBuffs.WeaponBuffName.POISON, customDuration);
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void ApplyBloodToWeapon(float customDuration)
        {
            ApplyWeaponBuffToWeapon(CharacterWeaponBuffs.WeaponBuffName.BLOOD, customDuration);
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void ApplySharpnessToWeapon(float customDuration)
        {
            ApplyWeaponBuffToWeapon(CharacterWeaponBuffs.WeaponBuffName.SHARPNESS, customDuration);
        }


        public void ApplyWeaponBuffToWeapon(CharacterWeaponBuffs.WeaponBuffName weaponBuffName, float customDuration)
        {
            if (!CanApplyBuff())
            {
                return;
            }

            if (customDuration > 0)
            {
                currentWeaponInstance?.characterWeaponBuffs?.ApplyBuff(weaponBuffName, customDuration);
                secondaryWeaponInstance?.characterWeaponBuffs?.ApplyBuff(weaponBuffName, customDuration);
            }
            else
            {
                currentWeaponInstance?.characterWeaponBuffs?.ApplyBuff(weaponBuffName);
                secondaryWeaponInstance?.characterWeaponBuffs?.ApplyBuff(weaponBuffName);
            }
        }

        public Damage GetBuffedDamage(Damage weaponDamage)
        {
            if (currentWeaponInstance == null || currentWeaponInstance.characterWeaponBuffs == null || currentWeaponInstance.characterWeaponBuffs.HasOnGoingBuff() == false)
            {
                return weaponDamage;
            }

            if (currentWeaponInstance.characterWeaponBuffs.appliedBuff == CharacterWeaponBuffs.WeaponBuffName.FIRE)
            {
                weaponDamage.fire += currentWeaponInstance.characterWeaponBuffs.weaponBuffs[CharacterWeaponBuffs.WeaponBuffName.FIRE].damageBonus;
            }

            if (currentWeaponInstance.characterWeaponBuffs.appliedBuff == CharacterWeaponBuffs.WeaponBuffName.FROST)
            {
                weaponDamage.frost += currentWeaponInstance.characterWeaponBuffs.weaponBuffs[CharacterWeaponBuffs.WeaponBuffName.FROST].damageBonus;
            }

            if (currentWeaponInstance.characterWeaponBuffs.appliedBuff == CharacterWeaponBuffs.WeaponBuffName.LIGHTNING)
            {
                weaponDamage.lightning += currentWeaponInstance.characterWeaponBuffs.weaponBuffs[CharacterWeaponBuffs.WeaponBuffName.LIGHTNING].damageBonus;
            }

            if (currentWeaponInstance.characterWeaponBuffs.appliedBuff == CharacterWeaponBuffs.WeaponBuffName.MAGIC)
            {
                weaponDamage.magic += currentWeaponInstance.characterWeaponBuffs.weaponBuffs[CharacterWeaponBuffs.WeaponBuffName.MAGIC].damageBonus;
            }

            if (currentWeaponInstance.characterWeaponBuffs.appliedBuff == CharacterWeaponBuffs.WeaponBuffName.DARKNESS)
            {
                weaponDamage.darkness += currentWeaponInstance.characterWeaponBuffs.weaponBuffs[CharacterWeaponBuffs.WeaponBuffName.DARKNESS].damageBonus;
            }

            if (currentWeaponInstance.characterWeaponBuffs.appliedBuff == CharacterWeaponBuffs.WeaponBuffName.WATER)
            {
                weaponDamage.water += currentWeaponInstance.characterWeaponBuffs.weaponBuffs[CharacterWeaponBuffs.WeaponBuffName.WATER].damageBonus;
            }

            if (currentWeaponInstance.characterWeaponBuffs.appliedBuff == CharacterWeaponBuffs.WeaponBuffName.SHARPNESS)
            {
                weaponDamage.physical += currentWeaponInstance.characterWeaponBuffs.weaponBuffs[CharacterWeaponBuffs.WeaponBuffName.SHARPNESS].damageBonus;
            }

            if (currentWeaponInstance.characterWeaponBuffs.appliedBuff == CharacterWeaponBuffs.WeaponBuffName.POISON)
            {
                StatusEffectEntry statusEffectToApply = new()
                {
                    statusEffect = currentWeaponInstance.characterWeaponBuffs.weaponBuffs[CharacterWeaponBuffs.WeaponBuffName.POISON].statusEffect,
                    amountPerHit = currentWeaponInstance.characterWeaponBuffs.weaponBuffs[CharacterWeaponBuffs.WeaponBuffName.POISON].statusEffectAmountToApply,
                };

                if (weaponDamage.statusEffects == null)
                {
                    weaponDamage.statusEffects = new StatusEffectEntry[] {
                        statusEffectToApply
                    };
                }
                else
                {
                    weaponDamage.statusEffects = weaponDamage.statusEffects.Append(statusEffectToApply).ToArray();
                }
            }

            if (currentWeaponInstance.characterWeaponBuffs.appliedBuff == CharacterWeaponBuffs.WeaponBuffName.BLOOD)
            {
                StatusEffectEntry statusEffectToApply = new()
                {
                    statusEffect = currentWeaponInstance.characterWeaponBuffs.weaponBuffs[CharacterWeaponBuffs.WeaponBuffName.BLOOD].statusEffect,
                    amountPerHit = currentWeaponInstance.characterWeaponBuffs.weaponBuffs[CharacterWeaponBuffs.WeaponBuffName.BLOOD].statusEffectAmountToApply,
                };

                if (weaponDamage.statusEffects == null)
                {
                    weaponDamage.statusEffects = new StatusEffectEntry[] {
                        statusEffectToApply
                    };
                }
                else
                {
                    weaponDamage.statusEffects = weaponDamage.statusEffects.Append(statusEffectToApply).ToArray();
                }
            }

            return weaponDamage;
        }

        public int GetCurrentBlockStaminaCost()
        {
            if (playerManager.playerWeaponsManager.currentShieldInstance == null)
            {
                return playerManager.characterBlockController.unarmedStaminaCostPerBlock;
            }

            return (int)playerManager.playerWeaponsManager.currentShieldInstance.shield.blockStaminaCost;
        }

        public Damage GetCurrentShieldDefenseAbsorption(Damage incomingDamage)
        {
            if (equipmentDatabase.isTwoHanding && equipmentDatabase.GetCurrentWeapon().Exists())
            {
                incomingDamage.physical = (int)(incomingDamage.physical * equipmentDatabase.GetCurrentWeapon().GetItem().blockAbsorption);
                return incomingDamage;
            }
            else if (currentShieldInstance == null || currentShieldInstance.shield == null)
            {
                incomingDamage.physical = (int)(incomingDamage.physical * playerManager.characterBlockController.unarmedDefenseAbsorption);
                return incomingDamage;
            }

            return currentShieldInstance.shield.FilterDamage(incomingDamage);
        }
        public Damage GetCurrentShieldPassiveDamageFilter(Damage incomingDamage)
        {
            if (currentShieldInstance == null || currentShieldInstance.shield == null)
            {
                return incomingDamage;
            }

            return currentShieldInstance.shield.FilterPassiveDamage(incomingDamage);
        }

        public void ApplyShieldDamageToAttacker(CharacterManager attacker)
        {
            if (currentShieldInstance == null || currentShieldInstance.shield == null)
            {
                return;
            }

            currentShieldInstance.shield.AttackShieldAttacker(attacker);
        }

        public void HandleWeaponSpecial()
        {
            if (
                playerManager.playerWeaponsManager.currentWeaponInstance == null
                || playerManager.playerWeaponsManager.currentWeaponInstance.onWeaponSpecial == null
                || playerManager.playerWeaponsManager.currentWeaponInstance.weapon == null
                )
            {
                return;
            }

            if (playerManager.manaManager.playerStatsDatabase.currentMana < playerManager.playerWeaponsManager.currentWeaponInstance.weapon.manaCostToUseWeaponSpecialAttack)
            {
                //                notificationManager.ShowNotification(NotEnoughManaToUseWeaponSpecial.GetLocalizedString());
                return;
            }

            playerManager.manaManager.DecreaseMana(
                playerManager.playerWeaponsManager.currentWeaponInstance.weapon.manaCostToUseWeaponSpecialAttack
            );

            playerManager.playerWeaponsManager.currentWeaponInstance.onWeaponSpecial?.Invoke();
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void ThrowWeapon()
        {

        }

        public Damage OnDamageEvent(CharacterBaseManager attacker, CharacterBaseManager receiver, Damage damage)
        {
            if (damage == null)
            {
                return null;
            }

            return GetCurrentShieldPassiveDamageFilter(damage);
        }

    }
}
