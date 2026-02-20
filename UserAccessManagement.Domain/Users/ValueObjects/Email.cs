namespace UserAccessManagement.Domain.Users.ValueObjects
{
    public record Email
    {
        public string Value { get; }

        private Email(string value)
        {
            Value = value;
        }

        public static Email Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
                throw new ArgumentException("El formato del email no es valido.");
           

            return new Email(value.ToLower().Trim());
        }
    }
}
