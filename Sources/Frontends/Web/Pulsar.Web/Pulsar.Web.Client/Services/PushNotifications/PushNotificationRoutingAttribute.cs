﻿using Pulsar.Services.Shared.PushNotifications;

namespace Pulsar.Web.Client.Services.PushNotifications;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class PushNotificationRoutingAttribute : Attribute
{
	public PushNotificationRouteKey RouteKey { get; private init; }
	public string Route { get; private init; }

	public PushNotificationRoutingAttribute(PushNotificationRouteKey routeKey, string route)
	{
		RouteKey = routeKey;
		Route = route;
	}
}
