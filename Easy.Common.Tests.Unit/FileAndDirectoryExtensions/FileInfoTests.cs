﻿namespace Easy.Common.Tests.Unit.FileAndDirectoryExtensions
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using Easy.Common.Extensions;
    using NUnit.Framework;
    using Shouldly;

    [TestFixture]
    public class FileInfoTests : Context
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Given_a_temp_hidden_file();
            Given_a_temp_non_hidden_file();

            When_checking_if_the_files_are_hidden();
        }

        [Test]
        public void Then_isHidden_for_the_hidden_file_should_be_correct()
        {
            ResultOne.ShouldBeTrue();
        }
        
        [Test]
        public void Then_isHidden_for_the_non_hidden_file_should_be_correct()
        {
            ResultTwo.ShouldBeFalse();
        }

        [Test]
        public void When_reading_all_text_lines_in_file()
        {
            var fileInfo = new FileInfo(Path.GetRandomFileName());

            try
            {
                using (var writer = new StreamWriter(File.Open(fileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.Read)))
                {
                    writer.WriteLine("1stLine");
                    writer.WriteLine("2ndLine");
                    writer.Flush();

                    // This does not work
                    Should.Throw<IOException>(() => File.ReadAllLines(fileInfo.FullName))
                        .Message.ShouldStartWith($"The process cannot access the file '{fileInfo.FullName}' because it is being used by another process.");

                    // But this one does
                    var linesExplicitEncoding = fileInfo.ReadAllLines(Encoding.UTF8).ToArray();
                    linesExplicitEncoding.ShouldNotBeNull();
                    linesExplicitEncoding.Length.ShouldBe(2);
                    linesExplicitEncoding[0].ShouldBe("1stLine");
                    linesExplicitEncoding[1].ShouldBe("2ndLine");

                    // So does this one
                    var linesDefaultEncoding = fileInfo.ReadAllLines().ToArray();
                    linesDefaultEncoding.ShouldNotBeNull();
                    linesDefaultEncoding.Length.ShouldBe(2);
                    linesDefaultEncoding[0].ShouldBe("1stLine");
                    linesDefaultEncoding[1].ShouldBe("2ndLine");
                }
            }
            finally
            {
                fileInfo.Delete();
            }
        }
    }
}