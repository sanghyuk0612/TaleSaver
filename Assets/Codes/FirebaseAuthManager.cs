
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using Firebase.Extensions;
using System.Collections;



public class FirebaseAuthManager : MonoBehaviour
{
    private FirebaseUser currentUser;
    public FirebaseUser CurrentUser => currentUser;
    public static FirebaseAuthManager Instance { get; private set; }

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    private bool isFirebaseReady = false;
    public FirebaseAuth Auth => auth;

    void Awake()
    {
        Debug.Log(" FirebaseAuthManager Awake() 실행됨");

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log(" FirebaseAuthManager Singleton 등록 완료");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnFirebaseInitialized()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;
        isFirebaseReady = true;

        if (currentUser != null)
            Debug.Log("현재 로그인된 UID: " + currentUser.UserId);
        else
            Debug.Log("현재 로그인된 사용자 없음");

        Debug.Log("OnFirebaseInitialized 호출됨, Firebase 준비 완료!");
        Debug.Log("현재 로그인된 UID: " + auth.CurrentUser?.UserId);
    }
    public bool IsLoggedIn()
    {
        if (!isFirebaseReady || currentUser == null)
        {
            Debug.LogWarning("IsLoggedIn(): Firebase 미초기화 또는 currentUser가 null입니다.");
            return false;
        }
        return true;
    }

    public IEnumerator WaitForLoginReady(System.Action onReady)


    {
        float timer = 0f;
        float timeout = 5f;

        while (auth == null || auth.CurrentUser == null)
        {
            Debug.Log("? 로그인 상태 기다리는 중...");
            timer += Time.deltaTime;
            if (timer > timeout)
            {
                Debug.LogError("? 로그인 준비 시간 초과!");
                yield break;
            }
            yield return null;
        }

        Debug.Log("? Firebase 로그인 준비 완료!");
        onReady?.Invoke();
    }

