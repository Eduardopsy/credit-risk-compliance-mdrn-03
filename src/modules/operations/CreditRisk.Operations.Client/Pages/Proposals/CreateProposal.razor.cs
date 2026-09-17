// File: src/modules/operations/CreditRisk.Operations.Client/Pages/Proposals/CreateProposal.razor.cs
using CreditRisk.Operations.Client.Models;
using CreditRisk.Operations.Client.Services;
using FluentValidation;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CreditRisk.Operations.Client.Pages.Proposals;

public sealed partial class CreateProposal
{
    [Inject] private ApiClient Api { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private MudForm _form = null!;
    private CreateProposalRequest _request = new();
    private bool _isSubmitting;
    private readonly CreateProposalRequestClientValidator _validator = new();

    private async Task SubmitAsync()
    {
        await _form.Validate();
        if (!_form.IsValid) return;

        _isSubmitting = true;
        try
        {
            var result = await Api.CreateProposalAsync(_request);
            Snackbar.Add($"Proposal submitted successfully. ID: {result?.ProposalId}", MudBlazor.Severity.Success);
            Navigation.NavigateTo($"/proposals/{result?.ProposalId}");
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity)
        {
            Snackbar.Add("Validation failed. Please check the form.", MudBlazor.Severity.Error);
        }
        catch (Exception)
        {
            Snackbar.Add("An unexpected error occurred. Please try again.", MudBlazor.Severity.Error);
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void Cancel() => Navigation.NavigateTo("/proposals");
}

/// <summary>Client-side FluentValidation validator for CreateProposalRequest.</summary>
internal sealed class CreateProposalRequestClientValidator : AbstractValidator<CreateProposalRequest>
{
    public CreateProposalRequestClientValidator()
    {
        RuleFor(x => x.CustomerDocument).NotEmpty().WithMessage("Document is required.");
        RuleFor(x => x.CustomerDocumentType).Must(t => t is "CPF" or "CNPJ").WithMessage("Select CPF or CNPJ.");
        RuleFor(x => x.CustomerName).NotEmpty().MinimumLength(3).WithMessage("Full name is required.");
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().WithMessage("Valid email is required.");
        RuleFor(x => x.MonthlyIncome).GreaterThan(0).WithMessage("Monthly income must be greater than zero.");
        RuleFor(x => x.RequestedLimit).GreaterThan(0).LessThanOrEqualTo(500_000).WithMessage("Limit must be between R$ 1 and R$ 500,000.");
        RuleFor(x => x.BureauConsentGiven).Equal(true).WithMessage("Bureau consent is required to proceed.");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CreateProposalRequest>
            .CreateWithOptions((CreateProposalRequest)model, x => x.IncludeProperties(propertyName)));
        return result.IsValid ? [] : result.Errors.Select(e => e.ErrorMessage);
    };
}
