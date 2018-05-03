using System;
namespace KAMI_HIKOKI
{
    public class Player : asd.TextureObject2D
    {
        float speed;
        int hp;

        //プロパティ
        asd.Joystick Joystick { set; get; }//ジョイスティック
        asd.CircleShape ShapeOfCollige { set; get; }//衝突判定
        public readonly int MaxHP;

        //HP
        public int HP 
        { 
            set
            {
                if (value < 0)
                {
                    hp = 0;
                }
                else
                {
                    hp = value;
                }
            }
            get
            {
                return hp;
            }
        }

        //スピード
        public float Speed
        {
            set
            {
                if (value < 2.0f)
                {
                    speed = 2.0f;
                }
                else
                {
                    speed = value;
                }
            }
            get
            {
                return speed;
            }
        }

        public Player()
        {
            Texture = asd.Engine.Graphics.CreateTexture2D("./Resource/Image/Airplane.png");
            CenterPosition = Texture.Size.To2DF() / 2.0f;

            Joystick = asd.Engine.JoystickContainer.GetJoystickAt(0);

            Speed = 5.0f;
            Position = new asd.Vector2DF(-30.0f, 240.0f);

            ShapeOfCollige = new asd.CircleShape();
            ShapeOfCollige.OuterDiameter = Texture.Size.X / 2.0f - 5.0f;
            ShapeOfCollige.Position = Position;
            MaxHP = 100;
            HP = MaxHP;
        }

        //更新
        protected override void OnUpdate()
        {
            //前に進む
            Position += new asd.Vector2DF(Speed, 0.1f);

            //上昇・下降
            Position += new asd.Vector2DF(0.0f, Joystick.GetAxisState(1) * 3.0f);
            if (Joystick.GetAxisState(1) < 0.0f)
            {
                Speed -= 0.1f;
            }
            else if (Joystick.GetAxisState(1) > 0.5f)
            {
                Speed += 0.1f;
            }

            ShapeOfCollige.Position = Position;

            Speed += 0.001f;
            base.OnUpdate();
        }

        //衝突判定
        public void ColligeWith(asd.Object2D obj)
        {
            if (obj == null) return;

            if (obj is Wall)
            {
                if (ShapeOfCollige.GetIsCollidedWith(((Wall)obj).ShapeOfCollige))
                {
                    HP = 0;
                }
            }
            else if (obj is Cloud)
            {
                if (ShapeOfCollige.GetIsCollidedWith(((Cloud)obj).ShapeOfCollige))
                {
                    HP += -1;
                }
            }
            else if (obj is Rain)
            {
                if (ShapeOfCollige.GetIsCollidedWith(((Rain)obj).ShapeOfCollige))
                {
                    HP += -1;
                    obj.Dispose();
                }
            }
        }
    }
}
