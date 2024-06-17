using System;
using WinFormsApp1.Objects;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        List<BaseObject> objects = new();
        Player player;
        Marker marker;
        SpeedBoost speedBoost;

        public Form1()
        {
            InitializeComponent();
            player = new Player(pbMain.Width / 2, pbMain.Height / 2, 0);
            // добавляю реакцию на пересечение
            textBox1.Text = $"Очки: {player.Score}";
            player.OnOverlap += (p, obj) =>
            {
                txtLog.Text = $"[{DateTime.Now:HH:mm:ss:ff}] Игрок пересекся с {obj}\n" + txtLog.Text;
                if (obj is SpeedBoost speedBoost && !speedBoost.isPlayerOverlapping)
                {
                    textBox1.Text = $"Очки: {++player.Score}";
                    speedBoost.isPlayerOverlapping = true;
                }
            };
            
            player.OnMarkerOverlap += (m) =>
            {
                objects.Remove(m);
                marker = null;
            };
            speedBoost = new SpeedBoost(100, 100, 0, pbMain);
            objects.Add(player);
            objects.Add(speedBoost);
        }

        private void pbMain_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics; //отрисовка элементов на экране

            g.Clear(Color.White);

            // Сначала рисуем SpeedBoost
            foreach (var obj in objects.Where(o => o is SpeedBoost))
            {
                g.Transform = obj.GetTransform();
                obj.Render(g);
            }

            // Затем рисуем игрока
            updatePlayer();
            g.Transform = player.GetTransform();
            player.Render(g);

            // Рисуем остальные объекты
            foreach (var obj in objects.Where(o => o != player && o is not SpeedBoost))
            {
                g.Transform = obj.GetTransform();
                obj.Render(g);
            }

            // Пересчитываем пересечения
            foreach (var obj in objects.ToList())
            {
                if (obj != player && player.Overlaps(obj, g))
                {
                    player.Overlap(obj);
                    obj.Overlap(player);
                }
            }
        }

        private void updatePlayer()
        {
            if (marker != null)
            {
                float dx = marker.X - player.X;
                float dy = marker.Y - player.Y;
                float length = MathF.Sqrt(dx * dx + dy * dy);
                dx /= length;
                dy /= length;

                player.vX += dx * 0.5f;
                player.vY += dy * 0.5f;

                player.Angle = 90 - MathF.Atan2(player.vX, player.vY) * 180 / MathF.PI;
            }

            player.vX += -player.vX * 0.1f;
            player.vY += -player.vY * 0.1f;

            player.X += player.vX;
            player.Y += player.vY;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pbMain.Invalidate();
        }

        private void pbMain_MouseClick(object sender, MouseEventArgs e)
        {
            if (marker == null)
            {
                marker = new Marker(e.X, e.Y, 0);
                objects.Add(marker);
            }
            else
            {
                marker.X = e.X;
                marker.Y = e.Y;
            }
        }
    }
}
