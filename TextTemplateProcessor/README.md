# Text Template Processor
<!--TOC-->
  - [Overview](#overview)
  - [Public Classes](#public-classes)
  - [Public Interfaces](#public-interfaces)
  - [Internal Classes](#internal-classes)
  - [Template Files](#template-files)
    - [Introduction](#introduction)
    - [Template File Layout](#template-file-layout)
    - [Template Control Codes](#template-control-codes)
    - [Comment Lines](#comment-lines)
    - [Segment Header Lines](#segment-header-lines)
    - [Segment Options](#segment-options)
    - [Text Lines](#text-lines)
    - [Tokens](#tokens)
    - [Segment and Token Names](#segment-and-token-names)
    - [Sample Text Template File](#sample-text-template-file)
  - [***TextTemplateProcessor*** Class](#texttemplateprocessor-class)
    - [Overview](#overview)
    - [Constructors](#constructors)
    - [Properties](#properties)
      - [***CurrentIndent***](#currentindent)
      - [***CurrentSegment***](#currentsegment)
      - [***GeneratedText***](#generatedtext)
      - [***IsOutputFileWritten***](#isoutputfilewritten)
      - [***IsTemplateLoaded***](#istemplateloaded)
      - [***LineNumber***](#linenumber)
      - [***TabSize***](#tabsize)
      - [***TemplateFileName***](#templatefilename)
      - [***TemplateFilePath***](#templatefilepath)
    - [Methods](#methods)
      - [***GenerateSegment***](#generatesegment)
      - [***GetMessages***](#getmessages)
      - [***LoadTemplate***](#loadtemplate)
      - [***ResetAll***](#resetall)
      - [***ResetGeneratedText***](#resetgeneratedtext)
      - [***ResetSegment***](#resetsegment)
      - [***ResetTokenDelimiters***](#resettokendelimiters)
      - [***SetTabSize***](#settabsize)
      - [***SetTemplateFilePath***](#settemplatefilepath)
      - [***SetTokenDelimiters***](#settokendelimiters)
      - [***WriteGeneratedTextToFile***](#writegeneratedtexttofile)
  - [***TextTemplateConsoleBase*** Class](#texttemplateconsolebase-class)
    - [Overview](#overview)
    - [Constructors](#constructors)
    - [Properties](#properties)
      - [***OutputDirectory*** Property](#outputdirectory-property)
      - [***SolutionDirectory*** Property](#solutiondirectory-property)
    - [Methods](#methods)
      - [***ClearOutputDirectory*** Method](#clearoutputdirectory-method)
      - [***LoadTemplate*** Method](#loadtemplate-method)
      - [***PromptUserForInput*** Method](#promptuserforinput-method)
      - [***SetOutputDirectory*** Method](#setoutputdirectory-method)
      - [***WriteGeneratedTextToFile*** Method](#writegeneratedtexttofile-method)
<!--/TOC-->
## Overview
***Text Template Processor*** is a class library written in C# 10 and .NET 6 that allows you to easily create console applications for processing
text template files and generating new text files from the templates. The library implements the following features:

- Control of the indentation of each generated text line (see the section covering [Text Lines](#text-lines).)
- Support for named tokens that act as placeholders for generated text (see the section covering [Tokens](#tokens).)
- Template files are made up of named blocks of text lines (called *Segments*) that are logical units that can be pieced together in any order to
  generate the final output file (see the section on [Segment Header Lines](#segment-header-lines).)
- A special *Segment* called a *Pad Segment* can be defined to be automatically placed between consecutive occurrences of the same named *Segment*
- Special handling of the indentation of the first line of a *Segment* the first time that *Segment* is processed (called a "first time indent",
  this and the *Pad Segment* are covered in the section on [Segment Options](#segment-options).)
- Extensive validation of the text template file to ensure it is valid and usable
- Detailed informational, warning, and error messages (messages are displayed in a *Console* window)

These features will be described in much more detail in the sections that follow.

## Public Classes
The ***Text Template Processor*** class library contains two public classes that can be used for creating your specific text template processor
application.

- ***TextTemplateProcessor*** - This is the main class in this library. It contains all the functionality needed for processing text template
  files and generating new files from the template. Although you can use this class directly, you will likely want to use the second class
  described next. (see [***TextTemplateProcessor***](#texttemplateprocessor-class) class later in this document)
- ***TextTemplateConsoleBase*** - This is an abstract base class that derives from the ***TextTemplateProcessor*** class. It provides some
  additional functionality, such as writing log messages to the *Console*, among others. This is the class from which you would normally
  derive your custom text template processor class. (see [***TextTemplateConsoleBase***](#texttemplateconsolebase-class) class later in this
  document)

## Public Interfaces
The only public interface supplied by the ***Text Template Processor*** class library is the ***ITextTemplateProcessor*** interface. This
interface is implemented by the ***TextTemplateProcessor*** class. The properties and methods defined in this interface are the same
properties and methods that are covered in detail in the section dealing with the [***TextTemplateProcessor***](#texttemplateprocessor-class) class.

## Internal Classes
The following classes are defined as internal to the class library. They provide the functionality to support the two public classes described above.
You normally would never need to access any of these classes directly in your code. They are just listed here to give you an idea of what went into
the design of this class library.

- ***FileAndDirectoryService*** - This class handles all I/O for the ***Text Template Processor*** class library, including creating
  directories, reading, writing, and deleting text files, etc.
- ***PathValidater*** - This class is used for validating file and directory path strings to ensure they're valid.
- ***TextReader*** - This class handles the reading of text files. (The actual reading is performed by the
  ***FileAndDirectoryService*** class.)
- ***TextWriter*** - This class handles the writing of text files (the actual writing is performed by the
  ***FileAndDirectoryService*** class.)
- ***ConsoleLogger*** - All informational, warning, and error messages generated by the ***Text Template Processor*** class library are
  routed through this class.
- ***ConsoleReader*** - Displays a prompt on the *Console* and then retrieves the user response to that prompt.
- ***ConsoleWriter*** - Writes text lines to the *Console*.
- ***MessageWriter*** - Formats messages and then calls the ***ConsoleWriter*** class to write the messages to the *Console*.
- ***Locater*** - This class is shared by many of the other classes described here. It is used for keeping track of the current location in the
  text template file that is being processed.
- ***TemplateLoader*** - This class manages the reading, parsing, and validation of the text template file.
- ***SegmentHeaderParser*** - Parses and validates segment header lines in the text template file and saves the segment names and control
  information.
- ***TextLineParser*** - Parses and validates text lines in the text template file and saves the indent control information and text for each line.
- ***TokenProcessor*** - Parses and validates the tokens contained on the text lines in the text template file and saves the token names. This class
  is also called to replace tokens with generated text in the generated output file.
- ***NameValidater*** - Validates *Segment* names and token names to ensure that they are valid.
- ***DefaultSegmentNameGenerator*** - Used for generating default names for *Segments* whose names are either missing or invalid in the
  text template file.
- ***IndentProcessor*** - Maintains the proper indentation of generated text lines based on the control information that was
  retrieved from the segment headers and the indent control information found on the text lines of the text template file.
- ***ServiceLocater*** - This class implements a basic *Inversion of Control* container used for resolving class dependencies in the
  ***Text Template Processor*** class library. It uses the [***BasicIoC***](https://github.com/Dave031387/BasicIoC) package.

## Template Files
### Introduction
A template file is a specially formatted text file that can be used as the basis for generating other text files. In the case of the ***Text
Template Processor*** class library, the template files are made up of one or more *Segments*. Each *Segment* begins with a segment header line
followed by one or more text lines. A text line can contain zero or more named token placeholders. Each named token is replaced with the
appropriate text value when the output file is being generated.

### Template File Layout
Each line in a template file must adhere to the following pattern:

```
C C C   R R R ...
1 2 3 4 5 6 7 ...
```

The first three character positions of each line (the "CCC") must contain a valid text template control code
(see [Template Control Codes](#template-control-codes), below). The fourth character position must be blank. The remainder of the line
(the "RRR...") depends on the control code. There are three possibilities:

- For comment lines, the remainder of the line contains the comment. (see [Comment Lines](#comment-lines))
- For segment header lines, the remainder contains the segment name (must start in column 5) followed by zero or more optional segment options
  (see [Segment Header Lines](#segment-header-lines) and [Segment Options](#segment-options)).
- For text lines, the remainder contains the tokenized text that will be used to generate the output files. The fifth character position on the
  text line is the first character position of the generated text line, and so on. (see [Text Lines](#text-lines) and [Tokens](#tokens))

### Template Control Codes
As mentioned above, each line in a text template file must start with a 3-character control code located in the first three character positions
of the line. (There is one exception, described later.) The first two control codes are these:

- ***///*** - This control code (three slashes) starts a comment line.
- ***\###*** - This control code (three hash symbols) starts a segment header line.

Each text line begins with an tab control code which affects the indentation of the generated text line. The code is made up of three characters:

- The first character must be "@" (normal tab) or the letter "O" (one-time tab). This character defines whether the specified tab amount on this
  line carries over to the next line (normal tab) or if it only applies to the current line (one-time tab).
- The second character must be one of the characters "-" (reduce the line indentation), "+" (increase the line indentation), or "=" (specify the
  exact tab amount). The first two are called "relative" since they are taken relative to the current indent amount. The third one is called
  "absolute" since it is always taken from the left margin.
- The last character must be a number between 0 and 9. This specifies the number of tab positions used in calculating the new indent amount.

Often you may want the indentation of one text line to remain unchanged from the previous text line. In this case you could use the control codes
"@+0" or "@-0", which say to tab 0 from the right or left of the current indent position, effectively leaving the indentation unchanged from
the previous line. Since many, if not most, of the text lines in a typical template file would probably be of this type, the ***Text Template
Processor*** class library allows you to leave the control code off these lines. Whenever it comes across a text line having nothing in the first
three character positions, it assumes that the text line is to be indented the same amount as the previous text line.

> [!IMPORTANT]
> *This is the only instance in which it is okay to omit the control code. All other lines in the text template file must have a valid control
> code in the first three character positions of the line.*

### Comment Lines
Template lines that begin with "///" are considered to be comment lines. These lines can appear anywhere in a text template file. These lines are
essentially ignored by the ***Text Template Processor***. As such, comment lines serve no purpose other than as documentation for the user of the
text template file.

The following example shows a valid comment line.

```
/// This is a comment.
```

> [!CAUTION]
> *Comments are only allowed on lines that start with the comment control code (the three slashes). There is no option, neither does it make
> sense, to add a comment to the end of a segment header line or text line. Doing so will either result in errors when the text template file
> is parsed, or unexpected output when the generated text file is generated.*

### Segment Header Lines
Template lines that begin with "###" are considered to be segment header lines. The segment header denotes the beginning of a *Segment* in the
text template file and it must be followed by one or more text lines that make up that segment. The segment header must adhere to the following
structure:

```
### SegmentName [zero or more segment options]
123456789...
```

As already mentioned, the three hash characters must appear in the first three character positions, and the fourth character position must be
blank. The segment name must start in the fifth character position. Together, these constitute the minimum requirements of a segment header line.

Optionally, you can include one or more segment options after the segment name. At least one space must appear between the segment name and
the first segment option. Subsequent segment options must be separated from each other by one or more spaces and/or commas.

### Segment Options

The format of a segment option must adhere to the following format:

```
OptionName=OptionValue
```

The option name must be followed immediately by an equals sign with no intervening spaces, and then the value assigned to that option must
immediately follow the equals sign, again with no intervening spaces.

There are three segment options available in the current version of the ***Text Template Processor*** class library. These are:

- **TAB** - This option changes the tab size value. The tab size gives the number of space characters that make up a single indent. For example,
  if the tab size is set to 3 and the next text line to be processed specifies an indent value of 2, then that text line will be indented 6 spaces
  (3x2) from the current indent position. The value assigned to the **TAB** option must be a number between 1 and 9.
- **FTI** - This is the *First Time Indent* option. A *Segment* may be processed multiple times during the processing of a template file. The
  *First Time Indent* option overrides the tab control code of the first text line of the segment the first time that the segment is
  processed. The value assigned to this option must be a number between -9 and -1 (left tab), or between 1 and 9 (right tab). This is
  always treated as a "normal" tab, not a "one-time" tab.
- **PAD** - When a particular *Segment* is processed two or more times consecutively, sometimes it's nice to be able to insert padding
  (typically one or more blank lines) between each occurrence of the *Segment* in the output file. The **PAD** option allows you to do that.
  The value assigned to this option must be the name of a segment that appears earlier in the text template file. This segment is referred to
  as the *Pad Segment*. The text lines in the *Pad Segment* will automatically be inserted just before the text lines of the *Segment* on the
  second and subsequent times that the *Segment* is processed.

Segment options can appear in any order on a segment header line. The following example shows a segment header line which makes use of all
three segment options. (Assumes a *Segment* named *PadSegment* has been defined earlier in the text template file.)

```
### Segment1 TAB=3, FTI=1, PAD=PadSegment
```
<br>

> [!CAUTION]
> *Assigning 0 (zero) to the **FTI** option effectively disables the First Time Indent processing for that segment. In this case the **FTI**
> option will be ignored and the first text line of the segment will always be indented according to the tab control code on that text line.*

> [!NOTE]
> *A given segment option must appear only once on a segment header line. If an option appears more than once, only the first occurrence of
> that option will be used. All other occurrences will be ignored.*

> [!NOTE]
> *If a segment specifies the **PAD** option then that segment can't itself be specified as a pad segment for some other segment. In other
> words, nesting of padded segments is not allowed.*

### Text Lines
Text lines define the text that gets written to the generated output file. The text on a text line will be written verbatim to the output file
after applying the appropriate indentation and after replacing all tokens with their corresponding token values. ([Tokens](#tokens) will be covered
later in this document.) The amount of indentation depends on the current indent amount carried forward from the previous text line and the indent
amount specified in the control code of the current text line.

> [!NOTE]
> *The current indent amount always gives the number of space characters to be inserted at the start of a generated text line. This is
> different than the indent amount specified in the control code of a text line, which gives the number of tabs.*

The current indent amount always starts at 0 (zero) at the beginning of the text template process. It may or may not get adjusted when a text
line is processed, depending on the nature of the tab control code at the start of the text line. One-time tab control codes (with the
letter "O" in the first character position) never alter the current indent amount. The current indent amount also remains unchanged if the
tab control code is three spaces.

Normal tab control codes (with "@" in the first character position) will affect the current indent as explained below. (The current tab size
value in each of the following examples is assumed to be 2 and the current indent amount is assumed to be 6.)

- **@+1** - This is a normal right tab control code with a tab value of 1. When this code is processed the current indent amount will be
  increased from 6 to 8 spaces (adding 1 tab to the current indent amount).
- **@-2** - This is a normal left tab control code with a tab value of 2. When this code is processed the current indent amount will be
  decreased from 6 to 2 spaces (removing 2 tabs from the current indent amount).
- **@=2** - This is a normal absolute tab control code with a tab value of 2. Two tabs contain a total of 4 spaces (tab value of 2
  multiplied by the current tab size value of 2). The current indent amount will therefore be set to 4 spaces.

> [!NOTE]
> *The current indent amount can never be less than zero. If a normal left tab control code contains an tab value that would cause
> the current indent amount to go negative, the current indent amount will be set to zero instead.*

The following table shows examples of the various tab control codes along with the impact on the current indent position and the generated
text line. The current indent amount starts at zero with the first text line and is adjusted by the tab control code of each subsequent text line.
The tab size value is assumed to be 2 spaces. The current indent, text line indent, and new current indent columns are all in terms of the number
of spaces. The tab value column is in terms of the number of tabs.

```
|             |         |        |             | NEW     |                |
| TEXT LINE   | CURRENT | TAB    | TEXT LINE   | CURRENT | GENERATED TEXT |
| 1234567890  | INDENT  | VALUE  | INDENT      | INDENT  | 12345678901234 |
| ----------- | ------- | ------ | ----------- | ------- | -------------- |
| @+1 Line 1  |    0    |   +1   | 0+(1x2) = 2 |    2    |   Line 1       |
| O+1 Line 2  |    2    |   +1   | 2+(1x2) = 4 |    2    |     Line 2     |
|     Line 3  |    2    |    0   | 2+(0x2) = 2 |    2    |   Line 3       |
| @-3 Line 4  |    2    |   -3   | 2-(3x2) ->0 |    0    | Line 4         |
| O=3 Line 5  |    0    |   =3   |   (3x2) = 6 |    0    |       Line 5   |
| @+2 Line 6  |    0    |   +2   | 0+(2x2) = 4 |    4    |     Line 6     |
| O-1 Line 7  |    4    |   -1   | 4-(1x2) = 2 |    4    |   Line 7       |
| @-1 Line 8  |    4    |   -1   | 4-(1x2) = 2 |    2    |   Line 8       |
| @=2 Line 9  |    2    |   =2   |   (2x2) = 4 |    4    |     Line 9     |
| O-3 Line 10 |    4    |   -3   | 4-(3*2) ->0 |    4    | Line 10        |
```

In the above table, notice that only lines having tab control codes beginning with "@" impact the current indent value. Also notice that in the fourth
line the text line indent and new current indent values get set to zero because the calculated value (2-(3x2)) would have been negative. The text line
indent on the tenth line is zero for the same reason, although the current indent amount isn't affected since it is a one-time tab control code.

### Tokens
A token is a named placeholder on a text line. A text line may contain one or more tokens, or no tokens at all. Any tokens on a text line will be
replaced with appropriate text values before the generated text line is written to the output file.

A token must begin with the characters "<#" and end with "#>". The token name appears between these two delimiters. Zero or more spaces can appear
between the start delimiter and the token name, and between the token name and the end delimiter. Optionally, a "case" indicator character (either
"+" or "-") can appear immediately after the start delimiter. The following would all be valid tokens according to these rules:

```
<#Name#>
<# Name #>
<#+Name   #>
<#-    Name#>
```

The optional case indicator character can be used to alter the case of the first character of the token's text value that gets substituted for the token
before the text line is written to the output file. The "+" character causes the first character of the token's text value to be converted to uppercase.
The "-" character causes the first character to be converted to lowercase. Keep in mind that this only affects the first character of the token's text
value.

In the following examples assume that the text value for Token1 is "red" and the text value for Token2 is "Sal". Each line in the example shows the
original tokenized text line followed by the same line after the tokens have been replaced by their corresponding text values. Only the text portion of
the text line is displayed. The tab control code is omitted to simplify things.

```
TEXT LINE                           GENERATED TEXT
----------------------------------  ------------------
<#Token2#>ly and F<#  Token1 #>     Sally and Fred
F<# Token1 #> is bo<#Token1 #>      Fred is bored
<#+Token1#>uce this <#- Token2#>e   Reduce this sale
```

In the event that you don't want the token start delimiter to be treated as a token delimiter, you must escape it with a backslash character ("\\"),
like so:

```
TEXT LINE                                  GENERATED TEXT
-----------------------------------------  ----------------------------------
The value of \<#Token1#> is "<#Token1#>".  The value of <#Token1#> is "red".
```

Notice that the backslash escape character is not written to the output file.

The ***Text Template Processor*** class library includes a public method that can be called to change the start and end delimiters and the escape
character if there should be the need to do so. (see [***SetTokenDelimiters***](#settokendelimiters))

### Segment and Token Names
Segment and token names must follow these rules:

1. The name must begin with an upper- or lowercase letter.
1. After the first character, the rest of the name can contain upper- or lowercase letters, the digits 0 through 9, and underscores.
1. The name must not contain spaces.
1. Names containing characters other than those mentioned above are flagged as invalid.
1. Names can be of any length, from one character and upwards. However, it makes sense to keep them as short as possible.

Segment and token names are validated when the text template file is loaded and parsed. Missing or invalid segment names are replaced by a default name
that starts with "*DefaultSegment*" and ends with a number that gets incremented for each default name that gets used. The first missing or invalid
segment name will be replaced with *DefaultSegment1*, the second with *DefaultSegment2*, and so on.

If a token has an invalid name, then the entire token will be escaped and treated like regular text.

### Sample Text Template File
The following is a sample text template file that demonstrates many of the features described earlier in this document. This template can be used for
generating the syntax for a C# source code file.

```
/// The following is the pad segment which will be used to
/// insert a blank line between occurrences of the PropertySegment.
### PadSegment
    
/// The following segment defines the first lines of
/// the class definition.
### Top
    namespace <#Namespace#>
    {
#+1 public class <#ClassName#>
    {
/// The following segment defines the last lines of
/// the class definition.
### Bottom
@-1 }
@-1 }
/// The following segment defines a property with a
/// backing field.
### Property FTI=1, PAD=PadSegment
    private <#PropertyType#> _<#-PropertyName#>;
    
    public <#PropertyType#> <#+PropertyName#>
    {
@+1 get => _<#-PropertyName#>;
    set => _<#-PropertyName#> = value;
@-1 }
```

Now assume you have created your own app derived from the ***TextTemplateConsoleBase*** class. In that class you perform the following actions:

1. Generate the *Top* segment with the *Namespace* token value set to "MyNamespace" and the *ClassName* token value set to "MyClass".
1. Generate the *Property* segment with the *PropertyType* token value set to "bool" and the *PropertyName* token value set to "IsChanged".
1. Generate the *Property* segment with the *PropertyType* token value set to "string" and the *PropertyName* token value set to "firstName".
1. Generate the *Property* segment with the *PropertyType* token value set to "int" and the *PropertyName* token value set to "Age".
1. Generate the *Bottom* segment.

Performing these steps will result in the following generated text file being created:

```csharp
namespace MyNamespace
{
    public class MyClass
    {
        private bool _isChanged;

        public bool IsChanged
        {
            get => _isChanged;
            set => _isChanged = value;
        }

        private string _firstName;

        public string FirstName
        {
            get => _firstName;
            set => _firstName = value;
        }

        private int _age;

        public int Age
        {
            get => _age;
            set => _age = value;
        }
    }
}
```

The **FTI** and **PAD** options play a critical role in the proper formatting of this text file. To demonstrate, if we were to remove these options from
the *Property* segment our output would look like the following, which is not at all what we want.

```csharp
namespace MyNamespace
{
    public class MyClass
    {
    private bool _isChanged;

    public bool IsChanged
    {
        get => _isChanged;
        set => _isChanged = value;
    }
    private string _firstName;

    public string FirstName
    {
        get => _firstName;
        set => _firstName = value;
    }
    private int _age;

    public int Age
    {
        get => _age;
        set => _age = value;
    }
}
}
```

There are a couple other things to note in this example:

1. The tab size value defaults to 4 spaces if no tab size is set by the **TAB** option on a segment header, or by the ***SetTabSize*** method of the
   ***TextTemplateProcessor*** class.
1. Blank text lines in a text template file can be shortened to 4 characters (the three-character tab control code, and the required space in
   the fourth character position).

## ***TextTemplateProcessor*** Class
### Overview
The ***TextTemplateProcessor*** class contains all the functionality you need to work with text template files that are structured according to the
rules given in the previous section of this document. You can either derive your custom text template processor class from the ***TextTemplateProcessor***
class, or instantiate an instance of the ***TextTemplateProcessor*** class in your custom class. Here are two examples that demonstrate both methods:

```csharp
/// Custom class derived from TextTemplateProcessor
public class MyTemplateProcessor : TextTemplateProcessor
{
    private void LoadMyTemplate()
    {
        SetTemplateFilePath(@"C:\Templates\MyTemplate.txt");
        LoadTemplate();
    }
    ...
}
```
<br>

```csharp
/// Custom class that instantiates TextTemplateProcessor
public class MyTemplateProcessor
{
    private ITextTemplateProcessor _processor = new TextTemplateProcessor(@"C:\Templates\MyTemplate.txt");

    private void LoadMyTemplate()
    {
        _processor.LoadTemplate();
    }
    ...
}
```

Note that the ***TextTemplateProcessor*** class implements the ***ITextTemplateProcessor*** interface.

The following sections will describe the properties and methods that are provided by the ***TextTemplateProcessor*** class. These same properties and
methods are also defined in the ***ITextTemplateProcessor*** interface.

### Constructors
The ***TextTemplateProcessor*** class has two constructors having the following signatures:

```csharp
public TextTemplateProcessor()

public TextTemplateProcessor(string templateFilePath)
```

The default constructor takes no arguments. It initializes all properties to their initial default values. The other constructor takes a string
argument which it uses to set up the file path of the text template file that is about to be processed. Both of these constructors invoke internal
constructors that initialize all of the internal class dependencies.

> [!NOTE]
> *The [**BasicIoC**](https://github.com/Dave031387/BasicIoC) class library is used to handle the dependency injection in the class constructors.*

### Properties
#### ***CurrentIndent***
The ***CurrentIndent*** property is used to retrieve the current indent amount. This property is initialized to 0 (zero) when the class is
instantiated. The ***CurrentIndent*** then gets updated as the indent control codes are processed in the text template file. The value of this
property will always be a positive number (or zero). See the example in the [*Text Lines*](#text-lines) section earlier in this document.

#### ***CurrentSegment***
The ***Text Template Processor*** class library keeps track of which segment it is working on. This happens in two situations. First, when the text
template file is being parsed, the name of the segment that is currently being parsed is stored in the ***CurrentSegment*** property. Then, when the
text output file is being generated, the name of the segment being generated is stored in this property.

#### ***GeneratedText***
As the text output file is being generated, each generated text line is appended to the end of the generated text buffer. The ***GeneratedText***
property returns a copy of the generated text buffer. Note that this is just a copy, and, as such, making changes to this copy will have absolutely
no impact on the generated text buffer.

#### ***IsOutputFileWritten***
This property is set to *false* when the ***TextTemplateProcessor*** class is initialized. It gets set to *true* when the contents of the generated text
buffer are written to the output file. The property will get set back to *false* on any of the following conditions:

- A *Segment* from the current text template file is processed to generate text that gets appended to the generated text buffer
- A new text template file is loaded and parsed
- The [***ResetAll***](#resetall) method is invoked

#### ***IsTemplateLoaded***
This property is set to *false* when the ***TextTemplateProcessor** class is initialized. It gets set to *true* when a valid text template file
containing one or more segments gets loaded and parsed. After that, it will only ever be set back to *false* under two conditions:

- An attempt is made to load a text template file using an invalid file path
- The [***ResetAll***](#resetall) method is invoked

#### ***LineNumber***
When a text template file is loaded and parsed, the current line number being parsed is stored in the ***LineNumber*** property.
When a *Segment* from a text template is being processed to generate the output file, the ***LineNumber*** property is set to the number of each
text line as it is processed in that *Segment*. The following example should clarify this.

```
| PARSING | GENERATING |                     |
| LINE #  | LINE #     | TEXT TEMPLATE LINES |
| ------- | ---------- | ------------------- |
|     1   |            | /// Comment line    |
|     2   |            | ### Segment1        |
|     3   |            | /// Comment line    |
|     4   |      1     | @+1 Text line       |
|     5   |            | ### Segment2        |
|     6   |      1     | @+1 Text line       |
|     7   |            | /// Comment line    |
|     8   |      2     |     Text line       |
|     9   |            | ### Segment3        |
|    10   |      1     |     Text line       |
|    11   |      2     | @+1 Text line       |
```

> [!NOTE]
> *Every line of a text template file is counted when the file is read in and parsed. During the generation of the output file, however, only text lines
> are counted. Segment header lines and comment lines are not counted. The line counter gets reset every time [***GenerateSegment***](#generatesegment)
> is called to generate another segment.*

#### ***TabSize***
The ***TabSize*** property retrieves the current tab size value. This value gives the number of spaces that make up a single tab stop. The
tab size is set to a default value of 4 when the ***TextTemplateProcessor*** class is initialized. This property only allows for retrieving the
current value. The [***SetTabSize***](#settabsize) method must be used if you want to change the tab size value.

#### ***TemplateFileName***
This property retrieves the file name of the text template file. This property is set to an empty string by the
default constructor of the ***TextTemplateProcessor*** class. It will also be set to an empty string if an invalid file path is passed into
the other constructor, or into the [***LoadTemplate***](#loadtemplate) or [***SetTemplateFilePath***](#settemplatefilepath) methods.

#### ***TemplateFilePath***
This property retrieves the full file path of the text template file. This property is set to an empty string by the
default constructor of the ***TextTemplateProcessor*** class. It will also be set to an empty string if an invalid file path is passed into
the other constructor, or into the [***LoadTemplate***](#loadtemplate) or [***SetTemplateFilePath***](#settemplatefilepath) methods.

### Methods
#### ***GenerateSegment***
The ***GenerateSegment*** method processes the text lines for the specified segment name and generates new text lines that are appended to the
generated text buffer. The method has the following signature:

```csharp
void GenerateSegment(string segmentName, Dictionary<string, string>? tokenValues = null)
```

The `segmentName` argument must specify the name of a segment in the text template file. The optional `tokenValues` argument must be a
dictionary of key/value pairs where the key is a token name and the value is the token value to be substituted for the given token name.

For example, assume the text template file contains the following segment:

```
### Segment1
    Text line <#Token1#>
@+1 Text <#Token2#> 2
```

Your custom token processor app might contain code for processing this segment that looks like this:

```csharp
public class MyTemplateProcessor
{
    private ITextTemplateProcessor _processor = new TextTemplateProcessor();

    public void GenerateSegment1()
    {
        Dictionary<string, string> tokenValues = new()
        {
            { "Token1", "1" },
            { "Token2", "line" }
        };

        _processor.GenerateSegment("Segment1", tokenValues);
    }
}
```

When you invoke your ***GenerateSegment1*** method, the following lines will be appended to the end of the generated text buffer (assuming the
current indent position is 0 to start with):

```
Text line 1
    Text line 2
```

> [!NOTE]
> *The token values that are supplied to the **GenerateSegment** method are saved by the **Text Template Processor** class library. If you later
> call **GenerateSegment** for another segment that has some of the same token names and associated values then you would need to supply token
> values only for any new token names or for previous token names that have changed values.*

Before processing any text lines, the ***GenerateSegment*** method will take care of processing any segment options that may be defined on the
specified *Segment*. Suppose we have a *Segment* named **Segment1** which specifies a pad segment named **PadSegment**, a first time indent value
of 1, and a tab size value of 2. The following steps will be taken in the order given each time ***GenerateSegment*** is called for **Segment1**:

1. The text lines for **PadSegment** will be generated and appended onto the end of the generated text buffer. (Note that this happens only on the
   second and subsequent times that ***GenerateSegment*** is called for **Segment1**.)
1. The tab size will be set to 2 spaces.
1. Each text line for **Segment1** will be processed in the order they were defined in the template file. For each text line:
   a. Determine the indent amount for the text line. (Note that the first time ***GenerateSegment*** is called for **Segment1**, the indent amount
      of the first text line is determined from the first time indent value on the segment header. All other times the indent amount is determined
      from the indent control code on the corresponding template line.)
   a. Replace all token placeholders on the text line with their respective token values.
   a. Add the required number of spaces to the beginning of the text line. The number of spaces is equal to the indent amount that was determined earlier.
   a. Append the resulting string to the end of the generated text buffer.

#### ***GetMessages***

#### ***LoadTemplate***
As the name implies, the ***LoadTemplate*** method loads a text template file into memory so that it can be processed. The contents of the template
file are validated during the loading process and any errors that are detected result in appropriate error messages being written to the console.
The names of all tokens are also determined and saved for later use.

The ***LoadTemplate*** method has two variations with the following signatures:

```csharp
void LoadTemplate()
void LoadTemplate(string filePath)
```

The first variation assumes that the template file path has already been set (either by the constructor or the
[***SetTemplateFilePath***](#settemplatefilepath) method). The second variation takes a file path string and sets the template file path to that value
(assuming it's a valid existing file path). Both variations will load the contents of the specified text template file into memory, unless it has already
been loaded previously, in which case an error message will be logged. An error message will also be logged if the specified file path is invalid.

Here is an example of the first variation:

```csharp
public class MyTemplateProcessor
{
    private ITextTemplateProcessor _processor = new TextTemplateProcessor(@"C:\Templates\MyTemplate.txt");

    void LoadMyTemplate()
    {
        _processor.LoadTemplate();
    }
}
```

And, here is an example of the second variation:

```csharp
public class MyTemplateProcessor
{
    private ITextTemplateProcessor _processor = new TextTemplateProcessor();

    void LoadMyTemplate()
    {
        _processor.LoadTemplate(@"C:\Templates\MyTemplate.txt");
    }
}
```

#### ***ResetAll***
The ***ResetAll*** method is used to reset the state of the ***TextTemplateProcessor*** class object. It has the following signature:

```csharp
void ResetAll(bool shouldDisplayMessage = true)
```

This method performs the following tasks:
- Clears the contents of the generated text buffer
- Removes the currently loaded template from memory
- Resets the current indent amount to 0
- Resets the tab size to its default value of 4
- Resets the token delimiters and escape character back to their default values (if they were changed)
- Sets the [**IsTemplateLoaded**](#istemplateloaded) and [**IsOutputFileWritten**](#isoutputfilewritten) properties to *false*

The optional `shouldDisplayMessage` parameter determines whether or not a message gets logged at the end of the reset process. The default is
*true* (a message is logged). The message can be suppressed by passing *false* into the method.

> [!NOTE]
> *You would normally only use the **ResetAll** method after you're done processing one text template file and want to process a different one.
> However, **ResetAll** is called automatically whenever a new text template file is loaded.*

#### ***ResetGeneratedText***
The ***ResetGeneratedText*** method is used to clear the generated text buffer. The method has the following signature:

```csharp
void ResetGeneratedText(bool shouldDisplayMessage = true)
```

This method performs the following tasks:
- Clears the contents of the generated text buffer
- Resets the current indent amount to 0
- Resets the tab size to its default value of 4
- Sets the [**IsOutputFileWritten**](#isoutputfilewritten) property to *false*
- Sets the [**IsFirstTime**](#isfirsttime) flag to *true* for each *Segment* in the text template file

The optional `shouldDisplayMessage` parameter determines whether or not a message gets logged at the end of the reset process. The default is
*true* (a message is logged). The message can be suppressed by passing *false* into the method.

The ***ResetGeneratedText*** method is normally called after you have written the contents of the generated text buffer to an output file. (See the
[***WriteGeneratedTextToFile***](#writegeneratedtexttofile) method.) You can then reprocess the same text template to generate another file using
different token values (for example).

> [!NOTE]
> *The [**WriteGeneratedTextToFile**](#writegeneratedtexttofile) method automatically calls the **ResetGeneratedText** method by default after the
> output file has been written.*

#### ***ResetSegment***
The ***ResetSegment*** method is used to set the [**IsFirstTime**](#isfirsttime) property back to *true* for the specified segment. It has the
following signature:

```csharp
void ResetSegment(string segmentName)
```

The `segmentName` parameter specifies the name of the segment that is to be reset.

The [**IsFirstTime**](#isfirsttime) property determines whether the **FTI** (first time indent) or **PAD** (pad segment) options are processed.
This property is initialized to *true* for each segment in the text template file when the template is loaded. It gets set to *false* for a given
segment after that segment has been generated one time (see the [***GenerateSegment***](#generatesegment) method). It then remains *false* for the
life of the application, or until the ***ResetSegment*** method is called.

The ***ResetSegment*** method would normally be called only if a given segment is processed in two or more groupings within the same generated output
file. For example, assume you're generating a class file that has a private class definition contained within the main class definition. Assume both the
containing class and the contained class have two or more properties which will be generated by a single segment named *PropertySegment* in the template
file, and this segment specifies both the **FTI** and **PAD** options in its segment header. In this scenario you would want to call ***ResetSegment***
for the *PropertySegment* after you have generated the properties for the first class and before you generate the properties for the second class.

#### ***ResetTokenDelimiters***
The ***ResetTokenDelimiters*** method resets the token start and token end delimiters and the token escape character to their default values of "**<#**",
"**#>**", and "**\\**", respectively. (See also the [***SetTokenDelimiters***](#settokendelimiters) method.) This method has the following signature:

```csharp
void ResetTokenDelimiters()
```
<br>

> [!NOTE]
> *Normally the **ResetTokenDelimiters** method would be called only after processing one text template file and before processing a different
> text template file where the first file uses delimiters different than the default delimiters and the second one uses the default.
> However, the **ResetTokenDelimiters** method is called automatically by the [**ResetAll**](#resetall) method whenever a new template file
> is loaded.*

#### ***SetTabSize***
The ***SetTabSize*** method simply sets the tab size to the specified value. The method has the following signature:

```csharp
void SetTabSize(int tabSize)
```

The `tabSize` parameter specifies the new value for the tab size. It must be an integer value between 1 and 9, inclusive. The default value is 4.

Once the tab size has been changed, all future indentation of the lines written to the generated text buffer will be based on this new value. For
example, if you change the value from 4 to 2, and then generate a text line that specifies a tab value of 1, the line will be indented 2 spaces from
the current indent position instead of 4.

#### ***SetTemplateFilePath***
The ***SetTemplateFilePath*** method sets the file path of the template file to the specified value. Note that it only sets the file path. It does not
load the template file. For that you need to call the [***LoadTemplate***](#loadtemplate) method.

The method has the following signature:

```csharp
void SetTemplateFilePath(string templateFilePath)
```

The `templateFilePath` parameter must specify a valid file path of an existing text template file. The template file path will be set to an empty
string if this is not the case.

#### ***SetTokenDelimiters***
The ***SetTokenDelimiters*** method allows you to specify token start and end delimiters and token escape character that differ from the default values
of "**<#**", "**#>**", and "**\\**", respectively. The method has the following signature:

```csharp
bool SetTokenDelimiters(string tokenStart, string tokenEnd, char tokenEscapeChar)
```

The `tokenStart` and `tokenEnd` parameters must specify strings of one or more characters, whereas the `tokenEscapeChar` parameter must be a
single character. The `tokenStart` and `tokenEnd` parameter values must not be the same and must not match the `tokenEscapeChar` parameter.

The ***SetTokenDelimiters*** method is useful only in cases where the default delimiters may cause confusion or other issues in the text template
file. For example, maybe the string "<#" appears repeatedly in the template file as normal text. Rather than forcing you to escape all instances of
this string where it isn't the start of a token, you can instead choose to use a different string, such as "<{" for the token start delimiter and
"}>" for the token end delimiter. You would then call ***SetTokenDelimiters*** in your custom template processor class prior to loading the template
into memory.

Here's an example of how ***SetTokenDelimiters*** might be called:

```csharp
public class MyTemplateProcessor
{
    private ITextTemplateProcessor _processor = new TextTemplateProcessor(@"C:\Templates\MyTemplate.txt");

    private void LoadMyTemplate()
    {
        _processor.SetTokenDelimiters("<{", "}>", '\');
        _processor.LoadTemplate();
    }
    ...
}
```
<br>

> [!IMPORTANT]
> *You must call **SetTokenDelimiters** to set the correct delimiter values before calling [**LoadTemplate**](#loadtemplate) to load the text
> template file that is using those delimiter values.*

#### ***WriteGeneratedTextToFile***
The ***WriteGeneratedTextToFile*** method is used to write the contents of the generated text buffer to the specified output file. The method has the
following signature:

```csharp
void WriteGeneratedTextToFile(string filePath, bool resetGeneratedText = true)
```

The `filePath` parameter must specify a valid file path for the output file. If the file already exists it will be overwritten. Otherwise, a new file
will be created.

By default the generated text buffer will be cleared after the output file has been successfully written to. This is normally what you would want.
However, you can retain the contents of the generated text buffer by passing *false* into the optional `resetGeneratedText` parameter.

Once the output file has been successfully written to, the [**IsOutputFileWritten**](#isoutputfilewritten) property will be set to *true*.

Here's an example:

```csharp
public class MyTemplateProcessor
{
    private ITextTemplateProcessor _processor = new TextTemplateProcessor(@"C:\Templates\MyTemplate.txt");
    ...
    public void GenerateMyOutput()
    {
        _processor.LoadTemplate();
        _processor.GenerateSegment("Segment1");
        _processor.GenerateSegment("Segment2");
        ...
        _processor.WriteGeneratedTextToFile(@"C:\Generated\MyFile.cs");
    }
}
```

## ***TextTemplateConsoleBase*** Class
### Overview
### Constructors
### Properties
#### ***OutputDirectory*** Property
#### ***SolutionDirectory*** Property
### Methods
#### ***ClearOutputDirectory*** Method
#### ***LoadTemplate*** Method
#### ***PromptUserForInput*** Method
#### ***SetOutputDirectory*** Method
#### ***WriteGeneratedTextToFile*** Method