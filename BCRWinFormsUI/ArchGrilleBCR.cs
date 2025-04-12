using BCR.Library;

namespace BCRWinFormsUI;

public partial class ArchGrilleBCR : Form
{
    public ArchGrilleBCR()
    {
        InitializeComponent();
    }

    private void CheckFolder_Click(object sender, EventArgs e)
    {
        InitialSweep.CheckFolderForPDF("\\\\ARCH-FRIGATE\\Scans\\BCR Test");
    }
}
