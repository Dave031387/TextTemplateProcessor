namespace TextTemplateProcessor.TestShared
{
    using System;
    using System.Linq.Expressions;

    public class MockHelper
    {
        internal Expression<Action<IFileAndDirectoryService>> ClearDirectoryExpression { get; private set; } = x => x.ClearDirectory("test");
        internal Expression<Action<IConsoleWriter>> ConsoleWriterExpression { get; private set; } = x => x.WriteLine("test");
        internal Expression<Action<IFileAndDirectoryService>> CreateDirectoryExpression { get; private set; } = x => x.ClearDirectory("test");
        internal Expression<Func<IIndentProcessor, int>> CurrentIndentExpression { get; private set; } = x => x.CurrentIndent;
        internal Expression<Func<ILocater, string>> CurrentSegmentExpression { get; private set; } = x => x.CurrentSegment;
        internal Expression<Func<IDefaultSegmentNameGenerator, string>> DefaultSegmentNameExpression { get; private set; } = x => x.Next;
        internal Expression<Func<IFileAndDirectoryService, string>> GetDirectoryNameExpression { get; private set; } = x => "test";
        internal Expression<Func<IFileAndDirectoryService, string>> GetFileNameExpression { get; private set; } = x => "test";
        internal Expression<Func<IIndentProcessor, int>> GetFirstTimeIndentExpression { get; private set; } = x => 0;
        internal Expression<Func<IIndentProcessor, int>> GetIndentExpression { get; private set; } = x => 0;
        internal Expression<Action<IIndentProcessor>> IndentProcessorResetExpression { get; private set; } = x => x.Reset();
        internal Expression<Func<IIndentProcessor, bool>> IsValidIndentValueExpression { get; private set; } = x => true;
        internal Expression<Func<IIndentProcessor, bool>> IsValidTabSizeExpression { get; private set; } = x => false;
        internal Expression<Func<ILocater, int>> LineNumberExpression { get; private set; } = x => x.LineNumber;
        internal Expression<Func<ILocater, (string, int)>> LocationExpression { get; private set; } = x => x.Location;
        internal Expression<Func<IFileAndDirectoryService, IEnumerable<string>>> ReadTextFileExpression { get; private set; } = x => new[] { "test" };
        internal Expression<Action<IIndentProcessor>> RestoreCurrentStateExpression { get; private set; } = x => x.RestoreCurrentState();
        internal Expression<Action<IIndentProcessor>> SaveCurrentStateExpression { get; private set; } = x => x.SaveCurrentState();
        internal Action<ILocater> SetCurrentSegmentAction { get; private set; } = x => { };
        internal Action<ILocater> SetLineNumberAction { get; private set; } = x => { };
        internal Expression<Action<IIndentProcessor>> SetTabSizeExpression { get; private set; } = x => x.SetTabSize(0);
        internal Expression<Func<IIndentProcessor, int>> TabSizeExpression { get; private set; } = x => x.TabSize;
        internal Expression<Func<IPathValidater, string>> ValidateFullPathExpression { get; private set; } = x => "test";
        internal Expression<Action<IPathValidater>> ValidatePathExpression { get; private set; } = x => x.ValidatePath("test", false, false);
        internal Expression<Action<IFileAndDirectoryService>> WriteTextFileExpression { get; private set; } = x => x.WriteTextFile("test", new[] { "test" });

        internal static Expression<Action<ILogger>> GetLoggerExpression(string message,
                                                                        string? arg1 = null,
                                                                        string? arg2 = null)
        {
            if (arg1 is null)
            {
                return x => x.Log(message, null, null);
            }
            else
            {
                if (arg2 is null)
                {
                    if (arg1 is AnyString)
                    {
                        return x => x.Log(message, It.IsAny<string>(), null);
                    }
                    else
                    {
                        return x => x.Log(message, arg1, null);
                    }
                }
                else
                {
                    if (arg1 is AnyString)
                    {
                        if (arg2 is AnyString)
                        {
                            return x => x.Log(message, It.IsAny<string>(), It.IsAny<string>());
                        }
                        else
                        {
                            return x => x.Log(message, It.IsAny<string>(), arg2);
                        }
                    }
                    else
                    {
                        if (arg2 is AnyString)
                        {
                            return x => x.Log(message, arg1, It.IsAny<string>());
                        }
                        else
                        {
                            return x => x.Log(message, arg1, arg2);
                        }
                    }
                }
            }
        }

        internal static Expression<Action<ILogger>> SetupLogger(Mock<ILogger> mock,
                                                                string message,
                                                                string? arg1 = null,
                                                                string? arg2 = null)
        {
            Expression<Action<ILogger>> loggerExpression = GetLoggerExpression(message, arg1, arg2);
            mock.Setup(loggerExpression);
            return loggerExpression;
        }

        internal static Expression<Action<IMessageWriter>> SetupMessageWriter(Mock<IMessageWriter> mock, string message, Action<string> callback)
        {
            Expression<Action<IMessageWriter>> messageWriterExpression = x => x.WriteLine(message);
            mock.Setup(messageWriterExpression).Callback(callback);
            return messageWriterExpression;
        }

        internal static Expression<Func<INameValidater, bool>> SetupNameValidater(Mock<INameValidater> mock,
                                                                                  string name,
                                                                                  bool result)
        {
            Expression<Func<INameValidater, bool>> nameValidaterExpression = x => x.IsValidName(name);
            mock.Setup(nameValidaterExpression).Returns(result);
            return nameValidaterExpression;
        }

        internal void SetupConsoleWriter(Mock<IConsoleWriter> mock, string text)
        {
            ConsoleWriterExpression = x => x.WriteLine(text);
            mock.Setup(ConsoleWriterExpression);
        }

        internal void SetupDefaultSegmentNameGenerator(Mock<IDefaultSegmentNameGenerator> mock)
        {
            int counter = 0;
            mock.SetupSequence(DefaultSegmentNameExpression)
                .Returns($"{DefaultSegmentNamePrefix}{++counter}")
                .Returns($"{DefaultSegmentNamePrefix}{++counter}")
                .Returns($"{DefaultSegmentNamePrefix}{++counter}")
                .Returns($"{DefaultSegmentNamePrefix}{++counter}")
                .Returns($"{DefaultSegmentNamePrefix}{++counter}")
                .Throws<ArgumentOutOfRangeException>();
        }

        internal void SetupFileAndDirectoryService(Mock<IFileAndDirectoryService> mock,
                                                   string directoryPath,
                                                   string fileName,
                                                   string filePath,
                                                   IEnumerable<string>? textLines = null,
                                                   bool readTextThrows = false)
        {
            ClearDirectoryExpression = x => x.ClearDirectory(directoryPath);
            CreateDirectoryExpression = x => x.CreateDirectory(directoryPath);
            GetDirectoryNameExpression = x => x.GetDirectoryName(filePath);
            GetFileNameExpression = x => x.GetFileName(filePath);
            ReadTextFileExpression = x => x.ReadTextFile(filePath);
            mock.Setup(ClearDirectoryExpression);
            mock.Setup(CreateDirectoryExpression);
            mock.Setup(GetDirectoryNameExpression).Returns(directoryPath);
            mock.Setup(GetFileNameExpression).Returns(fileName);

            if (textLines is not null)
            {
                WriteTextFileExpression = x => x.WriteTextFile(filePath, textLines);
                mock.Setup(WriteTextFileExpression);

                if (readTextThrows)
                {
                    mock.Setup(ReadTextFileExpression).Throws<ArgumentException>();
                }
                else
                {
                    mock.Setup(ReadTextFileExpression).Returns(textLines);
                }
            }
        }

        internal void SetupIndentProcessorForIndentValues(Mock<IIndentProcessor> mock,
                                           string indentStringValue,
                                           bool isValidIndentStringValue,
                                           int indentIntegerValue,
                                           int returnedIndentValue,
                                           TextItem? textItem = null)
        {
            mock.Setup(CurrentIndentExpression).Returns(returnedIndentValue);
            IsValidIndentValueExpression = x => x.IsValidIndentValue(indentStringValue, out indentIntegerValue);
            mock.Setup(IsValidIndentValueExpression).Returns(isValidIndentStringValue);

            if (textItem is not null)
            {
                GetFirstTimeIndentExpression = x => x.GetFirstTimeIndent(indentIntegerValue, textItem);
                mock.Setup(GetFirstTimeIndentExpression).Returns(returnedIndentValue);
                GetIndentExpression = x => x.GetIndent(textItem);
                mock.Setup(GetIndentExpression).Returns(returnedIndentValue);
            }
        }

        internal void SetupIndentProcessorForOtherMethods(Mock<IIndentProcessor> mock)
        {
            mock.Setup(IndentProcessorResetExpression);
            mock.Setup(RestoreCurrentStateExpression);
            mock.Setup(SaveCurrentStateExpression);
        }

        internal void SetupIndentProcessorForTabSizeValues(Mock<IIndentProcessor> mock,
                                           string tabSizeStringValue,
                                           bool isValidTabSizeStringValue,
                                           int tabSizeIntegerValue,
                                           int returnedTabSizeValue)
        {
            mock.Setup(TabSizeExpression).Returns(returnedTabSizeValue);
            IsValidTabSizeExpression = x => x.IsValidTabSizeValue(tabSizeStringValue, out tabSizeIntegerValue);
            mock.Setup(IsValidTabSizeExpression).Returns(isValidTabSizeStringValue);
            SetTabSizeExpression = x => x.SetTabSize(tabSizeIntegerValue);
            mock.Setup(SetTabSizeExpression);
        }

        internal void SetupLocater(Mock<ILocater> mock,
                                   string segmentName,
                                   int lineNumber)
        {
            (string, int) location = (segmentName, lineNumber);
            SetCurrentSegmentAction = x => x.CurrentSegment = segmentName;
            SetLineNumberAction = x => x.LineNumber = lineNumber;
            mock.Setup(CurrentSegmentExpression).Returns(segmentName);
            mock.Setup(LineNumberExpression).Returns(lineNumber);
            mock.Setup(LocationExpression).Returns(location);
            mock.SetupSet(SetCurrentSegmentAction);
            mock.SetupSet(SetLineNumberAction);
        }

        internal void SetupPathValidater(Mock<IPathValidater> mock,
                                         string path,
                                         bool isFilePath,
                                         bool shouldExist,
                                         string? returnValue = null)
        {
            if (returnValue is null)
            {
                ValidatePathExpression = x => x.ValidatePath(path, isFilePath, shouldExist);
                mock.Setup(ValidatePathExpression);
            }
            else
            {
                ValidateFullPathExpression = x => x.ValidateFullPath(path, isFilePath, shouldExist);
                mock.Setup(ValidateFullPathExpression).Returns(returnValue);
            }
        }

        internal void SetupPathValidater(Mock<IPathValidater> mock,
                                         string path,
                                         bool isFilePath,
                                         bool shouldExist,
                                         bool throws)
        {
            if (throws)
            {
                ValidatePathExpression = x => x.ValidatePath(path, isFilePath, shouldExist);
                ValidateFullPathExpression = x => x.ValidateFullPath(path, isFilePath, shouldExist);
                mock.Setup(ValidatePathExpression).Throws<ArgumentException>();
                mock.Setup(ValidateFullPathExpression).Throws<ArgumentException>();
            }
        }
    }
}