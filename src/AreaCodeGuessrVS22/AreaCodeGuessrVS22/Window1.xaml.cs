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
using System.Windows.Shapes;

namespace AreaCodeGuessrVS22
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {              

        private SACManager sacm;

        public Window1()
        {               
            InitializeComponent();            
            numberSeriesLV.ItemsSource = new List<String> { "2", "3", "4", "5", "6", "7", "8", "9" };
            numberSeriesLV.SelectedIndex = 0;
            sacm = new SACManager(numberSeriesLV, _inputStateTxt, _areacodeLbl, _areaCodeTxt, _resultLbl);    
        }


        
        private void startBtn_Click(object sender, RoutedEventArgs e)
        {
            // Requires the user to have selected something
            if(numberSeriesLV.SelectedIndex == -1)
            {
                MessageBox.Show("Select a number series in the list");
                return;
            }

            sacm.NextQuestion();

        }

        private void _okBtn_Click(object sender, RoutedEventArgs e)
        {
            if(sacm.ValidateInput())
            {
                sacm.NextQuestion();
            }
        }
    }
}
