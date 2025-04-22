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

        string dataFolder = @"..\..\..\Data\";
        string htmlFilename = dataFolder + "index.html";

        // get the page content from the url and save it to the file
        //string url = "https://www.kgz.hr/hr/knjiznice/bibliobusna-sluzba/raspored-bibliobusnih-stajalista/65960";
        //WebContent.GetWebContent(url, htmlFilename);  //Process.Start("NOTEPAD.EXE", htmlFilename);

        string pageContent = System.IO.File.ReadAllText(htmlFilename);

        List<PodaciZaDan> dani = WebContent.ParseWebContent(pageContent);

        List<PodaciZaSesiju> sesije = ConvertToSesije(dani);

        foreach (var sesija in sesije)
        {
            Console.WriteLine(sesija);
        }

        Console.WriteLine("GOTOVO!");
        Console.ReadLine();
    }

    private static List<PodaciZaSesiju> ConvertToSesije(List<PodaciZaDan> dani)
    {
        List<PodaciZaSesiju> sesije = [];

        foreach (var dan in dani)
        {
            foreach (string vrijemeIlokacija in dan.VremenaILokacije)
            {
                string datum = "17.11.1957.";
                (string vrijeme, string lokacija) = ObradiVrijemeILokaciju(vrijemeIlokacija);

                PodaciZaSesiju sesija = new PodaciZaSesiju(dan.Dan, datum, vrijeme, lokacija);

                sesije.Add(sesija);
            }
        }

        return sesije;
    }

    public static (string vrijeme, string lokacija) ObradiVrijemeILokaciju(string vrijemeIlokacija)
    {
        string[] parts = vrijemeIlokacija.Split('#');
        if (parts.Length != 2)
            throw new Exception("Vrijeme i lokacija nisu u ispravnom formatu.");

        string text1 = parts[1].Trim().Replace(Environment.NewLine, " ").Replace("\n", " ").Trim();
        text1 = text1.Replace("\t", " ").Replace("&nbsp;", " ");


        while (text1.IndexOf("  ") > -1)
            text1 = text1.Replace("  ", " ");

        string text2 = parts[0].Trim().Replace(Environment.NewLine, " ").Replace("\n", " ").Replace(" ", "").Trim();
        text2 = text2.Replace(" ", "").Replace("&nbsp;", "");
        if (text2.IndexOf(':') == 1) text2 = "0" + text2;

        int secondColonPos = text2.IndexOf(':', 3);
        if (secondColonPos == 7)
        {
            text2 = text2.Insert(6, "0");
        }

        return (text2, text1);
    }
}
