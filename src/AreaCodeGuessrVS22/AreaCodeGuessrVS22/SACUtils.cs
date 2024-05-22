using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace AreaCodeGuessrVS22
{

    public class SACManager
    {
        private string hiddenStatename = "";
        private Random rand;

        private List<StateAreaCode> allStates;
        private List<StateAreaCode> statesWithOneAreaCode;
        private TextBox areaCodeTxt;
        private TextBox inputTextBox;
        private ListView numberSeriesLV;       // The Window1.xaml listview
        public List<StateAreaCode> AllStates { get => allStates; set => allStates = value; }
        public List<StateAreaCode> StatesWithOneAreaCode { get => statesWithOneAreaCode; set => statesWithOneAreaCode = value; }
        public ListView NumberSeriesLV { get => numberSeriesLV; set => numberSeriesLV = value; }
        public TextBox InputTextBox { get => inputTextBox; set => inputTextBox = value; }
        public TextBox AreaCodeTxt { get => areaCodeTxt; set => areaCodeTxt = value; }

        // The manager grabs all the controllers it needs,
        // So when its time to read or modify its values, it doesnt have to ask for it

        public SACManager(ListView listviewCtrl, TextBox _inputTextBox, Label _statusLbl, TextBox _areaCodeTxt)
        {
            // Assign Controls
            NumberSeriesLV = listviewCtrl;
            AreaCodeTxt = _areaCodeTxt;
            InputTextBox = _inputTextBox;


            // Load and parse from file
            rand = new Random(DateTime.UtcNow.Millisecond);
            Tuple<List<StateAreaCode>, List<StateAreaCode>> tuple = SACUtils.LoadFile();
            AllStates = tuple.Item1;
            StatesWithOneAreaCode = tuple.Item2;


            
        }

        
        public void NextQuestion()
        {
            StateAreaCode randomSAC = RandomizeAreaCode(1);           

            if (randomSAC != null && randomSAC.areaCodes.Any())
            {

                AreaCodeTxt.Text = randomSAC.areaCodes[0].ToString();
                hiddenStatename = randomSAC.stateName;
                InputTextBox.Text = "";
                
            }

        }


        public StateAreaCode RandomizeAreaCode(int mode) // 0 for normal, 1 for only one areacode
        {

            StateAreaCode res = new StateAreaCode();

            int tries = 0;
            int maxTries = 200;
            bool done = false;

            while (tries++ < maxTries && !done)
            {

                int ac = -1;

                if (mode == 1)
                {
                    
                    var statesOneAcIdx = rand.Next(0, StatesWithOneAreaCode.Count);


                    int currAreacodeIdx = 0;

                    ac = StatesWithOneAreaCode[statesOneAcIdx].areaCodes[currAreacodeIdx];


                    // This is the one, use it
                    res.stateName = StatesWithOneAreaCode[statesOneAcIdx].stateName;
                    res.areaCodes.Add(ac);

                    done = true;
                }
                else if (mode == 0)
                {
                    //allstateIdx = rand.Next(0, 51);
                    //currAreacodeIdx = rand.Next(0, allStates[allstateIdx].areaCodes.Count);
                    //ac = allStates[allstateIdx].areaCodes[currAreacodeIdx];
                    //var thing = ac.ToString()[0].ToString();



                    //foreach (ListViewItem lvi in NumberSeriesLV.CheckedItems)
                    //{
                    //    if (lvi.Text == thing)
                    //    {
                    //        // This is the one, use it
                    //        res.stateName = allStates[allstateIdx].stateName;
                    //        res.areaCodes.Add(ac);
                    //        done = true;
                    //    }
                    //}

                }
            }

            if (tries >= maxTries)
            {
                MessageBox.Show("could not find any");
            }

            return res;
        }


        // test the input
        public void ValidateInput()
        {
            if(inputTextBox.Text == hiddenStatename)
            {
                // Good
            }            else
            {
                // BAD
            }
        }

    }


    // SAC as in State AreaCodes
    public static class SACUtils
    {
        // Loads the file and returns a tuple containing:
        // - All states with their area codes (item1)
        // - States with only one area code (item2)
        public static Tuple<List<StateAreaCode>, List<StateAreaCode>> LoadFile()
        {
            var allStates = new List<StateAreaCode>();
            var statesWithOneAreaCode = new List<StateAreaCode>();

            try
            {
                // Read all lines from the file
                List<string> fileLines = File.ReadAllLines("areacodes.txt", Encoding.UTF8).ToList();

                // Process each line
                foreach (string line in fileLines)
                {
                    var stateAreaCode = new StateAreaCode();
                    var parts = line.Split(new char[] { ' ', '\t', ',' });

                    stateAreaCode.stateName = parts[0];

                    foreach (var part in parts)
                    {
                        if (int.TryParse(part, out int areaCode))
                        {
                            Debug.WriteLine($"{part}");
                            stateAreaCode.areaCodes.Add(areaCode);
                        }
                    }

                    allStates.Add(stateAreaCode);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Could not open file.");
            }

            // Filter states with exactly one area code
            statesWithOneAreaCode = allStates.Where(state => state.areaCodes.Count == 1).ToList();

            return new Tuple<List<StateAreaCode>, List<StateAreaCode>>(allStates, statesWithOneAreaCode);
        }
    }

}
