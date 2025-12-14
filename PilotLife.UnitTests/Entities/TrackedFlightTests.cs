using PilotLife.Domain.Entities;
using PilotLife.Domain.Enums;

namespace PilotLife.UnitTests.Entities;

public class TrackedFlightTests
{
    [Fact]
    public void NewTrackedFlight_HasValidId()
    {
        var flight = new TrackedFlight();

        Assert.NotEqual(Guid.Empty, flight.Id);
    }

    [Fact]
    public void NewTrackedFlight_DefaultsToPendingState()
    {
        var flight = new TrackedFlight();

        Assert.Equal(FlightState.Pending, flight.State);
    }

    [Fact]
    public void NewTrackedFlight_HasEmptyFlightJobsCollection()
    {
        var flight = new TrackedFlight();

        Assert.NotNull(flight.FlightJobs);
        Assert.Empty(flight.FlightJobs);
    }

    [Fact]
    public void NewTrackedFlight_HasCreatedAt()
    {
        var before = DateTimeOffset.UtcNow;
        var flight = new TrackedFlight();
        var after = DateTimeOffset.UtcNow;

        Assert.InRange(flight.CreatedAt, before, after);
    }

    [Fact]
    public void NewTrackedFlight_NumericPropertiesDefaultToZero()
    {
        var flight = new TrackedFlight();

        Assert.Equal(0, flight.FlightTimeMinutes);
        Assert.Equal(0, flight.DistanceNm);
        Assert.Equal(0, flight.MaxAltitude);
        Assert.Equal(0, flight.MaxGroundSpeed);
        Assert.Equal(0, flight.HardLandingCount);
        Assert.Equal(0, flight.OverspeedCount);
        Assert.Equal(0, flight.StallWarningCount);
    }

    [Fact]
    public void NewTrackedFlight_NullablePropertiesAreNull()
    {
        var flight = new TrackedFlight();

        Assert.Null(flight.AircraftId);
        Assert.Null(flight.Aircraft);
        Assert.Null(flight.AircraftTitle);
        Assert.Null(flight.AircraftIcaoType);
        Assert.Null(flight.DepartureAirportId);
        Assert.Null(flight.DepartureAirport);
        Assert.Null(flight.DepartureIcao);
        Assert.Null(flight.DepartureTime);
        Assert.Null(flight.ArrivalAirportId);
        Assert.Null(flight.ArrivalAirport);
        Assert.Null(flight.ArrivalIcao);
        Assert.Null(flight.ArrivalTime);
        Assert.Null(flight.CurrentLatitude);
        Assert.Null(flight.CurrentLongitude);
        Assert.Null(flight.CurrentAltitude);
        Assert.Null(flight.CurrentHeading);
        Assert.Null(flight.CurrentGroundSpeed);
        Assert.Null(flight.LastPositionUpdate);
        Assert.Null(flight.LandingRate);
        Assert.Null(flight.FuelUsedGallons);
        Assert.Null(flight.StartFuelGallons);
        Assert.Null(flight.EndFuelGallons);
        Assert.Null(flight.PayloadWeightLbs);
        Assert.Null(flight.TotalWeightLbs);
        Assert.Null(flight.CompletedAt);
        Assert.Null(flight.Notes);
        Assert.Null(flight.ConnectorSessionId);
        Assert.Null(flight.Financials);
    }

    [Theory]
    [InlineData(FlightState.Pending)]
    [InlineData(FlightState.PreFlight)]
    [InlineData(FlightState.Taxiing)]
    [InlineData(FlightState.Departing)]
    [InlineData(FlightState.EnRoute)]
    [InlineData(FlightState.Arriving)]
    [InlineData(FlightState.Arrived)]
    [InlineData(FlightState.Shutdown)]
    [InlineData(FlightState.Cancelled)]
    [InlineData(FlightState.Failed)]
    public void TrackedFlight_CanSetAllFlightStates(FlightState state)
    {
        var flight = new TrackedFlight { State = state };

        Assert.Equal(state, flight.State);
    }

    [Fact]
    public void TrackedFlight_CanAddFlightJobs()
    {
        var flight = new TrackedFlight();
        var job = new FlightJob
        {
            TrackedFlightId = flight.Id,
            JobId = Guid.CreateVersion7()
        };

        flight.FlightJobs.Add(job);

        Assert.Single(flight.FlightJobs);
        Assert.Contains(job, flight.FlightJobs);
    }

    [Fact]
    public void TrackedFlight_CanSetFinancials()
    {
        var flight = new TrackedFlight();
        var financials = new FlightFinancials
        {
            TrackedFlightId = flight.Id,
            JobRevenue = 1000m
        };

        flight.Financials = financials;

        Assert.NotNull(flight.Financials);
        Assert.Equal(1000m, flight.Financials.JobRevenue);
    }
}
