
namespace AF
{
    using UnityEngine;
    using AF.StatusEffects;
    using System.Collections.Generic;
    using AYellowpaper.SerializedCollections;

    public class CharacterStatusEffectUIManager : MonoBehaviour, IStatusEffectUI
    {
        public CharacterStatusEffectIndicator characterStatusEffectIndicatorPrefab;

        [Header("References")]
        public Transform indicatorInstancesParent;

        [SerializedDictionary("Status Effect", "UI Indicator")]
        public Dictionary<StatusEffect, CharacterStatusEffectIndicator> appliedStatusUIIndicatorInstances = new();

        public void AddEntry(AppliedStatusEffect statusEffect, float currentMaximumResistanceToStatusEffect)
        {
            CharacterStatusEffectIndicator characterStatusEffectIndicator = Instantiate(
                characterStatusEffectIndicatorPrefab, indicatorInstancesParent);

            appliedStatusUIIndicatorInstances.Add(statusEffect.statusEffect, characterStatusEffectIndicator);
        }

        public void UpdateEntry(AppliedStatusEffect appliedStatusEffect, float currentMaximumResistanceToStatusEffect)
        {
            if (appliedStatusUIIndicatorInstances.ContainsKey(appliedStatusEffect.statusEffect))
            {
                appliedStatusUIIndicatorInstances[appliedStatusEffect.statusEffect].UpdateUI(
                    appliedStatusEffect, currentMaximumResistanceToStatusEffect);
            }
        }

        public void RemoveEntry(AppliedStatusEffect appliedStatusEffect)
        {
            if (appliedStatusUIIndicatorInstances.ContainsKey(appliedStatusEffect.statusEffect))
            {
                GameObject tmp = appliedStatusUIIndicatorInstances[appliedStatusEffect.statusEffect].gameObject;
                appliedStatusUIIndicatorInstances.Remove(appliedStatusEffect.statusEffect);
                Destroy(tmp);
            }
        }
    }
}
