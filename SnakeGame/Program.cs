using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnakeGame
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                SnakeGameManager.Play();
            }
            else if (args.Length == 2)
            {
                int.TryParse(args[0], out int width);
                int.TryParse(args[1], out int height);
                SnakeGameManager.Play(width, height);
            }

            else if (args.Length == 3)
            {
                int.TryParse(args[0], out int width);
                int.TryParse(args[1], out int height);
                int.TryParse(args[2], out int fruitcount);
                SnakeGameManager.Play(width, height, fruitcount);
            }
            else if (args.Length == 4)
            {
                int.TryParse(args[0], out int width);
                int.TryParse(args[1], out int height);
                int.TryParse(args[2], out int fruitcount);
                int.TryParse(args[3], out int deltaTime);
                SnakeGameManager.Play(width, height, fruitcount, deltaTime);
            }
        }
    }

    static class SnakeGameManager
    {
        static PointType[,] World { get; set; }
        static int MaxSnakeSize { get; set; }
        static int WorldWidth { get => World.GetLength(0); }
        static int WorldHeight { get => World.GetLength(1); }
        static Timer Update { get; set; }
        static event Action<ConsoleKey> Input;

        public static void Play(int worldWidth = 30, int worldHeight = 30, int fruitCount = 10, int deltaTime = 100)
        {
            if (worldWidth < 3 || worldHeight < 3) throw new Exception("The World min size must be 3!");
            if (deltaTime < 1) throw new Exception("The DeltaTime must be greater than 0!");

            int consoleWidth = worldWidth * 2;
            int consoleHeight = worldHeight + 1;
            Console.Title = "Snake Game";
            Console.SetWindowSize(consoleWidth, consoleHeight);
            Console.SetBufferSize(consoleWidth, consoleHeight);
            Console.CursorVisible = false;
            Console.Clear();

            World = new PointType[worldWidth, worldHeight];
            MaxSnakeSize = (WorldWidth - 2) * (WorldHeight - 2) - fruitCount;

            int downWellPos = worldHeight - 1;
            for(int x = 0; x < worldWidth; x++)
            {
                World[x, 0] = PointType.Well;
                Painter.Draw(x, 0, ConsoleColor.White);

                World[x, downWellPos] = PointType.Well;
                Painter.Draw(x, downWellPos, ConsoleColor.White);
            }
            int rightWellPos = worldWidth - 1;
            for(int y = 1; y < downWellPos; y++)
            {
                World[0, y] = PointType.Well;
                Painter.Draw(0, y, ConsoleColor.White);

                World[rightWellPos, y] = PointType.Well;
                Painter.Draw(rightWellPos, y, ConsoleColor.White);
            }
            Update = new Timer(deltaTime);

            Snake snake = new Snake(new Point(1, 1), 4, ConsoleColor.DarkGreen);

            for (int i = 0; i < fruitCount; i++)
                GenerateFruit();

            Update.Enabled = true;

            while (snake.IsLive)
                Input.Invoke(Console.ReadKey(true).Key);

            Update.Enabled = false;
            Console.Clear();
            Console.ResetColor();
        }

        static void GenerateFruit()
        {
            Point pos;
            do pos = new Point(Random.Range(1, WorldWidth - 1), Random.Range(1, WorldHeight - 1));
            while (World[pos.x, pos.y] != PointType.Free);

            World[pos.x, pos.y] = PointType.Fruit;
            Painter.Draw(pos, ConsoleColor.Red);
        }
        static class Random
        {
            private static System.Random random = new System.Random();

            public static int Range(int min, int max) =>
                random.Next(min, max);
        }
        class Snake
        {
            private List<Point> Body { get; set; }
            private Point previousDirection;
            
            private Point direction;
            public Point Direction
            {
                get => direction;
                set
                {
                    if (previousDirection.x != -value.x || previousDirection.y != -value.y)
                        direction = value;
                }
            }
            private ConsoleColor Color { get; set; }
            public bool IsLive { get; private set; }

            public Snake(Point point, int size, ConsoleColor color)
            {
                if (size < 0) throw new Exception("The snake size must be greater than 0!");
                if (point.x < 0 || point.x >= WorldWidth || point.y < 0 || point.y >= WorldHeight)
                    throw new Exception("The snake creating point must be in World");

                IsLive = true;

                Body = new List<Point>(size);

                for (int i = 0; i < size; i++)
                    Body.Add(point);

                Color = color;
                Painter.Draw(point, Color);

                Direction = Point.Right;

                Input += this.ChangeDirection;

                Update.Tick += this.Move; ;
            }

            private void Move()
            {
                previousDirection = direction;

                Point last = Body[Body.Count - 1];
                
                if (last != Body[Body.Count - 2])
                {
                    Painter.Clear(last);
                    World[last.x, last.y] = PointType.Free;
                }

                for (int i = Body.Count - 1; i > 0; i--)
                    Body[i] = Body[i - 1];
                Body[0] += Direction;

                if (World[Body[0].x, Body[0].y] == PointType.Well)
                {
                    for (int i = 1; i < Body.Count; i++)
                        Painter.Clear(Body[i]);

                    IsLive = false;
                    Update.Tick -= this.Move;
                    return;

                }
                if (World[Body[0].x, Body[0].y] == PointType.Fruit)
                {
                    Body.Add(last);
                    if (Body.Count == MaxSnakeSize)
                    {
                        IsLive = false;

                        Update.Tick -= this.Move;

                        Painter.Draw(last, Color);
                        World[last.x, last.y] = PointType.Well;

                        Painter.Draw(Body[0], Color);
                        World[Body[0].x, Body[0].y] = PointType.Well;

                        return;
                    }
                    GenerateFruit();

                }

                Painter.Draw(Body[0], Color);
                World[Body[0].x, Body[0].y] = PointType.Well;
            }

                

            private void ChangeDirection(ConsoleKey key)
            {

                 switch (key)
                 {
                     case ConsoleKey.LeftArrow: Direction = Point.Left; break;
                     case ConsoleKey.RightArrow: Direction = Point.Right; break;
                     case ConsoleKey.UpArrow: Direction = Point.Up; break;
                     case ConsoleKey.DownArrow: Direction = Point.Down; break;
                 }
            }
            
        }


        enum PointType
        {
            Free,
            Well,
            Fruit
        }

        static class Painter
        {
            public static void Draw(Point point, ConsoleColor color)
            {
                Console.SetCursorPosition(point.x * 2, point.y);
                Console.ForegroundColor = color;
                Console.Write("::");
            }
            public static void Draw (int x, int y, ConsoleColor color)
            {
                Console.SetCursorPosition(x * 2, y);
                Console.ForegroundColor = color;
                Console.Write("::");
            }
            public static void Clear(Point point)
            {
                Console.SetCursorPosition(point.x * 2, point.y);
                Console.Write("   ");
            }
            public static void Clear(int x, int y)
            {
                Console.SetCursorPosition(x * 2, y);
                Console.Write("   ");
            }
        }

        struct Point
        {
            public static readonly Point Zero = new Point(0, 0);
            public static readonly Point Left = new Point(-1, 0);
            public static readonly Point Right = new Point(1, 0);
            public static readonly Point Up = new Point(0, -1);
            public static readonly Point Down = new Point(0, 1);


            public int x {get; set;}
            public int y { get; set;}

            public Point(int x, int y)
            {
                this.x = x;
                this.y = y;
            }

            public static bool operator ==(Point ob1, Point ob2) =>
                ob1.x == ob2.x && ob1.y == ob2.y;
            public static bool operator !=(Point ob1, Point ob2) =>
               ob1.x == ob2.x && ob1.y == ob2.y;

            public static Point operator +(Point ob1, Point ob2) =>
                new Point(ob1.x + ob2.x, ob1.y + ob2.y);

        }
        class Timer
        {
            public int Interval { get; set; }
            public event Action Tick;
            private bool enabled;

            public bool Enabled
            {
                get => enabled;
                set
                {
                    if (value && !enabled)
                    Work();
                    enabled = value;
                }

            }
            
            public Timer(int interval)
            {
                Interval = interval;
            }

            private async void Work()
            {
                enabled = true;
                while(enabled)
                {
                    await Task.Delay(Interval);
                    Tick?.Invoke();
                }
            }
            
        }
    }
}
