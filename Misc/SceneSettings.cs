﻿using UnityEngine;
using System.Collections;
using AF.Music;
using TigerForge;
using AF.Events;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using DG.Tweening;

namespace AF
{
    public class SceneSettings : MonoBehaviour
    {
        [Header("Components")]
        public BGMManager bgmManager;
        public CursorManager cursorManager;
        public PlayerManager playerManager;
        public TeleportManager teleportManager;

        [Header("Music")]
        public AudioClip dayMusic;
        public AudioClip nightMusic;

        public AudioClip dayAmbience;
        public AudioClip nightAmbience;

        [Header("Scene Musics")]
        public AudioClip[] playlist;
        Coroutine ChooseNextSongCoroutine;
        bool isPlayingMusicFromThePlaylist = false;

        [Header("Map")]
        public bool isInterior;
        public bool displaySceneName = true;
        public float displaySceneNameDelay = 3f;
        public float displaySceneNameDuration = 3f;
        public Location location;
        public UIDocument sceneNameDocument;
        public AudioClip sceneNameSfx;

        [Header("Tutorial")]
        public DestroyableParticle respawnFx;

        [Header("Systems")]
        public GameSession gameSession;
        public GameSettings gameSettings;
        public PickupDatabase pickupDatabase;

        [Header("Events")]
        public UnityEvent onSceneStart;

        void Awake()
        {
            sceneNameDocument.rootVisualElement.contentContainer.style.opacity = 0;

            onSceneStart?.Invoke();
        }

        private void Start()
        {
            if (displaySceneName)
            {
                StartCoroutine(DisplaySceneName_Coroutine(() =>
                {
                    HandleSceneSound(true);
                }));
            }
            else
            {
                HandleSceneSound(true);
            }

            OnHourChanged(!displaySceneName);

            EventManager.StartListening(EventMessages.ON_HOUR_CHANGED, () => OnHourChanged(true));
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        public void DisplaySceneName()
        {
            StartCoroutine(DisplaySceneName_Coroutine(() =>
            {
                HandleSceneSound(true);
            }));
        }

        IEnumerator DisplaySceneName_Coroutine(UnityAction onFinish)
        {
            yield return new WaitForSeconds(displaySceneNameDelay);

            sceneNameDocument.rootVisualElement.Q<Label>().text = location.GetLocationDisplayName();

            DOTween.To(
                  () => sceneNameDocument.rootVisualElement.contentContainer.style.opacity.value,
                  (value) => sceneNameDocument.rootVisualElement.contentContainer.style.opacity = value,
                  1,
                  1f
            );

            if (sceneNameSfx != null)
            {
                bgmManager.PlaySound(sceneNameSfx, null);
                yield return new WaitForSeconds(sceneNameSfx.length);
            }

            yield return new WaitForSeconds(displaySceneNameDuration);

            DOTween.To(
                  () => sceneNameDocument.rootVisualElement.contentContainer.style.opacity.value,
                  (value) => sceneNameDocument.rootVisualElement.contentContainer.style.opacity = value,
                  0,
                  1f
            );

            onFinish?.Invoke();
        }

        public IEnumerator DisplaySceneNameCoroutine(string sceneName)
        {
            {
                sceneNameDocument.rootVisualElement.Q<Label>().text = sceneName;

                DOTween.To(
                      () => sceneNameDocument.rootVisualElement.contentContainer.style.opacity.value,
                      (value) => sceneNameDocument.rootVisualElement.contentContainer.style.opacity = value,
                      1,
                      1f
                );

                yield return new WaitForSeconds(displaySceneNameDuration);

                DOTween.To(
                      () => sceneNameDocument.rootVisualElement.contentContainer.style.opacity.value,
                      (value) => sceneNameDocument.rootVisualElement.contentContainer.style.opacity = value,
                      0,
                      1f
                );

            }
        }

        /// <summary>
        /// Unity Event
        /// </summary>
        /// <param name="audioClip"></param>
        public void SetDayMusic(AudioClip audioClip)
        {
            this.dayMusic = audioClip;
        }

        public void HandleSceneSound(bool evaluateMusic)
        {
            if (evaluateMusic)
            {
                EvaluateMusic();
            }

            EvaluateAmbience();
        }

        public void StopPlaylist()
        {
            isPlayingMusicFromThePlaylist = false;
        }

        void EvaluatePlaylist()
        {
            if (isPlayingMusicFromThePlaylist)
            {
                return;
            }

            bgmManager.StopMusic();

            AudioClip chosenAudioClip = playlist[Random.Range(0, playlist.Length)];
            bgmManager.PlayMusic(chosenAudioClip);
            isPlayingMusicFromThePlaylist = true;

            if (ChooseNextSongCoroutine != null)
            {
                StopCoroutine(ChooseNextSongCoroutine);
            }

            ChooseNextSongCoroutine = StartCoroutine(ChooseNextSong_Coroutine(chosenAudioClip.length));
        }

        IEnumerator ChooseNextSong_Coroutine(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);

            bgmManager.StopMusic();

            yield return new WaitForSeconds(5f);
            isPlayingMusicFromThePlaylist = false;

            EvaluatePlaylist();
        }

        /// <summary>
        /// Evaluate and control the music based on time of day.
        /// </summary>
        void EvaluateMusic()
        {
            if (bgmManager.isPlayingBossMusic)
            {
                return;
            }

            if (playlist != null && playlist.Length > 0)
            {
                EvaluatePlaylist();
                return;
            }

            if (dayMusic == null && nightMusic == null)
            {
                // Stop the music playback if there are no available tracks.
                bgmManager.StopMusic();
                return;
            }

            if (dayMusic != null && CanPlayDaySfx(dayMusic))
            {
                if (IsPlayingSameMusicTrack(dayMusic.name) == false)
                {
                    bgmManager.PlayMusic(dayMusic);
                }
            }
            else if (nightMusic != null && CanPlayNightSfx(nightMusic))
            {
                if (IsPlayingSameMusicTrack(nightMusic.name) == false)
                {
                    bgmManager.PlayMusic(nightMusic);
                }
            }
        }

        void EvaluateAmbience()
        {
            if (nightAmbience == null && dayAmbience == null)
            {
                bgmManager.StopAmbience();
                return;
            }

            if (dayAmbience != null && CanPlayDaySfx(dayAmbience))
            {
                if (IsPlayingSameAmbienceTrack(dayAmbience.name) == false)
                {
                    bgmManager.PlayAmbience(dayAmbience);
                }
            }
            else if (nightAmbience != null && CanPlayNightSfx(nightAmbience))
            {
                if (IsPlayingSameAmbienceTrack(nightAmbience.name) == false)
                {
                    bgmManager.PlayAmbience(nightAmbience);
                }
            }
        }

        bool IsPlayingSameMusicTrack(string musicClipName)
        {
            return bgmManager.bgmAudioSource.clip != null && bgmManager.bgmAudioSource.clip.name == musicClipName;
        }

        bool IsPlayingSameAmbienceTrack(string musicClipName)
        {
            return bgmManager.ambienceAudioSource.clip != null && bgmManager.ambienceAudioSource.clip.name == musicClipName;
        }

        bool IsNightTime()
        {
            return gameSession.IsNightTime();
        }

        bool CanPlayNightSfx(AudioClip clip)
        {
            return IsNightTime() && clip != null;
        }

        bool CanPlayDaySfx(AudioClip clip)
        {
            return !IsNightTime() && clip != null;
        }

        public void OnHourChanged(bool handleMusic)
        {
            HandleSceneSound(handleMusic);

            pickupDatabase.OnHourChangedCheckForReplenishablesToClear();
        }
    }
}
