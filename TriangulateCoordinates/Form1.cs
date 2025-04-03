using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace TriangulateCoordinates
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private float outLierSize = 10f;

        private void Form1_Load(object sender, EventArgs e)
        {
            string filePath = Path.Combine(Application.StartupPath, "MasterCoordinates.txt");

            if (File.Exists(filePath))
            {
                List<PointF> coordinates = ReadCoordinatesFromFile(filePath);
                if (coordinates.Count > 0)
                {
                    List<PointF> outliers = FindOutliers(coordinates, outLierSize);
                    List<PointF> inliers = coordinates.Except(outliers).ToList();

                    if (outliers.Count > 0)
                    {
                        Console.WriteLine("Outliers (Removed from Average):");
                        foreach (var point in outliers)
                        {
                            Console.WriteLine($"({point.X}, {point.Y})");
                        }

                        // Output outliers to the LastOutliersOutput.txt file
                        WriteOutliersToFile(outliers);
                    }

                    if (inliers.Count > 0)
                    {
                        PointF center = CalculateCentroid(inliers);
                        Console.WriteLine($"Adjusted Center Coordinate: ({center.X}, {center.Y})" + " Outlier size: (any long or lat greater than) " + outLierSize.ToString());
                        MessageBox.Show($"Adjusted Center Coordinate: ({center.X}, {center.Y})" + " Outlier size: (any long or lat greater than) " + outLierSize.ToString());
                    }
                    else
                    {
                        Console.WriteLine("No valid inliers remaining after filtering outliers.");
                        MessageBox.Show("No valid inliers remaining after filtering outliers.", "Error");
                    }
                }
                else
                {
                    MessageBox.Show("No valid coordinates found in the file.", "Error");
                }
            }
            else
            {
                MessageBox.Show("MasterCoordinates.txt not found!", "Error");
            }
        }

        private List<PointF> ReadCoordinatesFromFile(string filePath)
        {
            List<PointF> coordinates = new List<PointF>();
            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(',');
                if (parts.Length == 2 && float.TryParse(parts[0], out float x) && float.TryParse(parts[1], out float y))
                {
                    coordinates.Add(new PointF(x, y));
                }
            }
            return coordinates;
        }

        private PointF CalculateCentroid(List<PointF> points)
        {
            float sumX = points.Sum(p => p.X);
            float sumY = points.Sum(p => p.Y);
            return new PointF(sumX / points.Count, sumY / points.Count);
        }

        private List<PointF> FindOutliers(List<PointF> points, float threshold)
        {
            PointF centroid = CalculateCentroid(points);
            return points.Where(p => Math.Abs(p.X - centroid.X) > threshold || Math.Abs(p.Y - centroid.Y) > threshold).ToList();
        }

        private void WriteOutliersToFile(List<PointF> outliers)
        {
            string outliersFilePath = Path.Combine(Application.StartupPath, "LastOutliersOutput.txt");

            using (StreamWriter writer = new StreamWriter(outliersFilePath))
            {
                writer.WriteLine("Outliers (Removed from Average):");
                foreach (var point in outliers)
                {
                    writer.WriteLine($"({point.X}, {point.Y})");
                }
            }

            Console.WriteLine($"Outliers written to: {outliersFilePath}");
        }
    }
}
