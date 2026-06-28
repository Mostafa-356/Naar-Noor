namespace NaarNoor.Domain.ValueObjects;

/// <summary>
/// TimeSlot value object representing a time range.
/// This value object is immutable and implements equality comparison and overlap detection.
/// </summary>
public class TimeSlot : IEquatable<TimeSlot>
{
    /// <summary>
    /// The start time of the time slot.
    /// </summary>
    public TimeOnly StartTime { get; }

    /// <summary>
    /// The end time of the time slot.
    /// </summary>
    public TimeOnly EndTime { get; }

    /// <summary>
    /// Creates a new TimeSlot value object with the specified start and end times.
    /// </summary>
    /// <param name="startTime">The start time of the slot.</param>
    /// <param name="endTime">The end time of the slot. Must be after startTime.</param>
    /// <exception cref="ArgumentException">Thrown when endTime is not after startTime.</exception>
    public TimeSlot(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("End time must be after start time.", nameof(endTime));

        StartTime = startTime;
        EndTime = endTime;
    }

    /// <summary>
    /// Gets the duration of this time slot.
    /// </summary>
    public TimeSpan Duration => EndTime.ToTimeSpan() - StartTime.ToTimeSpan();

    /// <summary>
    /// Determines whether this time slot overlaps with the specified time slot.
    /// Two time slots overlap if they share any time period.
    /// </summary>
    /// <param name="other">The other time slot to check for overlap.</param>
    /// <returns>True if the time slots overlap; otherwise, false.</returns>
    public bool Overlaps(TimeSlot other)
    {
        if (other is null)
            return false;

        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    /// <summary>
    /// Determines whether this time slot contains the specified time.
    /// </summary>
    /// <param name="time">The time to check.</param>
    /// <returns>True if the time is within this slot (inclusive of start, exclusive of end); otherwise, false.</returns>
    public bool Contains(TimeOnly time)
    {
        return time >= StartTime && time < EndTime;
    }

    /// <summary>
    /// Determines whether the specified TimeSlot object is equal to the current TimeSlot object.
    /// Two TimeSlot objects are equal if they have the same start and end times.
    /// </summary>
    public bool Equals(TimeSlot? other)
    {
        if (other is null)
            return false;

        return StartTime == other.StartTime && EndTime == other.EndTime;
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current TimeSlot object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is TimeSlot timeSlot && Equals(timeSlot);
    }

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode()
    {
        return HashCode.Combine(StartTime, EndTime);
    }

    /// <summary>
    /// Returns a string representation of the TimeSlot object in the format "{StartTime:HH:mm} - {EndTime:HH:mm}".
    /// </summary>
    public override string ToString()
    {
        return $"{StartTime:HH:mm} - {EndTime:HH:mm}";
    }

    /// <summary>
    /// Determines whether two TimeSlot objects are equal.
    /// </summary>
    public static bool operator ==(TimeSlot? left, TimeSlot? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two TimeSlot objects are not equal.
    /// </summary>
    public static bool operator !=(TimeSlot? left, TimeSlot? right)
    {
        return !(left == right);
    }
}
