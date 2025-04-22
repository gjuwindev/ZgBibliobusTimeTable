using System;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
namespace HtmlLinkChecker;

internal class Dan
{
    public HtmlNode danNode = null!;
    public List<string> lokacije = [];

    public override string ToString()
    {
        string innerText = danNode.InnerText;

        int blankPos = innerText.IndexOf(' ');

        if (blankPos == -1)
            return innerText;
        else
            return innerText[0..blankPos];
    }
}

internal class Program
{
    static Encoding win1250 = null!;
    static UTF8Encoding utf8Encoding = null;

    static async Task Main(string[] args)
    {
        var url = "https://www.kgz.hr/hr/knjiznice/bibliobusna-sluzba/raspored-bibliobusnih-stajalista/65960";

        string dataFolder = @"..\..\..\Data\";
        string htmlFilename = dataFolder + "index.html";
        string htmlLinksFilename = dataFolder + "index_links.txt";

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        win1250 = Encoding.GetEncoding(1250);
        utf8Encoding = new UTF8Encoding(false, true); // BOM = false, EncoderShouldEmitUTF8Identifier = true

        using (HttpClient client = new HttpClient())
        {

            try
            {
                Task t = 2 switch
                {
                    // get the page content from the url and save it to the file
                    1 => Task.Run(() => GetWebContent(client, url, htmlFilename)),
                    2 => Task.Run(() => ParseWebContent(client, htmlFilename, htmlLinksFilename, out List<Dan> dani)),
                    _ => throw new NotImplementedException()
                };

                await t;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        Console.WriteLine("GOTOVO!");
        Console.ReadLine();
    }

    private static void ParseWebContent(HttpClient client, string htmlFilename, string outputFilename, out List<Dan> dani)
    {
        StringBuilder sb = new();

        dani = new();
        Dan? tekuciDan = null;

        string pageContent = System.IO.File.ReadAllText(htmlFilename);

        // Parse the HTML content using HtmlAgilityPack
        HtmlDocument doc = new HtmlDocument();
        doc.LoadHtml(pageContent);

        // Extract specific elements (e.g., all <a> tags)
        var tables = doc.DocumentNode.SelectNodes("//table");
        if (tables is null)
        {
            Console.WriteLine("No tables found on the page.");
        }
        else
        {
            foreach (var table in tables)
            {
                var rows = table.SelectNodes(".//tr");

                if (rows is null)
                {
                    Console.WriteLine("No rows found in the table.");
                }
                else
                {
                    foreach (var row in rows)
                    {
                        var tableDatas = row.SelectNodes(".//td");

                        if (tableDatas is null)
                        {
                            Console.WriteLine("No tableDatas found in the row.");
                        }
                        else
                        {
                            int tdCount = tableDatas.Count();

                            if (tdCount == 3)
                            {
                                tekuciDan = new Dan();
                                tekuciDan.danNode = tableDatas[0];
                                tekuciDan.lokacije.Add(GetLocation(tableDatas[1], tableDatas[2]));
                                dani.Add(tekuciDan);
                            }
                            else if (tdCount == 2)
                            {
                                if (tekuciDan is null)
                                {
                                    Console.WriteLine("Tekuci dan je prazan.");
                                }
                                else
                                {
                                    tekuciDan.lokacije.Add(GetLocation(tableDatas[0], tableDatas[1]));
                                }
                            }
                            else
                            {
                                Console.WriteLine("TableDatas count is not 2 or 3.");
                            }
                        }
                    }
                }

                tekuciDan = null;
            }
        }
    }

    private static string GetLocation(HtmlNode node1, HtmlNode node2)
    {
        string text1 = node1.InnerText.Trim().Replace(Environment.NewLine, " ").Replace("\n", " ").Trim();
        string text2 = node2.InnerText.Trim().Replace(Environment.NewLine, " ").Replace("\n", " ").Replace(" ", "").Trim();
        text2 = text2.Replace(" ", "").Replace("&nbsp;", "");
        if (text2.IndexOf(':') == 1) text2 = "0" + text2;
        return text2 + "#" + text1;
    }

    private static async Task GetWebContent(HttpClient client, string url, string htmlFilename)
    {
        // Fetch the HTML content of the web page
        var response = await client.GetByteArrayAsync(url);

        Console.WriteLine("Page content fetched successfully.");

        //string s = win1250.GetString(response);
        string s = utf8Encoding.GetString(response);

        System.IO.File.WriteAllText(htmlFilename, s);

        Process.Start("NOTEPAD.EXE", htmlFilename);
    }
}
