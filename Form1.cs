using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static TP6.Emitter;

namespace TP6
{
    public partial class Form1 : Form
    {
        private Emitter emitter; // ������� ������� ��� �������� ������
        private List<LawnZone> lawnZones; // ������ �� ������ ��� �������
        private AntiGravityPoint antiGravityPointLeft; // ������������������ ����� ��� ������ ��������
        private AntiGravityPoint antiGravityPointRight; // ������������������ ����� ��� �������� ��������
        private TeleportPoint teleportPoint; // ������ �������� ����� 1-� � 2-� ���������
        private TeleportPoint teleportPointSecond; // ������ �������� ����� 3-� � 4-� ���������
        private int score = 0; // ����, ��������� �� �������
        private int tickCounter = 0; // ������� ����� ��� ���������� �����
        private int rainCount = 0; // ������� ��������� ������
        private int rainTicks = 0; // ������� ����� ��� ������������ �����
        private TopEmitter rainEmitter; // ������� ��� �����
        private int lastScoreMilestone = 0; // ��������� ������� ����� ��� ���������� ������

        public Form1()
        {
            InitializeComponent();

            // ������������� ������� �������� ��� picDisplay
            try
            {
                // ����� ���� � �����������, ��������, "img/background.jpg"
                // ������ ����������� � ������, �������� Copy to Output Directory = Copy always
                picDisplay.Image = Image.FromFile("img/background.jpg");
                picDisplay.SizeMode = PictureBoxSizeMode.StretchImage; // ����������� �����������
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ �������� ����: {ex.Message}. ���������� ��� ����.");
            }

            // �������������� �������� �������
            emitter = new Emitter
            {
                ParticlesPerTick = 3, // ������ 3 ������� �� ���
                ColorFrom = Color.Blue, // ��������� ���� � ����� (�������� ����)
                ColorTo = Color.FromArgb(0, Color.Blue), // �������� ����
                SpeedMin = 5,
                SpeedMax = 10,
                LifeMin = 50,
                LifeMax = 100,
                GravitationY = 1.5f, // ���������� ��� ������� ����
                Direction = 270, // ����������� ����
                Spreading = 30, // ��������� �������
                RadiusMin = 2,
                RadiusMax = 10
            };

            // �������������� ������� ��� �����
            rainEmitter = new TopEmitter
            {
                Width = picDisplay.Width, // ������ ������
                ParticlesPerTick = 10, // ������ ������ ��� ������� �����
                ColorFrom = Color.Blue,
                ColorTo = Color.FromArgb(0, Color.Blue),
                SpeedMin = 5,
                SpeedMax = 10,
                LifeMin = 50,
                LifeMax = 100,
                GravitationY = 1.5f,
                RadiusMin = 2,
                RadiusMax = 10
            };
            rainEmitter.impactPoints = emitter.impactPoints; // ����� ������ �� �� �� ���� � �����

            // �������������� ������ ���� ������� � ��� (1x4) � ������ ����� PictureBox
            lawnZones = new List<LawnZone>();
            int zoneWidth = picDisplay.Width / 4;
            int zoneHeight = 100; // ������������� ������ ���
            int zoneY = picDisplay.Height - zoneHeight; // ��������� � ������ �������
            lawnZones.Add(new LawnZone(0, zoneY, zoneWidth, zoneHeight)); // ������ ����
            lawnZones.Add(new LawnZone(zoneWidth, zoneY, zoneWidth, zoneHeight)); // ������ ����
            lawnZones.Add(new LawnZone(zoneWidth * 2, zoneY, zoneWidth, zoneHeight)); // ������ ����
            lawnZones.Add(new LawnZone(zoneWidth * 3, zoneY, zoneWidth, zoneHeight)); // �������� ����

            // �������������� ����� �����������
            antiGravityPointLeft = new AntiGravityPoint
            {
                X = picDisplay.Width / 8, // ����� ������ �������
                Y = picDisplay.Height / 2, // �������� �� ������
                Power = 150 // ����������� ���� ������������
            };

            antiGravityPointRight = new AntiGravityPoint
            {
                X = picDisplay.Width * 7 / 8, // ����� �������� �������
                Y = picDisplay.Height / 2, // �������� �� ������
                Power = 150 // ����������� ���� ������������
            };

            teleportPoint = new TeleportPoint
            {
                X = picDisplay.Width / 4, // ����� 1-� � 2-� ���������
                Y = picDisplay.Height / 2, // �������� �� ���������
                Radius = 50,
                OutSpeed = 10,
                ExitX = picDisplay.Width * 7 / 8, // ����� �������� �������
                ExitY = picDisplay.Height / 2 + 50 // ���� ������� �������������
            };
            teleportPoint.UpdateDirection(); // ��������� ����������� ������� ���������

            teleportPointSecond = new TeleportPoint
            {
                X = picDisplay.Width * 3 / 4, // ����� 3-� � 4-� ���������
                Y = picDisplay.Height / 2, // �������� �� ���������
                Radius = 50,
                OutSpeed = 10,
                ExitX = picDisplay.Width / 8, // ����� ������ �������
                ExitY = picDisplay.Height / 2 + 50 // ���� ������ �������������
            };
            teleportPointSecond.UpdateDirection(); // ��������� ����������� ������� ���������

            // ��������� ���� � ����� ����������� � �������� �������
            foreach (var zone in lawnZones)
            {
                emitter.impactPoints.Add(zone);
            }
            emitter.impactPoints.Add(antiGravityPointLeft);
            emitter.impactPoints.Add(antiGravityPointRight);
            emitter.impactPoints.Add(teleportPoint);
            emitter.impactPoints.Add(teleportPointSecond);

            // ����������� ������
            btnRain.Enabled = false;
            btnRain.Text = "����� x0";
            btnRain.Click += BtnRain_Click;

            btnRestart.Enabled = false;
            btnRestart.Click += BtnRestart_Click;

            // ����������� ������ (���� �� �������� � ���������)
            timer1.Tick += Timer1_Tick;
            timer1.Start();

            // ������������� �� ������� �������� ���� ��� PictureBox
            picDisplay.MouseMove += PicDisplay_MouseMove;

            // ������������� �� ������� ��������� PictureBox
            picDisplay.Paint += PicDisplay_Paint;

            // ������������� �� ������� ��������� ������� PictureBox
            picDisplay.Resize += PicDisplay_Resize;

            // �������� ������� ����������� ��� PictureBox
            picDisplay.GetType().GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(picDisplay, true);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            // ��������� ��������� ��������� ��������
            emitter.UpdateState();

            // ��������� ��������� �������� �����, ���� ����� �������
            if (rainTicks > 0)
            {
                rainEmitter.UpdateState();
                rainTicks--;
            }

            // ��������� ����: 1 ���� �� ������ 50 ����� (1 �������)
            tickCounter++;
            if (tickCounter >= 50) // 50 ����� = 1000 �� = 1 �������
            {
                score++;
                tickCounter = 0; // ���������� �������

                // ���������, �������� �� ����� ������� ��� ���������� �����
                if (score % 10 == 0 && score > lastScoreMilestone)
                {
                    rainCount++;
                    lastScoreMilestone = score;
                    btnRain.Text = $"����� x{rainCount}";
                    btnRain.Enabled = true;
                }
            }

            // ��������� ��������� ������ �����
            btnRain.Enabled = rainCount > 0;

            // ���������� ��� ����
            foreach (var zone in lawnZones)
            {
                zone.Dry();
                if (zone.IsDead)
                {
                    timer1.Stop();
                    btnRestart.Enabled = true; // ���������� ������ ��������
                    MessageBox.Show($"���� ��������! ���� �� ��� ������� �������. ����: {score}");
                    return;
                }
            }

            // �������������� PictureBox
            picDisplay.Invalidate();
        }

