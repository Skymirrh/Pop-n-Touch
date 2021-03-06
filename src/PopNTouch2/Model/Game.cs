﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;

namespace PopNTouch2.Model
{
    /// <summary>
    /// Class instanciated by GameMaster, represents an ongoing Game. Handles changing game state and music playing.
    /// </summary>
    public class Game
    {
        public Song Song { get; set; }
        public Boolean IsPlaying { get; set; }
        public CloneableStopwatch GameStopwatch { get; set; }
        public AudioController MusicPlayback { get; set; }

        public Game(Song s)
        {
            this.Song = s;
        }

        /// <summary>
        /// Launch the game 
        /// Launch the sheet reading by players
        /// Launch the time elapsed counter
        /// </summary>
        public void Launch()
        {
            this.IsPlaying = true;
            foreach (Player player in GameMaster.Instance.Players)
            {
                player.ResetScores();
                player.ReadSheet();
            }
            this.MusicPlayback = new AudioController(Song.File, GameMaster.TIMEBEFORERESUME);
            this.MusicPlayback.MediaEnded += new EventHandler(AudioFinished);
        }

        /// <summary>
        /// Pause the sound and the timers associated
        /// </summary>
        public void Pause()
        {
            this.MusicPlayback.Pause();
            this.GameStopwatch.Stop();
        }

        /// <summary>
        /// Resume the sound and the timers associated after a certain period of time
        /// </summary>
        public void Resume()
        {
            this.MusicPlayback.Dispatcher.Invoke((Action)(() => { this.MusicPlayback.Play(); }));
            this.GameStopwatch.Start();
        }

        /// <summary>
        /// Add a player in the middle of a game
        /// Can only be done when the game is paused
        /// </summary>
        /// <param name="player">The player to add</param>
        public void AddPlayerInGame(Player player)
        {
            this.GameStopwatch.Stop();
            long time = this.GameStopwatch.ElapsedMilliseconds;

            List<Tuple<double, double, Note>>.Enumerator enumerator = player.SheetMusic.Notes.GetEnumerator();
            double noteTime = 0;
            while(time > noteTime)
            {
                enumerator.MoveNext();
                noteTime = enumerator.Current.Item1;
            }
            player.ReadSheet(true, enumerator);
            player.Pause();
        }

        /// <summary>
        /// Do what to do when the song is finished
        /// </summary>
        public void AudioFinished(object sender, EventArgs e)
        {
            this.IsPlaying = false;
            this.MusicPlayback.Close();
            this.GameStopwatch.Reset();
        }
    }
}
