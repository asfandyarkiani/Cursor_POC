using FacilitiesMgmtSystem.Helper;
using FluentAssertions;

namespace FacilitiesMgmtSystem.Tests.Helpers;

public class XMLHelperTests
{
    [Fact]
    public void GetElementValue_WithValidXPath_ReturnsValue()
    {
        // Arrange
        var xml = @"<root><child>test value</child></root>";

        // Act
        var result = XMLHelper.GetElementValue(xml, "//child");

        // Assert
        result.Should().Be("test value");
    }

    [Fact]
    public void GetElementValue_WithInvalidXPath_ReturnsNull()
    {
        // Arrange
        var xml = @"<root><child>test value</child></root>";

        // Act
        var result = XMLHelper.GetElementValue(xml, "//nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetElementValue_WithInvalidXml_ReturnsNull()
    {
        // Arrange
        var xml = "not valid xml";

        // Act
        var result = XMLHelper.GetElementValue(xml, "//child");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void SetElementValue_WithValidXPath_SetsValue()
    {
        // Arrange
        var xml = @"<root><child>old value</child></root>";

        // Act
        var result = XMLHelper.SetElementValue(xml, "//child", "new value");

        // Assert
        result.Should().Contain("new value");
        result.Should().NotContain("old value");
    }

    [Fact]
    public void SetElementValue_WithInvalidXPath_ReturnsUnchangedXml()
    {
        // Arrange
        var xml = @"<root><child>test value</child></root>";

        // Act
        var result = XMLHelper.SetElementValue(xml, "//nonexistent", "new value");

        // Assert
        result.Should().Contain("test value");
    }

    [Fact]
    public void Serialize_WithValidObject_ReturnsXmlString()
    {
        // Arrange
        var obj = new TestSerializable { Name = "Test", Value = 123 };

        // Act
        var result = XMLHelper.Serialize(obj);

        // Assert
        result.Should().Contain("<Name>Test</Name>");
        result.Should().Contain("<Value>123</Value>");
    }

    [Fact]
    public void Deserialize_WithValidXml_ReturnsObject()
    {
        // Arrange
        var xml = @"<?xml version=""1.0"" encoding=""utf-16""?>
            <TestSerializable>
                <Name>Test</Name>
                <Value>456</Value>
            </TestSerializable>";

        // Act
        var result = XMLHelper.Deserialize<TestSerializable>(xml);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test");
        result.Value.Should().Be(456);
    }

    [Fact]
    public void Deserialize_WithInvalidXml_ReturnsNull()
    {
        // Arrange
        var xml = "not valid xml";

        // Act
        var result = XMLHelper.Deserialize<TestSerializable>(xml);

        // Assert
        result.Should().BeNull();
    }

    public class TestSerializable
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }
}
