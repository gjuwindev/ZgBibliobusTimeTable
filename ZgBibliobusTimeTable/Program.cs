using HtmlAgilityPack;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ZgBibliobusTimeTable;

internal class Program
{
    private static (List<DateTime> sviDatumi, List<DateTime> radniDatumi, List<DateTime> neradniDatumi) IzvadiDatume(HtmlNode danNode)
    {
        (string prviDan, string sviDatumi, string radniDatumi, string neradniDatumi) = IzvadiStringDatume(danNode);

        List<DateTime> sviDatumiList = StringDatumiToDateList(sviDatumi);
        List<DateTime> radniDatumiList = StringDatumiToDateList(radniDatumi);
        List<DateTime> neradniDatumiList = StringDatumiToDateList(neradniDatumi);

        if (sviDatumiList.Count == 0)
            throw new Exception($"Nema datuma za dan {prviDan}.");
        if (radniDatumiList.Count == 0)
            throw new Exception($"Nema radnih datuma za dan {prviDan}.");

        ProvjeriPrviDan(prviDan, sviDatumiList[0]);

        return (sviDatumiList, radniDatumiList, neradniDatumiList);
    }

    private static void ProvjeriPrviDan(string prviDan, DateTime dateTime)
    {
        string[] dani = ["nedjelja", "ponedjeljak", "utorak", "srijeda", "četvrtak", "petak", "subota"];

        int danIndex = Array.IndexOf(dani, prviDan.ToLower());

        if (danIndex != (int)dateTime.DayOfWeek)
            throw new Exception($"Prvi dan {prviDan} ne odgovara datumu {dateTime:yyyy-MM-dd}.");
    }

    private static List<DateTime> StringDatumiToDateList(string stringDatumi)
    {
        List<DateTime> datumi = new List<DateTime>();

        string[] parts = stringDatumi.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        int tekucaGodina = DateTime.Now.Year;
        foreach (string part in parts)
        {
            string[] subparts = part.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            string datumString;

            if (subparts.Length == 2)
                datumString = $"{tekucaGodina}-{subparts[1]}-{subparts[0]}";
            else if (subparts.Length == 3 && int.TryParse(subparts[2], out tekucaGodina))
                datumString = $"{tekucaGodina}-{subparts[1]}-{subparts[0]}";
            else
                throw new Exception($"Neispravan format datuma: {part}");

            if (!DateTime.TryParse(datumString, out DateTime datum) ||
                                  (datum.Year < 2000 || datum.Year > 2100))
                throw new Exception($"Neispravan datum: {datumString}");

            datumi.Add(datum);
        }

        return datumi;
    }

    private static (string prviDan, string sviDatumi, string radniDatumi, string neradniDatumi) IzvadiStringDatume(HtmlNode danNode)
    {
        string sviDatumi = IzvadiSveDatume(danNode);
        string praznicniDatumi = IzvadiPraznicneDatume(danNode);
        string radniDatumi = sviDatumi;

        string[] sviParts = sviDatumi.Split(['\t', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string[] praznicniParts = praznicniDatumi.Split(['\t', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        string sviPrviDan = "";
        string praznicniPrviDan = "";

        if (sviParts.Length == 1 && praznicniParts.Length == 1)
        {; }
        else if (sviParts.Length == 2 && praznicniParts.Length == 2)
        {
            sviPrviDan = sviParts[0];
            sviDatumi = sviParts[1];
            radniDatumi = sviDatumi;
            praznicniPrviDan = praznicniParts[0];
            praznicniDatumi = praznicniParts[1];
        }
        else if (sviParts.Length == 2 && praznicniParts.Length <= 1)
        {
            sviPrviDan = sviParts[0];
            sviDatumi = sviParts[1];
            radniDatumi = sviDatumi;
        }
        else if (sviParts.Length == 1 && praznicniParts.Length == 2)
        {
            praznicniPrviDan = praznicniParts[0];
            praznicniDatumi = praznicniParts[1];
        }
        else
        {
            throw new Exception($"sviParts.Length == {sviParts.Length}, praznicniParts.Length == {praznicniParts.Length}");
        }

        if (string.IsNullOrEmpty(praznicniPrviDan))
        {
            if (string.IsNullOrEmpty(sviPrviDan))
                throw new Exception($"Prvi dani svi prazni.");
        }
        else if (string.IsNullOrEmpty(sviPrviDan))
            throw new Exception($"Prvi dan je prazan, a praznicni nije.");
        else if (praznicniPrviDan != sviPrviDan)
            throw new Exception($"{praznicniPrviDan} != {sviPrviDan})");

        if (!string.IsNullOrEmpty(praznicniDatumi))
        {
            radniDatumi = radniDatumi.Replace(praznicniDatumi, "");
        }

        return (sviPrviDan, sviDatumi, radniDatumi, praznicniDatumi);
    }

    private static string IzvadiSveDatume(HtmlNode danNode)
    {
        return danNode.InnerText.Replace("&nbsp;", "").Replace(" ", "").Trim();
    }

    private static string IzvadiPraznicneDatume(HtmlNode danNode)
    {
        StringBuilder sb = new StringBuilder();

        IzvadiPraznicneDatume2(sb, "", danNode);

        return sb.ToString().Replace("&nbsp;", "").Replace(" ", "").Trim();
    }

    private static void IzvadiPraznicneDatume2(StringBuilder sb, string parentNodeName, HtmlNode node)
    {
        string nodeName = node.Name;
        string nodeInnerText = node.InnerText.Trim();
        string nodeInnerHTML = node.InnerHtml.Trim();

        IEnumerable<HtmlAttribute> nodeAttributes = node.GetAttributes();

        if (nodeAttributes is not null && nodeAttributes.Count() > 0)
            foreach (var attribute in nodeAttributes)
            {
                if (attribute.Value.IndexOf("color:") > -1)
                {
                    //Console.WriteLine($"===================>>>    {parentNodeName}+{nodeName}   {nodeInnerText}  ({attribute.Name}) = {attribute.Value}");
                    //Console.WriteLine($"===================>>>    {nodeInnerText} ");
                    sb.Append(nodeInnerText);
                    sb.Append(',');
                }
            }

        if (node.ChildNodes.Count > 0)
            foreach (var childNode in node.ChildNodes)
            {
                IzvadiPraznicneDatume2(sb, parentNodeName + "/" + node.Name, childNode);
            }
    }

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
            (List<DateTime> sviDatumi, List<DateTime> radniDatumi, List<DateTime> neradniDatumi) = IzvadiDatume(dan.danNode);

            foreach (var datum in radniDatumi)
            {
                foreach (string vrijemeIlokacija in dan.VremenaILokacije)
                {
                    (string vrijeme, string lokacija) = ObradiVrijemeILokaciju(vrijemeIlokacija);

                    PodaciZaSesiju sesija = new PodaciZaSesiju(dan.Dan, $"{datum:yyyy-MM-dd}", vrijeme, lokacija);

                    sesije.Add(sesija);
                }
            }

            foreach (var datum in neradniDatumi)
            {
                PodaciZaSesiju sesija = new PodaciZaSesiju(dan.Dan, $"{datum:yyyy-MM-dd}", "", "=== neradni dan ===");

                sesije.Add(sesija);
            }
        }

        sesije.Sort((x, y) =>
        {
            int result = x.Datum.CompareTo(y.Datum);
            if (result == 0)
                result = x.Vrijeme.CompareTo(y.Vrijeme);
            return result;
        });

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
