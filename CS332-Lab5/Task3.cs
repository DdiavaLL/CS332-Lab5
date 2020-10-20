using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CS332_Lab5
{
    public partial class Task3 : Form
    {
        private Graphics g;
        private List<PointF> points = new List<PointF>();
        private List<PointF> pointsFirst = new List<PointF>();
        int pointToChangeInd = -1;
        bool SetNewPoint = false;
        public Task3()
        {
            InitializeComponent();
            pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(pictureBox1.Image);
            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            button3.Enabled = button4.Enabled = false;
        }

        bool IsInRange(float numberToCheck, float bottom, float top) => (numberToCheck >= bottom && numberToCheck <= top);

        private void PictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            var isChangingPoint = false;

            if (button3.Enabled == true)
                button3.Enabled = button4.Enabled = false;

            for (int i = 0; i < points.Count; i++)
            {
                PointF p = points[i];
                if (IsInRange(e.X, p.X - 2.5f, p.X + 2.5f))
                {
                    isChangingPoint = true;
                    pointToChangeInd = i;
                }
            }

            if (!SetNewPoint)
            {
                if (isChangingPoint && pointToChangeInd > -1)
                    button3.Enabled = button4.Enabled = true;
                else
                {
                    if (points.Count % 3 == 0 && points.Count >= 4)
                    {
                        points.Add(MiddlePoint(e.Location, points[points.Count - 1]));
                        points.Add(e.Location);
                    }
                    else
                        points.Add(e.Location);
                    g.FillEllipse(new SolidBrush(Color.Red), e.X - 2.5f, e.Y - 2.5f, 5, 5);
                    pointsFirst.Add(e.Location);
                }
            }
            else
            {
                var temp = pointsFirst.FindIndex(p => p == points[pointToChangeInd]);
                points[pointToChangeInd] = e.Location;
                if (temp >= 0)
                    pointsFirst[temp] = e.Location;
                SetNewPoint = false;

                RecountPoints();

                button1_Click(sender, e);
            }

            pictureBox1.Invalidate();

            if (points.Count > 4)
                RecountPoints();
        }

        private int Fuctorial(int n)
        {
            int res = 1;
            for (int i = 1; i <= n; i++)
                res *= i;
            return res;
        }

        float polinom(int i, int n, float t) => ((float)Fuctorial(n) / (Fuctorial(i) * Fuctorial(n - i))) * (float)Math.Pow(t, i) * (float)Math.Pow(1 - t, n - i);

        void thirdBezier(List<PointF> pointFs)
        {
            int j = 0;
            float step = 0.01f;

            PointF[] result = new PointF[101];
            for (float t = 0; t <= 1; t += step)
            {
                float ytmp = 0;
                float xtmp = 0;
                for (int i = 0; i < pointFs.Count; i++)
                {
                    float b = polinom(i, pointFs.Count - 1, t);
                    xtmp += pointFs[i].X * b;
                    ytmp += pointFs[i].Y * b;
                }
                result[j] = new PointF(xtmp, ytmp);
                j++;
            }
            g.DrawLines(new Pen(Color.Blue), result);
            pictureBox1.Invalidate();
        }

        private IEnumerable<IEnumerable<PointF>> SplitPoints(IEnumerable<PointF> source, int count)
        {
            return source.Select((x, y) => new { Index = y, Value = x })
              .GroupBy(x => x.Index / count)
              .Select(x => x.Select(y => y.Value).ToList())
              .ToList();
        }

        private PointF MiddlePoint(PointF first, PointF second) => new PointF((first.X + second.X) / 2, (first.Y + second.Y) / 2);

        private void button2_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            pointsFirst.Clear();
            points.Clear();
            pictureBox1.Invalidate();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if ((pointToChangeInd - 1) % 3 == 0 && pointToChangeInd >= 3)
            {
                var temp = pointsFirst.FindIndex(p => p == points[pointToChangeInd]);

                points.RemoveAt(pointToChangeInd);
                pointsFirst.RemoveAt(temp);

                if (points.Count >= 4)
                    points.RemoveAt(pointToChangeInd - 1);
            }
            else if ((pointToChangeInd + 1) % 3 == 0)
            {
                var temp = pointsFirst.FindIndex(p => p == points[pointToChangeInd]);

                points.RemoveAt(pointToChangeInd);
                pointsFirst.RemoveAt(temp);

                if (points.Count >= 4)
                {
                    points.RemoveAt(pointToChangeInd);
                    RecountPoints();
                }
            }
            else
            {
                points.RemoveAt(pointToChangeInd);
                pointsFirst.RemoveAt(pointToChangeInd);
                RecountPoints();
            }
            button1_Click(sender, e);
            button3.Enabled = button4.Enabled = false;
        }

        private void RecountPoints()
        {
            points = points.Intersect<PointF>(pointsFirst).ToList();

            if (points.Count > 4)
                for (int i = 0; i < points.Count; i++)
                    if (i % 3 == 0 && i >= 3)
                        points.Insert(i, MiddlePoint(points[i - 1], points[i]));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SetNewPoint = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            g.Clear(Color.White);

            var b = SplitPoints(points, 3);

            for (int i = 0; i < b.Count(); i++)
            {
                List<PointF> tmp = new List<PointF>();
                foreach (var v in b.ElementAt(i))
                    tmp.Add(v);
                if (i + 1 < b.Count())
                    tmp.Add(b.ElementAt(i + 1).ElementAt(0));

                thirdBezier(tmp);
            }

            foreach (var p in points)
            {
                if (pointsFirst.Contains(p))
                    g.FillEllipse(new SolidBrush(Color.Red), p.X - 2.5f, p.Y - 2.5f, 5, 5);
                else
                    g.FillEllipse(new SolidBrush(Color.Gray), p.X - 2f, p.Y - 2f, 4, 4);
            }
            pictureBox1.Invalidate();
        }
    }
}
