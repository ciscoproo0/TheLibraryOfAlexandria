using TheLibraryOfAlexandria.Models;
using TheLibraryOfAlexandria.Utils;

/// <summary>
/// IShippingInfoService defines the contract for shipping information management operations.
/// This service handles shipping details, tracking information, status updates, and delivery management.
/// Each order has associated shipping information tracking its delivery lifecycle.
/// </summary>
public interface IShippingInfoService
{
    /// <summary>
    /// Creates shipping information for an order.
    /// </summary>
    /// <param name="shippingInfo">The ShippingInfo object containing address, shipping cost, and tracking details.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Created ShippingInfo object with generated ID
    /// - Failure: Error message if creation fails (e.g., invalid order, validation errors)
    /// </returns>
    /// <remarks>
    /// Initial shipping status is "Preparing".
    /// Estimated delivery date can be calculated based on shipping method and address.
    /// </remarks>
    Task<ServiceResponse<ShippingInfo>> CreateShippingInfoAsync(ShippingInfo shippingInfo);

    /// <summary>
    /// Retrieves all shipping records without filtering.
    /// </summary>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: List of all ShippingInfo objects for all orders
    /// - Failure: Error message if query fails
    /// </returns>
    Task<ServiceResponse<List<ShippingInfo>>> GetAllShippingInfosAsync();

    /// <summary>
    /// Retrieves a single shipping record by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the shipping information to retrieve.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Complete ShippingInfo object with address and status details
    /// - Failure: Error message if shipping record not found
    /// </returns>
    Task<ServiceResponse<ShippingInfo>> GetShippingInfoByIdAsync(int id);

    /// <summary>
    /// Updates shipping information with new details, particularly status changes and tracking updates.
    /// </summary>
    /// <param name="id">The unique identifier of the shipping record to update.</param>
    /// <param name="updated">The updated ShippingInfo object with new values.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Updated ShippingInfo object with new status and dates
    /// - Failure: Error message if shipping record not found or update fails
    /// </returns>
    /// <remarks>
    /// Status transitions: Preparing → Shipped → Delivered
    /// ShippedDate is populated when status changes to "Shipped"
    /// DeliveredDate is populated when status changes to "Delivered"
    /// TrackingNumber is updated when carrier information becomes available.
    /// </remarks>
    Task<ServiceResponse<ShippingInfo>> UpdateShippingInfoAsync(int id, ShippingInfo updated);

    /// <summary>
    /// Deletes a shipping record from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the shipping record to delete.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: Boolean true if deletion succeeded
    /// - Failure: Error message if shipping record not found or deletion fails
    /// </returns>
    /// <remarks>
    /// Deletion may be restricted for delivered orders to preserve tracking history.
    /// </remarks>
    Task<ServiceResponse<bool>> DeleteShippingInfoAsync(int id);

    /// <summary>
    /// Retrieves shipping information for a specific order.
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <returns>
    /// A ServiceResponse containing:
    /// - Success: ShippingInfo object for the order with current status and tracking
    /// - Failure: Error message if order not found or has no shipping information
    /// </returns>
    /// <remarks>
    /// Each order should have exactly one ShippingInfo record.
    /// </remarks>
    Task<ServiceResponse<ShippingInfo>> GetShippingInfoByOrderIdAsync(int orderId);
}


