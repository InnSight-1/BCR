namespace BCRWinFormsUI;

partial class ArchGrilleBCR
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        CheckFolder = new Button();
        ProgressTextBox = new RichTextBox();
        statusStrip1 = new StatusStrip();
        toolStripStatusLabel1 = new ToolStripStatusLabel();
        statusStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // CheckFolder
        // 
        CheckFolder.Location = new Point(12, 396);
        CheckFolder.Name = "CheckFolder";
        CheckFolder.Size = new Size(75, 23);
        CheckFolder.TabIndex = 0;
        CheckFolder.Text = "Start";
        CheckFolder.UseVisualStyleBackColor = true;
        CheckFolder.Click += CheckFolder_Click;
        // 
        // ProgressTextBox
        // 
        ProgressTextBox.Location = new Point(38, 46);
        ProgressTextBox.Name = "ProgressTextBox";
        ProgressTextBox.ReadOnly = true;
        ProgressTextBox.Size = new Size(726, 331);
        ProgressTextBox.TabIndex = 1;
        ProgressTextBox.Text = "";
        // 
        // statusStrip1
        // 
        statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel1 });
        statusStrip1.Location = new Point(0, 428);
        statusStrip1.Name = "statusStrip1";
        statusStrip1.Size = new Size(800, 22);
        statusStrip1.TabIndex = 2;
        statusStrip1.Text = "Status";
        // 
        // toolStripStatusLabel1
        // 
        toolStripStatusLabel1.Name = "toolStripStatusLabel1";
        toolStripStatusLabel1.Size = new Size(118, 17);
        toolStripStatusLabel1.Text = "toolStripStatusLabel1";
        // 
        // ArchGrilleBCR
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(800, 450);
        Controls.Add(statusStrip1);
        Controls.Add(ProgressTextBox);
        Controls.Add(CheckFolder);
        Name = "ArchGrilleBCR";
        Text = "ArchGrilleBCR";
        statusStrip1.ResumeLayout(false);
        statusStrip1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Button CheckFolder;
    private RichTextBox ProgressTextBox;
    private StatusStrip statusStrip1;
    private ToolStripStatusLabel toolStripStatusLabel1;
}