        private void BtnRain_Click(object sender, EventArgs e)
        {
            if (rainCount > 0)
            {
                rainCount--;
                rainTicks = 250; // ����� ������ 5 ������ (250 �����)
                btnRain.Text = $"����� x{rainCount}";
                btnRain.Enabled = rainCount > 0;
            }
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            ResetGame();
            picDisplay.Invalidate(); // �������������� �����
        }

        private void ResetGame()
        {
            // ���������� ��������� ����
            score = 0;
            tickCounter = 0;
            rainCount = 0;
            rainTicks = 0;
            lastScoreMilestone = 0;

            // ���������� ��������� ���
            foreach (var zone in lawnZones)
            {
                zone.Moisture = 100; // ��������������� ��������� ���������
            }

            // ���������� ��������� ������
            btnRain.Enabled = false;
            btnRain.Text = "����� x0";
            btnRestart.Enabled = false;

            // ������������� ������
            timer1.Start();
        }

        private void PicDisplay_Paint(object sender, PaintEventArgs e)
        {
            // �������� �������� ������� (������� � ����) �� PictureBox
            emitter.Render(e.Graphics);

            // �������� ������� �����, ���� ����� �������
            if (rainTicks > 0)
            {
                rainEmitter.Render(e.Graphics);
            }

            // ������������ ���� � ����� ������� ����
            var g = e.Graphics;
            var font = new Font("Verdana", 12);
            var text = $"����: {score}";
            var textSize = g.MeasureString(text, font);
            g.FillRectangle(Brushes.Black, 10, 10, textSize.Width, textSize.Height); // ׸���� ��������
            g.DrawString(text, font, Brushes.White, 10, 10); // ����� �����
        }

