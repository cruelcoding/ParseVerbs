using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace ParseVerbs
{
    internal class Program
    {
        static string[] VerbsToProcess;

        static void LoadVerbs()
        {
            VerbsToProcess = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"verbs.txt"));
            Array.Sort(VerbsToProcess);
        }

        static void Main()
        {
            LoadVerbs();

            string[] Times = { "Indicativo", "Formas compuestas comunes", "Imperativo", "Subjuntivo", "Tiempos compuestos del subjuntivo", "Indicativo" };

            const string VerbConjugationURL = "https://wordreference.com/conj/esverbs.aspx?v={0}";

            List<Dictionary<string, object>> VerbsToStore = new List<Dictionary<string, Object>>();

            foreach (string VerbToConjugate in VerbsToProcess)
            {
                string ConjugationURL = string.Format(VerbConjugationURL, VerbToConjugate);
                Console.WriteLine(ConjugationURL);
                HtmlDocument VerbConjugationDoc = new HtmlWeb().Load(ConjugationURL);
                Dictionary<string, object> WordDictionary = new Dictionary<string, object>
                {
                    ["Spanish"] = VerbToConjugate,
                    ["English"] = new string[0],
                    ["Russian"] = new string[0],
                    ["gerundio"] = "",
                    ["participio"] = "",
                    ["themes"] = new string[0]
                };

                var conjTables = VerbConjugationDoc.DocumentNode.SelectNodes("//table");
                var b = conjTables.Where(x => x.Id == "conjtable");

                foreach (var table1 in b)
                {
                    var rows = table1.SelectNodes("tr");
                    if (rows != null)
                    {
                        for (int j = 0; j < rows.Count; j++)
                        {
                            try
                            {
                                WordDictionary["gerundio"] = rows[j].ChildNodes[3].ChildNodes[2].InnerHtml + rows[j].ChildNodes[3].ChildNodes[3].InnerHtml;
                                for (int k = 4; k < rows[j].ChildNodes[3].ChildNodes.Count - 2; k++)
                                    WordDictionary["participio"] = WordDictionary["participio"] + rows[j].ChildNodes[3].ChildNodes[k].InnerHtml;
                            }
                            catch
                            {

                            }
                        }
                    }
                }

                var h4 = VerbConjugationDoc.DocumentNode.SelectNodes("//h4");

                if (h4 != null)
                {
                    foreach (string timeName in Times)
                    {
                        Process(timeName);
                    }
                }

                void Process(string MainTimeName)
                {
                    HtmlNode h4Time = null;

                    if (MainTimeName == "Indicativo" && WordDictionary.ContainsKey(MainTimeName.ToLower()))
                    {
                            h4Time = h4.Where(x => x.InnerText.Trim() == MainTimeName).Last();
                            MainTimeName = "formas compuestas comunes";
                    }
                    else
                    {
                        h4Time = h4.Where(x => x.InnerText.Trim() == MainTimeName).First();
                        MainTimeName = MainTimeName.ToLower();
                        if (MainTimeName == "tiempos compuestos del subjuntivo")
                        {
                            MainTimeName = "subjuntivo";
                        } else
                        {
                            WordDictionary.Add(MainTimeName, new Dictionary<string, Dictionary<string, string>>());
                        }
                    }
                    
                    if (h4Time != null)
                    {
                        var parentDIVNode = h4Time.ParentNode;
                        var tables = parentDIVNode.SelectNodes("table");

                        if (tables != null)
                        {
                            foreach (var table in tables)
                            {
                                var rows = table.SelectNodes("tr");
                                if (rows != null)
                                {
                                    var LocalTimeName = rows[0].ChildNodes[0].InnerText;
                                    int indexof = LocalTimeName.IndexOf('&');
                                    if (indexof != -1)
                                    {
                                        LocalTimeName = LocalTimeName.Substring(0, indexof);
                                    }
                                    if (LocalTimeName == "imperfecto")
                                    {
                                        LocalTimeName = "Pret\u00E9rito imperfecto";
                                    }
                                    if (LocalTimeName == "pret\u00E9rito")
                                    {
                                        LocalTimeName = "Pret\u00E9rito indefenido";
                                    }
                                    if (LocalTimeName == "pluscuamperfecto")
                                    {
                                        LocalTimeName = "Pret\u00E9rito pluscuamperfecto";
                                    }
                                    LocalTimeName = LocalTimeName.ToLower();
                                    (WordDictionary[MainTimeName] as IDictionary<string, Dictionary<string, string>>).Add(LocalTimeName, new Dictionary<string, string>());
                                    for (int j = 1; j < rows.Count; j++)
                                    {
                                        try
                                        {
                                            var pronombre = rows[j].ChildNodes[0].InnerText;
                                            pronombre = pronombre.Replace("(", "");
                                            pronombre = pronombre.Replace(")", "");
                                            if (pronombre == "t\u00FA")
                                            {
                                                pronombre = "t\u00FA/vos";
                                            }
                                            if (pronombre == "\u00E9l, ella, usted")
                                            {
                                                pronombre = "\u00E9l/ella/Ud.";
                                            }
                                            if (pronombre == "nosotros, nosotras")
                                            {
                                                pronombre = "nosotros";
                                            }
                                            if (pronombre == "vosotros, vosotras")
                                            {
                                                pronombre = "vosotros";
                                            }
                                            if (pronombre == "ellos, ellas, ustedes")
                                            {
                                                pronombre = "ellos/ellas/Uds.";
                                            }
                                            if (pronombre == "usted")
                                            {
                                                pronombre = "Ud.";
                                            }
                                            if (pronombre == "ustedes")
                                            {
                                                pronombre = "Uds.";
                                            }
                                            if (pronombre == "vos")
                                            {
                                                continue;
                                            }
                                            var verbConjugated = rows[j].ChildNodes[1].InnerText;
                                            if (verbConjugated != "–")
                                            {
                                                verbConjugated = verbConjugated.Replace("*", "");
                                                verbConjugated = verbConjugated.Replace("!", "");
                                                verbConjugated = verbConjugated.Replace("¡", "");
                                                verbConjugated = verbConjugated.Replace(" o ", " | ");
                                                verbConjugated = verbConjugated.Replace(" u ", " | ");
                                                if (pronombre == "t\u00FA/vos")
                                                {
                                                    var vosValue = rows[7].ChildNodes[1].InnerText;
                                                    vosValue = vosValue.Replace("*", "");
                                                    vosValue = vosValue.Replace("!", "");
                                                    vosValue = vosValue.Replace("¡", "");
                                                    vosValue = vosValue.Replace(" o ", " | ");
                                                    vosValue = vosValue.Replace(" u ", " | ");
                                                    int commaPos = vosValue.IndexOf(",", StringComparison.Ordinal);
                                                    if (commaPos != -1)
                                                    {
                                                        vosValue = vosValue.Substring(0, commaPos);
                                                    }
                                                    if (vosValue != verbConjugated)
                                                    {
                                                        Console.WriteLine(pronombre + " -> " + verbConjugated);
                                                        (WordDictionary[MainTimeName] as IDictionary<string, Dictionary<string, string>>)[LocalTimeName].Add(pronombre, verbConjugated + " / " + vosValue);
                                                        continue;
                                                    }
                                                }
                                                if (verbConjugated.Contains(" | "))
                                                {
                                                    string[] split = verbConjugated.Split(' ');
                                                    if (split.Length == 4)
                                                    {
                                                        StringBuilder sb = new StringBuilder(split[0]);
                                                        sb.Append(" ");
                                                        sb.Append(split[3]);
                                                        sb.Append(" | ");
                                                        sb.Append(split[2]);
                                                        sb.Append(" ");
                                                        sb.Append(split[3]);
                                                        verbConjugated = sb.ToString();
                                                    }
                                                    if (split.Length == 5 && LocalTimeName == "pretérito pluscuamperfecto")
                                                    {
                                                        StringBuilder sb = new StringBuilder(split[0]);
                                                        sb.Append(" ");
                                                        sb.Append(split[1]);
                                                        sb.Append(" ");
                                                        sb.Append(split[4]);
                                                        sb.Append(" | ");
                                                        sb.Append(split[0]);
                                                        sb.Append(" ");
                                                        sb.Append(split[3]);
                                                        sb.Append(" ");
                                                        sb.Append(split[4]);
                                                        verbConjugated = sb.ToString();
                                                    }
                                                }
                                                Console.WriteLine(pronombre + " -> " + verbConjugated);
                                                (WordDictionary[MainTimeName] as IDictionary<string, Dictionary<string, string>>)[LocalTimeName].Add(pronombre, verbConjugated);
                                            }
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    if ((WordDictionary[MainTimeName] as IDictionary<string, Dictionary<string, string>>)[LocalTimeName].Keys.Count == 0)
                                    {
                                        (WordDictionary[MainTimeName] as IDictionary<string, Dictionary<string, string>>).Remove(LocalTimeName);
                                    }
                                }
                                Console.WriteLine();
                            }
                        }
                        if ((WordDictionary[MainTimeName] as IDictionary<string, Dictionary<string, string>>).Keys.Count == 0)
                        {
                            WordDictionary.Remove(MainTimeName);
                        }
                    }
                }
                VerbsToStore.Add(WordDictionary);
            }

            var jsonString = JsonSerializer.Serialize(VerbsToStore, new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, WriteIndented = true, Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.All) });
            File.WriteAllText("Result.json", jsonString);
        }
    }
}
