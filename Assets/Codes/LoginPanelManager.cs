using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using TMPro;
using UnityEngine.EventSystems;

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

    void Awake()
    {
        // 필요한 참조들을 찾아서 할당
        FindAndAssignReferences();
    }
    
    // 참조들을 자동으로 찾아서 할당하는 메서드
    private void FindAndAssignReferences()
    {
        // loginPanel이 할당되지 않았으면 찾기
        if (loginPanel == null)
        {
            loginPanel = GameObject.Find("LoginPanel");
            if (loginPanel == null)
            {
                // 태그나 이름으로 찾기 시도
                loginPanel = GameObject.FindWithTag("LoginPanel");
                
                if (loginPanel == null)
                {
                    // 씬에서 "Login" 또는 "Panel"이 포함된 GameObject 찾기
                    GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
                    foreach (GameObject obj in allObjects)
                    {
                        if (obj.name.Contains("Login") && obj.name.Contains("Panel"))
                        {
                            loginPanel = obj;
                            break;
                        }
                    }
                }
            }
            
            if (loginPanel != null)
            {
                Debug.Log("loginPanel을 자동으로 찾아 할당했습니다: " + loginPanel.name);
            }
            else
            {
                Debug.LogError("loginPanel을 찾을 수 없습니다. Inspector에서 수동으로 할당해주세요.");
            }
        }
        
        // 이메일 입력 필드 찾기
        if (emailInput == null && loginPanel != null)
        {
            // loginPanel의 자식에서 이메일 관련 TMP_InputField 찾기
            TMP_InputField[] inputFields = loginPanel.GetComponentsInChildren<TMP_InputField>(true);
            foreach (TMP_InputField field in inputFields)
            {
                if (field.name.Contains("Email") || field.gameObject.name.Contains("Email") ||
                    (field.placeholder != null && field.placeholder.GetComponent<TextMeshProUGUI>() != null && 
                    field.placeholder.GetComponent<TextMeshProUGUI>().text.Contains("이메일")))
                {
                    emailInput = field;
                    Debug.Log("emailInput을 자동으로 찾아 할당했습니다: " + emailInput.name);
                    break;
                }
            }
            
            if (emailInput == null && inputFields.Length > 0)
            {
                // 첫 번째 TMP_InputField를 이메일 필드로 가정
                emailInput = inputFields[0];
                Debug.Log("emailInput을 첫 번째 InputField로 할당했습니다: " + emailInput.name);
            }
        }
        
        // 비밀번호 입력 필드 찾기
        if (passwordInput == null && loginPanel != null)
        {
            // loginPanel의 자식에서 비밀번호 관련 TMP_InputField 찾기
            TMP_InputField[] inputFields = loginPanel.GetComponentsInChildren<TMP_InputField>(true);
            foreach (TMP_InputField field in inputFields)
            {
                if (field.name.Contains("Password") || field.gameObject.name.Contains("Password") ||
                    (field.placeholder != null && field.placeholder.GetComponent<TextMeshProUGUI>() != null && 
                    field.placeholder.GetComponent<TextMeshProUGUI>().text.Contains("비밀번호")))
                {
                    passwordInput = field;
                    Debug.Log("passwordInput을 자동으로 찾아 할당했습니다: " + passwordInput.name);
                    break;
                }
            }
            
            if (passwordInput == null && inputFields.Length > 1)
            {
                // 두 번째 TMP_InputField를 비밀번호 필드로 가정
                passwordInput = inputFields[1];
                Debug.Log("passwordInput을, 두 번째 InputField로 할당했습니다: " + passwordInput.name);
            }
        }
        
        // alertText 찾기
        if (alertText == null && loginPanel != null)
        {
            // loginPanel의 자식에서 alert 관련 TMP_Text 찾기
            alertText = loginPanel.GetComponentInChildren<TMP_Text>(true);
            if (alertText != null)
            {
                Debug.Log("alertText를 자동으로 찾아 할당했습니다: " + alertText.name);
            }
        }
        
        // loginWarningText 할당
        if (loginWarningText == null)
        {
            loginWarningText = GameObject.Find("LoginWarningText")?.GetComponent<TMP_Text>();
            if (loginWarningText == null)
            {
                Debug.LogError("LoginWarningText가 존재하지 않습니다! Inspector 설정 확인 필요!");
            }
            else
            {
                Debug.Log("LoginWarningText가 존재합니다. 존재 확인 완료.");
            }
        }
        
        // 버튼들 찾기
        if (gameStartButton == null)
            gameStartButton = GameObject.Find("GameStartButton");
            
        if (gameOptionButton == null)
            gameOptionButton = GameObject.Find("GameOptionButton");
            
        if (gameLoginButton == null)
            gameLoginButton = GameObject.Find("GameLoginButton");
            
        if (gameExitButton == null)
            gameExitButton = GameObject.Find("GameExitButton");
            
        // authManager 찾기
        if (authManager == null)
            authManager = FindObjectOfType<FirebaseAuthManager>();
    }

    void Start()
    {
        if (loginWarningText == null)
        {
            Debug.LogError("?? loginWarningText가 null입니다! Inspector 설정 확인 필요!");
            return;
        }

        // InputField에 클릭 이벤트 추가 (마우스 클릭으로만 텍스트 지우기)
        if (emailInput != null)
        {
            // 이전 이벤트 리스너 제거
            emailInput.onSelect.RemoveAllListeners();
            
            // 이메일 필드의 초기 텍스트 저장
            string emailDefaultText = emailInput.text;
            bool emailFirstClick = true;
            
            // 마우스 클릭시에만 발동하는 로직
            emailInput.onSelect.AddListener((string text) => {
                // 첫 클릭시에만 비우기
                if (emailFirstClick && Input.mousePresent && Input.GetMouseButton(0))
                {
                emailInput.text = "";
                    emailFirstClick = false;
                }
            });
        }
        
        if (passwordInput != null)
        {
            // 이전 이벤트 리스너 제거
            passwordInput.onSelect.RemoveAllListeners();
            
            // 비밀번호 필드의 초기 상태 관리 변수
            bool passwordFirstFocus = true;
            
            // 비밀번호 필드 선택 이벤트 - 처음 포커스될 때만 내용 지우기
            passwordInput.onSelect.AddListener((string text) => {
                if (passwordFirstFocus)
                {
                passwordInput.text = "";
                    passwordFirstFocus = false;
                    Debug.Log("비밀번호 필드 첫 포커스 - 내용 지움");
                }
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
        
        // 키보드 네비게이션을 위한 설정
        SetupKeyboardNavigation();
    }

    // 키보드 네비게이션 설정
    private void SetupKeyboardNavigation()
    {
        if (emailInput != null && passwordInput != null)
        {
            // Navigation 설정 (코드로 네비게이션 설정)
            Navigation emailNav = new Navigation();
            emailNav.mode = Navigation.Mode.Explicit;
            emailNav.selectOnDown = passwordInput;
            
            Navigation passwordNav = new Navigation();
            passwordNav.mode = Navigation.Mode.Explicit;
            passwordNav.selectOnUp = emailInput;
            
            // 네비게이션 적용
            emailInput.navigation = emailNav;
            passwordInput.navigation = passwordNav;
        }
    }
    
    // Update 메서드 수정
    void Update()
    {
        // loginPanel 변수가 Inspector에서 할당되지 않았으면 아무것도 하지 않음
        if (loginPanel == null)
            {
            Debug.LogWarning("loginPanel이 할당되지 않았습니다. Inspector에서 확인해주세요.");
            return;
        }
        
        // 로그인 패널이 활성화되어 있을 때만 키 입력 처리
        if (!loginPanel.activeSelf || emailInput == null || passwordInput == null || EventSystem.current == null)
            return;
        
        // Enter 키 감지 - 로그인 버튼 자동 클릭
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // 로그인 창이 활성화 되어 있을 때만 작동
            if (loginPanel.activeSelf)
            {
                // 로그인 함수 직접 호출
                OnClickLogin();
                Debug.Log("Enter 키 감지 - 로그인 시도");
            }
        }
            
        // 탭 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // 현재 선택된 게임 오브젝트 확인
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            
            // 현재 선택된 객체가 없으면 이메일 필드 선택
            if (currentSelected == null)
            {
                EventSystem.current.SetSelectedGameObject(emailInput.gameObject);
                return;
            }
            
            if (currentSelected == emailInput.gameObject)
            {
                // 이메일 입력란에서 비밀번호 입력란으로 이동
                EventSystem.current.SetSelectedGameObject(passwordInput.gameObject);
                
                // 비밀번호 필드의 텍스트 커서를 맨 끝으로 이동
                if (!string.IsNullOrEmpty(passwordInput.text))
                {
                    passwordInput.caretPosition = passwordInput.text.Length;
                }
            }
            else if (currentSelected == passwordInput.gameObject)
            {
                // 비밀번호 입력란에서 이메일 입력란으로 이동 (Shift+Tab 효과)
                EventSystem.current.SetSelectedGameObject(emailInput.gameObject);
                
                // 이메일 필드의 텍스트 커서를 맨 끝으로 이동
                if (!string.IsNullOrEmpty(emailInput.text))
                {
                    emailInput.caretPosition = emailInput.text.Length;
                }
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
        
        // 이메일 입력란에 자동 포커스 제거
        // 사용자가 직접 클릭해야 필드가 선택됨
    }

    public void HideLoginPanel()
    {
        // 참조된 게임 오브젝트들의 null 체크 추가
        if (loginPanel != null)
        loginPanel.SetActive(false);
            
        if (gameStartButton != null)
        gameStartButton.SetActive(true);
            
        if (gameOptionButton != null)
        gameOptionButton.SetActive(true);
            
        if (gameLoginButton != null)
        gameLoginButton.SetActive(true);
            
        if (gameExitButton != null)
        gameExitButton.SetActive(true);
            
        Debug.Log("로그인 패널 닫힘");
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

                StartCoroutine(WaitForGoodsDataThenLoadLobby());
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
    private IEnumerator WaitForGoodsDataThenLoadLobby()
    {
        float timeout = 5f;
        float timer = 0f;

        // 재화 데이터 로딩 기다리기 (0은 기본값일 수 있으니 최소 시간만 체크)
        while ((GameDataManager.Instance.storybookPage == 0 && GameDataManager.Instance.machineParts == 0) && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        Debug.Log("📦 재화 로딩 완료. LobbyScene으로 이동합니다.");
        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }
}
