﻿#if UNITY_IOS
using UnityEngine;
using System.Collections;
using System;
using AOT;
using System.Collections.Generic;
using EasyMobile.MiniJSON;
using EasyMobile.Internal;
using EasyMobile.NotificationsInternal.iOS;

namespace EasyMobile.NotificationsInternal
{
    internal class iOSLocalNotificationClient : ILocalNotificationClient
    {
        private bool mIsInitialized;

        #region ILocalNotificationClient implementation

        public void Init(NotificationsSettings settings, INotificationListener listener)
        {
            // Authorization options.
            var iOSAuthOptions = iOSNotificationHelper.ToIOSNotificationAuthOptions(settings.iOSAuthOptions);

            // Listener.
            var iOSListenerInfo = iOSNotificationHelper.ToIOSNotificationListenerInfo(listener);

            // Categories.
            var categories = new List<NotificationCategory>();
            categories.Add(settings.DefaultCategory);

            if (settings.UserCategories != null)
                categories.AddRange(settings.UserCategories);

            var iOSCategories = iOSNotificationHelper.ToIOSNotificationCategories(categories.ToArray());
            string iOSCategoriesJson = iOSNotificationHelper.ToIOSNotificationCategoriesJson(iOSCategories);

            iOSNotificationNative._InitNotifications(ref iOSAuthOptions, ref iOSListenerInfo, iOSCategoriesJson);
            mIsInitialized = true;
        }

        public bool IsInitialized()
        {
            return mIsInitialized;
        }

        public void ScheduleLocalNotification(string id, DateTime fireDate, NotificationContent content)
        {
            fireDate = fireDate.ToLocalTime();
            TimeSpan delay = fireDate <= DateTime.Now ? TimeSpan.Zero : fireDate - DateTime.Now;
            ScheduleLocalNotification(id, delay, content);
        }

        public void ScheduleLocalNotification(string id, TimeSpan delay, NotificationContent content)
        {
            if (!mIsInitialized)
            {
                Debug.Log("Please initialize first.");
                return;
            }

            // Prepare iOSNotificationContent
            var iOSContent = iOSNotificationHelper.ToIOSNotificationContent(content);

            iOSNotificationNative._ScheduleLocalNotification(id, ref iOSContent, (long)delay.TotalSeconds);
        }

        public void ScheduleLocalNotification(string id, TimeSpan delay, NotificationContent content, NotificationRepeat repeat)
        {
            if (repeat == NotificationRepeat.None)
            {
                ScheduleLocalNotification(id, delay, content);
                return;
            }

            if (!mIsInitialized)
            {
                Debug.Log("Please initialize first.");
                return;
            }

            // Prepare iOSNotificationContent
            var iOSContent = iOSNotificationHelper.ToIOSNotificationContent(content);

            // Prepare dateComponents
            var fireDate = DateTime.Now + delay;
            var dateComponents = new iOSDateComponents();
            dateComponents.year = fireDate.Year;
            dateComponents.month = fireDate.Month;
            dateComponents.day = fireDate.Day;
            dateComponents.hour = fireDate.Hour;
            dateComponents.minute = fireDate.Minute;
            dateComponents.second = fireDate.Second;

            iOSNotificationNative._ScheduleRepeatLocalNotification(id, ref iOSContent, ref dateComponents, repeat);
        }

        public void GetPendingLocalNotifications(Action<NotificationRequest[]> callback)
        {
            Helper.NullArgumentTest(callback);
            callback = Helper.ToMainThread<NotificationRequest[]>(callback);

            InternalGetPendingNotificationRequests(
                (iOSNotificationRequest[] iOSRequests) =>
                {
                    var pendingRequests = new List<NotificationRequest>();

                    for (int i = 0; i < iOSRequests.Length; i++)
                    {
                        // The remote check is kind of overkill. APNS should post
                        // remote notifications as soon as it can so they're not likely get into the pending list.
                        bool isRemote;
                        var request = iOSNotificationHelper.ToCrossPlatformNotificationRequest(iOSRequests[i], out isRemote);

                        if (!isRemote)
                            pendingRequests.Add(request);
                    }

                    callback(pendingRequests.ToArray()); 
                });
        }

        public void CancelPendingLocalNotification(string id)
        {
            iOSNotificationNative._RemovePendingNotificationRequestWithId(id);
        }

        public void CancelAllPendingLocalNotifications()
        {
            iOSNotificationNative._RemoveAllPendingNotificationRequests();
        }

        public void RemoveAllDeliveredNotifications()
        {
            iOSNotificationNative._RemoveAllDeliveredNotifications();
        }

        #endregion

        #region Internal methods

        private static void InternalGetPendingNotificationRequests(Action<iOSNotificationRequest[]> callback)
        {
            Helper.NullArgumentTest(callback);

            iOSNotificationNative._GetPendingNotificationRequests(
                InternalGetPendingNotificationRequestsCallback,
                PInvokeCallbackUtil.ToIntPtr<GetPendingNotificationRequestsResponse>(
                    response =>
                    {
                        callback(response.GetNotificationRequests());
                    },
                    GetPendingNotificationRequestsResponse.FromPointer
                )
            );
        }

        [MonoPInvokeCallback(typeof(iOSNotificationNative.GetPendingNotificationRequestsCallback))]
        private static void InternalGetPendingNotificationRequestsCallback(IntPtr response, IntPtr callbackPtr)
        {
            PInvokeCallbackUtil.PerformInternalCallback(
                "iOSNotificationClient#GetPendingNotificationRequestsCallback",
                PInvokeCallbackUtil.Type.Temporary,
                response,
                callbackPtr);
        }
    }

    #endregion
}
#endif