using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Content;

namespace WorldNMilSim.Rendering;

// Loads and plays music/SFX. Loads are defensive - if an asset isn't present, we skip it
// rather than crashing.
public class AudioManager
{
    private readonly ContentManager _content;
    private readonly Random _random = new();

    private readonly Song[] _musicTracks;
    private readonly SoundEffect[] _chatterClips;
    private readonly SoundEffect _defcon1Sound;
    private readonly SoundEffect _sonarSound;

    private int _currentTrackIndex = -1;
    private double _nextChatterInSeconds;
    private bool _defcon1Played;

    public AudioManager(ContentManager content)
    {
        _content = content;

        _musicTracks = new[]
        {
            TryLoad<Song>("Audio/Track1"),
            TryLoad<Song>("Audio/Track2"),
            TryLoad<Song>("Audio/Track3"),
            TryLoad<Song>("Audio/Track4"),
        };

        _chatterClips = new[]
        {
            TryLoad<SoundEffect>("Audio/Chatter1"),
            TryLoad<SoundEffect>("Audio/Chatter2"),
            TryLoad<SoundEffect>("Audio/Chatter3"),
            TryLoad<SoundEffect>("Audio/Chatter4"),
            TryLoad<SoundEffect>("Audio/Chatter5"),
            TryLoad<SoundEffect>("Audio/Chatter6"),
            TryLoad<SoundEffect>("Audio/Chatter7"),
            TryLoad<SoundEffect>("Audio/Chatter8"),
            TryLoad<SoundEffect>("Audio/Chatter9"),
        };

        _defcon1Sound = TryLoad<SoundEffect>("Audio/DEFCON1");
        _sonarSound = TryLoad<SoundEffect>("Audio/Sonar");

        MediaPlayer.IsRepeating = false; // we cycle tracks ourselves so we can shuffle
        MediaPlayer.Volume = 0.2f;       // music sits quietly under everything else
        SoundEffect.MasterVolume = 0.8f;

        ScheduleNextChatter();
    }

    private T TryLoad<T>(string assetName)
    {
        try
        {
            return _content.Load<T>(assetName);
        }
        catch (ContentLoadException)
        {
            return default;
        }
    }

    public void Update(double dtSeconds)
    {
        if (MediaPlayer.State != MediaState.Playing)
            PlayNextTrack();

        _nextChatterInSeconds -= dtSeconds;
        if (_nextChatterInSeconds <= 0)
        {
            PlayRandomChatter();
            ScheduleNextChatter();
        }
    }

    private void ScheduleNextChatter()
    {
        _nextChatterInSeconds = 25 + _random.NextDouble() * 35; // next line somewhere 25-60s out
    }

    private void PlayNextTrack()
    {
        var validTracks = Array.FindAll(_musicTracks, t => t != null);
        if (validTracks.Length == 0) return;

        int next = _random.Next(validTracks.Length);
        if (validTracks.Length > 1)
        {
            while (next == _currentTrackIndex)
                next = _random.Next(validTracks.Length);
        }
        _currentTrackIndex = next;
        MediaPlayer.Play(validTracks[next]);
    }

    private void PlayRandomChatter()
    {
        var validClips = Array.FindAll(_chatterClips, c => c != null);
        if (validClips.Length == 0) return;

        validClips[_random.Next(validClips.Length)].Play();
    }

    public void PlayDefcon1Alert()
    {
        if (_defcon1Played) return;
        _defcon1Played = true;
        _defcon1Sound?.Play();
    }

    public void PlaySonarPing() => _sonarSound?.Play();
}