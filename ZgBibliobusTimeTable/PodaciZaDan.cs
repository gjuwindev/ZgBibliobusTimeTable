using HtmlAgilityPack;
namespace HtmlLinkChecker;

public class PodaciZaDan
{
    public HtmlNode danNode = null!;
    public List<string> lokacije = [];

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
