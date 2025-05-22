using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TP6.Emitter;
using static TP6.Particle;

namespace TP6
{
    class Emitter
    {
        List<Particle> particles = new List<Particle>();
        public int MousePositionX;
        public int MousePositionY;
        public float GravitationX = 0;
        public float GravitationY = 1; // пусть гравитация будет силой один пиксель за такт, нам хватит
        public List<IImpactPoint> impactPoints = new List<IImpactPoint>(); // <<< ТАК ВОТ
        public int ParticlesCount = 500;

        public int X; // координата X центра эмиттера, будем ее использовать вместо MousePositionX
        public int Y; // соответствующая координата Y 
        public int Direction = 0; // вектор направления в градусах куда сыпет эмиттер
        public int Spreading = 360; // разброс частиц относительно Direction
        public int SpeedMin = 1; // начальная минимальная скорость движения частицы
        public int SpeedMax = 10; // начальная максимальная скорость движения частицы
        public int RadiusMin = 2; // минимальный радиус частицы
        public int RadiusMax = 10; // максимальный радиус частицы
        public int LifeMin = 20; // минимальное время жизни частицы
        public int LifeMax = 100; // максимальное время жизни частицы

        public int ParticlesPerTick = 1; // добавил новое поле

        public Color ColorFrom = Color.White; // начальный цвет частицы
        public Color ColorTo = Color.FromArgb(0, Color.Black); // конечный цвет частиц
        public void UpdateState()
        {
            int particlesToCreate = ParticlesPerTick; // фиксируем счетчик сколько частиц нам создавать за тик

            foreach (var particle in particles)
            {
                if (particle.Life <= 0) // если частицы умерла
                {
                    /* 
                     * то проверяем надо ли создать частицу
                     */
                    if (particlesToCreate > 0)
                    {
                        /* у нас как сброс частицы равносилен созданию частицы */
                        particlesToCreate -= 1; // поэтому уменьшаем счётчик созданных частиц на 1
                        ResetParticle(particle);
                    }
                }
                else
                {
                    /* теперь двигаю вначале */
                    particle.X += particle.SpeedX;
                    particle.Y += particle.SpeedY;

                    particle.Life -= 1;
                    foreach (var point in impactPoints)
                    {
                        point.ImpactParticle(particle);
                    }

                    particle.SpeedX += GravitationX;
                    particle.SpeedY += GravitationY;
                }
            }

            // добавил генерацию частиц
            // генерирую не более 10 штук за тик
            // второй цикл меняем на while, 
            // этот новый цикл также будет срабатывать только в самом начале работы эмиттера
            // собственно пока не накопится критическая масса частиц
            while (particlesToCreate >= 1)
            {
                particlesToCreate -= 1;
                var particle = CreateParticle();
                ResetParticle(particle);
                particles.Add(particle);
            }
        }

        public void Render(Graphics g)
        {
            // ну тут так и быть уж сам впишу...
            // это то же самое что на форме в методе Render
            foreach (var particle in particles)
            {
                particle.Draw(g);
            }

            // рисую точки притяжения красными кружочками
            foreach (var point in impactPoints)
            {
                point.Render(g); // это добавили
            }
        }

        // добавил новый метод, виртуальным, чтобы переопределять можно было
        public virtual void ResetParticle(Particle particle)
        {
            particle.Life = Particle.rand.Next(LifeMin, LifeMax);

            particle.X = X;
            particle.Y = Y;

            var direction = Direction
                + (double)Particle.rand.Next(Spreading)
                - Spreading / 2;

            var speed = Particle.rand.Next(SpeedMin, SpeedMax);

            particle.SpeedX = (float)(Math.Cos(direction / 180 * Math.PI) * speed);
            particle.SpeedY = -(float)(Math.Sin(direction / 180 * Math.PI) * speed);

            particle.Radius = Particle.rand.Next(RadiusMin, RadiusMax);
        }

        public virtual Particle CreateParticle()
        {
            var particle = new ParticleColorful();
            particle.FromColor = ColorFrom;
            particle.ToColor = ColorTo;

            return particle;
        }




        public abstract class IImpactPoint
        {
            public float X; // ну точка же, вот и две координаты
            public float Y;

            // абстрактный метод с помощью которого будем изменять состояние частиц
            // например притягивать
            public abstract void ImpactParticle(Particle particle);

            // базовый класс для отрисовки точечки
            public virtual void Render(Graphics g)
            {
                g.FillEllipse(
                        new SolidBrush(Color.Red),
                        X - 5,
                        Y - 5,
                        10,
                        10
                    );
            }
        }

        public class GravityPoint : IImpactPoint
        {
            public int Power = 100; // сила притяжения

            // а сюда по сути скопировали с минимальными правками то что было в UpdateState
            public override void ImpactParticle(Particle particle)
            {
                float gX = X - particle.X;
                float gY = Y - particle.Y;

                double r = Math.Sqrt(gX * gX + gY * gY); // считаем расстояние от центра точки до центра частицы
                if (r + particle.Radius < Power / 2) // если частица оказалось внутри окружности
                {
                    // то притягиваем ее
                    float r2 = (float)Math.Max(100, gX * gX + gY * gY);
                    particle.SpeedX += gX * Power / r2;
                    particle.SpeedY += gY * Power / r2;
                }
            }