    public void SignUp(string email, string password, Action<bool, string> callback)
    {
        Debug.Log($" SignUp 호출됨 - isFirebaseReady = {isFirebaseReady}");

        if (!isFirebaseReady)
        {
            Debug.LogError(" Firebase 초기화가 완료되지 않았습니다.");
            callback(false, "Firebase 초기화가 완료되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError(" 이메일 또는 비밀번호가 비어 있습니다.");
            callback(false, "이메일 또는 비밀번호가 비어 있습니다.");
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                currentUser = task.Result.User;
                FirebaseUser newUser = task.Result.User;
                Debug.Log($" 회원가입 성공: {newUser.Email} (UID: {newUser.UserId})");

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
                        Debug.Log($" Firestore에 사용자 문서 생성 완료: {fullEmail}");
                    else
                        Debug.LogError($" Firestore 저장 실패: {docTask.Exception?.Message}");
                });
                Dictionary<string, object> goodsData = new Dictionary<string, object>()
                {
                    { "storybookpages", 0 },
                    { "machineparts", 0 }
                };

                firestore.Collection("goods").Document(uid).SetAsync(goodsData).ContinueWithOnMainThread(goodsTask =>
                {
                    if (goodsTask.IsCompletedSuccessfully)
                    {
                        Debug.Log($"기본 goods 데이터 생성 완료 for UID: {uid}");
                    }
                    else
                    {
                        Debug.LogError($"goods 저장 실패: {goodsTask.Exception?.Message}");
                    }
                });

                callback(true, "회원가입 성공");
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
                                Debug.LogError(" 이미 사용 중인 이메일입니다.");
                                callback(false, "이미 사용 중인 이메일입니다.");
                                break;
                            case AuthError.WeakPassword:
                                Debug.LogError(" 비밀번호가 너무 약합니다.");
                                callback(false, "비밀번호가 너무 약합니다.");
                                break;
                            case AuthError.InvalidEmail:
                                Debug.LogError(" 이메일 형식이 잘못되었습니다.");
                                callback(false, "이메일 형식이 잘못되었습니다.");
                                break;
                            default:
                                Debug.LogError($" 기타 회원가입 오류: {errorCode} - {fbEx.Message}");
                                callback(false, fbEx.Message);
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogError($" 회원가입 중 알 수 없는 오류: {task.Exception.Message}");
                        callback(false, task.Exception.Message);
                    }
                }
            }
        });
    }

    public void Login(string email, string password, Action<bool, string> callback)
    {
        if (!isFirebaseReady)
        {
            Debug.LogError(" Firebase 초기화가 완료되지 않았습니다.");
            callback(false, "Firebase 초기화가 완료되지 않았습니다.");
            return;
        }
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError(" 이메일 또는 비밀번호가 비어 있습니다.");
            callback(false, "이메일 또는 비밀번호가 비어 있습니다.");
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                currentUser = task.Result.User;
                Debug.Log("? currentUser 세팅됨: " + currentUser?.Email);
                Debug.Log("? IsLoggedIn(): " + IsLoggedIn());
                Debug.Log("? Auth.CurrentUser: " + auth?.CurrentUser?.Email);
                FirebaseUser user = task.Result.User;
                Debug.Log($" 로그인 성공: {user.Email} (UID: {user.UserId})");
                string uid = user.UserId;

                firestore.Collection("goods").Document(uid).GetSnapshotAsync().ContinueWithOnMainThread(goodsTask =>
                {
                    if (goodsTask.IsCompletedSuccessfully)
                    {
                        DocumentSnapshot snapshot = goodsTask.Result;
                        if (snapshot.Exists)
                        {
                            Dictionary<string, object> goodsData = snapshot.ToDictionary();
                            Debug.Log(" 유저의 goods 정보 불러오기 성공");

                            foreach (var kvp in goodsData)
                            {
                                Debug.Log($"{kvp.Key} : {kvp.Value}");
                            }
                            GameDataManager.Instance.SetGoodsData(goodsData);
                        }
                        else
                        {
                            Debug.LogWarning(" 해당 UID의 goods 문서가 존재하지 않습니다.");
                        }
                    }
                    else
                    {
                        Debug.LogError($" goods 불러오기 실패: {goodsTask.Exception?.Message}");
                    }
                });
                callback(true, "로그인 성공");
            }
            else
            {
                if (task.Exception != null)
                {
                    try
                    {
                        var flatEx = task.Exception.Flatten();
                        if (flatEx.InnerExceptions.Count > 0)
                        {
                            FirebaseException fbEx = flatEx.InnerExceptions[0] as FirebaseException;

                            if (fbEx != null)
                            {
                                var errorCode = (AuthError)fbEx.ErrorCode;
                                switch (errorCode)
                                {
                                    case AuthError.EmailAlreadyInUse:
                                        Debug.LogWarning("이미 사용 중인 이메일입니다.");
                                        callback(false, "이미 사용 중인 이메일입니다.");
                                        break;
                                    case AuthError.WeakPassword:
                                        Debug.LogWarning("비밀번호가 너무 약합니다.");
                                        callback(false, "비밀번호가 너무 약합니다.");
                                        break;
                                    case AuthError.InvalidEmail:
                                        Debug.LogWarning("이메일 형식이 잘못되었습니다.");
                                        callback(false, "이메일 형식이 잘못되었습니다.");
                                        break;
                                    default:
                                        Debug.LogWarning($"기타 회원가입 오류: {errorCode}");
                                        callback(false, "회원가입 중 오류가 발생했습니다.");
                                        break;
                                }
                            }
                            else
                            {
                                Debug.LogWarning("알 수 없는 Firebase 오류.");
                                callback(false, "회원가입 중 알 수 없는 오류가 발생했습니다.");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("예외 처리 중 오류 발생: " + ex.Message);
                        callback(false, "회원가입 중 시스템 오류가 발생했습니다.");
                    }
                }
            }
        });
    }
    public IEnumerator WaitUntilUserIsReady(Action onReady)
    {
        // currentUser가 null이거나 UID가 비어있으면 기다림
        yield return new WaitUntil(() =>
            FirebaseAuth.DefaultInstance.CurrentUser != null &&
            !string.IsNullOrEmpty(FirebaseAuth.DefaultInstance.CurrentUser.UserId)
        );

        // 완료되면 currentUser 설정하고 콜백 실행
        currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        Debug.Log("? 로그인 준비 완료! UID: " + currentUser.UserId);
        onReady?.Invoke();
    }
}
