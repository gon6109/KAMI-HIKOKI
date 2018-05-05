using System;
namespace KAMI_HIKOKI
{
    public class Wind : asd.TextureObject2D
    {
        static asd.Texture2D TextureOfWind;
        int CountOfAnime;

        //プロパティ
        public asd.CircleShape ShapeOfCollige { set; get; }
        public bool IsMove { set; get; }

        //共通初期化
        public static bool Init()
        {
            TextureOfWind = asd.Engine.Graphics.CreateTexture2D("./Resource/Image/Wind.png");
            if (TextureOfWind == null) return false;
            return true;
        }

        public Wind(asd.Vector2DF position)
        {
            Position = position;
            Texture = TextureOfWind;
            CenterPosition = Texture.Size.To2DF() / 2.0f;
            ShapeOfCollige = new asd.CircleShape();
            ShapeOfCollige.OuterDiameter = Texture.Size.X / 2.0f - 5.0f;
            ShapeOfCollige.Position = Position;
            IsMove = false;
            Src = new asd.RectF(0.0f, 0.0f, 50.0f, 50.0f);
            CountOfAnime = 0;
        }

        //更新
        protected override void OnUpdate()
        {
            if (IsMove) Position += new asd.Vector2DF(-2.0f, 0.0f);
            ShapeOfCollige.Position = Position;
            CountOfAnime++;
            //Src = new asd.RectF((float)(CountOfAnime % 50 / 10) * 50.0f, 0.0f, 50.0f, 50.0f);

            base.OnUpdate();
        }
    }
}
