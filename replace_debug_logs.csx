using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main(string[] args)
    {
        string scriptsPath = @"D:\Unity Project\HO\Assets\scripts";
        
        foreach (string file in Directory.GetFiles(scriptsPath, "*.cs", SearchOption.AllDirectories))
        {
            string content = File.ReadAllText(file);
            
            // Заменяем Debug.Log, Debug.LogWarning, Debug.LogError на закомментированные версии
            content = Regex.Replace(content, @"(Debug\.Log(?:Warning|Error)?\s*\([^)]*\);)", @"// $1");
            
            File.WriteAllText(file, content);
            Console.WriteLine($"Processed: {file}");
        }
    }
}