using UnityEngine;
using Fusion;
using System.Collections.Generic;
using UnityEngine.InputSystem.Interactions;

[CreateAssetMenu(fileName = "CodeManager", menuName = "CodeManager")]
public class CodeManager : ScriptableObject
{
    public List<string> sessionList;

    public string GenerateCode()
    {
        string code = null;

        while(CompareCodeToSessionList(code))
        {
            Debug.Log("Generating new code.");
            code = RandomCode();
        }

        return code; // Returns a unique code
    }

    public bool CompareCodeToSessionList(string code)
    {
        if (code == null) return true;

        if (sessionList.Contains(code))
        {
            Debug.LogWarning("Code already exists in session list.");
            return true;
        }
        else
        {
            Debug.Log("Code is unique.");
            return false;
        }
    }

    string RandomCode() // Generates a random code using ASCII 
    {
        var code = "";

        for(int i = 0; i < 6; i++)
        {
            int num = Random.Range(48, 83);
            if(num >= 58)
            {
                num += 7;
            }
            code += (char)num;
        }

        return code;
    }
}