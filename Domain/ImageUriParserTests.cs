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
            var result = ImageUriParser.ParseUri("domain.eu/image");

            result.tag.Should().Be("latest");
            result.uri.Should().Be("domain.eu/image");
        }

        [Fact]
        public void WhenTagAndUriIsGiven_ThenParseResultsCorrectly()
        {
            var result = ImageUriParser.ParseUri("domain.eu/image:sometag");

            result.tag.Should().Be("sometag");
            result.uri.Should().Be("domain.eu/image");
        }

        [Fact]
        public void WhenImageWithoutDomainIsGiven_ThenParseItAsExpected()
        {
            var result = ImageUriParser.ParseUri("image:sometag");

            result.tag.Should().Be("sometag");
            result.uri.Should().Be("image");
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
            test.ShouldThrow<ArgumentException>();
        }
    }
}