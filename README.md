# We've moved!

Find the latest source code at our new home:<br/>
https://dev.azure.com/lecord/Public/_git/Cornerstone_RemoteControlClient

---

The source code provided in this project demonstrates how to connect to a LECO© Cornerstone© instrument using the purchased remote control options of the instrument's copy protection key.

To publish from source code, run:

```
dotnet publish -r win-x86 -p:PublishSingleFile=true --self-contained true -o publish -c Release
```