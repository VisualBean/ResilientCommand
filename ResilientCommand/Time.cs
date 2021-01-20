using System;
using System.Collections.Generic;
using System.Text;

namespace ResilientCommand
{
    public class Time
    {
        private int seconds;

        private Time(int seconds)
        {
            this.seconds = seconds;
        }

        public static Time Hours(int n)
        {
            return new Time(n * 60 * 60);
        }

        public static Time Minutes(int n)
        {
            return new Time(n * 60);
        }

        public static Time Seconds(int n)
        {
            return new Time(n);
        }

        public int InMiliseconds => seconds * 1000;

        public int InSeconds => seconds;

        public double InMinutes => seconds / 60;

        public double InHours => InMinutes / 60;
    }

}
