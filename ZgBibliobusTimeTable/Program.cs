namespace ZgBibliobusTimeTable;

internal class Program : Tools
{
    static void Main(string[] args)
    {
        //>>> uncomment >>>  the GetWebContent line to fetch the content from the URL
        //>>> uncomment >>>  and save it to the specified file

        string dataFolder = @"..\..\..\Data\";
        string htmlFilename = dataFolder + "index.html";

        // get the page content from the url and save it to the file
        //>>> uncomment >>>  string url = "https://www.kgz.hr/hr/knjiznice/bibliobusna-sluzba/raspored-bibliobusnih-stajalista/65960";
        //>>> uncomment >>>  WebContent.GetWebContent(url, htmlFilename);  //Process.Start("NOTEPAD.EXE", htmlFilename);

        string pageContent = System.IO.File.ReadAllText(htmlFilename);

        List<PodaciZaDan> dani = WebContent.ParseWebContent(pageContent);

        List<PodaciZaSesiju> sesije = Tools.PretvoriUSesije(dani);

        foreach (var sesija in sesije)
        {
            Console.WriteLine(sesija);
        }

        Console.WriteLine("GOTOVO!");
        Console.ReadLine();
    }
}
