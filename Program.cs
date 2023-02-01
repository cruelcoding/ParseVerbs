using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace ParseVerbs
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<char> Chars = new List<char>();
            List<ExpandoObject> Verbs = new List<ExpandoObject>();  

            const string LetterBaseURL = "https://lingolex.com/verbs/az_verbs.php?letra=";
            const string LetterPageURL = "https://lingolex.com/verbs/az_verbs.php?page={0}&letra={1}";
            const string VerbConjugationURL = "https://wordreference.com/conj/esverbs.aspx?v={0}";
            var doc = new HtmlWeb().Load(String.Concat(LetterBaseURL, "A"));

            // получаем список ненумерованных списков (ul)
            var a = doc.DocumentNode.SelectNodes("//ul");

            // во втором списке (индекс 1) - буквы алфавита
            var characters = a[1].SelectNodes("li");

            // кладём их в массив
            foreach ( var c in characters )
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
                            newVerb.Spanish = spainVerb.Replace("\u0026ntilde;","ñ");
                            newVerb.English = englishVerb;
                            newVerb.Russian = string.Empty;

                            Verbs.Add(newVerb);
                        }
                        Console.WriteLine();
                    }
                }
                //break;
            }

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"reflexive.txt");
            string[] reflexive = File.ReadAllLines(path);

            for (int i = 0; i < reflexive.Count(); i++)
            {
                if (i % 2 != 0)
                {
                    dynamic newVerb = new ExpandoObject();
                    newVerb.Spanish = reflexive[i - 1].Replace("\u0026ntilde;","ñ");
                    newVerb.English = reflexive[i];
                    newVerb.Russian = string.Empty;
                    Verbs.Add(newVerb);
                }
            }

            foreach (dynamic verb in Verbs)
            {
                string ConjugationURL = string.Format(VerbConjugationURL, verb.Spanish);
                Console.WriteLine(ConjugationURL);
                HtmlDocument VerbConjugationDoc = new HtmlWeb().Load(ConjugationURL);
                var h4 = VerbConjugationDoc.DocumentNode.SelectNodes("//h4");

                if (h4 != null)
                {
                    var h4Indicativo = h4.Where(x => x.InnerText.Trim() == "Indicativo").First();

                    if (h4Indicativo != null)
                    {
                        var parentDIVNode = h4Indicativo.ParentNode;

                        var tables = parentDIVNode.SelectNodes("table");

                        if (tables != null)
                        {
                            foreach (var table in tables)
                            {
                                var rows = table.SelectNodes("tr");
                                if (rows != null)
                                {
                                    var TimeName = rows[0].ChildNodes[0].InnerText;
                                    int indexof = TimeName.IndexOf('&');
                                    if (indexof != -1)
                                    {
                                        TimeName = TimeName.Substring(0, indexof);
                                    }
                                    (verb as IDictionary<string, object>).Add(TimeName, new Dictionary<string, string>());
                                    for (int j = 1; j < rows.Count; j++)
                                    {
                                        try
                                        {
                                            var pronombre = rows[j].ChildNodes[0].InnerText;
                                            var verbConjugated = rows[j].ChildNodes[1].InnerText;
                                            Console.WriteLine(pronombre + " -> " + verbConjugated);
                                            ((verb as IDictionary<string, object>)[TimeName] as IDictionary<string, String>).Add(pronombre, verbConjugated);
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                                Console.WriteLine();
                            }
                        }
                    }
                }
            }
            var jsonString = JsonSerializer.Serialize(Verbs, new JsonSerializerOptions() { DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, WriteIndented = true });
            File.WriteAllText("SpanishVerbs.json", jsonString);
        }
    }
}
