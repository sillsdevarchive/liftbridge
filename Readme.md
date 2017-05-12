# Overview

A plugin for FieldWorks Language Explorer which permits sharing of LIFT dictionary
data with remote colleagues using FLEx or WeSay.

### Status

(Last revision of this page: 12 April 2013)

Lift Bridge (Windows) is available for download. This version can only be installed
on FLEx versions 7.2.4 -> 7.2.7.

Lift Bridge is now in 'maintenance mode' as new versions of it will be provided as
crucial bug foxes are found and fixed in Lift Bridge or in code it uses. No new
capability is expected for Lift Bridge that works with the Flex 7.2.4-7 series. There
will also not be a Linux version of Lift Bridge, as there will not be a Linux version
of the FLEx 7.2.x series. The Linux team jumped from FW 7.0.6 to the FW 7.3 series
and skipped the FW 7.1 and FW 7.2 versions.

While there won't be new development for Lift Bridge in this edition, Lift Bridge
will definitely continue under development, but in a different way. The Lift Bridge
code base has been merged with the FLEx Bridge code base. Access to the functionality
of both bridges is be available in the FLEx 8.0 series.

Some documentation is available, but it is not complete.

### Caveats

Lift Bridge works best with FieldWorks 7.2.7 (available here).

### Set up a project on the Language Depot server

- Register a user account.
- Request that a new project be added to the Language Depot server by sending an
  email to admin@languagedepot.org with the following information:

I would like to set up an additional project for &lt;Some Language>.

- My username: myusername
- Project name: &lt;XYZ> Dictionary
- ISO-639 code: &lt;MyCode>

## Developers

### Mailing List

Sign up here: <https://groups.google.com/forum/#!forum/liftbridgedev>

### Source Code

Lift Bridge is written in C#. The UI widgets uses Windows Forms from the Chorus
library.

To get the source code, you'll need Mercurial. Windows users, install TortoiseHg.
Then from a command line, go to where you keep your development projects, and give
this command:

    hg clone <http://hg.palaso.org/liftbridge>

You should now have a solution which you can build using any 2010 edition of Visual
Studio 2010, including the free Express version.

### Getting up-to-date libraries

Some of the dependencies are large, and others are updated frequently. For both of
those reasons, you can't just pull the code and expect it to compile. First, you will
have to do some extra work to get Lift Bridge's library dependencies up to date.

### 1) Palaso Libraries

Get binaries of Palaso libraries from
<http://build.palaso.org/repository/downloadAll/bt32/.lastSuccessful/artifacts.zip>.
This is really the latest, so don't be disheartened if there's some API change which
Lift Bridge hasn't updated to yet. We keep them in sync generally, but a few times a
year they may be out of sync by a day or so.

Using the contents of the zip, update the palaso libraries in lib/Debug and
lib/Release.

### 2) Chorus Libraries

Get binaries of Chorus libraries from
<http://build.palaso.org/repository/downloadAll/bt2/14146:id/artifacts.zip>. This is
really the latest, so don't be disheartened if there's some API change which Lift
Bridge hasn't updated to yet. We keep them in sync generally, but a few times a year
they may be out of sync by a day or so.

Using the contents of the zip, update the Chorus libraries in lib/Debug and
lib/Release.

- DownloadsUrl: <http://downloads.palaso.org/LiftBridge/>

