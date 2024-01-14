using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Media;
using System.IO;

namespace Music_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MediaPlayer mp = new MediaPlayer();
        private List<Uri> songsToPlay = new List<Uri>();

        public MainWindow()
        {
            // First initializations
            InitializeComponent();
            InitSongsToPlay();

            // Initialize settings and play first random song
            mp.Open(NextRandomSong());
            songButton.Text = songTitle.Text;
            mp.Volume = 0.02;
            volumeText.Text = "Volume: " + (mp.Volume * 100) + "%";
            volumeSlider.Value = 0.2;
            durationText.Text = "";

            // Thread to constantly update the time of the song currently playing
            Thread handleDuration = new Thread(() =>
            {
                while (true)
                {
                    Dispatcher.Invoke(() =>
                    {
                        string duration = mp.Position.Minutes + ":";
                        if (mp.Position.Seconds < 10)
                        {
                            duration += "0";
                        }
                        duration += mp.Position.Seconds;

                        durationText.Text = duration;
                    });
                    Thread.Sleep(30);
                }
            });
            handleDuration.Start();

            // Call the Mp.MediaEnded function for the Mp_MediaEnded event
            mp.MediaEnded += Mp_MediaEnded;

            // Late initializations
            InitPlaylists();
            InitSongs();
        }

        // Initiate playlistMenu variable
        private void InitPlaylists ()
        {
            string path = Directory.GetCurrentDirectory() + @"../../../Music/";
            string[] files = Directory.GetDirectories(path);

            for (int i = 0; i < files.Length; i++)
            {
                MenuItem item = new MenuItem();
                string name = files[i].Substring(path.Length);
                item.Header = name;
                item.Width = 178;
                item.Click += new RoutedEventHandler(playlistMenu_Click);

                playlistMenu.Items.Add(item);
            }
        }

        // A function to clear the songs variable
        private void ClearSongs ()
        {
            songMenu.Items.Clear();
        }

        // Initiate songMenu variable
        private void InitSongs ()
        {
            string path = Directory.GetCurrentDirectory() + @"../../../Music/" + playlistButton.Text;
            string[] files = Directory.GetFiles(path);

            for (int i = 0; i < files.Length; i++)
            {
                MenuItem item = new MenuItem();
                string name = files[i].Substring(path.Length + 1, files[i].Length - path.Length - 1 - 4);
                item.Header = name;
                item.Width = 178;
                item.Click += new RoutedEventHandler(songMenu_Click);

                songMenu.Items.Add(item);
            }
        }

        // Initiate songsToPlay variable
        private void InitSongsToPlay ()
        {
            songsToPlay.Clear();

            string path = Directory.GetCurrentDirectory() + @"../../../Music/" + playlistButton.Text;

            for (int i = 0; i < Directory.GetFiles(path).Length; i++)
            {
                songsToPlay.Add(new Uri(Directory.GetFiles(path)[i]));
            }
        }

        // Handle new song if a song is ended
        private void Mp_MediaEnded(object sender, EventArgs e)
        {
            // Play new random song (that has not been played in current playlist run yet)
            mp.Close();
            Uri next = NextRandomSong();
            mp.Open(next);
            mp.Play();

            // Update volume
            double vol = Math.Round(volumeSlider.Value * 10) * 1 / 100;
            mp.Volume = vol;

            // Update text
            songButton.Text = songTitle.Text;
        }

        // Return a random song to play
        private Uri NextRandomSong () // NEEDS ORGANIZING AND FIXING !!!
        {
            // Random instance
            Random rnd = new Random();

            // Check for songs in playlist
            if (songsToPlay.Count == 0)
                InitSongsToPlay();

            // Find path
            string path = Directory.GetCurrentDirectory() + @"../../../Music/" + playlistButton.Text;
            int index = rnd.Next(0, songsToPlay.Count - 1);
            Uri fileUri = songsToPlay[index];
            songsToPlay.Remove(songsToPlay[index]);
            songTitle.Text = GetSongName(fileUri.AbsolutePath);

            // Return new random song
            return fileUri;
        }

        private string GetSongName (string path)
        {
            // Clean path
            path = path.Replace("%20", " ");

            // Look for the beginning of the song's name (remove path before name)
            for (int i = 0; i < path.Length - 5; i++)
            {
                if (path[i] == '/')
                {
                    string music = "";

                    // Build the word following the '/' and save it to music variable
                    for (int j = 1; j <= 6; j++)
                    {
                        music += path[i + j];
                    }

                    // If music directory is found
                    if (music == "Music/")
                    {
                        // Return a substring of the name without the path to it
                        string name = path.Substring(i + 8 + playlistButton.Text.Length);
                        // Remove file type text (.mp3)
                        name = name.Substring(0, name.Length - 4);
                        // Return song's name
                        return name;
                    }
                }
            }

            // This should never be reached but it's a default return value
            // Since it should never be reached null is returned to symbolize
            // It is not part of the code
            return null;
        }

        // Return a chosen song to play
        private Uri NextSong()
        {
            // Find path
            string path = Directory.GetCurrentDirectory() + @"../../../Music/" + playlistButton.Text;
            // Save selected song's path
            string songPath = Directory.GetCurrentDirectory() + @"../../../Music/" + playlistButton.Text + "/" + songButton.Text + ".mp3";

            // Set song title to current song
            songTitle.Text = songPath.Substring(path.Length + 1, Math.Min(songPath.Length - path.Length - 5, 15));
            // Return the Uri of the song's path
            return new Uri(songPath);
        }

        // Pause/Play button
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Handle pausing and playing of the songs (text and actual song playing)
            if (playButton.Content.ToString() == "Play")
            {
                // Play song
                playButton.Content = "Pause";
                mp.Play();
            }
            else
            {
                // Pause song
                playButton.Content = "Play";
                mp.Pause();
            }
        }

        // Handle volume
        private void volumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Handle volume (slider)
            // Save volume from slider
            double vol = Math.Round(volumeSlider.Value * 10) / 100;
            // Set new volume
            mp.Volume = vol;
            // Update volume text
            volumeText.Text = ("Volume: " + (vol * 100) + "%");
        }

        // Skip button
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Switch to selected song
            mp.Close();
            if (songButton.Text != songTitle.Text) // If selected song is already playing, switch to a random song
                mp.Open(NextSong());
            else
                mp.Open(NextRandomSong());
            mp.Play();

            songButton.Text = songTitle.Text;

            // Update Play/Pause button text
            if (playButton.Content.ToString() == "Play")
            {
                playButton.Content = "Stop";
            }

            // Update volume
            double vol = Math.Round(volumeSlider.Value * 10) * 1 / 100;
            mp.Volume = vol;
        }

        // Handle playlist button
        private void playlistMenu_Click(object sender, RoutedEventArgs e)
        {
            // Switch playlist and update songs
            MenuItem clickedItem = (MenuItem)e.Source;
            playlistButton.Text = clickedItem.Header.ToString();
            ClearSongs();
            InitSongs();
            songsToPlay.Clear();
            InitSongsToPlay();
        }

        // Handle song button
        private void songMenu_Click(object sender, RoutedEventArgs e)
        {
            // Update song button text
            MenuItem clickedItem = (MenuItem) e.Source;

            songButton.Text = clickedItem.Header.ToString();
        }
    }
}