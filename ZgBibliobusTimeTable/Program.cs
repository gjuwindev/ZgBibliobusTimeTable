using System;
using System.Net.Http;
using HtmlAgilityPack;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using HtmlLinkChecker;

namespace ZgBibliobusTimeTable;

internal class Program
{
    static void Main(string[] args)
    {
        string url = "https://www.kgz.hr/hr/knjiznice/bibliobusna-sluzba/raspored-bibliobusnih-stajalista/65960";

        string dataFolder = @"..\..\..\Data\";
        string htmlFilename = dataFolder + "index.html";

        // get the page content from the url and save it to the file
        //WebContent.GetWebContent(url, htmlFilename);  //Process.Start("NOTEPAD.EXE", htmlFilename);

        string pageContent = System.IO.File.ReadAllText(htmlFilename);

        List<PodaciZaDan> dani = WebContent.ParseWebContent(pageContent);

        foreach (var dan in dani)
        {
            foreach (string lokacija in dan.lokacije)
            {
                Console.WriteLine($"{dan.Dan} {lokacija}");
            }
        }

        Console.WriteLine("GOTOVO!");
        Console.ReadLine();
    }
}
