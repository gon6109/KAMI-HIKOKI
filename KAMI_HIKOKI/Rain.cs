using System;
namespace KAMI_HIKOKI
{
    public class Rain : asd.TextureObject2D
    {
        static asd.Texture2D TextureOfRain;

        //共通初期化
        public static bool Init()
        {
            TextureOfRain = asd.Engine.Graphics.CreateTexture2D("./Resource/Image/Rain.png");
            if (TextureOfRain == null) return false;
            return true;
        }

        public asd.CircleShape ShapeOfCollige { set; get; }//衝突判定

        public Rain(asd.Vector2DF position)
        {
            Position = position;
            Texture = TextureOfRain;

            ShapeOfCollige = new asd.CircleShape();
            ShapeOfCollige.Position = Position;
            ShapeOfCollige.OuterDiameter = Texture.Size.X - 2.0f;

            DrawingPriority = 1;
        }

        //更新
        protected override void OnUpdate()
        {
            Position += new asd.Vector2DF(0.0f, 3.0f);
            ShapeOfCollige.Position = Position;
            base.OnUpdate();

            if (Position.Y > 650.0f) Dispose();
        }
    }
}
