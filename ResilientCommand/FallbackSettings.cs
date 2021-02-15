namespace ResilientCommand
{
    public sealed class FallbackSettings
    {
        public FallbackSettings(
            bool isEnabled = Default.IsEnabled)
        {
            IsEnabled = isEnabled;
        }

        public static FallbackSettings DefaultFallbackSettings => new FallbackSettings();

        public bool IsEnabled { get; set; }

        public sealed class Default
        {
            public const bool IsEnabled = true;
        }
    }
}