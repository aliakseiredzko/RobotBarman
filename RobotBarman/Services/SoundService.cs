using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using FreshMvvm;
using Plugin.SimpleAudioPlayer;
using RobotBarman.Models;
using RobotBarman.Services.Interfaces;

namespace RobotBarman.Services
{
    public class SoundService: ISoundService
    {
        private readonly List<Sound> _sounds;
        private readonly List<Sound> _openingSounds;
        private readonly List<Sound> _beforeSpillSounds;
        private readonly List<Sound> _afterSpillSounds;
        private readonly List<Sound> _changeBottleSounds;
        private readonly IJsonDatabaseService _databaseService;
        private static readonly Random _random;

        static SoundService()
        {
            _random = new Random();
        }

        public SoundService()
        {
            _sounds = new List<Sound>();
            _databaseService = FreshIOC.Container.Resolve<IJsonDatabaseService>();

            _sounds = _databaseService.GetSoundInfo();
            _openingSounds = _sounds.Where(x => x.Path.Contains("Opening")).ToList();
            _beforeSpillSounds = _sounds.Where(x => x.Path.Contains("BeforeSpill")).ToList();
            _afterSpillSounds = _sounds.Where(x => x.Path.Contains("AfterSpill")).ToList();
            _changeBottleSounds = _sounds.Where(x => x.Path.Contains("ChangeBottle")).ToList();
        }

        private void PlayAudio(Sound sound)
        {
            if (CrossSimpleAudioPlayer.Current.Load(sound.SoundStream))
            {
                CrossSimpleAudioPlayer.Current.Loop = false;
                CrossSimpleAudioPlayer.Current.Play();
            }
            else Debug.WriteLine($"Can't play {sound.Path}");
        }

        public void PlayOpeningSound()
        {
            PlayAudio(_openingSounds[_random.Next(0, _openingSounds.Count)]);
        }

        public void PlayBeforeSpillSound()
        {
            PlayAudio(_beforeSpillSounds[_random.Next(0, _beforeSpillSounds.Count)]);
        }

        public void PlayAfterSpillSound()
        {
            PlayAudio(_afterSpillSounds[_random.Next(0, _afterSpillSounds.Count)]);
        }

        public void PlayChangeBottleSound(DrinkPosition position)
        {
            PlayAudio(_changeBottleSounds[(int)position]);
        }
    }
}
