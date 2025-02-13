namespace AF
{
    using System.Linq;
    using AF.Health;
    using AF.StatusEffects;
    using UnityEngine;
    using UnityEngine.AI;

    public abstract class CharacterBaseManager : MonoBehaviour
    {
        public Combatant combatant;

        [Header("Components")]
        public Animator animator;
        public NavMeshAgent agent;
        public CharacterController characterController;

        [Header("Audio Sources")]
        public AudioSource combatAudioSource;

        [Header("Flags")]
        public bool isBusy = false;

        public bool isConfused = false;

        [Header("Components")]
        public StatusController statusController;
        public CharacterBaseHealth health;
        public CharacterAbstractPosture characterPosture;
        public CharacterAbstractPoise characterPoise;
        public CharacterAbstractBlockController characterBlockController;
        public DamageReceiver damageReceiver;
        public CharacterPushController characterPushController;
        public abstract void ResetStates();

        public bool IsBusy()
        {
            return isBusy;
        }

        public void SetIsBusy(bool value)
        {
            isBusy = value;
        }

        public void PlayAnimationWithCrossFade(string animationName)
        {
            PlayAnimationWithCrossFade(animationName, false, false, 0.2f);
        }

        public void PlayAnimationWithCrossFade(string animationName, bool isBusy, bool applyRootMotion, float crossFade)
        {
            this.isBusy = isBusy;
            animator.applyRootMotion = applyRootMotion;

            animator.CrossFade(animationName, 0.2f);
        }

        public void PlayBusyAnimation(string animationName)
        {
            isBusy = true;
            animator.Play(animationName);
        }

        public void PlayBusyAnimationWithRootMotion(string animationName)
        {
            animator.applyRootMotion = true;
            PlayBusyAnimation(animationName);
        }


        public void PlayCrossFadeBusyAnimationWithRootMotion(string animationName, float crossFade)
        {
            animator.applyRootMotion = true;
            isBusy = true;
            animator.CrossFade(animationName, crossFade);
        }

        #region Hashed Animations
        public void PlayBusyHashedAnimationWithRootMotion(int hashedAnimationName)
        {
            animator.applyRootMotion = true;
            PlayBusyHashedAnimation(hashedAnimationName);
        }

        public void PlayBusyHashedAnimation(int animationName)
        {
            isBusy = true;
            animator.Play(animationName);
        }
        #endregion

        public abstract Damage GetAttackDamage();

        public bool IsFromSameFaction(CharacterBaseManager target)
        {
            if (target == null)
            {
                return false;
            }

            return combatant.IsFriendsWith(target.combatant);
        }

        public void SetIsConfused(bool value)
        {
            this.isConfused = value;
        }

        public void ResetIsConfused()
        {
            this.isConfused = false;
        }
    }
}
