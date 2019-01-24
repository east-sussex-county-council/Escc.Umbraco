namespace Escc.Umbraco.Permissions
{
    /// <summary>
    /// Umbraco permissions are represented internally as alphanumeric codes, represented here as constants
    /// </summary>
    public static class UmbracoPermission
    {
        public const string NONE = "-";
        public const string CULTURE_AND_HOSTNAMES = "I";
        public const string AUDIT_TRAIL = "Z";
        public const string BROWSE_NODE = "F";
        public const string CHANGE_DOCUMENT_TYPE = "7";
        public const string COPY = "O";
        public const string DELETE = "D";
        public const string MOVE = "M";
        public const string CREATE = "C";
        public const string PUBLIC_ACCESS= "P";
        public const string UNPUBLISH = "U";
        public const string PERMISSIONS = "R";
        public const string ROLLBACK = "K";
        public const string SEND_TO_TRANSLATION = "5";
        public const string SORT = "S";
        public const string SEND_TO_PUBLISH= "H";
        public const string TRANSLATE = "4";
        public const string UPDATE = "A";
    }
}