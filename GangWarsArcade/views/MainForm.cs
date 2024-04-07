using GangWarsArcade.views;
using System.Windows.Forms;

namespace GangWarsArcade
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            var buttons = new LoadLevelControl();
            var dungeonControl = new DungeonTaskControl(buttons);
            var rivalsControl = new RivalsTaskControl(buttons);

            var table = new TableLayoutPanel();
            table.RowStyles.Clear();
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize, 100));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            table.Controls.Add(buttons);
            table.Controls.Add(dungeonControl);
            table.Controls.Add(rivalsControl);

            table.Dock = DockStyle.Fill;
            buttons.Dock = DockStyle.Fill;
            dungeonControl.Dock = DockStyle.Fill;
            Controls.Add(table);

            dungeonControl.ShowedLevel += rivalsControl.Hide;
            rivalsControl.ShowedLevel += dungeonControl.Hide;
        }
    }
}
