using System.Diagnostics.Metrics;
using static TP6.Emitter;
using static TP6.Particle;

namespace TP6
{
    public partial class Form1 : Form
    {
        List<Emitter> emitters = new List<Emitter>();
        Emitter emitter; // ��� ������ ����� ��������
        TeleportPoint teleport;

        GravityPoint point1; // ������� ���� ��� ������ �����
        GravityPoint point2; // ������� ���� ��� ������ �����

        public Form1()
        {
            InitializeComponent();
            picDisplay.Image = new Bitmap(picDisplay.Width, picDisplay.Height);

            this.emitter = new Emitter // ������ ������� � ���������� ��� � ���� emitter
            {
                Direction = 0,
                Spreading = 10,
                SpeedMin = 10,
                SpeedMax = 10,
                ColorFrom = Color.Gold,
                ColorTo = Color.FromArgb(0, Color.Red),
                ParticlesPerTick = 10,
                X = picDisplay.Width / 2,
                Y = picDisplay.Height / 2,
            };

            emitters.Add(this.emitter); // ��� ����� �������� � ������ emitters, ����� �� ���������� � ����������

            //// ����������� ��������� � �����
            //point1 = new GravityPoint
            //{
            //    X = picDisplay.Width / 2 + 100,
            //    Y = picDisplay.Height / 2,
            //};
            //point2 = new GravityPoint
            //{
            //    X = picDisplay.Width / 2 - 100,
            //    Y = picDisplay.Height / 2,
            //};

            //// ����������� ���� � ��������
            //emitter.impactPoints.Add(point1);
            //emitter.impactPoints.Add(point2);

            teleport = new TeleportPoint();
            teleport.X = picDisplay.Width / 2 + 200;
            teleport.Y = picDisplay.Height / 2;
            teleport.ExitX = picDisplay.Width / 2 - 200;
            teleport.ExitY = picDisplay.Height / 2;
            teleport.UpdateDirection();
            emitter.impactPoints.Add(teleport);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            emitter.UpdateState();

            using (var g = Graphics.FromImage(picDisplay.Image))
            {
                g.Clear(Color.Black);
                emitter.Render(g);
            }

            picDisplay.Invalidate();
        }

        private void picDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            //// � ��� � ������� �������� ��������� �����
            //emitter.MousePositionX = e.X;
            //emitter.MousePositionY = e.Y;

            //// � ��� �������� ��������� ����, � ��������� ���������
            //point2.X = e.X;
            //point2.Y = e.Y;
        }

        //private void picDisplay_MouseClick(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Left)
        //    {
        //        teleport.X = e.X;
        //        teleport.Y = e.Y;
        //    }
        //    else if (e.Button == MouseButtons.Right)
        //    {
        //        teleport.ExitX = e.X;
        //        teleport.ExitY = e.Y;
        //    }
        //    teleport.UpdateDirection();
        //}


        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            var rnd = new Random();
            emitter.Direction = tbDirection.Value; // ����������� �������� ����������� �������� �������� 
            lblDirection.Text = $"{tbDirection.Value}�"; // ������� ����� ��������
        }

        private void tbGraviton2_Scroll(object sender, EventArgs e)
        {
            teleport.Radius = tbTeleport.Value;
        }

        private void picDisplay_MouseClick_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                teleport.X = e.X;
                teleport.Y = e.Y;
            }
            else if (e.Button == MouseButtons.Right)
            {
                teleport.ExitX = e.X;
                teleport.ExitY = e.Y;
            }
            teleport.UpdateDirection();
        }
    }
}
