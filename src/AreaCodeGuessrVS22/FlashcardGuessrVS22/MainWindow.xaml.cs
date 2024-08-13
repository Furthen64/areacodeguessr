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
using static System.Net.WebRequestMethods;

namespace FlashcardGuessrVS22
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string defaultError = "sorry for being a bitch, but some of the files have malformed naming. example of filename: Sweden_0001_kista.png, where the nr needs to be between 0001 to 9999";
        private List<CountryImg> countryImages;
        private int amountAtStart = 0;
        private CountryImg currentCountry;
        private Random rand;
        private double userScale = 0.5;

        private int canvasWidth = 0;
        private int canvasHeight = 0;

        private string fileFormat = "SyntaxCountry";
        private GameModes gameMode = GameModes.RapidFire;
        public enum GameModes { Standard, RapidFire };

        private Score score;

        public MainWindow()
        {
            InitializeComponent();            
            rand = new Random(DateTime.UtcNow.Millisecond);
            this.SizeChanged += OnWindowSizeChanged;
            try
            {
                CurrentPath.Content = Properties.Settings.Default.LastOpenedFolder.ToString();
            } 
            catch(Exception )
            {

            }
        }

        private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
        {

            canvasWidth = (int)Math.Round(e.NewSize.Height * 0.75); // 25% of the surface is for the buttons and stuff
            canvasHeight = (int)Math.Round(e.NewSize.Width * 0.75);
            //double prevWindowHeight = e.PreviousSize.Height;
            //double prevWindowWidth = e.PreviousSize.Width;
        }     

        public void NextCountry()
        {
            if(countryImages.Count() == 0)
            {
                MessageBox.Show("GG CHAMP, you done them all!!");
                // Show the gif of the guy giving thumbs up
                return;
            }
            int randIdx = rand.Next(0, countryImages.Count-1);
            
            currentCountry = countryImages[randIdx];
            
            double imgToWindowFactor = 1.0;
            int imgHeight = currentCountry.getRawImage().PixelHeight;
            int imgWidth = currentCountry.getRawImage().PixelWidth;
            
            
            if (imgHeight > canvasHeight)
            {
                imgToWindowFactor = (double)imgHeight / canvasHeight;   
            }

            if (imgWidth > canvasWidth)
            {
                imgToWindowFactor = (double)imgWidth / canvasWidth;
            }

            if (imgToWindowFactor < 0.1)
            {
                imgToWindowFactor = 0.1;
            }
            double newWidthDbl = userScale * imgWidth / imgToWindowFactor;
            double newHeightDbl = userScale *imgHeight / imgToWindowFactor;
            if (newWidthDbl < 500)
            {
                newWidthDbl = 500;
            }
            
            int newWidth = (int)Math.Round(newWidthDbl);
            int newHeight = (int)Math.Round(newHeightDbl);
            Image resizedImage = currentCountry.GetTransformedImage(newWidth, newHeight);
                           
            
            // need some tips
            var tt = new ToolTip();
            tt.Content = $"Country starts with: {countryImages[randIdx].GetCountryName()[0]}";
            resizedImage.ToolTip = tt;
            resizedImage.MouseLeftButtonUp += ResizedImage_MouseLeftButtonUp;
            


            // ok we have a country, show its image
            stackpanBottom.Children.Clear();
            stackpanBottom.Children.Add(resizedImage);

        }

        private void ResizedImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show(currentCountry.GetCountryName());
            _filenameLbl.Content = currentCountry.GetId();
        }

           
        public void StartGame()
        {
            score = new Score();
            progressBar.Value = 0;   

            // if !settings is system32 then go aghead and start
            if (Properties.Settings.Default.LastOpenedFolder != "C:\\windows\\system32")
            {
                countryImages = LoadCountriesFromFolder(Properties.Settings.Default.LastOpenedFolder);
                

                if (countryImages != null && countryImages.Any())
                {
                    score.amountAtStart = countryImages.Count();
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


        // Returns true if it should proceed with next question
        public bool ValidateInput(CountryImg countryImg, string inputStr, Score score)
        {
            bool nextQuestion = false;
            string resultStr = ""; 

            if (gameMode == GameModes.Standard)
            {
                if (countryImg == null || string.IsNullOrEmpty(inputStr)) { return false; }

                if (countryImg.GetCountryName().ToLower() == inputStr.ToLower())
                {
                    resultStr = "Great!";
                    score.correct++;
                    if(score.failedCountries.Contains(countryImg))//UNTESTED
                    {
                        score.failedCountries.Remove(countryImg);//UNTESTED
                    }
                    
                    countryImages.Remove(currentCountry);   // todo: maybe we want to save these but for now this is easy way to progress the game
                    nextQuestion = true;
                }
                else
                {
                    score.failedCountries.Add(countryImg);
                    score.misses++;
                    resultStr = "Wrong";
                }

            } 
            else if(gameMode == GameModes.RapidFire)            
            {
                if (countryImg.GetCountryName().ToLower() == inputStr.ToLower())
                {
                    resultStr = $"Great!";
                    countryImages.Remove(currentCountry);   // todo: maybe we want to save these but for now this is easy way to progress the game
                }   
                else
                {
                    resultStr = $"Wrong. The correct country was: {countryImg.GetCountryName()}";
                }

                nextQuestion = true;

            }      
             
            inputCountry.Text = "";

            MessageBox.Show(resultStr);

            return nextQuestion;
            
        }

        private void UpdateProgressbar(Score score)
        {
            progressBar.Maximum = score.amountAtStart;
            progressBar.Value = score.amountAtStart - countryImages.Count();

        }

        private void _okBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentCountry == null) { return; }

            bool doNextQuestion = ValidateInput(currentCountry, inputCountry.Text, score);

            UpdateProgressbar(score);

            if(doNextQuestion)
            {
                NextCountry();
            } 
        }

        
        private List<CountryImg> LoadCountriesFromFolder(string pathToPngFiles)
        {
            List<CountryImg> result = new List<CountryImg>();

            // Get the full path to each of the png files.
            // Break it up and create a Country for each file that matches the regexp pattern 
            try
            {
                List<string> fullPaths =
                    Directory.EnumerateFiles(pathToPngFiles, "*.png", SearchOption.TopDirectoryOnly).ToList();

                foreach (string fullPath in fullPaths)
                {

                    
                    string fileName = System.IO.Path.GetFileName(fullPath);
                    string pattern = "";
                    RegexOptions options = RegexOptions.Singleline;


                    // "Sweden_0001_Kista.png"

                    if (fileFormat == "SyntaxCountryNumberExtrainfo")
                    {
                        pattern = @"[a-z]+_[0-9]{4}[_a-zA-Z0-9]*\.(png|PNG)";
                        foreach (Match m in Regex.Matches(fileName, pattern, options))
                        {
                            CountryImg countryImg = new CountryImg();

                            List<string> thing = fileName.Split('_', '.').ToList();
                            if (thing.Count < 2)
                            {
                                MessageBox.Show($"filename={fileName} {defaultError}");
                            }
                            else
                            {
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
                                    MessageBox.Show($"filename={fileName} {defaultError}");
                                }
                            }
                        }
                    }
                    else if(fileFormat == "SyntaxCountry")
                    {   


                        // "Country.png"

                        pattern = @"^[a-zA-Z]*\.(png|PNG)";
                        foreach (Match m in Regex.Matches(fileName, pattern, options))
                        {
                            CountryImg countryImg = new CountryImg();
                            try
                            {
                                string countryStr = m.Value.Split('.')[0];

                                if (countryImg.SetValues(fullPath, fileName, countryStr, 0, ""))
                                {
                                    result.Add(countryImg);
                                }
                            }
                            catch(Exception ex)
                            {
                                MessageBox.Show($"Something wrong with this filename= {m.Value}, {defaultError}");
                            }     
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



            if(!Directory.Exists(Properties.Settings.Default.LastOpenedFolder))
            {
                Properties.Settings.Default.LastOpenedFolder = "C:\\windows\\system32\\";
            }


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


                                 
        private void FormatRadiobtn_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton? li = sender as RadioButton;
            
            if (li != null && li.Content != null && li.Name != null) 
            {  
                fileFormat = li.Name;
            }   
        }

        private void GameModeRapidfire_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton? li = sender as RadioButton;

            if (li != null && li.Content != null && li.Name != null)
            {
                if(li.Content.ToString() == "Default mode")
                {
                    gameMode = GameModes.Standard;
                }
                if(li.Content.ToString() == "Rapid fire mode")
                {
                    gameMode = GameModes.RapidFire;
                } 
            }
        }

        private void ImageSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            userScale = 0.1 + ((double) e.NewValue*4.0);
            
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


        public BitmapImage getRawImage()
        {
            return originalBitmap; 
        }

        public string GetCountryName()
        {
            return countryName;
        }

        public Image GetTransformedImage(int wantedWidthPx, int wantedHeightPx)
        {
            Image croppedImage = new Image();


            // How big is it? Too big?
            croppedImage.Width = wantedWidthPx;
            croppedImage.Margin = new Thickness(2);

            // Create a CroppedBitmap from BitmapImage


            // 
            // this needs THINKING
            //

            Double scale = 1.0;

            //if (originalBitmap.PixelWidth > wantedWidthPx)
            //{
            //    scale = originalBitmap.PixelWidth / wantedWidthPx;
            //    var targetBitmap = new TransformedBitmap((BitmapSource)originalBitmap, new ScaleTransform(1 / scale, 1 / scale));

            //    // after scaling... does it look good on height too?
            //    if(targetBitmap.PixelHeight > wantedHeightPx)
            //    {
            //        // jag är för trött. scale = targetBitmap...

            //        // men här slutade du iaf :) bra kodat 

            //    }

            //    // ok? ok:
            //    croppedImage.Source = targetBitmap;   

            //}
            //else if(originalBitmap.PixelHeight > wantedHeightPx)
            //{   
             
            //}
            //else
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
            return other.GetId() == this.GetId();
        }
    }
}