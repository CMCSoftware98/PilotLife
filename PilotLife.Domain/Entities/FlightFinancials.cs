using PilotLife.Domain.Common;

namespace PilotLife.Domain.Entities;

/// <summary>
/// Tracks all financial aspects of a tracked flight.
/// One-to-one relationship with TrackedFlight.
/// </summary>
public class FlightFinancials : BaseEntity
{
    /// <summary>
    /// The tracked flight these financials belong to.
    /// </summary>
    public Guid TrackedFlightId { get; set; }
    public TrackedFlight TrackedFlight { get; set; } = null!;

    // Revenue
    /// <summary>
    /// Total payout from completed jobs.
    /// </summary>
    public decimal JobRevenue { get; set; }

    /// <summary>
    /// Bonus for on-time delivery.
    /// </summary>
    public decimal OnTimeBonus { get; set; }

    /// <summary>
    /// Bonus for smooth landing (based on landing rate).
    /// </summary>
    public decimal LandingBonus { get; set; }

    /// <summary>
    /// Bonus for fuel efficiency.
    /// </summary>
    public decimal FuelEfficiencyBonus { get; set; }

    /// <summary>
    /// Any other miscellaneous bonuses.
    /// </summary>
    public decimal OtherBonuses { get; set; }

    // Costs
    /// <summary>
    /// Cost of fuel used during the flight.
    /// </summary>
    public decimal FuelCost { get; set; }

    /// <summary>
    /// Landing fees at the destination airport.
    /// </summary>
    public decimal LandingFees { get; set; }

    /// <summary>
    /// Handling and ground service fees.
    /// </summary>
    public decimal HandlingFees { get; set; }

    /// <summary>
    /// Navigation and ATC fees.
    /// </summary>
    public decimal NavigationFees { get; set; }

    /// <summary>
    /// Aircraft maintenance cost for this flight.
    /// </summary>
    public decimal MaintenanceCost { get; set; }

    /// <summary>
    /// Insurance cost allocated to this flight.
    /// </summary>
    public decimal InsuranceCost { get; set; }

    /// <summary>
    /// Crew salary cost for this flight.
    /// </summary>
    public decimal CrewCost { get; set; }

    // Penalties
    /// <summary>
    /// Penalty for late delivery.
    /// </summary>
    public decimal LatePenalty { get; set; }

    /// <summary>
    /// Penalty for damaged cargo.
    /// </summary>
    public decimal DamagePenalty { get; set; }

    /// <summary>
    /// Penalty for hard landings or other incidents.
    /// </summary>
    public decimal IncidentPenalty { get; set; }

    // Calculated properties
    /// <summary>
    /// Total revenue (jobs + all bonuses).
    /// </summary>
    public decimal TotalRevenue => JobRevenue + OnTimeBonus + LandingBonus +
                                   FuelEfficiencyBonus + OtherBonuses;

    /// <summary>
    /// Total operating costs.
    /// </summary>
    public decimal TotalCosts => FuelCost + LandingFees + HandlingFees +
                                 NavigationFees + MaintenanceCost +
                                 InsuranceCost + CrewCost;

    /// <summary>
    /// Total penalties applied.
    /// </summary>
    public decimal TotalPenalties => LatePenalty + DamagePenalty + IncidentPenalty;

    /// <summary>
    /// Net profit/loss for this flight.
    /// </summary>
    public decimal NetProfit => TotalRevenue - TotalCosts - TotalPenalties;

    /// <summary>
    /// Whether this flight was profitable.
    /// </summary>
    public bool IsProfitable => NetProfit > 0;
}
