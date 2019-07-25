using System;
namespace KAMI_HIKOKI
{
    public enum TypeOfCloud
    {
        Small,
        Medium,
        Large
    }

    public class Cloud : asd.TextureObject2D
    {
        static Random random = new Random();
        static asd.Texture2D TextureOfCloud_S;
        static asd.Texture2D TextureOfCloud_M;
        static asd.Texture2D TextureOfCloud_L;

        //共通初期化
        public static bool Init()
        {
            TextureOfCloud_S = asd.Engine.Graphics.CreateTexture2D("Image/Cloud_S.png");
            TextureOfCloud_M = asd.Engine.Graphics.CreateTexture2D("Image/Cloud_M.png");
            TextureOfCloud_L = asd.Engine.Graphics.CreateTexture2D("Image/Cloud_L.png");
            if (TextureOfCloud_S == null || TextureOfCloud_M == null || TextureOfCloud_L == null) return false;
            return true;
        }

        //プロパティ
        public TypeOfCloud SizeOfCloud { get; }
        public asd.RectangleShape ShapeOfCollige { get; }//衝突判定
        public int Count { get; private set; }

        public Cloud(asd.Vector2DF position, TypeOfCloud typeOfCloud)
        {
            Position = position;
            SizeOfCloud = typeOfCloud;
            ShapeOfCollige = new asd.RectangleShape();
            DrawingPriority = 2;

            switch (SizeOfCloud)
            {
                case TypeOfCloud.Small:
                    Texture = TextureOfCloud_S;
                    break;
                case TypeOfCloud.Medium:
                    Texture = TextureOfCloud_M;
                    break;
                case TypeOfCloud.Large:
                    Texture = TextureOfCloud_L;
                    break;
            }
            CenterPosition = Texture.Size.To2DF() / 2.0f;
            ShapeOfCollige.DrawingArea =
                new asd.RectF(Position + new asd.Vector2DF(10.0f, 10.0f) - Texture.Size.To2DF() / 2.0f, Texture.Size.To2DF() - new asd.Vector2DF(10.0f, 10.0f));

            Count = 0;
        }

        //更新
        protected override void OnUpdate()
        {
            Count++;
            base.OnUpdate();
        }

		//雨生成
		public Rain GenerateRain()
        {
            if (SizeOfCloud == TypeOfCloud.Small && Count % 2 != 0) return null;

            asd.Vector2DF temp = Position;
            switch (SizeOfCloud)
            {
                case TypeOfCloud.Small:
                    temp.X += random.Next() % 30 - 15;
                    break;
                case TypeOfCloud.Medium:
                    temp.X += random.Next() % 130 - 65;
                    break;
                case TypeOfCloud.Large:
                    temp.X += random.Next() % 210 - 105;
                    break;
            }
            return new Rain(temp);
        }
    }
}
