using System.Text.RegularExpressions;

namespace OpenDMSBackend.Core.Constants
{
    public partial class CodeUnitSpecificConstants
    {
        internal const string UsernameAdmin = "admin";
        internal const string RolenameUsers = "Users";
        internal const string RolenameAdmins = "Administrators";
        internal const string RolenameModerartors = "Moderators";
        internal const string ActionAccessLoginArea = "AccessLoginArea";

        internal const string BusinessMetricsPrefix = $"{GeneralConstants.CodeUnitName}_business_";
        internal const string MetricsNameAmountOfDocuments = $"{BusinessMetricsPrefix}_AmountOfDocuments";

        [GeneratedRegex("^[a-zA-Z][a-zA-Z0-9]*$")]
        public static partial Regex UserNameRegex();
    }
}
