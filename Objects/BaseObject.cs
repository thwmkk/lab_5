using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1.Objects
{
    class BaseObject
    {
        public Random random = new();
        public float X;
        public float Y;
        public float Angle;
        // добавил поле делегат, к которому можно будет привязать реакцию на собыития
        public Action<BaseObject, BaseObject> OnOverlap;

        public BaseObject(float x, float y, float angle)
        {
            X = x;
            Y = y;
            Angle = angle;
        }
        public Matrix GetTransform()
        {
            var matrix = new Matrix();
            matrix.Translate(X, Y);
            matrix.Rotate(Angle);
            return matrix;
        }
        // добавил виртуальный метод для отрисовки
        public virtual void Render(Graphics g)
        {
            // тут пусто
        }
        public virtual GraphicsPath GetGraphicsPath()
        {
            // пока возвращаем пустую форму
            return new GraphicsPath();
        }
        public virtual bool Overlaps(BaseObject obj, Graphics g)
        {
            // берем информацию о форме
            var path1 = this.GetGraphicsPath();
            var path2 = obj.GetGraphicsPath();

            // применяем к объектам матрицы трансформации
            path1.Transform(this.GetTransform());
            path2.Transform(obj.GetTransform());

            // используем класс Region, который позволяет определить 
            // пересечение объектов в данном графическом контексте
            var region = new Region(path1);
            region.Intersect(path2); // пересекаем формы
            return !region.IsEmpty(g); // если полученная форма не пуста то значит было пересечение
        }
        public virtual void Overlap(BaseObject obj) { 
        if (this.OnOverlap != null)
            {
                this.OnOverlap(this, obj);
            }
        }
    }
    class Player : BaseObject
    {
        public int Score = 0;
        public Action<Marker> OnMarkerOverlap;
        public float vX, vY;
        public Player(float x, float y, float angle) : base(x, y, angle)
        {
        }
        public override void Render(Graphics g) {
            g.FillEllipse(
                new SolidBrush(Color.AliceBlue), -15, -15, 30, 30);
            g.DrawEllipse(
                new Pen(Color.Black, 2), -15, -15, 30, 30);
            g.DrawLine(
                new Pen(Color.Black, 2), 0, 0, 25, 0);
        }
        public override GraphicsPath GetGraphicsPath()
        {
            var path = base.GetGraphicsPath();
            path.AddEllipse(-15, -15, 30, 30);
            return path;
        }
        public override void Overlap(BaseObject obj)
        {
            base.Overlap(obj);
            if (obj is Marker)
            {
                OnMarkerOverlap(obj as Marker);
            }
        }
    }
    class Marker : BaseObject
    {
        public Marker(float x, float y, float angle) : base(x, y, angle)
        {
        }
        public override void Render(Graphics g)
        {
            g.FillEllipse(
                new SolidBrush(Color.DarkSeaGreen), -3, -3, 6, 6);
            g.DrawEllipse(
                new Pen(Color.DarkSeaGreen, 2), -6, -6, 12, 12);
            g.DrawEllipse(
                new Pen(Color.DarkSeaGreen, 2), -10, -10, 20, 20);
        }
        public override GraphicsPath GetGraphicsPath()
        {
            var path = base.GetGraphicsPath();
            path.AddEllipse(-3, -3, 6, 6);
            return path;
        }
    }


    class SpeedBoost : BaseObject
    {
        private System.Timers.Timer countdownTimer;
        private int timeRemaining = 30;
        private PictureBox pbMain;
        public bool isPlayerOverlapping = false;

        public SpeedBoost(float x, float y, float angle, PictureBox pictureBox) : base(x, y, angle)
        {
            pbMain = pictureBox;
            countdownTimer = new System.Timers.Timer();
            countdownTimer.Interval = 100; 
            countdownTimer.Start();
            countdownTimer.Elapsed += CountdownTimer_Tick;
        }

        private void CountdownTimer_Tick(object? sender, EventArgs e)
        {
            timeRemaining--;
            if (timeRemaining == 0)
            {
                X = random.Next(50, pbMain.Width - 50);
                Y = random.Next(50, pbMain.Height - 50);
                timeRemaining = 30;
            }
            else if (isPlayerOverlapping)
            {
                timeRemaining = 30;
                X = random.Next(50, pbMain.Width - 50);
                Y = random.Next(50, pbMain.Height - 50);
                isPlayerOverlapping = false;
            }
        }
        public override void Render(Graphics g)
        {
            g.FillEllipse(
                new SolidBrush(Color.CornflowerBlue), -15, -15, 40, 40);
            g.DrawEllipse(
                new Pen(Color.CornflowerBlue, 2), -15, -15, 40, 40);
            g.DrawString(
                timeRemaining.ToString(),
                new Font("Verdana", 8), // шрифт и размер
                new SolidBrush(Color.Green), // цвет шрифта
                20, 20); // точка в которой нарисовать текст
        }

        public override GraphicsPath GetGraphicsPath()
        {
            var path = base.GetGraphicsPath();
            path.AddEllipse(-15, -15, 30, 30);
            return path;
        }

        public override void Overlap(BaseObject obj)
        {
            base.Overlap(obj);
            if (obj is Player)
            {
                isPlayerOverlapping = true;
            }
        }
    }
}
