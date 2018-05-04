using System;
namespace KAMI_HIKOKI
{
    public class Wall : asd.TextureObject2D
    {
        public static asd.Texture2D TextureOfWall;

        //共通初期化
        public static bool Init()
        {
            TextureOfWall = asd.Engine.Graphics.CreateTexture2D("./Resource/Image/Wall.png");
            if (TextureOfWall == null) return false;
            return true;
        }

        public asd.RectangleShape ShapeOfCollige { get; }//衝突判定

        public Wall(asd.Vector2DF position)
        {
            Position = position;
            Texture = TextureOfWall;
            CenterPosition = Texture.Size.To2DF() / 2.0f;

            ShapeOfCollige = new asd.RectangleShape();
            ShapeOfCollige.DrawingArea = new asd.RectF(Position - CenterPosition, Texture.Size.To2DF());
        }

        //更新
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}
