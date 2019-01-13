using System;
using FluentAssertions;
using Xunit;

namespace Updater.Domain
{
    public class ImageUriParserTests
    {
        [Fact]
        public void WhenEmptyTagIsDefined_ThenReturnItAsLatest()
        {
            var (uri, tag) = ImageUriParser.ParseUri("domain.eu/image");

            tag.Should().Be("latest");
            uri.Should().Be("domain.eu/image");
        }

        [Fact]
        public void WhenTagAndUriIsGiven_ThenParseResultsCorrectly()
        {
            var (uri, tag) = ImageUriParser.ParseUri("domain.eu/image:sometag");

            tag.Should().Be("sometag");
            uri.Should().Be("domain.eu/image");
        }

        [Fact]
        public void WhenImageWithoutDomainIsGiven_ThenParseItAsExpected()
        {
            var (uri, tag) = ImageUriParser.ParseUri("image:sometag");

            tag.Should().Be("sometag");
            uri.Should().Be("image");
        }

        [Fact]
        public void WhenImagesAreEqual_ThenEqualReturnsTrue()
        {
            ImageUriParser.ParseUri("image:sometag")
                .Equals(ImageUriParser.ParseUri("image:sometag"))
                .Should()
                .Be(true);
        }

        [Fact]
        public void WhenImagesAreNotEqual_ThenEqualShouldReturnFalse()
        {
            ImageUriParser.ParseUri("image:sometag")
                .Equals(ImageUriParser.ParseUri("image:someothertag"))
                .Should()
                .Be(false);
        }

        [Fact]
        public void WhenParsesInvalidFormatOfUri_ThenThrowError()
        {
            Action test = () => ImageUriParser.ParseUri(":sometag");
            test.Should().Throw<ArgumentException>();
        }
    }
}