namespace AF.Animations
{
    using UnityEngine;
    using UnityEngine.Events;
    using Unity.Cinemachine;

    public class PlayerAnimationEventListener : MonoBehaviour, IAnimationEventListener
    {
        public PlayerManager playerManager;

        [Header("Unity Events")]
        public UnityEvent onLeftFootstep;
        public UnityEvent onRightFootstep;
        public CinemachineImpulseSource cinemachineImpulseSource;

        [Header("Components")]
        public AudioSource combatAudioSource;
        public Soundbank soundbank;

        [Header("Settings")]
        public float animatorSpeed = 1f;
        float defaultAnimatorSpeed;

        private void Awake()
        {
            playerManager.animator.speed = animatorSpeed;
            defaultAnimatorSpeed = animatorSpeed;
        }

        public void OpenHeadWeaponHitbox()
        {
        }

        public void CloseHeadWeaponHitbox()
        {
        }

        public void OpenLeftWeaponHitbox()
        {
            if (playerManager.playerWeaponsManager.secondaryWeaponInstance != null)
            {
                playerManager.playerWeaponsManager.secondaryWeaponInstance.EnableHitbox();
            }
            else if (playerManager.playerWeaponsManager.leftHandHitbox != null)
            {
                playerManager.playerWeaponsManager.leftHandHitbox.EnableHitbox();
            }

            DisableRotation();
        }

        public void CloseLeftWeaponHitbox()
        {
            if (playerManager.playerWeaponsManager.secondaryWeaponInstance != null)
            {
                playerManager.playerWeaponsManager.secondaryWeaponInstance.DisableHitbox();
            }
            else if (playerManager.playerWeaponsManager.leftHandHitbox != null)
            {
                playerManager.playerWeaponsManager.leftHandHitbox.DisableHitbox();
            }
        }

        public void OpenRightWeaponHitbox()
        {
            if (playerManager.playerWeaponsManager.currentWeaponWorldInstance != null)
            {
                playerManager.playerWeaponsManager.currentWeaponWorldInstance.EnableHitbox();
            }
            else if (playerManager.playerWeaponsManager.rightHandHitbox != null)
            {
                playerManager.playerWeaponsManager.rightHandHitbox.EnableHitbox();
            }

            DisableRotation();
        }

        public void CloseRightWeaponHitbox()
        {
            if (playerManager.playerWeaponsManager.currentWeaponWorldInstance != null)
            {
                playerManager.playerWeaponsManager.currentWeaponWorldInstance.DisableHitbox();
            }

            if (playerManager.playerWeaponsManager.rightHandHitbox != null)
            {
                playerManager.playerWeaponsManager.rightHandHitbox.DisableHitbox();
            }
        }

        public void OpenLeftFootHitbox()
        {
            if (playerManager.playerWeaponsManager.leftFootHitbox != null)
            {
                playerManager.playerWeaponsManager.leftFootHitbox.EnableHitbox();
            }

            DisableRotation();
        }

        public void CloseLeftFootHitbox()
        {
            if (playerManager.playerWeaponsManager.leftFootHitbox != null)
            {
                playerManager.playerWeaponsManager.leftFootHitbox.DisableHitbox();
            }
        }

        public void OpenRightFootHitbox()
        {
            if (playerManager.playerWeaponsManager.rightFootHitbox != null)
            {
                playerManager.playerWeaponsManager.rightFootHitbox.EnableHitbox();
            }

            DisableRotation();
        }

        public void CloseRightFootHitbox()
        {
            if (playerManager.playerWeaponsManager.rightFootHitbox != null)
            {
                playerManager.playerWeaponsManager.rightFootHitbox.DisableHitbox();
            }
        }
        public void EnableRotation()
        {
            playerManager.thirdPersonController.canRotateCharacter = true;
        }

        public void DisableRotation()
        {
            playerManager.thirdPersonController.canRotateCharacter = false;
        }

        public void EnableRootMotion()
        {
            playerManager.animator.applyRootMotion = true;
        }

        public void DisableRootMotion()
        {
            playerManager.animator.applyRootMotion = false;
        }

        public void FaceTarget()
        {
        }

        public void SetAnimatorBool_True(string parameterName)
        {
            playerManager.animator.SetBool(parameterName, true);
        }

        public void SetAnimatorBool_False(string parameterName)
        {
            playerManager.animator.SetBool(parameterName, false);
        }

        public void OnSpellCast()
        {
            playerManager.playerShootingManager.CastSpell();
        }

        public void OnFireArrow()
        {
            playerManager.playerShootingManager.OnShoot();
        }

        public void OnFireMultipleArrows()
        {
            playerManager.playerShootingManager.HandleProjectileShot(false);
        }

        public void OnLeftFootstep()
        {

            onLeftFootstep?.Invoke();
        }

        public void OnRightFootstep()
        {
            onRightFootstep?.Invoke();
        }

        public void OnCloth()
        {
            if (playerManager.thirdPersonController.Grounded)
            {
                soundbank.PlaySound(soundbank.dodge, combatAudioSource);
            }
            else
            {
                soundbank.PlaySound(soundbank.cloth, combatAudioSource);
            }
        }

        public void OnImpact()
        {
            soundbank.PlaySound(soundbank.impact, combatAudioSource);
        }

        public void OnBuff()
        {
        }

        public void OnThrow()
        {
            playerManager.projectileSpawner.ThrowProjectile();
        }

        public void OnBlood()
        {
        }

        public void RestoreDefaultAnimatorSpeed()
        {
            this.animatorSpeed = defaultAnimatorSpeed;
            playerManager.animator.speed = animatorSpeed;

        }

        public void SetAnimatorSpeed(float speed)
        {
            this.animatorSpeed = speed;
            playerManager.animator.speed = animatorSpeed;

        }

        public void OnShakeCamera()
        {
            cinemachineImpulseSource.GenerateImpulse();
        }

        public void ShowShield()
        {
            playerManager.playerWeaponsManager.ShowShield();
        }

        public void DropIKHelper()
        {
            playerManager.SetCanUseIK_False();
        }

        public void UseIKHelper()
        {
            playerManager.SetCanUseIK_True();
        }

        public void SetCanTakeDamage_False()
        {
            playerManager.damageReceiver.SetCanTakeDamage(false);
        }

        public void OnWeaponSpecial()
        {
            playerManager.playerWeaponsManager.HandleWeaponSpecial();
        }

        public void MoveTowardsTarget()
        {
        }

        public void StopMoveTowardsTarget()
        {
        }

        public void OnSwim()
        {
            playerManager.thirdPersonController.OnSwimAnimationEvent();
        }

        public void PauseAnimation()
        {
        }

        public void ResumeAnimation()
        {
        }

        public void StopIframes()
        {
            playerManager.dodgeController.StopIframes();
        }
        public void EnableIframes()
        {
            playerManager.dodgeController.EnableIframes();
        }

        public void OnCard()
        {
            playerManager.playerCardManager.UseCurrentCard();
        }

        public void OnExecuted()
        {
        }

        public void OnExecuting()
        {
            playerManager.executionerManager.OnExecuting();
        }

        public void ShowRifleWeapon()
        {
        }

        public void HideRifleWeapon()
        {
        }

        public void OnPushObject()
        {
            playerManager.pushObjectManager.OnPushObject();
        }
    }
}
