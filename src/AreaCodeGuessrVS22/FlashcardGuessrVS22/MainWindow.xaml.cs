using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlashcardGuessrVS22
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string defaultError = "sorry for being a bitch, but some of the files have malformed naming. example of filename: Sweden_0001_kista.png, where the nr needs to be between 0001 to 9999";
        private List<CountryImg> countryImages = null;
        private CountryImg currentCountry = null;    

        public MainWindow()
        {
            InitializeComponent();
        }     

        // untested
        public void NextCountry()
        {
            // decide on which one
            currentCountry = countryImages.Last();
            Image resizedImage = currentCountry.GetSetSizedImage(1920, 1080);


            // ok we have a country, show its image
            stackpanBottom.Children.Clear();            
            stackpanBottom.Children.Add(resizedImage);
        }

        // untested
        public void StartGame()
        {

            // if !settings is system32 then go aghead and start
            if (Properties.Settings.Default.LastOpenedFolder != "C:\\windows\\system32")
            {
                countryImages = LoadCountriesFromFolder(Properties.Settings.Default.LastOpenedFolder);

                if (countryImages != null && countryImages.Any())
                {
                    NextCountry();
                }
                else
                {
                    MessageBox.Show($"The folder {Properties.Settings.Default.LastOpenedFolder} does not fit the mold, " + defaultError);
                }
            }
        }
        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            StartGame();
        }

        public bool ValidateInput(CountryImg countryImg, string inputStr)
        {
            if (countryImg == null || string.IsNullOrEmpty(inputStr)) { return false; }
            return (countryImg.GetCountry().ToLower() == inputStr.ToLower());
        }

        private void _okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentCountry == null) { return; }

            if (ValidateInput(currentCountry, inputCountry.Text))
            {
                MessageBox.Show("Great!");
                countryImages.Remove(currentCountry);   // todo: maybe we want to save these but for now this is easy way to progress the game
                NextCountry();
            }
            else
            {
                MessageBox.Show("Wrong.");
            }
            inputCountry.Text = "";
        }

        // untested
        private List<CountryImg> LoadCountriesFromFolder(string pathToPngFiles)
        {
            List<CountryImg> result = new List<CountryImg>();

            // Get the full path to the png files "fullPath"
            // Break it up and create a Country for each file that matches the regexp pattern 
            try
            {
                List<string> fullPaths =
                    Directory.EnumerateFiles(pathToPngFiles, "*.png", SearchOption.TopDirectoryOnly).ToList();

                foreach (string fullPath in fullPaths)
                {
                    string fileName = System.IO.Path.GetFileName(fullPath);
                    string pattern = @"[a-z]+_[0-9]{4}[_a-zA-Z0-9]*\.(png|PNG)";
                    RegexOptions options = RegexOptions.Singleline;

                    foreach (Match m in Regex.Matches(fileName, pattern, options))
                    {
                        CountryImg countryImg = new CountryImg();


                        Debug.WriteLine("-----");

                        // Create a Country instance 
                        // DOnt get sidetracked, I know you wanna play geoguessr or do something else... OH BOY DO I 
                        // fileName = "Sweden_0001_kista.png"
                        // lets get stuff out of this so split it on the _


                        List<string> thing = fileName.Split('_', '.').ToList();
                        foreach (string str in thing)
                        {
                            Debug.WriteLine($"\tsplit=\"{str}\"");
                        }

                        string countryStr = "";
                        string seqNr = "";
                        string extraInfo = "";

                        switch (thing.Count)
                        {

                            case 3:
                                countryStr = thing[0];
                                seqNr = thing[1];
                                break;
                            case 4:
                                countryStr = thing[0];
                                seqNr = thing[1];
                                extraInfo = thing[2];
                                break;

                        }

                        int nr = -1;
                        if (int.TryParse(seqNr, out nr))
                        {
                            // Nice, a real country
                            if (countryImg.SetValues(fullPath, fileName, countryStr, nr, extraInfo))
                            {
                                result.Add(countryImg);
                            }
                        }
                        else
                        {
                            MessageBox.Show(defaultError);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }

            return result;
        }
        private void browseBtn_Click(object sender, RoutedEventArgs e)
        {
            string fullFolderPath = "";
            var dlg = new FolderPicker();

            dlg.InputPath = Properties.Settings.Default.LastOpenedFolder;

            if (dlg.ShowDialog() == true)
            {
                fullFolderPath = dlg.ResultPath;
            }

            if (fullFolderPath != "")
            {
                Properties.Settings.Default.LastOpenedFolder = fullFolderPath;
                Properties.Settings.Default.Save();                
            }
        }
    }



    public class CountryImg : IEquatable<CountryImg>
    {

        private string fullPathToImage = "";
        private string filename = "";               // Example: "Sweden_0001_Kista"

        private string countryName = "";    // "Sweden"
        private int seqNo = -1;             // 0001
        private string extraInfo = "";      // "Kista"

        private BitmapImage originalBitmap = null;

        public string GetId()
        {
            return filename;
        }

        public string GetCountry()
        {
            return countryName;
        }

        public Image GetSetSizedImage(int widthPx, int heightPx)
        {
            Image croppedImage = new Image();


            // How big is it? Too big?
            croppedImage.Width = 400;
            croppedImage.Margin = new Thickness(2);

            // Create a CroppedBitmap from BitmapImage
            if (originalBitmap.PixelWidth >= 400 && originalBitmap.PixelHeight >= 400)
            {
                var scale = Math.Max(originalBitmap.PixelWidth, originalBitmap.PixelHeight) / 400.0;

                var targetBitmap = new TransformedBitmap((BitmapSource)originalBitmap, new ScaleTransform(1 / scale, 1 / scale));

                croppedImage.Source = targetBitmap;
            }
            else
            {
                croppedImage.Source = originalBitmap;
            }

            return croppedImage;
        }

        
        // returns false on error
        public bool SetValues(string _fullPathToImage, string _filename, string _countryName, int _seqNo, string _extraInfo)
        {
            fullPathToImage = _fullPathToImage;
            filename = _filename;
            countryName = _countryName;
            seqNo = _seqNo;
            extraInfo = _extraInfo;

            // sanitychecks
            if (fullPathToImage == "") { return false; }

            // Load image
            originalBitmap = new BitmapImage();
            originalBitmap.BeginInit();
            originalBitmap.UriSource = new Uri(fullPathToImage);
            originalBitmap.EndInit();   

            return true;
        }


        public bool CompareCountry(CountryImg other)
        {
            return (other.GetId() == this.GetId());
        }

        public bool Equals(CountryImg? other)
        {
            if (other == null) return false;
            return other.countryName == this.countryName;
        }
    }
}