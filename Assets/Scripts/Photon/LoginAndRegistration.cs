using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Application = UnityEngine.Application;

#pragma warning disable 618

namespace Photon
{
    public class LoginAndRegistration : MonoBehaviour
    {
        private const string LoginUrl = "https://shicr.ru/BloxTank/PHP/Login.php";
        private const string SignUpUrl = "https://shicr.ru/BloxTank/PHP/Register.php";
        private const string ResetUrl = "https://shicr.ru/BloxTank/PHP/resetPassword.php";
    
        public InputField loginName;
        public InputField loginPassword;
        public InputField regName;
        public InputField regMail;
        public InputField regPassword;
        public InputField repeatPassword;
        public InputField resetMail;

        public GameObject resetFrame;
        public GameObject succesResetFrame;
    
        public Button signUpBtn;
        public Button resetPasswordBtn;

        [SerializeField] private Color red;
        [SerializeField] private Color green;

        private AndroidJavaObject _currentActivity, _context, _js, _toast2;
        private AndroidJavaClass _unityPlayer, _toast;

        private enum LoginType
        {
            Manual,
            Automatic
        }

        private void Awake()
        {
            if (PlayerPrefs.GetString("Name", "").Equals("") || PlayerPrefs.GetString("Password", "").Equals(""))
                return;
            StartCoroutine(_login(LoginType.Automatic));
        }

        public void Register()
        {
            StartCoroutine(_registerUser());
        }

        public void Login()
        {
            StartCoroutine(_login(LoginType.Manual));
        }

        public void ResetPassword()
        {
            StartCoroutine(_resetPassword());
        }

        private IEnumerator _registerUser()
        {
            var form = new WWWForm();
            form.AddField("Mail", regMail.text);
            form.AddField("Name", regName.text);
            form.AddField("Password", regPassword.text);
            
            var www = UnityWebRequest.Post(SignUpUrl, form);
            
            yield return www.SendWebRequest();
            
            var text = www.downloadHandler.text;
            Debug.Log(text);
            
            if (www.error != null) yield break;
            
            var code = int.Parse(text.Split()[0]);
            
            if (Application.platform == RuntimePlatform.Android)
            {
                _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                _currentActivity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                _context = _currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                _toast = new AndroidJavaClass("android.widget.Toast");
            }

            switch (code)
            {
                case -1:
                    Debug.LogError("Connection error: -01");
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        _js = new AndroidJavaObject("java.lang.String", "Connection error");
                        _toast2 = _toast.CallStatic<AndroidJavaObject>("makeText", _context, _js,
                            _toast.GetStatic<int>("LENGTH_LONG"));
                        _toast2.Call("show");
                    }
                    break;
                case 0:
                    Debug.Log("Succes: 00");
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        _js = new AndroidJavaObject("java.lang.String", "Succes! Check your email");
                        _toast2 = _toast.CallStatic<AndroidJavaObject>("makeText", _context, _js,
                            _toast.GetStatic<int>("LENGTH_SHORT"));
                        _toast2.Call("show");
                    }
                    
