using MediatR;
using Pulsar.BuildingBlocks.EventBus.Contracts.PushNotifications;
using Pulsar.Services.Shared;

namespace Pulsar.Web.Client.Services.PushNotifications;

public class PushNotificationEvent : INotification
{
	public PushNotificationEvent(PushNotificationDataWithId pushNotificationData)
	{
		PushNotificationData = pushNotificationData;
	}

	public PushNotificationDataWithId PushNotificationData {  get; private init; }

	public static INotification StronglyTyped(PushNotificationDataWithId pushNotificationData, Type dataType)
	{
		return (INotification)Activator.CreateInstance(typeof(PushNotificationEvent<>).MakeGenericType(dataType), pushNotificationData)!;
	}
}

public class PushNotificationEvent<TData> : PushNotificationEvent, INotification where TData : class
{
	public PushNotificationEvent(PushNotificationDataWithId pushNotificationData) : base(pushNotificationData)
	{
		Data = pushNotificationData.Data != null ? pushNotificationData.Data.FromJsonString<TData>() : null;
	}

	public TData? Data { get; private init; }
}
