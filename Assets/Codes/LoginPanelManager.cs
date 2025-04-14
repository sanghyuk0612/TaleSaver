using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using TMPro;

public class LoginPanelManager : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject gameStartButton;
    public GameObject gameOptionButton;
    public GameObject gameLoginButton;
    public GameObject gameExitButton;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text alertText;
    public FirebaseAuthManager authManager;
    public TMP_Text loginWarningText;

    void Start()
    {
        if (loginWarningText == null)
        {
            Debug.LogError("?? loginWarningText가 null입니다! Inspector 설정 확인 필요!");
            return;
        }

        // InputField에 클릭 이벤트 추가
        if (emailInput != null)
        {
            emailInput.onSelect.AddListener((string text) => {
                emailInput.text = "";
            });
        }
        if (passwordInput != null)
        {
            passwordInput.onSelect.AddListener((string text) => {
                passwordInput.text = "";
            });
        }

        if (authManager != null && authManager.IsLoggedIn())
        {
            loginWarningText.gameObject.SetActive(false);
        }
        else
        {
            loginWarningText.gameObject.SetActive(true);
        }
    }

    void Awake()
    {
        if (loginWarningText == null)
        {
            loginWarningText = GameObject.Find("LoginWarningText")?.GetComponent<TMP_Text>();
            if (loginWarningText == null)
            {
                Debug.LogError("?? LoginWarningText가 존재하지 않습니다! Inspector 설정 확인 필요!");
            }
            else
            {
                Debug.Log("? LoginWarningText가 존재합니다. 존재 확인 완료.");
            }
        }
    }

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        gameStartButton.SetActive(false);
        gameOptionButton.SetActive(false);
        gameLoginButton.SetActive(false);
        gameExitButton.SetActive(false);
        alertText.text = "";
    }

    public void HideLoginPanel()
    {
        loginPanel.SetActive(false);
        gameStartButton.SetActive(true);
        gameOptionButton.SetActive(true);
        gameLoginButton.SetActive(true);
        gameExitButton.SetActive(true);
    }

    public void OnClickLogin()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        authManager.Login(email, password, (success, message) =>
        {
            if (success)
            {
                alertText.text = " 로그인 성공했습니다.";
                loginWarningText.gameObject.SetActive(false);
                Invoke(nameof(HideLoginPanel), 1.0f);
            }
            else
            {
                alertText.text = message;

                // ? 로그인 실패 시 UI 재확인 필요
                loginPanel.SetActive(true);
                gameStartButton.SetActive(false);
                gameLoginButton.SetActive(false);
                gameOptionButton.SetActive(false);
                gameExitButton.SetActive(false); 

                emailInput.interactable = true;
                passwordInput.interactable = true;

                // 입력 후 커서 이동
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(emailInput.gameObject);
            }
        });
    }

    public void OnClickSignup()
    {
        Debug.Log(" 회원가입 확인");

        string email = emailInput.text;
        string password = passwordInput.text;

        if (authManager == null)
        {
            Debug.LogError(" authManager가 null입니다! Inspector 설정 확인 필요");
            return;
        }

        // ?입력 후 확인 필요 (고려 필요, 추가 필요)
        emailInput.interactable = true;
        passwordInput.interactable = true;

        authManager.SignUp(email, password, (success, message) =>
        {
            if (success)
            {
                Debug.Log(" 회원가입 성공");
                alertText.text = " 회원가입 성공했습니다.";

                // 성공 시 로그인 후 자동 로그인 처리
                // HideLoginPanel();
            }
            else
            {
                Debug.LogError(" 회원가입 실패: " + message);
                alertText.text = message;

                // ? 입력 UI 재확인
                loginPanel.SetActive(true); // 재확인 후 재입력
                emailInput.interactable = true;
                passwordInput.interactable = true;

                // ? 입력 후 커서 이동
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(emailInput.gameObject);
            }
        });
    }
}
