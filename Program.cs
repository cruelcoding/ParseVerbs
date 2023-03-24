//#define CREATE_FILE

using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace ParseVerbs
{
    internal class Program
    {
        static void Main(string[] args)
        {
#if CREATE_FILE
            List<ExpandoObject> Verbs = new List<ExpandoObject>();
            List<char> Chars = new List<char>();
            const string LetterBaseURL = "https://lingolex.com/verbs/az_verbs.php?letra=";
            const string LetterPageURL = "https://lingolex.com/verbs/az_verbs.php?page={0}&letra={1}";
            var doc = new HtmlWeb().Load(String.Concat(LetterBaseURL, "A"));

            // получаем список ненумерованных списков (ul)
            var a = doc.DocumentNode.SelectNodes("//ul");

            // во втором списке (индекс 1) - буквы алфавита
            var characters = a[1].SelectNodes("li");

            // кладём их в массив
            foreach (var c in characters)
            {
                Chars.Add(c.InnerText[0]);
            }
            Console.WriteLine(Chars.ToArray());
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            // для каждой буквы алфавита
            foreach (var letter in Chars)
            {
                Console.Clear();
                Console.WriteLine(letter);
                var LetterDoc = new HtmlWeb().Load(String.Concat(LetterBaseURL, letter));
                var lists = LetterDoc.DocumentNode.SelectNodes("//ul");
                var pages = lists[2].SelectNodes("li").ToList();
                List<int> PagesList = new List<int>();
                for (int i = 0; i < pages.Count; i++)
                {
                    var PageNum = pages[i].InnerText;
                    if (int.TryParse(PageNum, out var Page) == true)
                    {
                        PagesList.Add(Page);
                    }
                }
                if (PagesList.Count > 0)
                {
                    foreach (var page in PagesList)
                    {
                        string OnePageURL = string.Format(LetterPageURL, page, letter);
                        Console.WriteLine(OnePageURL);
                        var OnePageDoc = new HtmlWeb().Load(OnePageURL);
                        var divs = OnePageDoc.DocumentNode.SelectNodes("//div");
                        var verbs = divs[5].SelectNodes("div").ToList();
                        for (int i = 3; i < verbs.Count - 2; i++)
                        {
                            var spainVerb = verbs[i].SelectNodes("div")[0].InnerText.Trim();
                            var englishVerb = verbs[i].SelectNodes("div")[1].InnerText.Trim();
                            Console.WriteLine(String.Format("{0} -> {1}", spainVerb, englishVerb));

                            dynamic newVerb = new ExpandoObject();
                            newVerb.Spanish = spainVerb.Replace("\u0026ntilde;", "ñ");
                            var EnglishWords = englishVerb.Split(new[] { ',', '/' });
                            for (int j = 0; j < EnglishWords.Length; j++)
                            {
                                EnglishWords[j] = EnglishWords[j].Trim();
                                if (EnglishWords[j].StartsWith("to "))
                                {
                                    EnglishWords[j] = EnglishWords[j].Remove(0, 3);
                                }
                            }

                            newVerb.English = EnglishWords;
                            newVerb.Russian = new string[0];

                            Verbs.Add(newVerb);
                        }
                        Console.WriteLine();
                    }
                }
            }

            string[] reflexive = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"reflexive.txt"));

            for (int i = 0; i < reflexive.Count(); i++)
            {
                if (i % 2 != 0)
                {
                    dynamic newVerb = new ExpandoObject();
                    newVerb.Spanish = reflexive[i - 1].Replace("\u0026ntilde;", "ñ");
                    var EnglishWords = reflexive[i].Split(new[] { ',', '/' });
                    for (int j = 0; j < EnglishWords.Length; j++)
                    {
                        EnglishWords[j] = EnglishWords[j].Trim();
                        if (EnglishWords[j].StartsWith("to "))
                        {
                            EnglishWords[j] = EnglishWords[j].Remove(0, 3);
                        }
                    }
                    newVerb.English = EnglishWords;
                    newVerb.Russian = new string[0];
                    Verbs.Add(newVerb);
                }
            }


            Verbs.Sort((x, y) => (String.Compare((((IDictionary<string, object>)x)["Spanish"]).ToString(), (((IDictionary<string, object>)y)["Spanish"]).ToString())));

            var verbsJsonString = JsonSerializer.Serialize(Verbs, new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, WriteIndented = true });
            File.WriteAllText("VerbsInfnitivo.json", verbsJsonString);
            return;
