
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
        // 게임 시작 시 로그인 상태 확인
        if (authManager != null && authManager.IsLoggedIn())
        {
            loginWarningText.gameObject.SetActive(false);
        }
        else
        {
            loginWarningText.gameObject.SetActive(true);
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
        Debug.Log(" 로그인 버튼 눌림");

        string email = emailInput.text;
        string password = passwordInput.text;

        if (authManager == null)
        {
            Debug.LogError("? authManager가 null입니다! Inspector 연결 누락");
            return;
        }

        authManager.Login(email, password, (success, message) =>
        {
            if (success)
            {
                Debug.Log(" 로그인 성공 콜백");
                alertText.text = " 로그인에 성공하였습니다.";
                loginWarningText.gameObject.SetActive(false); // ? 경고 숨기기
                Invoke(nameof(HideLoginPanel), 1.0f);
            }
            else
            {
                Debug.LogError(" 로그인 실패 콜백: " + message);
                alertText.text = " " + message;
            }
        });
    }

    public void OnClickSignup()
    {
        Debug.Log(" 회원가입 버튼 눌림");

        string email = emailInput.text;
        string password = passwordInput.text;

        if (authManager == null)
        {
            Debug.LogError(" authManager가 null입니다! Inspector 연결 누락");
            return;
        }

        authManager.SignUp(email, password, (success, message) =>
        {
            if (success)
            {
                Debug.Log(" 회원가입 성공 콜백");
                alertText.text = " 회원가입 되었습니다.";
            }
            else
            {
                Debug.LogError(" 회원가입 실패 콜백: " + message);
                alertText.text = "? " + message;
            }
        });
    }
}
