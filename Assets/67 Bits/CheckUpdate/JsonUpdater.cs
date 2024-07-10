using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[Serializable]
public class BuildInfo
{
    public string Version;
}

public class JsonUpdater
{
    private static string localFilePath = "G:/Meu Drive/";
    private static string sufix = "Dev/buildVersion.json";
    private static bool manual = false;
    public static void UpdateJson()
    {
        var filePath = String.Concat(localFilePath, sufix);
        BuildInfo buildInfo = new BuildInfo
        {
            Version = Application.version
        };
        string json = JsonConvert.SerializeObject(buildInfo, Formatting.Indented);

        if(!manual && !File.Exists(filePath))
            Debug.LogError($"Arquivo JSON não localizado. Crie um arquivo no local");
        else
            File.WriteAllText(filePath, json);
        Debug.Log($"Arquivo JSON atualizado: {localFilePath}");
    }
}
