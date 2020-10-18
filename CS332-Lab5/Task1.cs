using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CS332_Lab5
{
    public partial class Task1 : Form
    {
        Graphics g;

        string filename;
        SortedDictionary<char, string> rules;

        public Task1()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
            rules = new SortedDictionary<char, string>();

            button2.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            rules.Clear();
            string[] rule;
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    filename = ofd.FileName;
                    string[] lines = File.ReadAllLines(filename);
                    string[] parameters = lines[0].Split(' ');
                    for (int i = 1; i < lines.Length; ++i)
                    {
                        rule = lines[i].Split('=');
                        rules[Convert.ToChar(rule[0])] = rule[1];
                    }
                    button2.Enabled = true;
                }
                catch
                {
                    button2.Enabled = false;
                    DialogResult result = MessageBox.Show("Ошибка открытия файла",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            pictureBox1.Invalidate();
        }
    }
}
