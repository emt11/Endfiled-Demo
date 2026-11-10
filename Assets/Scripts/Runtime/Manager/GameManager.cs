using UnityEngine;

namespace GP
{
    public class GameManager : SingletonMono<GameManager>
    {
        protected override bool IsPersistent => true;

        public static bool GameRunning { get; private set; } = false;

        public static GameMode GameMode { get; private set; } = GameMode.MainMenu;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void GameManagerWake()
        {
            GameRunning = true;
            Instance.ForceWakeUp();
        }

        public static void SetGameMode(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.MainMenu:
                    GameMode = GameMode.MainMenu;
                    ShowCursor(true);
                    break;
                case GameMode.Menu:
                    GameMode = GameMode.Menu;
                    ShowCursor(true);
                    break;
                case GameMode.Game:
                    GameMode = GameMode.Game;
                    ShowCursor(false);
                    break;
                case GameMode.Loading:
                    GameMode = GameMode.Loading;
                    ShowCursor(false);
                    break;
            }
        }

        public static GameObject RequireGameObject()
        {
            return new GameObject();
        }

        public static GameObject RequireGameObject(GameObject prefab)
        {
            return Instantiate(prefab);
        }

        public static T RequireGameObject<T>() where T : Component
        {
            var newObject = RequireGameObject();

            return newObject.AddComponent<T>();
        }

        public static void DestroyGameObject(GameObject objectToDestroy)
        {
            Destroy(objectToDestroy);
        }

        protected override void AwakeSingleton()
        {
            base.AwakeSingleton();

            SetGameMode(GameMode.Game);
        }

        private static void ShowCursor(bool show)
        {
            Cursor.visible = show;
            Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
        }

        protected override void OnApplicationQuit()
        {
            GameRunning = false;

            base.OnApplicationQuit();
        }
    }

    public enum GameMode
    {
        [InspectorName("游戏模式")] Game,
        [InspectorName("菜单模式")] Menu,
        [InspectorName("暂停模式")] Loading,
        [InspectorName("主菜单模式")] MainMenu,
    }
}
