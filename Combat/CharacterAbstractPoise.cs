namespace AF
{
    using System.Collections;
    using AF.Health;
    using UnityEngine;
    using UnityEngine.Events;

    public abstract class CharacterAbstractPoise : MonoBehaviour
    {
        [Header("Animations")]
        public string takingDamageAnimationClip = "Taking Damage";

        public int currentPoiseHitCount = 0;

        [Header("Components")]
        public CharacterBaseManager characterManager;

        [Header("Settings")]
        [Tooltip("How many hits can the enemy take and continuing attacking")]
        public float maxTimeBeforeResettingPoise = 5f;

        [Header("Unity Events")]
        public UnityEvent onPoiseDamagedEvent;

        Coroutine ResetPoiseCoroutine;

        private void Awake()
        {
            characterManager.damageReceiver.onDamageEvent += OnDamageEvent;
        }

        public abstract void ResetStates();

        public virtual bool TakePoiseDamage(int poiseDamage)
        {
            if (!CanTakePoiseDamage())
            {
                return false;
            }

            currentPoiseHitCount = poiseDamage > 0 ? Mathf.Clamp(currentPoiseHitCount + 1 + poiseDamage, 0, GetMaxPoiseHits()) : 0;

            if (ResetPoiseCoroutine != null)
            {
                StopCoroutine(ResetPoiseCoroutine);
            }

            bool hasBrokenPoise = false;

            if (currentPoiseHitCount >= GetMaxPoiseHits())
            {
                hasBrokenPoise = true;

                currentPoiseHitCount = 0;

                if (CanCallPoiseDamagedEvent())
                {
                    onPoiseDamagedEvent?.Invoke();
                    characterManager.PlayBusyAnimationWithRootMotion(takingDamageAnimationClip);
                }

                characterManager.combatant.PlayHurt(characterManager.combatAudioSource);
            }
            else
            {
                StartCoroutine(ResetPoise());
            }

            return hasBrokenPoise;
        }

        IEnumerator ResetPoise()
        {
            yield return new WaitForSeconds(maxTimeBeforeResettingPoise);
            currentPoiseHitCount = 0;
        }

        public abstract int GetMaxPoiseHits();
        public abstract bool CanCallPoiseDamagedEvent();

        bool CanTakePoiseDamage()
        {
            if (characterManager.characterPosture.isStunned)
            {
                return false;
            }

            if (characterManager.health.GetCurrentHealth() <= 0)
            {
                return false;
            }

            if (characterManager is CharacterManager character && character.characterBackstabController.isBeingBackstabbed)
            {
                return false;
            }

            return true;
        }

        public Damage OnDamageEvent(CharacterBaseManager attacker, CharacterBaseManager receiver, Damage incomingDamage)
        {
            if (incomingDamage == null)
            {
                return incomingDamage;
            }

            TakePoiseDamage(incomingDamage.poiseDamage);

            return incomingDamage;
        }
    }

}
