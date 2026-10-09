using Umbraco.Automate.Core.Actions;
using Webinar.Supplier;

namespace Webinar.Automate;

[Action("travel.createSupplierBooking", "Create Supplier Booking",
    Description = "Books the trip with our travel supplier.",
    Group = "Bookings",
    Icon = "icon-handshake",
    ConnectionTypeAlias = "travel.supplierPortal")]
public sealed class CreateSupplierBookingAction
    : ActionBase<CreateSupplierBookingSettings, CreateSupplierBookingOutput>
{
    private readonly ITravelSupplierClient _supplier;

    public CreateSupplierBookingAction(ActionInfrastructure infrastructure, ITravelSupplierClient supplier)
        : base(infrastructure)
        => _supplier = supplier;

    public override async Task<ActionResult> ExecuteAsync(
        ActionContext context,
        CancellationToken cancellationToken)
    {
        var settings = context.GetSettings<CreateSupplierBookingSettings>();
        var connection = context.Connection?.GetSettings<TravelSupplierConnectionSettings>();

        if (connection is null || connection.ApiKey.Length == 0)
        {
            // The category is what the run log shows the editor, so pick a real one.
            return ActionResult.Failed(
                new InvalidOperationException("No Global Travel Suppliers connection is configured on this step."),
                StepRunErrorCategory.ConfigurationError);
        }

        var bookingReference = await _supplier.CreateBookingAsync(
            connection.ApiKey,
            settings.Email,
            settings.CompanyName,
            cancellationToken);

        context.LogInfo($"Booked with Global Travel Suppliers: {bookingReference}");

        return Success(new CreateSupplierBookingOutput { BookingReference = bookingReference });
    }
}