                    PlayerPrefs.SetString("Mail", regMail.text);
                    PlayerPrefs.SetString("Name", regName.text);
                    PlayerPrefs.SetString("Password", regPassword.text);
                    break;
                case 1:
                    Debug.LogError("Name is set error: 01");
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        _js = new AndroidJavaObject("java.lang.String", "User already exists");
                        _toast2 = _toast.CallStatic<AndroidJavaObject>("makeText", _context, _js,
                            _toast.GetStatic<int>("LENGTH_LONG"));
                        _toast2.Call("show");
                    }
                    break;
                case 2:
                    Debug.LogError("Mail is set error: 02");
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        _js = new AndroidJavaObject("java.lang.String", "User with this email already exists");
                        _toast2 = _toast.CallStatic<AndroidJavaObject>("makeText", _context, _js,
                            _toast.GetStatic<int>("LENGTH_LONG"));
                        _toast2.Call("show");
                    }
                    break;
                case 3:
                    Debug.LogError("Request error: 03");
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        _js = new AndroidJavaObject("java.lang.String", "Request error");
                        _toast2 = _toast.CallStatic<AndroidJavaObject>("makeText", _context, _js,
                            _toast.GetStatic<int>("LENGTH_LONG"));
                        _toast2.Call("show");
                    }
                    break;
            }
        }

        private IEnumerator _login(LoginType request)
        {
            var form = new WWWForm();
            string username, password;
            switch (request)
            {
                case LoginType.Automatic:
                
                    username = PlayerPrefs.GetString("Name", "");
                    password = PlayerPrefs.GetString("Password", "");
                    break;
                case LoginType.Manual:
                    username = loginName.text;
                    password = loginPassword.text;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(request), request, null);
            }
        
            form.AddField("Name", username);
            form.AddField("Pass", password);

            var www = UnityWebRequest.Post(LoginUrl, form);
            yield return www.SendWebRequest();

            var text = www.downloadHandler.text;
            Debug.Log(text);
            if (www.error != null) yield break;
            var code = int.Parse(text.Split()[0]);
            if (Application.platform == RuntimePlatform.Android)
            {
                _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                _currentActivity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                _context = _currentActivity.Call<AndroidJavaObject>("getApplicationContext");
                _toast = new AndroidJavaClass("android.widget.Toast");
            }

            switch (code)
            {
                case -1:
                    Debug.LogError("Connection error: -01");
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        _js = new AndroidJavaObject("java.lang.String", "Connection error");
                        _toast2 = _toast.CallStatic<AndroidJavaObject>("makeText", _context, _js,
                            _toast.GetStatic<int>("LENGTH_LONG"));
                        _toast2.Call("show");
                    }
                    break;
                case 0:
                    Debug.Log("Succes: 00");
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        _js = new AndroidJavaObject("java.lang.String", "Succes!");
                        _toast2 = _toast.CallStatic<AndroidJavaObject>("makeText", _context, _js,
                            _toast.GetStatic<int>("LENGTH_SHORT"));
                        _toast2.Call("show");
                    }
                    var data = text.Split(";");
                    PlayerPrefs.SetString("Mail", data[1]);
                    PlayerPrefs.SetString("Name", username);
                    PlayerPrefs.SetString("Password", password);
                    SceneManager.LoadScene("SampleScene");
                    break;
                case 1:
                    Debug.LogError("Wrong password: 01");
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        _js = new AndroidJavaObject("java.lang.String", "Wrong password");
                        _toast2 = _toast.CallStatic<AndroidJavaObject>("makeText", _context, _js,
                            _toast.GetStatic<int>("LENGTH_LONG"));
                        _toast2.Call("show");
                    }
                    if (request != LoginType.Automatic)
                    {
                        loginPassword.image.color = red;
                        loginPassword.text = "";
                        loginPassword.placeholder.GetComponent<Text>().text = "Wrong password";
                        resetPasswordBtn.gameObject.SetActive(true);
                        yield break;
                    }

                    PlayerPrefs.SetString("Mail", "");
                    PlayerPrefs.SetString("Name", "");
                    PlayerPrefs.SetString("Password", "");
                    break;
                case 2:
                    Debug.LogError("Wrong nick: 02");
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        _js = new AndroidJavaObject("java.lang.String", "Wrong nickname");
                        _toast2 = _toast.CallStatic<AndroidJavaObject>("makeText", _context, _js,
                            _toast.GetStatic<int>("LENGTH_LONG"));
                        _toast2.Call("show");
                    }
                    if (request != LoginType.Automatic)
                    {
                        loginName.text = "";
                        loginName.image.color = red;
                        loginName.placeholder.GetComponent<Text>().text = "Player does not exist";
                        yield break;
                    }

                    PlayerPrefs.SetString("Mail", "");
                    PlayerPrefs.SetString("Name", "");
                    PlayerPrefs.SetString("Password", "");
                    break;
                case 3:
                    Debug.LogError("Email has not been verified error: 03");
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        _js = new AndroidJavaObject("java.lang.String", "Verify your email address");
                        _toast2 = _toast.CallStatic<AndroidJavaObject>("makeText", _context, _js,
                            _toast.GetStatic<int>("LENGTH_LONG"));
                        _toast2.Call("show");
                    }
                    if (request != LoginType.Automatic)
                    {
                        loginName.text = "";
                        loginName.image.color = red;
                        loginName.placeholder.GetComponent<Text>().text = "Verify the email, please";
                    }
                    break;
                case 4:
                    Debug.LogError("Request error: 04");
                    if(Application.platform == RuntimePlatform.Android)
                    {
                        _js = new AndroidJavaObject("java.lang.String", "Request error");
                        _toast2 = _toast.CallStatic<AndroidJavaObject>("makeText", _context, _js,
                            _toast.GetStatic<int>("LENGTH_LONG"));
                        _toast2.Call("show");
                    }
                    break;
            }
        }

        private IEnumerator _resetPassword()
        {
            var form = new WWWForm();
            form.AddField("Mail", resetMail.text);
            var www = UnityWebRequest.Post(ResetUrl, form);
            yield return www.SendWebRequest();
            var text = www.downloadHandler.text;
            Debug.Log(text);

            if (www.error != null) yield break;

            var code = int.Parse(text.Split()[0]);
            
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext");
            var toast = new AndroidJavaClass("android.widget.Toast");
            switch (code)
            {
                case -1:
                    Debug.LogError("Connection error: -01");
                    var js = new AndroidJavaObject("java.lang.String", "Connection error");
                    var toast2 = toast.CallStatic<AndroidJavaObject>("makeText", context, js,
                        toast.GetStatic<int>("LENGTH_LONG"));
                    toast2.Call("show");
                    break;
                case 0:
                    Debug.Log("Succes: 00");
                    js = new AndroidJavaObject("java.lang.String", "Succes!");
                    toast2 = toast.CallStatic<AndroidJavaObject>("makeText", context, js,
                        toast.GetStatic<int>("LENGTH_SHORT"));
                    toast2.Call("show");
                    resetFrame.SetActive(false);
                    succesResetFrame.SetActive(true);
                    break;
                case 1:
                    Debug.LogError("Wrong mail: 01");
                    js = new AndroidJavaObject("java.lang.String", "Wrong email address");
                    toast2 = toast.CallStatic<AndroidJavaObject>("makeText", context, js,
                        toast.GetStatic<int>("LENGTH_LONG"));
                    toast2.Call("show");
                    resetMail.image.color = red;
                    resetMail.placeholder.GetComponent<Text>().text = "Player does not exist";
                    break;
                case 2:
                    Debug.LogError("Request error: 02");
                    js = new AndroidJavaObject("java.lang.String", "Request error");
                    toast2 = toast.CallStatic<AndroidJavaObject>("makeText", context, js,
                        toast.GetStatic<int>("LENGTH_LONG"));
                    toast2.Call("show");
                    break;
            }
        }

        private bool _passCorrect, _passMatch, _mailCorrect, _nameCorrect;

        public void CheckRegAbility()
        {
            signUpBtn.interactable = _passCorrect && _passMatch && _mailCorrect && _nameCorrect;
        }

        public void CheckPassword()
        {
            _passCorrect = regPassword.text.Length >= 8;
            regPassword.image.color = _passCorrect ? Color.white : red;
            if (regPassword.text.Length > 0) CheckMatchPassword();
        }

        public void CheckMatchPassword()
        {
            _passMatch = regPassword.text.Equals(repeatPassword.text);
            repeatPassword.image.color = _passMatch ? green : red;
        }

        public void CheckMail()
        {
            if (!regMail.text.Contains("@"))
            {
                _mailCorrect = false;
                regMail.image.color = red;
                return;
            }

            _mailCorrect = regMail.text.Split("@")[1].Length > 4 && regMail.text.Split("@")[0].Length > 2;
            regMail.image.color = _mailCorrect ? Color.white : red;
        }

        public void CheckName()
        {
            _nameCorrect = regName.text.Length >= 3;
            regName.image.color = _nameCorrect ? Color.white : red;
        }

        public void Clear()
        {
            regMail.image.color = Color.white;
            regName.image.color = Color.white;
            regPassword.image.color = Color.white;
            repeatPassword.image.color = Color.white;
            loginName.image.color = Color.white;
            loginPassword.image.color = Color.white;
            loginName.placeholder.GetComponent<Text>().text = "Nickname";
            loginPassword.placeholder.GetComponent<Text>().text = "Password";
            regMail.text = "";
            regName.text = "";
            regPassword.text = "";
            repeatPassword.text = "";
            loginName.text = "";
            loginPassword.text = "";
        }

        public void SetWhite()
        {
            regMail.image.color = Color.white;
            regName.image.color = Color.white;
            regPassword.image.color = Color.white;
            repeatPassword.image.color = Color.white;
            loginName.image.color = Color.white;
            loginPassword.image.color = Color.white;
        }
    }
}