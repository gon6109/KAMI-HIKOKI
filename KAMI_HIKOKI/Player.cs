using System;
namespace KAMI_HIKOKI
{
    public class Player : asd.TextureObject2D
    {
        float speed;
        float acceleration;
        int hp;
        public readonly int MaxHP;

        //プロパティ
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
                if (value < MinSpeed)
                {
                    speed = MinSpeed;
                }
                else if (value > MaxSpeed)
                {
                    speed = MaxSpeed;
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

        //加速度
        public float Acceleration
        {
            set
            {
                if (value < -1.0f)
                {
                    acceleration = -1.0f;
                }
                else if (value > 1.0f)
                {
                    acceleration = 1.0f;
                }
                else
                {
                    acceleration = value;
                }
            }
            get
            {
                return acceleration;
            }
        }

        public Player()
        {
            Texture = asd.Engine.Graphics.CreateTexture2D("./Resource/Image/Airplane.png");
            CenterPosition = Texture.Size.To2DF() / 2.0f;

            Joystick = asd.Engine.JoystickContainer.GetJoystickAt(0);

            MaxSpeed = 6.0f;
            MinSpeed = 2.0f;
            Speed = 4.0f;
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
            asd.Vector2DF velocity = new asd.Vector2DF(Speed, 0.1f);
            Position += new asd.Vector2DF(Speed, 0.1f);

            //上昇・下降
            Position += new asd.Vector2DF(0.0f, Joystick.GetAxisState(1) * 4.0f);
            velocity += new asd.Vector2DF(0.0f, Joystick.GetAxisState(1) * 4.0f);
            if (Joystick.GetAxisState(1) < 0.0f)
            {
                Acceleration -= 0.05f;
            }
            else if (Joystick.GetAxisState(1) > 0.1f)
            {
                Acceleration += 0.05f;
            }
            else
            {
                Acceleration = 0;
            }

            Speed += Acceleration;

            ShapeOfCollige.Position = Position;

            //MaxSpeed += 0.001f;
            //MinSpeed += 0.001f;
            //Speed += 0.001f;

            if (Position.Y < 25.0f) Position = new asd.Vector2DF(Position.X,25.0f);
            Angle = velocity.Degree;

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
