using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUT
{
    internal class AlternativesTableDisplay
    {

        public string Header { get; set; }
        public List<string> Values { get; set; }

        public AlternativesTableDisplay(string header, List<string> values)
        {
            Header = header;
            Values = values;
        }
    }   
}
