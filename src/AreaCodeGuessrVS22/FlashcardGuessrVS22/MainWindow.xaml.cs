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
        private string currentCountry = "";
        private List<string> imagePaths = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("starting game now");
        }

        public bool ValidateInput(string inputStr)
        {
            bool correctInput = false;
            if (inputStr == currentCountry)
            {
                correctInput = true;

                // remove the country

            }

            return correctInput;
        }

        private void _okBtn_Click(object sender, RoutedEventArgs e)
        {
            if(ValidateInput(inputCountry.Text))
            {
                MessageBox.Show("Great!");       
            }
            else
            {
                MessageBox.Show("Wrong.");
            }
        }


        private void showimage(string pathToPngFiles)
        {
           imagePaths = new List<string>();

            try
            {
                var files = Directory.EnumerateFiles(pathToPngFiles);

                foreach (string filename in files)
                {
                    string pattern = @"[a-z]+_[0-9]{4}[_a-zA-Z0-9]*\.(png|PNG)";
                    RegexOptions options = RegexOptions.Singleline;

                    //string item = "example_2024_image123.PNG"; // Example input string

                    foreach (Match m in Regex.Matches(filename, pattern, options))
                    {
                        Console.WriteLine("'{0}' found at index {1}.", m.Value, m.Index);
                        imagePaths.Add(filename);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }      


            // Now show it
            if (imagePaths.Any())
            {   
                // Create a BitmapImage
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                string fullpath = imagePaths.Last();
                bitmap.UriSource = new Uri(fullpath);
                bitmap.EndInit();

                // Create an Image
                Image croppedImage = new Image();
                croppedImage.Width = 400;
                croppedImage.Margin = new Thickness(2);

                // Create a CroppedBitmap from BitmapImage
                if(bitmap.PixelWidth >= 400 && bitmap.PixelHeight>= 400)
                {
                    var scale = Math.Max(bitmap.PixelWidth, bitmap.PixelHeight)/400.0;

                    var targetBitmap = new TransformedBitmap((BitmapSource)bitmap, new ScaleTransform(1/scale, 1/scale));

                    croppedImage.Source = targetBitmap;    
                }  
                else
                {
                    
                    croppedImage.Source = bitmap;
                }

                                         

                // Add Image to Window
                stackpanBottom.Children.Add(croppedImage);

            }       

            return;
            
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

            if(fullFolderPath != "")
            {
                Properties.Settings.Default.LastOpenedFolder = fullFolderPath;
                Properties.Settings.Default.Save();
                showimage(fullFolderPath);     
            }
        }
    }
}