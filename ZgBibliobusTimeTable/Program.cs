using HtmlAgilityPack;

namespace ZgBibliobusTimeTable;

internal class Program
{
    private static List<DateTime> IzvadiDatume(HtmlNode danNode)
    {
        //if (danNode.InnerText.IndexOf("30.5.") > -1)
            IzvadiDatume2("", danNode);

        return new List<DateTime>();
    }

    private static void IzvadiDatume2(string parentNodeName, HtmlNode node)
    {
        //foreach (var node in danNode.ChildNodes)
        //{
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
                    Console.WriteLine($"===================>>>    {nodeInnerText} ");
                }
            }

        if (node.ChildNodes.Count > 0)
            foreach (var childNode in node.ChildNodes)
            {
                IzvadiDatume2(parentNodeName + "/" + node.Name, childNode);
            }
        //}
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
            List<DateTime> datumi = IzvadiDatume(dan.danNode);

            foreach (var datum in datumi)
            {
                Console.WriteLine($"Datum: {datum}");

                foreach (string vrijemeIlokacija in dan.VremenaILokacije)
                {
                    (string vrijeme, string lokacija) = ObradiVrijemeILokaciju(vrijemeIlokacija);

                    PodaciZaSesiju sesija = new PodaciZaSesiju(dan.Dan, $"{datum:yyyy-NN-dd}", vrijeme, lokacija);

                    sesije.Add(sesija);
                }
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
