// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Interoperability", "CA1404:CallGetLastErrorImmediatelyAfterPInvoke", Scope = "member", Target = "NuGet.Data.BrowserCache.#GetInternal(System.IntPtr,System.UInt32,System.UInt32)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "NuGet.Data.Native.#DeleteUrlCacheEntry(System.String)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "NuGet.Data.Native.#RetrieveUrlCacheEntryStream(System.String,System.IntPtr,System.UInt32&,System.Boolean,System.UInt32)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "NuGet.Data.Native.#ReadUrlCacheEntryStream(System.IntPtr,System.UInt32,System.IntPtr,System.UInt32&,System.UInt32)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "NuGet.Data.Native.#UnlockUrlCacheEntryStream(System.IntPtr,System.UInt32)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "NuGet.Data.Native.#CommitUrlCacheEntry(System.String,System.String,System.Runtime.InteropServices.ComTypes.FILETIME,System.Runtime.InteropServices.ComTypes.FILETIME,NuGet.Data.Native+EntryType,System.String,System.Int32,System.String,System.String)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass", Scope = "member", Target = "NuGet.Data.Native.#CreateUrlCacheEntry(System.String,System.Int32,System.String,System.Text.StringBuilder,System.Int32)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "member", Target = "NuGet.Data.JsonLdPage.#Dispose()")]
[assembly: SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "type", Target = "NuGet.Data.DataClient")]
[assembly: SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "member", Target = "NuGet.Data.DataClient.#Dispose()")]
[assembly: SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "type", Target = "NuGet.Data.EntityCache")]
[assembly: SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "member", Target = "NuGet.Data.EntityCache.#Dispose()")]
[assembly: SuppressMessage("Microsoft.Reliability", "CA2002:DoNotLockOnObjectsWithWeakIdentity", Scope = "member", Target = "NuGet.Data.EntityCache.#TidyTimerTick(System.Object)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "type", Target = "NuGet.Data.JsonLdPage")]
[assembly: SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible", Scope = "type", Target = "NuGet.Data.CacheHttpClient")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Scope = "member", Target = "NuGet.Data.BrowserCache.#Add(System.String,System.IO.Stream,System.DateTime)")]
[assembly: SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "member", Target = "NuGet.Data.UriLock.#Dispose()")]
[assembly: SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Scope = "type", Target = "NuGet.Data.UriLock")]
[assembly: SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible", Scope = "type", Target = "NuGet.Data.DataClient")]
[assembly: SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible", Scope = "type", Target = "NuGet.Data.DataClientMessageHandler")]
[assembly: SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible", Scope = "type", Target = "NuGet.Data.CacheHandler")]
[assembly: SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible", Scope = "type", Target = "NuGet.Data.CacheResponse")]
[assembly: SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible", Scope = "type", Target = "NuGet.Data.RequestModifierHandler")]
[assembly: SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible", Scope = "type", Target = "NuGet.Data.RetryHandler")]
