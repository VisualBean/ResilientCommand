using System;

namespace ResilientCommand
{
    public sealed class CollapserSettings
    {
        public CollapserSettings(
            bool isEnabled = Default.IsEnabled,
            TimeSpan? window = null)
        {
            IsEnabled = isEnabled;
            Window = window ?? Default.Window;
        }

        public static CollapserSettings DefaultCollapserSettings => new CollapserSettings();

        public bool IsEnabled { get; set; }

        public TimeSpan Window { get; private set; }

        public static class Default
        {
            public const bool IsEnabled = false;


            public static readonly TimeSpan Window;

            static Default()
            {
                Window = TimeSpan.FromMilliseconds(100);
            }
        }
    }
}