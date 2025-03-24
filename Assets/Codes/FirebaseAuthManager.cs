using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;

public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseAuth auth;

    void Start()
    {
        // Firebase 인증 인스턴스 가져오기
        auth = FirebaseAuth.DefaultInstance;
    }

    // ?? 회원가입 함수
    public void SignUp(string email, string password)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("? 회원가입 실패: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.Log("? 회원가입 성공: " + newUser.Email);
        });
    }

    // ?? 로그인 함수
    public void Login(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("? 로그인 실패: " + task.Exception);
                return;
            }

            FirebaseUser user = task.Result.User;
            Debug.Log("? 로그인 성공: " + user.Email);
        });
    }
}