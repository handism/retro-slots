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
            SceneManager.LoadScene("Main");
        }
    }
}
