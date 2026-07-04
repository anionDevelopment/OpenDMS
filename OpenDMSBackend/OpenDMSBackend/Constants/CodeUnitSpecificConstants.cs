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

        internal const string BusinessMetricsPrefix = $"{GeneralConstants.CodeUnitName}_business_";
        internal const string MetricsNameAmountOfDocuments = $"{BusinessMetricsPrefix}_AmountOfDocuments";

        [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9]*$")]
        public static partial Regex UserNameRegex();
    }
}
