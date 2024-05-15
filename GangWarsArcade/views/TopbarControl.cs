﻿using GangWarsArcade.domain;
using System.Drawing.Text;
using Point = System.Drawing.Point;

namespace GangWarsArcade.views;

public partial class TopbarControl : UserControl
{
    private Point _roundWinnersBarPosition;
    
    private readonly GameState _gameState;
    private readonly Label _timeLable;

    public TopbarControl(GameState gameState, Player[] players, Point location, Size size, PrivateFontCollection font)     {
        InitializeComponent();
                BackColor = Color.DimGray;
        
        Location = location;
        Size = size;

        _roundWinnersBarPosition = new Point(size.Width / 2 - 69, 20);

        _gameState = gameState;
        _gameState.InvalidateTopbarVisual += Update;

                Paint += DrawRoundWinners;

                CreatePlayerBars(players, font);

                _timeLable = new Label
        {
            Location = new Point(Size.Width / 2 - 60, Size.Height / 2 - 5),
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.White,
            Font = new Font(font.Families[0], 24),
            Size = new Size(120, 40),
            Text = TimeSpan.FromSeconds(0).ToString(@"mm\:ss")
        };
        Controls.Add(_timeLable);
    }

    private void CreatePlayerBars(Player[] players, PrivateFontCollection font)     {
        for (var i = 0; i < players.Length; i++)
        {
            var player = players[i];

            var offset = 50;
            if (i >= players.Length / 2) offset = 330;
            var bar = new PlayerBarControl(player, font) { Location = new Point(i * 230 + offset, 15) };
            Controls.Add(bar);

            player.PlayerUpdated += bar.UpdatePlayerBar;
        }
    }

    public void Update()
    {
        _timeLable.Text = TimeSpan.FromSeconds(_gameState.TimeLeft).ToString(@"mm\:ss");             Invalidate();       }

        private void DrawRoundWinners(object sender, PaintEventArgs e)     {
        var g = e.Graphics;

        var rounds = _gameState.RoundsWinners;
        for (var i = 0; i < rounds.Length; i++)          {
            var rect = new RectangleF(_roundWinnersBarPosition.X + i * 30f, _roundWinnersBarPosition.Y, 15, 15);
            var shadow = new RectangleF(_roundWinnersBarPosition.X + i * 30f + 2, _roundWinnersBarPosition.Y + 2, 15, 15);
            g.DrawEllipse(new Pen (new SolidBrush(Color.FromKnownColor(KnownColor.Black))), shadow);
            g.FillEllipse(new SolidBrush(Color.FromKnownColor(KnownColor.Black)), shadow);
            if (rounds[i] != 0)             {
                var color = GameplayPainter.colourValues[(int)rounds[i] % GameplayPainter.colourValues.Length];
                g.DrawEllipse(new Pen(color), rect);
                g.FillEllipse(color, rect);
            }
            else
            {
                g.DrawEllipse(new Pen(Color.FromKnownColor(KnownColor.White)), rect);
                g.FillEllipse(new SolidBrush(Color.FromKnownColor(KnownColor.White)), rect);
            }
        }
    }
}