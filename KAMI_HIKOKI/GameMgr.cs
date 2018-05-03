using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
namespace KAMI_HIKOKI
{
    //状態
    public enum ReadState
    {
        X,
        Y,
        Object
    }

    public class GameMgr : asd.Scene
    {
        //プロパティ
        public asd.Layer2D LayerOfBackGround { set; get; }//背景
        public asd.Layer2D LayerOfMain { set; get; }//スクロールするレイヤー
        public asd.CameraObject2D CameraOfMain { set; get; }//メインカメラ
        public asd.Layer2D LayerOfStatus { set; get; }//ステータス
        public asd.Layer2D LayerOfGameOver { set; get; }//ゲームオーバー

        public asd.TextureObject2D BackGround { set; get; }//背景
        public Player Airplane { set; get; }//自機
        public List<Wall> Walls { set; get; }//壁
        public List<Cloud> Clouds { set; get; }//雲
        public List<Rain> Rains { set; get; }//雨
        public asd.GeometryObject2D HPBar { set; get; }//HPバー
        public asd.RectangleShape BoxOfHPBar { set; get; }
        public const int WidthOfHPBar = 500;
        public asd.TextObject2D TextOfHP { set; get; }//テキスト「HP」
        public asd.TextObject2D TextOfGameOver { set; get; }//テキスト「Game Over」
        public int CountOfGameOver;

        public GameMgr()
        {
            //レイヤー登録
            {
                LayerOfBackGround = new asd.Layer2D();
                AddLayer(LayerOfBackGround);
                LayerOfMain = new asd.Layer2D();
                AddLayer(LayerOfMain);
                CameraOfMain = new asd.CameraObject2D();
                CameraOfMain.Src = new asd.RectI(0, 0, asd.Engine.WindowSize.X, asd.Engine.WindowSize.Y);
                CameraOfMain.Dst = new asd.RectI(0, 0, asd.Engine.WindowSize.X, asd.Engine.WindowSize.Y);
                LayerOfMain.AddObject(CameraOfMain);
                LayerOfStatus = new asd.Layer2D();
                AddLayer(LayerOfStatus);
            }

            //オブジェクト配置

            //背景
            {
                BackGround = new asd.TextureObject2D();
                BackGround.Texture = asd.Engine.Graphics.CreateTexture2D("./Resource/Image/BackGround.png");
                LayerOfBackGround.AddObject(BackGround);
            }

            //メイン
            {
                Airplane = new Player();
                LayerOfMain.AddObject(Airplane);
                Walls = new List<Wall>();
                Clouds = new List<Cloud>();
                Rains = new List<Rain>();
            }

            //ステータス
            {
                HPBar = new asd.GeometryObject2D();
                BoxOfHPBar = new asd.RectangleShape();
                BoxOfHPBar.DrawingArea = new asd.RectF(120, 440, Airplane.HP / Airplane.MaxHP * WidthOfHPBar, 20);
                HPBar.Shape = BoxOfHPBar;
                HPBar.Color = new asd.Color(255, 0, 0);

                TextOfHP = new asd.TextObject2D();
                TextOfHP.Font = asd.Engine.Graphics.CreateDynamicFont("", 20, new asd.Color(255, 0, 0), 0, new asd.Color(255, 0, 0));
                TextOfHP.Text = "HP";
                TextOfHP.CenterPosition = TextOfHP.Font.CalcTextureSize(TextOfHP.Text, asd.WritingDirection.Horizontal).To2DF() / 2.0f;
                TextOfHP.Position = new asd.Vector2DF(70.0f, 450.0f);

                LayerOfStatus.AddObject(HPBar);
                LayerOfStatus.AddObject(TextOfHP);
            }

            //ゲームオーバー
            {
                LayerOfGameOver = new asd.Layer2D();
                TextOfGameOver = new asd.TextObject2D();
                TextOfGameOver.Font = asd.Engine.Graphics.CreateDynamicFont("", 100, new asd.Color(0, 0, 0), 0, new asd.Color(255, 255, 255));
                TextOfGameOver.Text = "Game Over";
                TextOfGameOver.CenterPosition = TextOfGameOver.Font.CalcTextureSize(TextOfGameOver.Text, asd.WritingDirection.Horizontal).To2DF() / 2.0f;
                TextOfGameOver.Position = new asd.Vector2DF(320.0f, 600.0f);

                LayerOfGameOver.AddObject(TextOfGameOver);

                CountOfGameOver = 0;
            }

            LoadMap("./Resource/MapData/Map.csv");
        }

