using GangWarsArcade.domain;
using GangWarsArcade.Properties;
using Point = System.Drawing.Point;
using Timer = System.Windows.Forms.Timer;

namespace GangWarsArcade.views;

public partial class InventoryControl : UserControl
{
    private Bitmap _gunImage;
    private Bitmap _perkImage;

    private Point _gunImagePosition = new(0, 0);
    private Point _perkImagePosition = new(66, 0);
    private Size cellSize = new(64, 66);

    private readonly Timer _gunCooldown;

    public InventoryControl(Point location)
    {
        InitializeComponent();
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Enabled = false;

        Paint += OnPaint;

        Location = location;
        Size = new Size(131, 67);

        _gunCooldown = new Timer();
        _gunCooldown.Stop();

        Invalidate();
    }

    public void SetDrawGunCooldown(int interval)
    {
        _gunCooldown.Interval = interval;
        _gunCooldown.Tick += TimerTick;
        _gunCooldown.Start();
        Invalidate();
    }

    private void TimerTick(object sender, EventArgs e)
    {
        _gunCooldown.Stop();
        Invalidate();
    }

    public void Update(Player player)
    {
        if (player.Weapon != 0) _gunImage = Resource.FireBolt;
        else _gunImage = null;
        if (player.Inventory != 0) _perkImage = Resource.Trap;
        else _perkImage = null;
        Invalidate();
    }

    public void OnPaint(object sender, PaintEventArgs e)
    {
        var g = e.Graphics;

        g.DrawRectangle(new Pen(Color.Black), new Rectangle(_gunImagePosition, cellSize));
        g.FillRectangle(new SolidBrush(Color.FromArgb(150, Color.Gray.R, Color.Gray.G, Color.Gray.B)),
               new Rectangle(_gunImagePosition, cellSize));
        g.DrawRectangle(new Pen(Color.Black), new Rectangle(_perkImagePosition, cellSize));
        g.FillRectangle(new SolidBrush(Color.FromArgb(150, Color.Gray.R, Color.Gray.G, Color.Gray.B)),
               new Rectangle(_perkImagePosition, cellSize));

        if (_gunImage != null)
            g.DrawImage(_gunImage, _gunImagePosition.X, _gunImagePosition.Y, 64, 64);
        if (_perkImage != null)
            g.DrawImage(_perkImage, _perkImagePosition.X, _perkImagePosition.Y, 64, 64);

        if (_gunCooldown.Enabled)
            g.FillRectangle(new SolidBrush(Color.FromArgb(150, Color.Gray.R, Color.Gray.G, Color.Gray.B)),
                new Rectangle(_gunImagePosition, cellSize));
    }
}
