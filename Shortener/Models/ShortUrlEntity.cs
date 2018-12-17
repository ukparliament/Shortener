namespace Shortener
{
    using Microsoft.WindowsAzure.Storage.Table;

    public class ShortUrlEntity : TableEntity
    {
        public string Url { get; set; }
    }
}
