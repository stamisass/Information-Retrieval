using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class DeleteFile : Form
    {
        private IndexTable index_table_DeleteFile;

        public DeleteFile(Dictionary<int, string> selected_paths_dictinary, IndexTable index_table)
        {
            InitializeComponent();

            index_table_DeleteFile = index_table;

            DeleteFilelistBox.DataSource = new BindingSource(selected_paths_dictinary, null);
            DeleteFilelistBox.DisplayMember = "Value";
            DeleteFilelistBox.ValueMember = "Key";
        }

        private void DeleteFileButton_Click(object sender, EventArgs e)
        {
            index_table_DeleteFile.delete_file_index(DeleteFilelistBox.SelectedValue.ToString());
            MessageBox.Show("Corrent index was deleted from index file : " + DeleteFilelistBox.SelectedValue.ToString());
        }
    }
}
