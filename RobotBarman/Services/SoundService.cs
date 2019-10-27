using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using FreshMvvm;
using Plugin.SimpleAudioPlayer;
using RobotBarman.Models;
using RobotBarman.Services.Interfaces;

namespace RobotBarman.Services
{
    public class SoundService: ISoundService
    {
        private List<Sound> _sounds;
        private List<Sound> _openingSounds;
        private List<Sound> _beforeSpillSounds;
        private List<Sound> _afterSpillSounds;
        private List<Sound> _changeBottleSounds;
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
        }

        public async Task InitDataAsync()
        {
            if (_sounds == null)
            {
                _sounds = await _databaseService.GetSoundInfoAsync();
                _openingSounds = await GetAsListAsync("Opening");
                _beforeSpillSounds = await GetAsListAsync("BeforeSpill");
                _afterSpillSounds = await GetAsListAsync("BeforeSpill");
                _changeBottleSounds = await GetAsListAsync("ChangeBottle");
            }
        }

        private async Task<List<Sound>> GetAsListAsync(string data)
        {
            return await Task.FromResult(_sounds.Where(x => x.Path.Contains(data)).ToList());
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
