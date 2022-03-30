using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IndecopiVirtualAsistant.Others
{
    public class Utils
    {
        public static string getName(string text, string other) {
            Regex regex = new Regex(@"\s[A-Z\u00C0-\u017F][a-zA-Z\u00C0-\u017F]+(\s[A-Z\u00C0-\u017F][a-zA-Z\u00C0-\u017F]+)?(\s[A-Z\u00C0-\u017F][a-zA-Z\u00C0-\u017F]+)?(\s[A-Z\u00C0-\u017F][a-zA-Z\u00C0-\u017F]+)?");
            Match match =  regex.Match(text);
            if (match.Success)
                return match.Value.Trim();
            else
                return other;
        }

        public static string getDocument(string text, string other)
        {
            Regex regex = new Regex(@"(\w+(-)*\d+(-)*\w+)");
            Match match = regex.Match(text);
            if (match.Success)
                return match.Value;
            else
                return other;
        }

        public static string getExpedient(string text, string other)
        {
            Regex regex = new Regex(@"((\d+)+(-\d+)?)+");
            Match match = regex.Match(text);
            if (match.Success)
                return match.Value;
            else
                return other;
        }
    }
}
