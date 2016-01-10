using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace WindowsFormsApplication1
{
    public class IndexTable
    {
        public ArrayList index_row;

        private string path_to_IndexTable = "IndexTable.xml";

        public IndexTable(Dictionary<int, string> dict_with_index_paths, List<string> stopwords)
        {
            index_row = new ArrayList();

            foreach (var item in dict_with_index_paths)
            {
                StreamReader reader = new StreamReader(item.Value);
                string[] words = Regex.Split(reader.ReadToEnd(), @"\W+");
                foreach(string word in words)
                {
                    if (!stopwords.Contains(word) &&
                        word.Count() > 1 &&
                        word != "")
                        index_row.Add(new Index(word.ToLower(), 1, item.Key.ToString()));
                }
            }

            IndexCompareWords sort_index = new IndexCompareWords();
            index_row.Sort(sort_index);

            foreach(string duplicate in findDuplicates(index_row))
            {
               reduceDuplicateAndUpdateIndexTable(index_row, duplicate);
            }

            saveIndexTableToDisk(index_row);
        }

        public IndexTable()
        {
            loadIndexTableFromDisk();
        }

        public void updateWithNewDoc(Dictionary<int, string> dict_with_index_paths, List<string> stopwords, int start_index_key)
        {
            ArrayList index_array_to_update = new ArrayList();

            for (int index = start_index_key + 1; index < dict_with_index_paths.Count; index++ )
            {
                StreamReader reader = new StreamReader(dict_with_index_paths[index]);
                string[] words = Regex.Split(reader.ReadToEnd(), @"\W+");

                foreach (string word in words)
                {
                    if (!stopwords.Contains(word) &&
                        word.Count() > 1 &&
                        word != "")
                        index_array_to_update.Add(new Index(word.ToLower(), 1, index.ToString()));
                }
            }

            IndexCompareWords sort_index = new IndexCompareWords();

            index_row.AddRange(index_array_to_update);

            index_row.Sort(sort_index);


            foreach (string duplicate in findDuplicates(index_row))
            {
                reduceDuplicateAndUpdateIndexTable(index_row, duplicate);
            }

            saveIndexTableToDisk(index_row);
        }

        public List<string> findDuplicates(ArrayList index_row_to_update)
        {
            List<string> duplicate_words = new List<string>();
            var dublicateGroups =
               (from Index item in index_row_to_update select item).GroupBy(s => s.word).Select(
               group => new { word = group.Key, Count = group.Count()}).Where(x => x.Count >= 2);


            foreach (var duplicate in dublicateGroups)
            {
                duplicate_words.Add(duplicate.word);
            }

            return duplicate_words;
        }

        public void reduceDuplicateAndUpdateIndexTable(ArrayList index_row_to_find_index, string find_word)
        {
            List<int> index_list = new List<int>();
            foreach (Index index in index_row_to_find_index)
            {
                if (index.word == find_word)
                {
                    index_list.Add(index_row_to_find_index.IndexOf(index));
                }
            }

            if (index_list.Count > 1)
            {
                ((Index)index_row_to_find_index[index_list[0]]).hits = index_list.Count;
                for (int count = 1; count < index_list.Count; count++)
                {
                    foreach (string index_path in ((Index)index_row_to_find_index[index_list[count]]).index)
                    {
                        if (!((Index)index_row_to_find_index[index_list[0]]).index.Contains(index_path))
                            ((Index)index_row_to_find_index[index_list[0]]).index.Add(index_path);
                    }
                }

                index_row_to_find_index.RemoveRange(index_list[1], index_list.Count - 1);
            }
          
        }

        public void saveIndexTableToDisk(ArrayList index_row)
        {
            //FileStream fs = new FileStream(path_to_IndexTable, FileMode.OpenOrCreate, FileAccess.Write);
            //var serializer = new XmlSerializer(typeof(ArrayList), new Type[] { typeof(Index) });
            //serializer.Serialize(fs, index_row);

            using (XmlWriter xml_writer = XmlWriter.Create(path_to_IndexTable))
            {
                xml_writer.WriteStartDocument();
                xml_writer.WriteStartElement("IndexTable");
                foreach (Index entry in index_row)
                {
                    xml_writer.WriteStartElement("IndexRow");
                    xml_writer.WriteAttributeString("Word", entry.word);
                    xml_writer.WriteAttributeString("Hits", entry.hits.ToString());

                    StringBuilder index_array_toString = new StringBuilder();
                    foreach (var s in entry.index)
                    {
                        index_array_toString.Append(s.ToString());
                        index_array_toString.Append(",");
                    }
                    xml_writer.WriteAttributeString("Index", index_array_toString.ToString().TrimEnd(','));
                    xml_writer.WriteEndElement();
                }
                xml_writer.WriteEndElement();
                xml_writer.WriteEndDocument();
            }

        }

        public void loadIndexTableFromDisk()
        {
            index_row = new ArrayList();

            var elements = XElement.Load(path_to_IndexTable).Elements("IndexRow");

            if (index_row.Count>0)index_row.Clear();

            foreach (var el in elements)
            {
                string indexes = (string)el.Attribute("Index");
                index_row.Add(new Index((string)el.Attribute("Word"), (int)el.Attribute("Hits"), new ArrayList(indexes.Split(','))));
            }    
        }

        public void delete_file_index(string index)
        {
            foreach (Index checking_index in index_row)
            {
                if (checking_index.index.Contains(index))
                {
                    checking_index.index.Remove(index);
                }
            }

            saveIndexTableToDisk(index_row);
            loadIndexTableFromDisk();
        }

        public ArrayList searchResults(string search_query)
        {
            bool flag_AND = false;
            bool flag_OR = false;
            bool flag_NOT = false;

            string and_pattern = @"(?<before>\w+) AND (?<after>\w+)";
            string or_pattern = @"(?<before>\w+) OR (?<after>\w+)";
            string not_pattern = @"(?<before>\w+) NOT (?<after>\w+)";

            string and_pattern_after = @"AND (?<after>\w+)";
            string or_pattern_after = @"OR (?<after>\w+)";
            string not_pattern_after = @"NOT (?<after>\w+)";

            string and_pattern_before = @"(?<before>\w+) AND";
            string or_pattern_before = @"(?<before>\w+) OR";
            string not_pattern_before = @"(?<before>\w+) NOT";

            string produced_search_query = search_query;

            ArrayList and_arraylist = new ArrayList();
            ArrayList or_arraylist = new ArrayList();
            ArrayList not_arraylist = new ArrayList();
            ArrayList bracket_arraylist = new ArrayList();

            MatchCollection brackets_pattern = Regex.Matches(produced_search_query, @"\(([^)]*)\)");

            if (brackets_pattern.Count > 0)
            {
                for (int i = 0; i < brackets_pattern.Count; i++)
                {
                    produced_search_query = produced_search_query.Replace(brackets_pattern[i].ToString(), "");

                    MatchCollection and_matches_brackets = Regex.Matches(brackets_pattern[i].Groups[1].ToString(), and_pattern);
                    MatchCollection or_matches_brackets = Regex.Matches(brackets_pattern[i].Groups[1].ToString(), or_pattern);
                    MatchCollection not_matches_brackets = Regex.Matches(brackets_pattern[i].Groups[1].ToString(), not_pattern);

                    for (int y = 0; y < brackets_pattern.Count; y++)
                    {
                        if (and_matches_brackets.Count > 0)
                        {
                            foreach (string index in searchQueryAND(and_matches_brackets[i].ToString().Replace("AND", "")))
                                if (!bracket_arraylist.Contains(index)) bracket_arraylist.Add(index);
                        }

                        if (or_matches_brackets.Count > 0)
                        {
                            foreach (string index in searchQueryOR(or_matches_brackets[i].ToString().Replace("OR", "")))
                                if (!bracket_arraylist.Contains(index)) bracket_arraylist.Add(index);
                        }

                        if (not_matches_brackets.Count > 0)
                        {
                            foreach (string index in searchQueryNOT(not_matches_brackets[i].ToString().Replace("NOT", ""),
                                                          not_matches_brackets))
                                if (!bracket_arraylist.Contains(index)) bracket_arraylist.Add(index);
                        }


                    }
                }
                //MessageBox.Show(produced_search_query);
                MatchCollection and_matches_after = Regex.Matches(produced_search_query, and_pattern_before);
                MatchCollection or_matches_after = Regex.Matches(produced_search_query, or_pattern_before);
                MatchCollection not_matches_after = Regex.Matches(produced_search_query, not_pattern_before);

                if (and_matches_after.Count > 0)
                {
                    ArrayList and_array_after_brackets = new ArrayList();
                    ArrayList intersection = new ArrayList();

                    produced_search_query = produced_search_query.Replace("AND", "");
                    and_array_after_brackets = searchQueryAND(produced_search_query);

                    foreach (string i in and_array_after_brackets.Cast<string>().Intersect(bracket_arraylist.Cast<string>()))
                    {
                        intersection.Add(i);
                    }

                    return intersection;
                }

                if (or_matches_after.Count > 0)
                {
                    ArrayList or_array_after_brackets = new ArrayList();

                    produced_search_query = produced_search_query.Replace("OR", "");
                    or_array_after_brackets = searchQueryOR(produced_search_query);

                    foreach (string index in bracket_arraylist)
                    {
                        if (!or_array_after_brackets.Contains(index)) or_array_after_brackets.Add(index);
                    }

                    return or_array_after_brackets;
                }

                if(not_matches_after.Count > 0)
                {
                    ArrayList not_array_after_brackets = new ArrayList();
                    produced_search_query = produced_search_query.Replace("NOT", "");
                    not_array_after_brackets = searchQueryOR(produced_search_query);

                    foreach (string index in bracket_arraylist)
                    {
                        if (not_array_after_brackets.Contains(index)) not_array_after_brackets.Remove(index);
                    }

                    return not_array_after_brackets;
                }

                return bracket_arraylist;
            }
            else
            {
                MatchCollection and_matches = Regex.Matches(search_query, and_pattern);
                MatchCollection or_matches = Regex.Matches(search_query, or_pattern);
                MatchCollection not_matches = Regex.Matches(search_query, not_pattern);

                if (and_matches.Count > 0)
                {
                    flag_AND = true;
                    produced_search_query = produced_search_query.Replace("AND", "");             
                }

                if (or_matches.Count > 0)
                {
                    flag_OR = true;
                    produced_search_query = produced_search_query.Replace("OR", "");    
                }

                if (not_matches.Count > 0)
                {
                    flag_NOT = true;
                    produced_search_query = produced_search_query.Replace("NOT", ""); 
                }

                if (flag_AND) return searchQueryAND(produced_search_query);

                if (flag_NOT) return searchQueryNOT(produced_search_query, not_matches);

                if (flag_OR) return searchQueryOR(produced_search_query);
            
            }

            return searchQueryAND(produced_search_query);

            //MatchCollection and_matches = Regex.Matches(search_query, and_pattern);
            //MatchCollection or_matches = Regex.Matches(search_query, or_pattern);
            //MatchCollection not_matches = Regex.Matches(search_query, not_pattern);


            //for (int i = 0; i < and_matches.Count; i++)
            //{
            //    MessageBox.Show("And before " + and_matches[i].Groups["before"].ToString());
            //    MessageBox.Show("And after " + and_matches[i].Groups["after"].ToString());
            //}

            //for (int i = 0; i < or_matches.Count; i++)
            //{
            //    MessageBox.Show("or before " + or_matches[i].Groups["before"].ToString());
            //    MessageBox.Show("or after " + or_matches[i].Groups["after"].ToString());
            //}

            //for (int i = 0; i < not_matches.Count; i++)
            //{
            //    MessageBox.Show("not before " + not_matches[i].Groups["before"].ToString());
            //    MessageBox.Show("not after " + not_matches[i].Groups["after"].ToString());
            //}


            //if (and_matches.Count > 0)
            //{
            //    flag_AND = true;
            //    produced_search_query = produced_search_query.Replace("AND", "");             
            //}

            //if (or_matches.Count > 0)
            //{
            //    flag_OR = true;
            //    produced_search_query = produced_search_query.Replace("OR", "");    
            //}

            //if (not_matches.Count > 0)
            //{
            //    flag_NOT = true;
            //    produced_search_query = produced_search_query.Replace("NOT", ""); 
            //}
        }

        public ArrayList searchQueryAND(string produced_search_query)
        {
            ArrayList result_array = new ArrayList();
            ArrayList intersection = new ArrayList();

            string[] words = produced_search_query.ToLower().Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int found_words = 0;

            foreach (string word in words)
            {
                foreach (Index index in index_row)
                {
                    if (index.word == word)
                    {
                        found_words++;

                        if (result_array.Count == 0)
                        {
                            result_array = new ArrayList(index.index);
                        }

                        else
                        {
                            foreach (string i in index.index.Cast<string>().Intersect(result_array.Cast<string>()))
                            {
                                intersection.Add(i);
                            }

                            result_array = new ArrayList(intersection);
                            intersection.Clear();
                        }

                        break;
                    }
                }
            }

            if (found_words != words.Length)
            {
                result_array.Clear();
                return result_array;
            }

            return result_array;
        }

        public ArrayList searchQueryOR(string produced_search_query)
        {
            ArrayList result_array = new ArrayList();

            string[] words = produced_search_query.ToLower().Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                foreach (Index index in index_row)
                {
                    if (index.word == word)
                    {
                        foreach (string index_placed in index.index)
                        {
                            if (!result_array.Contains(index_placed)) result_array.Add(index_placed);
                        }
                        break;
                        
                    }
                }
            }
            return result_array;        
        }

        public ArrayList searchQueryNOT(string produced_search_query, MatchCollection not_matches)
        {
            string word_to_find = null;
            string word_to_exclude = null;

            for (int i = 0; i < not_matches.Count; i++)
            {
                word_to_find = not_matches[i].Groups["before"].ToString();
                word_to_exclude = not_matches[i].Groups["after"].ToString();
            }

            ArrayList result_array = new ArrayList();
            ArrayList reduce_array = new ArrayList();

                foreach (Index index in index_row)
                {
                    if (index.word == word_to_find)
                    {
                        foreach (string index_placed in index.index)
                        {
                            if (!result_array.Contains(index_placed)) result_array.Add(index_placed);
                        }
                        break;
                    }
                }

                foreach (Index index in index_row)
                {
                    if (index.word == word_to_exclude)
                    {
                        foreach (string index_placed in index.index)
                        {
                            if (!reduce_array.Contains(index_placed)) reduce_array.Add(index_placed);
                        }
                        break; 
                    }
                                  
                }
           
                foreach (string reduce in reduce_array)
                {
                    result_array.Remove(reduce);   
                }

            return result_array;

        }

        public class IndexCompareWords : IComparer
        {
            public int Compare(object x, object y)
            {
                Index word_1 = (Index)x;
                Index word_2 = (Index)y;
                return string.Compare(word_1.word, word_2.word );
            }
        }

    }
}
