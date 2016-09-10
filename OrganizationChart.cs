using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace VisualiazdorLogica
{

    // No encontré ningún control winform decente 
    // Así que tuve que hacerlo yo mismo
    public static class OrganizationChart
    {
        public static Bitmap Generate(ChartNode root, int width)
        {
            const int picHeight = 70;

            var ourLevel = new Bitmap(width, picHeight);

            Font txtFont = new Font(FontFamily.GenericSansSerif, 14f, FontStyle.Regular);

            SizeF strSize;
            using (var g = Graphics.FromImage(ourLevel))
            {
                strSize = g.MeasureString(root.Text, txtFont);

                g.DrawRectangle(Pens.Black, width / 2 - strSize.Width / 2 - 5, 5, strSize.Width + 10, strSize.Height + 10);
                g.DrawString(root.Text, txtFont, Brushes.Black, width / 2 - strSize.Width / 2, 10);
            }

            Bitmap[] childPics = new Bitmap[root.Children.Count];
            for (int i = 0; i < root.Children.Count; i++)
            {
                childPics[i] = Generate(root.Children[i], width / root.Children.Count);
            }

            int maxHeight = 0;

            if (root.Children.Count > 0)
                maxHeight = childPics.Max(bm => bm.Height);

            var newBitmap = new Bitmap(width, picHeight + maxHeight);

            using (var g = Graphics.FromImage(newBitmap))
            {
                g.DrawImage(ourLevel, 0, 0);

                for (int i = 0; i < childPics.Length; i++)
                {
                    g.DrawImage(childPics[i], i * (width / root.Children.Count), picHeight);

                    g.DrawLine(Pens.Black, width / 2, strSize.Height + 15, i * (width / root.Children.Count) + (width / root.Children.Count) / 2, picHeight + 5);

                    childPics[i].Dispose();
                }
            }

            ourLevel.Dispose();
            txtFont.Dispose();

            return newBitmap;
        }
    }

    public class ChartNode
    {
        public ChartNode(string txt)
        {
            Text = txt;
            Children = new List<ChartNode>();
        }

        public string Text { get; set; }
        public List<ChartNode> Children { get; set; }
    }
}