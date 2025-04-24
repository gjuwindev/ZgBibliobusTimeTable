using HtmlAgilityPack;

namespace ZgBibliobusTimeTable;

public class PodaciZaSesiju
{
    public string Dan;
    public string Datum;
    public string Vrijeme;
    public string Lokacija;

    public PodaciZaSesiju(string dan, string datum, string vrijeme, string lokacija)
    {
        Dan = dan;
        Datum = datum;
        Vrijeme = vrijeme;
        Lokacija = lokacija;
    }

    public override string ToString()
    {
        return $"{Dan,10} {Datum,12} {Vrijeme,12}  {Lokacija}";
    }
}

public class PodaciZaDan
{
    public HtmlNode danNode = null!;
    public List<string> VremenaILokacije = [];

    public string Dan
    {
        get
        {
            if (danNode == null)
                return "<empty>";

            string innerText = danNode.InnerText.Trim();
            int blankPos = innerText.IndexOfAny([' ', '\n' ]);
            if (blankPos == -1)
                return innerText;
            else
                return innerText[0..blankPos];
        }
    }

    public override string ToString()
    {
        return Dan;
    }
}
