# Umbraco Permissions

Since version 7.7 Umbraco has supported user groups. The `Escc.Umbraco.Permissions` namespace contains tools to help understand the permissions assigned to these groups.

## Understanding permission sets

Umbraco permissions are represented internally as alphanumeric codes. For example the permission set `H5UR` grants:

* Send to publish (H)
* Send to translation (5)
* Publish (U)
* Permissions (R)
 
The `UmbracoPermissions` class in this project provides constants to represent these codes as more memorable names that describe what they do. 

## Who has permissions here?

Permissions can be inherited and overridden, so it's useful to have an easy way to work out who has permission to a particular content node.

	var contentNodeId = 1;
	var permission = UmbracoPermission.UPDATE;

	var permissions = new UmbracoPermissionsReader(
					     ApplicationContext.Current.Services.UserService,
				  		 ApplicationContext.Current.Services.ContentService)
	var groupIds = permissions.GroupsWithPermissionForPage(contentNodeId, permission);

## Get active users in a group

When getting users in a group, you usually want just those whose accounts are able to sign in (not disabled etc). `UmbracoPermissionsReader` has a shortcut to doing the right checks:

	var groupId = 1;
	var permissions = new UmbracoPermissionsReader(
					     ApplicationContext.Current.Services.UserService,
				  		 ApplicationContext.Current.Services.ContentService)
	var users = permissions.ActiveUsersInGroup(groupId); 