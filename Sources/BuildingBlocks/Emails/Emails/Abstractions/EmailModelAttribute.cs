namespace Pulsar.BuildingBlocks.Emails.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EmailModelAttribute : Attribute
    {
        public string View { get; }
        public string? SubjectProperty { get; set; }
        public string? ToProperty { get; set; }

        public EmailModelAttribute(string view)
        {
            View = view;
        }

        public static EmailModelAttribute? GetAttribute(Type ty) => ty.GetCustomAttributes(typeof(EmailModelAttribute), true).Cast<EmailModelAttribute>().FirstOrDefault();

        public string? GetSubject(object model)
        {
            if (SubjectProperty is null)
                return null;

            return model.GetType().GetProperty(SubjectProperty)?.GetValue(model) as string;
        }

        public string? GetTo(object model)
        {
            if (ToProperty is null)
                return null;

            return model.GetType().GetProperty(ToProperty)?.GetValue(model) as string;
        }
    }
}
