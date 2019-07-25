using System;
namespace KAMI_HIKOKI
{
    public class Player : asd.TextureObject2D
    {
        int hp;
        public readonly int MaxHP;
        readonly asd.Vector2DF G = new asd.Vector2DF(0.0f, 1.0f);
        int WindCount;

        //プロパティ
        public asd.Vector2DF Speed { set; get; }//速度
        public asd.Vector2DF Acceleration { set; get; }//加速度
        asd.Joystick Joystick { set; get; }//ジョイスティック
        asd.CircleShape ShapeOfCollige { set; get; }//衝突判定
        public float MinSpeed { set; get; }//最低スピード
        public float MaxSpeed { set; get; }//最高スピード

        //HP
        public int HP
        {
            set
            {
                if (value < 0)
                {
                    hp = 0;
                }
                else if (value > MaxHP)
                {
                    hp = MaxHP;
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

        public Player()
        {
            Texture = asd.Engine.Graphics.CreateTexture2D("Image/Airplane.png");
            CenterPosition = Texture.Size.To2DF() / 2.0f;
            Joystick = asd.Engine.JoystickContainer.GetJoystickAt(0);

            MaxSpeed = 6.0f;
            MinSpeed = 2.0f;
            Speed = new asd.Vector2DF(5.0f, 0.0f);
            Acceleration = new asd.Vector2DF(0.0f, 0.0f);
            Position = new asd.Vector2DF(-30.0f, 240.0f);

            ShapeOfCollige = new asd.CircleShape();
            ShapeOfCollige.OuterDiameter = Texture.Size.X / 2.0f - 5.0f;
            ShapeOfCollige.Position = Position;
            MaxHP = 100;
            HP = MaxHP;

            WindCount = 0;
        }

        //更新
        protected override void OnUpdate()
        {
            //上昇・下降
            if (!asd.Engine.JoystickContainer.GetIsPresentAt(0))
            {
                if (asd.Engine.Keyboard.GetKeyState(asd.Keys.Up) == asd.KeyState.Hold)
                {
                    Speed += new asd.Vector2DF(-0.05f, -0.2f);
                }
                else if (asd.Engine.Keyboard.GetKeyState(asd.Keys.Down) == asd.KeyState.Hold)
                {
                    Speed += new asd.Vector2DF(0.05f, 0.2f);
                }
            }
            else
            {
                if (Joystick.GetAxisState(1) < 0.0f)
                {
                    Speed += new asd.Vector2DF(-0.05f, -0.2f);
                }
                else if (Joystick.GetAxisState(1) > 0.1f)
                {
                    Speed += new asd.Vector2DF(0.05f, 0.2f);
                }
            }

            if (WindCount > 0) 
            {
                Position += new asd.Vector2DF(-0.8f, -2.0f);
                WindCount--;
            }

            Speed += Acceleration;
            Position += Speed;

            if (Acceleration.Length > 0.01f) Acceleration -= Acceleration.Normal * 0.01f;
            else Acceleration = new asd.Vector2DF(0.0f, 0.0f);

            if (Position.Y < 25.0f) Position = new asd.Vector2DF(Position.X, 25.0f);
            ShapeOfCollige.Position = Position;
            Angle = Speed.Degree;

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
            else if (obj is Healer)
            {
                if (ShapeOfCollige.GetIsCollidedWith(((Healer)obj).ShapeOfCollige))
                {
                    HP += 10;
                    obj.Dispose();
                }
            }
            else if (obj is Wind)
            {
                if (ShapeOfCollige.GetIsCollidedWith(((Wind)obj).ShapeOfCollige))
                {
                    WindCount += 20;
                    obj.Dispose();
                }
            }
        }
    }
}
