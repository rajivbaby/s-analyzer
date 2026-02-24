using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Numerics;
using System.Reflection;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
//using SParameter_Analyzer.Properties;
using ZedGraph;
using static System.Windows.Forms.DataFormats;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace S_Analyzer
{
    public partial class Form1 : Form
    {
        // private List<TTouchstoneFileContainer> _FileList = new List<TTouchstoneFileContainer>();
        private List<SParameterEntry> sParameterData = new List<SParameterEntry>();

        // private ComboBox cbxListOfLoadedFiles;
        private ToolStripMenuItem ToolStripMenuItem_Popup_OpenFile;

        private Dictionary<string, List<SParameterEntry>> sParameterFiles = new Dictionary<string, List<SParameterEntry>>();
        private List<string> fileNames = new List<string>();
        private double[] newColor;
        Random rand = new Random();
        private SParamFormat format;
        private int index;
        private Complex z0;
        private object frequency;
        private int currentFrequency;
        private object frequencyMultiplier;
        private readonly object impedance;
        public string FreqLength { get; }
        public double fm { get; private set; }

        public Form1()
        {
            InitializeComponent();
            this.cmbFiles.Text = " First, load a file/files . . .";
            tabControl1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            zedGraphSmith.MouseClick += zedGraphSmith_MouseClick;
            zedGraphControl1.MouseClick += zedGraphControl1_MouseClick;
            this.Resize += Form1_Resize;
            Form1_Resize(this, EventArgs.Empty); // force initial layout
            //zedGraphSmith.Invalidate();
            zedGraphSmith.MouseMove += zedGraphSmith_MouseMove;
            this.zedGraphControl1.MouseClick += new MouseEventHandler(zedGraphControl1_MouseClick);
            // Hover mouse to see value 
            zedGraphControl1.IsShowPointValues = true;
            zedGraphControl1.PointValueEvent += ZedGraph_PointValueEvent;
            zedGraphSmith.IsShowPointValues = true;
           // zedGraphSmith.PointValueEvent += ZedGraphSmith_PointValueEvent;

        }
        public enum SParamFormat { RI, MA, DB }

        public class SParameter
        {
            public SParameter(double val1, double val2)
            {
                Val1 = val1;
                Val2 = val2;
            }

            public double Val1 { get; set; }  // Real or Magnitude
            public double Val2 { get; set; }  // Imag or Angle (degrees)
            public double[] Real { get; internal set; }
            public double[] Imag { get; internal set; }
        }

        public class SParameterEntry
        {
            public double Frequency { get; set; } // in MHz
            public List<SParameter> SParameters { get; set; } = new();
            public SParamFormat Format { get; set; }

            public SParameterEntry(double frequency, SParamFormat format)
            {
                Frequency = frequency;
                Format = format;
            }

            public SParameterEntry(double frequency, List<SParameter> sParameters)
            {
                Frequency = frequency;
                SParameters = sParameters;
            }
        }


        private void plotToolStripMenuItem_Click(object sender, EventArgs e)

        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Touchstone Files (*.s2p;*.s3p;*.snp)|*.s2p;*.s3p;*.snp|All Files (*.*)|*.*",
                Title = "Select an S-Parameter File"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = openFileDialog.FileName;
                sParameterData = ReadSParameterFile(filePath);
                // PlotSParameters();
            }
        }


        private List<SParameterEntry> ReadSParameterFile(string filePath)


        {

            List<SParameterEntry> dataList = new List<SParameterEntry>();

            if (!File.Exists(filePath))
            {
                MessageBox.Show("File not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return dataList;
            }

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                SParamFormat format = SParamFormat.RI; // Default format

                int numPorts = 0;
                string ext = Path.GetExtension(filePath).ToLower();
                if (ext.StartsWith(".s") && ext.EndsWith("p") && int.TryParse(ext.Substring(2, ext.Length - 3), out int ports))
                {
                    numPorts = ports;
                }
                else
                {
                    MessageBox.Show("Could not detect number of ports from file extension (e.g. .s3p)", "Error");
                    return dataList;
                }

                int expectedValues = numPorts * numPorts * 2;

                List<double> valueBuffer = new List<double>();
                double currentFreq = 0;
                bool headerParsed = false;

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith("!"))
                        continue;

                    if (trimmedLine.StartsWith("#"))
                    {
                        string[] tokens = trimmedLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.Length >= 4)
                        {
                            string formatToken = tokens[3].ToUpper();

                            
                            switch (formatToken)
                            {
                                case "RI": format = SParamFormat.RI; break;
                                case "MA": format = SParamFormat.MA; break;
                                case "DB": format = SParamFormat.DB; break;
                                default:
                                    MessageBox.Show($"Unknown format '{formatToken}', defaulting to RI", "Warning");
                                    format = SParamFormat.RI;
                                    break;
                            }
                        }
                        headerParsed = true;
                        continue;
                    }



                    if (!headerParsed)
                        continue;

                    string[] parts = trimmedLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        if (double.TryParse(part, out double val))
                            valueBuffer.Add(val);
                    }



                    // We need 1 freq + N*N*2 values
                    while (valueBuffer.Count >= (1 + expectedValues))
                    {
                        currentFreq = valueBuffer[0];
                        //double currentFrequency = currentFreq * fm; // Store in Hz
                        
                        List<SParameter> sParams = new List<SParameter>();

                        for (int i = 1; i < 1 + expectedValues; i += 2)
                        {
                            double val1 = valueBuffer[i];
                            double val2 = valueBuffer[i + 1];
                            double real = 0, imag = 0;

                            switch (format)
                            {
                                case SParamFormat.RI:
                                    real = val1;
                                    imag = val2;
                                    break;
                                case SParamFormat.MA:
                                    double mag = val1;
                                    double angleRad = val2 * Math.PI / 180.0;
                                    real = mag * Math.Cos(angleRad);
                                    imag = mag * Math.Sin(angleRad);
                                    break;
                                case SParamFormat.DB:
                                    double linMag = Math.Pow(10, val1 / 20.0);
                                    double angleRadDb = val2 * Math.PI / 180.0;
                                    real = linMag * Math.Cos(angleRadDb);
                                    imag = linMag * Math.Sin(angleRadDb);
                                    break;

                            }

                            sParams.Add(new SParameter(real, imag));
                            // Later in the loop:
                            
                        }
                       // currentFrequency = currentFreq * frequencyMultiplier; // Store in Hz
                        dataList.Add(new SParameterEntry(currentFreq, sParams));
                        valueBuffer.RemoveRange(0, 1 + expectedValues); // Remove used values
                       
                    }
                }
               // MessageBox.Show(Convert.ToString(format));

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return dataList;
            

        }

        private SParameterEntry ParseSParameterEntry(double currentFrequency, List<string> accumulatedData)
        {
            throw new NotImplementedException();
        }

       


        private void cmbFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedFile = cmbFiles.SelectedItem.ToString();
            if (sParameterFiles.ContainsKey(selectedFile))
            {
                List<SParameterEntry> sParameterData = sParameterFiles[selectedFile];

                // Extract available S-parameters
                int numPorts = (int)Math.Sqrt(sParameterData[0].SParameters.Count); // n x n matrix assumption
                cmbPorts.Items.Clear();

                //for (int row = 1; row <= numPorts; row++)
                //{
                //    for (int col = 1; col <= numPorts; col++)
                //    {
                //        cmbPorts.Items.Add($"S{row}{col}");
                //    }
                //}
                for (int i = 1; i <= numPorts; i++)
                {
                    for (int j = 1; j <= numPorts; j++)
                    {
                        cmbPorts.Items.Add($"S{i}{j}");
                    }
                }

                if (cmbPorts.Items.Count > 0)
                {
                    cmbPorts.SelectedIndex = 0; // Select first port by default
                }
            }

        }



        private void cmbFiles_SelectedIndexChanged_1(object sender, EventArgs e)
        {
        


            string selectedFile = cmbFiles.SelectedItem.ToString();
            if (sParameterFiles.ContainsKey(selectedFile))
            {
                List<SParameterEntry> sParameterData = sParameterFiles[selectedFile];
                cmbPorts.Items.Clear();

                if (sParameterData == null || sParameterData.Count == 0)
                    return;

                int count = sParameterData[0].SParameters.Count;
                double sqrt = Math.Sqrt(count);
                int numPorts = (int)sqrt;

                // Validate perfect square
                if (Math.Abs(sqrt - numPorts) > 1e-6)
                {
                    MessageBox.Show("S-parameter data does not represent a square matrix (NxN).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                for (int row = 1; row <= numPorts; row++)
                {
                    for (int col = 1; col <= numPorts; col++)
                    {
                        cmbPorts.Items.Add($"S{row}{col}");
                    }
                }

                if (cmbPorts.Items.Count > 0)
                {
                    cmbPorts.SelectedIndex = 0;
                }
            }


        }


        private void cmbPorts_SelectedIndexChanged(object sender, EventArgs e)
        {
            


        }

        private void PlotSelectedSParameters(List<SParameterEntry> sParameterData, int index)
        {

            //MessageBox.Show(Convert.ToString(format));
            if (sParameterData == null || sParameterData.Count == 0)
                return;

            GraphPane myPane = zedGraphControl1.GraphPane;

            double freqinMHz = 0;

            myPane.Title.Text = $"S-Parameter Plot ({cmbPorts.SelectedItem})";
            myPane.XAxis.Title.Text = "Frequency (MHz)";
            myPane.YAxis.Title.Text = "Magnitude (dB)";
            myPane.YAxis.Type = AxisType.Linear;

            PointPairList pointList = new PointPairList();

           //var format = sParameterData[0].Format; // Assume all entries use the same format
            //MessageBox.Show(Convert.ToString(format));

            foreach (var entry in sParameterData)
            {
                if (index >= 0 && index < entry.SParameters.Count)
                {
                    var param = entry.SParameters[index];
                    var format = sParameterData[1].Format;

                    double magnitude = 0;
                    //double freq = 0;

                    switch (format)
                    {
                        case SParamFormat.RI:
                            // Real + Imaginary → Linear magnitude
                            magnitude = Math.Sqrt(param.Val1 * param.Val1 + param.Val2 * param.Val2);
                            break;
                           // double freqinMHz = entry.Frequency / 1e6;

                        case SParamFormat.MA:
                            // Magnitude is already linear
                            magnitude = param.Val1;
                            break;

                        case SParamFormat.DB:
                            // Convert dB to linear magnitude
                            magnitude = Math.Pow(10, param.Val1 / 20);
                            break;
                    }




                    if (magnitude > 0)
                    {
                        double magnitude_dB = 20 * Math.Log10(magnitude);

                        // Create PointPair with magnitude_dB on Y-axis


                        PointPair pt = new PointPair(entry.Frequency, magnitude_dB)
                        {

                            Tag = entry.Frequency  // Attach frequency to the tag

                         };
                         

                        pointList.Add(pt); 
                    }
                    
                }
            }


            Color newColor = Color.FromArgb(rand.Next(100, 255), rand.Next(100, 255), rand.Next(100, 255));

            string fileName = Path.GetFileNameWithoutExtension(cmbFiles.SelectedItem?.ToString() ?? "Unknown");
            string selectedParam = cmbPorts.SelectedItem?.ToString() ?? "Sxx";

            LineItem curve = myPane.AddCurve($"{selectedParam} ({fileName})", pointList, newColor, SymbolType.None);

            curve.Line.Width = 2.0f;

            myPane.XAxis.MajorGrid.IsVisible = true;
            myPane.YAxis.MajorGrid.IsVisible = true;

            double minFreq = sParameterData.First().Frequency;
            double maxFreq = sParameterData.Last().Frequency;

            myPane.XAxis.Scale.Min = minFreq;
            myPane.XAxis.Scale.Max = maxFreq;

            //curve.Line.IsSmooth = true;
            //curve.Line.SmoothTension = 0.5F; // Optional: controls curve tightness

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
           
        }

        private void btnClearPane_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "Cartesian Plot")
            {
                GraphPane myPane = zedGraphControl1.GraphPane;
                myPane.CurveList.Clear();
                zedGraphControl1.AxisChange();
                zedGraphControl1.Invalidate();
                myPane.XAxis.Scale.MinAuto = true;
                myPane.XAxis.Scale.MaxAuto = true;
                myPane.YAxis.Scale.MinAuto = true;
                myPane.YAxis.Scale.MaxAuto = true;
                myPane.CurveList.Clear();
                myPane.GraphObjList.Clear();
            }
            else
            {

                GraphPane mySmith = zedGraphSmith.GraphPane;
                mySmith.CurveList.Clear();
                mySmith.GraphObjList.Clear();
                zedGraphSmith.AxisChange();
                zedGraphSmith.Invalidate();
            }

        }

        private void btnPlot_Click(object sender, EventArgs e)
        {
            if (cmbFiles.SelectedItem == null || cmbPorts.SelectedItem == null)
                return;

            string selectedFile = cmbFiles.SelectedItem.ToString();
            string selectedPort = cmbPorts.SelectedItem.ToString();

            if (sParameterFiles.ContainsKey(selectedFile))
            {
                List<SParameterEntry> sParameterData = sParameterFiles[selectedFile];

                int row = int.Parse(selectedPort.Substring(1, 1)) - 1; // Extract row index
                int col = int.Parse(selectedPort.Substring(2, 1)) - 1; // Extract column index
                int index = row * (int)Math.Sqrt(sParameterData[0].SParameters.Count) + col;

                if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Text == "Cartesian Plot")

                {

                    PlotSelectedSParameters(sParameterData, index);
                }
                else
                {


                    int numPorts = (int)Math.Sqrt(sParameterFiles[selectedFile][0].SParameters.Count);
                    int selectedIndex = row * numPorts + col;

                    Complex z0 = new Complex(50, 0); // 50-ohm system

                    PlotImpedanceMagnitude(sParameterFiles[selectedFile], selectedIndex, z0);

                }
            }

        }

        private void selectFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Touchstone Files (*.s2p;*.s3p;*.snp)|*.s2p;*.s3p;*.snp|All Files (*.*)|*.*",
                Title = "Select S-Parameter Files",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                fileNames.Clear();
                sParameterFiles.Clear();
                cmbFiles.Items.Clear();

                foreach (string filePath in openFileDialog.FileNames)
                {
                    string fileName = Path.GetFileName(filePath);
                    List<SParameterEntry> sData = ReadSParameterFile(filePath);
                    if (sData.Count > 0)
                    {
                        sParameterFiles[fileName] = sData;
                        fileNames.Add(fileName);
                        cmbFiles.Items.Add(fileName);
                    }
                }

                if (cmbFiles.Items.Count > 0)
                {
                    cmbFiles.SelectedIndex = 0;
                }
            }
        }



        private void btnSmith_Click(object sender, EventArgs e)
        {

            string selectedFile = cmbFiles.SelectedItem.ToString();

            string selectedPort = cmbPorts.SelectedItem.ToString(); // "S11"
            int row = int.Parse(selectedPort.Substring(1, 1)) - 1;
            int col = int.Parse(selectedPort.Substring(2, 1)) - 1;
            int numPorts = (int)Math.Sqrt(sParameterFiles[selectedFile][0].SParameters.Count);
            int selectedIndex = row * numPorts + col;

            Complex z0 = new Complex(50, 0); // 50-ohm system

            PlotImpedanceMagnitude(sParameterFiles[selectedFile], selectedIndex, z0);

        }

        private void PlotImpedanceMagnitude(List<SParameterEntry> sParameterData, int selectedIndex, Complex z0)
        {
            if (sParameterData == null || sParameterData.Count == 0)
                return;

            GraphPane pane = zedGraphSmith.GraphPane;
            //pane.CurveList.Clear();
            pane.GraphObjList.Clear();

            pane.Title.Text = "Impedance";
            //pane.XAxis.Title.Text = "Frequency (Hz)";
            //pane.YAxis.Title.Text = "|Z| (Ohms)";
            pane.XAxis.Title.Text = "";
            pane.YAxis.Title.Text = "";

            pane.XAxis.MajorGrid.IsVisible = true;
            pane.YAxis.MajorGrid.IsVisible = true;



            PointPairList pointList = new PointPairList();
            PointPairList pointList1 = new PointPairList();
            foreach (var entry in sParameterData)
            {
                if (selectedIndex >= entry.SParameters.Count)
                    continue;

                var s = entry.SParameters[selectedIndex];
                Complex gamma;

                // Convert S-parameter format to complex reflection coefficient Γ
                switch (entry.Format)
                {
                    case SParamFormat.MA:
                        double mag = s.Val1;
                        double angleRad = s.Val2 * Math.PI / 180.0;
                        gamma = Complex.FromPolarCoordinates(mag, angleRad);
                        break;

                    case SParamFormat.DB:
                        double linearMag = Math.Pow(10, s.Val1 / 20.0);
                        double angleDbRad = s.Val2 * Math.PI / 180.0;
                        gamma = Complex.FromPolarCoordinates(linearMag, angleDbRad);
                        break;

                    case SParamFormat.RI:
                    default:
                        gamma = new Complex(s.Val1, s.Val2);
                        break;
                }

                

                // Convert Γ to Impedance
                Complex z = z0 * (1 + gamma) / (1 - gamma);

                // Normalize impedance for Smith Chart
                Complex zNorm = z / z0;

               

                // Convert impedance back to Γ for Smith chart ! FIX
                Complex gammaForPlot = (z - z0) / (z + z0);

                pointList.Add(gammaForPlot.Real, gammaForPlot.Imaginary);



                PointPair pt = new PointPair(gamma.Real, gamma.Imaginary)
                {
                    Tag = Tuple.Create(entry.Frequency, z)
                };

                pointList.Add(pt);


            }



            DrawResistanceCircles(pane);

            double[] reactanceValues = { 10, 25, 50, 100, 150 };
            foreach (double x in reactanceValues)
            {
                DrawReactanceArc(pane, x, true);  // upper arc
                DrawReactanceArc(pane, x, false); // lower arc
            }

            pane.XAxis.Scale.Min = -1.1;
            pane.XAxis.Scale.Max = 1.1;
            pane.YAxis.Scale.Min = -1.1;
            pane.YAxis.Scale.Max = 1.1;

            // Hide X Axis
            pane.XAxis.Title.IsVisible = false;
            pane.XAxis.Scale.IsVisible = false;
            pane.XAxis.MajorTic.IsOutside = false;
            pane.XAxis.MinorTic.IsOutside = false;
            pane.XAxis.MajorTic.IsInside = false;
            pane.XAxis.MinorTic.IsInside = false;

            // Hide Y Axis
            pane.YAxis.Title.IsVisible = false;
            pane.YAxis.Scale.IsVisible = false;
            pane.YAxis.MajorTic.IsOutside = false;
            pane.YAxis.MinorTic.IsOutside = false;
            pane.YAxis.MajorTic.IsInside = false;
            pane.YAxis.MinorTic.IsInside = false;


            Color newColor = Color.FromArgb(rand.Next(100, 255), rand.Next(100, 255), rand.Next(100, 255));

            string fileName = Path.GetFileNameWithoutExtension(cmbFiles.SelectedItem?.ToString() ?? "Unknown");
            string selectedParam = cmbPorts.SelectedItem?.ToString() ?? "Sxx";

            LineItem curve = pane.AddCurve($"{selectedParam} ({fileName})", pointList, newColor, SymbolType.None);
            curve.Line.Width = 2.5f;

            //curve.Line.IsSmooth = true;
            curve.Line.SmoothTension = 0.5F; // Optional: controls curve tightness

            zedGraphSmith.AxisChange();
            zedGraphSmith.Invalidate();

        }

        private void zedGraphSmith_MouseMove(object sender, MouseEventArgs e)
        {
            GraphPane pane = zedGraphSmith.GraphPane;

            double x, y;
            pane.ReverseTransform(e.Location, out x, out y); // Converts screen coords to graph coords

            // Check if point is inside the Smith Chart unit circle
            if (x * x + y * y <= 1.0)
            {
                zedGraphSmith.IsShowPointValues = false; // We override default behavior

                // Create formatted text
                string realPart = x.ToString("0.000");
                string imagPart = y.ToString("0.000");

                //string info = $"Re: {realPart}, Im: {imagPart}";
                string info = $"Re: {realPart}, Im: {imagPart}";

                
            }
            else
            {
               //label1.Text = "Outside Smith Chart";
            }
        }



        private void DrawReactanceArc(GraphPane pane, double x, bool upper)
        {
            PointPairList arcPoints = new PointPairList();

            double rMin = 0.01;
            double rMax = 1000;
            int numPoints = 500;
            double z0 = 50.0;

            for (int i = 0; i <= numPoints; i++)
            {
                double r = rMin + (rMax - rMin) * i / numPoints;
                Complex z = new Complex(r, upper ? x : -x);
                Complex gamma = (z / z0 - 1) / (z / z0 + 1);
                arcPoints.Add(gamma.Real, gamma.Imaginary);
            }

            LineItem arc = pane.AddCurve(null, arcPoints, Color.Black, SymbolType.None);
            arc.Line.Width = 1.0f;
            arc.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
        }


        private void zedGraphControl1_MouseClick(object sender, MouseEventArgs e)
        {

            GraphPane pane = zedGraphControl1.GraphPane;
            double x, y;
            pane.ReverseTransform(e.Location, out x, out y);

            CurveItem nearestCurve;
            int pointIndex;

            if (pane.FindNearestPoint(e.Location, out nearestCurve, out pointIndex))
            {
                if (nearestCurve != null && pointIndex >= 0)
                {
                    // pane.GraphObjList.RemoveAll(obj => obj is TextObj);

                    PointPair pt = nearestCurve[pointIndex];

                    double freqHz = (pt.Tag is double tagFreq) ? tagFreq : pt.X;
                    
                    double freq = 0;

                    
                    string LenthFreq = Convert.ToString(freqHz);

                    int decimalIndex = LenthFreq.IndexOf('.');
                    int lengthofFreq = (decimalIndex >= 0) ? decimalIndex : LenthFreq.Length;



                    if (lengthofFreq > 4)
                    {
                        freq = pt.X / 1e6;

                    }
                    else
                    {
                        freq = pt.X;
                    }

                    //freq = freqHz / 1e6;
                    double freqMHz = freq;

                    // Magnitude to dB (assumes voltage ratio)
                    double magnitude = pt.Y;
                    //double dBm = 20 * Math.Log10(magnitude);
                    double dBm = magnitude;

                    // Display label
                    string label = $"Freq: {freqMHz:F2} MHz\nMag: {dBm:F2} dB";

                    TextObj text = new TextObj(label, pt.X, pt.Y, CoordType.AxisXYScale, AlignH.Left, AlignV.Top)
                    {
                        FontSpec = new FontSpec("Arial", 10, Color.DarkBlue, false, false, false),
                        ZOrder = ZOrder.A_InFront
                    };

                    pane.GraphObjList.Add(text);
                    zedGraphControl1.Invalidate();

                }

            }

        }



        private void Form1_Resize(object sender, EventArgs e)
        {

            int size = Math.Abs(this.tabControl1.Height - 45);


            // Set ZedGraph control size
            zedGraphSmith.Width = size;
            zedGraphSmith.Height = size;

            // Optionally center it

            zedGraphSmith.Invalidate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Your initialization logic

            this.Resize += Form1_Resize;
            Form1_Resize(this, EventArgs.Empty); // force initial layout
        }

        private void DrawResistanceCircles(GraphPane pane)
        {
            double[] rValues = new double[] { 0, 0.2, 0.5, 1, 2, 5 }; // Normalized resistance values

            foreach (double r in rValues)
            {
                double radius = 1.0 / (1.0 + r);
                double centerX = r / (1.0 + r);
                double centerY = 0;

                // Create points for a full circle (or semicircle if needed)
                PointPairList circlePoints = new PointPairList();
                for (double angle = 0; angle <= 360; angle += 5)
                {
                    double radians = angle * Math.PI / 180.0;
                    double x = centerX + radius * Math.Cos(radians);
                    double y = centerY + radius * Math.Sin(radians);
                    circlePoints.Add(x, y);
                }

                LineItem circle = pane.AddCurve("", circlePoints, Color.Gray, SymbolType.None);
                circle.Line.Width = 1;
                circle.Line.Style = System.Drawing.Drawing2D.DashStyle.Dash;
            }
        }


        private void zedGraphSmith_MouseClick(object sender, MouseEventArgs e)
        {


            GraphPane pane = zedGraphSmith.GraphPane;

            double x, y;
            pane.ReverseTransform(e.Location, out x, out y);

            //CurveItem nearestCurve;
            int pointIndex;

            // Set pixel tolerance (e.g., 10 pixels)
            //int tolerance = 20;


            PointF mousePt = new PointF(e.X, e.Y);
            double minDist = double.MaxValue;
            CurveItem nearestCurve = null;
            int nearestIndex = -1;

            const float tolerance = 20f; // pixels

            foreach (CurveItem curve in pane.CurveList)
            {
                for (int i = 0; i < curve.NPts; i++)
                {
                    PointPair pt = curve[i];
                    if (pt == null || pt.X == PointPair.Missing || pt.Y == PointPair.Missing)
                        continue;

                    // Convert point to screen coordinates
                    PointF screenPt = pane.GeneralTransform(new PointF((float)pt.X, (float)pt.Y), CoordType.AxisXYScale);

                    float dx = screenPt.X - mousePt.X;
                    float dy = screenPt.Y - mousePt.Y;
                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    if (dist < minDist && dist <= tolerance)
                    {
                        minDist = dist;
                        nearestCurve = curve;
                        nearestIndex = i;
                    }
                }
            }




            if (pane.FindNearestPoint(e.Location, out nearestCurve, out pointIndex))
            {
                if (nearestCurve != null && pointIndex >= 0)
                {
                    // Clear previous labels
                    //pane.GraphObjList.RemoveAll(obj => obj is TextObj);

                    PointPair pt = nearestCurve[pointIndex];

                    // Extract frequency and impedance from Tag
                    if (pt.Tag is Tuple<double, Complex> tag)
                    {


                        double freq = 0;


                        string LenthFreq = Convert.ToString(tag.Item1);

                        int decimalIndex = LenthFreq.IndexOf('.');
                        int lengthofFreq = (decimalIndex >= 0) ? decimalIndex : LenthFreq.Length;



                        if (lengthofFreq > 4)
                        {
                            freq = tag.Item1 / 1e6;

                        }
                        else
                        {
                            freq = tag.Item1;
                        }

                        //freq = freqHz / 1e6;
                        double freqMHz = freq;

                        Complex z = tag.Item2;
                        double magnitude = z.Magnitude;

                        //string label = $"Freq: {freqMHz:F2} MHz\n|Z|: {magnitude:F2} Ω";
                        string label = $"Freq: {freqMHz:F2} MHz\nZ = {z.Real:F2} + j{z.Imaginary:F2} Ω";
                        TextObj text = new TextObj(label, pt.X, pt.Y, CoordType.AxisXYScale, AlignH.Left, AlignV.Top)
                        {
                            FontSpec = new FontSpec("Arial", 10, Color.DarkBlue, false, false, false),
                            ZOrder = ZOrder.A_InFront
                        };
                        text.FontSpec.Border.IsVisible = false;
                        text.FontSpec.Fill.IsVisible = true;
                        text.FontSpec.Fill.Color = Color.FromArgb(255, 255, 255, 220);

                        pane.GraphObjList.Add(text);
                        zedGraphSmith.Invalidate();
                    }
                }
            }


        }

