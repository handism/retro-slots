using UnityEngine;
using UnityEngine.SceneManagement;

namespace SlotGame.Core
{
    /// <summary>
    /// タイトルシーンの表示と遷移を担当するマネージャー。
    /// </summary>
    public class TitleManager : MonoBehaviour
    {
        public void StartGame()
        {
            // Boot シーンで生成された GameContextInitializer（DontDestroyOnLoad）が
            // GameManager の Awake() で参照されるため、非同期ロードで Main シーンへ遷移する。
            SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
        }
    }
}
