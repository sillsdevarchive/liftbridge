mkdir ..\output
mkdir ..\output\common

ILMerge /lib:..\lib\Release\ /lib:..\lib\common\ /target:library /out:..\output\common\ChorusPlus.dll Chorus.exe LibChorus.dll Autofac.dll Palaso.dll PalasoUIWindowsForms.dll icu.net.dll Enchant.Net.dll Interop.WIA.dll Keyman7Interop.dll KeymanLink.dll
ILMerge /lib:..\lib\Release\ /lib:..\lib\common\ /out:..\output\common\ChorusMerge.exe ChorusMerge.exe LibChorus.dll Autofac.dll Palaso.dll PalasoUIWindowsForms.dll icu.net.dll Enchant.Net.dll Interop.WIA.dll Keyman7Interop.dll KeymanLink.dll