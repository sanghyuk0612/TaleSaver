
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
            Debug.LogError("?? loginWarningText가 null입니다! Inspector 연결 확인 필요!");
            return;
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
                Debug.LogError("?? LoginWarningText를 자동으로 찾을 수 없습니다!");
            }
            else
            {
                Debug.Log("? LoginWarningText를 코드에서 자동으로 연결했습니다.");
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
                alertText.text = " 로그인에 성공하였습니다.";
                loginWarningText.gameObject.SetActive(false);
                Invoke(nameof(HideLoginPanel), 1.0f);
            }
            else
            {
                alertText.text = message;

                // ? 실패 시 UI를 정확히 로그인 전 상태로 돌려주기
                loginPanel.SetActive(true);
                gameStartButton.SetActive(false);
                gameLoginButton.SetActive(false);
                gameOptionButton.SetActive(false);
                gameExitButton.SetActive(false); 

                emailInput.interactable = true;
                passwordInput.interactable = true;

                // 필요하면 재포커싱
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(emailInput.gameObject);
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

        // ?입력 필드와 버튼 잠깐 비활성화 (중복 클릭 방지용, 선택 사항)
        emailInput.interactable = true;
        passwordInput.interactable = true;

        authManager.SignUp(email, password, (success, message) =>
        {
            if (success)
            {
                Debug.Log(" 회원가입 성공 콜백");
                alertText.text = " 회원가입 되었습니다.";

                // 성공 시 원한다면 로그인 패널 자동으로 닫거나 초기화 가능
                // HideLoginPanel();
            }
            else
            {
                Debug.LogError(" 회원가입 실패 콜백: " + message);
                alertText.text = message;

                // ? 실패 시 입력 UI 복구
                loginPanel.SetActive(true); // 혹시 꺼졌다면 다시 표시
                emailInput.interactable = true;
                passwordInput.interactable = true;

                // ? 입력 필드 자동 포커싱
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(emailInput.gameObject);
            }
        });
    }
}