#else
            const string VerbConjugationURL = "https://wordreference.com/conj/esverbs.aspx?v={0}"; //https://deleahora.com/conjugacion/
            string[] Times = { "Indicativo", "Formas compuestas comunes", "Imperativo", "Subjuntivo" };
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"OneWord.json");

            // const string VerbConjugationURL = "https://deleahora.com/conjugacion/{0}";
            // string[] Times = { "Indicativo", "Formas compuestas comunes", "Imperativo", "Subjuntivo", "Subjuntivo formas compuestas" };
            // string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"SpanishVerbs.json");


            List<ExpandoObject> Verbs = JsonSerializer.Deserialize<List<ExpandoObject>>(File.ReadAllText(path));


            foreach (dynamic verb in Verbs)
            {
                string ConjugationURL = string.Format(VerbConjugationURL, verb.Spanish); //soler tener atraer
                Console.WriteLine(ConjugationURL);
                HtmlDocument VerbConjugationDoc = new HtmlWeb().Load(ConjugationURL);

                //var TablesList = VerbConjugationDoc.DocumentNode.SelectNodes("//table");

                //var h2List = VerbConjugationDoc.DocumentNode.SelectNodes("//h2");

                //if (h2List.Count < 2)
                //{
                //    String a = Convert.ToString(verb.Spanish);
                //    Debug.WriteLine(a);
                //    continue;
                //}

                //var IndicativoH2 = h2List[0];
                //var SubjuntivoH2 = h2List[1];
                //var ImperativoH2 = h2List[2];

                //var main = TablesList[0];

                //var rows = main.SelectNodes("tbody/tr");

                //var columns = rows[1].SelectNodes("td");

                //(verb as IDictionary<string, object>).Add("gerundio", columns[1].InnerText);
                //(verb as IDictionary<string, object>).Add("participio", columns[2].InnerText);

                //void ProcessTime(String TimeName)
                //{
                //    HtmlNode DivsParent = null;
                //    switch (TimeName)
                //    {
                //        case "Indicativo":
                //            {
                //                DivsParent = IndicativoH2.NextSibling.NextSibling;
                //                break;
                //            }
                //        case "Formas compuestas comunes":
                //            {
                //                DivsParent = IndicativoH2.NextSibling.NextSibling.NextSibling.NextSibling;
                //                break;
                //            }
                //        case "Imperativo":
                //            {
                //                DivsParent = ImperativoH2.NextSibling.NextSibling;
                //                break;
                //            }
                //        case "Subjuntivo":
                //            {
                //                DivsParent = SubjuntivoH2.NextSibling.NextSibling;
                //                break;
                //            }
                //        case "Subjuntivo formas compuestas":
                //            {
                //                DivsParent = SubjuntivoH2.NextSibling.NextSibling.NextSibling.NextSibling;
                //                TimeName = "Subjuntivo";
                //                break;
                //            }
                //    }

                //    if ((verb as IDictionary<string, object>).ContainsKey(TimeName) == false)
                //    {
                //        (verb as IDictionary<string, object>).Add(TimeName, new Dictionary<String, Dictionary<String, String>>());
                //    }

                //    var divs = DivsParent.SelectNodes("div");

                //    for (int i = 0; i < divs.Count; i++)
                //    {
                //        var div = divs[i];
                //        var LocalTimeName = div.ChildNodes[1].ChildNodes[1].ChildNodes[1].InnerText;
                //        var table = div.ChildNodes[1].ChildNodes[3];
                //        var td = table.SelectNodes("table");
                //        var rows1 = td[0].SelectNodes("tbody/tr");
                //        Console.WriteLine(LocalTimeName);
                //        ((verb as IDictionary<string, object>)[TimeName] as IDictionary<string, Dictionary<String, String>>).Add(LocalTimeName, new Dictionary<string, string>());
                //        for (int j = 0; j < rows1.Count; j++)
                //        {
                //            var columns1 = rows1[j].SelectNodes("td");
                //            if (columns1[1].InnerHtml.IndexOf('-') == -1)
                //            {
                //                ((verb as IDictionary<string, object>)[TimeName] as IDictionary<string, Dictionary<String, String>>)[LocalTimeName].Add(columns1[0].InnerText, columns1[1].InnerHtml);
                //                Console.WriteLine(columns1[0].InnerText + "->" + columns1[1].InnerHtml);
                //            }
                //        }
                //        if (((verb as IDictionary<string, object>)[TimeName] as IDictionary<string, Dictionary<String, String>>)[LocalTimeName].Keys.Count == 0)
                //        {
                //            ((verb as IDictionary<string, object>)[TimeName] as IDictionary<string, Dictionary<String, String>>).Remove(LocalTimeName);
                //        }
                //        Console.WriteLine("");
                //    }
                //    if (((verb as IDictionary<string, object>)[TimeName] as IDictionary<string, Dictionary<String, String>>).Keys.Count == 0)
                //    {
                //        (verb as IDictionary<string, object>).Remove(TimeName);
                //    }
                //}

                //foreach (string timeName in Times)
                //{
                //    ProcessTime(timeName);
                //}


                var conjTables = VerbConjugationDoc.DocumentNode.SelectNodes("//table");

                var b = conjTables.Where(x => x.Id == "conjtable");

                (verb as IDictionary<string, object>)["Spanish"] = (verb as IDictionary<string, object>)["Spanish"].ToString();

                foreach (var table1 in b)
                {
                    var rows = table1.SelectNodes("tr");
                    if (rows != null)
                    {
                        for (int j = 0; j < rows.Count; j++)
                        {
                            try
                            {
                                (verb as IDictionary<string, object>).Add("gerundio", rows[j].ChildNodes[3].ChildNodes[2].InnerHtml + rows[j].ChildNodes[3].ChildNodes[3].InnerHtml);
                                (verb as IDictionary<string, object>).Add("participio", rows[j].ChildNodes[3].ChildNodes[4].InnerHtml + rows[j].ChildNodes[3].ChildNodes[5].InnerHtml);
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
                    var h4Time = h4.Where(x => x.InnerText.Trim() == MainTimeName).First();
                    if (h4Time != null)
                    {
                        var parentDIVNode = h4Time.ParentNode;

                        var tables = parentDIVNode.SelectNodes("table");
                        (verb as IDictionary<string, object>).Add(MainTimeName, new Dictionary<String, Dictionary<String, String>>());

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
                                    if (LocalTimeName == "presente")
                                    {
                                        LocalTimeName = "Presente";
                                    }
                                    if (LocalTimeName == "imperfecto")
                                    {
                                        LocalTimeName = "Pret\u00E9rito imperfecto";
                                    }
                                    if (LocalTimeName == "pret\u00E9rito")
                                    {
                                        LocalTimeName = "Pret\u00E9rito perfecto simple";
                                    }
                                    if (LocalTimeName == "futuro")
                                    {
                                        LocalTimeName = "Futuro";
                                    }
                                    if (LocalTimeName == "condicional")
                                    {
                                        LocalTimeName = "Condicional";
                                    }
                                    if (LocalTimeName == "afirmativo")
                                    {
                                        LocalTimeName = "Afirmativo";
                                    }
                                    if (LocalTimeName == "negativo")
                                    {
                                        LocalTimeName = "Negativo";
                                    }
                                    if (LocalTimeName == "condicional perfecto")
                                    {
                                        LocalTimeName = "Condicional perfecto";
                                    }
                                    if (LocalTimeName == "futuro perfecto")
                                    {
                                        LocalTimeName = "Futuro perfecto";
                                    }
                                    if (LocalTimeName == "pluscuamperfecto")
                                    {
                                        LocalTimeName = "Pret\u00E9rito pluscuamperfecto";
                                    }
                                    if (LocalTimeName == "pret\u00E9rito perfecto")
                                    {
                                        LocalTimeName = "Pret\u00E9rito perfecto compuesto";
                                    }

                                    ((verb as IDictionary<string, object>)[MainTimeName] as IDictionary<string, Dictionary<String, String>>).Add(LocalTimeName, new Dictionary<string, string>());
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
                                                if (pronombre == "t\u00FA/vos")
                                                {
                                                    var vosValue = rows[7].ChildNodes[1].InnerText;
                                                    vosValue = vosValue.Replace("*", "");
                                                    if (vosValue != verbConjugated)
                                                    {
                                                        Console.WriteLine(pronombre + " -> " + verbConjugated);
                                                        ((verb as IDictionary<string, object>)[MainTimeName] as IDictionary<string, Dictionary<String, String>>)[LocalTimeName].Add(pronombre, verbConjugated+" / "+vosValue);
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine(pronombre + " -> " + verbConjugated);
                                                        ((verb as IDictionary<string, object>)[MainTimeName] as IDictionary<string, Dictionary<String, String>>)[LocalTimeName].Add(pronombre, verbConjugated);
                                                    }
                                                }
                                                else
                                                {
                                                    Console.WriteLine(pronombre + " -> " + verbConjugated);
                                                    ((verb as IDictionary<string, object>)[MainTimeName] as IDictionary<string, Dictionary<String, String>>)[LocalTimeName].Add(pronombre, verbConjugated);
                                                }
                                            }
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    if (((verb as IDictionary<string, object>)[MainTimeName] as IDictionary<string, Dictionary<String, String>>)[LocalTimeName].Keys.Count == 0)
                                    {
                                        ((verb as IDictionary<string, object>)[MainTimeName] as IDictionary<string, Dictionary<String, String>>).Remove(LocalTimeName);
                                    }
                                }
                                Console.WriteLine();
                            }
                        }
                        if (((verb as IDictionary<string, object>)[MainTimeName] as IDictionary<string, Dictionary<String, String>>).Keys.Count == 0)
                        {
                            (verb as IDictionary<string, object>).Remove(MainTimeName);
                        }
                    }
                }
            }

            var jsonString = JsonSerializer.Serialize(Verbs, new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, WriteIndented = true });
            File.WriteAllText("SpanishVerbsOne.json", jsonString);
#endif
        }
    }
}
