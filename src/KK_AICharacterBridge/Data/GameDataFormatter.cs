using System.Collections.Generic;

namespace AICharacterBridge.Data
{
    /// <summary>
    /// ゲーム内データを人間/AI可読な文字列にフォーマットする静的クラス
    /// Static class for formatting game data into human/AI-readable strings
    /// </summary>
    public static class GameDataFormatter
    {
        #region Time Period Mapping

        private static readonly Dictionary<string, string> TimePeriodMap = new Dictionary<string, string>
        {
            { "WakeUp", "Early Morning (Wake Up)" },
            { "Morning", "Morning" },
            { "GotoSchool", "Morning Commute" },
            { "HR1", "Morning Homeroom" },
            { "Lesson1", "First Period" },
            { "LunchTime", "Lunch Time" },
            { "Lesson2", "Afternoon Classes" },
            { "HR2", "Afternoon Homeroom" },
            { "StaffTime", "Club Activities" },
            { "AfterSchool", "After School" },
            { "GotoMyHouse", "Going Home" },
            { "MyHouse", "At Home" }
        };

        /// <summary>
        /// 時間帯を読みやすい形式に変換
        /// Converts time period to readable format
        /// </summary>
        public static string FormatTimePeriod(string timePeriod)
        {
            if (string.IsNullOrEmpty(timePeriod))
                return "Unknown Time";

            if (TimePeriodMap.TryGetValue(timePeriod, out string formatted))
                return formatted;

            return timePeriod;
        }

        #endregion

        #region Location Mapping

        private static readonly Dictionary<string, string> LocationMap = new Dictionary<string, string>
        {
            { "1F", "1st Floor Hallway" },
            { "2F", "2nd Floor Hallway" },
            { "3F", "3rd Floor Hallway" },
            { "教室1-1", "Classroom 1-1" },
            { "教室2-1", "Classroom 2-1" },
            { "教室2-2", "Classroom 2-2" },
            { "教室3-1", "Classroom 3-1" },
            { "1F女子トイレ", "Girls' Restroom (1F)" },
            { "2F女子トイレ", "Girls' Restroom (2F)" },
            { "3F女子トイレ", "Girls' Restroom (3F)" },
            { "職員室", "Staff Room" },
            { "職員トイレ(男)", "Men's Restroom (Staff)" },
            { "保健室", "Nurse's Office" },
            { "図書室", "Library" },
            { "部室", "Club Room" },
            { "漫研", "Manga Club Room" },
            { "和室", "Japanese-style Room" },
            { "体育倉庫", "P.E. Storage" },
            { "体育館", "Gymnasium" },
            { "中庭", "Courtyard" },
            { "グラウンド", "School Grounds" },
            { "屋上", "Rooftop" },
            { "プール", "Swimming Pool" },
            { "食堂", "Cafeteria" },
            { "シャワールーム", "Shower Room" },
            { "ロッカールーム", "Locker Room" },
            { "裏庭", "Back Garden" }
        };

        /// <summary>
        /// 場所を読みやすい形式に変換
        /// Converts location to readable format
        /// </summary>
        public static string FormatLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
                return "Unknown Location";

            if (LocationMap.TryGetValue(location, out string formatted))
                return formatted;

            return location;
        }

        #endregion

        #region Week Mapping

        private static readonly Dictionary<string, string> WeekMap = new Dictionary<string, string>
        {
            { "Monday", "Monday" },
            { "Tuesday", "Tuesday" },
            { "Wednesday", "Wednesday" },
            { "Thursday", "Thursday" },
            { "Friday", "Friday" },
            { "Saturday", "Saturday" },
            { "Sunday", "Sunday" }
        };

        /// <summary>
        /// 曜日を読みやすい形式に変換
        /// Converts week to readable format
        /// </summary>
        public static string FormatWeek(string week)
        {
            if (string.IsNullOrEmpty(week))
                return "Unknown Day";

            if (WeekMap.TryGetValue(week, out string formatted))
                return formatted;

            return week;
        }

        #endregion

    }
}
