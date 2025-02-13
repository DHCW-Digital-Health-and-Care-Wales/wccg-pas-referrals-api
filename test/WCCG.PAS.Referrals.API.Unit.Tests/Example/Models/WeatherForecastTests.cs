using FluentAssertions;
using WCCG.PAS.Referrals.API.Models;

namespace WCCG.PAS.Referrals.API.Unit.Tests.Example.Models;

public class WeatherForecastTests
{
    [Fact]
    public void TemperatureFShouldCalculateCorrectly()
    {
        // Arrange
        var forecast = new WeatherForecast { TemperatureC = 0 };

        // Act
        var tempF = forecast.TemperatureF;

        // Assert
        tempF.Should().Be(32);
    }

    [Fact]
    public void TemperatureFShouldCalculateCorrectlyForNegativeTemperature()
    {
        // Arrange
        var forecast = new WeatherForecast { TemperatureC = -40 };

        // Act
        var tempF = forecast.TemperatureF;

        // Assert
        tempF.Should().Be(-39);
    }

    [Fact]
    public void DateShouldBeSetCorrectly()
    {
        // Arrange
        var date = new DateOnly(2025, 2, 12);
        var forecast = new WeatherForecast { Date = date };

        // Act
        var result = forecast.Date;

        // Assert
        result.Should().Be(date);
    }

    [Fact]
    public void SummaryShouldBeSetCorrectly()
    {
        // Arrange
        var summary = "Sunny";
        var forecast = new WeatherForecast { Summary = summary };

        // Act
        var result = forecast.Summary;

        // Assert
        result.Should().Be(summary);
    }
}
