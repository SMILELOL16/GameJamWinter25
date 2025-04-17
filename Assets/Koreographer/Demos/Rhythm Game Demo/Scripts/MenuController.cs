using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] GameObject creditsPanel;
    [SerializeField] GameObject pauseMenu;
    bool _isPaused = false;
    bool _isCreditsOpen = false;

    void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
    }

    public void PlayGame() => SceneManager.LoadScene("Gameplay");
   
    public void QuitGame() => Application.Quit();

    public void OpenCreditsPanel()
    {
        _isCreditsOpen = !_isCreditsOpen;
        creditsPanel.SetActive(_isCreditsOpen);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    //^^Called by Buttons^^

    //VVCalled by keyboardVV
    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        _isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _isPaused = !_isPaused;

            if (_isPaused) { Pause(); }
            else { Resume(); }
        }
    }
}
