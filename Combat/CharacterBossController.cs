namespace AF
{
    using System.Linq;
    using AF.Events;
    using AF.Flags;
    using AF.Music;
    using TigerForge;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UIElements;

    public class CharacterBossController : MonoBehaviour
    {
        public bool isBoss = false;

        [Header("Settings")]
        public string bossName;
        public GameObject fogwall;
        public AudioClip bossMusic;

        public UIDocument bossHud;
        public IMGUIContainer bossFillBar;

        public CharacterManager characterManager;

        [Header("Events")]
        public UnityEvent onBattleBegin;
        public UnityEvent onBossDefeated;

        // Flags
        [HideInInspector] public bool bossBattleHasBegun = false;

        [Header("Flags")]
        public MonoBehaviourID monoBehaviourID;
        public FlagsDatabase flagsDatabase;

        // Scene References
        private BGMManager bgmManager;
        private SceneSettings _sceneSettings;

        public void Start()
        {
            if (fogwall != null)
            {
                fogwall.SetActive(false);
            }

            HideBossHud();
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void UpdateUI()
        {
            if (IsBossHUDEnabled())
            {
                if (!isBoss)
                {
                    return;
                }

                bossFillBar ??= bossHud.rootVisualElement.Q<IMGUIContainer>("hp-bar");
                bossFillBar.style.width = new Length(characterManager.health.GetCurrentHealth() * 100 / characterManager.health.GetMaxHealth(), LengthUnit.Percent);

                if (characterManager.health.GetCurrentHealth() <= 0)
                {
                    HideBossHud();
                }
            }
        }

        public void ShowBossHud()
        {
            if (bossHud == null)
            {
                return;
            }

            bossHud.enabled = true;
            bossHud.rootVisualElement.Q<Label>("boss-name").text = bossName;

            UIUtils.PlayFadeInAnimation(bossHud.rootVisualElement, .5f);

            bossHud.rootVisualElement.Q<VisualElement>("container").style.marginBottom = characterManager.partnerOrder == 0 ? 0 : 60 * characterManager.partnerOrder;
        }

        public void HideBossHud()
        {
            if (bossHud == null || bossHud?.rootVisualElement == null)
            {
                return;
            }

            UIUtils.PlayFadeOutAnimation(bossHud.rootVisualElement, 2f);
        }

        public bool IsBossHUDEnabled()
        {
            return bossHud != null && bossHud.enabled;
        }

        public void BeginBossBattle()
        {
            if (bossBattleHasBegun)
            {
                return;
            }

            if (characterManager.health.GetCurrentHealth() <= 0)
            {
                return;
            }

            bossBattleHasBegun = true;

            ShowBossHud();

            if (bossMusic != null && GetBGMManager() != null)
            {
                if (GetBGMManager().IsPlayingMusicClip(bossMusic.name) == false)
                {
                    GetBGMManager().PlayMusic(bossMusic);
                    GetSceneSettings().StopPlaylist();
                    GetBGMManager().isPlayingBossMusic = true;
                }
            }

            if (characterManager.partnerOrder == 0)
            {
                // Notify other boss companions that battle has begun
                foreach (CharacterManager partner in characterManager.partners)
                {
                    if (partner.gameObject.activeInHierarchy)
                    {
                        partner.characterBossController.BeginBossBattle();
                    }
                }
            }

            onBattleBegin?.Invoke();

            EventManager.EmitEvent(EventMessages.ON_BOSS_BATTLE_BEGINS);

            if (fogwall != null)
            {
                fogwall.SetActive(true);
            }
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void OnAllBossesDead()
        {
            bool isDead = characterManager.health.GetCurrentHealth() <= 0;

            if (isDead)
            {
                LogAnalytic(AnalyticsUtils.OnBossKilled(bossName));
            }

            bool allPartnersAreDead = isDead && characterManager.partners?.Length > 0
                && characterManager.partners.All(partner => partner.health.GetCurrentHealth() <= 0);

            if (characterManager.partners?.Length > 0 ? allPartnersAreDead : isDead)
            {
                if (fogwall != null)
                {
                    fogwall.SetActive(false);
                }

                if (GetBGMManager() != null)
                {
                    GetBGMManager().isPlayingBossMusic = false;
                }

                GetSceneSettings().HandleSceneSound(true);

                EventManager.EmitEvent(EventMessages.ON_BOSS_BATTLE_ENDS);
                onBossDefeated?.Invoke();
                UpdateBossFlag();
            }
        }

        void UpdateBossFlag()
        {
            if (monoBehaviourID == null || flagsDatabase == null)
            {
                return;
            }

            flagsDatabase.AddFlag(monoBehaviourID.ID, "Boss killed: " + bossName);
        }

        public bool IsBoss()
        {
            return isBoss;
        }

        BGMManager GetBGMManager()
        {
            if (bgmManager == null)
            {
                bgmManager = FindAnyObjectByType<BGMManager>(FindObjectsInactive.Include);
            }

            return bgmManager;
        }

        SceneSettings GetSceneSettings()
        {
            if (_sceneSettings == null)
            {
                _sceneSettings = FindAnyObjectByType<SceneSettings>(FindObjectsInactive.Include);
            }
            return _sceneSettings;
        }

        void LogAnalytic(string eventName)
        {
        }
    }
}
