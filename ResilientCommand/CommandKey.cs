namespace ResilientCommand
{
    public class CommandKey
    {
        private string key;
        public CommandKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new System.ArgumentException($"'{nameof(key)}' cannot be null or whitespace", nameof(key));
            }

            this.key = key;
        }

        public string Key => key;

        public override bool Equals(object obj)
        {
            return Equals(obj as CommandKey);
        }

        public bool Equals(CommandKey key)
        {
            return key != null && this.key == key.key;
        }

        public override int GetHashCode()
        {
            return key.GetHashCode();
        }

        public override string ToString()
        {
            return key.ToString();
        }
    }
}