using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using Firebase.Extensions;

public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    private bool isFirebaseReady = false;

    void Awake()
    {
        Debug.Log("?? FirebaseAuthManager Awake() 실행됨");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("? FirebaseAuthManager Singleton 등록 완료");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // FirebaseInit.cs에서 초기화 이후 호출할 것
    public void OnFirebaseInitialized()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
        isFirebaseReady = true;

        Debug.Log("? OnFirebaseInitialized 호출됨, Firebase 준비 완료!");
    }

    public void SignUp(string email, string password)
    {
        Debug.Log($"?? SignUp 호출됨 - isFirebaseReady = {isFirebaseReady}");

        if (!isFirebaseReady)
        {
            Debug.LogError("? Firebase 초기화가 완료되지 않았습니다.");
            return;
        }

        if (!isFirebaseReady)
        {
            Debug.LogError("? Firebase 초기화가 완료되지 않았습니다.");
            return;
        }
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("? 이메일 또는 비밀번호가 비어 있습니다.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                FirebaseUser newUser = task.Result.User;  // ? 여기 수정됨
                Debug.Log($"? 회원가입 성공: {newUser.Email} (UID: {newUser.UserId})");

                string fullEmail = newUser.Email;
                string uid = newUser.UserId;
                string username = fullEmail.Split('@')[0];

                Dictionary<string, object> userData = new Dictionary<string, object>()
                {
                    { "userId", fullEmail },
                    { "username", username }
                };

                firestore.Collection("users").Document(uid).SetAsync(userData).ContinueWithOnMainThread(docTask =>
                {
                    if (docTask.IsCompletedSuccessfully)
                        Debug.Log($"? Firestore에 사용자 문서 생성 완료: {fullEmail}");
                    else
                        Debug.LogError($"? Firestore 저장 실패: {docTask.Exception?.Message}");
                });
            }
            else
            {
                if (task.Exception != null)
                {
                    Exception innerEx = task.Exception.Flatten().InnerExceptions[0];
                    FirebaseException fbEx = innerEx as FirebaseException;
                    if (fbEx != null)
                    {
                        var errorCode = (AuthError)fbEx.ErrorCode;
                        switch (errorCode)
                        {
                            case AuthError.EmailAlreadyInUse:
                                Debug.LogError("? 이미 사용 중인 이메일입니다.");
                                break;
                            case AuthError.WeakPassword:
                                Debug.LogError("? 비밀번호가 너무 약합니다.");
                                break;
                            case AuthError.InvalidEmail:
                                Debug.LogError("? 이메일 형식이 잘못되었습니다.");
                                break;
                            default:
                                Debug.LogError($"? 기타 회원가입 오류: {errorCode} - {fbEx.Message}");
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"? 회원가입 중 알 수 없는 오류: {task.Exception.Message}");
                    }
                }
            }
        });
    }

    public void Login(string email, string password)
    {
        if (!isFirebaseReady)
        {
            Debug.LogError("? Firebase 초기화가 완료되지 않았습니다.");
            return;
        }
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("? 이메일 또는 비밀번호가 비어 있습니다.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                FirebaseUser user = task.Result.User;  // ? 여기 수정됨
                Debug.Log($"? 로그인 성공: {user.Email} (UID: {user.UserId})");
            }
            else
            {
                if (task.Exception != null)
                {
                    Exception innerEx = task.Exception.Flatten().InnerExceptions[0];
                    FirebaseException fbEx = innerEx as FirebaseException;
                    if (fbEx != null)
                    {
                        var errorCode = (AuthError)fbEx.ErrorCode;
                        switch (errorCode)
                        {
                            case AuthError.WrongPassword:
                                Debug.LogError("? 비밀번호가 틀렸습니다.");
                                break;
                            case AuthError.InvalidEmail:
                                Debug.LogError("? 이메일 형식이 잘못되었습니다.");
                                break;
                            case AuthError.UserNotFound:
                                Debug.LogError("? 해당 이메일로 가입된 사용자가 없습니다.");
                                break;
                            default:
                                Debug.LogError($"? 기타 로그인 오류: {errorCode} - {fbEx.Message}");
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($"? 로그인 중 알 수 없는 오류: {task.Exception.Message}");
                    }
                }
            }
        });
    }
}