        //更新１
        protected override void OnUpdating()
        {
            //雨生成
            GenerateRain();

            base.OnUpdating();
        }

        //更新２
        protected override void OnUpdated()
        {
            //カメラ
            if (Airplane.Position.X > asd.Engine.WindowSize.X / 2)
                CameraOfMain.Src = new asd.RectI(Convert.ToInt32(Airplane.Position.X) - asd.Engine.WindowSize.X / 2, 0, asd.Engine.WindowSize.X, asd.Engine.WindowSize.Y);

            //衝突判定
            Collige();

            //HPバー
            BoxOfHPBar.DrawingArea = new asd.RectF(120, 440, (float)Airplane.HP / (float)Airplane.MaxHP * (float)WidthOfHPBar, 20);

            //ゲームオーバー処理
            if (Airplane.HP == 0) GameOver();

            //オブジェクト破棄
            DisposeObject();

            base.OnUpdated();
        }

        //マップロード
        public void LoadMap(string path)
        {
            StreamReader file = new StreamReader(path, Encoding.Default);

            int c = 0;
            string str = "";
            ReadState state = ReadState.X;
            asd.Vector2DF pos = new asd.Vector2DF();
            while ((c = file.Read()) != -1)
            {
                switch (c)
                {
                    case '/':
                        file.ReadLine();
                        str = "";
                        break;
                    case ',':
                        switch (state)
                        {
                            case ReadState.X:
                                pos.X = Convert.ToSingle(str);
                                state = ReadState.Y;
                                break;
                            case ReadState.Y:
                                pos.Y = Convert.ToSingle(str);
                                state = ReadState.Object;
                                break;
                        }
                        str = "";
                        break;
                    case '\n':
                        CreateObject(pos, Convert.ToInt32(str));
                        state = ReadState.X;
                        str = "";
                        break;
                    default:
                        str += Convert.ToChar(c);
                        break;
                }
            }
            CreateObject(pos, Convert.ToInt32(str));
            file.Close();
        }

        //敵オブジェクト配置
        void CreateObject(asd.Vector2DF position, int obj)
        {
            switch (obj)
            {
                case 0:
                    Walls.Add(new Wall(position));
                    LayerOfMain.AddObject(Walls[Walls.Count - 1]);
                    break;
                case 1:
                    Clouds.Add(new Cloud(position, TypeOfCloud.Small));
                    LayerOfMain.AddObject(Clouds[Clouds.Count - 1]);
                    break;
                case 2:
                    Clouds.Add(new Cloud(position, TypeOfCloud.Medium));
                    LayerOfMain.AddObject(Clouds[Clouds.Count - 1]);
                    break;
                case 3:
                    Clouds.Add(new Cloud(position, TypeOfCloud.Large));
                    LayerOfMain.AddObject(Clouds[Clouds.Count - 1]);
                    break;
                case 4:
                    break;
            }
        }

        //衝突判定
        void Collige()
        {
            foreach (var item in LayerOfMain.Objects)
            {
                Airplane.ColligeWith(item as asd.Object2D);

                if (item is Wall)
                {
                    List<Rain> rainsOfColliged = Rains.FindAll((Rain obj) => ((Wall)item).ShapeOfCollige.GetIsCollidedWith(obj.ShapeOfCollige));
                    foreach (var item2 in rainsOfColliged)
                    {
                        item2.Dispose();
                    }
                }
            }
        }

        //雨生成
        void GenerateRain()
        {
            foreach (var item in LayerOfMain.Objects)
            {
                if (item is Cloud)
                {
                    if (item.Position.X > CameraOfMain.Src.Position.X + CameraOfMain.Src.Width + 1300.0f) continue;
                    Rain rain = ((Cloud)item).GenerateRain();
                    if (rain == null) continue;
                    LayerOfMain.AddObject(rain);
                    Rains.Add(rain);
                }
            }
        }

        //オブジェクト破棄
        void DisposeObject()
        {
            foreach (var item in LayerOfMain.Objects)
            {
                if (item.Position.X < CameraOfMain.Src.Position.X - 300.0f)
                {
                    if (!(item is asd.CameraObject2D)) item.Dispose();
                }
            }
        }

        //ゲームオーバー処理
        void GameOver()
        {
            LayerOfMain.IsUpdated = false;
            if (CountOfGameOver == 0)
            {
                AddLayer(LayerOfGameOver);
            }
            if (CountOfGameOver < 120)
            {
                TextOfGameOver.Position += new asd.Vector2DF(0.0f, -3.0f);
            }
            else if (CountOfGameOver == 150)
            {
                Dispose();
            }
            CountOfGameOver++;
        }
    }
}
