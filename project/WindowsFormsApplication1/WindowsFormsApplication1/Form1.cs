using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace WindowsFormsApplication1
{
    //write class to load/save choosen path for XMLDB
    //write class to load/save words found in the documents
    //use IndexTable's save and load XML function to store and load words and indexes
    //
    public partial class Form1 : Form
    {
        private List<string> selected_files_paths;
        private Dictionary<int, string> selected_paths_dictinary = new Dictionary<int, string>(); //our prime source for paths of the files
        private string root_directory_for_application = Environment.CurrentDirectory;             
        private string root_path_to_paging_file = "paging_file.txt";                              
        private string root_path_to_readme_file = "readme.txt";         
        private string root_path_to_localDbFolder = "LocalDB";                                    //Directory of copied docs
        private string root_path_to_xml_DB = "xmlDB.xml";                                         //page files stores offline paths of the files
        private string empty_path = "*****";
        private OpenFileDialog open_file_dialog = new OpenFileDialog();
        private string path_to_stopwords = "stopwords.txt";
        private List<string> stopwords;
        private IndexTable index_table;

        private int ItemMargin = 5;
 
        public Form1()
        {
            InitializeComponent();

            //we need this for our custom listbox, cause the basic one don't support mutlilines
            SearchListBox.DrawMode = DrawMode.OwnerDrawVariable;

            //load up our list of stop words, you can add or remove words from the list
            //look for stopwords.txt in the same directory as the executable
            stopwords = readStopWords(path_to_stopwords);            

            //if our DB exist we will load it into the program
            if (File.Exists(root_path_to_xml_DB))
            {
                loadXMLdb(root_path_to_xml_DB); 
                index_table = new IndexTable();
            }
            //else we will give message about empty DB and ask to supply docs
            else
            {
                MessageBox.Show("Empty Database", "Empty Database", MessageBoxButtons.OK, MessageBoxIcon.Information);
                addFilesToPostFileWithIndexAndPaths();
                index_table = new IndexTable(selected_paths_dictinary, stopwords);
            }

            //deleteFileFromPostFileWithIndexAndPaths(@"C:\Users\AlexGruber\Desktop\WpfApplication3\WpfApplication3\bin\Debug\Data\Test2.txt");
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);

        }
        //event which using enter key, to shortcut pressing a button
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchButton_Click(null, null);
            }   
            e.Handled = false;
        }

        //function loads existing xml DB into dictonary for further internal use
        private void loadXMLdb(string root_path_to_xml_DB)
        {
           selected_paths_dictinary = XElement.Load(root_path_to_xml_DB)
                             .Elements("Word")
                             .ToDictionary(
                              el => (int)el.Attribute("index"),
                              el => (string)el.Attribute("path"));              
        }
        
        //function fires up only if DB is empty, populates DB and xml files.
        private void addFilesToPostFileWithIndexAndPaths()
        {
            open_file_dialog.Multiselect = true; 
            open_file_dialog.DefaultExt = ".txt";
            open_file_dialog.Filter = "TXT Files (*.txt)|*.txt";
            open_file_dialog.ShowDialog();

            if (open_file_dialog.FileNames.Count() != 0)
            {

                selected_files_paths = open_file_dialog.FileNames.ToList();

                copyFilesToLocalDbStorage(selected_files_paths.ToArray(), root_path_to_localDbFolder);

                selected_paths_dictinary = selected_files_paths.Select((s, i) => new { s, i }).ToDictionary(x => x.i, x => Path.Combine(root_path_to_localDbFolder,Path.GetFileName(x.s))); // Path.Combine(root_path_to_localDbFolder, Path.GetFileName(s)
                
                createLocalPostFileWithIndexAndPaths(root_path_to_paging_file);
             
            }
            else
            {
                MessageBox.Show("No Files Selected","Empty request", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            
        }

        //Stores localy dictnary of indexes and paths
        private void createLocalPostFileWithIndexAndPaths(string root_path_to_localDB)
        {
           try
           {
               if (!File.Exists(root_path_to_localDB))
               {
                   using (XmlWriter xml_writer = XmlWriter.Create(root_path_to_xml_DB))
                   {
                       xml_writer.WriteStartDocument();
                       xml_writer.WriteStartElement("Words");
                       foreach (var entry in selected_paths_dictinary)
                       {
                           xml_writer.WriteStartElement("Word");
                           xml_writer.WriteAttributeString("index", entry.Key.ToString());
                           xml_writer.WriteAttributeString("path", entry.Value.ToString());
                           xml_writer.WriteEndElement();
                       }
                       xml_writer.WriteEndElement();
                       xml_writer.WriteEndDocument();
                   }
               }
               else
               {
                   XDocument xDocument = XDocument.Load(root_path_to_xml_DB);
                   XElement root = xDocument.Element("Words");
                   IEnumerable<XElement> rows = root.Descendants("Word");
                   XElement last_row = rows.Last();
                   last_row.AddAfterSelf(
                                     new XElement("Word",
                                     new XAttribute("index", selected_paths_dictinary.Keys.Last().ToString()),
                                     new XAttribute("path", selected_paths_dictinary[selected_paths_dictinary.Keys.Last()].ToString())));
                   xDocument.Save(root_path_to_xml_DB);

               }

               
           }

           catch (IOException ex)
           {
               MessageBox.Show(ex.Message);
           }
        }

        //function fires up only when adding new files/file to the DB
        private void addNewFileToPostFileWithIndexAndPaths(IndexTable index_table)
        {
            open_file_dialog.Multiselect = true;
            open_file_dialog.DefaultExt = ".txt";
            open_file_dialog.Filter = "TXT Files (*.txt)|*.txt";
            open_file_dialog.ShowDialog();

            var newpaths = (from newpath in open_file_dialog.FileNames.ToList()
                            let found = selected_paths_dictinary.Any(existpath => existpath.Value == Path.Combine(root_path_to_localDbFolder , Path.GetFileName(newpath)))
                            where !found
                            select newpath).ToList();

            if (newpaths.Count > 0) copyFilesToLocalDbStorage(newpaths.ToArray(), root_path_to_localDbFolder);

            int start_key_for_index = selected_paths_dictinary.Keys.Last();

            foreach (string path in newpaths)
            {
                int last_key = selected_paths_dictinary.Keys.Last();
                selected_paths_dictinary.Add(++last_key, Path.Combine(root_path_to_localDbFolder, Path.GetFileName(path))); 
            }

            createLocalPostFileWithIndexAndPaths(root_path_to_paging_file);

            index_table.updateWithNewDoc(selected_paths_dictinary,stopwords, start_key_for_index);

            MessageBox.Show("Complete");
      
        }

        private void deleteFileFromPostFileWithIndexAndPaths(string file_to_delete)
        {
            int key_to_delete = selected_paths_dictinary.First(x => x.Value.Contains(file_to_delete)).Key;
            //x.Value.Contains(state)
            selected_paths_dictinary[key_to_delete] = empty_path;
            createLocalPostFileWithIndexAndPaths(root_path_to_paging_file);
        }

        //function load up words for stop list 
        private List<string> readStopWords(string path_to_stopwords)
        {
            List<string> lines = new List<string>();

            using (StreamReader r = new StreamReader(path_to_stopwords))
            {
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }
            return lines;
        }

        //function copy files to local db folder
        public void copyFilesToLocalDbStorage(string[] sources, string destination)
        {
            foreach (string source_path in sources)
            { 
               File.Copy(source_path, Path.Combine(destination, Path.GetFileName(source_path)), true);
            }        
        }

        //program buttons, lists, controls etc

        private void addNewFilesFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addNewFileToPostFileWithIndexAndPaths(index_table);
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            ArrayList results = new ArrayList();
            SearchListBox.Items.Clear();
            results = index_table.searchResults(SearchTextBox.Text);

            foreach (var index in results)
            {
                StringBuilder three_first_lines = new StringBuilder();

                List<string> lines = new List<string>(File.ReadLines(selected_paths_dictinary[Convert.ToInt32(index)]));

                three_first_lines.AppendLine("Found at index [" + index + "]");

                for (int i = 0; i < 3; i++)
                {
                  three_first_lines.AppendLine(lines[i]);
                }
                
                SearchListBox.Items.Add(three_first_lines.ToString());
            }



        }

        private void SearchListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            // Get the ListBox and the item.
            ListBox lst = sender as ListBox;
            string txt = (string)lst.Items[e.Index];
            int maximum_sentence_long = 90;

            // Measure the string.
            SizeF txt_size = e.Graphics.MeasureString(txt, this.Font);

            // Set the required size.
            e.ItemHeight = (int)txt_size.Height + 2 * ItemMargin;
            e.ItemWidth = (int)txt_size.Width;


            /////////////////////////
            if ((int)txt_size.Width > 1000)
            {
                //string[] words = System.Text.RegularExpressions.Regex.Split(txt, @"\W+");
                string[] words = txt.Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                StringBuilder phrase = new StringBuilder();

                phrase.Append(words[0] + " ");
                phrase.Append(words[1] + " ");
                phrase.Append(words[2] + " ");
                phrase.AppendLine(words[3] + " ");
                for (int i = 4; maximum_sentence_long > i; i++)
                {
                    if (i % 30 == 0)
                    {
                        phrase.AppendLine(words[i] + " ");
                    }
                    else
                    {
                        phrase.Append(words[i] + " ");
                    }

                }

                lst.Items[e.Index] = phrase.ToString();

                txt_size = e.Graphics.MeasureString(phrase.ToString(), this.Font);

                // Set the required size.
                e.ItemHeight = (int)txt_size.Height + 3 * ItemMargin;
                e.ItemWidth = (int)txt_size.Width;

            }
            /////////////////////////

        }

        private void SearchListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Get the ListBox and the item.
            ListBox lst = sender as ListBox;
            string txt = (string)lst.Items[e.Index];

            // Draw the background.
            e.DrawBackground();

            // See if the item is selected.
            if ((e.State & DrawItemState.Selected) ==
                DrawItemState.Selected)
            {
                // Selected. Draw with the system highlight color.
                e.Graphics.DrawString(txt, this.Font,
                    SystemBrushes.HighlightText, e.Bounds.Left,
                        e.Bounds.Top + ItemMargin);
            }
            else
            {
                // Not selected. Draw with ListBox's foreground color.
                using (SolidBrush br = new SolidBrush(e.ForeColor))
                {
                    e.Graphics.DrawString(txt, this.Font, br,
                        e.Bounds.Left, e.Bounds.Top + ItemMargin);
                }
            }

            // Draw the focus rectangle if appropriate.
            e.DrawFocusRectangle();
        }

        private void SearchListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox list_box = sender as ListBox;

            string index = Regex.Match(list_box.SelectedItem.ToString(), @"\[([^]]*)\]").Groups[1].Value; 

            ArticleBox text_box_for_article = new ArticleBox(selected_paths_dictinary[Convert.ToInt32(index)], SearchTextBox.Text);

            text_box_for_article.ShowDialog();
        }

        private void deleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteFile delete_file_window = new DeleteFile(selected_paths_dictinary, index_table);
            delete_file_window.ShowDialog();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ArticleBox text_box_for_Help = new ArticleBox(root_path_to_readme_file, "HelperForUser");

            text_box_for_Help.ShowDialog();
        }
        
    }
}
