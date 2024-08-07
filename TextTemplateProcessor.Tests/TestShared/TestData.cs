﻿namespace TextTemplateProcessor.TestShared
{
    public static class TestData
    {
#pragma warning disable IDE0028 // Simplify collection initialization
        public static TheoryData<string> InvalidFileNameCharacters => new()
        {
            "\0",
            "\u0001",
            "\u0002",
            "\u0003",
            "\u0004",
            "\u0005",
            "\u0006",
            "\a",
            "\b",
            "\t",
            "\n",
            "\v",
            "\f",
            "\r",
            "\u000e",
            "\u000f",
            "\u0010",
            "\u0011",
            "\u0012",
            "\u0013",
            "\u0014",
            "\u0015",
            "\u0016",
            "\u0017",
            "\u0018",
            "\u0019",
            "\u001a",
            "\u001b",
            "\u001c",
            "\u001d",
            "\u001e",
            "\u001f",
            "\"",
            "*",
            ":",
            "<",
            ">",
            "?",
            "|"
        };

        public static TheoryData<string> InvalidPathCharacters => new()
        {
            "\0",
            "\u0001",
            "\u0002",
            "\u0003",
            "\u0004",
            "\u0005",
            "\u0006",
            "\a",
            "\b",
            "\t",
            "\n",
            "\v",
            "\f",
            "\r",
            "\u000e",
            "\u000f",
            "\u0010",
            "\u0011",
            "\u0012",
            "\u0013",
            "\u0014",
            "\u0015",
            "\u0016",
            "\u0017",
            "\u0018",
            "\u0019",
            "\u001a",
            "\u001b",
            "\u001c",
            "\u001d",
            "\u001e",
            "\u001f",
            "|"
        };

        public static TheoryData<string> Whitespace => new()
        {
            "",
            "\t",
            "\n",
            "\v",
            "\f",
            "\r",
            " ",
            "\u0085",
            "\u00a0",
            "\u2002",
            "\u2003",
            "\u2028",
            "\u2029",
            Globals.Whitespace
        };
#pragma warning restore IDE0028 // Simplify collection initialization
    }
}