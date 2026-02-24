
namespace S_Analyzer
{
    partial class Form1
    {
    
        private System.ComponentModel.IContainer components = null;

        
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            menuStrip1 = new MenuStrip();
            selectFilesToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            zedGraphControl1 = new ZedGraph.ZedGraphControl();
            cmbFiles = new ComboBox();
            cmbPorts = new ComboBox();
            btnClearPane = new Button();
            lblSelectFile = new Label();
            label1 = new Label();
            btnPlot = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            zedGraphSmith = new ZedGraph.ZedGraphControl();
            pictureBox2 = new PictureBox();
            menuStrip1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { selectFilesToolStripMenuItem, aboutToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(857, 31);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // selectFilesToolStripMenuItem
            // 
            selectFilesToolStripMenuItem.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            selectFilesToolStripMenuItem.Name = "selectFilesToolStripMenuItem";
            selectFilesToolStripMenuItem.Size = new Size(91, 27);
            selectFilesToolStripMenuItem.Text = "Load File";
            selectFilesToolStripMenuItem.TextAlign = ContentAlignment.TopLeft;
            selectFilesToolStripMenuItem.Click += selectFilesToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(71, 27);
            aboutToolStripMenuItem.Text = "About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // zedGraphControl1
            // 
            zedGraphControl1.Dock = DockStyle.Fill;
            zedGraphControl1.Location = new Point(3, 3);
            zedGraphControl1.Margin = new Padding(4, 5, 4, 5);
            zedGraphControl1.Name = "zedGraphControl1";
            zedGraphControl1.ScrollGrace = 0D;
            zedGraphControl1.ScrollMaxX = 0D;
            zedGraphControl1.ScrollMaxY = 0D;
            zedGraphControl1.ScrollMaxY2 = 0D;
            zedGraphControl1.ScrollMinX = 0D;
            zedGraphControl1.ScrollMinY = 0D;
            zedGraphControl1.ScrollMinY2 = 0D;
            zedGraphControl1.Size = new Size(842, 449);
            zedGraphControl1.TabIndex = 1;
            // 
            // cmbFiles
            // 
            cmbFiles.FormattingEnabled = true;
            cmbFiles.Location = new Point(66, 60);
            cmbFiles.Name = "cmbFiles";
            cmbFiles.Size = new Size(279, 28);
            cmbFiles.TabIndex = 2;
            cmbFiles.SelectedIndexChanged += cmbFiles_SelectedIndexChanged;
            // 
            // cmbPorts
            // 
            cmbPorts.FormattingEnabled = true;
            cmbPorts.Location = new Point(368, 59);
            cmbPorts.Name = "cmbPorts";
            cmbPorts.Size = new Size(79, 28);
            cmbPorts.TabIndex = 3;
            cmbPorts.SelectedIndexChanged += cmbPorts_SelectedIndexChanged;
            // 
            // btnClearPane
            // 
            btnClearPane.Location = new Point(595, 59);
            btnClearPane.Name = "btnClearPane";
            btnClearPane.Size = new Size(94, 29);
            btnClearPane.TabIndex = 4;
            btnClearPane.Text = "Clear";
            btnClearPane.UseVisualStyleBackColor = true;
            btnClearPane.Click += btnClearPane_Click;
            // 
            // lblSelectFile
            // 
            lblSelectFile.AutoSize = true;
            lblSelectFile.Location = new Point(64, 37);
            lblSelectFile.Name = "lblSelectFile";
            lblSelectFile.Size = new Size(76, 20);
            lblSelectFile.TabIndex = 5;
            lblSelectFile.Text = "Select File";
            //lblSelectFile.Click += lblSelectFile_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(368, 34);
            label1.Name = "label1";
            label1.Size = new Size(79, 20);
            label1.TabIndex = 6;
            label1.Text = "Select Port";
            // 
            // btnPlot
            // 
            btnPlot.Location = new Point(471, 59);
            btnPlot.Name = "btnPlot";
            btnPlot.Size = new Size(94, 29);
            btnPlot.TabIndex = 7;
            btnPlot.Text = "Plot";
            btnPlot.UseVisualStyleBackColor = true;
            btnPlot.Click += btnPlot_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(0, 108);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(856, 488);
            tabControl1.TabIndex = 9;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(zedGraphControl1);
            tabPage1.Location = new Point(4, 29);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(848, 455);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Cartesian Plot";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(zedGraphSmith);
            tabPage2.Location = new Point(4, 29);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(848, 455);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Smith Chart";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // zedGraphSmith
            // 
            zedGraphSmith.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            zedGraphSmith.Location = new Point(173, 5);
            zedGraphSmith.Margin = new Padding(4, 5, 4, 5);
            zedGraphSmith.Name = "zedGraphSmith";
            zedGraphSmith.ScrollGrace = 0D;
            zedGraphSmith.ScrollMaxX = 0D;
            zedGraphSmith.ScrollMaxY = 0D;
            zedGraphSmith.ScrollMaxY2 = 0D;
            zedGraphSmith.ScrollMinX = 0D;
            zedGraphSmith.ScrollMinY = 0D;
            zedGraphSmith.ScrollMinY2 = 0D;
            zedGraphSmith.Size = new Size(517, 442);
            zedGraphSmith.TabIndex = 0;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = (Image)resources.GetObject("pictureBox2.Image");
            pictureBox2.Location = new Point(7, 47);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(51, 47);
            pictureBox2.TabIndex = 11;
            pictureBox2.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ButtonHighlight;
            ClientSize = new Size(857, 599);
            Controls.Add(pictureBox2);
            Controls.Add(tabControl1);
            Controls.Add(btnPlot);
            Controls.Add(label1);
            Controls.Add(lblSelectFile);
            Controls.Add(btnClearPane);
            Controls.Add(cmbPorts);
            Controls.Add(cmbFiles);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(800, 600);
            Name = "Form1";
            Text = "RF Page S-Analyzer";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }



        #endregion

        private MenuStrip menuStrip1;
        private ZedGraph.ZedGraphControl zedGraphControl1;
        private ToolStripMenuItem selectFilesToolStripMenuItem;
        private ComboBox cmbFiles;
        private ComboBox cmbPorts;
        private Button btnClearPane;
        private Label lblSelectFile;
        private Label label1;
        private Button btnPlot;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private ZedGraph.ZedGraphControl zedGraphSmith;
        private PictureBox pictureBox2;
    }
}
