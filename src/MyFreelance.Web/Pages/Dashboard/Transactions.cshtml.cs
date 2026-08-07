using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyFreelance.Application.DTOs.Transactions;
using MyFreelance.Application.Interfaces;

namespace MyFreelance.Web.Pages.Dashboard;

public class TransactionsModel(ITransactionService transactionService) : PageModel
{
    public IReadOnlyList<TransactionListItemDto> Transactions { get; set; } = [];

    public async Task OnGetAsync()
    {
        ViewData["ActiveNav"] = "transactions";
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Transactions = await transactionService.GetUserTransactionsAsync(userId);
    }
}
