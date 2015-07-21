# elmah.io.nlog
Log to elmah.io from Nlog.

## Installation
elmah.io.nlog installs through NuGet:

```
PS> Install-Package elmah.io.nlog
```

Add the elmah.io target to your app.config, web.config or NLog.config:

```xml
<extensions>
  <add assembly="Elmah.Io.NLog"/>
</extensions>

<targets>
  <target name="elmahio" type="elmah.io" logId="cc6043e9-5d7b-4986-8056-cb76d4d52e5e"/>
</targets>

<rules>
  <logger name="*" minlevel="Info" writeTo="elmahio" />
</rules>
```

In the example we specify the level minimum as INFO. This tells NLog to log only information, warning, error and fatal messages. You may adjust this but be aware, that your elmah.io log may run full pretty fast, if you log thousands and thousands of trace and debug messages.

## Usage
Log messages to elmah.io, just as with every other target and NLog:

```c#
log.Warn("This is a warning message");
log.Error(new Exception(), "This is an error message");
```
