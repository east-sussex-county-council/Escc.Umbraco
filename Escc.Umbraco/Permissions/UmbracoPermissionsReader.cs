using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Web;

namespace Escc.Umbraco.Permissions
{
    /// <summary>
    /// A service that provides information about Umbraco permissions
    /// </summary>
    public class UmbracoPermissionsReader : IUmbracoPermissionsReader
    {
        private readonly IContentService _contentService;
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoPermissionsReader" /> class.
        /// </summary>
        /// <param name="userService">The user service.</param>
        /// <param name="contentService">The content service.</param>
        /// <exception cref="ArgumentNullException">
        /// userService
        /// or
        /// contentService
        /// </exception>
        public UmbracoPermissionsReader(IUserService userService, IContentService contentService)
        {
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
        }

        /// <summary>
        /// Gets the users in the specified group who are approved and not locked out or disabled
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public IList<IUser> ActiveUsersInGroup(int groupId)
        {
            return _userService.GetAllInGroup(groupId).Where(x => x.IsApproved && !x.IsLockedOut && x.UserState != UserState.Disabled).ToList();
        }

        /// <summary>
        /// Gets the ids of all user groups with permissions to a page
        /// </summary>
        /// <param name="pageId">The integer id of a content node</param>
        /// <param name="permission">The permission code to look for, available as constants in <see cref="UmbracoPermission"/></param>
        /// <exception cref="ArgumentException">Thrown if pageId does not refer to a content node</exception>
        /// <returns></returns>
        public List<int> GroupsWithPermissionForPage(int pageId, string permission)
        {
            var groupsWithAllowPermissionsForNode = new List<int>();
            var groupsWithDenyPermissionsForNode = new List<int>();
            var contentNode = _contentService.GetById(pageId);
            if (contentNode == null) throw new ArgumentException($"pageId {pageId} was not found", nameof(pageId));
            GetPermissionsForNodeWithInheritance(contentNode, permission, groupsWithAllowPermissionsForNode, groupsWithDenyPermissionsForNode);
            return groupsWithAllowPermissionsForNode;
        }

        private void GetPermissionsForNodeWithInheritance(IContent entity, string permission, List<int> groupIdsWithAllowPermission, List<int> groupIdsWithDenyPermission)
        {
            // if no permissions at all, then there will be only one element which will contain a "-" so exclude those
            var entityPermissions = _contentService.GetPermissionsForEntity(entity)
                    .Where(x => x.AssignedPermissions.Count() > 1 || x.AssignedPermissions[0] != UmbracoPermission.NONE);

            foreach (var entityPermission in entityPermissions)
            {
                // If a new permissions set is assigned which does not contain the permission we're interested in,
                // it's effectively a Deny permission for the one we're interested in.
                if (entityPermission.AssignedPermissions.Count() == 1 && !entityPermission.AssignedPermissions[0].Contains(permission))
                {
                    groupIdsWithDenyPermission.Add(entityPermission.UserGroupId);
                }

                if (entityPermission.AssignedPermissions[0].Contains(permission) && !groupIdsWithDenyPermission.Contains(entityPermission.UserGroupId))
                {
                    groupIdsWithAllowPermission.Add(entityPermission.UserGroupId);
                }
            }

            // Permissions in Umbraco are inherited from ancestor nodes, so look up the tree for further permissions
            entity = _contentService.GetById(entity.ParentId);
            if (entity != null)
            {
                GetPermissionsForNodeWithInheritance(entity, permission, groupIdsWithAllowPermission, groupIdsWithDenyPermission);
            }
        }
    }
}