using IronOcr;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace iron_ocr2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

			string file = @"C:\temp\test.jpg"
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            OCR_API o = new OCR_API(file);
            Debug.WriteLine(o.iron_ocr_perf());

            watch.Stop();
            Debug.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
        }
        
    }
}
