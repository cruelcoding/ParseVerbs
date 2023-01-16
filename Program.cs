using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ParseVerbs
{
    public class Verb
    {
        public String Spanish { get; set; }
        public String English { get; set; }
        public String Russian { get; set; }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            List<char> Chars = new List<char>();
            List<Verb> Verbs = new List<Verb>();  

            var BaseUrl = "https://lingolex.com/verbs/az_verbs.php?letra=A";
            var BaseLetterUrl = "https://lingolex.com/verbs/az_verbs.php?letra=";
            var web = new HtmlWeb();
            var doc = web.Load(BaseUrl);

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
            Console.ReadKey();

            // для каждой буквы алфавита
            foreach (var letter in Chars)
            {
                Console.Clear();
                Console.WriteLine(letter);
                StringBuilder sb = new StringBuilder(BaseLetterUrl);
                sb.Append(letter);
                var LetterWeb = new HtmlWeb();
                var LetterDoc = LetterWeb.Load(sb.ToString());
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
                    //Console.WriteLine();
                    //Console.WriteLine(string.Join(" ", PagesList));
                    //Console.WriteLine();
                    foreach (var page in PagesList)
                    {
                        string OnePageURL = string.Format("https://lingolex.com/verbs/az_verbs.php?page={1}&letra={0}", letter, page);
                        Console.WriteLine(OnePageURL);
                        var OnePageWeb = new HtmlWeb();
                        var OnePageDoc = OnePageWeb.Load(OnePageURL);
                        var divs = OnePageDoc.DocumentNode.SelectNodes("//div");
                        var verbs = divs[5].SelectNodes("div").ToList();
                        for (int i = 3; i < verbs.Count-2; i++)
                        {
                            Console.WriteLine(String.Format("{0} -> {1}", verbs[i].SelectNodes("div")[0].InnerText, verbs[i].SelectNodes("div")[1].InnerText));
                            Verbs.Add(new Verb() { Spanish = verbs[i].SelectNodes("div")[0].InnerText.Trim(), English = verbs[i].SelectNodes("div")[1].InnerText.Trim(), Russian = String.Empty });
                        }
                        Console.WriteLine();
                    }
                    //Console.ReadKey();
                }
            }
            var jsonString = JsonSerializer.Serialize(Verbs, new JsonSerializerOptions() {  DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull, WriteIndented = true });
            File.WriteAllText("SpanishVerbs.json", jsonString);
        }
    }
}
