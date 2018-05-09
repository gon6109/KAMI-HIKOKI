using System;
using System.IO;
using System.Text;
namespace KAMI_HIKOKI
{
    public class Program
    {
        //シーン
        static GameMgr Game;

        [STAThread]
        static void Main(string[] args)
        {

            //初期化
            if (!Init())
            {
                Console.WriteLine("Error");
                return;
            }

            //メインループ
            while (asd.Engine.DoEvents())
            {
                asd.Engine.Update();

                //リスタート
                if (!Game.IsAlive)
                {
                    int best = Game.BestScore;
                    Game = new GameMgr(best, true);
                    asd.Engine.ChangeScene(Game);
                }
            }

            //終了処理
            End();
        }

        //初期化
        static bool Init()
        {
            if (!asd.Engine.Initialize("KAMI_HIKOKI", 640, 480, new asd.EngineOption())) return false;
            if (!Wall.Init()) return false;
            if (!Cloud.Init()) return false;
            if (!Rain.Init()) return false;
            if (!Healer.Init()) return false;
            if (!Wind.Init()) return false;

            int best = 0;
            try
            {
                StreamReader file = new StreamReader("Score", Encoding.Default);
                best = Convert.ToInt32(file.ReadLine());
                file.Close();
            }
            catch
            {
                System.Diagnostics.Debug.Write("Error");
            }

            Game = new GameMgr(best,false);
            asd.Engine.ChangeScene(Game);

            return true;
        }

        //終了処理
        static void End()
        {
            StreamWriter file = new StreamWriter("Score");
            file.WriteLine(Game.BestScore);
            file.Close();

            asd.Engine.Terminate();
        }
    }
}
