namespace Pulsar.Services.Identity.UI.Models
{
    public class SelectOption
    {
        public string Id { get; }
        public string Text { get; }

        public SelectOption(string id, string text)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
    }
}
