using System;
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
                    Game = new GameMgr();
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
            if (!asd.Engine.JoystickContainer.GetIsPresentAt(0)) return false;
            if (!Wall.Init()) return false;
            if (!Cloud.Init()) return false;
            if (!Rain.Init()) return false;
                
            Game = new GameMgr();
            asd.Engine.ChangeScene(Game);

            return true;
        }

        //終了処理
        static void End()
        {
            asd.Engine.Terminate();
        }
    }
}
