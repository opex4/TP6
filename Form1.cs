using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using static TP6.Emitter;

namespace TP6
{
    public partial class Form1 : Form
    {
        private Emitter emitter; // Обычный эмиттер для создания частиц
        private List<LawnZone> lawnZones; // Список из четырёх зон лужайки
        private AntiGravityPoint antiGravityPointLeft; // Антигравитационная точка над первой лужайкой
        private AntiGravityPoint antiGravityPointRight; // Антигравитационная точка над четвёртой лужайкой
        private TeleportPoint teleportPoint; // Первый телепорт между 1-й и 2-й лужайками
        private TeleportPoint teleportPointSecond; // Второй телепорт между 3-й и 4-й лужайками
        private int score = 0; // Очки, зависящие от времени
        private int tickCounter = 0; // Счётчик тиков для начисления очков
        private int rainCount = 0; // Счётчик доступных дождей
        private int rainTicks = 0; // Счётчик тиков для длительности дождя
        private TopEmitter rainEmitter; // Эмиттер для дождя
        private int lastScoreMilestone = 0; // Последняя отметка очков для начисления дождей

        public Form1()
        {
            InitializeComponent();

            // Устанавливаем фоновую картинку для picDisplay
            try
            {
                // Укажи путь к изображению, например, "img/background.jpg"
                // Добавь изображение в проект, установи Copy to Output Directory = Copy always
                picDisplay.Image = Image.FromFile("img/background.jpg");
                picDisplay.SizeMode = PictureBoxSizeMode.StretchImage; // Растягиваем изображение
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки фона: {ex.Message}. Продолжаем без фона.");
            }

            // Инициализируем основной эмиттер
            emitter = new Emitter
            {
                ParticlesPerTick = 3, // Создаём 3 частицы за тик
                ColorFrom = Color.Blue, // Начальный цвет — синий (имитация воды)
                ColorTo = Color.FromArgb(0, Color.Blue), // Конечный цвет
                SpeedMin = 5,
                SpeedMax = 10,
                LifeMin = 50,
                LifeMax = 100,
                GravitationY = 1.5f, // Гравитация для падения вниз
                Direction = 270, // Направление вниз
                Spreading = 30, // Небольшой разброс
                RadiusMin = 2,
                RadiusMax = 10
            };

            // Инициализируем эмиттер для дождя
            rainEmitter = new TopEmitter
            {
                Width = picDisplay.Width, // Ширина экрана
                ParticlesPerTick = 10, // Больше частиц для эффекта дождя
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
            rainEmitter.impactPoints = emitter.impactPoints; // Дождь влияет на те же зоны и точки

            // Инициализируем четыре зоны лужайки в ряд (1x4) в нижней части PictureBox
            lawnZones = new List<LawnZone>();
            int zoneWidth = picDisplay.Width / 4;
            int zoneHeight = 100; // Фиксированная высота зон
            int zoneY = picDisplay.Height - zoneHeight; // Прижимаем к нижней границе
            lawnZones.Add(new LawnZone(0, zoneY, zoneWidth, zoneHeight)); // Первая зона
            lawnZones.Add(new LawnZone(zoneWidth, zoneY, zoneWidth, zoneHeight)); // Вторая зона
            lawnZones.Add(new LawnZone(zoneWidth * 2, zoneY, zoneWidth, zoneHeight)); // Третья зона
            lawnZones.Add(new LawnZone(zoneWidth * 3, zoneY, zoneWidth, zoneHeight)); // Четвёртая зона

            // Инициализируем точки воздействия
            antiGravityPointLeft = new AntiGravityPoint
            {
                X = picDisplay.Width / 8, // Центр первой лужайки
                Y = picDisplay.Height / 2, // Середина по высоте
                Power = 150 // Увеличенная сила отталкивания
            };

            antiGravityPointRight = new AntiGravityPoint
            {
                X = picDisplay.Width * 7 / 8, // Центр четвёртой лужайки
                Y = picDisplay.Height / 2, // Середина по высоте
                Power = 150 // Увеличенная сила отталкивания
            };

            teleportPoint = new TeleportPoint
            {
                X = picDisplay.Width / 4, // Между 1-й и 2-й лужайками
                Y = picDisplay.Height / 2, // Середина по вертикали
                Radius = 50,
                OutSpeed = 10,
                ExitX = picDisplay.Width * 7 / 8, // Центр четвёртой лужайки
                ExitY = picDisplay.Height / 2 + 50 // Ниже правого антигравитона
            };
            teleportPoint.UpdateDirection(); // Обновляем направление первого телепорта

            teleportPointSecond = new TeleportPoint
            {
                X = picDisplay.Width * 3 / 4, // Между 3-й и 4-й лужайками
                Y = picDisplay.Height / 2, // Середина по вертикали
                Radius = 50,
                OutSpeed = 10,
                ExitX = picDisplay.Width / 8, // Центр первой лужайки
                ExitY = picDisplay.Height / 2 + 50 // Ниже левого антигравитона
            };
            teleportPointSecond.UpdateDirection(); // Обновляем направление второго телепорта

            // Добавляем зоны и точки воздействия в основной эмиттер
            foreach (var zone in lawnZones)
            {
                emitter.impactPoints.Add(zone);
            }
            emitter.impactPoints.Add(antiGravityPointLeft);
            emitter.impactPoints.Add(antiGravityPointRight);
            emitter.impactPoints.Add(teleportPoint);
            emitter.impactPoints.Add(teleportPointSecond);

            // Настраиваем кнопки
            btnRain.Enabled = false;
            btnRain.Text = "Дождь x0";
            btnRain.Click += BtnRain_Click;

            btnRestart.Enabled = false;
            btnRestart.Click += BtnRestart_Click;

            // Настраиваем таймер (если не настроен в дизайнере)
            timer1.Tick += Timer1_Tick;
            timer1.Start();

            // Подписываемся на событие движения мыши для PictureBox
            picDisplay.MouseMove += PicDisplay_MouseMove;

            // Подписываемся на событие отрисовки PictureBox
            picDisplay.Paint += PicDisplay_Paint;

            // Подписываемся на событие изменения размера PictureBox
            picDisplay.Resize += PicDisplay_Resize;

            // Включаем двойную буферизацию для PictureBox
            picDisplay.GetType().GetProperty("DoubleBuffered",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)
                .SetValue(picDisplay, true);
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            // Обновляем состояние основного эмиттера
            emitter.UpdateState();

            // Обновляем состояние эмиттера дождя, если дождь активен
            if (rainTicks > 0)
            {
                rainEmitter.UpdateState();
                rainTicks--;
            }

            // Начисляем очки: 1 очко за каждые 50 тиков (1 секунда)
            tickCounter++;
            if (tickCounter >= 50) // 50 тиков = 1000 мс = 1 секунда
            {
                score++;
                tickCounter = 0; // Сбрасываем счётчик

                // Проверяем, достигли ли новой отметки для начисления дождя
                if (score % 10 == 0 && score > lastScoreMilestone)
                {
                    rainCount++;
                    lastScoreMilestone = score;
                    btnRain.Text = $"Дождь x{rainCount}";
                    btnRain.Enabled = true;
                }
            }

            // Обновляем состояние кнопки дождя
            btnRain.Enabled = rainCount > 0;

            // Высушиваем все зоны
            foreach (var zone in lawnZones)
            {
                zone.Dry();
                if (zone.IsDead)
                {
                    timer1.Stop();
                    btnRestart.Enabled = true; // Активируем кнопку рестарта
                    MessageBox.Show($"Игра окончена! Одна из зон лужайки высохла. Очки: {score}");
                    return;
                }
            }

            // Перерисовываем PictureBox
            picDisplay.Invalidate();
        }

        private void BtnRain_Click(object sender, EventArgs e)
        {
            if (rainCount > 0)
            {
                rainCount--;
                rainTicks = 250; // Дождь длится 5 секунд (250 тиков)
                btnRain.Text = $"Дождь x{rainCount}";
                btnRain.Enabled = rainCount > 0;
            }
        }

        private void BtnRestart_Click(object sender, EventArgs e)
        {
            ResetGame();
            picDisplay.Invalidate(); // Перерисовываем экран
        }

        private void ResetGame()
        {
            // Сбрасываем параметры игры
            score = 0;
            tickCounter = 0;
            rainCount = 0;
            rainTicks = 0;
            lastScoreMilestone = 0;

            // Сбрасываем состояние зон
            foreach (var zone in lawnZones)
            {
                zone.Moisture = 100; // Восстанавливаем начальную влажность
            }

            // Сбрасываем состояние кнопок
            btnRain.Enabled = false;
            btnRain.Text = "Дождь x0";
            btnRestart.Enabled = false;

            // Перезапускаем таймер
            timer1.Start();
        }

        private void PicDisplay_Paint(object sender, PaintEventArgs e)
        {
            // Рендерим основной эмиттер (частицы и зоны) на PictureBox
            emitter.Render(e.Graphics);

            // Рендерим эмиттер дождя, если дождь активен
            if (rainTicks > 0)
            {
                rainEmitter.Render(e.Graphics);
            }

            // Отрисовываем очки в левом верхнем углу
            var g = e.Graphics;
            var font = new Font("Verdana", 12);
            var text = $"Очки: {score}";
            var textSize = g.MeasureString(text, font);
            g.FillRectangle(Brushes.Black, 10, 10, textSize.Width, textSize.Height); // Чёрная подложка
            g.DrawString(text, font, Brushes.White, 10, 10); // Белый текст
        }

        private void PicDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            // Обновляем позицию основного эмиттера в соответствии с положением мыши
            emitter.X = e.X;
            emitter.Y = 0; // Фиксируем Y вверху PictureBox
        }

