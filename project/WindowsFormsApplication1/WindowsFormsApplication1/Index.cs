using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public class Index
    {
        public string word { get; set; }
        public int hits { get; set; }
        public ArrayList index;

        public Index()
        { }

        public Index(string _word, int _hits, string _index)
        {
            index = new ArrayList();
            word = _word;
            hits = _hits;
            index.Add(_index);
        }

        public Index(string _word, int _hits, ArrayList _index)
        {
            index = _index;
            word = _word;
            hits = _hits;
        }
    }

}
