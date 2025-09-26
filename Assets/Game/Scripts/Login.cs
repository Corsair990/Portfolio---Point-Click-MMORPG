using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.SceneManagement;

public class Login : MonoBehaviour
{
    public TMP_InputField username;
    public TMP_InputField password;

    public UserDatabase UserDatabase;

    private void Update()
    {
        if (UserDatabase != null)
        {
            
        }
    }

    public void LoginPressed()
    {
        if (!string.IsNullOrEmpty(username.text) || !string.IsNullOrEmpty(password.text))
        {
            if (UserDatabase.Login(username.text, password.text))
            {
                SceneManager.LoadScene("SampleScene");
            }

            else
            { 
                UserDatabase.CreateUser(username.text, password.text);

                if (UserDatabase.Login(username.text, password.text))
                {
                    SceneManager.LoadScene("SampleScene");
                }
            }
        }
    }
}
