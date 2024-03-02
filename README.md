jsisie
======

A .NET parser for SIE files that can read files of version 1 to 4 (including 4i)

Follow [this link](https://sie.se/vadsie/) for swedish versions and [this link](https://sie.se/in-english/) for english versions of the SIE specifications.

**Read a SIE file**

To read a file create an instance of SieDocument and call ReadDocument.

There are some properties on SieDocument that changes how the parsing works:

+ AllowUnbalancedVoucher: If true the parser will allow vouchers that do not sum to zero.
+ IgnoreMissingOMFATTNING: If true the parser will not flag a missing #OMFATTN as an error.
+ IgnoreBTRANS: If true #BTRANS (removed voucher rows) will be ignored.
+ IgnoreRTRANS: If true #RTRANS (added voucher rows) will be ignored.
+ AllowMissingDate: If true some errors for missing dates will be ignored. (same as IgnoreMissingDate)
+ StreamValues: If true don't store values internally. The user has to use the Callback class to get the values. Usefull for large files.
+ ThrowErrors: If false then cache all Exceptions in SieDocument.ValidationExceptions
+ DateFormat: The standard says yyyyMMdd and parser will default to that, but you can change the format to whatever you want.
+ Encoding: The standard says codepage 437, but that is not supported by dotnet core. You can change it to whatever you want. It will default to codepage 437 when running in Dotnet Framework and 28591 (ISO-8859-1) when running dotnet core. Please note that #KSUMMA will not be caclutated if you choose a multibyte encoding.

**Not all features are implemented yet:**

+ #UNDERDIM: There are no instances of this in the published example files.

**Write a SIE file**

To write a file create an instance of SieDocumentWriter and call WriteDocument
The WriteOptions class contains properties to set DateFormat and Encoding.

**Compare SIE files**

To compare SIE files call the static method Compare on SieDocumentComparer. It will return a List&lt;string&gt; with differences between the files.  


Even if you use this parser you should get familiar with the file specification.
I have not made any extensive efforts to validate the resulting data against the files so please do your validation and tell me if you find any errors.


To install this package via nuget write

	Install-Package jsisie

in the nuget console in Visual Studio 


**Java**

There's a Java version of this parser here: https://github.com/perNyfelt/SIEParser  


**Change log**
+ 2023-03-02	Added AllowUnbalancedVoucher
+ 2021-07-10	Add all test files I got from the SIE organisation. And make the test program use them.
+ 2021-07-10	Fix bug that wrote #TRANS  instead of #BTRANS and #RTRANS
+ 2021-07-10	Add DateFormat, Encoding and IgnoreMIssingDate. Use ISO-8859-1 on dotnet core.
+ 2020-07-05	Now supports writing KSUMMA
+ 2017-12-22	Add #PROGRAM to the document comparer. Enable the test program to compare two files via command line.


