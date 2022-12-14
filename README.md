<p align="center">
<img src="https://raw.githubusercontent.com/AnemonOFF/BMLogger/main/logo.png" alt="BMLogger">
<h1 align="center">BMLogger</h1>
</p>
<p align="center">
<a href="https://www.nuget.org/packages/BMLogger/"><img alt="Nuget" src="https://img.shields.io/nuget/v/BMLogger"></a>
<a href="https://github.com/AnemonOFF/BMLogger/blob/main/LICENSE"><img alt="GitHub" src="https://img.shields.io/github/license/AnemonOFF/BMLogger"></a>
</p>
</br>

A simple C# file and console **logger** with log level separate.

## BMLogger 2 UPDATE
1. **Added flexable console log level setting**
- **LogAll** - log all messages in console
- **LogWarnsAndHigher** - log only WARN, ERROR and FATAL messages in console
- **LogErrorsAndHigher** - log only ERROR and FATAL messages in console
- **LogFatals** - log only FATAL messages in console
- **LogSuccess** - log only SUCCESS messages in console
- **LogSuccessAndInfos** - log only SUCCESS and INFO messages in console
- **LogSuccessWarnsAndHigher** - log only SUCCESS, WARN, ERROR and FATAL messages in console
- **LogSuccessErrorsAndHigher** - log only SUCCESS, ERROR, FATAL messages in console
- **LogSuccessFatals** - log only SUCCESS and FATALS messages in console
- **LogNothing** - do not log anything in console

2. **By default provider creating "default" logger on init**
You can get it by provider.GetLogger("default") or provider["default"].

## How to use

 1. Firstly **create log provider**
```c#
var provider = new BMLoggerProvider();
```
| Agrument        | type        | required | default  | description                                      |
|-----------------|-------------|----------|----------|--------------------------------------------------|
| dir             | string      | -        | bmlogger | path to directory of logs                        |
| expiration      | TimeSpan    | -        | 30 days  | time from last log in file. If greater - delete  |
| maxFileSizeInMb | int         | -        | 20 Mb    | max file size on provider init. If greater - cut |
| logInConsole    | ConsoleLogs | -        | LogAll   | which messages should be logged in console       |
 2.  Create logger by provider. **DO NOT** create it by yourself (using new BMLogger()).
 
```c#
var logger = provider.CreateLogger("name");
````
| Agrument        | type         | required | default          | description                                                    |
|-----------------|--------------|----------|------------------|----------------------------------------------------------------|
| name            | string       | +        |                  | name of logger and its file                                    |
| logDateTime     | bool         | -        | true             | flag to log date and time of message                           |
| logCallerPath   | bool         | -        | true             | flag to log path to file from where log function was called    |
| logCallerMember | bool         | -        | true             | flag to log method and line from where log function was called |
| logInConsole    | ConsoleLogs? | -        | Provider`s value | which messages should be logged in console                     |
3. **Log** your message
```c#
// Use default method with any LogLevel
logger.Log("any log level message", LogLevel.INFO);
await logger.LogAsync("async any log level message", LogLevel.INFO);
// Or you can use short cuts for LogLevels
logger.Info("info message");
await logger.InfoAsync("async info message");
logger.Warn("warn message");
await logger.WarnAsync("async warn message");
logger.Error("error message");
await logger.ErrorAsync("async error message");
logger.Fatal("fatal message");
await logger.FatalAsync("async fatal message");
logger.Success("success message");
await logger.SuccessAsync("async success message");
```
## Log levels

 - **INFO** - default log level
 - **SUCCESS** - success message with green color in console
 - **WARN** - warning message with orange color in console
 - **ERROR** - error message with red color in console
 - **FATAL** - fatal error message with red background color in console

## Asynchronous
All log methods have async analogues.
File operations are **thread-safe** by lock constructions.

**BUT!** When you use async methods be careful with third party console output, because colored message can not be in time to reset console color scheme to default.

## License
[License](https://github.com/AnemonOFF/BMLogger/blob/main/LICENSE)
