using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace World.Controller
{
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
        public float soundVolume = 1;
        public float musicVolume = 1;
        [Header("战斗音效池")]
        public AudioSource sfxSourcePrefab; // 预制体（建议禁用Play On Awake）
        private  List<AudioSource> sfxPool = new List<AudioSource>();
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
             bgSource.volume = musicVolume;
           //  buttonSource.volume = soundVolume;
            // InitSfxPool();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = volume;
            bgSource.volume = musicVolume;
        }
        
        public void SetSoundVolume(float volume)
        {
            soundVolume = volume;
           // buttonSource.volume = soundVolume;
        }
        
        
        #region === 战斗/角色音效 ===
        private void InitSfxPool()
        {
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
            foreach (var src in sfxPool)
            {
                if (!src.isPlaying)
                    return src;
            }
            return sfxPool[0]; // 若都在播放则复用第一个
        }

        /// <summary>
        /// 播放短音效（攻击、受击、死亡等）
        /// </summary>
        public void PlaySfx(AudioClip clip)
        {
            if (clip == null) return;
            var src = GetFreeSfxSource();

            src.volume = soundVolume;
            src.PlayOneShot(clip);
        }
        #endregion
        
    }
}