        private void PicDisplay_Resize(object sender, EventArgs e)
        {
            if (emitter != null && lawnZones != null)
            {
                int zoneWidth = picDisplay.Width / 4;
                int zoneHeight = 100; // Фиксированная высота зон
                int zoneY = picDisplay.Height - zoneHeight; // Прижимаем к нижней границе
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

                // Обновляем позиции точек воздействия
                antiGravityPointLeft.X = picDisplay.Width / 8; // Центр первой лужайки
                antiGravityPointLeft.Y = picDisplay.Height / 2; // Середина по высоте
                antiGravityPointRight.X = picDisplay.Width * 7 / 8; // Центр четвёртой лужайки
                antiGravityPointRight.Y = picDisplay.Height / 2; // Середина по высоте
                teleportPoint.X = picDisplay.Width / 4; // Между 1-й и 2-й лужайками
                teleportPoint.Y = picDisplay.Height / 2; // Середина по вертикали
                teleportPoint.ExitX = picDisplay.Width * 7 / 8; // Центр четвёртой лужайки
                teleportPoint.ExitY = picDisplay.Height / 2 + 50; // Ниже правого антигравитона
                teleportPoint.UpdateDirection();
                teleportPointSecond.X = picDisplay.Width * 3 / 4; // Между 3-й и 4-й лужайками
                teleportPointSecond.Y = picDisplay.Height / 2; // Середина по вертикали
                teleportPointSecond.ExitX = picDisplay.Width / 8; // Центр первой лужайки
                teleportPointSecond.ExitY = picDisplay.Height / 2 + 50; // Ниже левого антигравитона
                teleportPointSecond.UpdateDirection();

                // Обновляем ширину эмиттера дождя
                rainEmitter.Width = picDisplay.Width;
            }
        }
    }
}