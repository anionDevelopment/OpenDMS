using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OpenDMSBackend.Core.Constants
{
    public partial class CodeUnitSpecificConstants
    {
        internal const string UsernameAdmin = "admin";
        internal const string RolenameUsers = "Users";
        internal const string RolenameAdmins = "Administrators";
        internal const string ActionAccessLoginArea = "AccessLoginArea";

        /// <summary>Key of the setting which controls whether an AI-summary is generated automatically whenever a document is added or changed.</summary>
        internal const string SettingKeyAutoGenerateAISummary = "AutoGenerateAISummary";

        /// <summary>Key of the user-setting which holds the color-scheme the user chose.</summary>
        internal const string UserSettingKeyTheme = "Theme";

        /// <summary>The color-scheme of a user who did not choose one. It follows the setting of the operating-system of that user.</summary>
        internal const string ThemeSystem = "system";

        /// <summary>The color-scheme which is always light, regardless of the setting of the operating-system.</summary>
        internal const string ThemeLight = "light";

        /// <summary>The color-scheme which is always dark, regardless of the setting of the operating-system.</summary>
        internal const string ThemeDark = "dark";

        /// <summary>All values which are accepted as color-scheme of a user.</summary>
        internal static readonly ISet<string> Themes = new HashSet<string>() { ThemeSystem, ThemeLight, ThemeDark };

        internal const string BusinessMetricsPrefix = $"{GeneralConstants.CodeUnitName}_business_";
        internal const string MetricsNameAmountOfDocuments = $"{BusinessMetricsPrefix}_AmountOfDocuments";

        [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9]*$")]
        public static partial Regex UserNameRegex();
    }
}
