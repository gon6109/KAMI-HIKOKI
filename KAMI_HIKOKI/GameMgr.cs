using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
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
        const int MakeMapSize = 2000;
        static Random random = new Random();


        //プロパティ
        public asd.Layer2D LayerOfBackGround { set; get; }//背景
        public asd.Layer2D LayerOfMain { set; get; }//スクロールするレイヤー
        public asd.CameraObject2D CameraOfMain { set; get; }//メインカメラ
        public asd.Layer2D LayerOfStatus { set; get; }//ステータス
        public asd.Layer2D LayerOfGameOver { set; get; }//ゲームオーバー

        public asd.TextureObject2D BackGround { set; get; }//背景
        public Player Airplane { set; get; }//自機
        public List<Wall> Walls { set; get; }//壁
        public SortedList<float, float> PairsOfWall; //壁-雨衝突判定アクセラレータ
        public List<Cloud> Clouds { set; get; }//雲
        public List<Rain> Rains { set; get; }//雨
        public asd.GeometryObject2D HPBar { set; get; }//HPバー
        public asd.RectangleShape BoxOfHPBar { set; get; }
        public const int WidthOfHPBar = 500;
        public asd.TextObject2D TextOfHP { set; get; }//テキスト「HP」
        public asd.TextObject2D TextOfGameOver { set; get; }//テキスト「Game Over」
        public int CountOfGameOver;//ゲームオーバー用カウンタ

        public int Level { set; get; }
        public int Count { set; get; }

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

            PairsOfWall = new SortedList<float, float>();
            Level = 1;
            Count = 0;
            //LoadMap("./Resource/MapData/Map.csv");
            MakeMap(100.0f);
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
            //Collige();

            //HPバー
            BoxOfHPBar.DrawingArea = new asd.RectF(120, 440, (float)Airplane.HP / (float)Airplane.MaxHP * (float)WidthOfHPBar, 20);

            //マップ生成
            if ((Convert.ToInt32(Airplane.Position.X) - 100) % MakeMapSize > MakeMapSize - MakeMapSize * 0.8f && 100 + (Level - 1) * MakeMapSize < Airplane.Position.X)
            {
                Level++;
                MakeMap(100.0f + (Level - 1) * MakeMapSize);
            }

            //ゲームオーバー処理
            if (Airplane.HP == 0) GameOver();

            //オブジェクト破棄
            if (Count % 5 == 0) DisposeObject();

            Count++;

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

        //マップ生成
        public void MakeMap(float x)
        {
            int countOfCloud = 0;

            //地面の壁の配置
            for (int i = 0; i < MakeMapSize / Wall.TextureOfWall.Size.X; i++)
            {
                //地面の壁の配置
                CreateObject(new asd.Vector2DF(x + i * Wall.TextureOfWall.Size.X + Wall.TextureOfWall.Size.X / 2, 455.0f), 0);

                int temp = random.Next() % 1000;

                //雲
                if (temp < Level * 2 && countOfCloud <= 0)
                {
                    CreateObject(new asd.Vector2DF(x + i * Wall.TextureOfWall.Size.X + Wall.TextureOfWall.Size.X * 2.5f, 40.0f), 3);
                    countOfCloud = 5;
                }
                else if (temp < Level * 6 && countOfCloud <= 0)
                {
                    CreateObject(new asd.Vector2DF(x + i * Wall.TextureOfWall.Size.X + Wall.TextureOfWall.Size.X * 1.5f, random.Next() % 2 * 50 + 25.0f), 2);
                    countOfCloud = 3;
                }
                else if (temp < Level * 14 && countOfCloud <= 0)
                {
                    CreateObject(new asd.Vector2DF(x + i * Wall.TextureOfWall.Size.X + Wall.TextureOfWall.Size.X / 2.0f, random.Next() % 2 * 50 + 25.0f), 1);
                    countOfCloud = 1;
                }

                //壁
                if (temp < Level * 8 || countOfCloud > 0)
                {
                    CreateObject(new asd.Vector2DF(x + i * Wall.TextureOfWall.Size.X + Wall.TextureOfWall.Size.X / 2.0f, random.Next() % 5 * 50 + 155.0f), 0);
                }
                for (int l = 0; l < Level / 3 + 1; l++)
                {
                    if (random.Next() % 1000 < Level * 8)
                    {
                        CreateObject(new asd.Vector2DF(x + i * Wall.TextureOfWall.Size.X + Wall.TextureOfWall.Size.X / 2.0f, random.Next() % 7 * 50 + 105.0f), 0);
                    }
                }
                countOfCloud--;
            }
        }

        //敵オブジェクト配置
        void CreateObject(asd.Vector2DF position, int obj)
        {
            switch (obj)
            {
                case 0:
                    Walls.Add(new Wall(position));
                    LayerOfMain.AddObject(Walls[Walls.Count - 1]);
                    SetWallRainColligeData(Walls[Walls.Count - 1].Position);
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

        //アクセラレータデータの設定
        void SetWallRainColligeData(asd.Vector2DF position)
        {
            if (PairsOfWall.ContainsKey(position.X - Wall.TextureOfWall.Size.X / 2.0f))
            {
                if (PairsOfWall[position.X - Wall.TextureOfWall.Size.X / 2.0f] > position.Y)
                    PairsOfWall[position.X - Wall.TextureOfWall.Size.X / 2.0f] = position.Y - Wall.TextureOfWall.Size.Y / 2.0f;
            }
            else
            {
                PairsOfWall.Add(position.X - Wall.TextureOfWall.Size.X / 2.0f, position.Y - Wall.TextureOfWall.Size.Y / 2.0f);
            }
        }

        //アクセラレータデータをRainに登録
        void RegisterWallRainColligeData(Rain rain)
        {
            KeyValuePair<float, float> lessThan = new KeyValuePair<float, float>();
            KeyValuePair<float, float> lessEqual = new KeyValuePair<float, float>();
            KeyValuePair<float, float> greaterEqual = new KeyValuePair<float, float>();
            KeyValuePair<float, float> greaterThan = new KeyValuePair<float, float>();
            GetBoundKeys<float, float>(PairsOfWall, rain.Position.X, out lessThan, out lessEqual, out greaterEqual, out greaterThan);
            rain.YLimit = lessEqual.Value - 5.0f;
        }

        //衝突判定
        void Collige()
        {
            foreach (var item in Clouds)
            {
                Airplane.ColligeWith(item as asd.Object2D);
            }

            foreach (var item in Rains)
            {
                Airplane.ColligeWith(item as asd.Object2D);
            }

            foreach (var item in Walls)
            {
                Airplane.ColligeWith(item as asd.Object2D);
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
                    RegisterWallRainColligeData(rain);
                }
            }
        }

        //オブジェクト破棄
        void DisposeObject()
        {
            for (int i = 0; i < Walls.Count; i++)
            {
                if (Walls[i].Position.X < CameraOfMain.Src.Position.X - 300.0f)
                {
                    Walls[i].Dispose();
                    Walls.RemoveAt(i);
                    if (Walls.Count == i) break;
                    i--;
                }
                else if (!Walls[i].IsAlive)
                {
                    Walls.RemoveAt(i);
                    if (Walls.Count == i) break;
                    i--;
                }
            }

            for (int i = 0; i < Clouds.Count; i++)
            {
                if (Clouds[i].Position.X < CameraOfMain.Src.Position.X - 300.0f)
                {
                    Clouds[i].Dispose();
                    Clouds.RemoveAt(i);
                    if (Clouds.Count == i) break;
                    i--;
                }
                else if (!Clouds[i].IsAlive)
                {
                    Clouds.RemoveAt(i);
                    if (Clouds.Count == i) break;
                    i--;
                }
            }

            for (int i = 0; i < Rains.Count; i++)
            {
                if (Rains[i].Position.X < CameraOfMain.Src.Position.X - 300.0f)
                {
                    Rains[i].Dispose();
                    Rains.RemoveAt(i);
                    if (Rains.Count == i) break;
                    i--;
                }
                else if (!Rains[i].IsAlive)
                {
                    Rains.RemoveAt(i);
                    if (Rains.Count == i) break;
                    i--;
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

        void GetBoundKeys<TKey, TValue>(
    IEnumerable<KeyValuePair<TKey, TValue>> sortedList,
    TKey key,
    out KeyValuePair<TKey, TValue> lessThan,
    out KeyValuePair<TKey, TValue> lessEqual,
    out KeyValuePair<TKey, TValue> greaterEqual,
    out KeyValuePair<TKey, TValue> greaterThan,
    IComparer<TKey> comparer = null)
        {
            var comp = comparer ?? Comparer<TKey>.Default;

            lessThan =
                sortedList.LastOrDefault(
                    kv => comp.Compare(kv.Key, key) < 0);
            lessEqual =
                sortedList.LastOrDefault(
                    kv => comp.Compare(kv.Key, key) <= 0);
            greaterEqual =
                sortedList.FirstOrDefault(
                    kv => comp.Compare(kv.Key, key) >= 0);
            greaterThan =
                sortedList.FirstOrDefault(
                    kv => comp.Compare(kv.Key, key) > 0);
        }
    }
}