// Hover mouse to show value
        private string ZedGraph_PointValueEvent(
        ZedGraphControl control,
        GraphPane pane,
        CurveItem curve,
        int iPt)
         {
            PointPair pt = curve[iPt];

            // X-axis value (frequency)
            double freqMHz = pt.X; // assuming X already in MHz
           // double freqMHz = pt.X / 100000;


            // Y-axis value (magnitude / impedance / etc.)
            double value = pt.Y;

            return $"Freq: {freqMHz:F2} MHz\nValue: {value:F2}";
         }

       


        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
           

            string message = "RF Page S-Parameter Analyzer" + Environment.NewLine +
                "" + Environment.NewLine +
                 "Supported File Formats:" + Environment.NewLine +
                 "RI, MA & DB" + Environment.NewLine +
                 "Supported Plot Formats:" + Environment.NewLine +
                 "Cartesian (Magnitude) and Smith Chart" + Environment.NewLine +
                 "" + Environment.NewLine +
                 "Visit: www.rfpage.com/s-analyzer for more info";

            MessageBox.Show(message, "S-Parameter Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}



    public class SParameterEntry
    {
        public double Frequency { get; }
       // public List<ComplexNumber> SParameters { get; }
        public List<ComplexNumber> SParameters { get; set; }

    public SParameterEntry(double frequency, List<ComplexNumber> sParameters)
        {
            Frequency = frequency;
            SParameters = sParameters;
        }
    }



public class ComplexNumber
{
    public double Real { get; }
    public double Imag { get; }
  
    public ComplexNumber(double real, double imag)
    {
        Real = real;
        Imag = imag;
    }
}






