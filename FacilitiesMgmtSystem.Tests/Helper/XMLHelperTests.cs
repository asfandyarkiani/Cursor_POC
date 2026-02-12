using FluentAssertions;
using FacilitiesMgmtSystem.Helper;
using Xunit;

namespace FacilitiesMgmtSystem.Tests.Helper;

/// <summary>
/// Unit tests for XMLHelper class.
/// </summary>
public class XMLHelperTests
{
    [Fact]
    public void EscapeXml_WithSpecialCharacters_ShouldEscapeProperly()
    {
        // Arrange
        var input = "<test & \"value\" 'here'>";

        // Act
        var result = XMLHelper.EscapeXml(input);

        // Assert
        result.Should().Be("&lt;test &amp; &quot;value&quot; &apos;here&apos;&gt;");
    }

    [Fact]
    public void EscapeXml_WithNullInput_ShouldReturnEmptyString()
    {
        // Arrange
        string? input = null;

        // Act
        var result = XMLHelper.EscapeXml(input);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void EscapeXml_WithEmptyString_ShouldReturnEmptyString()
    {
        // Arrange
        var input = string.Empty;

        // Act
        var result = XMLHelper.EscapeXml(input);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ReplacePlaceholders_WithValidPlaceholders_ShouldReplaceAll()
    {
        // Arrange
        var template = "<Request><SessionId>{{SessionId}}</SessionId><Name>{{Name}}</Name></Request>";
        var values = new Dictionary<string, string?>
        {
            ["SessionId"] = "session-123",
            ["Name"] = "Test Name"
        };

        // Act
        var result = XMLHelper.ReplacePlaceholders(template, values);

        // Assert
        result.Should().Be("<Request><SessionId>session-123</SessionId><Name>Test Name</Name></Request>");
    }

    [Fact]
    public void ReplacePlaceholders_WithSpecialCharacters_ShouldEscapeValues()
    {
        // Arrange
        var template = "<Request><Description>{{Description}}</Description></Request>";
        var values = new Dictionary<string, string?>
        {
            ["Description"] = "Test & <special> \"chars\""
        };

        // Act
        var result = XMLHelper.ReplacePlaceholders(template, values);

        // Assert
        result.Should().Contain("&amp;");
        result.Should().Contain("&lt;");
        result.Should().Contain("&gt;");
        result.Should().Contain("&quot;");
    }

    [Fact]
    public void ReplacePlaceholders_WithNullValue_ShouldReplaceWithEmptyString()
    {
        // Arrange
        var template = "<Request><Value>{{Value}}</Value></Request>";
        var values = new Dictionary<string, string?>
        {
            ["Value"] = null
        };

        // Act
        var result = XMLHelper.ReplacePlaceholders(template, values);

        // Assert
        result.Should().Be("<Request><Value></Value></Request>");
    }

    [Fact]
    public void GetNodeValue_WithValidXPath_ShouldReturnValue()
    {
        // Arrange
        var xml = "<Root><Child>TestValue</Child></Root>";
        var xpath = "//Child";

        // Act
        var result = XMLHelper.GetNodeValue(xml, xpath);

        // Assert
        result.Should().Be("TestValue");
    }

    [Fact]
    public void GetNodeValue_WithInvalidXPath_ShouldReturnNull()
    {
        // Arrange
        var xml = "<Root><Child>TestValue</Child></Root>";
        var xpath = "//NonExistent";

        // Act
        var result = XMLHelper.GetNodeValue(xml, xpath);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetNodeValue_WithInvalidXml_ShouldReturnNull()
    {
        // Arrange
        var xml = "not valid xml";
        var xpath = "//Child";

        // Act
        var result = XMLHelper.GetNodeValue(xml, xpath);

        // Assert
        result.Should().BeNull();
    }
}
