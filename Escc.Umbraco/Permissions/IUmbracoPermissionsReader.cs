using System.Collections.Generic;
using Umbraco.Core.Models.Membership;

namespace Escc.Umbraco.Permissions
{
    /// <summary>
    /// Gets information on permissions to an instance of Umbraco
    /// </summary>
    public interface IUmbracoPermissionsReader
    {
        /// <summary>
        /// Gets the id and email address of users in the specified group who are approved and not locked out or disabled
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        IList<IUser> ActiveUsersInGroup(int groupId);

        /// <summary>
        /// Gets the ids of all user groups with permissions to a page
        /// </summary>
        /// <param name="pageId">The integer id of a content node</param>
        /// <param name="permission">The permission code to look for, available as constants in <see cref="UmbracoPermission"/></param>
        /// <returns></returns>
        List<int> GroupsWithPermissionForPage(int pageId, string permission);
    }
}