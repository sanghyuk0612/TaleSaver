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
    public TMP_InputField emailInput; // ¶Ç´Â TMP_InputField
    public TMP_InputField passwordInput;
    public FirebaseAuthManager authManager;

    public void ShowLoginPanel()
    {
        loginPanel.SetActive(true);
        gameStartButton.SetActive(false);
        gameOptionButton.SetActive(false);
        gameLoginButton.SetActive(false);
        gameExitButton.SetActive(false);
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

        authManager.Login(email, password);
    }

    public void OnClickSignup()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        authManager.SignUp(email, password);
    }

}