            public override void Render(Graphics g)
            {
                // буду рисовать окружность с диаметром равным Power
                g.DrawEllipse(
                       new Pen(Color.Red),
                       X - Power / 2,
                       Y - Power / 2,
                       Power,
                       Power
                );

                //g.DrawString(
                //    $"Я гравитон\nc силой {Power}", // надпись, можно перенос строки вставлять (если вы Катя, то может не работать и надо использовать \r\n)
                //    new Font("Verdana", 10), // шрифт и его размер
                //    new SolidBrush(Color.White), // цвет шрифта
                //    X, // расположение в пространстве
                //    Y
                //);

                //var stringFormat = new StringFormat(); // создаем экземпляр класса
                //stringFormat.Alignment = StringAlignment.Center; // выравнивание по горизонтали
                //stringFormat.LineAlignment = StringAlignment.Center; // выравнивание по вертикали
                //g.DrawString(
                //    $"Я гравитон\nc силой {Power}",
                //    new Font("Verdana", 10),
                //    new SolidBrush(Color.White),
                //    X,
                //    Y,
                //    stringFormat // передаем инфу о выравнивании
                //);

                //var stringFormat = new StringFormat();
                //stringFormat.Alignment = StringAlignment.Center;
                //stringFormat.LineAlignment = StringAlignment.Center;

                //// обязательно выносим текст и шрифт в переменные
                //var text = $"Я гравитон\nc силой {Power}";
                //var font = new Font("Verdana", 10);

                //// вызываем MeasureString, чтобы померить размеры текста
                //var size = g.MeasureString(text, font);

                //// рисуем подложнку под текст
                //g.FillRectangle(
                //    new SolidBrush(Color.Red),
                //    X - size.Width / 2, // так как я выравнивал текст по центру то подложка должна быть центрирована относительно X,Y
                //    Y - size.Height / 2,
                //    size.Width,
                //    size.Height
                //);

                //// ну и текст рисую уже на базе переменных
                //g.DrawString(
                //    text,
                //    font,
                //    new SolidBrush(Color.White),
                //    X,
                //    Y,
                //    stringFormat
                //);
            }
        }

        public class AntiGravityPoint : IImpactPoint
        {
            public int Power = 100; // сила отторжения

            // а сюда по сути скопировали с минимальными правками то что было в UpdateState
            public override void ImpactParticle(Particle particle)
            {
                float gX = X - particle.X;
                float gY = Y - particle.Y;
                float r2 = (float)Math.Max(100, gX * gX + gY * gY);

                particle.SpeedX -= gX * Power / r2; // тут минусики вместо плюсов
                particle.SpeedY -= gY * Power / r2; // и тут
            }
        }




        public class TopEmitter : Emitter
        {
            public int Width; // длина экрана

            public override void ResetParticle(Particle particle)
            {
                base.ResetParticle(particle); // вызываем базовый сброс частицы, там жизнь переопределяется и все такое

                // а теперь тут уже подкручиваем параметры движения
                particle.X = Particle.rand.Next(Width); // позиция X -- произвольная точка от 0 до Width
                particle.Y = 0;  // ноль -- это верх экрана 

                particle.SpeedY = 1; // падаем вниз по умолчанию
                particle.SpeedX = Particle.rand.Next(-2, 2); // разброс влево и вправа у частиц 
            }
        }


        public class TeleportPoint : IImpactPoint
        {
            public float ExitX, ExitY;
            public float Radius = 50;
            public float OutSpeed = 10;
            public float OutDirection;

            public void UpdateDirection()
            {
                float dx = ExitX - X;
                float dy = ExitY - Y;
                OutDirection = (float)(Math.Atan2(dy, dx) * (180.0 / Math.PI));
            }

            public override void ImpactParticle(Particle particle)
            {
                float dx = particle.X - X;
                float dy = particle.Y - Y;
                if (dx * dx + dy * dy < Radius * Radius)
                {
                    particle.X = ExitX;
                    particle.Y = ExitY;
                    //particle.Life = 20 + Particle.rand.Next(100);
                    float angleRad = OutDirection;
                    //* (float)(Math.PI / 180.0);
                    //particle.SpeedX = OutSpeed * (float)Math.Cos(angleRad);
                    //particle.SpeedY = OutSpeed * (float)Math.Sin(angleRad);
                }
            }

            public override void Render(Graphics g)
            {
                g.DrawEllipse(Pens.Red, X - Radius, Y - Radius, Radius * 2, Radius * 2);
                g.DrawLine(Pens.Blue, ExitX - 10, ExitY, ExitX + 10, ExitY);
                g.DrawLine(Pens.Blue, ExitX, ExitY - 10, ExitX, ExitY + 10);
            }
        }


        public class LawnZone : IImpactPoint
        {
            public int Width;
            public int Height;
            public float Moisture = 100; // от 0 (сухо) до 100 (влажно)
            public Color CurrentColor => GetColor();

            public LawnZone(int x, int y, int width, int height)
            {
                this.X = x;
                this.Y = y;
                this.Width = width;
                this.Height = height;
            }

            public override void ImpactParticle(Particle p)
            {
                // Проверка попадания частицы в прямоугольную область
                if (p.X >= X && p.X <= X + Width && p.Y >= Y && p.Y <= Y + Height)
                {
                    Moisture = Math.Min(100, Moisture + 0.5f); // увлажнение при попадании
                    p.Life = 0; // частица исчезает
                }
            }

            public override void Render(Graphics g)
            {
                // Сухость влияет на цвет
                var b = new SolidBrush(GetColor());
                g.FillRectangle(b, X, Y, Width, Height);
                b.Dispose();

                // Контур
                g.DrawRectangle(Pens.Black, X, Y, Width, Height);
            }

            private Color GetColor()
            {
                if (Moisture > 50)
                    return Color.Green;
                else if (Moisture > 25)
                    return Color.YellowGreen;
                else
                    return Color.Orange;
            }

            public void Dry()
            {
                Moisture = Math.Max(0, Moisture - 0.3f); // Усиленное высыхание
            }

            public bool IsDead => Moisture <= 0;
        }

    }
}
