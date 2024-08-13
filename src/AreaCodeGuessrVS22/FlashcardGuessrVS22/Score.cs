using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashcardGuessrVS22
{
    public class Score
    {
        public int correct = 0;
        public int misses = 0;
        public int amountAtStart = 0;
        public List<CountryImg> failedCountries;

        public Score()
        {
            failedCountries = new List<CountryImg>();
        }
    }
}
