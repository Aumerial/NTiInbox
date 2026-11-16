# NTi Inbox

**A message queue inbox for IBM i, in .NET.**
Proof of concept (POC) provided by [Aumerial](https://www.aumerial.com/) with the trial license of the **NTi** data provider (`Aumerial.Data.Nti`).

[Lire en français](README.fr.md)

NTi Inbox brings IBM i message queues, as seen on a 5250 screen with `WRKMSG`, `SNDMSG` and `SNDBRKMSG`, into a web interface laid out like a mailbox: the queues in a side column, the messages of the selected queue as a list, and a reading pane. Messages awaiting a reply come first, repeated messages are grouped, and the answer, the deletion or the new message is one click away.

It is also a **sample project**: all the code is provided to show how to drive an IBM i from C# with NTi (system API calls and user spaces), and to be modified or extended.

> ## Important disclaimer
>
> **NTi Inbox acts directly on the IBM i partition it is connected to.** It can send messages, reply to inquiry messages (a reply can restart or stop the job waiting for it), delete messages and send break messages that interrupt user sessions, with the authority of the profile used for the connection.
>
> - This project is a **demonstration POC**, provided **as is, without warranty** of any kind, including fitness for a particular purpose or error-free operation.
> - **Aumerial accepts no liability** for any data loss, service interruption, or direct or indirect damage resulting from the use, modification or deployment of this project, in particular because of the actions that are run.
> - The user is **solely responsible** for the actions they run and for the environment they run them on.
> - **Use a test or development partition**, never a production environment, unless you fully understand the consequences. Keep up-to-date backups.
> - Use an IBM i profile with **limited authority**: the application never elevates privileges, so a restricted profile mechanically limits what an action can do.
>
> Every change asks for a confirmation first, and a banner flags production and elevated authorities (see [Security](#7-security)), but this is an aid to vigilance, not a protection.

## Contents

1. [Features](#1-features)
2. [Requirements](#2-requirements)
3. [Step-by-step setup](#3-step-by-step-setup)
4. [Configuration](#4-configuration)
5. [Language](#5-language)
6. [Test walkthrough](#6-test-walkthrough)
7. [Security](#7-security)
8. [Code architecture](#8-code-architecture)
9. [Extending the project](#9-extending-the-project)
10. [Troubleshooting](#10-troubleshooting)
11. [Known limitations](#11-known-limitations)
12. [Support and terms of use](#12-support-and-terms-of-use)

## 1. Features

- **Queue discovery**: the message queues of a library or of a special value (`*LIBL`, `*ALLUSR`, `*CURLIB`, `*USRLIBL`), with a filter on the list and the number of messages awaiting a reply for the queues already read.
- **Inbox view**: the most recent messages of a queue, newest first, with the messages awaiting a reply in their own section at the top.
- **Filters and search**: reply status, minimum severity, message type, send period, one hour picked on the activity chart, and a text search on the id, the text, the substitution data, the sender job and the sender profile.
- **Grouping of repeated messages**: identical messages collapse into one row with their count and period, and expand on demand.
- **Activity chart**: the last 24 hours, hour by hour, stacked by severity tier.
- **Reading pane**: text, severity, type, sender job and profile, substitution data, and the message help (second level text) read from the system on demand.
- **Sender job panel**: status, current function, resource usage and lock waits of the job that sent the message.
- **Reply** to an inquiry message, with the default reply shown next to the field.
- **Send**: an immediate or predefined message (`*INFO`, `*COMP`, `*DIAG` or `*INQ`) to up to fifty queues, `*SYSOPR` and `*ALLACT` included. An `*INQ` message targets a single queue and names a reply queue.
- **Break message** to a workstation message queue, confirmed by typing the workstation name.
- **Delete** one message, a selection, or all the messages shown.
- **Queue attributes**: size, delivery method, severity filter, alerts and more.
- **Message catalog**: text, help and substitution variables of a predefined message from its id.
- **Connected profile page**: special authorities, message queue and delivery method.
- **Automatic reading** of the open queue every 30 seconds, with the count in the tab title and an optional browser notification for new messages awaiting a reply.
- **Saved connections** (without password), with a red production marking.
- **Light and dark themes** (dark by default) and an **English and French interface**.

Nothing is installed on the IBM i: everything goes through system API calls from .NET, plus one CL command (`DLTUSRSPC`) that deletes a temporary user space in `QTEMP`.

## 2. Requirements

### IBM i side

| Item | Detail |
|---|---|
| Version | IBM i V5R4 minimum, V7R4 or later recommended (NTi requirement) |
| TCP services | `*DATABASE`, `*RMTCMD` and `*SIGNON` started |
| Ports | 8471, 8475 and 8476 open from the machine running the application (9471, 9475 and 9476 with SSL/TLS) |
| User profile | A profile and its password. Its authorities determine what the application can read and do |
| NTi license | A license key installed on the partition, in library `KNTI` by default (see step 1 below) |

### Workstation side

| Item | Detail |
|---|---|
| [.NET 10 SDK](https://dotnet.microsoft.com/download) | Check with `dotnet --version` (must show `10.x`) |
| Git | To get the project |
| NuGet access | To restore the `Aumerial.Data.Nti` package |
| Editor (optional) | Visual Studio, Rider or VS Code |
| Network | The machine must reach the IBM i on the ports above |

## 3. Step-by-step setup

### 1. Get and install the NTi license

A license is tied to **one partition** (serial number and partition identifier) and is valid for a limited time. A free 30-day trial license is available. Once the license has expired, connections to the IBM i are refused.

1. Create an account on the customer portal: <https://portal.aumerial.com/>.
2. Declare the IBM i partition (LPAR) on which the application will be tested.
3. Download the automatically generated **license SQL script**. It contains the key and the values specific to the partition.
4. Run it on the IBM i, for example with **Run SQL Scripts** in IBM i Access Client Solutions (ACS). The script creates the `KNTI` library and registers the license in it.

The reference documentation is here: <https://www.documentation.aumerial.com/en/docs/installation>.

### 2. Start the IBM i TCP services

If these services are not already running, start them from a 5250 session or from ACS:

```
STRTCPSVR SVR(*DATABASE)
STRTCPSVR SVR(*RMTCMD)
STRTCPSVR SVR(*SIGNON)
```

### 3. Get the project

```
git clone https://github.com/Aumerial/NTiInbox.git
cd NTiInbox
```

### 4. Check the SDK and build

```
dotnet --version
dotnet build
```

The first build automatically restores the `Aumerial.Data.Nti` NuGet package. It must finish without errors. Otherwise, see [Troubleshooting](#10-troubleshooting).

### 5. Run the application

```
dotnet run --project NTiInbox
```

Then open the address shown in the console (by default <http://localhost:5115>).

To use HTTPS (recommended as soon as the application is no longer only local), have the development certificate trusted once, then run the `https` profile (<https://localhost:7263>):

```
dotnet dev-certs https --trust
dotnet run --project NTiInbox --launch-profile https
```

With Visual Studio, open `NTiInbox.slnx` and run the project (F5).

#### Alternative: run with Docker

No .NET SDK needed. The image is published on Docker Hub:

```
docker run -p 127.0.0.1:8080:8080 -v ntiinbox-data:/app/App_Data aumerial/ntiinbox
```

Then open <http://localhost:8080>. The `127.0.0.1` part keeps the application reachable from your machine only: it has no sign-in of its own, so do not expose it to a network or to the Internet. The volume keeps your saved connections between restarts. To build the image yourself instead, run `docker build -t ntiinbox .` from the `NTiInbox` project folder (the one containing `Dockerfile`).

### 6. Connect

The application opens on the sign-on screen. Fill in:

- the **system** (DNS name or IP address of the IBM i),
- the **user profile** and its **password**,
- the **library** searched for message queues (optional, `*LIBL` by default),
- the **SSL/TLS** option if the host servers are configured for encryption,
- the **production** option if the partition is a production one, which marks the whole interface in red.

Once the connection succeeds, it is remembered **without the password**: the next time, a click on the saved connection only asks for the password.

## 4. Configuration

Everything is optional: the sign-on form works without any configuration. The `Ibmi` section of `NTiInbox/appsettings.json` only pre-fills the form. All values are empty by default, and the password is never read from configuration.

| Key | Purpose |
|---|---|
| `Ibmi:Host` | System name or address proposed in the form |
| `Ibmi:User` | User profile proposed in the form |
| `Ibmi:Library` | Library (or special value) searched for message queues. `*LIBL` when empty |
| `Ibmi:UseSsl` | Ticks the TLS option by default |
| `Ibmi:ConnectionOptions` | Extra NTi connection string keywords, appended to every connection |

The extra keywords allow any option of the provider, for example another license library:

```
license library=MYLIB
```

The list of keywords is in the [connection string documentation](https://www.documentation.aumerial.com/en/docs/connection-string).

> **Never write a password in `appsettings.json` or in the repository.** Use .NET user secrets or environment variables.

With user secrets (stored outside the repository, in the machine's user profile):

```
dotnet user-secrets init --project NTiInbox
dotnet user-secrets set "Ibmi:Host" "myibmi.example.com" --project NTiInbox
dotnet user-secrets set "Ibmi:User" "MYPROFILE" --project NTiInbox
```

With environment variables (the double underscore replaces the colon):

```
# PowerShell
$env:Ibmi__Host = "myibmi.example.com"
$env:Ibmi__User = "MYPROFILE"
```

Connection profiles saved from the sign-on screen are written without password in the `App_Data` folder of the project (one JSON file per user, plus the cookie protection keys). That folder is ignored by Git.

## 5. Language

The interface is available in **English** and **French**. The first time the application is opened, it uses the language of the browser (English if it is neither). The **EN / FR** switch on the sign-on screen and in the left rail changes the language immediately, without losing the IBM i connection, and the choice is remembered for a year (cookie `.AspNetCore.Culture`).

Two kinds of text appear on screen:

- **Texts of the application** (labels, buttons, messages) are translated by the application. They live in two plain dictionaries, `Localization/Strings.En.cs` and `Localization/Strings.Fr.cs`. In development, the application refuses to start if a key exists in one language but not in the other.
- **Texts that come from the IBM i** (message text and help, queue and job names, descriptions) are returned by the system **in the language of the IBM i job**. They are displayed as received and are not translated by the application.

To add a language, create a third dictionary next to the two existing ones, register its culture in `Localizer.Cultures`, and select it in `Localizer.Texts` and `Strings.Get`.

## 6. Test walkthrough

This walkthrough validates the whole chain, from reading to answering. Use a test partition and a profile of your own.

1. Sign in.
2. Open `QSYSOPR` in the left column: the messages are listed, newest first. Open one and click **Show** to read its help.
3. Open the **Profile** page and click **Open my message queue**.
4. Click **New message**, keep your own queue as the recipient, type a text and send. Confirm. The message appears in your queue.
5. Delete it and confirm.
6. Send an `*INQ` message to your queue, with another queue of yours as reply queue. It appears in the **Awaiting reply** section. Reply to it and confirm.
7. Switch the language to FR and back: all application texts change, the message texts do not.
8. Click **Disconnect**, then sign in again: the saved connection only asks for the password.

## 7. Security

The code is meant to be read, tested and adapted by customers: the security choices are therefore explicit. Here is what the application guarantees, and what it does not.

| Point | Status |
|---|---|
| IBM i password | Kept only in server memory, for the duration of the session. Never written to disk, never in a saved connection, never in a log. |
| Error messages | Stripped of the current password and of any `password=` or `pwd=` pair before display (`IbmiSession.Sanitize`), since a driver may quote the connection string in its errors. |
| Displayed content | Message text, queue and job names come from the IBM i, hence from an uncontrolled source. They are encoded by Razor and never rendered as raw HTML. |
| Authority on the system | That of the IBM i profile used for the connection, and nothing else. The application never elevates privileges. |
| Changes | Sending, replying, deleting and sending a break message each open a confirmation window. A break message also requires typing the name of the workstation. |
| Warning banner | The confirmation windows flag deletions, a production connection, and a profile holding `*ALLOBJ` or `*SECADM`. |
| Temporary work space | A user space in `QTEMP`, specific to the IBM i job of the connection and deleted after use. |
| Web server | Outside development: HTTPS redirection, HSTS, and an error page without technical detail. |

### Limits to know before any shared deployment

**The application has no authentication of its own.** Anyone who reaches the URL can enter a connection. The only barrier is the IBM i profile and its password. This suits a workstation or a demonstration, **not a server exposed** on a network.

**Saved connections are not partitioned per person.** They are stored by user key (`App_Data/{key}.json`), but the provided implementation (`LocalCurrentUser`) returns a constant: on a shared server, all users would see the same saved connections (host, profile and options, never the passwords).

**The sign-on form accepts any host name.** On a server reachable by others, anyone could use it to try to reach any machine that the server itself can reach.

The replacement point for the first two limits is provided: `ICurrentUser` is an interface registered in `Program.cs`. For shared use, put the application behind real authentication and provide an implementation that returns the identity of the user. No other file needs to change.

## 8. Code architecture

A single project, `NTiInbox` (Blazor Server, .NET 10). Each folder answers one question, and the dependencies go in one direction: the interface calls `Messages` and `UserInterface`, which call `Connection` and `Storage`, which rely on `SystemApis`. `Localization` is read by every layer. Every file holds one type and is named after it.

```
NTiInbox/
├── Connection/     The IBM i connection: the user session, saved connection profiles
├── SystemApis/     The IBM i system APIs called through NTi, one class per API
├── Messages/       The message logic, with no screen: facade, formatting, filters, inbox state
├── UserInterface/  Notifications, dialog requests, browser notifications, theme
├── Storage/        Local JSON files, and who the current user is
├── Localization/   Language service, English and French dictionaries, error texts
├── Components/     Blazor interface (pages, shared components, layouts)
├── wwwroot/        app.css (single stylesheet, light and dark themes through CSS variables, every class prefixed nti-), logo, small scripts
├── Program.cs
├── appsettings.json
└── Dockerfile
```

### Connection: where the connection lives

| File | Purpose |
|---|---|
| `IbmiSession.cs` | The user's IBM i connection (NTi), its credentials in memory, and the single path every system call goes through |
| `ConnectionRequest.cs` | Parameters of a sign-on. The password lives in memory only |
| `ConnectionException.cs` | Sign-on failure, stripped of any credential |
| `ConnectionProfile.cs`, `ConnectionProfileStore.cs` | Saved connections (never the password) |
| `IbmiOptions.cs` | Optional defaults of the sign-on form (`Ibmi` section) |

### SystemApis: the system calls

| File | IBM i API | Purpose |
|---|---|---|
| `MessageListApi.cs`, `QueueMessage.cs` | `QMHLSTM` | List the messages of a queue |
| `MessageDescriptionApi.cs`, `MessageDescription.cs` | `QMHRTVM` | Read a predefined message (text, help, variables) |
| `SendMessageApi.cs`, `SendMessageRequest.cs` | `QMHSNDM` | Send a message to one or more queues |
| `BreakMessageApi.cs`, `BreakMessageRequest.cs` | `QMHSNDBM` | Send a break message |
| `ReplyMessageApi.cs` | `QMHSNDRM` | Reply to an inquiry message |
| `RemoveMessageApi.cs` | `QMHRMVM` | Remove a message from a queue |
| `QueueAttributesApi.cs`, `QueueAttributes.cs` | `QMHRMQAT` | Read the attributes of a queue |
| `QueueListApi.cs`, `QueueListEntry.cs` | `QUSLOBJ` | List the `*MSGQ` objects of a library |
| `JobInfoApi.cs`, `JobInformation.cs`, `JobRef.cs` | `QUSRJOBI` | Read a job, and the reference to a job |
| `UserInfoApi.cs`, `UserInformation.cs` | `QSYRUSRI` | Read the connected user profile |
| `UserSpaceApi.cs` | `QUSCRTUS`, `QUSRTVUS` | Create and read the temporary user spaces |
| `MessageQueueRef.cs` | n/a | Name and library of a queue, key of every queue operation |
| `ApiErrorCode.cs`, `IbmiApiException.cs` | n/a | Error structure of the APIs, and the exception raised from it |
| `NTiConnectionExtensions.cs` | n/a | `EnsureOpen()`: opens the connection only if it is not already open |

APIs that return a list go through a user space, as IBM i requires.

### Messages: the logic

| File | Purpose |
|---|---|
| `MessageQueueService.cs` | Single facade over the connection: discovery, reading, help, send, reply, delete |
| `MessageFormatter.cs`, `JobFormatter.cs` | Labels, severity and style classes of messages and jobs |
| `MessageGrouping.cs`, `MessageGroup.cs` | Grouping of identical messages |
| `QueueActivity.cs`, `ActivityBucket.cs` | Hourly activity of a queue, for the chart |
| `MessageFilter.cs`, `PeriodFilter.cs`, `ReplyStatusFilter.cs` | Filters of the message list |
| `SeverityLevel.cs` | Severity tiers |
| `InboxState.cs` | Queues, counts and current queue of the session |

### UserInterface, Storage and Localization

| File | Purpose |
|---|---|
| `UserInterface/ToastService.cs`, `Toast.cs`, `ToastKind.cs` | Success and error notifications |
| `UserInterface/BrowserNotifier.cs`, `NotificationPermission.cs` | Browser notification for new messages awaiting a reply |
| `UserInterface/ThemeService.cs`, `AppTheme.cs` | Dark and light theme |
| `UserInterface/DialogService.cs` | Requests to open the shared dialogs |
| `Storage/JsonUserStore.cs` | JSON file per user in `App_Data`, generic over the file shape |
| `Storage/ICurrentUser.cs`, `LocalCurrentUser.cs` | Identity of the user, a constant in this POC |
| `Localization/Localizer.cs` | Language, translation lookup, plurals, dates |
| `Localization/LocalizedComponentBase.cs` | Base class of components, with `L()` and `Plural()` |
| `Localization/Strings.cs`, `Strings.En.cs`, `Strings.Fr.cs` | Translation tables, same keys in both languages |
| `Localization/ErrorText.cs` | Translated text of an exception, without credential |
| `Localization/UserMessageException.cs` | Business error carrying a translation key |

### Components

| Folder | Purpose |
|---|---|
| `Layout/` | `MainLayout`, `SignInLayout`, `AppRail`, `AppHeader`, `QueueSidebar`, `ReconnectModal` |
| `Pages/` | `SignInPage`, `InboxPage`, `CatalogPage`, `ProfilePage`, `ErrorPage`, `NotFoundPage` |
| `Shared/` | Reusable components: dialogs, message list and detail, chart, icons, toggles |

### Design principles

**One session per Blazor circuit.** `IbmiSession` holds the connection, raises `Changed` and closes the connection when disposed.

**One path to the IBM i.** Every call goes through `IbmiSession.RunAsync`, which runs it off the render thread and one at a time, because an NTi connection does not support two simultaneous calls.

**`EnsureOpen()` at the start of each system call**, because NTi 5 throws if `Open()` is called on an open connection.

**Errors come back as message ids.** The error code parameter of each API is a binary structure with "bytes provided" set to 16, so the API returns the message id (`CPF...`) instead of escalating an exception.

**User spaces are temporary.** They live in `QTEMP` under names specific to the product and are deleted after use. A list that does not fit is read again with a larger user space.

**Display text is built at render time.** Formatters receive the raw values of the system and the language service, so a language switch never shows stale text.

## 9. Extending the project

The project is designed to receive new building blocks. The usual pattern to add a feature that queries the IBM i:

1. **System call**: add a static class in `SystemApis/` with an extension method on `NTiConnection` (see `UserInfoApi.cs` as a model). Start with `conn.EnsureOpen()`, then call the API with `CallProgram` and check the error structure with `ApiErrorCode.ThrowIfError`.
2. **Type**: if the call returns data, add a class next to the API in `SystemApis/`, in its own file.
3. **Facade**: expose the call in `Messages/MessageQueueService.cs`, through `connection.RunAsync(...)`, so that the interface never accesses the connection directly.
4. **Interface**: create a page or a component in `Components/`. Components inherit from `LocalizedComponentBase`: use `L("key")` for every visible text.
5. **Texts**: add the keys to `Strings.En.cs` and `Strings.Fr.cs`. Use `key.one` and `key.other` for plurals.

For connection keywords, parameter types and examples of calls to programs and system APIs, refer to the [NTi documentation](https://www.documentation.aumerial.com/) (in particular the "Call a system API" tutorial).

## 10. Troubleshooting

| Symptom | Likely cause | Lead |
|---|---|---|
| Sign-in fails with a connection error | TCP services stopped, port blocked or wrong host name | Check `STRTCPSVR` (step 2) and the firewalls on 8471, 8475 and 8476 |
| Sign-in fails with a license message | NTi license missing, expired or tied to another partition | Check the license in the `KNTI` library, renew it from the customer portal |
| Sign-in fails with the SSL/TLS option | No certificate configured for the host servers, or ports 94xx blocked | Configure DCM on the IBM i and open 9471, 9475 and 9476, or untick the option on a trusted network |
| Password rejected | Profile disabled, password expired or wrong | Check the profile with `DSPUSRPRF` |
| `CPF2433` on a queue such as `QHST` | The listing API refuses some system queues | Use `DSPLOG` on a 5250 screen |
| `CPF2451` when deleting a message | The queue is allocated to an interactive job (`*BREAK` or `*NOTIFY`) | Check the delivery method of the queue, or sign out of the interactive session |
| `CPF9810` when searching queues | The library does not exist | Check the library field of the sign-on form or of the left column |
| No queue found | Empty library or no authority on it | Try `*LIBL` or another library, check the rights of the profile |
| "Not connected" on a page | The session was lost (restart, long idle) | Sign in again |
| `dotnet build` fails on the NuGet package | No access to NuGet | Configure access to `nuget.org` or to an internal feed |
| `dotnet --version` does not show 10.x | .NET 10 SDK missing | Install the SDK from <https://dotnet.microsoft.com/download> |
| Texts show raw keys such as `inbox.refresh` | A key is missing from a dictionary | Add it to `Strings.En.cs` and `Strings.Fr.cs` |
| Message texts are in the wrong language | They come from the IBM i job language | Check the language settings of the profile on the system |

## 11. Known limitations

- **Most recent messages only**: a queue is read up to its 300 most recent messages. The count and the activity chart say so when the queue holds more.
- **Some system queues cannot be listed**: the listing API refuses queues such as `QHST` (see [Troubleshooting](#10-troubleshooting)).
- **Reading is periodic**: the open queue is read again every 30 seconds, not in real time.
- **Deleting many messages**: the system API removes one message per call, so deleting a large selection makes one call per message.
- **One recipient for an `*INQ` message**, and at most fifty queues for the other types.
- **A reply leaves the message in the queue**, marked as answered. The application does not remove it.
- **No application authentication** and no per-user data partitioning (see [Security](#7-security)).
- **Mixed languages on screen**: texts returned by the IBM i follow the language of the IBM i job, not the language chosen in the interface (see [Language](#5-language)).

## 12. Support and terms of use

This project is a demonstration provided by Aumerial. Its terms of use are those communicated with the NTi trial license, completed by the disclaimer at the top of this document.

The source code of this project is published under the [MIT license](LICENSE). The `Aumerial.Data.Nti` package and the NTi license key are not covered by it and remain subject to their own terms.

- NTi documentation: <https://www.documentation.aumerial.com/>
- Customer portal and licenses: <https://portal.aumerial.com/>
- NuGet package: <https://www.nuget.org/packages/Aumerial.Data.Nti>
- Contact: <contact@aumerial.com>
