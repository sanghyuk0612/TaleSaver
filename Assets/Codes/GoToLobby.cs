using UnityEngine;
using UnityEngine.SceneManagement;
public class GoToLobby : MonoBehaviour
{
    public void OnClickGoToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}
