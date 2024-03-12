using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using System.Windows.Threading;

namespace AudioPlayer
{
    public partial class MainWindow : Window
    {
        private List<string> playlist = new List<string>();
        private int currentSongIndex = 0;
        private bool repeatMode = false;
        private bool shuffleMode = false;

        private Thread positionThread;
        private Thread timeThread;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string[] files = Directory.GetFiles(Path.GetDirectoryName(openFileDialog.FileName));
                foreach (string file in files)
                {
                    if (file.EndsWith(".mp3") || file.EndsWith(".m4a") || file.EndsWith(".wav"))
                    {
                        playlist.Add(file);
                    }
                }
            }

            PlaySong();
        }

        private void PlaySong()
        {
            if (playlist.Count > 0)
            {
                mediaElement.Source = new Uri(playlist[currentSongIndex]);
                mediaElement.Play();

                positionThread = new Thread(new ThreadStart(UpdatePosition));
                positionThread.Start();

                timeThread = new Thread(new ThreadStart(UpdateTime));
                timeThread.Start();
            }
        }

        private void UpdatePosition()
        {
            while (true)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    if (mediaElement.NaturalDuration.HasTimeSpan)
                    {
                        sliderPosition.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                        sliderPosition.Value = mediaElement.Position.TotalSeconds;
                    }
                }));
                Thread.Sleep(100);
            }
        }

        private void UpdateTime()
        {
            while (true)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate ()
                {
                    if (mediaElement.NaturalDuration.HasTimeSpan)
                    {
                        txtTime.Text = $"{mediaElement.Position.Minutes:00}:{mediaElement.Position.Seconds:00} / {mediaElement.NaturalDuration.TimeSpan.Minutes:00}:{mediaElement.NaturalDuration.TimeSpan.Seconds:00}";
                    }
                }));
                Thread.Sleep(100);
            }
        }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (mediaElement != null)
            {
                if (mediaElement.Source != null)
                {
                    if (mediaElement.CanPause)
                    {
                        if (mediaElement.IsPlaying)
                        {
                            mediaElement.Pause();
                        }
                        else
                        {
                            mediaElement.Play();
                        }
                    }
                    else
                    {
                        PlaySong();
                    }
                }
            }
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            if (shuffleMode)
            {
                currentSongIndex = new Random().Next(0, playlist.Count);
            }
            else
            {
                currentSongIndex++;
                if (currentSongIndex >= playlist.Count)
                {
                    currentSongIndex = 0;
                }
            }
            PlaySong();
        }

        private void BtnPrevious_Click(object sender, RoutedEventArgs e)
        {
            currentSongIndex--;
            if (currentSongIndex < 0)
            {
                currentSongIndex = playlist.Count - 1;
            }

            PlaySong();
        }

        private void BtnRepeat_Click(object sender, RoutedEventArgs e)
        {
            repeatMode = !repeatMode;
        }

        private void BtnShuffle_Click(object sender, RoutedEventArgs e)
        {
            shuffleMode = !shuffleMode;
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (repeatMode)
            {
                mediaElement.Play();
            }
            else
            {
                BtnNext_Click(sender, e);
            }
        }
    }
}