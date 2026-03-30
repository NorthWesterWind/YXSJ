using System;
using System.Collections.Generic;
using Module.Data;
using UnityEngine;
using Utils;

namespace World.Controller
{
    [Serializable]
    public class MonsterHitSfxConfig
    {
        public MonsterType monsterType = MonsterType.None;
        public AudioClip[] clips;
    }

    public enum E_ClipType
    {
        None,
        InGame,
        OutGame
    }
    public class AudioSourceController : MonoSingleton<AudioSourceController>
    {
        public E_ClipType currentClipType =  E_ClipType.None;
        public AudioSource bgSource;
        public AudioSource buttonSource;    // 按钮/提示音效播放器
        public AudioClip  inGameAudioClip;
        public AudioClip buttonClip;
        [Header("默认怪物受击音效")]
        public AudioClip[] monsterHitClips;
        [Header("按怪物类型覆盖受击音效")]
        public List<MonsterHitSfxConfig> monsterHitSfxConfigs = new List<MonsterHitSfxConfig>();
        public float soundVolume = 1;
        public float musicVolume = 1;
        [Header("战斗音效池")]
        public AudioSource sfxSourcePrefab; // 预制体（建议禁用Play On Awake）
        private  List<AudioSource> sfxPool = new List<AudioSource>();
        private AudioSource runtimeSfxSource;
        [Range(1, 30)] public int sfxPoolSize = 15;
        [Range(0, 1)] public float sfxVolume = 1f;
        
        public void PlaySound()
        {
            bgSource.clip = inGameAudioClip;
            // 设置循环
            bgSource.loop = true;
            // 播放
            bgSource.Play();
        }
        
        public void StopSound()
        {
            bgSource.Stop();
        }


        private void Update()
        {
           
        }

        public void PlayUISound()
        {
          //  buttonSource.PlayOneShot(buttonClip, soundVolume);
        }

        private void Start()
        {
            if (bgSource != null)
            {
                bgSource.volume = musicVolume;
            }
           //  buttonSource.volume = soundVolume;
            EnsureSfxPoolInitialized();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = volume;
            if (bgSource != null)
            {
                bgSource.volume = musicVolume;
            }
        }
        
        public void SetSoundVolume(float volume)
        {
            soundVolume = volume;
           // buttonSource.volume = soundVolume;
        }
        
        
        #region === 战斗/角色音效 ===
        private void InitSfxPool()
        {
            if (sfxSourcePrefab == null)
            {
                return;
            }

            for (int i = 0; i < sfxPoolSize; i++)
            {
                var src = Instantiate(sfxSourcePrefab, transform);
                src.spatialBlend = 0; // 纯2D
                src.playOnAwake = false;
                src.loop = false;
                sfxPool.Add(src);
            }
        }

        private AudioSource GetFreeSfxSource()
        {
            if (sfxPool.Count == 0)
            {
                return null;
            }

            foreach (var src in sfxPool)
            {
                if (!src.isPlaying)
                    return src;
            }
            return sfxPool[0]; // 若都在播放则复用第一个
        }

        private void EnsureSfxPoolInitialized()
        {
            if (sfxPool.Count > 0 || sfxSourcePrefab == null)
            {
                return;
            }

            InitSfxPool();
        }

        private AudioSource GetFallbackSfxSource()
        {
            if (buttonSource != null && buttonSource != bgSource)
            {
                return buttonSource;
            }

            if (runtimeSfxSource == null)
            {
                var runtimeSfxObject = new GameObject("RuntimeSfxSource");
                runtimeSfxObject.transform.SetParent(transform, false);
                runtimeSfxSource = runtimeSfxObject.AddComponent<AudioSource>();
                runtimeSfxSource.playOnAwake = false;
                runtimeSfxSource.loop = false;
                runtimeSfxSource.spatialBlend = 0f;
            }

            return runtimeSfxSource;
        }

        /// <summary>
        /// 播放短音效（攻击、受击、死亡等）
        /// </summary>
        public void PlaySfx(AudioClip clip)
        {
            if (clip == null) return;
            EnsureSfxPoolInitialized();
            var src = GetFreeSfxSource();
            if (src == null)
            {
                src = GetFallbackSfxSource();
                if (src == null)
                {
                    return;
                }

                src.PlayOneShot(clip, soundVolume * sfxVolume);
                return;
            }

            src.volume = soundVolume * sfxVolume;
            src.PlayOneShot(clip);
        }

        public void PlayMonsterHitSfx(MonsterType monsterType = MonsterType.None)
        {
            var clip = GetMonsterHitClip(monsterType);
            if (clip == null)
            {
                return;
            }

            PlaySfx(clip);
        }

        private AudioClip GetMonsterHitClip(MonsterType monsterType)
        {
            if (monsterHitSfxConfigs != null)
            {
                for (int i = 0; i < monsterHitSfxConfigs.Count; i++)
                {
                    var config = monsterHitSfxConfigs[i];
                    if (config == null || config.monsterType != monsterType)
                    {
                        continue;
                    }

                    var typedClip = GetRandomClip(config.clips);
                    if (typedClip != null)
                    {
                        return typedClip;
                    }
                }
            }

            return GetRandomClip(monsterHitClips);
        }

        private AudioClip GetRandomClip(IReadOnlyList<AudioClip> clips)
        {
            if (clips == null || clips.Count == 0)
            {
                return null;
            }

            return clips[UnityEngine.Random.Range(0, clips.Count)];
        }
        #endregion
        
    }
}
