ConfigMocker
============

Allows to have machine specific connection strings and app settings.

NOTE: only web projects supported for now.

Usage
-----

 1. Create a "Web.config" variation with machine name in extension, eg "Web.PC-DEV.config".
 2. Add your machine specific connection strings or app settings to that file, using standard .NET configuration elements.
 3. Call `new ConfigMocker.ConfigMocker.Mock();` during app init - it will replace default configuration with machine specific ones in runtime, if alternative config file was found for this machine.

NOTE: "configSource" attribute is also supported. Just create new variation for config file specified in the attribute value. For example, for this line in the "Web.config":

    <connectionStrings configSource="App_Config\ConnectionStrings.config" />
    
create a variation in "App_Config" folder with the name "ConnectionStrings.%PC-NAME%.config".
