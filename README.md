# KCWinService

This project is a small wrapper around KeyCloak to run it on Windows as a Windows Service. 

This project tries to handle the automatic startup and shutdown of the KeyCloak server. Because 
there is a termination prompt when shutting down the KC server, shutdown procedure is looking for 
java.exe process and kills it. If there are other Java applications running on the computer, you 
need to make sure the right one is killed when this service is shutting down.

Any improvements are welcome.
