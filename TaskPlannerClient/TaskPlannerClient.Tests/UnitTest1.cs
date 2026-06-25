using System.Globalization;
using System.Windows;
using TaskPlannerClient.Converters;
using Xunit;

namespace TaskPlannerClient.Tests
{
    public class BooleanToVisibilityConverterTests
    {
        private readonly BooleanToVisibilityConverter _converter = new BooleanToVisibilityConverter();

        [Theory]
        [InlineData(true, null, Visibility.Visible)]
        [InlineData(false, null, Visibility.Collapsed)]
        [InlineData(true, "inverse", Visibility.Collapsed)]
        [InlineData(false, "inverse", Visibility.Visible)]
        [InlineData(true, "Inverse", Visibility.Collapsed)]  // регистронезависимость
        public void Convert_ShouldReturnExpectedVisibility(
            bool value,
            string parameter,
            Visibility expected)
        {
            // Act
            var result = _converter.Convert(value, typeof(Visibility), parameter, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Convert_WithNullValue_ShouldReturnCollapsed()
        {
            // Act
            var result = _converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_WithInvalidParameter_ShouldTreatAsNoInversion()
        {
            // Act
            var result = _converter.Convert(true, typeof(Visibility), "unknown", CultureInfo.InvariantCulture);

            // Assert
            Assert.Equal(Visibility.Visible, result);
        }
    }
}