        private void PicDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            // ��������� ������� ��������� �������� � ������������ � ���������� ����
            emitter.X = e.X;
            emitter.Y = 0; // ��������� Y ������ PictureBox
        }

        private void PicDisplay_Resize(object sender, EventArgs e)
        {
            if (emitter != null && lawnZones != null)
            {
                int zoneWidth = picDisplay.Width / 4;
                int zoneHeight = 100; // ������������� ������ ���
                int zoneY = picDisplay.Height - zoneHeight; // ��������� � ������ �������
                lawnZones[0].Width = zoneWidth;
                lawnZones[0].Height = zoneHeight;
                lawnZones[0].X = 0;
                lawnZones[0].Y = zoneY;
                lawnZones[1].Width = zoneWidth;
                lawnZones[1].Height = zoneHeight;
                lawnZones[1].X = zoneWidth;
                lawnZones[1].Y = zoneY;
                lawnZones[2].Width = zoneWidth;
                lawnZones[2].Height = zoneHeight;
                lawnZones[2].X = zoneWidth * 2;
                lawnZones[2].Y = zoneY;
                lawnZones[3].Width = zoneWidth;
                lawnZones[3].Height = zoneHeight;
                lawnZones[3].X = zoneWidth * 3;
                lawnZones[3].Y = zoneY;

                // ��������� ������� ����� �����������
                antiGravityPointLeft.X = picDisplay.Width / 8; // ����� ������ �������
                antiGravityPointLeft.Y = picDisplay.Height / 2; // �������� �� ������
                antiGravityPointRight.X = picDisplay.Width * 7 / 8; // ����� �������� �������
                antiGravityPointRight.Y = picDisplay.Height / 2; // �������� �� ������
                teleportPoint.X = picDisplay.Width / 4; // ����� 1-� � 2-� ���������
                teleportPoint.Y = picDisplay.Height / 2; // �������� �� ���������
                teleportPoint.ExitX = picDisplay.Width * 7 / 8; // ����� �������� �������
                teleportPoint.ExitY = picDisplay.Height / 2 + 50; // ���� ������� �������������
                teleportPoint.UpdateDirection();
                teleportPointSecond.X = picDisplay.Width * 3 / 4; // ����� 3-� � 4-� ���������
                teleportPointSecond.Y = picDisplay.Height / 2; // �������� �� ���������
                teleportPointSecond.ExitX = picDisplay.Width / 8; // ����� ������ �������
                teleportPointSecond.ExitY = picDisplay.Height / 2 + 50; // ���� ������ �������������
                teleportPointSecond.UpdateDirection();

                // ��������� ������ �������� �����
                rainEmitter.Width = picDisplay.Width;
            }
        }
    }
}