﻿using System.Collections;
using AF.Health;
using AF.Shooting;
using UnityEngine;
using UnityEngine.Events;

namespace AF
{
    public class Projectile : MonoBehaviour, IProjectile
    {
        [Header("Lifespan")]
        public float timeBeforeDestroying = 10;

        [Header("Velocity")]
        public ForceMode forceMode;
        public float forwardVelocity = 10f;
        public bool useOwnDirection = false;

        [Header("Stats")]
        public Damage damage;
        public bool scaleWithIntelligence = false;

        public Rigidbody rigidBody;
        public GameObject disappearingFx;

        [Header("Events")]

        [Tooltip("Fires immediately after instatied")] public UnityEvent onFired;
        [Tooltip("Fires after 0.1ms")] public UnityEvent onFired_After;
        public UnityEvent onCollision;
        public float onFired_AfterDelay = 0.1f;

        // Flags
        bool hasCollided = false;

        CharacterBaseManager shooter;


        [Header("Collision Options")]
        public bool collideWithAnything = false;
        public UnityEvent onAnyCollision;

        private void OnEnable()
        {
            onFired?.Invoke();

            StartCoroutine(HandleOnFiredAfter_Coroutine());
        }

        private void OnDisable()
        {
            if (disappearingFx != null)
            {
                Instantiate(disappearingFx, transform.position, Quaternion.identity);
            }
        }

        IEnumerator HandleOnFiredAfter_Coroutine()
        {
            yield return new WaitForSeconds(onFired_AfterDelay);
            onFired_After?.Invoke();
        }

        public void Shoot(CharacterBaseManager shooter, Vector3 aimForce, ForceMode forceMode)
        {
            this.shooter = shooter;

            if (useOwnDirection)
            {
                transform.rotation = Quaternion.LookRotation(aimForce - transform.position);
            }

            rigidBody.AddForce(useOwnDirection ? (transform.forward * GetForwardVelocity()) : aimForce, forceMode);
        }

        public void ShootForward()
        {
            rigidBody.AddForce(transform.forward * GetForwardVelocity(), forceMode);
        }

        void OnTriggerEnter(Collider other)
        {
            if (hasCollided)
            {
                return;
            }

            other.TryGetComponent(out DamageReceiver damageReceiver);

            HandleCollision(damageReceiver);
        }

        public void HandleCollision(DamageReceiver damageReceiver)
        {
            if (collideWithAnything == false && damageReceiver == null || damageReceiver?.character == shooter)
            {
                return;
            }

            hasCollided = true;

            if (collideWithAnything)
            {
                onAnyCollision?.Invoke();
                return;
            }

            if (shooter is PlayerManager playerManager && playerManager.attackStatManager.equipmentDatabase.GetCurrentWeapon().Exists())
            {
                if (scaleWithIntelligence)
                {
                    damage.ScaleSpell(
                        playerManager.attackStatManager, playerManager.attackStatManager.equipmentDatabase.GetCurrentWeapon(), 0, false, false, false);
                }
                else if (playerManager.attackStatManager.HasBowEquipped())
                {
                    damage.ScaleProjectile(playerManager.attackStatManager, playerManager.attackStatManager.equipmentDatabase.GetCurrentWeapon());
                }
            }
            else if (shooter is CharacterManager enemy)
            {
                damage.ScaleDamageForNewGamePlus(enemy.gameSession);
            }

            damageReceiver.ApplyDamage(shooter, damage);

            if (shooter != null
                && damageReceiver?.character is CharacterManager characterManager
                && characterManager.targetManager != null)
            {
                characterManager.targetManager.SetTarget(shooter);
            }

            onCollision?.Invoke();

            StartCoroutine(HandleDestroy_Coroutine());
        }

        IEnumerator HandleDestroy_Coroutine()
        {
            yield return new WaitForSeconds(timeBeforeDestroying);

            Destroy(this.gameObject);
        }

        public float GetForwardVelocity()
        {
            return forwardVelocity;
        }

        public ForceMode GetForceMode()
        {
            return forceMode;
        }
    }
}
