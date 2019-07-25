using System;
namespace KAMI_HIKOKI
{
    public class Healer : asd.TextureObject2D
    {
        static asd.Texture2D TextureOfHealer;

        //プロパティ
        public asd.CircleShape ShapeOfCollige { set; get; }

        //共通初期化
        public static bool Init()
        {
            TextureOfHealer = asd.Engine.Graphics.CreateTexture2D("Image/Heal.png");
            if (TextureOfHealer == null) return false;
            return true;
        }

        //コンストラクタ
        public Healer(asd.Vector2DF position)
        {
            Position = position;
            Texture = TextureOfHealer;
            CenterPosition = Texture.Size.To2DF() / 2.0f;
            ShapeOfCollige = new asd.CircleShape();
            ShapeOfCollige.OuterDiameter = Texture.Size.X / 2.0f - 4.0f;
            ShapeOfCollige.Position = Position;
        }

        //更新
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
    }
}
