using Microsoft.Win32;
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
using TagLib;
using System.Windows.Forms;
using System.IO;
using System.Windows.Threading;
using System.Threading;

namespace Osu_Music
{



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        bool Active = false;
        public MainWindow()
        {
            System.Windows.MessageBox.Show("By using this program you agree I, the creator of this program, am not responsible for your actions and or any repercussions caused by use of this program. ");
            InitializeComponent();
            
        }

        private void GoBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DirectoryLbl.Text == "Osu Songs Directory" || OutputDirectoryLbl.Text == "Output Directory")
            {
                System.Windows.MessageBox.Show("Select both directories");
                return;
            }
            if (Active == true)
            {
                Active = false;
                Singleton.CancelProcess();
                GoBtn.Content = "Start!";
                BrowseDirectoryBtn.IsEnabled = true;
                BrowseOutputBtn.IsEnabled = true;
                return;
            }
            else
            {
                Active = true;
                GoBtn.Content = "Cancel";
                BrowseDirectoryBtn.IsEnabled = false;
                BrowseOutputBtn.IsEnabled = false;
            }
            if (ArtworkCheck.IsChecked == false)
            {
                Singleton.Artwork = false;
            }
            else
            {
                Singleton.Artwork = true;
            }
            if (AlbumCombo.SelectedIndex == 1 || AlbumCombo.SelectedIndex == 2)
            {
                Singleton.SingleAlbum = true;
            }
            else
            {
                Singleton.SingleAlbum = false;
            }
            Singleton.Selection = AlbumCombo.SelectedIndex;
            Singleton.beatmapCount = System.IO.Directory.GetDirectories(DirectoryLbl.Text).Length;
            Singleton.originalBeatmapCount = Singleton.beatmapCount;
            try
            {
                Singleton.FilePaths.AddRange(System.IO.Directory.GetDirectories(DirectoryLbl.Text));
            }
            catch
            {
                ErrorLog("Invalid Directory: " + DirectoryLbl.Text);
                return;
            }
            System.Windows.MessageBox.Show(string.Format("*FOR BEST RESULTS MAKE SURE THE FOLDER IS EMPTY EVERY TIME YOU START*\r\n\r\n{0} beatmaps to process... approximate time to complete is {1} minutes. (This is extremely approximate)", Singleton.originalBeatmapCount, (Singleton.originalBeatmapCount * 1.5) / 1000));
            Singleton.OutputDirectory = OutputDirectoryLbl.Text;
            Thread copyThread = new Thread(CopySongs);
            copyThread.IsBackground = true;
            copyThread.Start();
        }

        void CopySongs()
        {
            try
            {
                Parallel.ForEach(Singleton.FilePaths, new ParallelOptions {MaxDegreeOfParallelism = 4},
                    msg =>
                    {
                        search();
                    });
            }
            catch
            {
                ErrorLog("Error launching threads");
            }
        }

        void search()
        {
            List<string> Files = new List<string>();
            char[] mp3 = new char[4] { '.', 'm', 'p', '3' };
            string path;
            try
            {
                path = Singleton.GetPath();
                if (path == null)
                {
                    System.Windows.MessageBox.Show("Done!");
                    return;
                }
            }
            catch
            {
                ErrorLog("Invalid path midway?: " + DirectoryLbl.Text);
                return;
            }
            Files.AddRange(System.IO.Directory.GetFiles(path));
            string Title = null;
            string Artist = null;
            string AudioName = null;
            string AudioPath = null;
            string ImagePath = null;
            string Version = null;
            IPicture[] Image = new IPicture[1];

            if(Files.Count <= 0)
            {
                return;
            }
            foreach (string SubFilePath in Files)
            {
                if (System.IO.Path.GetExtension(SubFilePath) == ".osu")
                {
                    List<string> Lines = System.IO.File.ReadLines(SubFilePath).ToList();
                    foreach (string Line in Lines)
                    {
                        if (Line == null || Line.Length < 6)
                        {
                            continue;
                        }
                        try
                        {
                            if (Line.Substring(0, 14) == "AudioFilename:")
                            {
                                AudioName = Line.Substring(15);
                                AudioPath = path + "\\" + AudioName;
                            }
                        }
                        catch
                        {

                        }
                        try
                        {
                            if (Line.Substring(0, 6) == "Title:")
                            {
                                Title = Line.Substring(6);
                            }
                        }
                        catch
                        {

                        }
                        try
                        {
                            if (Line.Substring(0, 7) == "Artist:")
                            {
                                Artist = Line.Substring(7);
                            }
                        }
                        catch
                        {
                        }
                        try
                        {
                            if (Line.Substring(0, 8) == "Version:")
                            {
                                Version = Line.Substring(8);
                            }
                        }
                        catch
                        {
                        }
                        try
                        {
                            if (Line.Substring(0, 5) == "0,0,\"")
                            {
                                string[] values = Line.Substring(5).Split('"');
                                ImagePath = path + "\\" + values[0];
                                Image[0] = new Picture(ImagePath);
                                break;
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }

            if (Artist != null && Title != null)
            {
                foreach (char c in System.IO.Path.GetInvalidFileNameChars())
                {
                    Artist = Artist.Replace(c, ' ');
                    Title = Title.Replace(c, ' ');
                }
            }
            /*if (System.IO.File.Exists(System.IO.Path.Combine(Singleton.OutputDirectory + "\\", Artist + " - " + Title + ".mp3"))) {
                Title = Title + " (" + AudioName.TrimEnd(mp3) + " " + Version + " " + ")";
                var file = TagLib.File.Create(System.IO.Path.Combine(Singleton.OutputDirectory + "\\", Artist + " - " + Title + ".mp3"));
                if (file.Tag.Comment == AudioName.TrimEnd(mp3)){
                    return;
                }
            }*/
            try
            {
                System.IO.File.Copy(AudioPath, (System.IO.Path.Combine(Singleton.OutputDirectory + "\\", Artist + " - " + Title + ".mp3")), true);
                var file = TagLib.File.Create(System.IO.Path.Combine(Singleton.OutputDirectory + "\\", Artist + " - " + Title + ".mp3"));
                file.Tag.Title = Title;
                file.Tag.Performers = new string[1] { Artist };
                file.Tag.Track = new uint();
                if (Singleton.SingleAlbum == true)
                {
                    file.Tag.Album = "osu! Music Exporter";
                    file.Tag.AlbumArtists = new string[1] { "osu! Music Exporter" };
                }
                else
                {
                    file.Tag.Album = Title;
                }

                file.Tag.Genres = new string[1] { "osu! Music Exporter" };
                file.Tag.Comment = AudioName.TrimEnd(mp3);
                if (Singleton.Artwork == true && Singleton.SingleAlbum == true && Singleton.Selection == 1)
                {
                    try
                    {
                        file.Tag.Pictures = new IPicture[0];
                        
                        var b = new System.Drawing.Bitmap(Properties.Resources.Osu_Music_Logo);
                        b.Save(Singleton.OutputDirectory + "\\_art.png");
                        Image[0] = new Picture(Singleton.OutputDirectory + "\\_art.png");
                        file.Tag.Pictures = Image;
                    }
                    catch
                    {

                    }
                }
                else if (Singleton.Artwork == true)
                {
                    try
                    {
                        file.Tag.Pictures = new IPicture[0];
                        file.Tag.Pictures = Image;
                    }
                    catch
                    {
                        ErrorLog("Error saving artwork:" + AudioPath + file.Name);
                    }
                }
                file.Save();
            }
            catch
            {
                ErrorLog("Error copying file:" + path);
            }
            Singleton.beatmapCount--;
            Singleton.threadCount--;
            if (Singleton.threadCount == 0 && Singleton.cancel != true)
            {
                Thread copyThread = new Thread(CopySongs);
                copyThread.Start();
            }
        }

        private void BrowseDirectoryBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            DirectoryLbl.Text = dialog.SelectedPath.ToString();
        }

        private void BrowseOutputBtn_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            OutputDirectoryLbl.Text = dialog.SelectedPath.ToString();
        }

        private void SupportBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Any support in any way is greatly appreciated! (Even just a nice message on reddit or the like :P)\r\n\r\nThank you for reading!\r\n\r\nReddit: Modricagon");
        }

        private void ErrorLog(string message)
        {
            string path = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.StartupPath) + "\\log" + System.DateTime.Now.Minute + System.DateTime.Now.Millisecond + ".txt";
            System.IO.File.WriteAllText(path, "Error " + message);
        }
    }
}
