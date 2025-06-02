namespace Trips_Database.Exceptions;

public class TripIsOnGoingException(string message) : Exception(message);