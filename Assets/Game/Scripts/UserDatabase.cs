using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUserDatabase", menuName = "RPG/Data/User Database")]
public class UserDatabase : ScriptableObject
{
    public Dictionary<string, string> userDatabase = new Dictionary<string, string>();

    private void Awake()
    {
        if (userDatabase.ContainsKey("Redbeard"))
        {
            return;
        }
        else
        {
            userDatabase.Add("Redbeard", "1q2w3e4r");
        }
    }

    public void CreateUser(string _user, string _password)
    {
        if (string.IsNullOrEmpty(_user) || string.IsNullOrEmpty(_password)) return;

        userDatabase.Add(_user, _password);
    }

    public bool Login(string _username, string _password)
    {
        if (string.IsNullOrEmpty(_username) || string.IsNullOrEmpty(_password))
        {
            return false;
        }

        if (userDatabase.ContainsKey(_username)) 
        {
            if (_password == userDatabase[_username])
            {
                Debug.Log($"Username: {_username} with password: {_password} was found.");

                return true;
            }
        }

        return false;
    }
}