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
        Bitmap bm;
        Graphics g;
        int main_angle;
        int dangle;
        Dictionary<char, string> rules;
        string atom;
        float f_length = 10;
        Queue<MyEdge> draw_queue;
        Queue<int> savedRandAngle;
        Random r = new Random();
        string filename;

        public Task1()
        {
            InitializeComponent();
            bm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.Image = bm;
            g = Graphics.FromImage(bm);
            rules = new Dictionary<char, string>();
            draw_queue = new Queue<MyEdge>();
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
                    atom = parameters[0];
                    dangle = Int16.Parse(parameters[1]);
                    main_angle = Int16.Parse(parameters[2]);
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

        //-------------------------------------------------------------------------------
        static public MyEdge EdgeByAngle(PointF start, int angle, float length)
        {
            double x = (Math.Cos((angle * Math.PI) / 180) * length);
            double y = (Math.Sin((angle * Math.PI) / 180) * length);
            return new MyEdge(new PointF(start.X, start.Y), new PointF(start.X + (float)x, start.Y + (float)y));
        }

        string iterativeString(string cur)
        {
            StringBuilder res = new StringBuilder();
            foreach (var x in cur)
            {
                if (rules.ContainsKey(x))
                {
                    res.Append(rules[x]);
                }
                else
                {
                    res.Append(x);
                }
            }
            return res.ToString();
        }

        public float ScaleSystem(string input, ref PointF start_point, int angle, int iter)
        {

            float length = f_length;
            double min_x = 0;
            double max_x = 0;
            double min_y = pictureBox1.Height - 10;
            double max_y = pictureBox1.Height - 10;

            Stack<PointF> savedPoint = new Stack<PointF>();
            Stack<int> savedAngle = new Stack<int>();
            savedRandAngle = new Queue<int>();
            bool rand_on = false;

            foreach (var ch in input)
            {
                if (ch == '+')
                {
                    if (rand_on)
                        angle += dangle + r.Next(-dangle, dangle) / 2;
                    else angle += dangle;

                }
                else if (ch == '-')
                {
                    if (rand_on)
                        angle -= dangle + r.Next(-dangle, dangle) / 2;
                    else angle -= dangle;
                }
                else if (ch == 'F')
                {
                    if (rand_on)
                        savedRandAngle.Enqueue(angle);

                    MyEdge e = EdgeByAngle(start_point, angle, length);
                    if (e.end.X > max_x)
                        max_x = e.end.X;
                    if (e.end.Y > max_y)
                        max_y = e.end.Y;
                    if (e.end.Y < min_y)
                        min_y = e.end.Y;
                    if (e.end.X < min_x)
                        min_x = e.end.X;
                    start_point = e.end;
                }
                else if (ch == '[')
                {
                    savedAngle.Push(angle);
                    savedPoint.Push(start_point);
                }
                else if (ch == ']')
                {
                    start_point = savedPoint.Pop();
                    angle = savedAngle.Pop();
                }
                else if (ch == '@')
                {
                    rand_on = true;
                }
            }
            double dx = max_x - min_x;
            double dy = max_y - min_y;
            double prop_x, prop_y, min_prop;
            if (dx != 0)
            {
                prop_x = (pictureBox1.Width - 1) / dx;
            } else
            {
                prop_x = -1;
            }

            if (dy != 0)
            {
                prop_y = (pictureBox1.Height - 1) / dy;
            } else
            {
                prop_y = -1;
            }

            if (prop_x < prop_y)
            {
                min_prop = prop_x;
            }
            else
            {
                min_prop = prop_y;
            }
            start_point = new PointF((float)Math.Abs(min_x * min_prop), (float)Math.Abs((min_y - pictureBox1.Height + 10) * min_prop));
            return length * (float)min_prop;
        }
        public void LSystem(int iter)
        {
            PointF cur_point = new PointF(0, pictureBox1.Height - 10);
            string cur_iter = atom;
            int cur_angle = main_angle;
            for (int i = 0; i < iter; i++)
            {
                cur_iter = iterativeString(cur_iter);
            }
            f_length = ScaleSystem(cur_iter, ref cur_point, cur_angle, iter);

            Stack<PointF> savedPoint = new Stack<PointF>();
            Stack<int> savedAngle = new Stack<int>();
            bool rand_on = false;

            foreach (var ch in cur_iter)
            {
                if (ch == '+')
                {
                    cur_angle += dangle;
                }
                else if (ch == '-')
                {
                    cur_angle -= dangle;

                }
                else if (ch == 'F')
                {
                    if (rand_on)
                    {
                        cur_angle = savedRandAngle.Dequeue();
                    }
                    MyEdge e = EdgeByAngle(cur_point, cur_angle, f_length);
                    draw_queue.Enqueue(e);
                    cur_point = e.end;
                }
                else if (ch == '[')
                {
                    savedAngle.Push(cur_angle);
                    savedPoint.Push(cur_point);
                }
                else if (ch == ']')
                {
                    cur_point = savedPoint.Pop();
                    cur_angle = savedAngle.Pop();
                }
                else if (ch == '@')
                {
                    rand_on = true;
                }
            }
        }

        public void DrawQueue()
        {
            bm = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(bm);
            while (draw_queue.Count > 0)
            {
                var edge = draw_queue.Dequeue();
                g.DrawLine(Pens.Black, edge.start, edge.end);
            }
            pictureBox1.Image = bm;
        }

        //-------------------------------------------------------------------------------

        private void button3_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            pictureBox1.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            int iter = (int)numericUpDown1.Value;
            LSystem(iter);
            DrawQueue();
        }
    }
}

public class MyEdge
{
    public PointF start;
    public PointF end;

    public MyEdge(PointF s, PointF e)
    {
        start = s;
        end = e;
    }

    public MyEdge()
    {
        start = end = new PointF(-1, -1);
    }

    public static bool operator ==(MyEdge e1, MyEdge e2)
    {
        return ((e1.start == e2.start) && (e1.end == e2.end));
    }
    public static bool operator !=(MyEdge e1, MyEdge e2)
    {
        return ((e1.start != e2.start) || (e1.end != e2.end));
    }

}
