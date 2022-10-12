using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace trc_parser
{
    public partial class Form1 : Form
    {
        string path = "";
        bool bvh = false;
        bool trc = true;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateFileExtensionConditions();
            if (folderBrowserDialog1.ShowDialog()==DialogResult.OK)
            {
                path= folderBrowserDialog1.SelectedPath;
                label2.Text = path;
                DirectoryInfo dinfo = new DirectoryInfo(path);
                listBox1.Items.Clear();
                foreach (var file in dinfo.GetFiles())
                {
                    if (trc)
                    {
                        if (file.Extension == ".trc")
                        {
                            listBox1.Items.Add(file.Name);
                        }
                    }
                    else
                    {
                        if (file.Extension == ".bvh")
                        {
                            listBox1.Items.Add(file.Name);
                        }
                    }
                    
                }
                statusLabel.Text = "Found " + listBox1.Items.Count + " files.";
            }
            
        }

        private void UpdateFileExtensionConditions()
        {
            trc = trcRadioButton.Checked;
            bvh = bvhradiobutton.Checked;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int itemCount = listBox1.Items.Count;
            int counter = 1;
            UpdateFileExtensionConditions();
            if (trc)
            {
                foreach (var item in listBox1.Items)
                {
                    ConvertToCSV(item.ToString());
                    toolStripProgressBar1.ProgressBar.Value = (int)(100 * (float)counter / itemCount);
                    counter++;
                }
                statusLabel.Text = "Done. " + itemCount + " .trc files converted to .csv";
            }
            if (bvh)
            {
                foreach (var item in listBox1.Items)
                {
                    ConvertBVHToCSV(item.ToString());
                    toolStripProgressBar1.ProgressBar.Value = (int)(100 * (float)counter / itemCount);
                    counter++;
                }
                statusLabel.Text = "Done. " + itemCount + " .bvh files converted to .csv";
            }
            
        }
        private string ConvertStringArrayToLine(string[] array)
        {
            string rowToWrite = "";
            for (int i = 0; i < array.Length-1; i++)
            {
                rowToWrite += array[i] + ";";
            }
            rowToWrite += array[array.Length - 1];
            return rowToWrite;
        }
        /// <summary>
        /// Converts .trc file to .csv, trc file has to be recorded with 
        /// 53 markers. For other marker setups the function needs
        /// to be arranged.
        /// </summary>
        /// <param name="trcFile">file name of the trc file, without folder path</param>
        private void ConvertToCSV(string trcFile)
        {
            using (TextReader tr = new StreamReader(path +"\\" + trcFile))
            {
                string text = tr.ReadToEnd();
                string[] rows = text.Split("\n");

               
                var header = rows[3].Split("\t");
                var header2 = rows[4].Split("\t");
                var values = rows[5].Split("\t");
                //First index: First marker x position index
                //last index: Last marker z position index
                int firstIndex = 119;
                int lastIndex = 277;
                int markerCount = 53;
                int counter = 0;
                string[] headerLine = new string[markerCount * 3];
                for (int i = firstIndex; i < lastIndex+1; i++)
                {
                    if (counter%3==0)
                    {
                        headerLine[counter] = header[i]+"_X";
                    }
                    else if (counter%3==1)
                    {
                        headerLine[counter] = header[i - 1] + "_Y";
                    }
                    else
                        headerLine[counter] = header[i - 2] + "Z";
                    
                    counter++;
                }
                using (TextWriter tw = new StreamWriter(path + "\\" + trcFile.Replace(".trc", ".csv"))) 
                {

                    tw.WriteLine(ConvertStringArrayToLine(headerLine));
                    for (int i = 5; i < rows.Length - 1; i++)
                    {
                        var vals = rows[i].Split("\t");
                        string[] markerPositions = new string[markerCount * 3];
                        int id = 0;
                        
                        
                        for (int m = firstIndex; m < lastIndex+1; m++)
                        {
                            if (vals[m] == "")
                            {
                                markerPositions[id] = "null";
                            }
                            else
                                markerPositions[id] = vals[m];
                            id++;
                        }
                        
                        
                        tw.WriteLine(ConvertStringArrayToLine(markerPositions));
                    }
                    tw.Flush();
                    tw.Close();
                }
                
            }
            
        }

        private void ConvertBVHToCSV(string bvhFile)
        {
            using (TextReader tr = new StreamReader(path + "\\" + bvhFile))
            {
                string text = tr.ReadToEnd();
                string[] rows = text.Split("\n");
                int totalFrames = 0;
                int firstRowIndex = 0;
                for (int i = 0; i < rows.Length; i++)
                {
                    if (rows[i].StartsWith("Frames:"))
                    {
                        totalFrames = int.Parse(rows[i].Split(" ")[1]);
                        firstRowIndex = i + 2;
                        break;
                    }
                }
                string[] headerRow = new string[28 * 3];
                headerRow[0] = "HIPS_Pos_X";
                headerRow[1] = "HIPS_Pos_Y";
                headerRow[2] = "HIPS_Pos_Z";
                using (TextWriter tw = new StreamWriter(path + "\\" + bvhFile.Replace(".bvh", ".csv")))
                {

                }
            }
        }
        
    }
}
